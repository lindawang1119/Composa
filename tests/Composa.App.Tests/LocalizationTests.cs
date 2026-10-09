using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Automation;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Composa.App.Dialogs;
using Composa.Editing;
using Composa.Filters;
using SkiaSharp;

namespace Composa.App.Tests;

public class LocalizationTests
{
    [Fact]
    public void Language_preference_is_backwards_compatible_and_preserves_other_settings()
    {
        var old = JsonSerializer.Deserialize<Settings>("""{"JpegQuality":83,"RecentFiles":["C:\\Photos\\Open.cmps"]}""")!;
        Assert.Equal("en", old.Language);
        old.Language = "zh-CN";
        var restored = JsonSerializer.Deserialize<Settings>(JsonSerializer.Serialize(old))!;
        Assert.Equal("zh-CN", restored.Language);
        Assert.Equal(83, restored.JpegQuality);
        Assert.Equal(old.RecentFiles, restored.RecentFiles);
        Assert.Equal("zh-CN", L10n.Normalize("ZH_cn"));
        Assert.Equal("en", L10n.Normalize("fr"));
        Assert.Equal("en", L10n.Normalize(null));
    }

    [Fact]
    public void Chinese_catalogs_have_valid_matching_format_arguments_and_English_fallback()
    {
        var previous = L10n.Language;
        try
        {
            var assembly = typeof(L10n).Assembly;
            var names = assembly.GetManifestResourceNames().Where(n => n.EndsWith(".zh-CN.json", StringComparison.Ordinal)).ToArray();
            Assert.NotEmpty(names);
            var entries = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var name in names)
            {
                using var stream = assembly.GetManifestResourceStream(name)!;
                foreach (var (english, chinese) in JsonSerializer.Deserialize<Dictionary<string, string>>(stream)!)
                {
                    Assert.False(string.IsNullOrWhiteSpace(chinese), english);
                    Assert.Equal(Arguments(english), Arguments(chinese));
                    // Plain captions may contain literal shortcut braces ({ }). Only formats have arguments.
                    if (Arguments(english).Length > 0)
                    {
                        var count = CompositeFormat.Parse(english).MinimumArgumentCount;
                        var values = Enumerable.Repeat<object>(new FormatProbe(), count).ToArray();
                        _ = string.Format(CultureInfo.InvariantCulture, chinese, values);
                    }
                    if (entries.TryGetValue(english, out var existing)) Assert.Equal(existing, chinese);
                    entries[english] = chinese;
                }
            }
            L10n.Language = "zh-CN";
            Assert.Equal("取消", L10n.T("Cancel"));
            Assert.Equal("图层", L10n.T("Layers"));
            Assert.Equal(L10n.T("Gaussian Blur") + "…", L10n.T("Gaussian Blur…"));
            Assert.Equal("my-photo-中文.png", L10n.T("my-photo-中文.png"));
            L10n.Language = "en";
            foreach (var key in entries.Keys) Assert.Equal(key, L10n.T(key));
        }
        finally { L10n.Language = previous; }
    }

    private static int[] Arguments(string text) => Regex.Matches(text, @"(?<!\{)\{(\d+)(?:,[^}:]+)?(?::[^}]+)?\}(?!\})")
        .Select(m => int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture)).Distinct().Order().ToArray();

    private sealed class FormatProbe : IFormattable
    {
        public string ToString(string? format, IFormatProvider? formatProvider) => "value";
    }

    [AvaloniaFact]
    public void Chinese_interface_preserves_user_names_editing_and_command_ids()
    {
        var previous = L10n.Language;
        MainWindow? window = null;
        try
        {
            L10n.Language = "zh-CN";
            window = new MainWindow { Width = 1280, Height = 800 };
            window.Show();
            Assert.True(Screenshots.Save(window, "zh-01-welcome"));
            var session = EditorSession.NewCanvas(480, 320, SKColors.White);
            session.SuggestedName = "Open";
            session.Document.Layers[0].Name = "Levels";
            window.AddSession(session);
            session.AddBlankLayer();
            Dispatcher.UIThread.RunJobs();
            var headers = window.GetLogicalDescendants().OfType<Menu>().First().GetLogicalDescendants().OfType<MenuItem>()
                .Select(i => i.Header as string).ToArray();
            Assert.Contains(L10n.T("_File"), headers);
            Assert.Contains(L10n.T("_Help"), headers);
            Assert.Matches(@"[\u4e00-\u9fff]", L10n.T("_File"));
            Assert.Equal("Open", session.Title);
            Assert.Equal("Levels", session.Document.Layers[0].Name);
            Assert.Contains(window.GetVisualDescendants().OfType<TextBlock>(), t => t.Text == "Levels");
            Assert.Equal("New Layer", session.History.UndoName);
            Assert.True(session.CanUndo);
            session.Undo();
            Assert.Single(session.Document.Layers);
            session.Redo();
            Assert.Equal(2, session.Document.Layers.Count);
            Assert.True(Screenshots.Save(window, "zh-02-editor"));
            // Shortcut editing searches the displayed Chinese label but persists the original command ID.
            window.KeyPressQwerty(PhysicalKey.F1, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
            var shortcuts = window.OwnedWindows.Last();
            shortcuts.GetVisualDescendants().OfType<TextBox>().First().Text = L10n.T("New Layer");
            Dispatcher.UIThread.RunJobs();
            var recorder = shortcuts.GetVisualDescendants().OfType<Button>().Single(b => b.Content as string == "Ctrl+Shift+N");
            recorder.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            shortcuts.KeyPressQwerty(PhysicalKey.K, RawInputModifiers.Control | RawInputModifiers.Shift);
            shortcuts.Close(true);
            Dispatcher.UIThread.RunJobs();
            Assert.Contains("New Layer", window.Settings.Shortcuts.Keys);
            window.KeyPressQwerty(PhysicalKey.K, RawInputModifiers.Control | RawInputModifiers.Shift);
            Assert.Equal(3, session.Document.Layers.Count);
        }
        finally
        {
            if (window != null)
            {
                foreach (var document in window.Sessions) document.MarkSaved("localization-test.cmps");
                window.Close();
                Dispatcher.UIThread.RunJobs();
            }
            L10n.Language = previous;
        }
    }

    [AvaloniaFact]
    public void Language_menu_remembers_selection_and_leaves_current_session_running_until_restart()
    {
        var previous = L10n.Language;
        MainWindow? window = null;
        try
        {
            L10n.Language = "en";
            window = new MainWindow { Width = 1000, Height = 700 };
            window.Show();
            var session = EditorSession.NewCanvas(200, 120, SKColors.White);
            window.AddSession(session);
            var chinese = window.GetLogicalDescendants().OfType<Menu>().First().GetLogicalDescendants().OfType<MenuItem>()
                .Single(i => i.Header as string == "简体中文");
            chinese.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("zh-CN", window.Settings.Language);
            Assert.True(chinese.IsChecked);
            Assert.Equal("en", L10n.Language);
            Assert.Same(session, window.Session);
            Assert.NotEmpty(window.OwnedWindows);
            foreach (var dialog in window.OwnedWindows.ToArray()) dialog.Close();
            Dispatcher.UIThread.RunJobs();
        }
        finally { window?.Close(); Dispatcher.UIThread.RunJobs(); L10n.Language = previous; }
    }

    [AvaloniaFact]
    public void Chinese_dialogs_render_with_localized_buttons_and_titles()
    {
        var previous = L10n.Language;
        MainWindow? window = null;
        try
        {
            L10n.Language = "zh-CN";
            window = new MainWindow { Width = 1280, Height = 800 };
            window.Show();
            _ = CanvasDialogs.NewCanvas(window, SKColors.White);
            CaptureDialog(window, "zh-03-new-canvas");
            _ = AdjustmentDialogs.EditFilter(window, new FilterSettings { Kind = FilterKind.MotionBlur }, _ => { });
            CaptureDialog(window, "zh-04-motion-blur");
            using var photo = Composa.Rendering.Pixels.NewColor(240, 160);
            photo.Erase(new SKColor(150, 120, 95));
            _ = CameraRawDialog.Show(window, new CameraRawSettings(), photo, _ => { }, () => photo);
            CaptureDialog(window, "zh-05-camera-raw");
            _ = Prompts.Color(window, "Foreground Color", SKColors.White);
            Dispatcher.UIThread.RunJobs();
            var colorDialog = window.OwnedWindows.Last();
            var picker = colorDialog.GetVisualDescendants().OfType<ColorView>().Single();
            var tabs = picker.GetVisualDescendants().OfType<TabControl>().Single();
            Assert.Equal(L10n.T("Spectrum"), ToolTip.GetTip(tabs.Items.OfType<TabItem>().ElementAt((int)ColorViewTab.Spectrum)));
            tabs.SelectedIndex = (int)ColorViewTab.Components;
            Dispatcher.UIThread.RunJobs();
            var hex = picker.GetVisualDescendants().OfType<TextBox>().Single(t => t.Name == "PART_HexTextBox");
            Assert.Equal(L10n.T("Hexadecimal Color"), AutomationProperties.GetName(hex));
            picker.Color = Avalonia.Media.Colors.CornflowerBlue;
            Dispatcher.UIThread.RunJobs();
            Assert.Contains("6495ed", hex.Text!.ToLowerInvariant());
            CaptureDialog(window, "zh-06-color");
        }
        finally { window?.Close(); Dispatcher.UIThread.RunJobs(); L10n.Language = previous; }
    }

    [AvaloniaFact]
    public void Chinese_import_report_translates_generated_sentences_and_preserves_names()
    {
        var previous = L10n.Language;
        MainWindow? window = null;
        try
        {
            L10n.Language = "zh-CN";
            window = new MainWindow();
            window.Show();
            var message = "The font \"My Font 123\" isn't installed; the text is shown in a fallback until it is.";
            _ = ImportConversionDialog.Confirm(window, "My Photo.psd", "Photoshop", [new("Levels", message)]);
            Dispatcher.UIThread.RunJobs();
            var dialog = window.OwnedWindows.Last();
            var labels = dialog.GetVisualDescendants().OfType<TextBlock>().Select(t => t.Text).ToArray();
            Assert.Contains("Levels", labels);
            Assert.Contains(L10n.F("The font \"{0}\" isn't installed; the text is shown in a fallback until it is.", "My Font 123"), labels);
            Assert.Contains("My Photo.psd", dialog.Title!);
            dialog.Close();
            Dispatcher.UIThread.RunJobs();
        }
        finally { window?.Close(); Dispatcher.UIThread.RunJobs(); L10n.Language = previous; }
    }

    private static void CaptureDialog(MainWindow owner, string name)
    {
        Dispatcher.UIThread.RunJobs();
        var dialog = owner.OwnedWindows.Last();
        Assert.Matches(@"[\u4e00-\u9fff]", dialog.Title!);
        Assert.Contains(dialog.GetLogicalDescendants().OfType<Button>(), b => b.Content as string == "取消");
        Assert.True(Screenshots.Save(dialog, name));
        dialog.Close();
        Dispatcher.UIThread.RunJobs();
    }
}

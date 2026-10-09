using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Composa.App.Dialogs;
using Composa.Editing;
using Composa.Rendering;
using SkiaSharp;

namespace Composa.App.Tests;

/// <summary>Image Size's Resample choice through the window: the dialog, the setting, the model behind the progress window, and Cancel.</summary>
public class ImageSizeUiTests
{
    private readonly MainWindow window;
    private readonly EditorSession session;

    public ImageSizeUiTests()
    {
        window = new MainWindow { Width = 1280, Height = 800 };
        window.Show();
        session = EditorSession.NewCanvas(60, 40, SKColors.Transparent);
        var photo = Pixels.NewColor(60, 40);
        for (var y = 0; y < 40; y++) for (var x = 0; x < 60; x++) photo.SetPixel(x, y, x < 30 ? new SKColor(200, 60, 40) : new SKColor(40, 80, 200));
        session.AddImageLayer("photo", photo, new SKPoint(30, 20));
        window.AddSession(session);
        Dispatcher.UIThread.RunJobs();
    }

    private static void PumpUntil(Func<bool> condition, int seconds = 30)
    {
        var deadline = DateTime.UtcNow.AddSeconds(seconds);
        while (!condition() && DateTime.UtcNow < deadline) { Dispatcher.UIThread.RunJobs(); Thread.Sleep(20); }
        Dispatcher.UIThread.RunJobs();
        Assert.True(condition(), "the awaited work did not finish in time");
    }

    [AvaloniaFact]
    public void The_dialog_offers_resample_and_its_note_follows_the_choice_and_the_size()
    {
        var result = CanvasDialogs.ImageSize(window, 60, 40, 72, ResampleMode.Enhance);
        Dispatcher.UIThread.RunJobs();
        var dialog = window.OwnedWindows.Last();
        var combo = dialog.GetVisualDescendants().OfType<ComboBox>().Single();
        var note = dialog.GetVisualDescendants().OfType<TextBlock>().Single(t => t.MaxWidth == 380);
        Assert.Equal(2, combo.SelectedIndex);
        Assert.Contains("Enhance only applies when enlarging", note.Text);
        var width = dialog.GetVisualDescendants().OfType<NumericUpDown>().First();
        width.Value = 240;
        Dispatcher.UIThread.RunJobs();
        Assert.Contains("inventing fine detail", note.Text);
        Screenshots.Save(dialog, "84-image-size-enhance");
        combo.SelectedIndex = 1;
        Dispatcher.UIThread.RunJobs();
        Assert.Contains("hard-edged block", note.Text);
        dialog.GetVisualDescendants().OfType<Button>().Single(b => b.Content as string == "OK").Command?.Execute(null);
        dialog.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        Assert.True(result.IsCompleted);
        Assert.Equal((240, 160, 72d, ResampleMode.Nearest), result.Result!.Value);
    }

    [AvaloniaFact]
    public async Task Enhance_runs_the_model_behind_the_progress_window_as_one_undo_step()
    {
        var delay = ProgressWindow.Delay;
        ProgressWindow.Delay = TimeSpan.FromMilliseconds(1);
        try
        {
            var task = window.ResizeImage(session, 150, 100, 72, ResampleMode.Enhance);
            PumpUntil(() => window.OwnedWindows.OfType<ProgressWindow>().Any(w => w.IsVisible) || task.IsCompleted, 5);
            if (window.OwnedWindows.OfType<ProgressWindow>().FirstOrDefault() is { } progress) Screenshots.Save(progress, "85-enhancing");
            PumpUntil(() => task.IsCompleted);
            Assert.True(await task);
            Assert.Equal((150, 100), (session.Document.Width, session.Document.Height));
            Assert.Equal("Image Size", session.History.UndoName);
            var pixels = session.ActiveLayer!.Pixels!;
            Assert.Equal((150, 100), (pixels.Width, pixels.Height));
            Assert.True(pixels.GetPixel(30, 50).Red > 150 && pixels.GetPixel(120, 50).Blue > 150);
            session.Undo();
            Assert.Equal((60, 40), (session.Document.Width, session.Document.Height));
            Assert.Empty(window.OwnedWindows.OfType<ProgressWindow>());
        }
        finally { ProgressWindow.Delay = delay; }
    }

    [AvaloniaFact]
    public async Task Cancelling_the_progress_window_leaves_the_document_untouched()
    {
        var delay = ProgressWindow.Delay;
        ProgressWindow.Delay = TimeSpan.FromMilliseconds(1);
        try
        {
            // A larger layer, so the model takes several tiles and the window is up long enough to cancel.
            var big = Pixels.NewColor(600, 400);
            big.Erase(SKColors.Teal);
            // Keep the source at full size: fitting it into this 60×40 canvas makes the model skip it,
            // leaving only tiny jobs that can finish before Escape reaches the progress window.
            session.AddImageLayer("big", big, new SKPoint(30, 20), fit: false);
            var before = session.History.CurrentId;
            var task = window.ResizeImage(session, 1200, 800, 72, ResampleMode.Enhance);
            PumpUntil(() => window.OwnedWindows.OfType<ProgressWindow>().Any(w => w.IsVisible), 5);
            window.OwnedWindows.OfType<ProgressWindow>().Single().KeyPressQwerty(PhysicalKey.Escape, RawInputModifiers.None);
            PumpUntil(() => task.IsCompleted);
            Assert.False(await task);
            Assert.Equal(before, session.History.CurrentId);
            Assert.Equal((60, 40), (session.Document.Width, session.Document.Height));
        }
        finally { ProgressWindow.Delay = delay; }
    }

    [AvaloniaFact]
    public void Nearest_neighbour_through_the_window_keeps_the_hard_edge_and_the_setting_is_remembered()
    {
        Assert.True(window.ResizeImage(session, 120, 80, 72, ResampleMode.Nearest).Result);
        var pixels = session.ActiveLayer!.Pixels!;
        Assert.Equal(pixels.GetPixel(59, 40), pixels.GetPixel(58, 40));
        Assert.NotEqual(pixels.GetPixel(59, 40), pixels.GetPixel(60, 40));
        Assert.Equal(ResampleMode.Automatic, window.Settings.Resample); // Only the dialog remembers a choice.
    }
}

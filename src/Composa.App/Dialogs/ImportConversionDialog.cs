using Avalonia.Controls;
using Avalonia.Media;
using Composa.IO;
using System.Text.RegularExpressions;

namespace Composa.App.Dialogs;

/// <summary>What a Photoshop or GIMP file loses on the way in, listed per layer, with the choice to go ahead or not.</summary>
public static class ImportConversionDialog
{
    // Importers keep their reports in English. Match only the editor's own sentences here;
    // captured values such as font names, blend IDs and counts remain unchanged.
    private static readonly (string Format, Regex Pattern)[] MessageFormats =
    [
        MessageFormat("Folder blend mode \"{0}\" isn't supported. The folder will be pass-through."),
        MessageFormat("Blend mode \"{0}\" isn't supported and will be applied as Normal."),
        MessageFormat("Blend mode {0} isn't known and will be applied as Normal."),
        MessageFormat("Blend mode \"{0}\" has no exact equivalent; \"{1}\" is the closest."),
        MessageFormat("The file was saved by a GIMP newer than this version of Composa knows (format {0}), so anything added since may be missing."),
        MessageFormat("The file keeps {0} pixels; Composa holds 8 bits per channel, so some precision was lost."),
        MessageFormat("{0} saved channels (selections or alpha channels) were left out."),
        MessageFormat("{0} paths were left out; Composa has no paths."),
        MessageFormat("{0} tiles of the layer couldn't be read and are transparent."),
        MessageFormat("The layer's {0} effects (GIMP filters) can't be applied here and were dropped."),
        MessageFormat("The \"{0}\" compositing isn't supported; the layer composites as GIMP's Union does."),
        MessageFormat("The font \"{0}\" isn't installed, so the text is drawn with the default font."),
        MessageFormat("The font \"{0}\" isn't installed; the text is shown in a fallback until it is.")
    ];

    private static (string Format, Regex Pattern) MessageFormat(string format)
    {
        var pattern = Regex.Escape(format).Replace(Regex.Escape("{0}"), "(.*?)").Replace(Regex.Escape("{1}"), "(.*?)");
        return (format, new Regex("\\A" + pattern + "\\z", RegexOptions.CultureInvariant | RegexOptions.Singleline | RegexOptions.NonBacktracking));
    }

    private static string MessageText(string message)
    {
        if (L10n.Language != "zh-CN") return message;
        var translated = L10n.T(message);
        if (translated != message) return translated;
        foreach (var (format, pattern) in MessageFormats)
        {
            var match = pattern.Match(message);
            if (match.Success) return L10n.F(format, match.Groups.Cast<Group>().Skip(1).Select(g => (object?)g.Value).ToArray());
        }
        return message;
    }

    /// <param name="format">What the file is, as the intro names it: "Photoshop" or "GIMP".</param>
    public static Task<bool> Confirm(Window owner, string fileName, string format, IReadOnlyList<ImportConversion> conversions)
    {
        var rows = new StackPanel { Spacing = 10 };
        foreach (var item in conversions.Take(500))
        {
            var message = Ui.Label(MessageText(item.Message), Palette.Foreground);
            message.TextWrapping = TextWrapping.Wrap;
            message.MaxWidth = 470;
            rows.Children.Add(Ui.Column(2, Ui.Label(item.LayerName, Palette.Foreground, weight: FontWeight.SemiBold), message));
        }
        if (conversions.Count > 500) rows.Children.Add(Ui.Label(L10n.F("…and {0} more.", conversions.Count - 500), Palette.Secondary));
        var intro = Ui.Label(L10n.F("Composa will convert these {0} features. Nothing is applied until you continue.", format), Palette.Secondary);
        intro.TextWrapping = TextWrapping.Wrap;
        intro.MaxWidth = 500;
        var list = new ScrollViewer { Content = rows, MaxHeight = 320, Width = 500, HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled };
        var body = Ui.Column(12, intro, list);
        return new DialogWindow(L10n.F("Open {0}?", fileName), body, "Import").Ask(owner);
    }
}

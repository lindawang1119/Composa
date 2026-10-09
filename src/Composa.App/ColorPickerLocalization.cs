using Avalonia.Automation;
using Avalonia.Controls;

namespace Composa.App;

/// <summary>
/// Localizes the fixed names of Avalonia's Fluent color picker without touching
/// its editable color text, component values, palette names or RGB/HSV notation.
/// </summary>
public static class ColorPickerLocalization
{
    /// <summary>Call after constructing a ColorView and before adding it to a window.</summary>
    public static void Attach(ColorView view)
    {
        if (L10n.Language != "zh-CN") return;
        // The template's name scope includes parts of inactive tabs as well as the visible one.
        // Reapply after a replacement template; no traversal of editable TextBox content is needed.
        view.TemplateApplied += (_, e) => Localize(e.NameScope);
    }

    private static void Localize(INameScope scope)
    {
        if (scope.Find<TabControl>("PART_TabControl") is { } tabs)
        {
            NameTab(tabs, ColorViewTab.Spectrum, "Spectrum");
            NameTab(tabs, ColorViewTab.Palette, "Palette");
            NameTab(tabs, ColorViewTab.Components, "Color Components");
        }

        NamePart(scope, "PART_HexTextBox", "Hexadecimal Color");
        NamePart(scope, "ColorSpectrumThirdComponentSlider", "Third Component");
        NamePart(scope, "ColorSpectrumAlphaSlider", "Alpha Component");
    }

    private static void NameTab(TabControl tabs, ColorViewTab kind, string label)
    {
        var index = (int)kind;
        if (index < 0 || index >= tabs.Items.Count || tabs.Items[index] is not TabItem tab) return;
        var text = L10n.T(label);
        ToolTip.SetTip(tab, text);
        AutomationProperties.SetName(tab, text);
    }

    private static void NamePart(INameScope scope, string part, string label)
    {
        if (scope.Find<Control>(part) is { } control)
            AutomationProperties.SetName(control, L10n.T(label));
    }
}

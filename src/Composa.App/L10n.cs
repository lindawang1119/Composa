using System.Globalization;
using System.Text.Json;

namespace Composa.App;

/// <summary>
/// Translates editor UI text only. English source text is the key and the fallback;
/// document content, command IDs and numeric parsing keep their original values.
/// The selected language is applied at startup so an open editing session is never rebuilt.
/// </summary>
public static class L10n
{
    private static readonly Lazy<IReadOnlyDictionary<string, string>> Chinese = new(LoadChinese);
    private static string language = "en";

    public static string Language
    {
        get => language;
        set => language = Normalize(value);
    }

    public static string Normalize(string? value) => value?.Trim().Replace('_', '-').ToLowerInvariant() switch
    {
        "zh" or "zh-cn" or "zh-hans" => "zh-CN",
        _ => "en"
    };

    public static string T(string text)
    {
        if (language != "zh-CN" || text.Length == 0) return text;
        if (Chinese.Value.TryGetValue(text, out var translated)) return translated;
        // Menus append an ellipsis to shared adjustment/filter captions.
        var suffix = text.EndsWith('…') ? "…" : text.EndsWith("...", StringComparison.Ordinal) ? "..." : "";
        if (suffix.Length > 0 && Chinese.Value.TryGetValue(text[..^suffix.Length], out translated)) return translated + suffix;
        return text;
    }

    public static string F(string format, params object?[] args) =>
        string.Format(CultureInfo.CurrentCulture, T(format), args);

    /// <summary>A contextual caption may have a different Chinese meaning while keeping the original English wording.</summary>
    public static string T(string text, string context) =>
        language == "zh-CN" && Chinese.Value.TryGetValue(context, out var translated) ? translated : T(text);

    private static IReadOnlyDictionary<string, string> LoadChinese()
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        var assembly = typeof(L10n).Assembly;
        foreach (var name in assembly.GetManifestResourceNames().Where(n => n.EndsWith(".zh-CN.json", StringComparison.Ordinal)).Order())
        {
            using var stream = assembly.GetManifestResourceStream(name)!;
            var entries = JsonSerializer.Deserialize<Dictionary<string, string>>(stream)
                ?? throw new InvalidDataException($"Empty translation catalog: {name}");
            foreach (var (key, value) in entries)
            {
                if (result.TryGetValue(key, out var existing) && existing != value)
                    throw new InvalidDataException($"Conflicting translations for '{key}' in {name}");
                result[key] = value;
            }
        }
        return result;
    }
}

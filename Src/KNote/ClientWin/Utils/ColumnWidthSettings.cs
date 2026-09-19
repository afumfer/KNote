using System.Globalization;

namespace KNote.ClientWin.Utils;

/// <summary>
/// Text form ("Number=80;Topic=520") in which grid column widths are persisted in AppConfig.
/// Keyed by column name so a width survives while its column is hidden or columns are reordered.
/// </summary>
public static class ColumnWidthSettings
{
    public static Dictionary<string, int> Parse(string? text)
    {
        var widths = new Dictionary<string, int>(StringComparer.Ordinal);

        if (string.IsNullOrWhiteSpace(text))
            return widths;

        foreach (var entry in text.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = entry.Split('=', 2, StringSplitOptions.TrimEntries);
            if (parts.Length == 2 && parts[0].Length > 0 &&
                int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var width) && width > 0)
                widths[parts[0]] = width;
        }

        return widths;
    }

    public static string Format(IReadOnlyDictionary<string, int> widths)
        => string.Join(';', widths.Select(w => $"{w.Key}={w.Value.ToString(CultureInfo.InvariantCulture)}"));
}

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace KNote.Model;

/// <summary>
/// Reimplements SQL LIKE matching folding case and diacritics (accents, ñ, ç, ü, ...), so that
/// e.g. "jose" matches "José". Used to override SQLite's built-in "like" function so its behavior
/// mirrors SQL Server's "Latin1_General_100_CI_AI_SC" collation used with COLLATE ... LIKE.
/// </summary>
public static class AccentInsensitiveLike
{
    public static bool IsMatch(string pattern, string value, char? escapeChar = null)
    {
        if (pattern == null || value == null)
            return false;

        // Regex.IsMatch(string, string, ...) caches compiled patterns internally (Regex.CacheSize,
        // default 15), so repeated calls with the same pattern - as happens once per row scanned
        // for a single search - don't recompile it every time.
        var regexPattern = BuildRegexPattern(Fold(pattern), escapeChar);
        return Regex.IsMatch(Fold(value), regexPattern, RegexOptions.Singleline);
    }

    private static string Fold(string value)
    {
        var decomposed = value.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposed.Length);

        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        return sb.ToString().Normalize(NormalizationForm.FormC).ToUpperInvariant();
    }

    private static string BuildRegexPattern(string pattern, char? escapeChar)
    {
        var sb = new StringBuilder("\\A");

        for (int i = 0; i < pattern.Length; i++)
        {
            var c = pattern[i];

            if (escapeChar.HasValue && c == escapeChar.Value && i + 1 < pattern.Length)
                sb.Append(Regex.Escape(pattern[++i].ToString()));
            else if (c == '%')
                sb.Append(".*");
            else if (c == '_')
                sb.Append('.');
            else
                sb.Append(Regex.Escape(c.ToString()));
        }

        sb.Append("\\z");

        return sb.ToString();
    }
}

using System.Globalization;
using System.Text;

namespace KNote.ClientWin.Core.Reports;

// CSV export of the management window's notes list: the same columns, order and cell texts as the printed
// list (NotesListSnapshot), header row first. RFC 4180 quoting, CRLF line ends; the separator is the
// regional list separator (";" in Spanish locales) so Excel splits the columns on double click.
public static class NotesListCsv
{
    // UTF-8 with BOM: without it Excel reads the file as ANSI and breaks accented characters.
    public static readonly Encoding FileEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);

    public static string DefaultSeparator => CultureInfo.CurrentCulture.TextInfo.ListSeparator;

    public static string Build(NotesListSnapshot snapshot, string separator = null)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        separator = string.IsNullOrEmpty(separator) ? DefaultSeparator : separator;

        var csv = new StringBuilder();
        AppendLine(csv, snapshot.Columns.Select(c => c.Header), separator);
        foreach (var row in snapshot.Rows)
            AppendLine(csv, row, separator);

        return csv.ToString();
    }

    public static string Field(string value, string separator)
    {
        value = NeutralizeFormula(value ?? "");

        var needsQuotes = value.Contains(separator, StringComparison.Ordinal)
            || value.Contains('"') || value.Contains('\r') || value.Contains('\n')
            || (value.Length > 0 && (char.IsWhiteSpace(value[0]) || char.IsWhiteSpace(value[^1])));

        return needsQuotes ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
    }

    // Spreadsheets run a cell starting with = + - @ as a formula: a topic like "- Quick links" would show
    // as an error, and a crafted one could run a formula ("CSV injection"). Such cells get a leading
    // apostrophe, the usual way to force text; plain numbers (e.g. "-5") are left as they are.
    private static string NeutralizeFormula(string value)
    {
        if (value.Length == 0 || "=+-@\t\r".IndexOf(value[0]) < 0)
            return value;

        if (double.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out _))
            return value;

        return "'" + value;
    }

    private static void AppendLine(StringBuilder csv, IEnumerable<string> fields, string separator)
    {
        csv.Append(string.Join(separator, fields.Select(f => Field(f, separator))));
        csv.Append("\r\n");
    }
}

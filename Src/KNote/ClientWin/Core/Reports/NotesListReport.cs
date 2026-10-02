using System.Globalization;
using System.Text;

namespace KNote.ClientWin.Core.Reports;

// Printable report of the management window's notes list: landscape, the same columns, order and cell
// texts the user is seeing (NotesListSnapshot), headed by where the notes come from (NotesListContext).
public static class NotesListReport
{
    public const string Title = "Notes list";

    // Minimum share of the page width (%) per column, so a column the user left very narrow on a wide
    // screen still fits its header and values on paper. Unknown columns get DefaultMinPercent.
    private static readonly Dictionary<string, double> MinPercent = new()
    {
        ["NoteNumber"] = 5.5,
        ["Topic"] = 22,
        ["Priority"] = 5.5,
        ["Tags"] = 9,
        ["InternalTags"] = 8,
        ["ModificationDateTime"] = 10.5,
        ["CreationDateTime"] = 10.5
    };

    private const double DefaultMinPercent = 6;

    private static readonly HashSet<string> NoWrapColumns = new() { "NoteNumber", "Priority", "ModificationDateTime", "CreationDateTime" };

    private static readonly HashSet<string> MutedColumns = new() { "Tags", "InternalTags" };

    public static ReportDocument Build(NotesListSnapshot snapshot, NotesListContext context, DateTime generatedAt)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(context);

        var filtered = !string.IsNullOrWhiteSpace(snapshot.TextFilter);
        var notesCount = filtered
            ? $"{snapshot.Rows.Count} of {snapshot.LoadedCount}"
            : snapshot.Rows.Count.ToString();

        var report = new ReportDocument
        {
            Title = Title,
            Heading = context.Heading,
            Subheading = context.Subheading,
            Orientation = ReportOrientation.Landscape,
            GeneratedAt = generatedAt,
            FileNameBase = context.FileNameBase(generatedAt),
            BodyHtml = BuildBody(snapshot)
        };

        if (!string.IsNullOrWhiteSpace(context.Repository))
            report.Meta.Add(new ReportMetaItem("Repository", context.Repository));
        report.Meta.Add(new ReportMetaItem("Notes", notesCount));
        report.Meta.Add(new ReportMetaItem("Generated", generatedAt.ToString("g")));

        return report;
    }

    // Widths (%) for the columns: their on-screen proportions, raised to each column's minimum; whatever
    // that adds is taken back from the columns with room to spare above their own minimum (in practice
    // Topic), so the table always spans exactly the page width.
    public static double[] ColumnPercents(IReadOnlyList<NotesListColumn> columns)
    {
        if (columns.Count == 0)
            return Array.Empty<double>();

        var totalWidth = columns.Sum(c => Math.Max(c.Width, 1));
        var mins = columns.Select(c => Math.Min(MinPercent.GetValueOrDefault(c.Name, DefaultMinPercent), 100.0 / columns.Count)).ToArray();
        var percents = columns.Select((c, i) => Math.Max(100.0 * Math.Max(c.Width, 1) / totalWidth, mins[i])).ToArray();

        var excess = percents.Sum() - 100;
        var slack = percents.Select((p, i) => p - mins[i]).ToArray();
        var totalSlack = slack.Sum();
        if (excess > 0 && totalSlack > 0)
            for (int i = 0; i < percents.Length; i++)
                percents[i] -= excess * slack[i] / totalSlack;

        return percents;
    }

    private static string BuildBody(NotesListSnapshot snapshot)
    {
        if (snapshot.Columns.Count == 0 || snapshot.Rows.Count == 0)
            return "<p class=\"knt-empty\">There are no notes in this list.</p>";

        var columns = snapshot.Columns;
        var percents = ColumnPercents(columns);
        var cellClasses = columns.Select(CellClass).ToArray();

        var html = new StringBuilder();
        html.AppendLine("<table class=\"knt-grid\">");

        html.Append("<colgroup>");
        foreach (var percent in percents)
            html.Append($"<col style=\"width:{percent.ToString("0.##", CultureInfo.InvariantCulture)}%\">");
        html.AppendLine("</colgroup>");

        html.Append("<thead><tr>");
        for (int i = 0; i < columns.Count; i++)
            html.Append($"<th{(columns[i].AlignRight ? " class=\"knt-right\"" : "")}>{ReportHtml.Encode(columns[i].Header)}</th>");
        html.AppendLine("</tr></thead>");

        html.AppendLine("<tbody>");
        foreach (var row in snapshot.Rows)
        {
            html.Append("<tr>");
            for (int i = 0; i < columns.Count; i++)
            {
                var text = i < row.Length ? row[i] : "";
                html.Append($"<td{cellClasses[i]}>{ReportHtml.Encode(text)}</td>");
            }
            html.AppendLine("</tr>");
        }
        html.AppendLine("</tbody>");
        html.AppendLine("</table>");

        var count = snapshot.Rows.Count == 1 ? "1 note" : $"{snapshot.Rows.Count} notes";
        html.AppendLine($"<p class=\"knt-summary\">{ReportHtml.Encode(count)}</p>");

        return html.ToString();
    }

    private static string CellClass(NotesListColumn column)
    {
        var classes = new List<string>();
        if (column.AlignRight)
            classes.Add("knt-right");
        if (NoWrapColumns.Contains(column.Name))
            classes.Add("knt-nowrap");
        if (MutedColumns.Contains(column.Name))
            classes.Add("knt-muted");
        if (column.Name == "Topic")
            classes.Add("knt-topic");

        return classes.Count == 0 ? "" : $" class=\"{string.Join(' ', classes)}\"";
    }
}

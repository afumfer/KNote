using System.Globalization;
using System.Text;
using KNote.Model.Dto;

namespace KNote.ClientWin.Core.Reports;

// Printable report with everything about one note (NoteDetailReportData): properties, description,
// extended attributes, resources (with image thumbnails), tasks and trace notes. Portrait. Never shows
// the note's script, its attributes' scripts or its alarms.
public static class NoteDetailReport
{
    public const string Title = "Note details";

    private const string None = "—";

    public static ReportDocument Build(NoteDetailReportData data, DateTime generatedAt)
    {
        ArgumentNullException.ThrowIfNull(data);
        var note = data.Note;
        var isNew = note.NoteId == Guid.Empty;

        var report = new ReportDocument
        {
            Title = Title,
            Heading = string.IsNullOrWhiteSpace(note.Topic) ? "(No topic)" : note.Topic,
            Subheading = string.IsNullOrWhiteSpace(data.FolderPath) ? null : data.FolderPath,
            Orientation = ReportOrientation.Portrait,
            GeneratedAt = generatedAt,
            FileNameBase = isNew ? $"Note - {note.Topic}" : $"Note {note.NoteNumber} - {note.Topic}"
        };

        report.Meta.Add(new ReportMetaItem("Note number", isNew ? "(new)" : $"#{note.NoteNumber}"));
        report.Meta.Add(new ReportMetaItem("Folder number", note.FolderDto != null ? $"#{note.FolderDto.FolderNumber}" : None));
        if (!string.IsNullOrWhiteSpace(data.Repository))
            report.Meta.Add(new ReportMetaItem("Repository", data.Repository));
        report.Meta.Add(new ReportMetaItem("Generated", generatedAt.ToString("g")));

        var body = new StringBuilder();

        if (isNew)
            body.AppendLine("<p class=\"knt-notice\"><span class=\"knt-badge knt-warning\">New note</span> Not saved yet: printed as shown in the editor.</p>");
        else if (data.UnsavedChanges)
            body.AppendLine("<p class=\"knt-notice\"><span class=\"knt-badge knt-warning\">Unsaved changes</span> Printed as shown in the editor, including changes not saved yet.</p>");

        AppendProperties(body, data);
        AppendDescription(body, data);
        AppendAttributes(body, note);
        AppendResources(body, data.Resources);
        AppendTasks(body, data.Tasks);
        AppendTraceNotes(body, data);

        report.BodyHtml = body.ToString();
        return report;
    }

    private static void AppendProperties(StringBuilder body, NoteDetailReportData data)
    {
        var note = data.Note;
        var isNew = note.NoteId == Guid.Empty;

        body.AppendLine("<section class=\"knt-section\"><h2>Properties</h2><dl class=\"knt-fields\">");
        // Two label/value pairs per line (see .knt-fields): kept in related pairs.
        Field(body, "Folder", string.IsNullOrWhiteSpace(data.FolderPath) ? None : data.FolderPath);
        Field(body, "Note type", note.NoteTypeDto?.Name);
        Field(body, "Tags", note.Tags);
        Field(body, "Status", note.InternalTags);
        Field(body, "Created", isNew ? None : note.CreationDateTime.ToString("g"));
        Field(body, "Modified", isNew ? None : note.ModificationDateTime.ToString("g"));
        Field(body, "Priority", note.Priority.ToString());
        body.AppendLine("</dl></section>");
    }

    private static void AppendDescription(StringBuilder body, NoteDetailReportData data)
    {
        body.AppendLine("<section class=\"knt-section\"><h2>Description</h2>");
        if (!string.IsNullOrEmpty(data.DescriptionUrl))
            body.AppendLine($"<p>Web page: <a href=\"{ReportHtml.Encode(data.DescriptionUrl)}\">{ReportHtml.Encode(data.DescriptionUrl)}</a></p>");
        else if (!string.IsNullOrWhiteSpace(data.DescriptionHtml))
            body.AppendLine($"<div class=\"knt-content\">{data.DescriptionHtml}</div>");
        else
            body.AppendLine("<p class=\"knt-empty\">No description.</p>");
        body.AppendLine("</section>");
    }

    private static void AppendAttributes(StringBuilder body, NoteDto note)
    {
        var attributes = note.KAttributesDto?.OrderBy(a => a.Order).ToList() ?? new List<NoteKAttributeDto>();

        SectionStart(body, "Attributes", attributes.Count);
        if (attributes.Count == 0)
        {
            body.AppendLine("<p class=\"knt-empty\">No attributes.</p></section>");
            return;
        }

        body.AppendLine("<table class=\"knt-grid\"><colgroup><col style=\"width:35%\"><col style=\"width:65%\"></colgroup>");
        body.AppendLine("<thead><tr><th>Attribute</th><th>Value</th></tr></thead><tbody>");
        foreach (var attribute in attributes)
            body.AppendLine($"<tr><td class=\"knt-topic\">{ReportHtml.Encode(attribute.Name)}</td><td>{TextOrNone(attribute.Value)}</td></tr>");
        body.AppendLine("</tbody></table></section>");
    }

    private static void AppendResources(StringBuilder body, List<NoteDetailResource> resources)
    {
        SectionStart(body, "Resources", resources.Count);
        if (resources.Count == 0)
        {
            body.AppendLine("<p class=\"knt-empty\">No resources.</p></section>");
            return;
        }

        body.AppendLine("<div class=\"knt-cards\">");
        foreach (var resource in resources)
        {
            body.AppendLine("<div class=\"knt-card\">");
            if (!string.IsNullOrEmpty(resource.ThumbnailSrc))
                body.AppendLine($"<div class=\"knt-thumb\"><img src=\"{ReportHtml.Encode(resource.ThumbnailSrc)}\" alt=\"{ReportHtml.Encode(resource.Name)}\"></div>");
            else
                body.AppendLine($"<div class=\"knt-thumb knt-file\"><span>{ReportHtml.Encode(FileKind(resource))}</span></div>");
            body.AppendLine($"<div class=\"knt-card-title\">{ReportHtml.Encode(resource.Name)}</div>");
            body.AppendLine($"<div class=\"knt-card-meta\">{ReportHtml.Encode(resource.FileType)}</div>");
            if (!string.IsNullOrWhiteSpace(resource.Description))
                body.AppendLine($"<div class=\"knt-card-text\">{ReportHtml.Encode(resource.Description)}</div>");
            body.AppendLine("</div>");
        }
        body.AppendLine("</div></section>");
    }

    private static void AppendTasks(StringBuilder body, List<NoteDetailTask> tasks)
    {
        SectionStart(body, "Tasks", tasks.Count);
        if (tasks.Count == 0)
        {
            body.AppendLine("<p class=\"knt-empty\">No tasks.</p></section>");
            return;
        }

        body.AppendLine("<table class=\"knt-grid\"><colgroup><col style=\"width:44%\"><col style=\"width:15%\"><col style=\"width:23%\"><col style=\"width:18%\"></colgroup>");
        body.AppendLine("<thead><tr><th>Task</th><th>Responsible</th><th>Dates</th><th>Effort</th></tr></thead><tbody>");
        foreach (var item in tasks)
        {
            var task = item.Task;
            var status = task.Resolved
                ? "<span class=\"knt-badge knt-success\">Resolved</span>"
                : "<span class=\"knt-badge\">Pending</span>";

            body.Append("<tr><td>");
            body.Append($"<div class=\"knt-task-head\">{status} <span class=\"knt-muted\">Priority {task.Priority}</span></div>");
            if (!string.IsNullOrWhiteSpace(task.Tags))
                body.Append($"<div class=\"knt-topic\">{ReportHtml.Encode(task.Tags)}</div>");
            if (!string.IsNullOrWhiteSpace(item.DescriptionHtml))
                body.Append($"<div class=\"knt-content knt-small\">{item.DescriptionHtml}</div>");
            body.Append($"</td><td>{TextOrNone(task.UserFullName)}</td><td class=\"knt-small\">");
            body.Append(DateLine("Start", task.StartDate) + DateLine("End", task.EndDate)
                + DateLine("Expected start", task.ExpectedStartDate) + DateLine("Expected end", task.ExpectedEndDate));
            body.Append("</td><td class=\"knt-small\">");
            body.Append(NumberLine("Estimated", task.EstimatedTime) + NumberLine("Spent", task.SpentTime) + NumberLine("Difficulty", task.DifficultyLevel));
            body.AppendLine("</td></tr>");
        }
        body.AppendLine("</tbody></table></section>");
    }

    private static void AppendTraceNotes(StringBuilder body, NoteDetailReportData data)
    {
        SectionStart(body, "Trace notes", data.TraceNotesFrom.Count + data.TraceNotesTo.Count);
        AppendTraceTable(body, "Trace notes from", data.TraceNotesFrom);
        AppendTraceTable(body, "Trace notes to", data.TraceNotesTo);
        body.AppendLine("</section>");
    }

    private static void AppendTraceTable(StringBuilder body, string title, List<TraceNoteRow> rows)
    {
        body.AppendLine($"<h3>{ReportHtml.Encode(title)}</h3>");
        if (rows.Count == 0)
        {
            body.AppendLine("<p class=\"knt-empty\">None.</p>");
            return;
        }

        body.AppendLine("<table class=\"knt-grid\"><colgroup><col style=\"width:10%\"><col style=\"width:42%\"><col style=\"width:18%\"><col style=\"width:14%\"><col style=\"width:8%\"><col style=\"width:8%\"></colgroup>");
        body.AppendLine("<thead><tr><th class=\"knt-right\">Number</th><th>Topic</th><th>Tags</th><th>Type</th><th class=\"knt-right\">Order</th><th class=\"knt-right\">Weight</th></tr></thead><tbody>");
        foreach (var row in rows)
            body.AppendLine($"<tr><td class=\"knt-right knt-nowrap\">{ReportHtml.Encode(row.Number)}</td><td class=\"knt-topic\">{ReportHtml.Encode(row.Topic)}</td>" +
                $"<td class=\"knt-muted\">{ReportHtml.Encode(row.Tags)}</td><td>{ReportHtml.Encode(row.Type)}</td>" +
                $"<td class=\"knt-right\">{row.Order}</td><td class=\"knt-right\">{row.Weight.ToString(CultureInfo.CurrentCulture)}</td></tr>");
        body.AppendLine("</tbody></table>");
    }

    private static void SectionStart(StringBuilder body, string title, int count)
        => body.AppendLine($"<section class=\"knt-section\"><h2>{ReportHtml.Encode(title)}<span class=\"knt-count\">{count}</span></h2>");

    private static void Field(StringBuilder body, string label, string value)
        => body.AppendLine($"<dt>{ReportHtml.Encode(label)}</dt><dd>{TextOrNone(value)}</dd>");

    private static string TextOrNone(string value)
        => string.IsNullOrWhiteSpace(value) ? $"<span class=\"knt-empty\">{None}</span>" : ReportHtml.Encode(value);

    private static string DateLine(string label, DateTime? date)
        => date == null ? "" : $"<div><span class=\"knt-muted\">{label}:</span> {date.Value.ToString("g")}</div>";

    private static string NumberLine(string label, double? value)
        => value == null ? "" : $"<div><span class=\"knt-muted\">{label}:</span> {value.Value.ToString("0.##", CultureInfo.CurrentCulture)}</div>";

    // Shown instead of a thumbnail for non-image resources: the extension or the file type's subtype.
    private static string FileKind(NoteDetailResource resource)
    {
        var extension = Path.GetExtension(resource.Name ?? "").TrimStart('.');
        if (!string.IsNullOrEmpty(extension))
            return extension.ToUpperInvariant();

        var slash = resource.FileType?.IndexOf('/') ?? -1;
        return slash >= 0 ? resource.FileType[(slash + 1)..].ToUpperInvariant() : "FILE";
    }
}

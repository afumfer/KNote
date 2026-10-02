using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using KNote.Model;

namespace KNote.ClientWin.Core.Reports;

// Turns a ReportDocument into a standalone HTML page: the layout's stylesheet (Resources/KNoteReport.css
// or KNoteBook.css, embedded in the assembly), the page setup (A4, orientation, running header and page
// numbers drawn by the browser's print engine) and, for reports, the header band. Pure string building -
// no WinForms/WebView2 - so it can be unit tested.
public static class ReportHtml
{
    private const string ReportStyleResourceName = "KNote.ClientWin.Resources.KNoteReport.css";
    private const string BookStyleResourceName = "KNote.ClientWin.Resources.KNoteBook.css";
    private const int MaxRunningHeaderLength = 90;

    private static readonly Dictionary<string, string> Styles = new();
    private static readonly Regex PageReferencePattern = new("<span class=\"knt-pageref\" data-ref=\"([^\"]+)\"></span>", RegexOptions.Compiled);

    public static string Render(ReportDocument report)
    {
        ArgumentNullException.ThrowIfNull(report);

        return report.Layout == ReportLayout.Book ? RenderBook(report) : RenderReport(report);
    }

    private static string RenderReport(ReportDocument report)
    {
        var orientation = report.Orientation == ReportOrientation.Landscape ? "landscape" : "portrait";
        var runningHeader = string.IsNullOrWhiteSpace(report.Heading)
            ? report.Title
            : $"{report.Title} · {report.Heading}";
        var footer = $"{KntConst.AppName} · {report.GeneratedAt:g}";

        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"en\">");
        html.AppendLine("<head>");
        html.AppendLine("<meta charset=\"utf-8\">");
        html.AppendLine($"<title>{Encode(string.IsNullOrWhiteSpace(report.Heading) ? report.Title : $"{report.Title} - {report.Heading}")}</title>");
        html.AppendLine("<style>");
        html.AppendLine(Style(ReportStyleResourceName));
        html.AppendLine($"@page {{ size: A4 {orientation};");
        html.AppendLine($"  @top-right {{ content: {CssString(Truncate(runningHeader, MaxRunningHeaderLength))}; }}");
        html.AppendLine($"  @bottom-left {{ content: {CssString(footer)}; }}");
        html.AppendLine("  @bottom-right { content: \"Page \" counter(page) \" of \" counter(pages); }");
        html.AppendLine("}");
        // The first page already shows the full header band.
        html.AppendLine("@page :first { @top-right { content: none; } }");
        html.AppendLine("</style>");
        html.AppendLine("</head>");
        html.AppendLine($"<body class=\"knt-{orientation}\">");

        html.AppendLine("<header class=\"knt-header\">");
        html.AppendLine($"<div class=\"knt-brand\"><span class=\"knt-app\">{Encode(KntConst.AppName)}</span><span class=\"knt-kind\">{Encode(report.Title)}</span></div>");
        if (!string.IsNullOrWhiteSpace(report.Heading))
            html.AppendLine($"<h1>{Encode(report.Heading)}</h1>");
        if (!string.IsNullOrWhiteSpace(report.Subheading))
            html.AppendLine($"<p class=\"knt-subheading\">{Encode(report.Subheading)}</p>");
        if (report.Meta.Count > 0)
        {
            html.AppendLine("<dl class=\"knt-meta\">");
            foreach (var item in report.Meta)
                html.AppendLine($"<div><dt>{Encode(item.Label)}</dt><dd>{Encode(item.Value)}</dd></div>");
            html.AppendLine("</dl>");
        }
        html.AppendLine("</header>");

        html.AppendLine("<main>");
        html.AppendLine(report.BodyHtml);
        html.AppendLine("</main>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");

        return html.ToString();
    }

    // A book: the body brings its cover, contents and chapters (see NotesBook). The running title and the
    // page number are left out of the "front" pages (cover and contents, see KNoteBook.css).
    private static string RenderBook(ReportDocument report)
    {
        var orientation = report.Orientation == ReportOrientation.Landscape ? "landscape" : "portrait";
        var title = string.IsNullOrWhiteSpace(report.Heading) ? report.Title : report.Heading;

        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine($"<html lang=\"{Encode(string.IsNullOrWhiteSpace(report.Language) ? "en" : report.Language)}\">");
        html.AppendLine("<head>");
        html.AppendLine("<meta charset=\"utf-8\">");
        html.AppendLine($"<title>{Encode(title)}</title>");
        html.AppendLine("<style>");
        html.AppendLine(Style(BookStyleResourceName));
        html.AppendLine($"@page {{ size: A4 {orientation};");
        html.AppendLine($"  @top-center {{ content: {CssString(Truncate(title, MaxRunningHeaderLength))}; }}");
        html.AppendLine("  @bottom-center { content: counter(page); }");
        html.AppendLine("}");
        html.AppendLine("@page front { @top-center { content: none; } @bottom-center { content: none; } }");
        html.AppendLine("</style>");
        html.AppendLine("</head>");
        html.AppendLine($"<body class=\"knt-book knt-{orientation}\">");
        html.AppendLine(report.BodyHtml);
        html.AppendLine("</body>");
        html.AppendLine("</html>");

        return html.ToString();
    }

    // Placeholder for the page number an element (by id) ends up on - filled by FillPageReferences once
    // the report has been paginated (ReportDocument.ResolvePageReferences). The element must also be the
    // target of an internal link (<a href="#id">) for Chromium to record its page in the printed PDF.
    public static string PageReference(string targetId)
        => $"<span class=\"knt-pageref\" data-ref=\"{Encode(targetId)}\"></span>";

    // Fills the page references with their page numbers (target id -> page); unknown targets stay empty.
    public static string FillPageReferences(string html, IReadOnlyDictionary<string, int> pages)
    {
        if (string.IsNullOrEmpty(html) || pages == null || pages.Count == 0)
            return html;

        return PageReferencePattern.Replace(html, m =>
            pages.TryGetValue(WebUtility.HtmlDecode(m.Groups[1].Value), out var page)
                ? $"<span class=\"knt-pageref\" data-ref=\"{m.Groups[1].Value}\">{page}</span>"
                : m.Value);
    }

    public static string Encode(string text)
        => WebUtility.HtmlEncode(text ?? "");

    // A CSS string literal (for the "content" of the page margin boxes): quotes and backslashes escaped,
    // line breaks flattened - a raw newline would end the declaration - and '<'/'>' as CSS escapes, since
    // it is written inside the <style> element (a "</style>" in a folder name must not close it).
    public static string CssString(string text)
    {
        var value = (text ?? "")
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("<", "\\3C ")
            .Replace(">", "\\3E ")
            .Replace("\r", " ")
            .Replace("\n", " ");
        return $"\"{value}\"";
    }

    private static string Truncate(string text, int maxLength)
        => text.Length <= maxLength ? text : text[..(maxLength - 1)] + "…";

    private static string Style(string resourceName)
    {
        lock (Styles)
        {
            if (!Styles.TryGetValue(resourceName, out var style))
            {
                using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
                    ?? throw new InvalidOperationException($"Embedded resource {resourceName} not found.");
                using var reader = new StreamReader(stream);
                Styles[resourceName] = style = reader.ReadToEnd();
            }
            return style;
        }
    }
}

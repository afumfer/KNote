using System.Net;
using System.Reflection;
using System.Text;
using KNote.Model;

namespace KNote.ClientWin.Core.Reports;

// Turns a ReportDocument into a standalone HTML page: the common stylesheet (Resources/KNoteReport.css,
// embedded in the assembly), the page setup (A4, orientation, running header and "Page X of Y" footer
// drawn by the browser's print engine) and the header band. Pure string building - no WinForms/WebView2 -
// so it can be unit tested.
public static class ReportHtml
{
    private const string StyleResourceName = "KNote.ClientWin.Resources.KNoteReport.css";
    private const int MaxRunningHeaderLength = 90;

    private static string _style;

    public static string Render(ReportDocument report)
    {
        ArgumentNullException.ThrowIfNull(report);

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
        html.AppendLine(Style);
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

    private static string Style
    {
        get
        {
            if (_style == null)
            {
                using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(StyleResourceName)
                    ?? throw new InvalidOperationException($"Embedded resource {StyleResourceName} not found.");
                using var reader = new StreamReader(stream);
                _style = reader.ReadToEnd();
            }
            return _style;
        }
    }
}

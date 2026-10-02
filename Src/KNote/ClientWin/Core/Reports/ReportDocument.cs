namespace KNote.ClientWin.Core.Reports;

public enum ReportOrientation
{
    Portrait,
    Landscape
}

public enum ReportLayout
{
    // Header band + data (Resources/KNoteReport.css).
    Report,

    // Book: the body brings its own cover, contents and chapters; serif typography, running title and
    // centered page numbers (Resources/KNoteBook.css).
    Book
}

// A label/value pair shown in the report's header band (repository, notes count, generation date...).
public record ReportMetaItem(string Label, string Value);

// Everything a printable report is made of, independent of how it is shown or printed. The body is
// already-built HTML (see ReportHtml's helpers); ReportHtml.Render wraps it in the common page layout,
// header band and page footer shared by every KNote report.
public class ReportDocument
{
    // Kind of report, e.g. "Notes list" / "Note details". Also the page's <title>, the running page header
    // and the default print job / PDF name.
    public string Title { get; set; } = "";

    // What the report is about, e.g. the folder path or the search/filter summary.
    public string Heading { get; set; } = "";

    // Optional second line under the heading (e.g. the text filter applied over the list).
    public string Subheading { get; set; }

    public List<ReportMetaItem> Meta { get; set; } = new();

    public ReportOrientation Orientation { get; set; } = ReportOrientation.Portrait;

    public ReportLayout Layout { get; set; } = ReportLayout.Report;

    // Language of the content (html lang), used by the browser for hyphenation. Report texts are English.
    public string Language { get; set; } = "en";

    // The body has page references (ReportHtml.PageReference) to fill with the real page numbers, which
    // only exist once the report is paginated: the preview prints it once to find them (see
    // PdfNamedDestinations) before showing it.
    public bool ResolvePageReferences { get; set; }

    public string BodyHtml { get; set; } = "";

    public DateTime GeneratedAt { get; set; } = DateTime.Now;

    // Proposed name (without extension) for the files saved from this report (PDF, CSV).
    public string FileNameBase { get; set; }
}

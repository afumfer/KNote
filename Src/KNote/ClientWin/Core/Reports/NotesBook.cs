using System.Text;
using KNote.Service.Core;

namespace KNote.ClientWin.Core.Reports;

// A chapter of the notes book: one note of the list. ContentHtml is its description rendered as HTML;
// Url is set instead for notes whose description is a web page. Both null when it has no description.
public record BookChapter(int Number, string Title, string ContentHtml, string Url);

// "Book" print of the management window's notes list: the notes in the order they are listed, one
// chapter each (title = Topic, text = Description), after a cover and a table of contents with page
// numbers. Sober, serif layout (Resources/KNoteBook.css); the page numbers of the contents are filled in
// by the report preview once paginated (ReportDocument.ResolvePageReferences).
public static class NotesBook
{
    public const string Title = "Notes book";

    public static string ChapterId(int number) => $"knt-ch-{number}";

    // Loads the listed notes, in order, as chapters. A note deleted meanwhile is left out.
    public static async Task<List<BookChapter>> LoadChaptersAsync(Store store, ServiceRef serviceRef, IEnumerable<Guid> noteIds)
    {
        ArgumentNullException.ThrowIfNull(serviceRef);

        var service = serviceRef.Service;
        var chapters = new List<BookChapter>();
        foreach (var noteId in noteIds)
        {
            var note = (await service.Notes.GetAsync(noteId)).Entity;
            if (note == null)
                continue;

            var (html, url) = NoteDetailReportData.RenderDescription(store, service, serviceRef.RepositoryRef, note);
            chapters.Add(new BookChapter(chapters.Count + 1, note.Topic, html, url));
        }
        return chapters;
    }

    public static ReportDocument Build(NotesListContext context, IReadOnlyList<BookChapter> chapters, DateTime generatedAt, string language = "en")
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(chapters);

        var (bookTitle, subtitle) = CoverTitles(context);

        var body = new StringBuilder();
        AppendCover(body, bookTitle, subtitle, context, chapters.Count, generatedAt);
        AppendContents(body, chapters);
        foreach (var chapter in chapters)
            AppendChapter(body, chapter);

        return new ReportDocument
        {
            Title = Title,
            Heading = bookTitle,
            Subheading = subtitle,
            Layout = ReportLayout.Book,
            Orientation = ReportOrientation.Portrait,
            Language = string.IsNullOrWhiteSpace(language) ? "en" : language,
            ResolvePageReferences = chapters.Count > 0,
            GeneratedAt = generatedAt,
            FileNameBase = context.FileNameBase(generatedAt, prefix: "Book"),
            BodyHtml = body.ToString()
        };
    }

    // A folder's book is titled after the folder itself, with its full path as subtitle; a search or
    // filter's book after the search/filter.
    public static (string Title, string Subtitle) CoverTitles(NotesListContext context)
    {
        if (context.Source != NotesListSource.Folder)
            return (context.Heading, context.Subheading);

        var path = context.FolderPath?.Trim() ?? "";
        var name = path.Split('\\', StringSplitOptions.RemoveEmptyEntries).LastOrDefault()?.Trim();
        if (string.IsNullOrEmpty(name))
            return (context.Heading, TextFilterLine(context));

        var parts = new List<string>();
        if (!string.Equals(name, path, StringComparison.Ordinal))
            parts.Add(path);
        if (TextFilterLine(context) is { } filter)
            parts.Add(filter);

        return (name, parts.Count > 0 ? string.Join(" · ", parts) : null);
    }

    private static string TextFilterLine(NotesListContext context)
        => string.IsNullOrWhiteSpace(context.TextFilter) ? null : $"List text filter: {context.TextFilter.Trim()}";

    private static void AppendCover(StringBuilder body, string title, string subtitle, NotesListContext context, int chapters, DateTime generatedAt)
    {
        body.AppendLine("<section class=\"knt-cover\">");
        body.AppendLine("<div>");
        body.AppendLine($"<div class=\"knt-cover-label\">{ReportHtml.Encode(KNote.Model.KntConst.AppName)}</div>");
        body.AppendLine($"<h1>{ReportHtml.Encode(title)}</h1>");
        body.AppendLine("<hr class=\"knt-cover-rule\">");
        if (!string.IsNullOrWhiteSpace(subtitle))
            body.AppendLine($"<div class=\"knt-cover-subtitle\">{ReportHtml.Encode(subtitle)}</div>");
        body.AppendLine("</div>");

        body.AppendLine("<div class=\"knt-cover-meta\">");
        body.AppendLine($"<div>{(chapters == 1 ? "1 chapter" : $"{chapters} chapters")}</div>");
        if (!string.IsNullOrWhiteSpace(context.Repository))
            body.AppendLine($"<div>{ReportHtml.Encode(context.Repository)}</div>");
        body.AppendLine($"<div>{ReportHtml.Encode(generatedAt.ToString("D"))}</div>");
        body.AppendLine("</div>");
        body.AppendLine("</section>");
    }

    private static void AppendContents(StringBuilder body, IReadOnlyList<BookChapter> chapters)
    {
        body.AppendLine("<nav class=\"knt-contents\">");
        body.AppendLine("<h2>Contents</h2>");
        foreach (var chapter in chapters)
        {
            var id = ChapterId(chapter.Number);
            body.AppendLine($"<a class=\"knt-toc-entry\" href=\"#{id}\">" +
                $"<span class=\"knt-toc-number\">{chapter.Number}</span>" +
                $"<span class=\"knt-toc-title\">{ReportHtml.Encode(ChapterTitle(chapter))}</span>" +
                "<span class=\"knt-toc-leader\"></span>" +
                $"{ReportHtml.PageReference(id)}</a>");
        }
        if (chapters.Count == 0)
            body.AppendLine("<p class=\"knt-chapter-empty\">There are no notes in this list.</p>");
        body.AppendLine("</nav>");
    }

    private static void AppendChapter(StringBuilder body, BookChapter chapter)
    {
        body.AppendLine($"<section class=\"knt-chapter\" id=\"{ChapterId(chapter.Number)}\">");
        body.AppendLine("<header class=\"knt-chapter-opening\">");
        body.AppendLine($"<p class=\"knt-chapter-label\">Chapter {chapter.Number}</p>");
        body.AppendLine($"<h1>{ReportHtml.Encode(ChapterTitle(chapter))}</h1>");
        body.AppendLine("<hr class=\"knt-chapter-rule\">");
        body.AppendLine("</header>");

        if (!string.IsNullOrEmpty(chapter.Url))
            body.AppendLine($"<div class=\"knt-text\"><p>Web page: <a href=\"{ReportHtml.Encode(chapter.Url)}\">{ReportHtml.Encode(chapter.Url)}</a></p></div>");
        else if (!string.IsNullOrWhiteSpace(chapter.ContentHtml))
            body.AppendLine($"<div class=\"knt-text\">{chapter.ContentHtml}</div>");
        else
            body.AppendLine("<p class=\"knt-chapter-empty\">This note has no text.</p>");

        body.AppendLine("</section>");
    }

    private static string ChapterTitle(BookChapter chapter)
        => string.IsNullOrWhiteSpace(chapter.Title) ? "(No topic)" : chapter.Title;
}

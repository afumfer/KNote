using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KNote.Model.Dto;

namespace KNote.Model;

/// <summary>
/// What the assistant receives of a document (KNote Atlas, stage 4, D14, 4.7a): the text kept (or read now, not kept) by pages, with
/// the scanned ones marked (transcribed by the AI, or not yet: to be looked at with view_document_pages), at most MaxTextLength
/// characters each time and cut at a page, with how many pages there are and how to ask for the next ones; and the pages it looks at.
/// </summary>
public static class AiDocumentToolRules
{
    // As get_note_details (decided by the owner): some 12 pages.
    public const int MaxTextLength = 40_000;
    // The pages it looks at each time, as the images of a note.
    public const int MaxPagesPerLook = AiAttachmentRules.MaxImagesPerLook;

    /// <summary>
    /// The document as the model reads it, from a page: each page with its heading, the transcribed ones and the scanned ones not
    /// transcribed yet marked, up to maxLength (whole pages: the first one, cut, if it alone is longer). A document without pages, its
    /// text. Ends with what was given and how to ask for the rest.
    /// </summary>
    public static string ForModel(string fileName, string extractedText, bool paged, int fromPage = 1, int? toPage = null,
        int maxLength = MaxTextLength)
    {
        var pages = DocumentTextRules.Pages(extractedText);
        if (pages.Count == 0)
            return $"{fileName}: nothing could be read from it.";

        if (!paged)
        {
            var text = string.Join("\n\n", pages.Select(p => p.Text)).Trim();
            if (text.Length <= maxLength)
                return $"{fileName}:\n---\n{text}\n---";
            return $"{fileName} (cut: its first {maxLength:N0} of {text.Length:N0} characters):\n---\n{Cut(text, maxLength)}\n---";
        }

        var first = Math.Clamp(fromPage, 1, pages.Count);
        var last = Math.Clamp(toPage ?? pages.Count, first, pages.Count);
        var body = new StringBuilder();
        var shown = first - 1;
        for (var number = first; number <= last; number++)
        {
            var page = Page(pages[number - 1]);
            if (body.Length > 0 && body.Length + page.Length > maxLength)
                break;
            body.Append(body.Length == 0 && page.Length > maxLength ? Cut(page, maxLength) : page);
            shown = number;
        }

        var heading = $"{fileName}: {pages.Count} {(pages.Count == 1 ? "page" : "pages")}. Pages {first}–{shown}";
        if (shown < last)
            heading += $" (cut at {maxLength:N0} characters: ask fromPage={shown + 1} for the next ones)";
        else if (shown < pages.Count)
            heading += $" (ask fromPage={shown + 1} for the next ones)";
        return $"{heading}.\n---\n{body.ToString().TrimEnd()}\n---";
    }

    // A page as the model reads it: its heading says what it is.
    private static string Page(DocumentPageDto page)
    {
        var heading = page.NeedsTranscription
            ? $"[Page {page.Number}: scanned, not transcribed yet; look at it with view_document_pages" +
              (string.IsNullOrWhiteSpace(page.Text) ? "]" : ". Its old OCR text, maybe wrong:]")
            : page.Transcribed ? $"[Page {page.Number}: automatic transcription of a scanned page, it may have mistakes]" : $"[Page {page.Number}]";
        return string.IsNullOrWhiteSpace(page.Text) ? $"{heading}\n\n" : $"{heading}\n{page.Text.Trim()}\n\n";
    }

    private static string Cut(string text, int maxLength)
    {
        if (text.Length <= maxLength)
            return text;
        var cut = text[..maxLength];
        var space = cut.LastIndexOf(' ');
        return (space > maxLength / 2 ? cut[..space] : cut).TrimEnd() + "…";
    }

    /// <summary>
    /// The pages to look at (from 1): the ones asked (at most MaxPagesPerLook) or, without them, the first ones; an error if a page is
    /// not in the document.
    /// </summary>
    public static (List<int> Pages, string Error) PickPages(int pagesCount, int[] pages)
    {
        if (pagesCount <= 0)
            return (null, "Error: the document has no pages to look at.");
        if (pages == null || pages.Length == 0)
            return (Enumerable.Range(1, Math.Min(pagesCount, MaxPagesPerLook)).ToList(), null);
        var outside = pages.Where(p => p < 1 || p > pagesCount).ToList();
        if (outside.Count > 0)
            return (null, $"Error: the document has {pagesCount} pages; there is no page {string.Join(", ", outside)}.");
        return (pages.Distinct().Take(MaxPagesPerLook).ToList(), null);
    }

    /// <summary>A document of a list by the name the model gives (its file name, without case) or, without one, the only one.</summary>
    public static (T Document, string Error) Find<T>(IReadOnlyList<T> documents, Func<T, string> fileName, string name, string whose)
        where T : class
    {
        static string Names(IEnumerable<string> names) => string.Join(", ", names);
        if (documents.Count == 0)
            return (null, $"Error: {whose} has no documents (PDF, DOCX, ODT, TXT or MD).");
        if (string.IsNullOrWhiteSpace(name))
            return documents.Count == 1 ? (documents[0], null) : (null, $"Error: say which document: {Names(documents.Select(fileName))}.");
        var found = documents.FirstOrDefault(d => string.Equals(fileName(d), name.Trim(), StringComparison.OrdinalIgnoreCase))
            ?? documents.FirstOrDefault(d => fileName(d).Contains(name.Trim(), StringComparison.OrdinalIgnoreCase));
        return found != null
            ? (found, null)
            : (null, $"Error: {whose} has no document '{name}'. Its documents are: {Names(documents.Select(fileName))}.");
    }
}

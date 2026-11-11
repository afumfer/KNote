using System.Text;
using KNote.Model.Dto;

namespace KNote.Model;

/// <summary>
/// What the two apps say of the text read of a document (KNote Atlas, stage 4, 4.4b): its state, what «Read document» did, the
/// scanned pages and how much of it is shown, so that the Web and ClientWin show the same.
/// </summary>
public static class DocumentTexts
{
    public const string ReadDocument = "Read document";
    public const string ReadAgain = "Read again";
    public const string ScannedPage = "Scanned page: transcription pending";
    public const string ScannedPageHelp = "Its transcription comes later; meanwhile, the text of its OCR layer, if it has one.";
    public const string ReadAgainQuestion = "The document has been read already. Read it again, in case KNote reads it better now?";
    public const string NothingRead = "Nothing was read from it.";

    public static string State(EnumExtractedState state) => state switch
    {
        EnumExtractedState.NotRead => "Not read yet. Read it to see its text and to find it by what it says.",
        EnumExtractedState.Partial => "Read, with scanned pages pending transcription.",
        EnumExtractedState.Complete => "Read.",
        EnumExtractedState.Unreadable => "It can't be read: it is protected, encrypted or damaged.",
        _ => ""
    };

    /// <summary>What «Read document» did.</summary>
    public static string ReadResult(DocumentReadDto read)
    {
        if (read == null)
            return "";
        if (read.State == EnumExtractedState.Unreadable)
            return State(read.State);

        var pages = read.Pages > 1 ? $" {read.Pages} pages" : "";
        if (read.PagesToTranscribe == 0)
            return $"Read{(pages.Length > 0 ? ":" + pages : "")}.";
        return $"Read:{pages}, {read.PagesToTranscribe} of them scanned. {ScannedPageHelp}";
    }

    public static string PageHeading(int number) => $"Page {number}";

    /// <summary>How much of the text is shown (null: all of it).</summary>
    public static string Showing(int pagesShown, int pagesCount) =>
        pagesShown < pagesCount ? $"Showing pages 1–{pagesShown} of {pagesCount}." : null;

    /// <summary>
    /// A kept text as a person reads it (ClientWin's text box): the pages of a PDF with their heading and the scanned ones
    /// marked; a document without pages, as it is.
    /// </summary>
    public static string Readable(string extractedText, bool paged)
    {
        var pages = DocumentTextRules.Pages(extractedText);
        if (!paged)
            return string.Join("\n\n", pages.ConvertAll(p => p.Text)).Replace("\n", "\r\n");

        var text = new StringBuilder();
        foreach (var page in pages)
        {
            text.Append($"── {PageHeading(page.Number)} ──\n");
            if (page.NeedsTranscription)
                text.Append($"[{ScannedPage}]\n");
            if (page.Text.Length > 0)
                text.Append(page.Text).Append('\n');
            text.Append('\n');
        }
        return text.ToString().TrimEnd().Replace("\n", "\r\n");
    }
}

using System;
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

    // KNote Atlas 4.5b: transcribing the scanned pages of one document, and the pages transcribed in the viewers.
    public const string TranscribePages = "Transcribe scanned pages";
    public const string AutomaticTranscription = "Automatic transcription";
    public const string AutomaticTranscriptionHelp = "Written by the AI from the scanned page: it may be wrong in a page of poor quality.";
    public const string SeeOriginalPage = "See the original page";
    public const string WithOcrPages = "Also the pages with the text of an old recognition (OCR layer), usually worse";
    public const string NoPagesToTranscribe = "There are no scanned pages to transcribe in this document.";

    /// <summary>
    /// What transcribing the pages of a document is going to do, before it starts: how many pages, with which model, how long
    /// (with the pace of the last run, if there was one) and, with a paid model, that they are sent.
    /// </summary>
    public static string TranscriptionConfirmation(int pages, ResourceDescriberDto describer, double? secondsPerPage)
    {
        var text = $"The AI transcribes {ResourceProcessingTexts.Pages(pages)} of this document with the model {describer.Model}, one after another.";
        if (ResourceProcessingRules.EstimateSeconds(pages, secondsPerPage) is double seconds && pages > 0)
            text += $" It should take about {SemanticIndexTexts.Duration(seconds)}.";
        if (describer.IsPaid)
            text += $" {describer.Model} is a paid model: the pages are sent to its provider.";
        return text + " Each page is kept as soon as it is transcribed: canceling keeps them, and you can go on later.";
    }

    public static string TranscriptionProgress(int done, int total) =>
        $"Transcribing page {Math.Min(done + 1, total)} of {total}…";

    /// <summary>What transcribing the pages of a document did.</summary>
    public static string TranscriptionResult(int transcribed, int total, bool canceled, bool summarized)
    {
        var text = canceled
            ? $"Canceled after {ResourceProcessingTexts.Pages(transcribed)} transcribed: what is done is kept."
            : $"{transcribed} of {total} scanned pages transcribed.";
        return summarized ? text + " The document was summarized too." : text;
    }

    /// <summary>
    /// The page of a readable text (Readable) where a position is: the one of the last page heading before it; null in a text
    /// without pages.
    /// </summary>
    public static int? PageAt(string readable, int position)
    {
        if (string.IsNullOrEmpty(readable))
            return null;
        var heading = PageHeadingMark + PageHeading(0)[..^1];
        // The heading may start at the position itself.
        var from = Math.Min(Math.Max(position, 0) + heading.Length - 1, readable.Length - 1);
        var at = readable.LastIndexOf(heading, from, StringComparison.Ordinal);
        if (at < 0)
            at = readable.IndexOf(heading, StringComparison.Ordinal);
        if (at < 0)
            return null;
        var start = at + heading.Length;
        var end = start;
        while (end < readable.Length && char.IsDigit(readable[end]))
            end++;
        return int.TryParse(readable[start..end], out var page) ? page : null;
    }

    private const string PageHeadingMark = "── ";

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
            text.Append($"{PageHeadingMark}{PageHeading(page.Number)} ──\n");
            if (page.NeedsTranscription)
                text.Append($"[{ScannedPage}]\n");
            else if (page.Transcribed)
                text.Append($"[{AutomaticTranscription}]\n");
            if (page.Text.Length > 0)
                text.Append(page.Text).Append('\n');
            text.Append('\n');
        }
        return text.ToString().TrimEnd().Replace("\n", "\r\n");
    }
}

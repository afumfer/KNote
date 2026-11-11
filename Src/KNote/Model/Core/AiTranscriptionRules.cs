using System;
using System.Text.RegularExpressions;

namespace KNote.Model;

/// <summary>
/// The rules of transcribing the scanned pages of a PDF with the AI (KNote Atlas, stage 4, D1, D13, 4.5), common to the Server,
/// the Web and ClientWin: how big the page is drawn, the request the model receives and how its answer is cleaned before it is
/// kept in the text of the document (DocumentTextRules).
/// </summary>
public static class AiTranscriptionRules
{
    // The page is drawn with this longest side (D13, measured in 4.0: 2,200 px cost 37 % more without a clear gain).
    public const int PageImageSide = 1600;
    // What the model may write of a page (4.0: a dense page is some 1,500 tokens): the cap stops a model that repeats itself.
    public const int MaxOutputTokens = 4096;
    // What the model answers for a page without text (TranscribePage's rules).
    public const string NoText = "(no text)";

    public const string NoTextMessage = "The page could not be transcribed: the model gave no text.";
    public const string NotPendingMessage = "The page is not pending of a transcription: it was transcribed or the document was read again.";

    private static readonly Regex Image = new(@"!\[[^\]\n]*\]\([^)\n]*\)", RegexOptions.Compiled);
    private static readonly Regex Fence = new(@"^```[a-zA-Z]*\s*\n(?<body>[\s\S]*?)\n```\s*$", RegexOptions.Compiled);

    /// <summary>
    /// The request of the model: the page and the document it belongs to. The last line says the language, as the other tasks:
    /// a small model follows it best (4.2, 4.4c).
    /// </summary>
    public static string Request(string fileName, int pageNumber, int pagesCount)
    {
        var of = pagesCount > 1 ? $" of {pagesCount}" : "";
        var document = string.IsNullOrWhiteSpace(fileName) ? "a scanned document" : $"the scanned document «{fileName.Trim()}»";
        return $"This is page {pageNumber}{of} of {document}.\n\n" +
            "Transcribe all the text of this page, in its own language, without translating it.";
    }

    /// <summary>
    /// The answer of the model as it is kept: without a code block around all of it, without images (in 4.0 the model wrote an
    /// image of its own with a link that doesn't exist, «![Image](https://i.imgur.com/...)») and empty for a page without text.
    /// </summary>
    public static string CleanTranscription(string answer)
    {
        var text = (answer ?? "").Replace("\r\n", "\n").Trim();
        var fence = Fence.Match(text);
        if (fence.Success)
            text = fence.Groups["body"].Value.Trim();
        text = Image.Replace(text, "").Trim();
        var bare = text.TrimEnd('.', ' ');
        if (string.Equals(bare, NoText, StringComparison.OrdinalIgnoreCase) || string.Equals(bare, NoText.Trim('(', ')'), StringComparison.OrdinalIgnoreCase))
            return "";
        return DocumentTextRules.Clean(text);
    }
}

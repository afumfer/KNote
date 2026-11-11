using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using KNote.Model.Dto;

namespace KNote.Model;

public enum EnumDocumentFormat
{
    None,
    Pdf,
    Docx,
    Odt,
    Text
}

/// <summary>
/// The text of the documents (KNote Atlas, stage 4, D1, D2, D5): which ones are read, which page of a PDF is scanned, how the
/// text is cleaned and how it is kept in Resources.ExtractedText. The pages are separated by PageSeparator (form feed) and a
/// page that needs a transcription starts with PendingMark and one transcribed by the AI (4.5) with TranscribedMark: control
/// characters, not letters nor numbers, so that searching «12» or «page» in the documents (D18) does not find them all. The
/// number of a page is its position.
/// </summary>
public static class DocumentTextRules
{
    public const char PageSeparator = '\f';
    public const char PendingMark = '\u0001';
    public const char TranscribedMark = '\u0002';

    // A page with fewer letters than this and an image is a scanned one (measured in 4.0).
    public const int MaxLettersOfAScannedPage = 20;
    // A page whose largest image covers this part of it is a scanned one, even with letters: its OCR layer is worse than a
    // transcription (D1, decided by the owner in 4.0: 28 of 28 pages of a scan with OCR, none of 53 of real text PDFs).
    public const double ScannedImageCoverage = 0.85;
    // What is kept of a document, at most (some 7,000 pages of text).
    public const int MaxTextLength = 20_000_000;
    // What the Web shows of a document's text, at most (some 100 pages; decided in 4.4b): ClientWin shows it whole.
    public const int MaxViewerLength = 200_000;

    private static readonly Regex HyphenatedWord = new(@"(\p{L})-\n(\p{Ll})", RegexOptions.Compiled);
    private static readonly Regex TrailingSpaces = new(@"[ \t]+\n", RegexOptions.Compiled);
    private static readonly Regex BlankLines = new(@"\n{3,}", RegexOptions.Compiled);

    /// <summary>The format of a document, by its type or, without one KNote knows, by its name.</summary>
    public static EnumDocumentFormat FormatOf(string fileType, string name)
    {
        var byType = (fileType ?? "").Trim().ToLowerInvariant() switch
        {
            "application/pdf" => EnumDocumentFormat.Pdf,
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => EnumDocumentFormat.Docx,
            "application/vnd.oasis.opendocument.text" => EnumDocumentFormat.Odt,
            "text/plain" or "text/markdown" or "text/x-markdown" => EnumDocumentFormat.Text,
            _ => EnumDocumentFormat.None
        };
        if (byType != EnumDocumentFormat.None)
            return byType;

        return (Path.GetExtension(name ?? "") ?? "").ToLowerInvariant() switch
        {
            ".pdf" => EnumDocumentFormat.Pdf,
            ".docx" => EnumDocumentFormat.Docx,
            ".odt" => EnumDocumentFormat.Odt,
            ".txt" or ".md" or ".markdown" => EnumDocumentFormat.Text,
            _ => EnumDocumentFormat.None
        };
    }

    public static bool IsDocument(string fileType, string name) => FormatOf(fileType, name) != EnumDocumentFormat.None;

    public static bool IsDocument(ResourceInfoDto resource) => resource != null && IsDocument(resource.FileType, resource.Name);

    /// <summary>Whether its fragments have a page: only a PDF's (a DOCX or an ODT depends on how it is laid out).</summary>
    public static bool IsPaged(EnumDocumentFormat format) => format == EnumDocumentFormat.Pdf;

    /// <summary>Whether a page of a PDF is scanned (D1): almost no letters and an image, or an image that covers almost all of it.</summary>
    public static bool NeedsTranscription(int letters, int images, double largestImageCoverage) =>
        images > 0 && (letters < MaxLettersOfAScannedPage || largestImageCoverage >= ScannedImageCoverage);

    /// <summary>
    /// A page's text as it is kept: its lines ends normalized, without control characters (the marks of the format are those),
    /// the words cut at the end of a line joined again («speciali-/zed», found in 4.0) and without blank lines in excess.
    /// </summary>
    public static string Clean(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        var normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
        var clean = new StringBuilder(normalized.Length);
        foreach (var c in normalized)
            if (c == '\n' || c == '\t' || !char.IsControl(c))
                clean.Append(c);

        var result = TrailingSpaces.Replace(clean.ToString(), "\n");
        result = HyphenatedWord.Replace(result, "$1$2");
        result = BlankLines.Replace(result, "\n\n");
        return result.Trim();
    }

    /// <summary>The text kept of a document's pages (Resources.ExtractedText), cut to MaxTextLength.</summary>
    public static string Compose(IEnumerable<DocumentPageDto> pages, bool paged)
    {
        var list = (pages ?? Enumerable.Empty<DocumentPageDto>()).ToList();
        var text = paged
            ? string.Join(PageSeparator, list.Select(p => Mark(p) + Clean(p.Text)))
            : Clean(string.Join("\n\n", list.Select(p => p.Text)));
        return text.Length > MaxTextLength ? text[..MaxTextLength] : text;
    }

    private static string Mark(DocumentPageDto page) =>
        page.NeedsTranscription ? PendingMark.ToString() : page.Transcribed ? TranscribedMark.ToString() : "";

    /// <summary>The pages of a kept text, numbered from 1.</summary>
    public static List<DocumentPageDto> Pages(string extractedText)
    {
        if (string.IsNullOrEmpty(extractedText))
            return new List<DocumentPageDto>();

        return extractedText.Split(PageSeparator).Select((page, i) =>
        {
            var mark = page.Length > 0 ? page[0] : ' ';
            var marked = mark == PendingMark || mark == TranscribedMark;
            return new DocumentPageDto
            {
                Number = i + 1,
                NeedsTranscription = mark == PendingMark,
                Transcribed = mark == TranscribedMark,
                Text = marked ? page[1..] : page
            };
        }).ToList();
    }

    /// <summary>
    /// The kept text with the transcription of a scanned page (4.5) in place of the text of its OCR layer, or null if the page
    /// is not pending of a transcription (already transcribed, or not there: the document was read again meanwhile).
    /// </summary>
    public static string Transcribe(string extractedText, int pageNumber, string transcription)
    {
        var pages = Pages(extractedText);
        var page = pages.FirstOrDefault(p => p.Number == pageNumber);
        if (page == null || !page.NeedsTranscription)
            return null;

        page.NeedsTranscription = false;
        page.Transcribed = true;
        page.Text = transcription ?? "";
        return Compose(pages, paged: true);
    }

    /// <summary>
    /// The pages of a document read again, with the transcriptions it had (4.5, decided by the owner): repeating them is
    /// expensive, so each page that is still scanned keeps its transcription, if the document has the same number of pages.
    /// </summary>
    public static List<DocumentPageDto> KeepTranscriptions(string previousText, List<DocumentPageDto> pages)
    {
        var previous = Pages(previousText);
        if (pages == null || previous.Count != pages.Count || !previous.Any(p => p.Transcribed))
            return pages;

        return pages.Select((page, i) => page.NeedsTranscription && previous[i].Transcribed
            ? new DocumentPageDto { Number = page.Number, Text = previous[i].Text, Transcribed = true }
            : page).ToList();
    }

    /// <summary>The scanned pages pending of a transcription: all, or only the ones without an OCR layer.</summary>
    public static List<int> PagesToTranscribe(string extractedText, bool withOcrLayer = true) =>
        Pages(extractedText).Where(p => p.NeedsTranscription && (withOcrLayer || !HasOcrLayer(p))).Select(p => p.Number).ToList();

    /// <summary>A scanned page that came with the text of an old text recognition (OCR): usable until it is transcribed.</summary>
    public static bool HasOcrLayer(DocumentPageDto page) => page.NeedsTranscription && !string.IsNullOrWhiteSpace(page.Text);

    /// <summary>
    /// The beginning of a kept text to show it: the whole pages that fit in maxLength (only the first one, cut, if it alone is
    /// longer), how many those are and how many the text has. maxLength 0: all of it.
    /// </summary>
    public static (string Text, int PagesShown, int PagesCount) Cut(string extractedText, int maxLength)
    {
        if (string.IsNullOrEmpty(extractedText))
            return ("", 0, 0);

        var count = extractedText.Count(c => c == PageSeparator) + 1;
        if (maxLength <= 0 || extractedText.Length <= maxLength)
            return (extractedText, count, count);

        // A separator at maxLength still leaves the pages before it within maxLength.
        var end = extractedText.LastIndexOf(PageSeparator, maxLength);
        if (end <= 0)
            return (extractedText[..maxLength], 1, count);
        var shown = extractedText[..end];
        return (shown, shown.Count(c => c == PageSeparator) + 1, count);
    }

    /// <summary>What a reading leaves: pages to transcribe, or read.</summary>
    public static EnumExtractedState StateOf(IEnumerable<DocumentPageDto> pages) =>
        (pages ?? Enumerable.Empty<DocumentPageDto>()).Any(p => p.NeedsTranscription) ? EnumExtractedState.Partial : EnumExtractedState.Complete;
}

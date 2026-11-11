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
/// page that needs a transcription starts with PendingMark: control characters, not letters nor numbers, so that searching
/// «12» or «page» in the documents (D18) does not find them all. The number of a page is its position.
/// </summary>
public static class DocumentTextRules
{
    public const char PageSeparator = '\f';
    public const char PendingMark = '\u0001';

    // A page with fewer letters than this and an image is a scanned one (measured in 4.0).
    public const int MaxLettersOfAScannedPage = 20;
    // A page whose largest image covers this part of it is a scanned one, even with letters: its OCR layer is worse than a
    // transcription (D1, decided by the owner in 4.0: 28 of 28 pages of a scan with OCR, none of 53 of real text PDFs).
    public const double ScannedImageCoverage = 0.85;
    // What is kept of a document, at most (some 7,000 pages of text).
    public const int MaxTextLength = 20_000_000;

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
            ? string.Join(PageSeparator, list.Select(p => (p.NeedsTranscription ? PendingMark.ToString() : "") + Clean(p.Text)))
            : Clean(string.Join("\n\n", list.Select(p => p.Text)));
        return text.Length > MaxTextLength ? text[..MaxTextLength] : text;
    }

    /// <summary>The pages of a kept text, numbered from 1.</summary>
    public static List<DocumentPageDto> Pages(string extractedText)
    {
        if (string.IsNullOrEmpty(extractedText))
            return new List<DocumentPageDto>();

        return extractedText.Split(PageSeparator).Select((page, i) => new DocumentPageDto
        {
            Number = i + 1,
            NeedsTranscription = page.Length > 0 && page[0] == PendingMark,
            Text = page.Length > 0 && page[0] == PendingMark ? page[1..] : page
        }).ToList();
    }

    /// <summary>What a reading leaves: pages to transcribe, or read.</summary>
    public static EnumExtractedState StateOf(IEnumerable<DocumentPageDto> pages) =>
        (pages ?? Enumerable.Empty<DocumentPageDto>()).Any(p => p.NeedsTranscription) ? EnumExtractedState.Partial : EnumExtractedState.Complete;
}

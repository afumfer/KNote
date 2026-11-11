using System.IO.Compression;
using System.Text;
using System.Xml;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace KNote.Ai;

/// <summary>
/// KNote's reader of documents (KNote Atlas, stage 4, D4, D5, D7), measured in 4.0: PdfPig for the PDF (the text of each page
/// in reading order, ContentOrderTextExtractor: Page.Text joins the words as they come in the file), and a reader of its own
/// for DOCX and ODT, a zip with the text in an XML (no package: DocumentFormat.OpenXml gave the same text), with a row of a
/// table per line. TXT and MD, in UTF-8 or, if they are not, in Latin-1. No AI: a scanned page is only marked
/// (DocumentTextRules.NeedsTranscription), with the text of its OCR layer if it has one. Whatever makes a document unreadable
/// (protected, encrypted, damaged) is KntDocumentUnreadableException; a failure reading the stream is not.
/// </summary>
public sealed class AiDocumentReader : IKntDocumentReader
{
    // What is unzipped of a DOCX or an ODT, at most (a zip bomb would be gigabytes).
    private const long MaxUnzippedLength = 64_000_000;
    // What is read of a TXT or an MD, at most.
    private const int MaxTextBytes = 64_000_000;

    public List<DocumentPageDto> Read(Stream content, EnumDocumentFormat format)
    {
        ArgumentNullException.ThrowIfNull(content);
        try
        {
            return format switch
            {
                EnumDocumentFormat.Pdf => ReadPdf(content),
                EnumDocumentFormat.Docx => OnePage(ReadZippedText(content, Docx)),
                EnumDocumentFormat.Odt => OnePage(ReadOdt(content)),
                EnumDocumentFormat.Text => OnePage(ReadText(content)),
                _ => throw new KntDocumentUnreadableException("It is not a document KNote reads.")
            };
        }
        catch (Exception ex) when (ex is not (KntDocumentUnreadableException or IOException or OutOfMemoryException or OperationCanceledException))
        {
            throw new KntDocumentUnreadableException(Reason(ex), ex);
        }
    }

    private static string Reason(Exception ex) => ex switch
    {
        UglyToad.PdfPig.Exceptions.PdfDocumentEncryptedException => "the PDF is protected with a password",
        _ => ex.Message
    };

    private static List<DocumentPageDto> OnePage(string text) => [new DocumentPageDto { Number = 1, Text = text }];

    #region PDF

    private static List<DocumentPageDto> ReadPdf(Stream content)
    {
        using var document = PdfDocument.Open(content);
        var pages = new List<DocumentPageDto>();
        foreach (var page in document.GetPages())
        {
            var images = page.GetImages().ToList();
            pages.Add(new DocumentPageDto
            {
                Number = page.Number,
                Text = ContentOrderTextExtractor.GetText(page),
                NeedsTranscription = DocumentTextRules.NeedsTranscription(page.Letters.Count, images.Count, LargestImageCoverage(page, images))
            });
        }
        return pages;
    }

    // The part of the page its largest image covers (0 to 1).
    private static double LargestImageCoverage(Page page, List<IPdfImage> images)
    {
        var area = page.Width * page.Height;
        if (area <= 0 || images.Count == 0)
            return 0;
        return Math.Min(1, images.Max(i => Math.Abs(i.BoundingBox.Area)) / area);
    }

    #endregion

    #region DOCX and ODT

    // The elements of each format: with text, paragraphs, skipped (with what is inside), tab, line breaks, spaces (ODT), and the
    // rows and cells of the tables. What is in w:delText (deleted with changes tracked) and w:instrText (the code of a field) is
    // not in w:t. A text box comes twice in a DOCX: mc:Fallback is the copy for old versions of Word.
    private sealed record ZippedText(string Entry, string[] WithText, string[] Paragraphs, string[] Skipped, string Tab, string[] Breaks,
        string Spaces, string Row, string Cell);

    private static readonly ZippedText Docx = new("word/document.xml", ["w:t"], ["w:p"], ["mc:Fallback"], "w:tab", ["w:br", "w:cr"], null,
        "w:tr", "w:tc");

    private static readonly ZippedText Odt = new("content.xml", ["text:p", "text:h"], ["text:p", "text:h"],
        ["office:annotation", "text:note-citation", "office:forms"], "text:tab", ["text:line-break"], "text:s",
        "table:table-row", "table:table-cell");

    private const string CellSeparator = " | ";

    private static string ReadOdt(Stream content)
    {
        using var zip = new ZipArchive(content, ZipArchiveMode.Read, leaveOpen: true);
        var manifest = zip.GetEntry("META-INF/manifest.xml");
        if (manifest != null && manifest.Length <= MaxUnzippedLength)
        {
            using var reader = new StreamReader(manifest.Open());
            if (reader.ReadToEnd().Contains("encryption-data", StringComparison.Ordinal))
                throw new KntDocumentUnreadableException("the document is protected with a password");
        }
        return ReadZippedText(zip, Odt);
    }

    private static string ReadZippedText(Stream content, ZippedText format)
    {
        using var zip = new ZipArchive(content, ZipArchiveMode.Read, leaveOpen: true);
        return ReadZippedText(zip, format);
    }

    private static string ReadZippedText(ZipArchive zip, ZippedText format)
    {
        var entry = zip.GetEntry(format.Entry) ?? throw new KntDocumentUnreadableException($"the document has no {format.Entry}");
        if (entry.Length > MaxUnzippedLength)
            throw new KntDocumentUnreadableException("the document is too big once unzipped");

        using var xml = XmlReader.Create(entry.Open(), new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });

        var text = new StringBuilder();
        var depth = 0;
        var cells = 0;
        xml.Read();
        while (!xml.EOF)
        {
            if (xml.NodeType == XmlNodeType.Element && format.Skipped.Contains(xml.Name))
            {
                xml.Skip();
                continue;
            }

            switch (xml.NodeType)
            {
                case XmlNodeType.Element:
                    if (format.WithText.Contains(xml.Name) && !xml.IsEmptyElement)
                        depth++;
                    if (xml.Name == format.Cell && !xml.IsEmptyElement)
                        cells++;
                    if (xml.Name == format.Tab)
                        text.Append('\t');
                    else if (format.Breaks.Contains(xml.Name))
                        text.Append(cells > 0 ? ' ' : '\n');
                    else if (xml.Name == format.Spaces)
                        text.Append(' ', int.TryParse(xml.GetAttribute("text:c"), out var count) ? Math.Clamp(count, 1, 1000) : 1);
                    else if (xml.Name == format.Cell && xml.IsEmptyElement)
                        text.Append(CellSeparator);
                    break;
                case XmlNodeType.EndElement:
                    if (format.WithText.Contains(xml.Name))
                        depth--;
                    // In a table, a row per line: the paragraphs of a cell go in it, the cells are separated.
                    if (format.Paragraphs.Contains(xml.Name))
                        text.Append(cells > 0 ? ' ' : '\n');
                    if (xml.Name == format.Cell)
                    {
                        cells--;
                        TrimEnd(text);
                        text.Append(CellSeparator);
                    }
                    else if (xml.Name == format.Row)
                    {
                        if (text.Length >= CellSeparator.Length && text.ToString(text.Length - CellSeparator.Length, CellSeparator.Length) == CellSeparator)
                            text.Length -= CellSeparator.Length;
                        TrimEnd(text);
                        text.Append('\n');
                    }
                    break;
                case XmlNodeType.Text or XmlNodeType.SignificantWhitespace or XmlNodeType.Whitespace when depth > 0:
                    text.Append(xml.Value);
                    break;
            }
            xml.Read();
        }
        return text.ToString();
    }

    private static void TrimEnd(StringBuilder text)
    {
        while (text.Length > 0 && text[^1] == ' ')
            text.Length--;
    }

    #endregion

    #region TXT and MD

    private static string ReadText(Stream content)
    {
        using var memory = new MemoryStream();
        var buffer = new byte[81920];
        int read;
        while ((read = content.Read(buffer, 0, buffer.Length)) > 0)
        {
            memory.Write(buffer, 0, read);
            if (memory.Length > MaxTextBytes)
                break;
        }
        var bytes = memory.ToArray();

        // With a byte order mark, its encoding; else UTF-8 if it is valid, else Latin-1 (old Windows files).
        using (var withMark = new StreamReader(new MemoryStream(bytes), Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
        {
            var text = withMark.ReadToEnd();
            if (!Equals(withMark.CurrentEncoding, Encoding.UTF8) || HasUtf8Mark(bytes))
                return text;
        }
        try
        {
            return new UTF8Encoding(false, throwOnInvalidBytes: true).GetString(bytes);
        }
        catch (DecoderFallbackException)
        {
            return Encoding.Latin1.GetString(bytes);
        }
    }

    private static bool HasUtf8Mark(byte[] bytes) => bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;

    #endregion
}

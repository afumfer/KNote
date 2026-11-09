using System.IO.Compression;
using System.Text;
using System.Xml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using PDFtoImage;
using SkiaSharp;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace KNote.ClientWin.Tests.Helpers;

// KNote Atlas 4.0: what the libraries of D4 do, for the tests that decide on them. Not KNote's reader: that one comes with 4.4 and
// 4.5, and these tests will use it then.
internal static class ResourceProbe
{
    // A draft: the rule is closed with the measure, in DocumentTextRules.
    public const int MaxLettersOfAScannedPage = 20;

    private const int JpegQuality = 85;
    private const long MaxUnzippedLength = 64_000_000;

    public sealed record PdfPageInfo(int Page, int Letters, int Images)
    {
        public bool NeedsTranscription => Letters < MaxLettersOfAScannedPage && Images > 0;
    }

    public static List<PdfPageInfo> InspectPdf(string path)
    {
        using var document = PdfDocument.Open(path);
        return Inspect(document);
    }

    public static List<PdfPageInfo> InspectPdf(byte[] pdf)
    {
        using var document = PdfDocument.Open(pdf);
        return Inspect(document);
    }

    private static List<PdfPageInfo> Inspect(PdfDocument document) =>
        document.GetPages().Select(page => new PdfPageInfo(page.Number, page.Letters.Count, page.GetImages().Count())).ToList();

    // In reading order: Page.Text gives the letters as they come in the file, and they may come in any order.
    public static List<string> ReadPdfText(string path)
    {
        using var document = PdfDocument.Open(path);
        return document.GetPages().Select(page => ContentOrderTextExtractor.GetText(page)).ToList();
    }

    public static List<string> ReadPdfRawText(string path)
    {
        using var document = PdfDocument.Open(path);
        return document.GetPages().Select(page => page.Text).ToList();
    }

    // A page as a JPEG with its longest side in maxSide pixels. Page: from 1.
    public static byte[] DrawPage(string path, int page, int maxSide)
    {
        using var stream = File.OpenRead(path);
        return DrawPage(stream, page, maxSide);
    }

    public static byte[] DrawPage(byte[] pdf, int page, int maxSide)
    {
        using var stream = new MemoryStream(pdf);
        return DrawPage(stream, page, maxSide);
    }

    private static byte[] DrawPage(Stream pdf, int page, int maxSide)
    {
        var size = Conversion.GetPageSize(pdf, page - 1, leaveOpen: true);
        pdf.Position = 0;

        // The other side, by its proportion. On white: a page without a background would come out black as a JPEG.
        var options = size.Width >= size.Height
            ? new RenderOptions(Width: maxSide, WithAspectRatio: true, BackgroundColor: SKColors.White)
            : new RenderOptions(Height: maxSide, WithAspectRatio: true, BackgroundColor: SKColors.White);

        using var bitmap = Conversion.ToImage(pdf, page - 1, leaveOpen: true, options: options);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, JpegQuality);
        return data.ToArray();
    }

    public static string ReadDocxWithOpenXml(string path)
    {
        using var document = WordprocessingDocument.Open(path, false);
        var body = document.MainDocumentPart?.Document?.Body;
        if (body == null)
            return "";

        var text = new StringBuilder();
        foreach (var paragraph in body.Descendants<Paragraph>())
            text.AppendLine(string.Concat(paragraph.Descendants<Text>().Select(t => t.Text)));
        return text.ToString();
    }

    // The two are a zip with the text in an XML. What is in w:delText (deleted with changes tracked) and w:instrText (the code of a
    // field) is not in w:t. A text box comes twice in a DOCX: mc:Fallback is the copy for old versions of Word.
    private static readonly ZippedText Docx = new("word/document.xml", ["w:t"], ["w:p"], ["mc:Fallback"], "w:tab", ["w:br", "w:cr"], null);

    private static readonly ZippedText Odt = new("content.xml", ["text:p", "text:h"], ["text:p", "text:h"],
        ["office:annotation", "text:note-citation", "office:forms"], "text:tab", ["text:line-break"], "text:s");

    public static string ReadDocxFromZip(string path) => ReadZippedText(path, Docx);

    public static string ReadOdt(string path)
    {
        using (var zip = ZipFile.OpenRead(path))
        {
            var manifest = zip.GetEntry("META-INF/manifest.xml");
            if (manifest != null)
            {
                using var reader = new StreamReader(manifest.Open());
                if (reader.ReadToEnd().Contains("encryption-data", StringComparison.Ordinal))
                    throw new InvalidDataException("The document is protected with a password.");
            }
        }

        return ReadZippedText(path, Odt);
    }

    private sealed record ZippedText(string Entry, string[] WithText, string[] Paragraphs, string[] Skipped, string Tab, string[] Breaks, string? Spaces);

    private static string ReadZippedText(string path, ZippedText format)
    {
        using var zip = ZipFile.OpenRead(path);
        var entry = zip.GetEntry(format.Entry) ?? throw new InvalidDataException($"The document has no {format.Entry}.");
        if (entry.Length > MaxUnzippedLength)
            throw new InvalidDataException("The document is too big once unzipped.");

        using var xml = XmlReader.Create(entry.Open(), new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });

        var text = new StringBuilder();
        var depth = 0;
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
                    if (xml.Name == format.Tab)
                        text.Append('\t');
                    else if (format.Breaks.Contains(xml.Name))
                        text.Append('\n');
                    else if (xml.Name == format.Spaces)
                        text.Append(' ', int.TryParse(xml.GetAttribute("text:c"), out var count) ? count : 1);
                    break;
                case XmlNodeType.EndElement:
                    if (format.WithText.Contains(xml.Name))
                        depth--;
                    if (format.Paragraphs.Contains(xml.Name))
                        text.Append('\n');
                    break;
                case XmlNodeType.Text or XmlNodeType.SignificantWhitespace or XmlNodeType.Whitespace when depth > 0:
                    text.Append(xml.Value);
                    break;
            }
            xml.Read();
        }
        return text.ToString();
    }

    // The same text, whatever its spaces and lines are: what two readers of a document have to agree on.
    public static string Comparable(string text) => string.Join(' ', text.Split((char[])null!, StringSplitOptions.RemoveEmptyEntries));

    #region Files for the tests

    // A document with a page of text and a page that is only an image, as a scanner leaves it: the words of the second one are not
    // in the file, only in the picture.
    public static byte[] MixedPdf(string textOfThePage, params string[] linesOfTheScan)
    {
        using var stream = new MemoryStream();
        using (var document = SKDocument.CreatePdf(stream))
        {
            using var font = new SKFont(SKTypeface.FromFamilyName("Arial"), 18);
            using var paint = new SKPaint { Color = SKColors.Black };
            var canvas = document.BeginPage(595, 842);
            canvas.DrawText(textOfThePage, 72, 100, SKTextAlign.Left, font, paint);
            document.EndPage();

            using var scan = Picture(1240, 1754, linesOfTheScan);
            canvas = document.BeginPage(595, 842);
            canvas.DrawImage(scan, new SKRect(0, 0, 595, 842), new SKSamplingOptions(SKFilterMode.Linear));
            document.EndPage();
            document.Close();
        }
        return stream.ToArray();
    }

    public static byte[] PhotoWithText(params string[] lines)
    {
        using var picture = Picture(1000, 700, lines);
        using var data = picture.Encode(SKEncodedImageFormat.Jpeg, JpegQuality);
        return data.ToArray();
    }

    private static SKImage Picture(int width, int height, params string[] lines)
    {
        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        surface.Canvas.Clear(SKColors.White);
        using var font = new SKFont(SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold), 60);
        using var paint = new SKPaint { Color = SKColors.Black };
        var y = 200f;
        foreach (var line in lines)
        {
            surface.Canvas.DrawText(line, 100, y, SKTextAlign.Left, font, paint);
            y += 100;
        }
        return surface.Snapshot();
    }

    public static void WriteDocx(string path, params string[] paragraphs)
    {
        using var document = WordprocessingDocument.Create(path, DocumentFormat.OpenXml.WordprocessingDocumentType.Document);
        var main = document.AddMainDocumentPart();
        main.Document = new Document(new Body(paragraphs.Select(p => new Paragraph(new Run(new Text(p))))));
        main.Document.Save();
    }

    public static void WriteOdt(string path, bool encrypted, params string[] paragraphs)
    {
        using var zip = ZipFile.Open(path, ZipArchiveMode.Create);

        var body = string.Concat(paragraphs.Select(p => $"<text:p>{System.Net.WebUtility.HtmlEncode(p)}</text:p>"));
        WriteEntry(zip, "content.xml",
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
            "<office:document-content xmlns:office=\"urn:oasis:names:tc:opendocument:xmlns:office:1.0\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\">" +
            $"<office:body><office:text>{body}</office:text></office:body></office:document-content>");

        var encryption = encrypted ? "<manifest:encryption-data manifest:checksum-type=\"SHA256\"/>" : "";
        WriteEntry(zip, "META-INF/manifest.xml",
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
            "<manifest:manifest xmlns:manifest=\"urn:oasis:names:tc:opendocument:xmlns:manifest:1.0\">" +
            $"<manifest:file-entry manifest:full-path=\"content.xml\" manifest:media-type=\"text/xml\">{encryption}</manifest:file-entry></manifest:manifest>");
    }

    private static void WriteEntry(ZipArchive zip, string name, string content)
    {
        using var writer = new StreamWriter(zip.CreateEntry(name).Open(), new UTF8Encoding(false));
        writer.Write(content);
    }

    #endregion
}

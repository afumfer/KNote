using System.IO.Compression;
using System.Net;
using System.Text;
using SkiaSharp;

namespace KNote.Tests.Helpers;

/// <summary>
/// Documents made by the tests of KNote Atlas, stage 4 (the reader and the index): PDF with text, scanned or with an OCR
/// layer, DOCX and ODT with paragraphs and a table, an ODT protected with a password and a damaged file.
/// </summary>
internal static class TestDocuments
{
    private const float PageWidth = 595, PageHeight = 842;

    // A PDF with a page of text for each string (a line per '\n').
    public static byte[] Pdf(params string[] pages) => Make(document =>
    {
        foreach (var page in pages)
        {
            DrawTextPage(document.BeginPage(PageWidth, PageHeight), page);
            document.EndPage();
        }
    });

    // A page of text and a page that is only an image, as a scanner leaves it: its words are only in the picture.
    public static byte[] MixedPdf(string textOfThePage, string textOfTheScan) => Make(document =>
    {
        DrawTextPage(document.BeginPage(PageWidth, PageHeight), textOfThePage);
        document.EndPage();

        using var scan = Picture(textOfTheScan);
        var canvas = document.BeginPage(PageWidth, PageHeight);
        canvas.DrawImage(scan, new SKRect(0, 0, PageWidth, PageHeight), new SKSamplingOptions(SKFilterMode.Linear));
        document.EndPage();
    });

    // A scanned page with an OCR layer: an image that covers the whole page and its text drawn over it.
    public static byte[] ScanWithOcrLayer(string ocrText) => Make(document =>
    {
        using var scan = Picture(ocrText);
        var canvas = document.BeginPage(PageWidth, PageHeight);
        canvas.DrawImage(scan, new SKRect(0, 0, PageWidth, PageHeight), new SKSamplingOptions(SKFilterMode.Linear));
        DrawTextPage(canvas, ocrText);
        document.EndPage();
    });

    // A PDF of scanned pages, one per string: a page whose string starts with "OCR:" brings its text over the image too (an OCR
    // layer), the others only the image.
    public static byte[] ScannedPdf(params string[] pages) => Make(document =>
    {
        foreach (var page in pages)
        {
            var ocr = page.StartsWith("OCR:", StringComparison.Ordinal);
            var text = ocr ? page[4..] : page;
            using var scan = Picture(text);
            var canvas = document.BeginPage(PageWidth, PageHeight);
            canvas.DrawImage(scan, new SKRect(0, 0, PageWidth, PageHeight), new SKSamplingOptions(SKFilterMode.Linear));
            if (ocr)
                DrawTextPage(canvas, text);
            document.EndPage();
        }
    });

    // A page of text with a small picture (a logo): not scanned.
    public static byte[] PdfWithLogo(string text) => Make(document =>
    {
        using var logo = Picture("LOGO");
        var canvas = document.BeginPage(PageWidth, PageHeight);
        canvas.DrawImage(logo, new SKRect(450, 40, 550, 110), new SKSamplingOptions(SKFilterMode.Linear));
        DrawTextPage(canvas, text);
        document.EndPage();
    });

    // A DOCX: the paragraphs, then a table with these rows (cells separated by '|').
    public static byte[] Docx(string[] paragraphs, params string[] tableRows)
    {
        var body = new StringBuilder();
        foreach (var paragraph in paragraphs)
            body.Append($"<w:p><w:r><w:t>{WebUtility.HtmlEncode(paragraph)}</w:t></w:r></w:p>");
        if (tableRows.Length > 0)
        {
            body.Append("<w:tbl>");
            foreach (var row in tableRows)
                body.Append("<w:tr>" + string.Concat(row.Split('|').Select(cell =>
                    $"<w:tc><w:p><w:r><w:t>{WebUtility.HtmlEncode(cell)}</w:t></w:r></w:p></w:tc>")) + "</w:tr>");
            body.Append("</w:tbl>");
        }

        return Zip(("word/document.xml",
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
            "<w:document xmlns:w=\"http://schemas.openxmlformats.org/wordprocessingml/2006/main\">" +
            $"<w:body>{body}</w:body></w:document>"));
    }

    // An ODT: the paragraphs, then a table with these rows (cells separated by '|'); encrypted, protected with a password.
    public static byte[] Odt(string[] paragraphs, bool encrypted = false, params string[] tableRows)
    {
        var body = new StringBuilder();
        foreach (var paragraph in paragraphs)
            body.Append($"<text:p>{WebUtility.HtmlEncode(paragraph)}</text:p>");
        if (tableRows.Length > 0)
        {
            body.Append("<table:table>");
            foreach (var row in tableRows)
                body.Append("<table:table-row>" + string.Concat(row.Split('|').Select(cell =>
                    $"<table:table-cell><text:p>{WebUtility.HtmlEncode(cell)}</text:p></table:table-cell>")) + "</table:table-row>");
            body.Append("</table:table>");
        }

        var encryption = encrypted ? "<manifest:encryption-data manifest:checksum-type=\"SHA256\"/>" : "";
        return Zip(
            ("content.xml",
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                "<office:document-content xmlns:office=\"urn:oasis:names:tc:opendocument:xmlns:office:1.0\" " +
                "xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\" xmlns:table=\"urn:oasis:names:tc:opendocument:xmlns:table:1.0\">" +
                $"<office:body><office:text>{body}</office:text></office:body></office:document-content>"),
            ("META-INF/manifest.xml",
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                "<manifest:manifest xmlns:manifest=\"urn:oasis:names:tc:opendocument:xmlns:manifest:1.0\">" +
                $"<manifest:file-entry manifest:full-path=\"content.xml\" manifest:media-type=\"text/xml\">{encryption}</manifest:file-entry></manifest:manifest>"));
    }

    public static byte[] Damaged => Encoding.ASCII.GetBytes("%PDF-1.7\nthis is not a PDF at all");

    private static byte[] Make(Action<SKDocument> draw)
    {
        using var stream = new MemoryStream();
        using (var document = SKDocument.CreatePdf(stream))
        {
            draw(document);
            document.Close();
        }
        return stream.ToArray();
    }

    private static void DrawTextPage(SKCanvas canvas, string text)
    {
        using var font = new SKFont(SKTypeface.FromFamilyName("Arial"), 14);
        using var paint = new SKPaint { Color = SKColors.Black };
        var y = 100f;
        foreach (var line in text.Split('\n'))
        {
            canvas.DrawText(line, 72, y, SKTextAlign.Left, font, paint);
            y += 20;
        }
    }

    private static SKImage Picture(string text)
    {
        using var surface = SKSurface.Create(new SKImageInfo(620, 877));
        surface.Canvas.Clear(SKColors.White);
        using var font = new SKFont(SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold), 40);
        using var paint = new SKPaint { Color = SKColors.Black };
        surface.Canvas.DrawText(text, 40, 200, SKTextAlign.Left, font, paint);
        return surface.Snapshot();
    }

    private static byte[] Zip(params (string Name, string Content)[] entries)
    {
        using var stream = new MemoryStream();
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            foreach (var (name, content) in entries)
            {
                using var writer = new StreamWriter(zip.CreateEntry(name).Open(), new UTF8Encoding(false));
                writer.Write(content);
            }
        return stream.ToArray();
    }
}

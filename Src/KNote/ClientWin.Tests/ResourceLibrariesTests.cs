using KNote.ClientWin.Tests.Helpers;
using SkiaSharp;

namespace KNote.ClientWin.Tests;

// KNote Atlas 4.0: the libraries of D4 do what the stage counts on, and go on doing it after a package is updated. No model and
// no files of anyone: the documents are made here.
[TestClass]
public class ResourceLibrariesTests
{
    private static readonly byte[] Mixed = ResourceProbe.MixedPdf("KNote Atlas page of text 7351", "SCANNED PAGE 4821");

    private string _folder = null!;

    [TestInitialize]
    public void CreateFolder()
    {
        _folder = Path.Combine(Path.GetTempPath(), "KNoteResourceLibraries" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_folder);
    }

    [TestCleanup]
    public void DeleteFolder() => Directory.Delete(_folder, true);

    private string Save(byte[] content, string name = "test.pdf")
    {
        var path = Path.Combine(_folder, name);
        File.WriteAllBytes(path, content);
        return path;
    }

    private static int DarkPixels(byte[] jpeg, out int width, out int height)
    {
        using var bitmap = SKBitmap.Decode(jpeg);
        (width, height) = (bitmap.Width, bitmap.Height);
        var dark = 0;
        for (var y = 0; y < bitmap.Height; y += 2)
            for (var x = 0; x < bitmap.Width; x += 2)
                if (bitmap.GetPixel(x, y).Red < 128)
                    dark++;
        return dark;
    }

    [TestMethod]
    public void AMixedPdf_TellsTheScannedPageFromTheOneWithText()
    {
        var pages = ResourceProbe.InspectPdf(Mixed);

        Assert.HasCount(2, pages);
        Assert.IsFalse(pages[0].NeedsTranscription);
        Assert.AreEqual(0, pages[0].Images);
        Assert.IsTrue(pages[1].NeedsTranscription);
        Assert.AreEqual(0, pages[1].Letters);
    }

    [TestMethod]
    public void TheTextOfAPage_IsRead()
    {
        var pages = ResourceProbe.ReadPdfText(Save(Mixed));

        StringAssert.Contains(ResourceProbe.Comparable(pages[0]), "KNote Atlas page of text 7351");
        Assert.AreEqual("", pages[1].Trim());
    }

    [TestMethod]
    public void APage_IsDrawnWithItsLongestSide_AndOnWhite()
    {
        var jpeg = ResourceProbe.DrawPage(Mixed, 2, 1600);

        var dark = DarkPixels(jpeg, out var width, out var height);

        Assert.AreEqual(1600, height);
        Assert.AreEqual(1131, width, 1);
        Assert.IsGreaterThan(0, dark, "The text of the scan is not on the page.");
        Assert.IsLessThan(width * height / 4 / 4, dark, "Most of the page is dark: it should be white.");
    }

    // PDFium is not thread-safe by itself, and the Server draws pages for a job and for the assistant at once.
    [TestMethod]
    public void Pages_AreDrawnInParallel()
    {
        var path = Save(Mixed);

        Parallel.For(0, 16, i =>
        {
            var jpeg = i % 2 == 0 ? ResourceProbe.DrawPage(path, 1 + i % 2, 800) : ResourceProbe.DrawPage(Mixed, 1 + i % 2, 800);
            Assert.IsGreaterThan(0, DarkPixels(jpeg, out _, out _));
        });
    }

    // What matters is that an exception can be caught: a failure in native code would end the process, and this test with it.
    [TestMethod]
    [DataRow("empty")]
    [DataRow("garbage")]
    [DataRow("header")]
    [DataRow("truncated")]
    public void ABrokenPdf_FailsWithAnException(string kind)
    {
        var content = kind switch
        {
            "empty" => Array.Empty<byte>(),
            "garbage" => Enumerable.Range(0, 100_000).Select(i => (byte)(i * 31 % 251)).ToArray(),
            "header" => "%PDF-1.7\n"u8.ToArray(),
            _ => Mixed[..(Mixed.Length / 2)]
        };

        Assert.ThrowsExactly<PDFtoImage.Exceptions.PdfInvalidFormatException>(() => ResourceProbe.DrawPage(content, 1, 800));
        Assert.Throws<Exception>(() => ResourceProbe.InspectPdf(content));
    }

    [TestMethod]
    public void TheTwoReadersOfADocx_GiveTheSameText()
    {
        var path = Path.Combine(_folder, "test.docx");
        ResourceProbe.WriteDocx(path, "First paragraph, con acentos: canción.", "Second paragraph 7351.");

        var zipped = ResourceProbe.ReadDocxFromZip(path);

        Assert.AreEqual(ResourceProbe.Comparable(ResourceProbe.ReadDocxWithOpenXml(path)), ResourceProbe.Comparable(zipped));
        StringAssert.Contains(zipped, "First paragraph, con acentos: canción.\nSecond paragraph 7351.\n");
    }

    [TestMethod]
    public void AnOdt_IsRead()
    {
        var path = Path.Combine(_folder, "test.odt");
        ResourceProbe.WriteOdt(path, false, "First paragraph, con acentos: canción & más.", "Second paragraph 7351.");

        Assert.AreEqual("First paragraph, con acentos: canción & más.\nSecond paragraph 7351.\n", ResourceProbe.ReadOdt(path));
    }

    [TestMethod]
    public void AnOdtWithAPassword_IsNotRead()
    {
        var path = Path.Combine(_folder, "test.odt");
        ResourceProbe.WriteOdt(path, true, "Nothing.");

        Assert.ThrowsExactly<InvalidDataException>(() => ResourceProbe.ReadOdt(path));
    }
}

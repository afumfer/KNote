using System.Text;
using KNote.Ai;
using KNote.Model;
using KNote.Service.Core;
using KNote.Tests.Helpers;

namespace KNote.Tests;

/// <summary>
/// AiDocumentReader (KNote Atlas, stage 4, 4.4a) with documents made by the test: the pages of a PDF in reading order, the
/// scanned ones marked (also with an OCR layer, D1), DOCX and ODT with a table row per line, TXT in UTF-8 and Latin-1, and
/// the documents that can't be read (an encrypted ODT, a damaged file).
/// </summary>
[TestClass]
public class AiDocumentReaderTests
{
    private static readonly AiDocumentReader Reader = new();

    private static List<KNote.Model.Dto.DocumentPageDto> Read(byte[] content, EnumDocumentFormat format)
    {
        using var stream = new MemoryStream(content);
        return Reader.Read(stream, format);
    }

    [TestMethod]
    public void APdf_GivesItsPages_InOrder()
    {
        var pages = Read(TestDocuments.Pdf("The first page.\nWith two lines.", "The second page."), EnumDocumentFormat.Pdf);

        Assert.AreEqual(2, pages.Count);
        StringAssert.Contains(pages[0].Text, "The first page.");
        StringAssert.Contains(pages[0].Text, "With two lines.");
        StringAssert.Contains(pages[1].Text, "The second page.");
        Assert.IsFalse(pages.Any(p => p.NeedsTranscription));
    }

    [TestMethod]
    public void AScannedPage_IsMarked_AlsoWithAnOcrLayer_ButNotAPageWithALogo()
    {
        var mixed = Read(TestDocuments.MixedPdf("A page of text.", "SCANNED"), EnumDocumentFormat.Pdf);
        var ocr = Read(TestDocuments.ScanWithOcrLayer("Text of the OCR layer of the scan."), EnumDocumentFormat.Pdf);
        var logo = Read(TestDocuments.PdfWithLogo("A letter with the logo of the company on top of it."), EnumDocumentFormat.Pdf);

        Assert.IsFalse(mixed[0].NeedsTranscription);
        Assert.IsTrue(mixed[1].NeedsTranscription);
        Assert.IsTrue(ocr[0].NeedsTranscription);
        StringAssert.Contains(ocr[0].Text, "OCR layer", "its text stays until it is transcribed");
        Assert.IsFalse(logo[0].NeedsTranscription);
    }

    [TestMethod]
    public void ADocx_AndAnOdt_GiveTheirText_WithARowOfATablePerLine()
    {
        var docx = Read(TestDocuments.Docx(["First paragraph.", "Second paragraph."], "Model|Year", "QL|1984"), EnumDocumentFormat.Docx).Single();
        var odt = Read(TestDocuments.Odt(["Primer párrafo."], false, "Modelo|Año", "QL|1984"), EnumDocumentFormat.Odt).Single();

        StringAssert.Contains(docx.Text, "First paragraph.\nSecond paragraph.\n");
        StringAssert.Contains(docx.Text, "Model | Year\nQL | 1984\n");
        StringAssert.Contains(odt.Text, "Primer párrafo.\n");
        StringAssert.Contains(odt.Text, "Modelo | Año\nQL | 1984\n");
    }

    [TestMethod]
    public void AText_IsReadInUtf8_OrInLatin1()
    {
        var utf8 = Read(Encoding.UTF8.GetBytes("Canción de cuna"), EnumDocumentFormat.Text).Single();
        var latin1 = Read(Encoding.Latin1.GetBytes("Canción de cuna"), EnumDocumentFormat.Text).Single();
        var withMark = Read([.. Encoding.UTF8.GetPreamble(), .. Encoding.UTF8.GetBytes("Ñandú")], EnumDocumentFormat.Text).Single();

        Assert.AreEqual("Canción de cuna", utf8.Text);
        Assert.AreEqual("Canción de cuna", latin1.Text);
        Assert.AreEqual("Ñandú", withMark.Text);
    }

    [TestMethod]
    public void AnEncryptedOdt_ADamagedPdf_OrADocxThatIsNotAZip_CantBeRead()
    {
        Assert.ThrowsExactly<KntDocumentUnreadableException>(() => Read(TestDocuments.Odt(["Secret."], encrypted: true), EnumDocumentFormat.Odt));
        Assert.ThrowsExactly<KntDocumentUnreadableException>(() => Read(TestDocuments.Damaged, EnumDocumentFormat.Pdf));
        Assert.ThrowsExactly<KntDocumentUnreadableException>(() => Read(TestDocuments.Damaged, EnumDocumentFormat.Docx));
    }
}

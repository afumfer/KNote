using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// DocumentTextRules (KNote Atlas, stage 4, D1, D2, D5): which documents are read, which page is scanned, how the text is
/// cleaned and the format it is kept in (pages separated by control characters, the pending ones marked).
/// </summary>
[TestClass]
public class DocumentTextRulesTests
{
    [TestMethod]
    [DataRow("application/pdf", "x.bin", EnumDocumentFormat.Pdf)]
    [DataRow(null, "Manual.PDF", EnumDocumentFormat.Pdf)]
    [DataRow("application/octet-stream", "report.docx", EnumDocumentFormat.Docx)]
    [DataRow("application/vnd.oasis.opendocument.text", "x", EnumDocumentFormat.Odt)]
    [DataRow("", "notes.md", EnumDocumentFormat.Text)]
    [DataRow("text/plain", "readme", EnumDocumentFormat.Text)]
    [DataRow("image/png", "photo.png", EnumDocumentFormat.None)]
    [DataRow("", "sheet.xlsx", EnumDocumentFormat.None)]
    [DataRow("", "old.doc", EnumDocumentFormat.None)]
    public void FormatOf_ByTypeOrByName(string? fileType, string name, EnumDocumentFormat expected)
    {
        Assert.AreEqual(expected, DocumentTextRules.FormatOf(fileType!, name));
    }

    [TestMethod]
    public void OnlyAPdf_HasPages()
    {
        Assert.IsTrue(DocumentTextRules.IsPaged(EnumDocumentFormat.Pdf));
        Assert.IsFalse(DocumentTextRules.IsPaged(EnumDocumentFormat.Docx));
        Assert.IsFalse(DocumentTextRules.IsPaged(EnumDocumentFormat.Text));
    }

    [TestMethod]
    [DataRow(0, 1, 0.3, true, DisplayName = "an image without letters")]
    [DataRow(5, 1, 0.5, true, DisplayName = "almost no letters")]
    [DataRow(800, 1, 0.9, true, DisplayName = "an OCR layer over an image of the whole page")]
    [DataRow(800, 2, 0.1, false, DisplayName = "text with small pictures")]
    [DataRow(0, 0, 0, false, DisplayName = "an empty page")]
    public void NeedsTranscription(int letters, int images, double coverage, bool expected)
    {
        Assert.AreEqual(expected, DocumentTextRules.NeedsTranscription(letters, images, coverage));
    }

    [TestMethod]
    public void Clean_JoinsTheWordsCutAtTheEndOfALine_AndTakesOutControlCharacters()
    {
        var clean = DocumentTextRules.Clean("A speciali-\r\nzed tool\f with\u0001 marks   \n\n\n\nAnd X-\nRay stays.");

        Assert.AreEqual("A specialized tool with marks\n\nAnd X-\nRay stays.", clean);
    }

    [TestMethod]
    public void ThePages_AreKeptAndReadBack_WithTheOnesToTranscribe()
    {
        var pages = new[]
        {
            new DocumentPageDto { Number = 1, Text = "First page." },
            new DocumentPageDto { Number = 2, Text = "OCR layer", NeedsTranscription = true },
            new DocumentPageDto { Number = 3, Text = "" }
        };

        var text = DocumentTextRules.Compose(pages, paged: true);
        var read = DocumentTextRules.Pages(text);

        Assert.AreEqual("First page.\f\u0001OCR layer\f", text);
        Assert.AreEqual(3, read.Count);
        Assert.AreEqual((2, "OCR layer", true), (read[1].Number, read[1].Text, read[1].NeedsTranscription));
        Assert.AreEqual(EnumExtractedState.Partial, DocumentTextRules.StateOf(read));
        Assert.AreEqual(EnumExtractedState.Complete, DocumentTextRules.StateOf(read.Where(p => !p.NeedsTranscription)));
    }

    [TestMethod]
    public void ADocumentWithoutPages_IsOne()
    {
        var text = DocumentTextRules.Compose([new DocumentPageDto { Number = 1, Text = "One\r\ntext." }], paged: false);

        Assert.AreEqual("One\ntext.", text);
        Assert.AreEqual(1, DocumentTextRules.Pages(text).Count);
        Assert.AreEqual(0, DocumentTextRules.Pages(null!).Count);
    }
}

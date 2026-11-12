using KNote.Model;

namespace KNote.Tests;

/// <summary>
/// What the assistant receives of a document (KNote Atlas, stage 4, D14, 4.7a): its pages with their headings and marks, cut at a page
/// at the maximum with how to ask for the next ones, a document without pages cut at a word; the pages it looks at; and a document by
/// its name.
/// </summary>
[TestClass]
public class AiDocumentToolRulesTests
{
    private static readonly string P = DocumentTextRules.PendingMark.ToString();
    private static readonly string T = DocumentTextRules.TranscribedMark.ToString();
    private static string Kept(params string[] pages) => string.Join(DocumentTextRules.PageSeparator, pages);

    [TestMethod]
    public void ForModel_GivesThePagesWithTheirMarks()
    {
        var text = Kept("One.", T + "Two, transcribed.", P + "old ocr", P);

        var all = AiDocumentToolRules.ForModel("m.pdf", text, paged: true);

        Assert.AreEqual("m.pdf: 4 pages. Pages 1–4.\n---\n[Page 1]\nOne.\n\n" +
            "[Page 2: automatic transcription of a scanned page, it may have mistakes]\nTwo, transcribed.\n\n" +
            "[Page 3: scanned, not transcribed yet; look at it with view_document_pages. Its old OCR text, maybe wrong:]\nold ocr\n\n" +
            "[Page 4: scanned, not transcribed yet; look at it with view_document_pages]\n---", all);
        StringAssert.StartsWith(AiDocumentToolRules.ForModel("m.pdf", text, true, fromPage: 2, toPage: 2), "m.pdf: 4 pages. Pages 2–2 (ask fromPage=3 for the next ones).");
        Assert.AreEqual("m.pdf: nothing could be read from it.", AiDocumentToolRules.ForModel("m.pdf", "", true));
    }

    [TestMethod]
    public void ForModel_CutsAtAPage_AndSaysHowToGoOn()
    {
        var text = Kept(new string('a', 60), new string('b', 60), new string('c', 60));

        var cut = AiDocumentToolRules.ForModel("m.pdf", text, true, maxLength: 100);
        var alone = AiDocumentToolRules.ForModel("m.pdf", Kept(string.Join(" ", Enumerable.Repeat("word", 40)), "Two."), true, maxLength: 50);
        var letter = AiDocumentToolRules.ForModel("a.docx", string.Join(" ", Enumerable.Repeat("word", 40)), false, maxLength: 50);

        StringAssert.StartsWith(cut, "m.pdf: 3 pages. Pages 1–1 (cut at 100 characters: ask fromPage=2 for the next ones).");
        Assert.IsFalse(cut.Contains("bbb"));
        StringAssert.Contains(alone, "Pages 1–1 (cut at 50 characters", "a page longer than the maximum, cut");
        StringAssert.Contains(alone, "…");
        StringAssert.StartsWith(letter, "a.docx (cut: its first 50 of 199 characters):");
    }

    [TestMethod]
    public void PickPages_AndFind()
    {
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, AiDocumentToolRules.PickPages(10, null).Pages);
        CollectionAssert.AreEqual(new[] { 2, 5, 7 }, AiDocumentToolRules.PickPages(10, [2, 5, 2, 7, 9]).Pages);
        StringAssert.Contains(AiDocumentToolRules.PickPages(10, [11]).Error, "there is no page 11");

        string[] files = ["manual.pdf", "scan.pdf"];
        Assert.AreEqual("scan.pdf", AiDocumentToolRules.Find(files, f => f, "SCAN.PDF", "The note").Document);
        Assert.AreEqual("manual.pdf", AiDocumentToolRules.Find(files, f => f, "manual", "The note").Document, "by part of its name");
        StringAssert.Contains(AiDocumentToolRules.Find(files, f => f, null, "The note").Error, "say which document");
        Assert.AreEqual("manual.pdf", AiDocumentToolRules.Find(new[] { "manual.pdf" }, f => f, null, "The note").Document);
        StringAssert.Contains(AiDocumentToolRules.Find(Array.Empty<string>(), f => f, null, "The note #3").Error, "The note #3 has no documents");
    }
}

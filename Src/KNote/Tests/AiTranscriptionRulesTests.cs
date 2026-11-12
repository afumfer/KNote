using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// The transcription of the scanned pages (KNote Atlas, stage 4, 4.5): the page transcribed in the kept text (its mark, the other
/// pages as they were), the transcriptions kept when a document is read again, the pages to transcribe with and without the OCR
/// ones, the request and the cleaning of the answer (the images the model invented in 4.0, the code block, «(no text)»), and the
/// scanned documents and the estimate of «Process pending resources».
/// </summary>
[TestClass]
public class AiTranscriptionRulesTests
{
    private static string Kept(params string[] pages) => string.Join(DocumentTextRules.PageSeparator, pages);

    private static readonly string P = DocumentTextRules.PendingMark.ToString();
    private static readonly string T = DocumentTextRules.TranscribedMark.ToString();

    [TestMethod]
    public void Transcribe_ReplacesThePendingPage_AndMarksIt()
    {
        var text = Kept("Page one.", P + "OCR layer", P);

        var once = DocumentTextRules.Transcribe(text, 2, "The real text.");
        var pages = DocumentTextRules.Pages(once);

        Assert.AreEqual(Kept("Page one.", T + "The real text.", P), once);
        Assert.AreEqual((false, true, false), (pages[1].NeedsTranscription, pages[1].Transcribed, pages[0].Transcribed));
        Assert.AreEqual("The real text.", pages[1].Text);
        Assert.AreEqual(EnumExtractedState.Partial, DocumentTextRules.StateOf(pages));
        Assert.AreEqual(EnumExtractedState.Complete, DocumentTextRules.StateOf(DocumentTextRules.Pages(DocumentTextRules.Transcribe(once, 3, ""))));
        Assert.IsNull(DocumentTextRules.Transcribe(once, 2, "Again."), "already transcribed");
        Assert.IsNull(DocumentTextRules.Transcribe(once, 1, "A page of text."), "not scanned");
        Assert.IsNull(DocumentTextRules.Transcribe(once, 9, "Not there."));
        Assert.IsNull(DocumentTextRules.Transcribe(null!, 1, "Not read."));
    }

    [TestMethod]
    public void ReadingAgain_KeepsTheTranscriptions_OnlyWithTheSamePages()
    {
        var previous = Kept("Page one.", T + "Transcribed.", P + "OCR");
        var read = new List<DocumentPageDto>
        {
            new() { Number = 1, Text = "Page one, read better." },
            new() { Number = 2, Text = "OCR of page two", NeedsTranscription = true },
            new() { Number = 3, Text = "OCR", NeedsTranscription = true }
        };

        var kept = DocumentTextRules.KeepTranscriptions(previous, read);
        var other = DocumentTextRules.KeepTranscriptions(previous, read.Take(2).ToList());

        Assert.AreEqual(Kept("Page one, read better.", T + "Transcribed.", P + "OCR"), DocumentTextRules.Compose(kept, paged: true));
        Assert.IsTrue(other[1].NeedsTranscription, "another number of pages: another document");
        Assert.AreSame(read, DocumentTextRules.KeepTranscriptions(Kept("A", P + "B", P), read), "nothing transcribed: as read");
    }

    [TestMethod]
    public void PagesToTranscribe_WithAndWithoutTheOcrOnes()
    {
        var text = Kept("Text.", P, P + "OCR layer", T + "Done.");

        CollectionAssert.AreEqual(new[] { 2, 3 }, DocumentTextRules.PagesToTranscribe(text));
        CollectionAssert.AreEqual(new[] { 2 }, DocumentTextRules.PagesToTranscribe(text, withOcrLayer: false));

        var scanned = ResourceProcessingRules.ScannedDocuments(
        [
            new ResourceTextDto { ResourceId = Guid.NewGuid(), Name = "a.pdf", State = EnumExtractedState.Partial, Text = text },
            new ResourceTextDto { ResourceId = Guid.NewGuid(), Name = "b.pdf", State = EnumExtractedState.Complete, Text = "Done." },
            new ResourceTextDto { ResourceId = Guid.NewGuid(), Name = "c.pdf", State = EnumExtractedState.Partial, Text = null }
        ]).Single();
        Assert.AreEqual((4, "a.pdf"), (scanned.PagesCount, scanned.Name));
        CollectionAssert.AreEqual(new[] { 2, 3 }, scanned.PendingPages);
        CollectionAssert.AreEqual(new[] { 3 }, scanned.OcrPages);
        CollectionAssert.AreEqual(new[] { 2 }, scanned.PagesToTranscribe(withOcrLayer: false));
    }

    [TestMethod]
    public void TheMarks_AreNotLettersNorNumbers()
    {
        foreach (var mark in new[] { DocumentTextRules.PendingMark, DocumentTextRules.TranscribedMark, DocumentTextRules.PageSeparator })
            Assert.IsTrue(char.IsControl(mark));
        Assert.AreEqual("", DocumentTextRules.Clean(T + P + "\f"), "cleaning a text removes them");
    }

    [TestMethod]
    public void Request_NamesThePageAndTheDocument_AndEndsWithTheLanguage()
    {
        Assert.AreEqual("This is page 3 of 12 of the scanned document «ql.pdf».\n\nTranscribe all the text of this page, in its own language, without translating it.",
            AiTranscriptionRules.Request("ql.pdf", 3, 12));
        StringAssert.StartsWith(AiTranscriptionRules.Request("", 1, 1), "This is page 1 of a scanned document.");
    }

    [TestMethod]
    [DataRow("10 PRINT \"HELLO\"\n20 GOTO 10", "10 PRINT \"HELLO\"\n20 GOTO 10")]
    [DataRow("```\nCHAPTER 4\n```", "CHAPTER 4")]
    [DataRow("```markdown\n# CHAPTER 4\n\nText.\n```", "# CHAPTER 4\n\nText.")]
    [DataRow("CHAPTER 4\n![Image](https://i.imgur.com/your_image_link_here)\nA figure.", "CHAPTER 4\n\nA figure.")]
    [DataRow("(no text)", "")]
    [DataRow("(No text).", "")]
    [DataRow("A text with (no text) inside.", "A text with (no text) inside.")]
    [DataRow("Line\r\n\r\n\r\n\r\nOther", "Line\n\nOther")]
    public void CleanTranscription(string answer, string expected)
    {
        Assert.AreEqual(expected, AiTranscriptionRules.CleanTranscription(answer));
    }

    [TestMethod]
    public void TheEstimate_CountsThePages_WithTheirOwnPace()
    {
        Assert.AreEqual(30.0 + 8 + 40, ResourceProcessingRules.EstimateSeconds(10, 3.0, 4, 2.0, 5, 8.0));
        Assert.IsNull(ResourceProcessingRules.EstimateSeconds(10, 3.0, 4, 2.0, 5, null), "pages without a pace of their own");
        Assert.AreEqual(38.0, ResourceProcessingRules.EstimateSeconds(10, 3.0, 4, 2.0), "no pages, no pace needed");
    }

    [TestMethod]
    public void TheTexts_OfTheRun_SayThePages()
    {
        var status = new ResourceProcessingStatusDto { PendingPages = 12, OcrPages = 4, ScannedDocuments = 2, SecondsPerPage = 8 };
        var describer = new ResourceDescriberDto { Model = "gpt-test", IsPaid = true, SupportsImages = true };

        StringAssert.Contains(ResourceProcessingTexts.Counts(status), "Scanned pages: 12 to transcribe (4 with an OCR layer) in 2 documents.");
        Assert.AreEqual((0, 0, 12), ResourceProcessingTexts.ToProcess(status, new ResourceProcessingOptionsDto()));
        Assert.AreEqual((0, 0, 8), ResourceProcessingTexts.ToProcess(status, new ResourceProcessingOptionsDto { OcrPages = false }));
        Assert.IsTrue(ResourceProcessingTexts.HasWork(status, new ResourceProcessingOptionsDto { Photos = false, Documents = false }));
        Assert.IsFalse(ResourceProcessingTexts.HasWork(status, new ResourceProcessingOptionsDto { ScannedPages = false }));

        var confirmation = ResourceProcessingTexts.RunConfirmation(status, new ResourceProcessingOptionsDto { OcrPages = false }, describer);
        StringAssert.Contains(confirmation, "The AI transcribes 8 scanned pages with the model gpt-test.");
        StringAssert.Contains(confirmation, "It should take about 1 min");
        StringAssert.Contains(confirmation, "these pages are sent");

        var transcribing = new ResourceProcessingProgressDto
        {
            Phase = EnumResourceProcessingPhase.Processing, Step = EnumResourceProcessingStep.Pages, PagesDone = 3, PagesTotal = 8
        };
        var done = new ResourceProcessingProgressDto
        {
            Phase = EnumResourceProcessingPhase.Completed, PagesTotal = 8, PagesTranscribed = 7, ElapsedSeconds = 60
        };
        Assert.AreEqual("Transcribing scanned pages: 3 of 8", ResourceProcessingTexts.Progress(transcribing));
        Assert.AreEqual((3, 8), ResourceProcessingTexts.Bar(transcribing));
        StringAssert.StartsWith(ResourceProcessingTexts.Progress(done), "Done: 7 scanned pages transcribed in 1 min");
    }

    // 4.5b: one document in the apps.
    [TestMethod]
    public void TheViewers_MarkThePagesTranscribed_AndFindThePageOfACursor()
    {
        var readable = DocumentTexts.Readable(Kept("First.", T + "Transcribed.", P + "OCR"), paged: true);

        Assert.AreEqual(string.Join("\r\n", "── Page 1 ──", "First.", "", "── Page 2 ──", "[Automatic transcription]", "Transcribed.", "",

            "── Page 3 ──", "[Scanned page: transcription pending]", "OCR"), readable);
        Assert.AreEqual(1, DocumentTexts.PageAt(readable, 0));
        Assert.AreEqual(1, DocumentTexts.PageAt(readable, readable.IndexOf("First.")));
        Assert.AreEqual(2, DocumentTexts.PageAt(readable, readable.IndexOf("── Page 2")), "on its heading");
        Assert.AreEqual(2, DocumentTexts.PageAt(readable, readable.IndexOf("Transcribed.")));
        Assert.AreEqual(3, DocumentTexts.PageAt(readable, readable.Length));
        Assert.IsNull(DocumentTexts.PageAt(DocumentTexts.Readable("A letter.", paged: false), 3), "no pages");
        Assert.IsNull(DocumentTexts.PageAt("", 0));
    }

    [TestMethod]
    public void TheTexts_OfOneDocument()
    {
        var local = new ResourceDescriberDto { Model = "qwen3.5:9b", SupportsImages = true };
        var paid = new ResourceDescriberDto { Model = "gpt-test", IsPaid = true, SupportsImages = true };

        Assert.AreEqual("The AI transcribes 12 scanned pages of this document with the model qwen3.5:9b, one after another. It should take about " +
            "1 min 36 s. Each page is kept as soon as it is transcribed: canceling keeps them, and you can go on later.",
            DocumentTexts.TranscriptionConfirmation(12, local, 8));
        Assert.IsFalse(DocumentTexts.TranscriptionConfirmation(12, local, null).Contains("take about"), "without a pace, no estimate");
        StringAssert.Contains(DocumentTexts.TranscriptionConfirmation(1, paid, null), "gpt-test is a paid model: the pages are sent to its provider.");
        Assert.AreEqual("Transcribing page 3 of 12…", DocumentTexts.TranscriptionProgress(2, 12));
        Assert.AreEqual("11 of 12 scanned pages transcribed. The document was summarized too.", DocumentTexts.TranscriptionResult(11, 12, false, true));
        Assert.AreEqual("Canceled after 4 scanned pages transcribed: what is done is kept.", DocumentTexts.TranscriptionResult(4, 12, true, false));
    }

    [TestMethod]
    public void ADocumentWaitsForItsSummary_OnlyWithoutADescriptionOfItsOwn()
    {
        static ResourceInfoDto Pdf(string? description = null, bool locked = false, string? model = null) =>
            new() { Name = $"{Guid.NewGuid()}_scan.pdf", FileType = "application/pdf", Description = description ?? "scan.pdf", DescriptionByUser = locked, DescriptionModel = model };

        Assert.IsTrue(AiDocumentRules.WaitsForSummary(Pdf()));
        Assert.IsFalse(AiDocumentRules.WaitsForSummary(Pdf("The manual of the QL.")), "a person's");
        Assert.IsFalse(AiDocumentRules.WaitsForSummary(Pdf("A manual.", model: "qwen3.5:9b")), "the AI's");
        Assert.IsFalse(AiDocumentRules.WaitsForSummary(Pdf(locked: true)));
    }
}

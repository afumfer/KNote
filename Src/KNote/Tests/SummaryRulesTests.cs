using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// The rules of the summaries of KNote Atlas (stage 2, D11, D18, D21): the pieces of a document, the batches that fit in the
/// context, the estimate and the texts of each call and of the note.
/// </summary>
[TestClass]
public class SummaryRulesTests
{
    private const char Page = DocumentTextRules.PageSeparator;

    [TestMethod]
    public void APagedDocument_GivesAPiecePerPage_WithItsLabel_AndTheScannedPagesWithoutText()
    {
        var text = $"First page{Page}{DocumentTextRules.PendingMark}{Page}{DocumentTextRules.PendingMark}Its OCR layer{Page}{Page}" +
                   $"{DocumentTextRules.TranscribedMark}Transcribed";

        var (pieces, missing) = SummaryPieces.FromDocument(text, paged: true);

        CollectionAssert.AreEqual(new[] { "p. 1", "p. 3", "p. 5" }, pieces.Select(p => p.Label).ToArray());
        CollectionAssert.AreEqual(new[] { "First page", "Its OCR layer", "Transcribed" }, pieces.Select(p => p.Text).ToArray());
        CollectionAssert.AreEqual(new[] { 2 }, missing, "the empty page that is not scanned is not missing");
    }

    [TestMethod]
    public void ADocumentWithoutPages_IsCutByParagraphs_WithoutLabels()
    {
        var paragraph = new string('a', 2_500);
        var text = string.Join("\n\n", Enumerable.Repeat(paragraph, 5));

        var (pieces, missing) = SummaryPieces.FromDocument(text, paged: false);

        Assert.IsTrue(pieces.All(p => p.Label == null));
        Assert.IsTrue(pieces.All(p => p.Text.Length <= SummaryPieces.MaxPieceLength));
        Assert.AreEqual(text.Replace("\n", "").Length, string.Concat(pieces.Select(p => p.Text)).Replace("\n", "").Length, "nothing is lost");
        Assert.HasCount(3, pieces);
        Assert.IsEmpty(missing);
    }

    [TestMethod]
    public void Cut_AParagraphLongerThanAPiece_BetweenWords()
    {
        var text = string.Join(" ", Enumerable.Repeat("word", 3_000));

        var parts = SummaryPieces.Cut(text, 1_000);

        Assert.IsTrue(parts.All(p => p.Length <= 1_000 && !p.StartsWith(' ') && p.Split(' ').All(w => w == "word")));
        Assert.AreEqual(3_000, parts.Sum(p => p.Split(' ').Length));
    }

    [TestMethod]
    public void Group_KeepsTheOrder_AndNeverGoesOverTheBudget()
    {
        var pieces = Enumerable.Range(1, 10).Select(i => new SummaryPiece($"p. {i}", new string('x', 1_000))).ToList();
        var each = SummaryBatches.PieceLength(pieces[0]);

        var batches = SummaryBatches.Group(pieces, SummaryBatches.PieceLength, each * 3);

        Assert.HasCount(4, batches);
        CollectionAssert.AreEqual(pieces, batches.SelectMany(b => b).ToList());
        Assert.IsTrue(batches.All(b => b.Sum(SummaryBatches.PieceLength) <= each * 3));
    }

    [TestMethod]
    public void TheBudget_IsTheContextLessTheAnswerTheRequestAndTheSystemPrompt()
    {
        Assert.AreEqual((8_192 - 800 - SummaryBatches.RequestOverheadTokens) * 4 - 2_000, SummaryBatches.InputCharacters(8_192, 2_000, 800));
        Assert.AreEqual(300 * 2 + 200, SummaryBatches.OutputTokens((int)EnumSummaryLength.Short));
    }

    [TestMethod]
    public void TheEstimate_WithoutJoins_AndWithThemWhenTheNotesDontFit()
    {
        var pieces = Enumerable.Range(1, 40).Select(i => new SummaryPiece($"p. {i}", new string('x', 3_000))).ToList();

        var fits = SummaryEstimate.For(pieces, batchCharacters: 20_000, finalCharacters: 100_000);
        var joins = SummaryEstimate.For(pieces, batchCharacters: 20_000, finalCharacters: 4_000);

        Assert.AreEqual(0, fits.Joins);
        Assert.AreEqual(fits.Batches + 1, fits.Calls);
        Assert.IsGreaterThan(0, joins.Joins);
        Assert.AreEqual(joins.Batches + joins.Joins + 1, joins.Calls);
        Assert.IsGreaterThan(fits.CharactersRead, joins.CharactersRead);
    }

    [TestMethod]
    public void TheTime_ComesFromThePaceOfTheModel()
    {
        Assert.IsNull(SummaryEstimate.Seconds(10_000, null), "without a summary before, no time");
        Assert.AreEqual(25, SummaryEstimate.Seconds(10_000, 2.5));
        Assert.AreEqual(2.5, SummaryEstimate.Pace(10_000, 25));
        Assert.IsNull(SummaryEstimate.Pace(0, 25));
    }

    [TestMethod]
    public void TheRequests_CitePagesOnlyWhenThereAre_AndTheLanguageGoesLast()
    {
        var paged = LongDocumentRules.BatchRequest("manual.pdf", [new SummaryPiece("p. 3", "Three"), new SummaryPiece("p. 4", "Four")]);
        var notPaged = LongDocumentRules.BatchRequest("notes.docx", [new SummaryPiece(null, "Text")]);
        var final = LongDocumentRules.FinalRequest("manual.pdf", 120, true, ["Notes one", "Notes two"], EnumSummaryLength.Long, "Spanish");
        var anyLanguage = LongDocumentRules.FinalRequest("notes.docx", 1, false, ["Notes"], EnumSummaryLength.Short, " ");

        StringAssert.Contains(paged, "(p. 3 to p. 4)");
        StringAssert.Contains(paged, "[p. 3]\nThree");
        StringAssert.Contains(paged, "(p. 12)");
        Assert.IsFalse(notPaged.Contains("(p. 12)") || notPaged.Contains('['));
        StringAssert.Contains(final, "about 2000 words");
        StringAssert.Contains(final, "(120 pages)");
        StringAssert.EndsWith(final, "Write the summary in Spanish.");
        StringAssert.EndsWith(anyLanguage, "Write the summary in the language of the document.");
        Assert.IsFalse(anyLanguage.Contains("(p. 12)"));
    }

    [TestMethod]
    public void TheNote_SaysWhereItComesFrom_AndThePagesLeftOut()
    {
        var document = new DocumentToSummarizeDto { FileName = "manual.pdf", NoteNumber = 42, Paged = true, PagesCount = 120 };
        var summary = new DocumentSummarySaveDto { Model = "qwen3.5:9b", Language = "Spanish", Summary = "  The summary.  ", MissingPages = [3, 7, 8, 9] };

        var text = LongDocumentRules.NoteDescription(document, summary, new DateTime(2026, 11, 14));

        StringAssert.StartsWith(text, "> Summary of the document **manual.pdf** of the note #42, 120 pages · qwen3.5:9b · 2026-11-14 · Spanish");
        StringAssert.Contains(text, "Scanned pages not read, without text yet: 3, 7–9.");
        StringAssert.EndsWith(text, "\n\nThe summary.");
        Assert.AreEqual("Summary of manual.pdf", LongDocumentRules.NoteTopic("manual.pdf"));
        Assert.AreEqual("1, 3–5, 9", LongDocumentRules.Ranges([9, 1, 3, 4, 5, 4]));
    }
}

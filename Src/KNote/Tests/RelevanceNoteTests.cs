using KNote.Model.Core;
using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// The text of «Note relevance» (KNote Atlas, stage 2, 2.3, chapter 30, D10): its two tables are read back from their hidden marks
/// (not from the columns), a topic with «|» or line breaks does not break them, a row without a mark is ignored, and the changes of
/// «Review selection» replace or take away a note's correction.
/// </summary>
[TestClass]
public class RelevanceNoteTests
{
    private static readonly Guid First = Guid.NewGuid();
    private static readonly Guid Second = Guid.NewGuid();

    [TestMethod]
    public void TheTables_AreReadBack_FromTheirMarks()
    {
        var corrections = new[]
        {
            new RelevanceCorrectionDto { NoteId = First, NoteNumber = 812, Topic = "Meeting | supplier\nnotes", Decision = EnumSelectionDecision.In,
                UserName = "Armando", Date = new DateTime(2026, 11, 16, 10, 0, 0) },
            new RelevanceCorrectionDto { NoteId = Second, NoteNumber = 455, Topic = "Shopping", Decision = EnumSelectionDecision.Out,
                UserName = "Marta", Date = new DateTime(2026, 11, 15) }
        };
        var answers = new[]
        {
            new RelevanceAiAnswerDto { NoteId = Second, NoteNumber = 455, Topic = "Shopping", Decision = EnumSelectionDecision.Out,
                Reason = "A personal reminder", ContentHash = "abc", Model = "qwen3.5:9b", Date = new DateTime(2026, 11, 16) }
        };

        var text = RelevanceNote.Write(corrections, answers);
        var (readCorrections, readAnswers) = RelevanceNote.Parse(text.Replace("\n", "\r\n"));

        StringAssert.Contains(text, "| #812 | Meeting \\| supplier notes | In | Armando | 2026-11-16");
        Assert.HasCount(2, readCorrections);
        var first = readCorrections.Single(c => c.NoteId == First);
        Assert.AreEqual((812, "Meeting | supplier notes", EnumSelectionDecision.In, "Armando"), (first.NoteNumber, first.Topic, first.Decision, first.UserName));
        Assert.AreEqual(new DateTime(2026, 11, 16, 10, 0, 0), first.Date);
        var answer = readAnswers.Single();
        Assert.AreEqual((Second, EnumSelectionDecision.Out, "A personal reminder", "abc", "qwen3.5:9b"),
            (answer.NoteId, answer.Decision, answer.Reason, answer.ContentHash, answer.Model));
    }

    [TestMethod]
    public void ARowWithoutItsMark_OrEditedByHand_IsIgnored()
    {
        var text = RelevanceNote.Write(new[] { new RelevanceCorrectionDto { NoteId = First, NoteNumber = 1, Topic = "A", Decision = EnumSelectionDecision.Out,
            UserName = "Ana", Date = DateTime.Today } }, null) + "| #2 | B | In | Ana | 2026-11-16 |\n";

        var (corrections, answers) = RelevanceNote.Parse(text.Replace("| Out | Ana", "| In | Ana"));

        Assert.AreEqual(EnumSelectionDecision.Out, corrections.Single().Decision, "the mark decides, not the column");
        Assert.IsEmpty(answers);
        Assert.IsEmpty(RelevanceNote.Parse(null).Corrections);
    }

    [TestMethod]
    public void TheChanges_ReplaceOrTakeAwayACorrection()
    {
        var kept = new List<RelevanceCorrectionDto>
        {
            new() { NoteId = First, NoteNumber = 1, Decision = EnumSelectionDecision.In, UserName = "Ana" },
            new() { NoteId = Second, NoteNumber = 2, Decision = EnumSelectionDecision.Out, UserName = "Ana" }
        };
        var third = Guid.NewGuid();
        var now = new DateTime(2026, 11, 16);

        var result = RelevanceNote.Apply(kept, new[]
        {
            new RelevanceCorrectionChangeDto { NoteId = First, Decision = EnumSelectionDecision.Out },
            new RelevanceCorrectionChangeDto { NoteId = Second, Decision = null },
            new RelevanceCorrectionChangeDto { NoteId = third, Decision = EnumSelectionDecision.In }
        }, id => id == Second ? null : (id == First ? 1 : 3, "Topic"), "Marta", now);

        Assert.HasCount(2, result);
        Assert.AreEqual((EnumSelectionDecision.Out, "Marta", now), (result.Single(c => c.NoteId == First).Decision, result.Single(c => c.NoteId == First).UserName, result.Single(c => c.NoteId == First).Date));
        Assert.AreEqual(3, result.Single(c => c.NoteId == third).NoteNumber);
        Assert.IsFalse(result.Any(c => c.NoteId == Second));
    }
}

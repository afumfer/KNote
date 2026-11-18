using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// The decision of a note of the selection (KNote Atlas, stage 2, 2.3, chapter 29, D8, S3, S5, S6): a person's correction above
/// the policy and the tags; the policy and the tags above the signals; with signals, in; without them, a very short note left the
/// same day out and a short one doubtful; the score whatever decides; and what «Review selection» keeps when a box changes.
/// </summary>
[TestClass]
public class NoteRelevanceRulesTests
{
    private static NoteRelevanceRules.Signals Signals(int words = 100, int tasks = 0, int active = 0, int resources = 0, int relations = 0,
        int citedBy = 0, bool sameDay = false) => new(words, tasks, active, resources, relations, citedBy, sameDay);

    private static NoteSelectionDto Decide(EnumNoiseReason noise, NoteRelevanceRules.Signals signals, RelevanceCorrectionDto? correction = null)
    {
        var note = new NoteSelectionDto { NoteNumber = 7 };
        NoteRelevanceRules.Decide(note, noise, signals, correction);
        return note;
    }

    [TestMethod]
    public void WithSignals_ItGoesIn_AndTheReasonSaysThem()
    {
        var note = Decide(EnumNoiseReason.In, Signals(words: 12, tasks: 3, active: 2, citedBy: 1, relations: 1));

        Assert.AreEqual(EnumSelectionDecision.In, note.Decision);
        Assert.AreEqual(EnumSelectionSource.Signals, note.Source);
        Assert.AreEqual("In: 2 tasks with activity in the period, 3 tasks in all, cited by 1 note of the selection, 1 relation, 12 words", note.Reason);
        Assert.AreEqual(2 * 3 + 3 + 2 + 1, note.Score);
    }

    [TestMethod]
    public void WithoutSignals_ItDependsOnItsSize()
    {
        var abandoned = Decide(EnumNoiseReason.In, Signals(words: 4, sameDay: true));
        var shortOne = Decide(EnumNoiseReason.In, Signals(words: 4));
        var doubtful = Decide(EnumNoiseReason.In, Signals(words: 39, sameDay: true));
        var longOne = Decide(EnumNoiseReason.In, Signals(words: 40, sameDay: true));

        Assert.AreEqual(EnumSelectionDecision.Out, abandoned.Decision);
        Assert.AreEqual("Out: a note of 4 words created and left the same day, without tasks, resources, relations or citations", abandoned.Reason);
        Assert.IsLessThan(0, abandoned.Score);
        Assert.AreEqual(EnumSelectionDecision.Doubtful, shortOne.Decision, "not left the same day");
        Assert.AreEqual(EnumSelectionDecision.Doubtful, doubtful.Decision);
        StringAssert.StartsWith(doubtful.Reason, "Doubtful: a short note (39 words)");
        Assert.AreEqual(EnumSelectionDecision.In, longOne.Decision);
        Assert.AreEqual("In: 40 words", longOne.Reason);
    }

    [TestMethod]
    public void ThePolicyAndTheTags_DecideBeforeTheSignals_AndTheScoreStays()
    {
        var folder = Decide(EnumNoiseReason.ExcludedFolder, Signals(tasks: 2, active: 2));
        var tag = Decide(EnumNoiseReason.KnowledgeTag, Signals(words: 3, sameDay: true));

        Assert.AreEqual((EnumSelectionDecision.Out, EnumSelectionSource.Policy), (folder.Decision, folder.Source));
        Assert.AreEqual("Out: its folder is left out by the noise policy", folder.Reason);
        Assert.IsGreaterThan(0, folder.Score);
        Assert.AreEqual((EnumSelectionDecision.In, EnumSelectionSource.Tag), (tag.Decision, tag.Source));
        Assert.AreEqual(EnumSelectionDecision.Out, Decide(EnumNoiseReason.NoKnowledgeTag, Signals()).Decision);
        Assert.AreEqual(EnumSelectionDecision.Out, Decide(EnumNoiseReason.ExcludedNoteType, Signals()).Decision);
    }

    [TestMethod]
    public void APersonsCorrection_DecidesAboveThePolicyAndTheTags()
    {
        var correction = new RelevanceCorrectionDto { Decision = EnumSelectionDecision.In, UserName = "Armando", Date = new DateTime(2026, 11, 16) };

        var note = Decide(EnumNoiseReason.NoKnowledgeTag, Signals(), correction);

        Assert.AreEqual((EnumSelectionDecision.In, EnumSelectionSource.Correction), (note.Decision, note.Source));
        Assert.AreEqual("In: included by Armando on 2026-11-16", note.Reason);
        Assert.AreEqual(EnumSelectionDecision.Out, note.ComputedDecision, "what KNote decides without it");
        Assert.ThrowsExactly<ArgumentException>(() => Decide(EnumNoiseReason.KNoteNote, Signals()));
    }

    [TestMethod]
    public void ReviewSelection_KeepsACorrection_OnlyWhenItDiffersFromKNotes()
    {
        var doubtful = new NoteSelectionDto { ComputedDecision = EnumSelectionDecision.Doubtful };
        var outOne = new NoteSelectionDto { ComputedDecision = EnumSelectionDecision.Out };

        Assert.AreEqual(EnumSelectionDecision.Out, NoteRelevanceRules.CorrectionFor(doubtful, include: false), "S5: unchecking a doubtful one");
        Assert.IsNull(NoteRelevanceRules.CorrectionFor(doubtful, include: true), "checked, as KNote decides: no correction");
        Assert.AreEqual(EnumSelectionDecision.In, NoteRelevanceRules.CorrectionFor(outOne, include: true));
        Assert.IsNull(NoteRelevanceRules.CorrectionFor(outOne, include: false), "back to KNote's: the correction is taken away");
    }

    [TestMethod]
    public void TheLists_AreInDoubtfulOut_TheHighestScoreFirst()
    {
        var notes = new[]
        {
            new NoteSelectionDto { NoteNumber = 1, Decision = EnumSelectionDecision.Out, Score = 9 },
            new NoteSelectionDto { NoteNumber = 2, Decision = EnumSelectionDecision.In, Score = 1 },
            new NoteSelectionDto { NoteNumber = 3, Decision = EnumSelectionDecision.In, Score = 5 },
            new NoteSelectionDto { NoteNumber = 4, Decision = EnumSelectionDecision.Doubtful, Score = 0 }
        };

        CollectionAssert.AreEqual(new[] { 3, 2, 4, 1 }, NoteRelevanceRules.Order(notes).Select(n => n.NoteNumber).ToList());
        Assert.AreEqual("2 in, 1 doubtful, 1 out. 3 notes of KNote's own, always out, are not listed.",
            NoteRelevanceRules.CountText(new NoteSelectionResultDto { Notes = notes.ToList(), KNoteNotes = 3 }));
    }
}

using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// What the two apps say of the semantic index (SemanticIndexTexts) and the key of an index (RepositoryAiModelsDto.GetIndexKey).
/// </summary>
[TestClass]
public class SemanticIndexTextsTests
{
    [TestMethod]
    [DataRow(0.2, "1 s")]
    [DataRow(42, "42 s")]
    [DataRow(125, "2 min 05 s")]
    [DataRow(3725, "1 h 02 min")]
    public void Duration(double seconds, string expected)
    {
        Assert.AreEqual(expected, SemanticIndexTexts.Duration(seconds));
    }

    [TestMethod]
    public void Progress_ByPhase()
    {
        Assert.AreEqual("Queued (position 2): one index is created at a time.",
            SemanticIndexTexts.Progress(new SemanticIndexProgressDto { Phase = EnumSemanticIndexPhase.Queued, QueuePosition = 2 }));
        Assert.AreEqual("Rebuilding: 3 of 10 notes · about 2 min 00 s left",
            SemanticIndexTexts.Progress(new SemanticIndexProgressDto { Phase = EnumSemanticIndexPhase.Indexing, Rebuild = true, NotesDone = 3, NotesTotal = 10, RemainingSeconds = 120 }));
        Assert.AreEqual("Failed: No model.",
            SemanticIndexTexts.Progress(new SemanticIndexProgressDto { Phase = EnumSemanticIndexPhase.Failed, Message = "No model." }));
    }

    [TestMethod]
    public void RunConfirmation_SaysHowManyNotes_HowLong_AndWhetherTheModelIsPaid()
    {
        var status = new SemanticIndexStatusDto
        {
            EmbeddingModel = "text-embedding-3-small", IndexKey = "text-embedding-3-small", TotalNotes = 120, PendingNotes = 30, NotesPerMinute = 60
        };

        var update = SemanticIndexTexts.RunConfirmation(status, rebuild: false, paidModel: true);
        var rebuild = SemanticIndexTexts.RunConfirmation(status, rebuild: true, paidModel: false);

        StringAssert.Contains(update, "30 notes with the model text-embedding-3-small. It should take about 30 s.");
        StringAssert.Contains(update, "paid model");
        StringAssert.Contains(rebuild, "calculates again the meaning of 120 notes");
        Assert.IsFalse(rebuild.Contains("paid"));
    }

    [TestMethod]
    public void IndexKey_IsTheModel_WithTheDimensionsWhenAskedFor()
    {
        Assert.IsNull(new RepositoryAiModelsDto().GetIndexKey());
        Assert.AreEqual("qwen3-embedding:4b", new RepositoryAiModelsDto { EmbeddingModel = " qwen3-embedding:4b " }.GetIndexKey());
        Assert.AreEqual("qwen3-embedding:4b@1024", new RepositoryAiModelsDto { EmbeddingModel = "qwen3-embedding:4b", EmbeddingDimensions = 1024 }.GetIndexKey());
    }

    [TestMethod]
    public void TheSearchByMeaning_NeedsAnIndexWithNotes_AndTheModel()
    {
        Assert.IsFalse(SemanticIndexTexts.CanSearch(null!));
        Assert.IsFalse(SemanticIndexTexts.CanSearch(new SemanticIndexStatusDto { IndexedNotes = 0, EmbeddingModelAvailable = true }));
        Assert.IsFalse(SemanticIndexTexts.CanSearch(new SemanticIndexStatusDto { IndexedNotes = 5, EmbeddingModelAvailable = false }));
        Assert.IsTrue(SemanticIndexTexts.CanSearch(new SemanticIndexStatusDto { IndexedNotes = 5, EmbeddingModelAvailable = true }));
    }

    [TestMethod]
    public void PendingInSearch_SaysHowManyNotes()
    {
        StringAssert.StartsWith(SemanticIndexTexts.PendingInSearch(1), "1 note is not up to date");
        StringAssert.StartsWith(SemanticIndexTexts.PendingInSearch(7), "7 notes are not up to date");
    }
}

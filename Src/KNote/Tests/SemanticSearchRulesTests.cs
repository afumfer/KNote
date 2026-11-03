using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>SemanticSearchRules: the query text, the ranking of the notes by their best fragment and the fragment shown.</summary>
[TestClass]
public class SemanticSearchRulesTests
{
    private static readonly Guid NoteA = new("00000000-0000-0000-0000-00000000000a");
    private static readonly Guid NoteB = new("00000000-0000-0000-0000-00000000000b");
    private static readonly Guid NoteC = new("00000000-0000-0000-0000-00000000000c");

    private static NoteChunkVectorDto Chunk(Guid noteId, params float[] vector) =>
        new() { NoteChunkId = Guid.NewGuid(), NoteId = noteId, SourceType = NoteChunkSourceType.Text, Vector = EmbeddingVector.Normalize(vector) };

    [TestMethod]
    public void QueryText_WithAnInstruction_IsInTheFormatOfTheModels_WithoutOne_TheQueryAlone()
    {
        Assert.AreEqual("Instruct: Find notes\nQuery:old computers", SemanticSearchRules.QueryText(" Find notes ", " old computers "));
        Assert.AreEqual("old computers", SemanticSearchRules.QueryText(null!, "old computers"));
        Assert.AreEqual("old computers", SemanticSearchRules.QueryText("  ", "old computers"));
    }

    [TestMethod]
    public void Rank_KeepsTheBestFragmentOfEachNote_TheMostSimilarFirst()
    {
        var bestOfA = Chunk(NoteA, 1, 0.2f);
        var chunks = new[] { Chunk(NoteA, 0.6f, 0.8f), bestOfA, Chunk(NoteB, 0.8f, 0.6f) };

        var ranked = SemanticSearchRules.Rank(chunks, EmbeddingVector.Normalize(new[] { 1f, 0f }));

        CollectionAssert.AreEqual(new[] { NoteA, NoteB }, ranked.Select(m => m.NoteId).ToList());
        Assert.AreEqual(bestOfA.NoteChunkId, ranked[0].NoteChunkId);
        Assert.AreEqual(EmbeddingVector.Dot(bestOfA.Vector, new[] { 1f, 0f }), ranked[0].Similarity, 1e-6f);
    }

    [TestMethod]
    public void Rank_LeavesOutTheNotesUnderTheMinimum_TheOnesNotAllowed_AndOtherDimensions()
    {
        var chunks = new[]
        {
            Chunk(NoteA, 1, 0),
            Chunk(NoteB, 0, 1),                 // similarity 0
            Chunk(NoteC, 1, 0),
            Chunk(NoteC, 1, 0, 0)               // another index's size: never compared
        };

        var ranked = SemanticSearchRules.Rank(chunks, new[] { 1f, 0f }, allowed: id => id != NoteC);

        CollectionAssert.AreEqual(new[] { NoteA }, ranked.Select(m => m.NoteId).ToList());
        Assert.AreEqual(0, SemanticSearchRules.Rank(chunks, new[] { 1f, 0f, 0f, 0f }).Count);
    }

    [TestMethod]
    public void Rank_TheMinimumIsIncluded_AndTiesAlwaysInTheSameOrder()
    {
        var chunks = new[] { Chunk(NoteB, 1, 0), Chunk(NoteA, 1, 0) };

        var ranked = SemanticSearchRules.Rank(chunks, new[] { 1f, 0f }, minSimilarity: 1f);

        CollectionAssert.AreEqual(new[] { NoteA, NoteB }, ranked.Select(m => m.NoteId).ToList());
    }

    [TestMethod]
    public void Fragment_InOneLine_AndCutAtTheEndOfAWord()
    {
        Assert.AreEqual("", SemanticSearchRules.Fragment(null!));
        Assert.AreEqual("## Repair Replaced the membrane.", SemanticSearchRules.Fragment("## Repair\n\n  Replaced the\tmembrane.\n"));

        var fragment = SemanticSearchRules.Fragment("one two three four five six", 15);

        Assert.AreEqual("one two three…", fragment);
    }

    [TestMethod]
    public void Fragment_OfAVeryLongWord_IsCutWhereTheLengthEnds()
    {
        Assert.AreEqual("ab abcdefghijkl…", SemanticSearchRules.Fragment("ab abcdefghijklmnopq", 15));
    }
}

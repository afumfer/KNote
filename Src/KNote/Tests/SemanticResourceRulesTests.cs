using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// SemanticSearchRules.RankResources (KNote Atlas, stage 4, D14): the best fragment of each resource of its own (its description, and a
/// document's text), from the minimum similarity, only the resources allowed, the most similar first; and where a fragment is.
/// </summary>
[TestClass]
public class SemanticResourceRulesTests
{
    private static float[] Unit(params float[] values) => EmbeddingVector.Normalize(values, values.Length);

    private static NoteChunkVectorDto Chunk(Guid noteId, NoteChunkSourceType type, Guid sourceId, params float[] vector) =>
        new() { NoteChunkId = Guid.NewGuid(), NoteId = noteId, SourceType = type, SourceId = sourceId, Vector = Unit(vector) };

    [TestMethod]
    public void RankResources_GivesTheBestFragmentOfEachResource_OfTheKindAllowed()
    {
        var note = Guid.NewGuid();
        var photo = Guid.NewGuid();
        var document = Guid.NewGuid();
        var other = Guid.NewGuid();
        var query = Unit(1, 0);
        var chunks = new[]
        {
            Chunk(note, NoteChunkSourceType.Text, Guid.Empty, 1, 0),
            Chunk(note, NoteChunkSourceType.Resource, photo, 1, 0.2f),
            Chunk(note, NoteChunkSourceType.Resource, document, 0.3f, 1),
            Chunk(note, NoteChunkSourceType.ResourceText, document, 1, 0.1f),
            Chunk(note, NoteChunkSourceType.Resource, other, 1, 0),
            Chunk(note, NoteChunkSourceType.Resource, Guid.NewGuid(), 0, 1)
        };

        var found = SemanticSearchRules.RankResources(chunks, query, (resource, _) => resource != other);

        CollectionAssert.AreEqual(new[] { document, photo }, found.Select(m => m.ResourceId).ToList(), "the best first; not the text of the note");
        Assert.AreEqual(NoteChunkSourceType.ResourceText, found[0].SourceType, "its page, better than its description");
        Assert.AreEqual(chunks[3].NoteChunkId, found[0].NoteChunkId);
        Assert.IsTrue(found.All(m => m.Similarity >= SemanticSearchRules.MinSimilarity));
    }

    // 4.6b: the notice of the notes not up to date.
    [TestMethod]
    public void TheNotice_SaysWhyTheNotesArePending_AndWhoCanUpdateThem()
    {
        Assert.AreEqual("2 notes are not up to date in the semantic index: they may be missing from the results or found by their previous " +
            "content. 1 of them waits for the full indexing (documents not read yet, or too long).",
            SemanticIndexTexts.PendingInSearch(2, 1, 0, canRunIndex: true));
        StringAssert.Contains(SemanticIndexTexts.PendingInSearch(1, 1, 0, true), "It waits for the full indexing");
        StringAssert.EndsWith(SemanticIndexTexts.PendingInSearch(1, 0, 0, false), "An Admin can bring the index up to date in KNote Atlas › Semantic index.");
        var withoutFiles = SemanticIndexTexts.PendingInSearch(3, 0, 1, true);
        StringAssert.StartsWith(withoutFiles, "2 notes are not up to date", "the ones without files are told apart");
        StringAssert.EndsWith(withoutFiles, "Besides, 1 note has documents whose file is not in this installation.");
        Assert.IsFalse(SemanticIndexTexts.ShowsPendingNotice(1, 1));
        Assert.IsTrue(SemanticIndexTexts.ShowsPendingNotice(2, 1));
    }

    [TestMethod]
    public void WhereAFragmentIs()
    {
        Assert.AreEqual("in manual.pdf, p. 12", SemanticSearchRules.FragmentPlace("manual.pdf", 12));
        Assert.AreEqual("in letter.docx", SemanticSearchRules.FragmentPlace("letter.docx", null));
        Assert.IsNull(SemanticSearchRules.FragmentPlace(null, 3));
        Assert.AreEqual("A note.", new NoteSearchHitDto { Fragment = "A note." }.FragmentShown());
        Assert.AreEqual("(in a.pdf, p. 2) Its text.", new NoteSearchHitDto { Fragment = "Its text.", FragmentResource = "a.pdf", FragmentPage = 2 }.FragmentShown());
    }
}

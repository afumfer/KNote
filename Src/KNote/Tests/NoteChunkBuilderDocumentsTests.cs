using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// The fragments of the documents in NoteChunkBuilder (KNote Atlas, stage 4, D8): one or more per page, with its number and
/// the heading «topic › file › p. N»; the small pages joined to the next; without pages, none; the text in the SourceHash.
/// And D9: the similar notes don't use them.
/// </summary>
[TestClass]
public class NoteChunkBuilderDocumentsTests
{
    private static NoteChunkSource Source(params NoteChunkDocument[] documents) => new()
    {
        Topic = "Sinclair QL",
        Documents = documents.ToList()
    };

    private static string Page(int length, char letter) => string.Join(" ", Enumerable.Repeat(new string(letter, 9), length / 10));

    [TestMethod]
    public void APdf_GivesFragmentsWithTheirPage()
    {
        var id = Guid.NewGuid();
        var text = string.Join(DocumentTextRules.PageSeparator, Page(600, 'a'), Page(2000, 'b'), "\u0001", Page(400, 'c'));

        var chunks = NoteChunkBuilder.Build(Source(new NoteChunkDocument(id, "manual.pdf", true, text)))
            .Chunks.Where(c => c.SourceType == NoteChunkSourceType.ResourceText).ToList();

        // Page 1; page 2 in two pieces; page 3 is a scanned one without text (left out); page 4.
        CollectionAssert.AreEqual(new int?[] { 1, 2, 2, 4 }, chunks.Select(c => c.Page).ToList());
        CollectionAssert.AreEqual(new[] { 0, 1, 2, 3 }, chunks.Select(c => c.ChunkIndex).ToList());
        Assert.IsTrue(chunks.All(c => c.SourceId == id && c.Content.Length <= NoteChunkBuilder.MaxChunkLength));
        Assert.AreEqual("Sinclair QL › manual.pdf › p. 2", chunks[1].Heading);
    }

    [TestMethod]
    public void SmallPages_JoinTheNext_AndADocxHasNoPages()
    {
        var pdf = NoteChunkBuilder.Build(Source(new NoteChunkDocument(Guid.NewGuid(), "short.pdf", true, "Title page\fIndex\fChapter 1.")))
            .Chunks.Where(c => c.SourceType == NoteChunkSourceType.ResourceText).ToList();
        var docx = NoteChunkBuilder.Build(Source(new NoteChunkDocument(Guid.NewGuid(), "letter.docx", false, "Dear friend.")))
            .Chunks.Single(c => c.SourceType == NoteChunkSourceType.ResourceText);

        Assert.AreEqual(1, pdf.Count);
        Assert.AreEqual(1, pdf[0].Page);
        Assert.AreEqual("Title page\n\nIndex\n\nChapter 1.", pdf[0].Content);
        Assert.IsNull(docx.Page);
        Assert.AreEqual("Sinclair QL › letter.docx", docx.Heading);
    }

    [TestMethod]
    public void TheTextOfADocument_ChangesTheSourceHash()
    {
        var id = Guid.NewGuid();
        var before = NoteChunkBuilder.Build(Source(new NoteChunkDocument(id, "a.pdf", true, "One.")));
        var after = NoteChunkBuilder.Build(Source(new NoteChunkDocument(id, "a.pdf", true, "Two.")));

        Assert.AreNotEqual(before.SourceHash, after.SourceHash);
    }

    [TestMethod]
    public void TheSimilarNotes_DontUseTheDocuments()
    {
        var noteId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        float[] Vector(float x) => [x, (float)Math.Sqrt(1 - x * x)];
        var own = new List<NoteChunkVectorDto>
        {
            new() { NoteId = noteId, SourceType = NoteChunkSourceType.Text, Vector = Vector(1) },
            new() { NoteId = noteId, SourceType = NoteChunkSourceType.ResourceText, Vector = Vector(0) }
        };
        var others = new List<NoteChunkVectorDto>
        {
            new() { NoteId = otherId, SourceType = NoteChunkSourceType.ResourceText, Vector = Vector(1) }
        };

        Assert.AreEqual(0, SemanticSearchRules.RankSimilar(own, others, 0f).Count, "only a document's text in common");
    }
}

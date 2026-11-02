using KNote.Model.Dto;
using KNote.Repository;
using KNote.Tests.Helpers;

namespace KNote.Tests.RepositoryParityTests;

/// <summary>
/// IKntSemanticIndexRepository (KNote Atlas, NoteChunks): what one ORM writes the other reads the same, and both answer
/// the same to the queries of the index (pending notes, vectors, watermark).
/// </summary>
[TestClass]
public class SemanticIndexParityTests
{
    private const string ModelA = "qwen3-embedding:0.6b";
    private const string ModelB = "text-embedding-3-small";

    private static async Task<Guid> AddNoteAsync(IKntRepository repo, string topic, Guid? noteTypeId = null)
    {
        var folderRes = await repo.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = $"Semantic index parity {Guid.NewGuid():N}" });
        Assert.IsTrue(folderRes.IsValid, folderRes.ErrorMessage);

        var noteId = Guid.NewGuid();
        var noteRes = await repo.Notes.AddAsync(new NoteDto
        {
            NoteId = noteId,
            Topic = topic,
            FolderId = folderRes.Entity.FolderId,
            NoteTypeId = noteTypeId,
            CreationDateTime = DateTime.Now,
            ModificationDateTime = DateTime.Now
        });
        Assert.IsTrue(noteRes.IsValid, noteRes.ErrorMessage);

        return noteId;
    }

    private static NoteChunkDto Chunk(NoteChunkSourceType sourceType, int index, string content, DateTime indexed, Guid? sourceId = null) => new()
    {
        SourceType = sourceType,
        SourceId = sourceId ?? Guid.Empty,
        ChunkIndex = index,
        Content = content,
        ContentHash = new string('c', 63) + index,
        SourceHash = new string('s', 64),
        Vector = new[] { 0.5f, -0.25f, index, 1e-7f },
        IndexedDateTime = indexed
    };

    private static async Task ReplaceAsync(IKntRepository repo, Guid noteId, string model, params NoteChunkDto[] chunks)
    {
        var res = await repo.SemanticIndex.ReplaceNoteChunksAsync(noteId, model, chunks.ToList());
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
    }

    private static async Task<List<NoteChunkDto>> ChunksOfAsync(IKntRepository repo, Guid noteId, string model)
    {
        var res = await repo.SemanticIndex.GetNoteChunksAsync(noteId, model);
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        return res.Entity;
    }

    private static async Task<List<Guid>> PendingAsync(IKntRepository repo, string model, List<Guid>? excluded = null, int max = 1000)
    {
        var res = await repo.SemanticIndex.GetPendingNoteIdsAsync(model, max, excluded);
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        return res.Entity;
    }

    [TestMethod]
    [DataRow("Dapper", "EntityFramework")]
    [DataRow("EntityFramework", "Dapper")]
    public async Task Chunks_WrittenByOneOrm_AreReadTheSameByTheOther(string writerOrm, string readerOrm)
    {
        using var db = new RepositoryTestDatabase();
        using var writer = db.CreateRepository(writerOrm);
        using var reader = db.CreateRepository(readerOrm);

        var noteId = await AddNoteAsync(writer, "Parity semantic note");
        var taskId = Guid.NewGuid();
        var indexed = new DateTime(2026, 11, 2, 10, 30, 15, 250);
        var givenId = Guid.NewGuid();
        var data = Chunk(NoteChunkSourceType.Data, 0, "Topic: Parity semantic note", indexed);
        data.NoteChunkId = givenId;
        await ReplaceAsync(writer, noteId, ModelA,
            Chunk(NoteChunkSourceType.Task, 0, "Task: Something\nStatus: pending", indexed, taskId),
            Chunk(NoteChunkSourceType.Text, 1, "Second piece \u203A with ñ and \u2026", indexed),
            data,
            Chunk(NoteChunkSourceType.Text, 0, "# First piece\n\n", indexed));

        var chunks = await ChunksOfAsync(reader, noteId, ModelA);

        CollectionAssert.AreEqual(
            new[] { NoteChunkSourceType.Data, NoteChunkSourceType.Text, NoteChunkSourceType.Text, NoteChunkSourceType.Task },
            chunks.Select(c => c.SourceType).ToArray());
        CollectionAssert.AreEqual(new[] { 0, 0, 1, 0 }, chunks.Select(c => c.ChunkIndex).ToArray());
        Assert.AreEqual(givenId, chunks[0].NoteChunkId);
        Assert.IsTrue(chunks.All(c => c.NoteChunkId != Guid.Empty && c.NoteId == noteId && c.Model == ModelA && !c.Stale));
        Assert.AreEqual(taskId, chunks[3].SourceId);
        Assert.AreEqual(Guid.Empty, chunks[1].SourceId);
        Assert.AreEqual("# First piece\n\n", chunks[1].Content);
        Assert.AreEqual("Second piece \u203A with ñ and \u2026", chunks[2].Content);
        Assert.AreEqual(new string('c', 63) + "1", chunks[2].ContentHash);
        Assert.AreEqual(new string('s', 64), chunks[2].SourceHash);
        Assert.AreEqual(4, chunks[2].Dimensions);
        CollectionAssert.AreEqual(new[] { 0.5f, -0.25f, 1f, 1e-7f }, chunks[2].Vector);
        Assert.AreEqual(indexed, chunks[2].IndexedDateTime);
    }

    [TestMethod]
    [DataRow("Dapper", "EntityFramework")]
    [DataRow("EntityFramework", "Dapper")]
    public async Task Replace_ReplacesTheRowsOfThatNoteAndModelOnly(string writerOrm, string readerOrm)
    {
        using var db = new RepositoryTestDatabase();
        using var writer = db.CreateRepository(writerOrm);
        using var reader = db.CreateRepository(readerOrm);

        var noteId = await AddNoteAsync(writer, "Replaced note");
        var otherId = await AddNoteAsync(writer, "Other note");
        var now = DateTime.Now;
        await ReplaceAsync(writer, noteId, ModelA, Chunk(NoteChunkSourceType.Data, 0, "old A", now), Chunk(NoteChunkSourceType.Text, 0, "old text", now));
        await ReplaceAsync(writer, noteId, ModelB, Chunk(NoteChunkSourceType.Data, 0, "B", now));
        await ReplaceAsync(writer, otherId, ModelA, Chunk(NoteChunkSourceType.Data, 0, "other", now));

        await ReplaceAsync(writer, noteId, ModelA, Chunk(NoteChunkSourceType.Data, 0, "new A", now));

        Assert.AreEqual("new A", (await ChunksOfAsync(reader, noteId, ModelA)).Single().Content);
        Assert.AreEqual("B", (await ChunksOfAsync(reader, noteId, ModelB)).Single().Content);
        Assert.AreEqual("other", (await ChunksOfAsync(reader, otherId, ModelA)).Single().Content);
    }

    [TestMethod]
    [DataRow("Dapper", "EntityFramework")]
    [DataRow("EntityFramework", "Dapper")]
    public async Task Replace_ThatFails_KeepsThePreviousRows(string writerOrm, string readerOrm)
    {
        using var db = new RepositoryTestDatabase();
        using var writer = db.CreateRepository(writerOrm);
        using var reader = db.CreateRepository(readerOrm);

        var noteId = await AddNoteAsync(writer, "Note of a failed replace");
        var now = DateTime.Now;
        await ReplaceAsync(writer, noteId, ModelA, Chunk(NoteChunkSourceType.Data, 0, "previous", now));

        // Two rows with the same key (NoteId, SourceType, SourceId, ChunkIndex, Model): the unique index refuses them.
        await Assert.ThrowsAsync<KntRepositoryException>(() => writer.SemanticIndex.ReplaceNoteChunksAsync(noteId, ModelA,
            new List<NoteChunkDto> { Chunk(NoteChunkSourceType.Text, 0, "one", now), Chunk(NoteChunkSourceType.Text, 0, "two", now) }));

        Assert.AreEqual("previous", (await ChunksOfAsync(reader, noteId, ModelA)).Single().Content);
    }

    [TestMethod]
    [DataRow("Dapper", "EntityFramework")]
    [DataRow("EntityFramework", "Dapper")]
    public async Task DeletingTheNote_DeletesItsRows(string writerOrm, string readerOrm)
    {
        using var db = new RepositoryTestDatabase();
        using var writer = db.CreateRepository(writerOrm);
        using var reader = db.CreateRepository(readerOrm);

        var noteId = await AddNoteAsync(writer, "Deleted note");
        await ReplaceAsync(writer, noteId, ModelA, Chunk(NoteChunkSourceType.Data, 0, "data", DateTime.Now));

        var deleteRes = await writer.Notes.DeleteAsync(noteId);
        Assert.IsTrue(deleteRes.IsValid, deleteRes.ErrorMessage);

        Assert.AreEqual(0, (await ChunksOfAsync(reader, noteId, ModelA)).Count);
        Assert.AreEqual(0, (await reader.SemanticIndex.GetIndexWatermarkAsync(ModelA)).Entity.Count);
    }

    [TestMethod]
    [DataRow("Dapper", "EntityFramework")]
    [DataRow("EntityFramework", "Dapper")]
    public async Task Pending_AreTheNotesWithoutRows_Stale_OrModifiedAfterIndexed(string writerOrm, string readerOrm)
    {
        using var db = new RepositoryTestDatabase();
        using var writer = db.CreateRepository(writerOrm);
        using var reader = db.CreateRepository(readerOrm);

        var withoutRows = await AddNoteAsync(writer, "Without rows");
        var indexed = await AddNoteAsync(writer, "Indexed");
        var stale = await AddNoteAsync(writer, "Stale");
        var modifiedAfter = await AddNoteAsync(writer, "Modified after indexed");
        var otherModel = await AddNoteAsync(writer, "Indexed with the other model");

        var later = DateTime.Now.AddMinutes(5);
        await ReplaceAsync(writer, indexed, ModelA, Chunk(NoteChunkSourceType.Data, 0, "indexed", later));
        await ReplaceAsync(writer, stale, ModelA, Chunk(NoteChunkSourceType.Data, 0, "stale", later));
        await ReplaceAsync(writer, stale, ModelB, Chunk(NoteChunkSourceType.Data, 0, "stale B", later));
        await ReplaceAsync(writer, modifiedAfter, ModelA, Chunk(NoteChunkSourceType.Data, 0, "old", DateTime.Now.AddDays(-1)));
        await ReplaceAsync(writer, otherModel, ModelB, Chunk(NoteChunkSourceType.Data, 0, "B", later));
        var staleRes = await writer.SemanticIndex.MarkStaleAsync(stale);
        Assert.IsTrue(staleRes.IsValid, staleRes.ErrorMessage);

        var pendingA = await PendingAsync(reader, ModelA);
        var pendingB = await PendingAsync(reader, ModelB);

        CollectionAssert.IsSubsetOf(new[] { withoutRows, stale, modifiedAfter, otherModel }, pendingA);
        CollectionAssert.DoesNotContain(pendingA, indexed);
        Assert.IsTrue((await ChunksOfAsync(reader, stale, ModelB)).Single().Stale, "MarkStale marks every model");
        CollectionAssert.Contains(pendingB, stale);
        CollectionAssert.DoesNotContain(pendingB, otherModel);
    }

    [TestMethod]
    [DataRow("Dapper", "EntityFramework")]
    [DataRow("EntityFramework", "Dapper")]
    public async Task Pending_LeavesOutTheExcludedTypes_AndHonorsMax_LastModifiedFirst(string writerOrm, string readerOrm)
    {
        using var db = new RepositoryTestDatabase();
        using var writer = db.CreateRepository(writerOrm);
        using var reader = db.CreateRepository(readerOrm);

        var noteTypeId = Guid.NewGuid();
        var typeRes = await writer.NoteTypes.AddAsync(new NoteTypeDto { NoteTypeId = noteTypeId, Name = "@ExcludedFromTheIndex" });
        Assert.IsTrue(typeRes.IsValid, typeRes.ErrorMessage);
        var excluded = await AddNoteAsync(writer, "Of an excluded type", noteTypeId);
        var first = await AddNoteAsync(writer, "First");
        await Task.Delay(20);
        var last = await AddNoteAsync(writer, "Last");

        var all = await PendingAsync(reader, ModelA);
        var withoutExcluded = await PendingAsync(reader, ModelA, new List<Guid> { noteTypeId });
        var top = await PendingAsync(reader, ModelA, max: 1);

        CollectionAssert.Contains(all, excluded);
        CollectionAssert.DoesNotContain(withoutExcluded, excluded);
        CollectionAssert.IsSubsetOf(new[] { first, last }, withoutExcluded);
        Assert.IsTrue(all.IndexOf(last) < all.IndexOf(first), "the last modified first");
        CollectionAssert.AreEqual(new[] { last }, top);
    }

    [TestMethod]
    [DataRow("Dapper", "EntityFramework")]
    [DataRow("EntityFramework", "Dapper")]
    public async Task Vectors_Watermark_Contents_AndDeleteModel(string writerOrm, string readerOrm)
    {
        using var db = new RepositoryTestDatabase();
        using var writer = db.CreateRepository(writerOrm);
        using var reader = db.CreateRepository(readerOrm);

        var noteId = await AddNoteAsync(writer, "Vectors note");
        var otherId = await AddNoteAsync(writer, "Other vectors note");
        var early = new DateTime(2026, 11, 1, 9, 0, 0);
        var late = new DateTime(2026, 11, 2, 9, 0, 0, 500);
        await ReplaceAsync(writer, noteId, ModelA, Chunk(NoteChunkSourceType.Data, 0, "data", early), Chunk(NoteChunkSourceType.Text, 0, "text", early));
        await ReplaceAsync(writer, otherId, ModelA, Chunk(NoteChunkSourceType.Text, 0, "late text", late));
        await ReplaceAsync(writer, otherId, ModelB, Chunk(NoteChunkSourceType.Data, 0, "B", late));

        var vectors = (await reader.SemanticIndex.GetVectorsAsync(ModelA)).Entity;
        var changed = (await reader.SemanticIndex.GetVectorsAsync(ModelA, late)).Entity;
        var watermark = (await reader.SemanticIndex.GetIndexWatermarkAsync(ModelA)).Entity;
        var empty = (await reader.SemanticIndex.GetIndexWatermarkAsync("no-model")).Entity;

        Assert.AreEqual(3, vectors.Count);
        CollectionAssert.AreEqual(new[] { 0.5f, -0.25f, 0f, 1e-7f }, vectors.First(v => v.SourceType == NoteChunkSourceType.Data).Vector);
        Assert.AreEqual(otherId, changed.Single().NoteId);
        Assert.AreEqual(NoteChunkSourceType.Text, changed.Single().SourceType);
        Assert.AreEqual(3, watermark.Count);
        Assert.AreEqual(late, watermark.LastIndexedDateTime);
        Assert.AreEqual(0, empty.Count);
        Assert.IsNull(empty.LastIndexedDateTime);

        var ids = vectors.Where(v => v.NoteId == noteId).Select(v => v.NoteChunkId).ToList();
        var contents = (await reader.SemanticIndex.GetChunkContentsAsync(ids)).Entity;
        CollectionAssert.AreEquivalent(new[] { "data", "text" }, contents.Select(c => c.Content).ToArray());
        Assert.IsTrue(contents.All(c => c.Vector == null && c.NoteId == noteId && c.Dimensions == 4));

        var deleteRes = await writer.SemanticIndex.DeleteModelAsync(ModelA);
        Assert.IsTrue(deleteRes.IsValid, deleteRes.ErrorMessage);
        Assert.AreEqual(0, (await reader.SemanticIndex.GetIndexWatermarkAsync(ModelA)).Entity.Count);
        Assert.AreEqual(1, (await reader.SemanticIndex.GetIndexWatermarkAsync(ModelB)).Entity.Count);
    }
}

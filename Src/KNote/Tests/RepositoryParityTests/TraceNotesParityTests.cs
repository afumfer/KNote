using KNote.Model.Dto;
using KNote.Repository;
using KNote.Tests.Helpers;
using Microsoft.Data.Sqlite;

namespace KNote.Tests.RepositoryParityTests;

/// <summary>
/// IKntTraceNoteRepository (Fase 1 of the TraceNotes feature): CRUD round-trip across ORMs
/// (CrudRoundTripParityTests-style) plus behavioral parity for the two directional queries
/// (NotesQueryParityTests-style) - GetAllByFromIdAsync/GetAllByToIdAsync map directly to the
/// two ClientWin lists agreed in Fase 0 (decision 4): "From" list in the UI = incoming
/// relations (ToId == the note being edited, so GetAllByToIdAsync), "To" list = outgoing
/// relations (FromId == the note being edited, so GetAllByFromIdAsync).
/// </summary>
[TestClass]
public class TraceNotesParityTests
{
    private static async Task<Guid> AddNoteAsync(IKntRepository repo, string topic)
    {
        var folderRes = await repo.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = $"TraceNote parity folder {Guid.NewGuid():N}" });
        Assert.IsTrue(folderRes.IsValid, folderRes.ErrorMessage);

        var noteId = Guid.NewGuid();
        var noteRes = await repo.Notes.AddAsync(new NoteDto
        {
            NoteId = noteId,
            Topic = topic,
            FolderId = folderRes.Entity.FolderId,
            CreationDateTime = DateTime.Now,
            ModificationDateTime = DateTime.Now
        });
        Assert.IsTrue(noteRes.IsValid, noteRes.ErrorMessage);

        return noteId;
    }

    // Inserted directly via SQL, bypassing IKntTraceNoteTypeRepository (covered separately in
    // CrudRoundTripParityTests), just to have a type id to reference from a TraceNoteDto here.
    //
    // The id parameter is passed as a Guid, not a pre-formatted string: Microsoft.Data.Sqlite gives
    // a raw Guid parameter a different physical storage than an explicit ToString(), and Dapper's/
    // EF's own Guid writes (via KntTraceNoteRepository) go through the same raw-Guid path - passing
    // a string here instead would store this row's id in a format the ORMs' own FK writes then never
    // match, which is exactly the mismatch that broke this test before this comment was added.
    private static void InsertTraceNoteTypeDirectly(RepositoryTestDatabase db, Guid id, string name)
    {
        using var connection = new SqliteConnection($"Data Source={db.DatabaseFilePath}");
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "INSERT INTO TraceNoteTypes (TraceNoteTypeId, Name) VALUES (@id, @name);";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@name", name);
        cmd.ExecuteNonQuery();
    }

    [TestMethod]
    [DataRow("Dapper", "EntityFramework")]
    [DataRow("EntityFramework", "Dapper")]
    public async Task TraceNote_WrittenByOneOrm_IsReadableByTheOther(string writerOrm, string readerOrm)
    {
        using var db = new RepositoryTestDatabase();
        using var writer = db.CreateRepository(writerOrm);
        using var reader = db.CreateRepository(readerOrm);

        var fromId = await AddNoteAsync(writer, "Parity From Note");
        var toId = await AddNoteAsync(writer, "Parity To Note");

        var traceNoteId = Guid.NewGuid();
        var traceNote = new TraceNoteDto { TraceNoteId = traceNoteId, FromId = fromId, ToId = toId, Order = 3, Weight = 1.5, TraceNoteTypeId = null };

        var addRes = await writer.TraceNotes.AddAsync(traceNote);
        Assert.IsTrue(addRes.IsValid, addRes.ErrorMessage);
        Assert.AreEqual(traceNoteId, addRes.Entity.TraceNoteId);

        var getRes = await reader.TraceNotes.GetAsync(traceNoteId);
        Assert.IsTrue(getRes.IsValid, getRes.ErrorMessage);
        Assert.AreEqual(fromId, getRes.Entity.FromId);
        Assert.AreEqual(toId, getRes.Entity.ToId);
        Assert.AreEqual(3, getRes.Entity.Order);
        Assert.AreEqual(1.5, getRes.Entity.Weight);
        Assert.IsNull(getRes.Entity.TraceNoteTypeId);
    }

    [TestMethod]
    [DataRow("Dapper", "EntityFramework")]
    [DataRow("EntityFramework", "Dapper")]
    public async Task TraceNote_UpdatedByOneOrm_IsVisibleToTheOther(string writerOrm, string readerOrm)
    {
        using var db = new RepositoryTestDatabase();
        using var writer = db.CreateRepository(writerOrm);
        using var reader = db.CreateRepository(readerOrm);

        var fromId = await AddNoteAsync(writer, "Parity From Note");
        var toId = await AddNoteAsync(writer, "Parity To Note");

        var typeId = Guid.NewGuid();
        InsertTraceNoteTypeDirectly(db, typeId, $"Parity type {typeId:N}");

        var traceNoteId = Guid.NewGuid();
        var addRes = await writer.TraceNotes.AddAsync(new TraceNoteDto { TraceNoteId = traceNoteId, FromId = fromId, ToId = toId, Order = 0, Weight = 1.0, TraceNoteTypeId = null });
        Assert.IsTrue(addRes.IsValid, addRes.ErrorMessage);

        var updated = addRes.Entity;
        updated.Order = 7;
        updated.Weight = 2.5;
        updated.TraceNoteTypeId = typeId;
        var updateRes = await writer.TraceNotes.UpdateAsync(updated);
        Assert.IsTrue(updateRes.IsValid, updateRes.ErrorMessage);

        var getRes = await reader.TraceNotes.GetAsync(traceNoteId);
        Assert.IsTrue(getRes.IsValid, getRes.ErrorMessage);
        Assert.AreEqual(7, getRes.Entity.Order);
        Assert.AreEqual(2.5, getRes.Entity.Weight);
        Assert.AreEqual(typeId, getRes.Entity.TraceNoteTypeId);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task GetAllByFromIdAndByToId_SplitRelationsAsAgreedInFase0(string orm)
    {
        using var db = new RepositoryTestDatabase();
        using var repo = db.CreateRepository(orm);

        var noteA = await AddNoteAsync(repo, "Note A");
        var noteB = await AddNoteAsync(repo, "Note B");
        var noteC = await AddNoteAsync(repo, "Note C");

        // A -> B (outgoing from A's point of view, incoming from B's), C -> A (incoming to A).
        var aToB = await repo.TraceNotes.AddAsync(new TraceNoteDto { TraceNoteId = Guid.NewGuid(), FromId = noteA, ToId = noteB, Order = 0, Weight = 1.0 });
        Assert.IsTrue(aToB.IsValid, aToB.ErrorMessage);
        var cToA = await repo.TraceNotes.AddAsync(new TraceNoteDto { TraceNoteId = Guid.NewGuid(), FromId = noteC, ToId = noteA, Order = 0, Weight = 1.0 });
        Assert.IsTrue(cToA.IsValid, cToA.ErrorMessage);

        var outgoingFromA = await repo.TraceNotes.GetAllByFromIdAsync(noteA); // "To" list in the ClientWin tab
        Assert.IsTrue(outgoingFromA.IsValid, outgoingFromA.ErrorMessage);
        Assert.HasCount(1, outgoingFromA.Entity);
        Assert.AreEqual(noteB, outgoingFromA.Entity[0].ToId);

        var incomingToA = await repo.TraceNotes.GetAllByToIdAsync(noteA); // "From" list in the ClientWin tab
        Assert.IsTrue(incomingToA.IsValid, incomingToA.ErrorMessage);
        Assert.HasCount(1, incomingToA.Entity);
        Assert.AreEqual(noteC, incomingToA.Entity[0].FromId);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task DeleteAsync_RemovesTheTraceNote(string orm)
    {
        using var db = new RepositoryTestDatabase();
        using var repo = db.CreateRepository(orm);

        var fromId = await AddNoteAsync(repo, "Parity From Note");
        var toId = await AddNoteAsync(repo, "Parity To Note");

        var addRes = await repo.TraceNotes.AddAsync(new TraceNoteDto { TraceNoteId = Guid.NewGuid(), FromId = fromId, ToId = toId, Order = 0, Weight = 1.0 });
        Assert.IsTrue(addRes.IsValid, addRes.ErrorMessage);

        var deleteRes = await repo.TraceNotes.DeleteAsync(addRes.Entity.TraceNoteId);
        Assert.IsTrue(deleteRes.IsValid, deleteRes.ErrorMessage);

        var getRes = await repo.TraceNotes.GetAsync(addRes.Entity.TraceNoteId);
        Assert.IsFalse(getRes.IsValid);
    }
}

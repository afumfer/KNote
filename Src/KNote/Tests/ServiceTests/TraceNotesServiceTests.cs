using KNote.Model.Dto;
using KNote.Repository;
using KNote.Service.Core;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// Fase 2 of the TraceNotes feature: NoteExtendedDto.TraceNotesFrom/TraceNotesTo, populated by
/// GetExtendedAsync and persisted by SaveExtendedAsync/DeleteExtendedAsync following the exact
/// same dirty-tracking loop pattern already used for Messages/Resources/Tasks (see
/// KntNoteCommands.cs). Exercises the Service layer directly (KntService over a real repository),
/// the same way ClientWin's NoteEditorCtrl does - not via HTTP, since NoteExtendedDto has no API
/// surface (see the earlier NoteExtendedDto investigation).
/// </summary>
[TestClass]
public class TraceNotesServiceTests
{
    private static async Task<Guid> AddNoteAsync(KntService service, string topic)
    {
        var folderRes = await service.Repository.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = $"TraceNote service folder {Guid.NewGuid():N}" });
        Assert.IsTrue(folderRes.IsValid, folderRes.ErrorMessage);

        // Tags is deliberately left null here (not set): it exercises the null-Tags fix in
        // KntNotesSaveExtendedAsyncCommand's message-broker check as a side effect of these tests.
        var noteId = Guid.NewGuid();
        var noteRes = await service.Repository.Notes.AddAsync(new NoteDto
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

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task GetExtendedAsync_PopulatesTraceNotesFromAndTo(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var repo = db.CreateRepository(orm);
        var service = new KntService(repo, activateMessageBroker: false);

        var noteA = await AddNoteAsync(service, "Note A");
        var noteB = await AddNoteAsync(service, "Note B");
        var noteC = await AddNoteAsync(service, "Note C");

        // B -> A (incoming to A) and A -> C (outgoing from A).
        var bToA = await service.Repository.TraceNotes.AddAsync(new TraceNoteDto { TraceNoteId = Guid.NewGuid(), FromId = noteB, ToId = noteA, Order = 0, Weight = 1.0 });
        Assert.IsTrue(bToA.IsValid, bToA.ErrorMessage);
        var aToC = await service.Repository.TraceNotes.AddAsync(new TraceNoteDto { TraceNoteId = Guid.NewGuid(), FromId = noteA, ToId = noteC, Order = 0, Weight = 1.0 });
        Assert.IsTrue(aToC.IsValid, aToC.ErrorMessage);

        var extendedRes = await service.Notes.GetExtendedAsync(noteA);
        Assert.IsTrue(extendedRes.IsValid, extendedRes.ErrorMessage);

        Assert.HasCount(1, extendedRes.Entity.TraceNotesFrom);
        Assert.AreEqual(noteB, extendedRes.Entity.TraceNotesFrom[0].FromId);

        Assert.HasCount(1, extendedRes.Entity.TraceNotesTo);
        Assert.AreEqual(noteC, extendedRes.Entity.TraceNotesTo[0].ToId);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task SaveExtendedAsync_PersistsANewOutgoingRelationAddedInMemory(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var repo = db.CreateRepository(orm);
        var service = new KntService(repo, activateMessageBroker: false);

        var noteA = await AddNoteAsync(service, "Note A");
        var noteB = await AddNoteAsync(service, "Note B");

        var extendedRes = await service.Notes.GetExtendedAsync(noteA);
        Assert.IsTrue(extendedRes.IsValid, extendedRes.ErrorMessage);
        var model = extendedRes.Entity;

        // Mirrors what the [+] button on the "Trace notes" tab will do (Fase 4): mutate the
        // in-memory Model, no direct repository call - persistence happens on the whole-note Save.
        model.TraceNotesTo.Add(new TraceNoteDto { FromId = noteA, ToId = noteB, Order = 0, Weight = 1.0 });

        var saveRes = await service.Notes.SaveExtendedAsync(model);
        Assert.IsTrue(saveRes.IsValid, saveRes.ErrorMessage);
        Assert.HasCount(1, saveRes.Entity.TraceNotesTo);
        Assert.AreNotEqual(Guid.Empty, saveRes.Entity.TraceNotesTo[0].TraceNoteId);

        var reloadRes = await service.Notes.GetExtendedAsync(noteA);
        Assert.IsTrue(reloadRes.IsValid, reloadRes.ErrorMessage);
        Assert.HasCount(1, reloadRes.Entity.TraceNotesTo);
        Assert.AreEqual(noteB, reloadRes.Entity.TraceNotesTo[0].ToId);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task SaveExtendedAsync_RejectsASelfLoopWithoutPersistingIt(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var repo = db.CreateRepository(orm);
        var service = new KntService(repo, activateMessageBroker: false);

        var noteA = await AddNoteAsync(service, "Note A");

        var extendedRes = await service.Notes.GetExtendedAsync(noteA);
        Assert.IsTrue(extendedRes.IsValid, extendedRes.ErrorMessage);
        var model = extendedRes.Entity;

        model.TraceNotesTo.Add(new TraceNoteDto { FromId = noteA, ToId = noteA, Order = 0, Weight = 1.0 }); // self-loop

        var saveRes = await service.Notes.SaveExtendedAsync(model);

        // The note itself still saves - only the invalid trace relation is rejected, reported as
        // an error on the aggregate result (KntCommandSaveServiceBase.ValidateParam() runs per
        // item inside the SaveTraceNoteAsync command, not on the whole NoteExtendedDto).
        Assert.IsFalse(saveRes.IsValid);
        Assert.IsTrue(saveRes.ErrorMessage.Contains("itself", StringComparison.OrdinalIgnoreCase), saveRes.ErrorMessage);

        var reloadRes = await service.Notes.GetExtendedAsync(noteA);
        Assert.IsTrue(reloadRes.IsValid, reloadRes.ErrorMessage);
        Assert.IsEmpty(reloadRes.Entity.TraceNotesTo);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task DeleteExtendedAsync_CascadesTraceNotesInBothDirections(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var repo = db.CreateRepository(orm);
        var service = new KntService(repo, activateMessageBroker: false);

        var noteA = await AddNoteAsync(service, "Note A");
        var noteB = await AddNoteAsync(service, "Note B");
        var noteC = await AddNoteAsync(service, "Note C");

        var bToA = await service.Repository.TraceNotes.AddAsync(new TraceNoteDto { TraceNoteId = Guid.NewGuid(), FromId = noteB, ToId = noteA, Order = 0, Weight = 1.0 });
        Assert.IsTrue(bToA.IsValid, bToA.ErrorMessage);
        var aToC = await service.Repository.TraceNotes.AddAsync(new TraceNoteDto { TraceNoteId = Guid.NewGuid(), FromId = noteA, ToId = noteC, Order = 0, Weight = 1.0 });
        Assert.IsTrue(aToC.IsValid, aToC.ErrorMessage);

        // Without the cascade, this would throw a FK violation (DeleteBehavior.Restrict) instead
        // of returning an invalid Result - either way this call must not leave the note undeleted
        // with orphaned trace rows.
        var deleteRes = await service.Notes.DeleteExtendedAsync(noteA);
        Assert.IsTrue(deleteRes.IsValid, deleteRes.ErrorMessage);

        var noteStillThere = await service.Notes.GetAsync(noteA);
        Assert.IsFalse(noteStillThere.IsValid);

        var orphanFrom = await service.Repository.TraceNotes.GetAsync(bToA.Entity.TraceNoteId);
        Assert.IsFalse(orphanFrom.IsValid);
        var orphanTo = await service.Repository.TraceNotes.GetAsync(aToC.Entity.TraceNoteId);
        Assert.IsFalse(orphanTo.IsValid);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task SaveExtendedAsync_RemovesATraceNoteMarkedDeleted(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var repo = db.CreateRepository(orm);
        var service = new KntService(repo, activateMessageBroker: false);

        var noteA = await AddNoteAsync(service, "Note A");
        var noteB = await AddNoteAsync(service, "Note B");

        var aToB = await service.Repository.TraceNotes.AddAsync(new TraceNoteDto { TraceNoteId = Guid.NewGuid(), FromId = noteA, ToId = noteB, Order = 0, Weight = 1.0 });
        Assert.IsTrue(aToB.IsValid, aToB.ErrorMessage);

        var extendedRes = await service.Notes.GetExtendedAsync(noteA);
        Assert.IsTrue(extendedRes.IsValid, extendedRes.ErrorMessage);
        var model = extendedRes.Entity;
        Assert.HasCount(1, model.TraceNotesTo);
        model.TraceNotesTo[0].SetIsDeleted(true);

        var saveRes = await service.Notes.SaveExtendedAsync(model);
        Assert.IsTrue(saveRes.IsValid, saveRes.ErrorMessage);
        Assert.IsEmpty(saveRes.Entity.TraceNotesTo);

        var afterDelete = await service.Repository.TraceNotes.GetAsync(aToB.Entity.TraceNoteId);
        Assert.IsFalse(afterDelete.IsValid);
    }
}

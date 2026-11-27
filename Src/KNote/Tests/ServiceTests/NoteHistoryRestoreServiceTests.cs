using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Service.ServicesCommands;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// «Compare» and «Restore» of the history of a note (H.4a, H22, H23): what a change did or that version against the note now; and
/// restoring the note as it was after a change, in a row Restored of its own with its copy, so that restoring the row before it
/// undoes it. What no longer exists (a folder, a type, an attribute) leaves what the note has, and says so. Dapper and EF.
/// </summary>
[TestClass]
public class NoteHistoryRestoreServiceTests
{
    private RepositoryTestDatabase db = null!;
    private KntService service = null!;
    private Guid folderId;

    private async Task OpenAsync(string orm)
    {
        db = new RepositoryTestDatabase();
        service = new KntService(db.CreateRepository(orm), activateMessageBroker: false)
        {
            UserIdentityName = "armando",
            HistoryOrigin = EnumHistoryOrigin.Desktop
        };
        var folder = await service.Repository.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "Equipment" });
        folderId = folder.Entity.FolderId;
    }

    [TestCleanup]
    public void TestCleanup()
    {
        service?.Dispose();
        db?.Dispose();
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task Compare_ShowsWhatTheChangeDid_OrThatVersionAgainstTheNoteNow(string orm)
    {
        await OpenAsync(orm);
        var noteId = await StoredNoteAsync("CPC", "Line 1\nThe keyboard works\nLine 3");
        await EditAsync(noteId, "u1", n => n.Description = "Line 1\nThe keyboard works fine\nLine 3");
        await EditAsync(noteId, "u2", n => n.Topic = "Amstrad CPC");
        var change = await RowAsync(noteId, "u1");

        var did = (await service.History.GetNoteComparisonAsync(noteId, change.KLogId)).Entity;
        var againstNow = (await service.History.GetNoteComparisonAsync(noteId, change.KLogId, withCurrentNote: true)).Entity;

        Assert.IsFalse(did.WithCurrentNote);
        Assert.AreEqual(0, did.Fields.Count, "u1 only changed the text");
        Assert.AreEqual(1, did.Text.Added);
        var added = did.Text.Lines.Single(l => l.Kind == EnumComparisonLine.Added);
        CollectionAssert.AreEqual(new[] { "fine" }, added.Segments.Where(s => s.Changed).Select(s => s.Text.Trim()).ToArray());
        Assert.AreEqual("Before the change", did.BeforeLabel);

        Assert.IsTrue(againstNow.WithCurrentNote);
        Assert.IsTrue(againstNow.Text.Same, "the text is the same now");
        Assert.AreEqual("Amstrad CPC", againstNow.Fields.Single(f => f.Name == "Topic").After);
        Assert.AreEqual("The note now", againstNow.AfterLabel);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task WithoutTheCopyBeforeTheChange_ItComparesWithTheNoteNow(string orm)
    {
        await OpenAsync(orm);
        var noteId = await StoredNoteAsync("A", "Text");
        await EditAsync(noteId, "u1", n => n.Topic = "B");
        var change = await RowAsync(noteId, "u1");
        await service.Repository.History.ClearPreviousContentAsync(new List<Guid> { change.KLogId });

        var comparison = (await service.History.GetNoteComparisonAsync(noteId, change.KLogId)).Entity;

        Assert.IsTrue(comparison.WithCurrentNote);
        Assert.IsFalse(comparison.CanCompareChange);
        StringAssert.Contains(comparison.Notice, KntHistoryGetNoteComparisonAsyncCommand.BeforeNotKept);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task Restore_BringsTheVersionBack_AndRestoringTheRowBeforeUndoesIt(string orm)
    {
        await OpenAsync(orm);
        var noteId = await StoredNoteAsync("A", "First text");
        await EditAsync(noteId, "u1", n => { n.Topic = "B"; n.Tags = "cpc"; });
        await EditAsync(noteId, "u2", n => { n.Topic = "C"; n.Description = "Second text"; n.Priority = 3; });
        var afterB = await RowAsync(noteId, "u1");

        service.UserIdentityName = "u3";
        var restored = await service.History.RestoreNoteVersionAsync(noteId, afterB.KLogId);

        Assert.IsTrue(restored.IsValid, restored.ErrorMessage);
        Assert.IsTrue(restored.Entity.Changed);
        var note = (await service.Notes.GetAsync(noteId)).Entity;
        Assert.AreEqual("B", note.Topic);
        Assert.AreEqual("First text", note.Description);
        Assert.AreEqual("cpc", note.Tags);
        Assert.AreEqual(0, note.Priority);

        var rows = (await service.History.GetNoteHistoryAsync(noteId)).Entity.Rows;
        var restoreRow = rows.First();
        Assert.AreEqual(nameof(EnumHistoryAction.Restored), restoreRow.ActionType);
        Assert.AreEqual("u3", restoreRow.UserName);
        StringAssert.StartsWith(restoreRow.RegistryMessage, "Restored the version of ");
        Assert.IsTrue(restoreRow.HasPreviousContent, "its copy: the note before restoring");

        // Undo: the row before the restore (u2's), as it was after it.
        var undo = await service.History.RestoreNoteVersionAsync(noteId, rows.Single(r => r.UserName == "u2").KLogId);
        Assert.IsTrue(undo.Entity.Changed, undo.ErrorMessage);
        note = (await service.Notes.GetAsync(noteId)).Entity;
        Assert.AreEqual("C", note.Topic);
        Assert.AreEqual("Second text", note.Description);
        Assert.AreEqual(3, note.Priority);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ARestore_IsARowOfItsOwn_AndTheNextEditToo(string orm)
    {
        await OpenAsync(orm);
        var noteId = await StoredNoteAsync("A", "Text");
        await EditAsync(noteId, "armando", n => n.Topic = "B");
        await EditAsync(noteId, "marta", n => n.Topic = "C");
        var afterB = await RowAsync(noteId, "armando");

        service.UserIdentityName = "armando";
        await service.History.RestoreNoteVersionAsync(noteId, afterB.KLogId);
        await EditAsync(noteId, "armando", n => n.Topic = "D");

        var rows = (await service.History.GetNoteHistoryAsync(noteId)).Entity.Rows;
        CollectionAssert.AreEqual(new[] { "Updated", "Restored", "Updated", "Updated", "Created" }, rows.Select(r => r.ActionType).ToArray(),
            "within ten minutes, by the same user: still not into the row of the restore");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task TheLastChange_HasNothingToRestore(string orm)
    {
        await OpenAsync(orm);
        var noteId = await StoredNoteAsync("A", "Text");
        await EditAsync(noteId, "u1", n => n.Topic = "B");
        var last = await RowAsync(noteId, "u1");

        var page = (await service.History.GetNoteHistoryAsync(noteId)).Entity;
        var result = await service.History.RestoreNoteVersionAsync(noteId, last.KLogId);

        Assert.IsFalse(page.CanRestore(last.KLogId));
        Assert.IsTrue(page.CanRestore(page.Rows.Single(r => r.ActionType == "Created").KLogId));
        Assert.IsFalse(result.IsValid);
        StringAssert.Contains(result.ErrorMessage, KntHistoryRestoreNoteVersionAsyncCommand.NothingToRestore);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task AFolderThatNoLongerExists_LeavesTheNoteWhereItIs(string orm)
    {
        await OpenAsync(orm);
        var old = await service.Repository.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "Old" });
        var noteId = await StoredNoteAsync("A", "Text", folder: old.Entity.FolderId);
        await EditAsync(noteId, "u1", n => n.Topic = "B");
        await EditAsync(noteId, "u2", n => { n.Topic = "C"; n.FolderId = folderId; });
        Assert.IsTrue((await service.Repository.Folders.DeleteAsync(old.Entity.FolderId)).IsValid);

        var result = await service.History.RestoreNoteVersionAsync(noteId, (await RowAsync(noteId, "u1")).KLogId);

        Assert.IsTrue(result.Entity.Changed, result.ErrorMessage);
        var note = (await service.Notes.GetAsync(noteId)).Entity;
        Assert.AreEqual("B", note.Topic);
        Assert.AreEqual(folderId, note.FolderId);
        StringAssert.Contains(string.Join(" ", result.Entity.Notices), "The folder of that version no longer exists: the note stays in Equipment.");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task TheTypeAndTheAttributes_AreRestored(string orm)
    {
        await OpenAsync(orm);
        var computer = await service.Repository.NoteTypes.AddAsync(new NoteTypeDto { NoteTypeId = Guid.NewGuid(), Name = "Computer" });
        var status = await service.Repository.KAttributes.AddAsync(new KAttributeDto
        {
            KAttributeId = Guid.NewGuid(), Name = "Status", NoteTypeId = computer.Entity.NoteTypeId, KAttributeDataType = EnumKAttributeDataType.String
        });
        var noteId = await StoredNoteAsync("CPC", "Text", type: computer.Entity.NoteTypeId,
            attributes: new NoteKAttributeDto { NoteKAttributeId = Guid.NewGuid(), KAttributeId = status.Entity.KAttributeId, Name = "Status", Value = "Works" });
        await EditAsync(noteId, "u1", n => n.Topic = "CPC 464");
        await EditAsync(noteId, "u2", n =>
        {
            n.NoteTypeId = null;
            n.KAttributesDto.RemoveAll(a => a.KAttributeNoteTypeId != null);
        });
        Assert.IsNull((await service.Notes.GetAsync(noteId)).Entity.NoteTypeId);

        var result = await service.History.RestoreNoteVersionAsync(noteId, (await RowAsync(noteId, "u1")).KLogId);

        Assert.IsTrue(result.Entity.Changed, result.ErrorMessage);
        var note = (await service.Notes.GetAsync(noteId)).Entity;
        Assert.AreEqual(computer.Entity.NoteTypeId, note.NoteTypeId);
        Assert.AreEqual("Works", note.KAttributesDto.Single(a => a.KAttributeId == status.Entity.KAttributeId).Value);
        Assert.AreEqual(0, result.Entity.Notices.Count);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ANoteAlreadyAsThatVersion_IsNotSaved(string orm)
    {
        await OpenAsync(orm);
        var noteId = await StoredNoteAsync("A", "Text");
        await EditAsync(noteId, "u1", n => n.Topic = "B");
        await EditAsync(noteId, "u2", n => n.Topic = "C");
        await EditAsync(noteId, "u3", n => n.Topic = "B");

        var result = await service.History.RestoreNoteVersionAsync(noteId, (await RowAsync(noteId, "u1")).KLogId);

        Assert.IsFalse(result.Entity.Changed);
        Assert.AreEqual(KntHistoryRestoreNoteVersionAsyncCommand.AlreadySo, result.Entity.Notices.First());
        Assert.AreEqual(4, (await service.History.GetNoteHistoryAsync(noteId)).Entity.Total, "nothing noted");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task AGuest_CantRestore(string orm)
    {
        await OpenAsync(orm);
        var noteId = await StoredNoteAsync("A", "Text");
        await EditAsync(noteId, "u1", n => n.Topic = "B");
        var created = (await service.History.GetNoteHistoryAsync(noteId)).Entity.Rows.Single(r => r.ActionType == "Created");
        Assert.IsTrue((await service.Repository.Users.AddAsync(new UserDto
        {
            UserId = Guid.NewGuid(), UserName = "guest", EMail = "guest@knote.tests", FullName = "Guest", RoleDefinition = "Guest"
        })).IsValid);
        service.UserIdentityName = "guest";
        service.EnforceAuthorization = true;

        var result = await service.History.RestoreNoteVersionAsync(noteId, created.KLogId);

        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.NotAuthorized);
        Assert.AreEqual("B", (await service.Repository.Notes.GetAsync(noteId)).Entity.Topic);
    }

    private async Task<KLogDto> RowAsync(Guid noteId, string user) =>
        (await service.History.GetNoteHistoryAsync(noteId)).Entity.Rows.Single(r => r.UserName == user);

    private async Task<Guid> StoredNoteAsync(string topic, string description, Guid? folder = null, Guid? type = null, params NoteKAttributeDto[] attributes)
    {
        var folderDto = (await service.Repository.Folders.GetAsync(folder ?? folderId)).Entity;
        var note = new NoteDto
        {
            NoteId = Guid.NewGuid(), Topic = topic, Description = description, Tags = "", FolderId = folderDto.FolderId, FolderDto = folderDto,
            NoteTypeId = type, ContentType = "markdown", CreationDateTime = new DateTime(2026, 1, 1), ModificationDateTime = new DateTime(2026, 1, 1)
        };
        foreach (var attribute in attributes)
        {
            attribute.NoteId = note.NoteId;
            note.KAttributesDto.Add(attribute);
        }
        service.UserIdentityName = "creator";
        var saved = await service.Notes.SaveAsync(note, false);
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        return saved.Entity.NoteId;
    }

    private async Task EditAsync(Guid noteId, string user, Action<NoteDto> change)
    {
        service.UserIdentityName = user;
        var note = (await service.Notes.GetAsync(noteId)).Entity;
        change(note);
        var saved = await service.Notes.SaveAsync(note, false);
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
    }
}

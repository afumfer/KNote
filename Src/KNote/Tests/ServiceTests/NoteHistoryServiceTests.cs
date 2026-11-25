using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;
using Microsoft.Data.Sqlite;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// The history of the notes, H.1 (03-Historial.md): saving, moving, retagging and deleting a note leave their row, written with
/// the change (if it can't be written, the note doesn't change); the autosave of a session is one row with the version the note
/// had before it; another user or origin is another row; nothing is noted of the status alone nor of the notes KNote writes; a
/// deleted note keeps its last version in the trash for DeletedNotesDays; and a note keeps at most MaxCopiesPerNote versions.
/// Dapper and EF.
/// </summary>
[TestClass]
public class NoteHistoryServiceTests
{
    private RepositoryTestDatabase db = null!;
    private KntService service = null!;
    private Guid folderId;
    private Guid otherFolderId;

    private async Task OpenAsync(string orm)
    {
        db = new RepositoryTestDatabase();
        service = new KntService(db.CreateRepository(orm), activateMessageBroker: false)
        {
            UserIdentityName = "armando",
            HistoryOrigin = EnumHistoryOrigin.Desktop
        };
        var home = await service.Repository.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "Equipment" });
        folderId = home.Entity.FolderId;
        var other = await service.Repository.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "Restored", ParentId = folderId });
        otherFolderId = other.Entity.FolderId;
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
    public async Task ANewNote_EditedInItsSession_IsOneCreatedRow(string orm)
    {
        await OpenAsync(orm);
        var note = await NewNoteAsync("Amstrad CPC 464", "# Status\r\nRestoring.\r\n");
        await EditAsync(note.NoteId, n => n.Description += "\r\n# Restoration\r\nKeyboard cleaned.\r\n");
        await EditAsync(note.NoteId, n => n.Topic = "Amstrad CPC 464 (green)");

        var row = (await RowsAsync(note.NoteId)).Single();
        Assert.AreEqual("Created", row.ActionType);
        Assert.AreEqual("armando", row.UserName);
        Assert.AreEqual("Desktop", row.Origin);
        Assert.IsNull(row.PreviousContentSize, "a new note has no version before it");
        Assert.AreEqual(@"New note in Equipment · Sections: «Status», «Restoration»", row.RegistryMessage);
        StringAssert.EndsWith(row.EntityTitle, "Amstrad CPC 464 (green)");
        StringAssert.StartsWith(row.EntityTitle, $"#{note.NoteNumber} ");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task TheAutosaveOfASession_IsOneRow_WithTheVersionBeforeIt(string orm)
    {
        await OpenAsync(orm);
        var noteId = await StoredNoteAsync("A", "Line one.");

        await EditAsync(noteId, n => n.Topic = "B");
        await EditAsync(noteId, n => n.Description = "Line one.\r\nLine two.");
        await EditAsync(noteId, n => n.Topic = "C");
        // A save that changes nothing of its own data (the editor's autosave of an unchanged note, its status).
        await EditAsync(noteId, n => n.InternalTags = "Alarms pending");

        var row = (await RowsAsync(noteId)).Single();
        Assert.AreEqual("Updated", row.ActionType);
        Assert.AreEqual(EnumHistoryFields.Topic | EnumHistoryFields.Description, row.Fields);
        Assert.AreEqual("Topic: «A» → «C» · Text changed (+1 line)", row.RegistryMessage, "described from how it was before the session");
        var version = await VersionAsync(row);
        Assert.AreEqual("A", version.Topic);
        Assert.AreEqual("Line one.", version.Description);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task AnotherUserOrOrigin_IsAnotherRow_WithItsOwnVersion(string orm)
    {
        await OpenAsync(orm);
        var noteId = await StoredNoteAsync("A", "Text");

        await EditAsync(noteId, n => n.Topic = "B");
        service.UserIdentityName = "marta";
        await EditAsync(noteId, n => n.Topic = "C");
        service.UserIdentityName = "armando";
        using (KntHistoryOrigin.Begin(EnumHistoryOrigin.Assistant))
            await EditAsync(noteId, n => n.Topic = "D");
        await EditAsync(noteId, n => n.Topic = "E");

        var rows = (await RowsAsync(noteId)).OrderBy(r => r.RegistryDateTime).ThenBy(r => r.LastDateTime).ToList();
        CollectionAssert.AreEquivalent(new[] { "armando/Desktop", "marta/Desktop", "armando/Assistant", "armando/Desktop" },
            rows.Select(r => $"{r.UserName}/{r.Origin}").ToArray());
        var versions = new List<string>();
        foreach (var row in rows)
            versions.Add((await VersionAsync(row)).Topic);
        CollectionAssert.AreEquivalent(new[] { "A", "B", "C", "D" }, versions);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task MovingOrRetaggingANote_IsNoted(string orm)
    {
        await OpenAsync(orm);
        var moved = await StoredNoteAsync("Moved", "Text");
        var tagged = await StoredNoteAsync("Tagged", "Text");

        Assert.IsTrue((await service.Notes.UtilPatchFolderAsync(moved, otherFolderId)).Entity);
        Assert.IsTrue((await service.Notes.UtilPatchChangeTagsAsync(tagged, null, "restored")).Entity);

        var move = (await RowsAsync(moved)).Single();
        Assert.AreEqual(EnumHistoryFields.Folder, move.Fields);
        Assert.AreEqual(@"Moved from Equipment to Equipment\Restored", move.RegistryMessage);
        Assert.AreEqual(folderId, (await VersionAsync(move)).FolderId);
        var tag = (await RowsAsync(tagged)).Single();
        Assert.AreEqual("Tags: +restored", tag.RegistryMessage);
        Assert.AreEqual("old", (await VersionAsync(tag)).Tags);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ADeletedNote_KeepsItsLastVersionInTheTrash(string orm)
    {
        await OpenAsync(orm);
        var noteId = await StoredNoteAsync("To delete", "Its text");
        await EditAsync(noteId, n => n.Description = "Its last text");

        Assert.IsTrue((await service.Notes.DeleteExtendedAsync(noteId)).IsValid);

        var deleted = (await RowsAsync(noteId)).Single(r => r.ActionType == "Deleted");
        Assert.AreEqual("Deleted", deleted.ActionType);
        Assert.AreEqual("Note deleted from Equipment", deleted.RegistryMessage);
        Assert.AreEqual("Its last text", (await VersionAsync(deleted)).Description);
        Assert.AreEqual(0, await KntHistory.PurgeDeletedNotesAsync(service), "30 days in the trash");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task AfterDeletedNotesDays_TheVersionsOfADeletedNoteGo_AndItsRowsStay(string orm)
    {
        await OpenAsync(orm);
        await SetRetentionAsync(new HistoryRetentionDto { DeletedNotesDays = 0 });
        var noteId = await StoredNoteAsync("To delete", "Its text");
        await EditAsync(noteId, n => n.Description = "Its last text");

        Assert.IsTrue((await service.Notes.DeleteAsync(noteId)).IsValid);
        await Task.Delay(50); // the trash empties what was deleted before now
        await KntHistory.PurgeDeletedNotesAsync(service);

        var rows = await RowsAsync(noteId);
        Assert.HasCount(2, rows);
        Assert.IsTrue(rows.All(r => r.PreviousContentSize == null));
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ANoteKeepsAtMostMaxCopiesPerNote(string orm)
    {
        await OpenAsync(orm);
        await SetRetentionAsync(new HistoryRetentionDto { MaxCopiesPerNote = 2 });
        var noteId = await StoredNoteAsync("V0", "Text");
        foreach (var user in new[] { "u1", "u2", "u3" })
        {
            service.UserIdentityName = user;
            await EditAsync(noteId, n => n.Topic = "After " + user);
        }

        var rows = await RowsAsync(noteId);
        Assert.HasCount(3, rows);
        Assert.AreEqual(2, rows.Count(r => r.PreviousContentSize != null));
        Assert.IsNull(rows.Single(r => r.UserName == "u1").PreviousContentSize, "the oldest goes");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task TheNotesKNoteWrites_AreNotNoted(string orm)
    {
        await OpenAsync(orm);
        var chatRoom = await service.Repository.NoteTypes.AddAsync(new NoteTypeDto { NoteTypeId = Guid.NewGuid(), Name = KntConst.ChatRoomTag });
        var noteId = await StoredNoteAsync("A thread", "Messages", chatRoom.Entity.NoteTypeId);

        await EditAsync(noteId, n => n.Description = "More messages");
        var created = await NewNoteAsync("Another thread", "Messages", chatRoom.Entity.NoteTypeId);
        Assert.IsTrue((await service.Notes.DeleteAsync(noteId)).IsValid);

        Assert.IsEmpty(await RowsAsync(noteId));
        Assert.IsEmpty(await RowsAsync(created.NoteId));
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task IfTheRowCantBeWritten_TheSaveFails_AndTheNoteDoesntChange(string orm)
    {
        await OpenAsync(orm);
        var noteId = await StoredNoteAsync("Kept", "Kept text");
        // The row can be read but not written.
        SqliteConnection.ClearAllPools();
        using (var connection = new SqliteConnection($"Data Source={db.DatabaseFilePath}"))
        {
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "ALTER TABLE \"KLogs\" DROP COLUMN \"PreviousContent\";";
            cmd.ExecuteNonQuery();
        }

        var note = (await service.Notes.GetAsync(noteId)).Entity;
        note.Topic = "Changed";
        await AssertFailsAsync(() => service.Notes.SaveAsync(note));
        await AssertFailsAsync(() => service.Notes.UtilPatchFolderAsync(noteId, otherFolderId));
        await AssertFailsAsync(() => service.Notes.DeleteAsync(noteId));

        var stored = (await service.Repository.Notes.GetAsync(noteId)).Entity;
        Assert.IsNotNull(stored);
        Assert.AreEqual("Kept", stored.Topic);
        Assert.AreEqual(folderId, stored.FolderId);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task WithoutHistoryOrigin_ItIsKNote(string orm)
    {
        await OpenAsync(orm);
        service.HistoryOrigin = EnumHistoryOrigin.KNote;
        var noteId = await StoredNoteAsync("A", "Text");

        await EditAsync(noteId, n => n.Topic = "B");

        Assert.AreEqual("KNote", (await RowsAsync(noteId)).Single().Origin);
    }

    [TestMethod]
    public async Task TheRetention_IsNotChangedByTheGenericCommands()
    {
        await OpenAsync("Dapper");

        var save = await service.SystemValues.SaveAsync(new SystemValueDto
        {
            SystemValueId = Guid.Empty, Scope = KntConst.SystemScope, Key = KntConst.HistoryRetentionKey, Value = "{\"keepAllMonths\":0}"
        });

        Assert.IsFalse(save.IsValid);
    }

    // A note already there before the history (no row of its own).
    private async Task<Guid> StoredNoteAsync(string topic, string text, Guid? noteTypeId = null)
    {
        var note = new NoteDto
        {
            NoteId = Guid.NewGuid(), Topic = topic, Description = text, Tags = "old", FolderId = folderId, NoteTypeId = noteTypeId,
            ContentType = "markdown", CreationDateTime = new DateTime(2026, 1, 1), ModificationDateTime = new DateTime(2026, 1, 1)
        };
        Assert.IsTrue((await service.Repository.Notes.AddAsync(note)).IsValid);
        return note.NoteId;
    }

    private async Task<NoteDto> NewNoteAsync(string topic, string text, Guid? noteTypeId = null)
    {
        var note = (await service.Notes.NewAsync()).Entity;
        note.Topic = topic;
        note.Description = text;
        note.Tags = "";
        note.FolderId = folderId;
        note.NoteTypeId = noteTypeId;
        var saved = await service.Notes.SaveAsync(note);
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        return (await service.Repository.Notes.GetAsync(saved.Entity.NoteId)).Entity;
    }

    private async Task EditAsync(Guid noteId, Action<NoteDto> change)
    {
        var note = (await service.Notes.GetAsync(noteId)).Entity;
        change(note);
        var saved = await service.Notes.SaveAsync(note, false);
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
    }

    private async Task<List<KLogDto>> RowsAsync(Guid noteId) =>
        (await service.Repository.History.GetEntityHistoryAsync(HistoryEntities.Note, noteId)).Entity;

    private async Task<NoteVersionDto> VersionAsync(KLogDto row) =>
        NoteVersionDto.FromJson((await service.Repository.History.GetPreviousContentAsync(row.KLogId)).Entity);

    private async Task SetRetentionAsync(HistoryRetentionDto retention)
    {
        var added = await service.Repository.SystemValues.AddAsync(new SystemValueDto
        {
            SystemValueId = Guid.NewGuid(), Scope = KntConst.SystemScope, Key = KntConst.HistoryRetentionKey, Value = retention.ToJson()
        });
        Assert.IsTrue(added.IsValid, added.ErrorMessage);
    }

    private static async Task AssertFailsAsync<T>(Func<Task<T>> action) where T : ResultBase
    {
        try
        {
            var result = await action();
            Assert.IsFalse(result.IsValid, "it should have failed");
        }
        catch (Exception)
        {
            // Failed: what matters is that the note didn't change.
        }
    }
}

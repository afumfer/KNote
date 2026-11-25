using KNote.Model;
using KNote.Model.Dto;
using KNote.Repository;
using KNote.Tests.Helpers;

namespace KNote.Tests.RepositoryParityTests;

/// <summary>
/// The history of the notes (KLogs, schema revision 11), the same with both ORMs: the rows written with a change of a note (in its
/// transaction: if the row can't be written, the note doesn't change), merged into the row of a session, read without their
/// versions, the version of a row, and the retention and the trash emptying versions without deleting rows.
/// </summary>
[TestClass]
public class HistoryParityTests
{
    private static readonly DateTime Start = new(2026, 11, 25, 10, 0, 0);

    [TestMethod]
    [DataRow("Dapper", "EntityFramework")]
    [DataRow("EntityFramework", "Dapper")]
    public async Task TheRowsOfANote_AreWrittenWithIt_AndReadWithoutTheirVersions(string writerOrm, string readerOrm)
    {
        using var db = new RepositoryTestDatabase();
        using var writer = db.CreateRepository(writerOrm);
        using var reader = db.CreateRepository(readerOrm);
        var folderId = await AddFolderAsync(writer);

        var note = NewNote(folderId, "First topic");
        var created = Row(note.NoteId, EnumHistoryAction.Created, Start);
        Assert.IsTrue((await writer.Notes.AddAsync(note, new KLogWriteDto { Row = created, TitleFromNewNote = true })).IsValid);

        note.Topic = "Second topic";
        var updated = Row(note.NoteId, EnumHistoryAction.Updated, Start.AddHours(1));
        Assert.IsTrue((await writer.Notes.UpdateAsync(note, history: new KLogWriteDto { Row = updated, PreviousContent = "{\"topic\":\"First topic\"}" })).IsValid);

        var rows = (await reader.History.GetEntityHistoryAsync(HistoryEntities.Note, note.NoteId)).Entity;
        Assert.HasCount(2, rows);
        Assert.AreEqual(updated.KLogId, rows[0].KLogId, "the newest first");
        Assert.AreEqual($"#{note.NoteNumber} First topic", rows[1].EntityTitle, "a new note, titled with its number");
        Assert.IsNull(rows[1].PreviousContentSize);
        Assert.AreEqual(Encoding("{\"topic\":\"First topic\"}"), rows[0].PreviousContentSize);
        Assert.AreEqual("armando", rows[0].UserName);
        Assert.AreEqual("Desktop", rows[0].Origin);
        Assert.AreEqual(EnumHistoryFields.Topic, rows[0].Fields);
        Assert.AreEqual("{\"topic\":{}}", rows[0].ChangeDetails);
        Assert.AreEqual(Start.AddHours(1), rows[0].LastDateTime);
        Assert.AreEqual(Start.AddHours(1), rows[0].RegistryDateTime);

        Assert.AreEqual(updated.KLogId, (await reader.History.GetLastAsync(HistoryEntities.Note, note.NoteId)).Entity.KLogId);
        Assert.AreEqual("{\"topic\":\"First topic\"}", (await reader.History.GetPreviousContentAsync(updated.KLogId)).Entity);
        Assert.IsNull((await reader.History.GetPreviousContentAsync(created.KLogId)).Entity);
    }

    [TestMethod]
    [DataRow("Dapper", "EntityFramework")]
    [DataRow("EntityFramework", "Dapper")]
    public async Task AMerge_UpdatesTheRowOfTheSession_AndSetsItsVersionOnlyIfItHasNone(string writerOrm, string readerOrm)
    {
        using var db = new RepositoryTestDatabase();
        using var writer = db.CreateRepository(writerOrm);
        using var reader = db.CreateRepository(readerOrm);
        var note = NewNote(await AddFolderAsync(writer), "Topic");
        Assert.IsTrue((await writer.Notes.AddAsync(note)).IsValid);

        var session = Row(note.NoteId, EnumHistoryAction.Updated, Start);
        Assert.IsTrue((await writer.Notes.UpdateAsync(note, history: new KLogWriteDto { Row = session })).IsValid);

        // Into the session: it had no version, so it gets this one.
        Assert.IsTrue((await writer.Notes.UpdateAsync(note, history: Merge(session, Start.AddMinutes(2), Start, "v1"))).IsValid);
        // Again: it keeps the first one.
        Assert.IsTrue((await writer.Notes.UpdateAsync(note, history: Merge(session, Start.AddMinutes(4), Start.AddMinutes(2), "v2"))).IsValid);

        var rows = (await reader.History.GetEntityHistoryAsync(HistoryEntities.Note, note.NoteId)).Entity;
        Assert.HasCount(1, rows);
        Assert.AreEqual(Start.AddMinutes(4), rows[0].LastDateTime);
        Assert.AreEqual(Start, rows[0].RegistryDateTime);
        Assert.AreEqual("Merged at 4", rows[0].RegistryMessage);
        Assert.AreEqual("v1", (await reader.History.GetPreviousContentAsync(session.KLogId)).Entity);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task AMergeIntoARowThatChanged_IsWrittenAlone(string orm)
    {
        using var db = new RepositoryTestDatabase();
        using var repository = db.CreateRepository(orm);
        var note = NewNote(await AddFolderAsync(repository), "Topic");
        Assert.IsTrue((await repository.Notes.AddAsync(note)).IsValid);
        var session = Row(note.NoteId, EnumHistoryAction.Updated, Start);
        Assert.IsTrue((await repository.Notes.UpdateAsync(note, history: new KLogWriteDto { Row = session })).IsValid);

        // Read when it was at 10:00:30: another write came first.
        var write = Merge(session, Start.AddMinutes(2), Start.AddSeconds(30), "v1");
        var alone = Row(note.NoteId, EnumHistoryAction.Updated, Start.AddMinutes(2));
        write.Standalone = alone;
        write.StandalonePreviousContent = "alone";
        Assert.IsTrue((await repository.Notes.UpdateAsync(note, history: write)).IsValid);

        var rows = (await repository.History.GetEntityHistoryAsync(HistoryEntities.Note, note.NoteId)).Entity;
        Assert.HasCount(2, rows);
        Assert.AreEqual(Start, rows.Single(r => r.KLogId == session.KLogId).LastDateTime, "untouched");
        Assert.AreEqual("alone", (await repository.History.GetPreviousContentAsync(alone.KLogId)).Entity);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task IfTheRowCantBeWritten_TheNoteDoesntChange(string orm)
    {
        using var db = new RepositoryTestDatabase();
        using var repository = db.CreateRepository(orm);
        var note = NewNote(await AddFolderAsync(repository), "Kept");
        note.KAttributesDto = new();
        Assert.IsTrue((await repository.Notes.AddAsync(note)).IsValid);
        var first = Row(note.NoteId, EnumHistoryAction.Updated, Start);
        Assert.IsTrue((await repository.Notes.UpdateAsync(note, history: new KLogWriteDto { Row = first })).IsValid);

        // The same KLogId again: the insert of the row fails (primary key) after the note was written.
        var edited = (await repository.Notes.GetAsync(note.NoteId)).Entity;
        edited.Topic = "Changed";
        var duplicate = Row(note.NoteId, EnumHistoryAction.Updated, Start.AddHours(1));
        duplicate.KLogId = first.KLogId;
        await AssertThrowsAsync(() => repository.Notes.UpdateAsync(edited, history: new KLogWriteDto { Row = duplicate }));
        await AssertThrowsAsync(() => repository.Notes.PatchFolderAsync(note.NoteId, note.FolderId, new KLogWriteDto { Row = duplicate }));
        await AssertThrowsAsync(() => repository.Notes.PatchChangeTagsAsync(note.NoteId, null, "new", new KLogWriteDto { Row = duplicate }));
        await AssertThrowsAsync(() => repository.Notes.DeleteAsync(note.NoteId, new KLogWriteDto { Row = duplicate }));

        var stored = (await repository.Notes.GetAsync(note.NoteId)).Entity;
        Assert.IsNotNull(stored, "not deleted");
        Assert.AreEqual("Kept", stored.Topic);
        Assert.AreNotEqual("new", stored.Tags);

        // Nor a new one: its number and its row go away together.
        var other = NewNote(note.FolderId, "Not added");
        await AssertThrowsAsync(() => repository.Notes.AddAsync(other, new KLogWriteDto { Row = duplicate }));
        Assert.IsNull((await repository.Notes.GetAsync(other.NoteId)).Entity);
    }

    [TestMethod]
    [DataRow("Dapper", "EntityFramework")]
    [DataRow("EntityFramework", "Dapper")]
    public async Task TheRetentionAndTheTrash_EmptyVersions_AndKeepTheRows(string writerOrm, string readerOrm)
    {
        using var db = new RepositoryTestDatabase();
        using var writer = db.CreateRepository(writerOrm);
        using var reader = db.CreateRepository(readerOrm);
        var folderId = await AddFolderAsync(writer);

        var kept = NewNote(folderId, "Kept");
        Assert.IsTrue((await writer.Notes.AddAsync(kept)).IsValid);
        var keptRows = new[] { Row(kept.NoteId, EnumHistoryAction.Updated, Start), Row(kept.NoteId, EnumHistoryAction.Updated, Start.AddHours(1)) };
        foreach (var row in keptRows)
            Assert.IsTrue((await writer.Notes.UpdateAsync(kept, history: new KLogWriteDto { Row = row, PreviousContent = "v" })).IsValid);

        var deletedLongAgo = NewNote(folderId, "Deleted long ago");
        Assert.IsTrue((await writer.Notes.AddAsync(deletedLongAgo)).IsValid);
        Assert.IsTrue((await writer.Notes.UpdateAsync(deletedLongAgo, history: new KLogWriteDto { Row = Row(deletedLongAgo.NoteId, EnumHistoryAction.Updated, Start.AddDays(-40)), PreviousContent = "v" })).IsValid);
        Assert.IsTrue((await writer.Notes.DeleteAsync(deletedLongAgo.NoteId, new KLogWriteDto { Row = Row(deletedLongAgo.NoteId, EnumHistoryAction.Deleted, Start.AddDays(-35)), PreviousContent = "last" })).IsValid);

        var deletedToday = NewNote(folderId, "Deleted today");
        Assert.IsTrue((await writer.Notes.AddAsync(deletedToday)).IsValid);
        Assert.IsTrue((await writer.Notes.DeleteAsync(deletedToday.NoteId, new KLogWriteDto { Row = Row(deletedToday.NoteId, EnumHistoryAction.Deleted, Start), PreviousContent = "last" })).IsValid);

        CollectionAssert.AreEqual(new[] { deletedLongAgo.NoteId },
            (await reader.History.GetDeletedWithContentAsync(HistoryEntities.Note, Start.AddDays(-30))).Entity);
        Assert.IsTrue((await reader.History.ClearEntityContentAsync(HistoryEntities.Note, deletedLongAgo.NoteId)).IsValid);
        Assert.IsTrue((await reader.History.ClearPreviousContentAsync(new List<Guid> { keptRows[0].KLogId })).IsValid);

        var longAgoRows = (await writer.History.GetEntityHistoryAsync(HistoryEntities.Note, deletedLongAgo.NoteId)).Entity;
        Assert.HasCount(2, longAgoRows);
        Assert.IsTrue(longAgoRows.All(r => r.PreviousContentSize == null));
        Assert.IsEmpty((await writer.History.GetDeletedWithContentAsync(HistoryEntities.Note, Start.AddDays(-30))).Entity);
        Assert.IsNull((await writer.History.GetPreviousContentAsync(keptRows[0].KLogId)).Entity);
        Assert.AreEqual("v", (await writer.History.GetPreviousContentAsync(keptRows[1].KLogId)).Entity);
        Assert.AreEqual("last", (await writer.History.GetPreviousContentAsync(
            (await writer.History.GetLastAsync(HistoryEntities.Note, deletedToday.NoteId)).Entity.KLogId)).Entity);
    }

    private static KLogDto Row(Guid noteId, EnumHistoryAction action, DateTime when) => new()
    {
        KLogId = Guid.NewGuid(),
        EntityName = HistoryEntities.Note,
        EntityId = noteId,
        RegistryDateTime = when,
        LastDateTime = when,
        ActionType = action.ToString(),
        UserName = "armando",
        Origin = EnumHistoryOrigin.Desktop.ToString(),
        Fields = EnumHistoryFields.Topic,
        ChangeDetails = "{\"topic\":{}}",
        RegistryMessage = $"{action} at {when:HH:mm}",
        EntityTitle = "#1 Topic"
    };

    private static KLogWriteDto Merge(KLogDto session, DateTime now, DateTime expected, string content)
    {
        var merged = Row(session.EntityId, EnumHistoryAction.Updated, session.RegistryDateTime);
        merged.KLogId = session.KLogId;
        merged.LastDateTime = now;
        merged.RegistryMessage = $"Merged at {now.Minute}";
        return new KLogWriteDto { Row = merged, PreviousContent = content, MergeIntoKLogId = session.KLogId, ExpectedLastDateTime = expected };
    }

    private static int Encoding(string text) => System.Text.Encoding.UTF8.GetByteCount(text);

    private static NoteDto NewNote(Guid folderId, string topic) => new()
    {
        NoteId = Guid.NewGuid(), Topic = topic, Description = "Text", FolderId = folderId,
        CreationDateTime = Start, ModificationDateTime = Start
    };

    private static async Task<Guid> AddFolderAsync(IKntRepository repository)
    {
        var folder = await repository.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "History folder" });
        Assert.IsTrue(folder.IsValid, folder.ErrorMessage);
        return folder.Entity.FolderId;
    }

    private static async Task AssertThrowsAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception)
        {
            return;
        }
        Assert.Fail("The write of the row should have failed.");
    }
}

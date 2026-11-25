using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;
using Microsoft.Data.Sqlite;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// The history of the notes, H.2a (03-Historial.md): the parts of a note (tasks, resources, alarms, relations) are rows of their
/// note (H3), in the row of its session (H5), written with the part (if it can't be noted, the part doesn't change, H10). A
/// relation is noted in both notes. Deleting a note doesn't note its parts one by one (H14). Not noted: the parts of the notes
/// KNote writes, the relations KNote makes (@DerivedFrom) and what KNote does by itself (NotNoted). Dapper and EF.
/// </summary>
[TestClass]
public class NotePartsHistoryServiceTests
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
        var user = await service.Repository.Users.AddAsync(new UserDto
        {
            UserId = Guid.NewGuid(), UserName = "armando", EMail = "armando@test.local", FullName = "Armando", RoleDefinition = "Staff"
        });
        Assert.IsTrue(user.IsValid, user.ErrorMessage);
        userId = user.Entity.UserId;
    }

    private Guid userId;

    [TestCleanup]
    public void TestCleanup()
    {
        service?.Dispose();
        db?.Dispose();
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ATask_IsNotedInItsNote_OneRowPerSession(string orm)
    {
        await OpenAsync(orm);
        var noteId = await StoredNoteAsync("Amstrad CPC 464");

        var task = await SaveTaskAsync(NewTask(noteId, "Clean the keyboard"));
        task.SpentTime = 1;
        await SaveTaskAsync(task);
        service.UserIdentityName = "marta";
        task.EndDate = DateTime.Today;
        task = await SaveTaskAsync(task);
        Assert.IsTrue((await service.Notes.DeleteNoteTaskAsync(task.NoteTaskId)).IsValid);

        var rows = await RowsAsync(noteId);
        Assert.HasCount(2, rows);
        var armando = rows.Single(r => r.UserName == "armando");
        Assert.AreEqual("Task added: «Clean the keyboard»", armando.RegistryMessage, "added and then changed, in its session");
        Assert.AreEqual(EnumHistoryFields.Tasks, armando.Fields);
        Assert.IsNull(armando.PreviousContentSize, "a part is not the note's own data: no version");
        Assert.AreEqual("Task removed: «Clean the keyboard»", rows.Single(r => r.UserName == "marta").RegistryMessage);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task TheEditor_SavingTheNoteAndItsParts_IsOneRow(string orm)
    {
        await OpenAsync(orm);
        var noteId = await StoredNoteAsync("A");

        var note = (await service.Notes.GetExtendedAsync(noteId)).Entity;
        note.Topic = "B";
        note.Tasks.Add(NewTask(noteId, "Order the belt"));
        var alarm = (await service.Notes.NewMessageAsync(noteId)).Entity;
        alarm.AlarmDateTime = new DateTime(2026, 12, 1, 9, 0, 0);
        note.Messages.Add(alarm);
        var saved = await service.Notes.SaveExtendedAsync(note);
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);

        var row = (await RowsAsync(noteId)).Single();
        Assert.AreEqual(EnumHistoryFields.Topic | EnumHistoryFields.Tasks | EnumHistoryFields.Alarms, row.Fields);
        Assert.AreEqual("Topic: «A» → «B» · Task added: «Order the belt» · Alarm added: 2026-12-01 09:00", row.RegistryMessage);
        Assert.IsNotNull(row.PreviousContentSize, "the version of the note before its session");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ANewNote_WithItsParts_IsOneCreatedRow(string orm)
    {
        await OpenAsync(orm);

        var note = (await service.Notes.NewExtendedAsync()).Entity;
        note.Topic = "New";
        note.Description = "Text";
        note.FolderId = folderId;
        note.FolderDto = (await service.Repository.Folders.GetAsync(folderId)).Entity;
        note.Tasks.Add(NewTask(Guid.Empty, "First task"));
        var saved = await service.Notes.SaveExtendedAsync(note);
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);

        var row = (await RowsAsync(saved.Entity.NoteId)).Single();
        Assert.AreEqual("Created", row.ActionType);
        Assert.AreEqual("New note in Equipment · Task added: «First task»", row.RegistryMessage);

        // Its own data described again in the session: its parts stay.
        var stored = (await service.Notes.GetAsync(saved.Entity.NoteId)).Entity;
        stored.Topic = "New (renamed)";
        Assert.IsTrue((await service.Notes.SaveAsync(stored, false)).IsValid);
        Assert.AreEqual("New note in Equipment · Task added: «First task»", (await RowsAsync(saved.Entity.NoteId)).Single().RegistryMessage);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task AResource_AddedThenRenamedAndLockedByAnother(string orm)
    {
        await OpenAsync(orm);
        var noteId = await StoredNoteAsync("With photo");

        var resource = await SaveResourceAsync(new ResourceDto
        {
            NoteId = noteId, Name = $"{Guid.NewGuid()}_board.jpg", Container = "NotesResources", FileType = "image/jpeg",
            ContentInDB = true, ContentArrayBytes = new byte[] { 1, 2, 3 }
        });
        service.UserIdentityName = "marta";
        resource.Name = resource.Name.Replace("board.jpg", "main-board.jpg");
        resource.DescriptionByUser = true;
        resource.Description = "The main board";
        resource.LoadedDescription = "";
        await SaveResourceAsync(resource); // with its content again, as the editors save it: renamed, not replaced

        var rows = await RowsAsync(noteId);
        Assert.AreEqual("Resource added: board.jpg", rows.Single(r => r.UserName == "armando").RegistryMessage);
        Assert.AreEqual("Resource changed: main-board.jpg (renamed, description, locked)", rows.Single(r => r.UserName == "marta").RegistryMessage);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ARelation_IsNotedInBothNotes_ButNotTheOnesKNoteMakes(string orm)
    {
        await OpenAsync(orm);
        var cpc = await StoredNoteAsync("Amstrad CPC 464");
        var pcw = await StoredNoteAsync("Amstrad PCW");
        var summary = await StoredNoteAsync("Summary");
        var derivedFrom = await service.Repository.TraceNoteTypes.AddAsync(new TraceNoteTypeDto { TraceNoteTypeId = Guid.NewGuid(), Name = KntConst.DerivedFromTraceNoteType });

        var relation = await service.Notes.SaveTraceNoteAsync(new TraceNoteDto { FromId = cpc, ToId = pcw });
        Assert.IsTrue(relation.IsValid, relation.ErrorMessage);
        var derived = await service.Notes.SaveTraceNoteAsync(new TraceNoteDto { FromId = summary, ToId = cpc, TraceNoteTypeId = derivedFrom.Entity.TraceNoteTypeId });
        Assert.IsTrue(derived.IsValid, derived.ErrorMessage);

        var pcwNumber = (await service.Repository.Notes.GetAsync(pcw)).Entity.NoteNumber;
        var cpcNumber = (await service.Repository.Notes.GetAsync(cpc)).Entity.NoteNumber;
        Assert.AreEqual($"Related to #{pcwNumber} Amstrad PCW", (await RowsAsync(cpc)).Single().RegistryMessage);
        Assert.AreEqual($"Related to #{cpcNumber} Amstrad CPC 464", (await RowsAsync(pcw)).Single().RegistryMessage);
        Assert.IsEmpty(await RowsAsync(summary), "@DerivedFrom is KNote's");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task DeletingANote_SaysWhatItHad_AndItsRelatedNoteNotesIt(string orm)
    {
        await OpenAsync(orm);
        var noteId = await StoredNoteAsync("To delete");
        var relatedId = await StoredNoteAsync("Related");
        await SaveTaskAsync(NewTask(noteId, "A task"));
        Assert.IsTrue((await service.Notes.SaveTraceNoteAsync(new TraceNoteDto { FromId = noteId, ToId = relatedId })).IsValid);
        service.UserIdentityName = "marta";

        Assert.IsTrue((await service.Notes.DeleteExtendedAsync(noteId)).IsValid);

        var deleted = (await RowsAsync(noteId)).Single(r => r.ActionType == "Deleted");
        Assert.AreEqual("Note deleted from Equipment · With 1 task, 1 relation", deleted.RegistryMessage);
        Assert.IsFalse((await RowsAsync(noteId)).Any(r => r.UserName == "marta" && r.ActionType != "Deleted"), "its parts, not one by one");
        var number = deleted.EntityTitle.Split(' ')[0];
        Assert.AreEqual($"Relation removed: {number} To delete (note deleted)",
            (await RowsAsync(relatedId)).Single(r => r.UserName == "marta").RegistryMessage);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task WhatKNoteDoesByItself_AndThePartsOfItsNotes_AreNotNoted(string orm)
    {
        await OpenAsync(orm);
        var noteId = await StoredNoteAsync("With an alarm");
        var alarm = (await service.Notes.NewMessageAsync(noteId)).Entity;
        using (KntHistory.NotNoted())
            Assert.IsTrue((await service.Notes.SaveMessageAsync(alarm)).IsValid);

        var chatRoom = await service.Repository.NoteTypes.AddAsync(new NoteTypeDto { NoteTypeId = Guid.NewGuid(), Name = KntConst.ChatRoomTag });
        var threadId = await StoredNoteAsync("A thread", chatRoom.Entity.NoteTypeId);
        await SaveTaskAsync(NewTask(threadId, "Participant"));

        Assert.IsEmpty(await RowsAsync(noteId));
        Assert.IsEmpty(await RowsAsync(threadId));
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task IfTheRowCantBeWritten_ThePartDoesntChange(string orm)
    {
        await OpenAsync(orm);
        var noteId = await StoredNoteAsync("Kept");
        var otherId = await StoredNoteAsync("Other");
        // The history can be read but not written.
        SqliteConnection.ClearAllPools();
        using (var connection = new SqliteConnection($"Data Source={db.DatabaseFilePath}"))
        {
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "CREATE TRIGGER NoHistory BEFORE INSERT ON KLogs BEGIN SELECT RAISE(ABORT, 'no history'); END;";
            cmd.ExecuteNonQuery();
        }

        await AssertFailsAsync(() => service.Notes.SaveNoteTaskAsync(NewTask(noteId, "Not saved")));
        await AssertFailsAsync(() => service.Notes.SaveTraceNoteAsync(new TraceNoteDto { FromId = noteId, ToId = otherId }));

        Assert.IsEmpty((await service.Repository.Notes.GetNoteTasksAsync(noteId)).Entity);
        Assert.IsEmpty((await service.Repository.TraceNotes.GetAllByFromIdAsync(noteId)).Entity);
    }

    private async Task<Guid> StoredNoteAsync(string topic, Guid? noteTypeId = null)
    {
        var note = new NoteDto
        {
            NoteId = Guid.NewGuid(), Topic = topic, Description = "Text", Tags = "", FolderId = folderId, NoteTypeId = noteTypeId,
            ContentType = "markdown", CreationDateTime = new DateTime(2026, 1, 1), ModificationDateTime = new DateTime(2026, 1, 1)
        };
        Assert.IsTrue((await service.Repository.Notes.AddAsync(note)).IsValid);
        return note.NoteId;
    }

    private NoteTaskDto NewTask(Guid noteId, string description)
    {
        var task = new NoteTaskDto { NoteId = noteId, UserId = userId, Description = description, Priority = 1 };
        task.SetIsNew(true);
        task.SetIsDirty(true);
        return task;
    }

    private async Task<NoteTaskDto> SaveTaskAsync(NoteTaskDto task)
    {
        var saved = await service.Notes.SaveNoteTaskAsync(task);
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        return saved.Entity;
    }

    private async Task<ResourceDto> SaveResourceAsync(ResourceDto resource)
    {
        var saved = await service.Notes.SaveResourceAsync(resource);
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        return saved.Entity;
    }

    private async Task<List<KLogDto>> RowsAsync(Guid noteId) =>
        (await service.Repository.History.GetEntityHistoryAsync(HistoryEntities.Note, noteId)).Entity;

    private static async Task AssertFailsAsync<T>(Func<Task<T>> action) where T : ResultBase
    {
        try
        {
            var result = await action();
            Assert.IsFalse(result.IsValid, "it should have failed");
        }
        catch (Exception)
        {
            // Failed: what matters is that the part didn't change.
        }
    }
}

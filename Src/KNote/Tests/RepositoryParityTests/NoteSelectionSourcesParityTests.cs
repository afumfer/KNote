using KNote.Model.Dto;
using KNote.Repository;
using KNote.Tests.Helpers;

namespace KNote.Tests.RepositoryParityTests;

/// <summary>
/// GetSelectionSourcesAsync (KNote Atlas, stage 2, 2.3, D6), the same with both ORMs, written with one and read with the other: the
/// notes modified in [from, to) or with a task that started (StartDate or, without it, its creation) or ended in it; with their
/// texts and how many tasks (and with activity in the period), resources and relations (both ways) they have.
/// </summary>
[TestClass]
public class NoteSelectionSourcesParityTests
{
    private static readonly DateTime From = new(2020, 3, 9);
    private static readonly DateTime To = new(2020, 3, 16);
    private static readonly DateTime Before = new(2019, 6, 1);

    [TestMethod]
    [DataRow("Dapper", "EntityFramework")]
    [DataRow("EntityFramework", "Dapper")]
    public async Task TheNotesOfThePeriod_ByTheirDateOrTheirTasks(string writerOrm, string readerOrm)
    {
        using var db = new RepositoryTestDatabase();
        using var writer = db.CreateRepository(writerOrm);
        using var reader = db.CreateRepository(readerOrm);
        var folderId = await AddFolderAsync(writer);
        var userId = (await writer.Users.GetByUserNameAsync("user1")).Entity.UserId;

        var modified = await AddNoteAsync(writer, folderId, new DateTime(2020, 3, 10, 15, 0, 0), "Modified #12 in the period");
        var lastDay = await AddNoteAsync(writer, folderId, new DateTime(2020, 3, 15, 23, 0, 0));
        var nextDay = await AddNoteAsync(writer, folderId, To);
        var taskStarted = await AddNoteAsync(writer, folderId, Before);
        var taskEnded = await AddNoteAsync(writer, folderId, Before);
        var taskOutside = await AddNoteAsync(writer, folderId, Before);
        await AddTaskAsync(writer, modified, userId, start: new DateTime(2020, 3, 11));
        await AddTaskAsync(writer, modified, userId, start: Before);
        await AddTaskAsync(writer, taskStarted, userId, start: new DateTime(2020, 3, 9));
        await AddTaskAsync(writer, taskEnded, userId, start: Before, end: new DateTime(2020, 3, 12, 8, 0, 0));
        await AddTaskAsync(writer, taskOutside, userId, start: Before, end: new DateTime(2020, 4, 1));
        Assert.IsTrue((await writer.Notes.AddResourceAsync(new ResourceDto { ResourceId = Guid.NewGuid(), NoteId = modified,
            Name = $"{Guid.NewGuid()}_a.pdf", Container = "R", Description = "a.pdf", FileType = "application/pdf" })).IsValid);
        Assert.IsTrue((await writer.TraceNotes.AddAsync(new TraceNoteDto { TraceNoteId = Guid.NewGuid(), FromId = taskEnded, ToId = modified,
            Order = 0, Weight = 1 })).IsValid);

        var rows = (await reader.Notes.GetSelectionSourcesAsync(From, To)).Entity.ToDictionary(r => r.NoteId);

        CollectionAssert.IsSubsetOf(new[] { modified, lastDay, taskStarted, taskEnded }, rows.Keys.ToList());
        Assert.IsFalse(rows.ContainsKey(nextDay), "the day after the period");
        Assert.IsFalse(rows.ContainsKey(taskOutside), "a task that goes on, without activity in the period");
        var row = rows[modified];
        Assert.AreEqual(("Modified #12 in the period", folderId, 2, 1, 1, 1), (row.Description, row.FolderId, row.Tasks, row.TasksWithActivity, row.Resources, row.Relations));
        Assert.AreEqual(new DateTime(2020, 3, 10, 15, 0, 0), row.ModificationDateTime);
        Assert.AreEqual((1, 1, 1), (rows[taskEnded].Tasks, rows[taskEnded].TasksWithActivity, rows[taskEnded].Relations));
        Assert.AreEqual(1, rows[taskStarted].TasksWithActivity);
    }

    // A task without StartDate starts on its creation (the repositories give it now).
    [TestMethod]
    [DataRow("Dapper", "EntityFramework")]
    [DataRow("EntityFramework", "Dapper")]
    public async Task ATaskWithoutStart_StartsOnItsCreation(string writerOrm, string readerOrm)
    {
        using var db = new RepositoryTestDatabase();
        using var writer = db.CreateRepository(writerOrm);
        using var reader = db.CreateRepository(readerOrm);
        var folderId = await AddFolderAsync(writer);
        var noteId = await AddNoteAsync(writer, folderId, Before);
        await AddTaskAsync(writer, noteId, (await writer.Users.GetByUserNameAsync("user1")).Entity.UserId, start: null);

        var today = await reader.Notes.GetSelectionSourcesAsync(DateTime.Today, DateTime.Today.AddDays(1));
        var past = await reader.Notes.GetSelectionSourcesAsync(From, To);

        Assert.AreEqual(1, today.Entity.Single(r => r.NoteId == noteId).TasksWithActivity);
        Assert.IsFalse(past.Entity.Any(r => r.NoteId == noteId));
    }

    private static async Task<Guid> AddFolderAsync(IKntRepository repository)
    {
        var folder = await repository.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = $"Selection {Guid.NewGuid():N}" });
        Assert.IsTrue(folder.IsValid, folder.ErrorMessage);
        return folder.Entity.FolderId;
    }

    private static async Task<Guid> AddNoteAsync(IKntRepository repository, Guid folderId, DateTime modified, string? description = null)
    {
        var noteId = Guid.NewGuid();
        var note = await repository.Notes.AddAsync(new NoteDto
        {
            NoteId = noteId, Topic = $"Note {noteId:N}", Description = description, FolderId = folderId,
            CreationDateTime = modified.Date, ModificationDateTime = modified
        });
        Assert.IsTrue(note.IsValid, note.ErrorMessage);
        return noteId;
    }

    private static async Task AddTaskAsync(IKntRepository repository, Guid noteId, Guid userId, DateTime? start, DateTime? end = null)
    {
        var task = await repository.Notes.AddNoteTaskAsync(new NoteTaskDto
        {
            NoteTaskId = Guid.NewGuid(), NoteId = noteId, UserId = userId, Description = "Do it", Tags = "", Priority = 1,
            StartDate = start, EndDate = end, Resolved = end != null
        });
        Assert.IsTrue(task.IsValid, task.ErrorMessage);
    }
}

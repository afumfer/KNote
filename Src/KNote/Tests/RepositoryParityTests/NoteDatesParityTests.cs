using KNote.Model.Dto;
using KNote.Repository;
using KNote.Tests.Helpers;

namespace KNote.Tests.RepositoryParityTests;

/// <summary>
/// The dates of a note, the same with both ORMs (KNote Atlas, stage 2, D1 and D6): adding a note keeps the dates it receives
/// (an import keeps the original ones; without them, now), moving it or changing its tags modifies it, and what is saved apart
/// from the note (its resources, the text of a document, the AI's description, its tasks) doesn't change its date.
/// </summary>
[TestClass]
public class NoteDatesParityTests
{
    private static readonly DateTime NoteDate = new(2026, 1, 2, 3, 4, 5);

    [TestMethod]
    [DataRow("Dapper", "EntityFramework")]
    [DataRow("EntityFramework", "Dapper")]
    public async Task AddingANote_KeepsTheDatesItReceives(string writerOrm, string readerOrm)
    {
        using var db = new RepositoryTestDatabase();
        using var writer = db.CreateRepository(writerOrm);
        using var reader = db.CreateRepository(readerOrm);
        var folderId = await AddFolderAsync(writer);
        var created = new DateTime(2020, 5, 6, 7, 8, 9);

        var noteId = await AddNoteAsync(writer, folderId, created, NoteDate);

        var note = (await reader.Notes.GetAsync(noteId)).Entity;
        Assert.AreEqual(created, note.CreationDateTime);
        Assert.AreEqual(NoteDate, note.ModificationDateTime);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task AddingANote_WithoutDates_GivesItNow(string orm)
    {
        using var db = new RepositoryTestDatabase();
        using var repository = db.CreateRepository(orm);
        var folderId = await AddFolderAsync(repository);
        var before = DateTime.Now.AddSeconds(-1);

        var noteId = await AddNoteAsync(repository, folderId, DateTime.MinValue, DateTime.MinValue);

        var note = (await repository.Notes.GetAsync(noteId)).Entity;
        Assert.IsGreaterThanOrEqualTo(before, note.CreationDateTime);
        Assert.IsGreaterThanOrEqualTo(before, note.ModificationDateTime);
    }

    [TestMethod]
    [DataRow("Dapper", "EntityFramework")]
    [DataRow("EntityFramework", "Dapper")]
    public async Task MovingANote_OrChangingItsTags_ModifiesIt(string writerOrm, string readerOrm)
    {
        using var db = new RepositoryTestDatabase();
        using var writer = db.CreateRepository(writerOrm);
        using var reader = db.CreateRepository(readerOrm);
        var folderId = await AddFolderAsync(writer);
        var otherFolderId = await AddFolderAsync(writer);
        var moved = await AddNoteAsync(writer, folderId, NoteDate, NoteDate);
        var tagged = await AddNoteAsync(writer, folderId, NoteDate, NoteDate);
        var retagged = await AddNoteAsync(writer, folderId, NoteDate, NoteDate, tags: "old");
        var before = DateTime.Now.AddSeconds(-1);

        Assert.IsTrue((await writer.Notes.PatchFolderAsync(moved, otherFolderId)).Entity);
        Assert.IsTrue((await writer.Notes.PatchChangeTagsAsync(tagged, null, "new")).Entity);
        Assert.IsTrue((await writer.Notes.PatchChangeTagsAsync(retagged, "old", "new")).Entity);

        foreach (var noteId in new[] { moved, tagged, retagged })
        {
            var note = (await reader.Notes.GetAsync(noteId)).Entity;
            Assert.IsGreaterThanOrEqualTo(before, note.ModificationDateTime, note.Topic);
            Assert.AreEqual(NoteDate, note.CreationDateTime, note.Topic);
        }
        Assert.AreEqual(otherFolderId, (await reader.Notes.GetAsync(moved)).Entity.FolderId);
        Assert.AreEqual("new", (await reader.Notes.GetAsync(tagged)).Entity.Tags);
        Assert.AreEqual("new", (await reader.Notes.GetAsync(retagged)).Entity.Tags);
    }

    [TestMethod]
    [DataRow("Dapper", "EntityFramework")]
    [DataRow("EntityFramework", "Dapper")]
    public async Task UpdatingANote_ChangesItsDate_UnlessItIsKept(string writerOrm, string readerOrm)
    {
        using var db = new RepositoryTestDatabase();
        using var writer = db.CreateRepository(writerOrm);
        using var reader = db.CreateRepository(readerOrm);
        var folderId = await AddFolderAsync(writer);
        var updated = await AddNoteAsync(writer, folderId, NoteDate, NoteDate);
        var kept = await AddNoteAsync(writer, folderId, NoteDate, NoteDate);
        var before = DateTime.Now.AddSeconds(-1);

        var note = (await writer.Notes.GetAsync(updated)).Entity;
        note.Topic = "Updated";
        Assert.IsTrue((await writer.Notes.UpdateAsync(note)).IsValid);
        var status = (await writer.Notes.GetAsync(kept)).Entity;
        status.InternalTags = "Alarms pending";
        var keptRes = await writer.Notes.UpdateAsync(status, keepModificationDate: true);
        Assert.IsTrue(keptRes.IsValid, keptRes.ErrorMessage);

        Assert.IsGreaterThanOrEqualTo(before, (await reader.Notes.GetAsync(updated)).Entity.ModificationDateTime);
        var stored = (await reader.Notes.GetAsync(kept)).Entity;
        Assert.AreEqual(NoteDate, stored.ModificationDateTime);
        Assert.AreEqual("Alarms pending", stored.InternalTags);
        Assert.AreEqual(NoteDate, keptRes.Entity.ModificationDateTime);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task WhatIsSavedApartFromTheNote_DoesNotChangeItsDate(string orm)
    {
        using var db = new RepositoryTestDatabase();
        using var repository = db.CreateRepository(orm);
        var noteId = await AddNoteAsync(repository, await AddFolderAsync(repository), NoteDate, NoteDate);

        var resource = new ResourceDto { ResourceId = Guid.NewGuid(), NoteId = noteId, Name = $"{Guid.NewGuid()}_manual.pdf", Container = "R",
            Description = "manual.pdf", FileType = "application/pdf" };
        Assert.IsTrue((await repository.Notes.AddResourceAsync(resource)).IsValid);
        var stored = (await repository.Notes.GetResourceAsync(resource.ResourceId)).Entity;
        stored.Description = "The manual.";
        Assert.IsTrue((await repository.Notes.UpdateResourceAsync(stored)).IsValid);
        Assert.IsTrue((await repository.Notes.SaveResourceTextAsync(resource.ResourceId, "Page one", EnumExtractedState.Complete, resource.Name)).Entity);
        var photo = new ResourceDto { ResourceId = Guid.NewGuid(), NoteId = noteId, Name = $"{Guid.NewGuid()}.png", Container = "R",
            Description = "photo.png", FileType = "image/png" };
        Assert.IsTrue((await repository.Notes.AddResourceAsync(photo)).IsValid);
        Assert.IsTrue((await repository.Notes.SaveResourceAiDescriptionAsync(photo.ResourceId, "A QL.", "qwen3.5:9b", "photo.png", photo.Name)).Entity);
        var user = new UserDto { UserId = Guid.NewGuid(), UserName = "dates", EMail = "dates@knote.tests", FullName = "Dates", RoleDefinition = "Staff" };
        Assert.IsTrue((await repository.Users.AddAsync(user)).IsValid);
        var task = new NoteTaskDto { NoteTaskId = Guid.NewGuid(), NoteId = noteId, UserId = user.UserId, Description = "A task",
            CreationDateTime = DateTime.Now, ModificationDateTime = DateTime.Now };
        Assert.IsTrue((await repository.Notes.AddNoteTaskAsync(task)).IsValid);
        task.Description = "The task";
        Assert.IsTrue((await repository.Notes.UpdateNoteTaskAsync(task)).IsValid);
        Assert.IsTrue((await repository.Notes.DeleteResourceAsync(photo.ResourceId)).IsValid);

        Assert.AreEqual(NoteDate, (await repository.Notes.GetAsync(noteId)).Entity.ModificationDateTime);
    }

    private static async Task<Guid> AddFolderAsync(IKntRepository repository)
    {
        var folder = await repository.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = $"Dates {Guid.NewGuid():N}" });
        Assert.IsTrue(folder.IsValid, folder.ErrorMessage);
        return folder.Entity.FolderId;
    }

    private static async Task<Guid> AddNoteAsync(IKntRepository repository, Guid folderId, DateTime created, DateTime modified, string? tags = null)
    {
        var noteId = Guid.NewGuid();
        var note = await repository.Notes.AddAsync(new NoteDto
        {
            NoteId = noteId,
            Topic = $"Note {noteId:N}",
            FolderId = folderId,
            Tags = tags,
            CreationDateTime = created,
            ModificationDateTime = modified
        });
        Assert.IsTrue(note.IsValid, note.ErrorMessage);
        return noteId;
    }
}

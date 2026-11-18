using KNote.Model.Dto;
using KNote.Repository;
using KNote.Tests.Helpers;

namespace KNote.Tests.RepositoryParityTests;

/// <summary>
/// GetClassificationsAsync (KNote Atlas, stage 2, 2.2): every note with its folder, its type and its tags, the same with both ORMs,
/// written with one and read with the other.
/// </summary>
[TestClass]
public class NoteClassificationsParityTests
{
    [TestMethod]
    [DataRow("Dapper", "EntityFramework")]
    [DataRow("EntityFramework", "Dapper")]
    public async Task EveryNote_WithItsFolderTypeAndTags(string writerOrm, string readerOrm)
    {
        using var db = new RepositoryTestDatabase();
        using var writer = db.CreateRepository(writerOrm);
        using var reader = db.CreateRepository(readerOrm);
        var folder = await writer.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = $"Noise {Guid.NewGuid():N}" });
        Assert.IsTrue(folder.IsValid, folder.ErrorMessage);
        var type = await writer.NoteTypes.AddAsync(new NoteTypeDto { NoteTypeId = Guid.NewGuid(), Name = $"Type {Guid.NewGuid():N}" });
        Assert.IsTrue(type.IsValid, type.ErrorMessage);
        var typed = await AddNoteAsync(writer, folder.Entity.FolderId, type.Entity.NoteTypeId, "work #NoKnowledge");
        var plain = await AddNoteAsync(writer, folder.Entity.FolderId, null, null);

        var rows = (await reader.Notes.GetClassificationsAsync()).Entity;

        var typedRow = rows.Single(r => r.NoteId == typed);
        Assert.AreEqual(folder.Entity.FolderId, typedRow.FolderId);
        Assert.AreEqual(type.Entity.NoteTypeId, typedRow.NoteTypeId);
        Assert.AreEqual("work #NoKnowledge", typedRow.Tags);
        Assert.IsGreaterThan(0, typedRow.NoteNumber);
        var plainRow = rows.Single(r => r.NoteId == plain);
        Assert.IsNull(plainRow.NoteTypeId);
        Assert.IsTrue(string.IsNullOrEmpty(plainRow.Tags));
    }

    private static async Task<Guid> AddNoteAsync(IKntRepository repository, Guid folderId, Guid? noteTypeId, string? tags)
    {
        var noteId = Guid.NewGuid();
        var note = await repository.Notes.AddAsync(new NoteDto
        {
            NoteId = noteId, Topic = $"Note {noteId:N}", FolderId = folderId, NoteTypeId = noteTypeId, Tags = tags
        });
        Assert.IsTrue(note.IsValid, note.ErrorMessage);
        return noteId;
    }
}

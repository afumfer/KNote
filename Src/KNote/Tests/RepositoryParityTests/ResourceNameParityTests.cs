using KNote.Model.Dto;
using KNote.Repository;
using KNote.Tests.Helpers;

namespace KNote.Tests.RepositoryParityTests;

/// <summary>
/// IKntNoteRepository.GetResourceIdByNameAsync, the same with both ORMs: the resource of a note with a name (the unique index of
/// NoteId + Name that the Service checks before saving a resource), and nothing for another note or another name.
/// </summary>
[TestClass]
public class ResourceNameParityTests
{
    [TestMethod]
    [DataRow("Dapper", "EntityFramework")]
    [DataRow("EntityFramework", "Dapper")]
    public async Task GetResourceIdByName_FindsTheResourceOfTheNoteWithThatName(string writerOrm, string readerOrm)
    {
        using var db = new RepositoryTestDatabase();
        using var writer = db.CreateRepository(writerOrm);
        using var reader = db.CreateRepository(readerOrm);
        var folderId = await AddFolderAsync(writer);
        var noteId = await AddNoteAsync(writer, folderId);
        var otherNoteId = await AddNoteAsync(writer, folderId);
        var resourceId = await AddResourceAsync(writer, noteId, "photo.png");
        await AddResourceAsync(writer, otherNoteId, "other.png");

        Assert.AreEqual(resourceId, (await reader.Notes.GetResourceIdByNameAsync(noteId, "photo.png")).Entity);
        Assert.IsNull((await reader.Notes.GetResourceIdByNameAsync(noteId, "other.png")).Entity, "a resource of another note");
        Assert.IsNull((await reader.Notes.GetResourceIdByNameAsync(otherNoteId, "photo.png")).Entity, "another note");
        Assert.IsNull((await reader.Notes.GetResourceIdByNameAsync(noteId, "missing.png")).Entity);
    }

    private static async Task<Guid> AddFolderAsync(IKntRepository repository)
    {
        var folder = await repository.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = $"Resources {Guid.NewGuid():N}" });
        Assert.IsTrue(folder.IsValid, folder.ErrorMessage);
        return folder.Entity.FolderId;
    }

    private static async Task<Guid> AddNoteAsync(IKntRepository repository, Guid folderId)
    {
        var noteId = Guid.NewGuid();
        var note = await repository.Notes.AddAsync(new NoteDto
        {
            NoteId = noteId,
            Topic = $"Note {noteId:N}",
            FolderId = folderId,
            CreationDateTime = DateTime.Now,
            ModificationDateTime = DateTime.Now
        });
        Assert.IsTrue(note.IsValid, note.ErrorMessage);
        return noteId;
    }

    private static async Task<Guid> AddResourceAsync(IKntRepository repository, Guid noteId, string name)
    {
        var resource = await repository.Notes.AddResourceAsync(new ResourceDto
        {
            ResourceId = Guid.NewGuid(),
            NoteId = noteId,
            Name = name,
            Container = "KntResources",
            FileType = "image/png",
            ContentInDB = true,
            ContentArrayBytes = [1, 2, 3]
        });
        Assert.IsTrue(resource.IsValid, resource.ErrorMessage);
        return resource.Entity.ResourceId;
    }
}

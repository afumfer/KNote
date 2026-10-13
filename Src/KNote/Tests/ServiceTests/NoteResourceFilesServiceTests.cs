using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// The files of the note resources (IKntNoteService.SaveResourceAsync / GetResourcesAsync), kept in
/// RepositoryRef.ResourcesContainerRootPath: a resource whose file can't be written isn't saved (the user gets an
/// error, not an empty resource), and the URL of a resource is given even if this process can't read its file (the
/// web server may serve it, e.g. IIS with an application pool without permissions on the folder).
/// </summary>
[TestClass]
public class NoteResourceFilesServiceTests
{
    private const string RootUrl = "http://intranet/KNote";
    private const string Container = "KntResources";

    private RepositoryTestDatabase db = null!;
    private KntService service = null!;
    private string rootPath = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        db = new RepositoryTestDatabase();
        service = new KntService(db.CreateRepository("EntityFramework"), activateMessageBroker: false);

        rootPath = Path.Combine(Path.GetTempPath(), $"knote-resources-{Guid.NewGuid():N}");
        Directory.CreateDirectory(rootPath);
        SetRootPath(rootPath);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        db.Dispose();
        if (Directory.Exists(rootPath))
            Directory.Delete(rootPath, true);
    }

    [TestMethod]
    public async Task SaveResourceAsync_WritesTheFileAndGivesItsUrl()
    {
        var noteId = await AddNoteAsync();

        var saved = await service.Notes.SaveResourceAsync(NewResource(noteId, "image.png"));

        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        Assert.IsTrue(File.Exists(service.Notes.UtilGetResourceFilePath(saved.Entity)));
        Assert.AreEqual($"{RootUrl}/{saved.Entity.Container}/image.png", saved.Entity.FullUrl);
    }

    [TestMethod]
    public async Task SaveResourceAsync_WhenTheFileCantBeWritten_ReturnsAnErrorAndSavesNothing()
    {
        var noteId = await AddNoteAsync();
        // A file where the folder should be: the folder of the resource can't be created.
        var notAFolder = Path.Combine(rootPath, "not-a-folder");
        File.WriteAllText(notAFolder, "");
        SetRootPath(notAFolder);

        var saved = await service.Notes.SaveResourceAsync(NewResource(noteId, "image.png"));

        Assert.IsFalse(saved.IsValid);
        var resources = await service.Notes.GetResourcesAsync(noteId);
        Assert.IsEmpty(resources.Entity);
    }

    [TestMethod]
    public async Task GetResourcesAsync_WhenTheFileCantBeRead_StillGivesItsUrl()
    {
        var noteId = await AddNoteAsync();
        var saved = await service.Notes.SaveResourceAsync(NewResource(noteId, "image.png"));
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        File.Delete(service.Notes.UtilGetResourceFilePath(saved.Entity));

        var resources = await service.Notes.GetResourcesAsync(noteId);

        Assert.HasCount(1, resources.Entity);
        Assert.AreEqual($"{RootUrl}/{saved.Entity.Container}/image.png", resources.Entity[0].FullUrl);
    }

    private void SetRootPath(string path)
    {
        var repositoryRef = service.Repository.RepositoryRef;
        repositoryRef.ResourcesContainer = Container;
        repositoryRef.ResourcesContainerRootPath = path;
        repositoryRef.ResourcesContainerRootUrl = RootUrl;
    }

    private static ResourceDto NewResource(Guid noteId, string name) => new()
    {
        NoteId = noteId,
        Name = name,
        Description = name,
        FileType = "image/png",
        ContentArrayBytes = [0x89, 0x50, 0x4E, 0x47]
    };

    private async Task<Guid> AddNoteAsync()
    {
        var folder = await service.Repository.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "Resources folder" });
        Assert.IsTrue(folder.IsValid, folder.ErrorMessage);

        var noteId = Guid.NewGuid();
        var note = await service.Repository.Notes.AddAsync(new NoteDto
        {
            NoteId = noteId,
            Topic = "Note with resources",
            FolderId = folder.Entity.FolderId,
            CreationDateTime = DateTime.Now,
            ModificationDateTime = DateTime.Now
        });
        Assert.IsTrue(note.IsValid, note.ErrorMessage);

        return noteId;
    }
}

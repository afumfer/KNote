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

    // Reading every file whole with the note kept the desktop app from answering while a document came from a slow network
    // folder (at startup, the first note of the last folder). The ones in the database come with the row, as always.
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task TheResourcesOfANote_ComeWithoutTheContentOfTheirFiles(bool contentInDB)
    {
        var noteId = await AddNoteAsync();
        var newResource = NewResource(noteId, "image.png");
        newResource.Container = $"{Container}/2026";   // without it, the repository's default (ContentInDB too) is taken
        newResource.ContentInDB = contentInDB;
        var saved = await service.Notes.SaveResourceAsync(newResource);
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);

        var resource = (await service.Notes.GetExtendedAsync(noteId)).Entity.Resources.Single();

        Assert.AreEqual($"{RootUrl}/{saved.Entity.Container}/image.png", resource.FullUrl);
        if (contentInDB)
            CollectionAssert.AreEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, resource.ContentArrayBytes);
        else
            Assert.IsNull(resource.ContentArrayBytes, "the file is not read");
        CollectionAssert.AreEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, (await service.Notes.GetResourceAsync(saved.Entity.ResourceId)).Entity.ContentArrayBytes,
            "a single resource still comes with its content");
    }

    [TestMethod]
    public async Task AResourceReadWithItsNote_ChangedAndSavedWithIt_KeepsItsFile()
    {
        var noteId = await AddNoteAsync();
        var saved = await service.Notes.SaveResourceAsync(NewResource(noteId, "image.png"));
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        var note = (await service.Notes.GetExtendedAsync(noteId)).Entity;
        note.SetIsDirty(false);

        note.Resources.Single().Description = "A new description";
        var result = await service.Notes.SaveExtendedAsync(note);

        Assert.IsTrue(result.IsValid, result.ErrorMessage);
        var stored = (await service.Notes.GetResourceAsync(saved.Entity.ResourceId)).Entity;
        Assert.AreEqual("A new description", stored.Description);
        CollectionAssert.AreEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, stored.ContentArrayBytes);
        CollectionAssert.AreEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, File.ReadAllBytes(service.Notes.UtilGetResourceFilePath(stored)));
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task SaveResourceAsync_AnExistingResourceInfoWithoutItsFile_OnlyChangesItsDescriptionAndOrder(bool contentInDB)
    {
        // The web edits a resource's description and order, without sending its file back.
        var noteId = await AddNoteAsync();
        var newResource = NewResource(noteId, "image.png");
        newResource.Container = $"{Container}/2026";
        newResource.ContentInDB = contentInDB;
        var saved = await service.Notes.SaveResourceAsync(newResource);
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);

        var edited = new ResourceInfoDto
        {
            ResourceId = saved.Entity.ResourceId,
            NoteId = noteId,
            Name = saved.Entity.Name,
            Container = saved.Entity.Container,
            Description = "New description",
            Order = 7
        };
        var result = await service.Notes.SaveResourceAsync(edited);

        Assert.IsTrue(result.IsValid, result.ErrorMessage);
        var stored = await service.Notes.GetResourceAsync(saved.Entity.ResourceId);
        Assert.AreEqual("New description", stored.Entity.Description);
        Assert.AreEqual(7, stored.Entity.Order);
        Assert.AreEqual(contentInDB, stored.Entity.ContentInDB);
        CollectionAssert.AreEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, stored.Entity.ContentArrayBytes);
        Assert.IsTrue(File.Exists(service.Notes.UtilGetResourceFilePath(stored.Entity)));
    }

    [TestMethod]
    [DataRow("KntResources/2026", "image.png")]
    [DataRow(@"KntResources\2026", "image.png")]
    [DataRow("KntResources/2026", "sub/image.png")]
    [DataRow(@"KntResources\2026", @"sub\image.png")]
    public void UtilGetResourceFilePath_AcceptsBothSeparators_AndUsesThisSystemsOne(string container, string name)
    {
        var resource = new ResourceDto { Container = container, Name = name };

        var path = service.Notes.UtilGetResourceFilePath(resource);

        var expected = Path.Combine(rootPath, "KntResources", "2026") + Path.DirectorySeparatorChar
            + name.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        Assert.AreEqual(expected, path);
        if (OperatingSystem.IsWindows())
            Assert.AreEqual(Path.Combine(rootPath, container.Replace("/", @"\"), name.Replace("/", @"\")), path, "Windows: the same path as before.");
    }

    [TestMethod]
    public async Task SaveResourceAsync_WithTheNameOfAnotherResourceOfTheNote_SaysSoAndKeepsTheOtherFile()
    {
        var noteId = await AddNoteAsync();
        var first = await service.Notes.SaveResourceAsync(NewResource(noteId, "image.png"));
        Assert.IsTrue(first.IsValid, first.ErrorMessage);
        var second = NewResource(noteId, "image.png");
        second.ContentArrayBytes = [1, 2, 3];

        var saved = await service.Notes.SaveResourceAsync(second);

        Assert.IsFalse(saved.IsValid);
        Assert.AreEqual("This note already has a resource with the file \"image.png\". Choose another file, or rename it before adding it.",
            saved.ErrorMessage);
        Assert.HasCount(1, (await service.Notes.GetResourcesAsync(noteId)).Entity);
        CollectionAssert.AreEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, File.ReadAllBytes(service.Notes.UtilGetResourceFilePath(first.Entity)),
            "the file of the other resource is not replaced");
    }

    [TestMethod]
    public async Task SaveResourceAsync_ANewResourceWithANameInUse_AsTheNoteEditorSavesIt_SaysTheFileName()
    {
        // The desktop note editor gives a new resource its id before saving it (forceNew).
        var noteId = await AddNoteAsync();
        var name = $"{Guid.NewGuid()}_report.pdf";
        Assert.IsTrue((await service.Notes.SaveResourceAsync(NewResource(noteId, name))).IsValid);
        var again = NewResource(noteId, name);
        again.ResourceId = Guid.NewGuid();

        var saved = await service.Notes.SaveResourceAsync(again, forceNew: true);

        Assert.IsFalse(saved.IsValid);
        StringAssert.Contains(saved.ErrorMessage, "\"report.pdf\"", "as the user sees it, without the guid");
    }

    [TestMethod]
    public async Task SaveResourceAsync_ItsOwnName_OrTheNameOfAResourceOfAnotherNote_IsSaved()
    {
        var noteId = await AddNoteAsync();
        var otherNoteId = await AddNoteAsync();
        var saved = await service.Notes.SaveResourceAsync(NewResource(noteId, "image.png"));
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);

        saved.Entity.Description = "Changed";
        var again = await service.Notes.SaveResourceAsync(saved.Entity);
        var inOtherNote = await service.Notes.SaveResourceAsync(NewResource(otherNoteId, "image.png"));

        Assert.IsTrue(again.IsValid, again.ErrorMessage);
        Assert.IsTrue(inOtherNote.IsValid, inOtherNote.ErrorMessage);
    }

    [TestMethod]
    public async Task SaveResourceAsync_RenamedToTheNameOfAnotherResourceOfTheNote_IsRefused()
    {
        var noteId = await AddNoteAsync();
        Assert.IsTrue((await service.Notes.SaveResourceAsync(NewResource(noteId, "a.png"))).IsValid);
        var b = await service.Notes.SaveResourceAsync(NewResource(noteId, "b.png"));
        Assert.IsTrue(b.IsValid, b.ErrorMessage);

        b.Entity.Name = "a.png";
        var saved = await service.Notes.SaveResourceAsync(b.Entity);

        Assert.IsFalse(saved.IsValid);
        StringAssert.Contains(saved.ErrorMessage, "\"a.png\"");
        Assert.AreEqual("b.png", (await service.Notes.GetResourceAsync(b.Entity.ResourceId)).Entity.Name);
    }

    [TestMethod]
    public async Task DeleteResourceAsync_DeletesItsFile()
    {
        var noteId = await AddNoteAsync();
        var saved = await service.Notes.SaveResourceAsync(NewResource(noteId, "image.png"));
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        var file = service.Notes.UtilGetResourceFilePath(saved.Entity);
        Assert.IsTrue(File.Exists(file));

        var deleted = await service.Notes.DeleteResourceAsync(saved.Entity.ResourceId);

        Assert.IsTrue(deleted.IsValid, deleted.ErrorMessage);
        Assert.IsFalse(File.Exists(file));
    }

    [TestMethod]
    public void ResourceInfoDto_Validate_OnlyANewResourceNeedsItsContent()
    {
        var newOne = new ResourceInfoDto { Name = "a.png", Container = Container };
        var existing = new ResourceInfoDto { ResourceId = Guid.NewGuid(), Name = "a.png", Container = Container };

        Assert.IsFalse(newOne.IsValid());
        Assert.IsTrue(existing.IsValid(), existing.GetErrorMessage());
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

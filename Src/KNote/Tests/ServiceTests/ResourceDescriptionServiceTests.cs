using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// The rule of the description of a resource when it is saved (KNote Atlas, stage 4, D12; ResourceDescriptionRules), with
/// both ORMs: the lock is what the editor says, an editor that didn't change the text keeps what the AI wrote meanwhile,
/// the model never comes from outside, and saving a resource doesn't change the date of its note.
/// </summary>
[TestClass]
public class ResourceDescriptionServiceTests
{
    private const string AiModel = "qwen3.5:9b";
    private const string AiText = "A Sinclair QL on a white desk.";

    private RepositoryTestDatabase db = null!;
    private KntService service = null!;
    private string rootPath = null!;

    private void Open(string orm)
    {
        db = new RepositoryTestDatabase();
        service = new KntService(db.CreateRepository(orm), activateMessageBroker: false);

        rootPath = Path.Combine(Path.GetTempPath(), $"knote-resources-{Guid.NewGuid():N}");
        Directory.CreateDirectory(rootPath);
        var repositoryRef = service.Repository.RepositoryRef;
        repositoryRef.ResourcesContainer = "KntResources";
        repositoryRef.ResourcesContainerRootPath = rootPath;
        repositoryRef.ResourcesContainerRootUrl = "http://intranet/KNote";
    }

    [TestCleanup]
    public void TestCleanup()
    {
        db?.Dispose();
        if (rootPath != null && Directory.Exists(rootPath))
            Directory.Delete(rootPath, true);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ANewResource_KeepsTheLockItComesWith_AndNeverAModelFromOutside(string orm)
    {
        Open(orm);
        var noteId = await AddNoteAsync();
        var resource = NewResource(noteId);
        resource.Description = "My QL.";
        resource.DescriptionByUser = true;
        resource.DescriptionModel = "made-up";

        var saved = await service.Notes.SaveResourceAsync(resource);

        var stored = await GetAsync(saved.Entity.ResourceId);
        Assert.AreEqual("My QL.", stored.Description);
        Assert.IsTrue(stored.DescriptionByUser);
        Assert.IsNull(stored.DescriptionModel);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task AnEditorOpenedBeforeTheAiWrote_KeepsTheAiText_AndTheLockItSays(string orm)
    {
        Open(orm);
        var noteId = await AddNoteAsync();
        var resourceId = await AddResourceAsync(noteId);
        var editor = await GetAsync(resourceId);          // the editor loads the file name...
        editor.LoadedDescription = editor.Description;
        await WriteAsTheAiAsync(resourceId);              // ...and the AI writes meanwhile.

        editor.DescriptionByUser = true;                  // only the lock is changed
        var saved = await service.Notes.SaveResourceAsync(editor);

        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        var stored = await GetAsync(resourceId);
        Assert.AreEqual(AiText, stored.Description);
        Assert.IsTrue(stored.DescriptionByUser);
        Assert.AreEqual(AiModel, stored.DescriptionModel);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task AnEditorChangingTheText_ClearsTheModel_AndTheCheckboxWins(string orm)
    {
        Open(orm);
        var noteId = await AddNoteAsync();
        var resourceId = await AddResourceAsync(noteId);
        await WriteAsTheAiAsync(resourceId);
        var editor = await GetAsync(resourceId);
        editor.LoadedDescription = editor.Description;

        editor.Description = "My own QL.";
        editor.DescriptionByUser = false;                 // unchecked while changing the text
        await service.Notes.SaveResourceAsync(editor);

        var stored = await GetAsync(resourceId);
        Assert.AreEqual("My own QL.", stored.Description);
        Assert.IsFalse(stored.DescriptionByUser);
        Assert.IsNull(stored.DescriptionModel);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task CodeThatBuildsTheResource_ChangingTheText_LocksIt(string orm)
    {
        Open(orm);
        var noteId = await AddNoteAsync();
        var resourceId = await AddResourceAsync(noteId);
        await WriteAsTheAiAsync(resourceId);
        var resource = await GetAsync(resourceId);
        resource.LoadedDescription = null;                // not read from KNote: what it had is not known

        resource.Description = "Written by a script.";
        await service.Notes.SaveResourceAsync(resource);

        var stored = await GetAsync(resourceId);
        Assert.IsTrue(stored.DescriptionByUser);
        Assert.IsNull(stored.DescriptionModel);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task TheWebEditor_SavesTheLockThroughResourceInfo(string orm)
    {
        Open(orm);
        var noteId = await AddNoteAsync();
        var resourceId = await AddResourceAsync(noteId);
        await WriteAsTheAiAsync(resourceId);
        var info = (await GetAsync(resourceId)).GetSimpleDto<ResourceInfoDto>();
        info.LoadedDescription = info.Description;
        info.ContentBase64 = null;

        info.DescriptionByUser = true;
        var saved = await service.Notes.SaveResourceAsync(info);

        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        var stored = await GetAsync(resourceId);
        Assert.AreEqual(AiText, stored.Description);
        Assert.IsTrue(stored.DescriptionByUser);
        Assert.AreEqual(AiModel, stored.DescriptionModel);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task SavingAResource_DoesNotChangeTheDateOfItsNote(string orm)
    {
        Open(orm);
        var noteId = await AddNoteAsync();
        var before = (await service.Notes.GetAsync(noteId)).Entity.ModificationDateTime;

        var resourceId = await AddResourceAsync(noteId);
        var editor = await GetAsync(resourceId);
        editor.LoadedDescription = editor.Description;
        editor.Description = "My QL.";
        editor.DescriptionByUser = true;
        await service.Notes.SaveResourceAsync(editor);

        Assert.AreEqual(before, (await service.Notes.GetAsync(noteId)).Entity.ModificationDateTime);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task SavingTheNote_WithAResourceNotTouched_KeepsWhatTheAiWrote(string orm)
    {
        Open(orm);
        var noteId = await AddNoteAsync();
        var resourceId = await AddResourceAsync(noteId);
        var note = (await service.Notes.GetExtendedAsync(noteId)).Entity;  // the editor opens the note...
        await WriteAsTheAiAsync(resourceId);                                // ...the AI writes...

        note.Topic = "Another topic";                                        // ...and the note is saved.
        var saved = await service.Notes.SaveExtendedAsync(note);

        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        var stored = await GetAsync(resourceId);
        Assert.AreEqual(AiText, stored.Description);
        Assert.AreEqual(AiModel, stored.DescriptionModel);
    }

    #region Helpers

    // What the engine of 4.2 will do, by a conditional update: here, through the repository.
    private async Task WriteAsTheAiAsync(Guid resourceId)
    {
        var resource = (await service.Repository.Notes.GetResourceAsync(resourceId)).Entity;
        resource.Description = AiText;
        resource.DescriptionModel = AiModel;
        var res = await service.Repository.Notes.UpdateResourceAsync(resource);
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
    }

    private async Task<ResourceDto> GetAsync(Guid resourceId)
    {
        var res = await service.Notes.GetResourceAsync(resourceId);
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        return res.Entity;
    }

    private async Task<Guid> AddResourceAsync(Guid noteId)
    {
        var saved = await service.Notes.SaveResourceAsync(NewResource(noteId));
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        Assert.IsFalse(saved.Entity.DescriptionByUser);
        return saved.Entity.ResourceId;
    }

    private static ResourceDto NewResource(Guid noteId) => new()
    {
        NoteId = noteId,
        Name = $"{Guid.NewGuid()}_photo.png",
        Description = "photo.png",
        FileType = "image/png",
        ContentArrayBytes = [0x89, 0x50, 0x4E, 0x47]
    };

    private async Task<Guid> AddNoteAsync()
    {
        var folder = await service.Repository.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "Resources folder" });
        Assert.IsTrue(folder.IsValid, folder.ErrorMessage);

        var noteId = Guid.NewGuid();
        var date = new DateTime(2026, 1, 2, 3, 4, 5);
        var note = await service.Repository.Notes.AddAsync(new NoteDto
        {
            NoteId = noteId,
            Topic = "Note with resources",
            FolderId = folder.Entity.FolderId,
            CreationDateTime = date,
            ModificationDateTime = date
        });
        Assert.IsTrue(note.IsValid, note.ErrorMessage);

        return noteId;
    }

    #endregion
}

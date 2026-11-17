using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// What the desktop editor gets back when it saves a note with its items (SaveExtendedAsync): the items saved are no longer new,
/// with Dapper and EF. Dapper's repository gives back the same object it saved and the editor marks its new items with
/// SetIsNew(true): kept new, the editor asked to save a document that was saved («Save the note first») for as long as it was
/// open, saw the note always changed and deleting one of those items only took it out of the list.
/// </summary>
[TestClass]
public class NoteExtendedSaveServiceTests
{
    private RepositoryTestDatabase db = null!;
    private KntService service = null!;
    private string rootPath = null!;
    private Guid folderId;

    private async Task OpenAsync(string orm)
    {
        db = new RepositoryTestDatabase();
        service = new KntService(db.CreateRepository(orm), activateMessageBroker: false);

        rootPath = Path.Combine(Path.GetTempPath(), $"knote-extended-{Guid.NewGuid():N}");
        Directory.CreateDirectory(rootPath);
        var repositoryRef = service.Repository.RepositoryRef;
        repositoryRef.ResourcesContainer = "KntResources";
        repositoryRef.ResourcesContainerRootPath = rootPath;
        repositoryRef.ResourcesContainerRootUrl = "http://intranet/KNote";

        var folder = await service.Repository.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "Extended folder" });
        Assert.IsTrue(folder.IsValid, folder.ErrorMessage);
        folderId = folder.Entity.FolderId;
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
    public async Task SavingTheExtendedNote_ItsNewItemsComeBackSaved(string orm)
    {
        await OpenAsync(orm);
        var noteId = await AddNoteAsync("Project");
        var otherId = await AddNoteAsync("Other");
        var user = new UserDto { UserId = Guid.NewGuid(), UserName = "extended", EMail = "extended@knote.tests", FullName = "Extended", RoleDefinition = "Staff" };
        Assert.IsTrue((await service.Repository.Users.AddAsync(user)).IsValid);
        var note = (await service.Notes.GetExtendedAsync(noteId)).Entity;
        note.SetIsDirty(false); // as the desktop editor loads it

        // As the desktop editor adds them (NoteEditorCtrl: NewResourceFromFile, AddTask, AddMessage, AddTraceNote).
        note.Resources.Add(New(new ResourceDto
        {
            ResourceId = Guid.NewGuid(), NoteId = noteId, Name = $"{Guid.NewGuid()}_report.docx", Description = "report.docx",
            FileType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            Container = service.Notes.UtilGetDefaultNewResourceContainer(), ContentArrayBytes = [0x50, 0x4B, 0x03, 0x04]
        }));
        note.Tasks.Add(New(new NoteTaskDto { NoteId = noteId, UserId = user.UserId, Description = "A task", CreationDateTime = DateTime.Now, ModificationDateTime = DateTime.Now }));
        note.Messages.Add(New(new KMessageDto { NoteId = noteId, UserId = user.UserId, Comment = "Call", AlarmActivated = true, AlarmDateTime = DateTime.Now.AddDays(1),
            AlarmType = EnumAlarmType.Standard, NotificationType = EnumNotificationType.AppInfo }));
        note.TraceNotesTo.Add(New(new TraceNoteDto { ToId = otherId, Weight = 1.0 }));

        var saved = await service.Notes.SaveExtendedAsync(note);

        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        var items = saved.Entity.Resources.Cast<SmartModelDtoBase>().Concat(saved.Entity.Tasks).Concat(saved.Entity.Messages).Concat(saved.Entity.TraceNotesTo).ToList();
        Assert.HasCount(4, items);
        foreach (var item in items)
            Assert.IsFalse(item.IsNew(), $"{item.GetType().Name} saved and still new");

        // What the editor does next (NoteEditorCtrl.SaveModelCore): the note, saved, has nothing left to save.
        saved.Entity.SetIsDirty(false);
        saved.Entity.SetIsNew(false);
        Assert.IsFalse(saved.Entity.IsDirty(), "the note saved still has changes");
    }

    private static T New<T>(T item) where T : SmartModelDtoBase
    {
        item.SetIsNew(true);
        return item;
    }

    private async Task<Guid> AddNoteAsync(string topic)
    {
        var noteId = Guid.NewGuid();
        var note = await service.Repository.Notes.AddAsync(new NoteDto
        {
            NoteId = noteId,
            Topic = topic,
            Description = "",
            ContentType = "markdown",
            FolderId = folderId,
            CreationDateTime = DateTime.Now,
            ModificationDateTime = DateTime.Now
        });
        Assert.IsTrue(note.IsValid, note.ErrorMessage);
        return noteId;
    }
}

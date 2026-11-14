using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// When a note is «modified» (KNote Atlas, stage 2, D1 and D6: the summaries of a period read it): saving it, moving it or
/// changing its tags; never what is saved apart from it (its resources, the AI's description, its tasks, alarms, relations and
/// its post-it window), also when the desktop editor saves the note with them (SaveExtendedAsync) or the assistant saves it for
/// its status. Dapper and EF.
/// </summary>
[TestClass]
public class NoteModificationDateServiceTests
{
    private static readonly DateTime NoteDate = new(2026, 1, 2, 3, 4, 5);

    private RepositoryTestDatabase db = null!;
    private KntService service = null!;
    private string rootPath = null!;
    private Guid folderId;

    private async Task OpenAsync(string orm)
    {
        db = new RepositoryTestDatabase();
        service = new KntService(db.CreateRepository(orm), activateMessageBroker: false);

        rootPath = Path.Combine(Path.GetTempPath(), $"knote-dates-{Guid.NewGuid():N}");
        Directory.CreateDirectory(rootPath);
        var repositoryRef = service.Repository.RepositoryRef;
        repositoryRef.ResourcesContainer = "KntResources";
        repositoryRef.ResourcesContainerRootPath = rootPath;
        repositoryRef.ResourcesContainerRootUrl = "http://intranet/KNote";

        var folder = await service.Repository.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "Dates folder" });
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
    public async Task SavingMovingOrTaggingANote_ModifiesIt(string orm)
    {
        await OpenAsync(orm);
        var saved = await AddNoteAsync("Saved");
        var moved = await AddNoteAsync("Moved");
        var tagged = await AddNoteAsync("Tagged");
        var otherFolder = await service.Repository.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "Other folder" });
        var before = DateTime.Now.AddSeconds(-1);

        var note = (await service.Notes.GetAsync(saved)).Entity;
        note.Description = "A new text.";
        var save = await service.Notes.SaveAsync(note);
        Assert.IsTrue(save.IsValid, save.ErrorMessage);
        Assert.IsTrue((await service.Notes.UtilPatchFolderAsync(moved, otherFolder.Entity.FolderId)).Entity);
        Assert.IsTrue((await service.Notes.UtilPatchChangeTagsAsync(tagged, null, "summary")).Entity);

        foreach (var noteId in new[] { saved, moved, tagged })
        {
            var stored = (await service.Repository.Notes.GetAsync(noteId)).Entity;
            Assert.IsGreaterThanOrEqualTo(before, stored.ModificationDateTime, stored.Topic);
            Assert.AreEqual(NoteDate, stored.CreationDateTime, stored.Topic);
        }
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ItsResources_DoNotModifyTheNote(string orm)
    {
        await OpenAsync(orm);
        var noteId = await AddNoteAsync("Trip");

        var photo = await service.Notes.SaveResourceAsync(new ResourceDto
        {
            NoteId = noteId, Name = $"{Guid.NewGuid()}_beach.png", Description = "beach.png", FileType = "image/png", ContentArrayBytes = Png()
        });
        Assert.IsTrue(photo.IsValid, photo.ErrorMessage);
        await AssertNotModifiedAsync(noteId, "a new resource");

        var stored = (await service.Notes.GetResourceAsync(photo.Entity.ResourceId)).Entity;
        stored.Order = 2;
        Assert.IsTrue((await service.Notes.SaveResourceAsync(stored)).IsValid);
        await AssertNotModifiedAsync(noteId, "the resource saved");

        var info = (await service.Notes.GetResourcesInfoAsync(noteId)).Entity.Single();
        info.Order = 3;
        Assert.IsTrue((await service.Notes.SaveResourceAsync(info)).IsValid);
        await AssertNotModifiedAsync(noteId, "its info saved");

        var current = (await service.Notes.GetResourceAsync(photo.Entity.ResourceId)).Entity;
        var described = await service.ResourceContent.SaveAiDescriptionAsync(new AiDescriptionSaveDto
        {
            ResourceId = current.ResourceId, Description = "A beach at sunset.", Model = "qwen3.5:9b",
            ExpectedDescription = current.Description, ExpectedName = current.Name
        });
        Assert.IsTrue(described.IsValid, described.ErrorMessage);
        await AssertNotModifiedAsync(noteId, "the AI's description");

        Assert.IsTrue((await service.Notes.DeleteResourceAsync(photo.Entity.ResourceId)).IsValid);
        await AssertNotModifiedAsync(noteId, "the resource deleted");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ItsTasksAlarmsRelationsAndWindow_DoNotModifyTheNote(string orm)
    {
        await OpenAsync(orm);
        var noteId = await AddNoteAsync("Project");
        var otherId = await AddNoteAsync("Other");
        var user = await AddUserAsync();

        var task = (await service.Notes.SaveNoteTaskAsync(new NoteTaskDto
        {
            NoteId = noteId, UserId = user.UserId, Description = "A task", CreationDateTime = DateTime.Now, ModificationDateTime = DateTime.Now
        })).Entity;
        task.Description = "The task";
        Assert.IsTrue((await service.Notes.SaveNoteTaskAsync(task)).IsValid);
        Assert.IsTrue((await service.Notes.DeleteNoteTaskAsync(task.NoteTaskId)).IsValid);
        await AssertNotModifiedAsync(noteId, "its tasks");

        var alarm = (await service.Notes.SaveMessageAsync(new KMessageDto
        {
            NoteId = noteId, UserId = user.UserId, Comment = "Call", AlarmActivated = true, AlarmDateTime = DateTime.Now.AddDays(1),
            AlarmType = EnumAlarmType.Standard, NotificationType = EnumNotificationType.AppInfo
        })).Entity;
        alarm.Comment = "Call again";
        Assert.IsTrue((await service.Notes.SaveMessageAsync(alarm)).IsValid);
        Assert.IsTrue((await service.Notes.DeleteMessageAsync(alarm.KMessageId)).IsValid);
        await AssertNotModifiedAsync(noteId, "its alarms");

        var relation = await service.Notes.SaveTraceNoteAsync(new TraceNoteDto { FromId = noteId, ToId = otherId, Weight = 1.0 });
        Assert.IsTrue(relation.IsValid, relation.ErrorMessage);
        Assert.IsTrue((await service.Notes.DeleteTraceNoteAsync(relation.Entity.TraceNoteId)).IsValid);
        await AssertNotModifiedAsync(noteId, "its relations");
        await AssertNotModifiedAsync(otherId, "the relations to it");

        Assert.IsTrue((await service.Notes.SaveWindowAsync(new WindowDto { NoteId = noteId, UserId = user.UserId, Visible = true })).IsValid);
        await AssertNotModifiedAsync(noteId, "its post-it window");
    }

    // The desktop editor saves the note with all its items (SaveExtendedAsync): saving only them keeps its date, all of them are
    // saved, they come back in the result and the status of the note is written.
    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task SavingTheExtendedNote_WithOnlyItsItemsChanged_KeepsItsDate(string orm)
    {
        await OpenAsync(orm);
        var noteId = await AddNoteAsync("Project");
        var otherId = await AddNoteAsync("Other");
        var user = await AddUserAsync();
        var note = (await service.Notes.GetExtendedAsync(noteId)).Entity;
        note.SetIsDirty(false); // as the desktop editor loads it

        note.Resources.Add(new ResourceDto { NoteId = noteId, Name = $"{Guid.NewGuid()}_beach.png", Description = "beach.png", FileType = "image/png", ContentArrayBytes = Png() });
        note.Tasks.Add(new NoteTaskDto { NoteId = noteId, UserId = user.UserId, Description = "A task", CreationDateTime = DateTime.Now, ModificationDateTime = DateTime.Now });
        note.Messages.Add(new KMessageDto { NoteId = noteId, UserId = user.UserId, Comment = "Call", AlarmActivated = true, AlarmDateTime = DateTime.Now.AddDays(1),
            AlarmType = EnumAlarmType.Standard, NotificationType = EnumNotificationType.AppInfo });
        note.TraceNotesTo.Add(new TraceNoteDto { ToId = otherId, Weight = 1.0 });
        var saved = await service.Notes.SaveExtendedAsync(note);

        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        await AssertNotModifiedAsync(noteId, "only its items changed");
        Assert.HasCount(1, saved.Entity.Resources);
        Assert.HasCount(1, saved.Entity.Tasks);
        Assert.HasCount(1, saved.Entity.Messages);
        Assert.HasCount(1, saved.Entity.TraceNotesTo);
        var stored = (await service.Notes.GetExtendedAsync(noteId)).Entity;
        stored.SetIsDirty(false);
        Assert.HasCount(1, stored.Resources);
        Assert.HasCount(1, stored.Tasks);
        Assert.HasCount(1, stored.Messages);
        Assert.HasCount(1, stored.TraceNotesTo);
        Assert.AreEqual(KntConst.Status[EnumStatus.AlarmsPending], stored.InternalTags, "its status is written");

        // Changed again (the task resolved, the alarm deleted): the same.
        stored.Tasks[0].EndDate = DateTime.Now;
        stored.Tasks[0].Resolved = true;
        stored.Messages[0].SetIsDeleted(true);
        Assert.IsTrue((await service.Notes.SaveExtendedAsync(stored)).IsValid);
        await AssertNotModifiedAsync(noteId, "its items changed again");
        Assert.AreEqual(KntConst.Status[EnumStatus.Resolved], (await service.Repository.Notes.GetAsync(noteId)).Entity.InternalTags);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task SavingTheExtendedNote_WithItsOwnDataChanged_ModifiesIt(string orm)
    {
        await OpenAsync(orm);
        var kattributeId = Guid.NewGuid();
        Assert.IsTrue((await service.Repository.KAttributes.AddAsync(new KAttributeDto
        {
            KAttributeId = kattributeId, Name = "Customer", KAttributeDataType = EnumKAttributeDataType.String
        })).IsValid);
        var topic = await AddNoteAsync("Topic");
        var attribute = await AddNoteAsync("Attribute");
        var mixed = await AddNoteAsync("Mixed");
        var user = await AddUserAsync();
        var before = DateTime.Now.AddSeconds(-1);

        var withTopic = (await service.Notes.GetExtendedAsync(topic)).Entity;
        withTopic.SetIsDirty(false);
        withTopic.Topic = "A new topic";
        Assert.IsTrue((await service.Notes.SaveExtendedAsync(withTopic)).IsValid);

        var withAttribute = (await service.Notes.GetExtendedAsync(attribute)).Entity;
        withAttribute.SetIsDirty(false);
        withAttribute.KAttributesDto.Single(a => a.KAttributeId == kattributeId).Value = "ACME";
        Assert.IsTrue((await service.Notes.SaveExtendedAsync(withAttribute)).IsValid);

        var withBoth = (await service.Notes.GetExtendedAsync(mixed)).Entity;
        withBoth.SetIsDirty(false);
        withBoth.Description = "A new text.";
        withBoth.Tasks.Add(new NoteTaskDto { NoteId = mixed, UserId = user.UserId, Description = "A task", CreationDateTime = DateTime.Now, ModificationDateTime = DateTime.Now });
        Assert.IsTrue((await service.Notes.SaveExtendedAsync(withBoth)).IsValid);

        foreach (var noteId in new[] { topic, attribute, mixed })
        {
            var stored = (await service.Repository.Notes.GetAsync(noteId)).Entity;
            Assert.IsGreaterThanOrEqualTo(before, stored.ModificationDateTime, stored.Topic);
        }
        Assert.AreEqual("ACME", (await service.Notes.GetAsync(attribute)).Entity.KAttributesDto.Single(a => a.KAttributeId == kattributeId).Value);
        Assert.HasCount(1, (await service.Notes.GetNoteTasksAsync(mixed)).Entity);
    }

    // The assistant saves the note after a task or an alarm, for its status: it keeps its date.
    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task TheAssistantsTasksAndAlarms_DoNotModifyTheNote(string orm)
    {
        await OpenAsync(orm);
        var noteId = await AddNoteAsync("Project");
        var user = await AddUserAsync();

        var task = new AiActionDto { Kind = AiActionRules.AddTask, NoteId = noteId, UserId = user.UserId, Content = "Buy the membrane", Text = "Task" };
        await KntAiActionExecutor.ExecuteAsync(service, task);
        var alarm = new AiActionDto { Kind = AiActionRules.AddAlarm, NoteId = noteId, Content = "Call the shop", Date = DateTime.Now.AddDays(1), Text = "Alarm" };
        await KntAiActionExecutor.ExecuteAsync(service, alarm);

        Assert.AreEqual(EnumAiActionStatus.Done, task.Status, task.Result);
        Assert.AreEqual(EnumAiActionStatus.Done, alarm.Status, alarm.Result);
        await AssertNotModifiedAsync(noteId, "the assistant's task and alarm");
        Assert.AreEqual(KntConst.Status[EnumStatus.AlarmsPending], (await service.Repository.Notes.GetAsync(noteId)).Entity.InternalTags, "its status is written");
    }

    // What the desktop editor gives back without anybody changing the note (its text with LF, empty boxes, the status it shows)
    // marks it as changed: the Service compares with what is stored, so it keeps its date.
    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task AnEditorThatMarksTheNoteWithoutChangingIt_DoesNotModifyIt(string orm)
    {
        await OpenAsync(orm);
        var noteId = await AddNoteAsync("Project", "Line one\r\nLine two\r\n");
        var user = await AddUserAsync();
        var note = (await service.Notes.GetExtendedAsync(noteId)).Entity;
        note.SetIsDirty(false);

        note.Description = "Line one\nLine two";
        note.Tags = "";
        note.Script = "";
        note.InternalTags = "(need to be updated)";
        note.Messages.Add(new KMessageDto { NoteId = noteId, UserId = user.UserId, Comment = "Call", AlarmActivated = true, AlarmDateTime = DateTime.Now,
            AlarmType = EnumAlarmType.Standard, NotificationType = EnumNotificationType.PostIt });
        Assert.IsTrue(note.IsDirty(false), "the editor marked the note");
        var saved = await service.Notes.SaveExtendedAsync(note);

        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        await AssertNotModifiedAsync(noteId, "nothing of the note changed");
        Assert.AreEqual(KntConst.Status[EnumStatus.AlarmsPending], (await service.Repository.Notes.GetAsync(noteId)).Entity.InternalTags, "its status is written");

        // The same through SaveAsync (the post-it, the web, the assistant).
        var plain = (await service.Notes.GetAsync(noteId)).Entity;
        plain.Topic = "Project";
        plain.SetIsDirty(true);
        Assert.IsTrue((await service.Notes.SaveAsync(plain)).IsValid);
        await AssertNotModifiedAsync(noteId, "saved as it was");
    }

    #region Helpers

    private async Task<UserDto> AddUserAsync()
    {
        var user = new UserDto { UserId = Guid.NewGuid(), UserName = "dates", EMail = "dates@knote.tests", FullName = "Dates", RoleDefinition = "Staff" };
        Assert.IsTrue((await service.Repository.Users.AddAsync(user)).IsValid);
        return user;
    }

    private async Task AssertNotModifiedAsync(Guid noteId, string what) =>
        Assert.AreEqual(NoteDate, (await service.Repository.Notes.GetAsync(noteId)).Entity.ModificationDateTime, what);

    private async Task<Guid> AddNoteAsync(string topic, string description = "")
    {
        var noteId = Guid.NewGuid();
        var note = await service.Repository.Notes.AddAsync(new NoteDto
        {
            NoteId = noteId,
            Topic = topic,
            Description = description,
            ContentType = "markdown",
            FolderId = folderId,
            CreationDateTime = NoteDate,
            ModificationDateTime = NoteDate
        });
        Assert.IsTrue(note.IsValid, note.ErrorMessage);
        return noteId;
    }

    private static byte[] Png()
    {
        using var bitmap = new SkiaSharp.SKBitmap(64, 48);
        bitmap.Erase(SkiaSharp.SKColors.Blue);
        using var image = SkiaSharp.SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    #endregion
}

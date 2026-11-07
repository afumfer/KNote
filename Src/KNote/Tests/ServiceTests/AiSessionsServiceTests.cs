using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// IKntAiSessionService: AI assistant sessions persisted as notes of the KntConst.ChatSessionsTag type, in the
/// KntConst.AiSessionsFolderName folder, linked to their user by a NoteTask. Run with authorization enforced
/// (as ClientWin does) and a Staff user, so the infrastructure the commands create on the fly (type, attributes,
/// folder - Admin/ProjectManager operations on their own) is shown to work for the assistant's minimum role.
/// </summary>
[TestClass]
public class AiSessionsServiceTests
{
    private readonly HashSet<string> _resourceFolders = new();

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task GetUserSessions_FirstTime_CreatesTheNoteTypeItsAttributesAndTheFolder_OnlyOnce(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var service = await StaffServiceAsync(db, orm, "staff1");

        var first = await service.AiSessions.GetUserSessionsAsync();
        var second = await service.AiSessions.GetUserSessionsAsync();

        Assert.IsTrue(first.IsValid, first.ErrorMessage);
        Assert.IsTrue(second.IsValid, second.ErrorMessage);
        Assert.AreEqual(0, first.Entity.Count);

        var noteTypes = (await service.Repository.NoteTypes.GetAllAsync()).Entity
            .Where(t => t.Name == KntConst.ChatSessionsTag).ToList();
        Assert.AreEqual(1, noteTypes.Count);

        var attributes = (await service.Repository.KAttributes.GetAllAsync(noteTypes[0].NoteTypeId)).Entity
            .Where(a => a.NoteTypeId == noteTypes[0].NoteTypeId).Select(a => a.Name).ToList();
        CollectionAssert.AreEquivalent(new[] { KntConst.AiProviderAttributeName, KntConst.AiModelAttributeName }, attributes);

        var folders = (await service.Repository.Folders.GetAllAsync()).Entity
            .Where(f => f.Name == KntConst.AiSessionsFolderName).ToList();
        Assert.AreEqual(1, folders.Count);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task Infrastructure_IsCreatedThroughTheDomainCommands_AndTheBypassEndsWithIt(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var service = await StaffServiceAsync(db, orm, "staff1");
        var succeeded = new List<string>();
        service.CommandExecuted += (_, e) =>
        {
            if (e.Outcome == CommandOutcome.Succeeded)
                succeeded.Add(e.CommandType.Name);
        };

        var res = await service.AiSessions.GetUserSessionsAsync();

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        Assert.AreEqual(1, succeeded.Count(n => n == "KntNoteTypeSaveAsyncCommand"));
        Assert.AreEqual(2, succeeded.Count(n => n == "KntKAttributesSaveAsyncCommand"));
        Assert.AreEqual(1, succeeded.Count(n => n == "KntFoldersSaveAsyncCommand"));

        // The bypass only covered that creation: the same Staff user still can't create a note type himself.
        var direct = await service.NoteTypes.SaveAsync(new NoteTypeDto { Name = $"type-{Guid.NewGuid():N}" });
        Assert.IsFalse(direct.IsValid);
        Assert.IsTrue(direct.NotAuthorized);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task Save_NewSession_CreatesTheNoteInTheFolder_WithItsAttributesAndATaskOfTheUser(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var service = await StaffServiceAsync(db, orm, "staff1");
        var user = await service.GetCurrentUserAsync();

        var saved = await service.AiSessions.SaveAsync(NewSession("What is KNote?\r\nAnswer briefly.", "A notes manager."));

        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        Assert.AreNotEqual(Guid.Empty, saved.Entity.NoteId);
        Assert.IsTrue(saved.Entity.NoteNumber > 0);
        Assert.AreEqual("What is KNote?", saved.Entity.Topic, "Topic: the first line of the first prompt.");

        var note = (await service.Repository.Notes.GetAsync(saved.Entity.NoteId)).Entity;
        var noteType = (await service.Repository.NoteTypes.GetAllAsync()).Entity.Single(t => t.Name == KntConst.ChatSessionsTag);
        var folder = (await service.Repository.Folders.GetAllAsync()).Entity.Single(f => f.Name == KntConst.AiSessionsFolderName);
        Assert.AreEqual(noteType.NoteTypeId, note.NoteTypeId);
        Assert.AreEqual(folder.FolderId, note.FolderId);
        Assert.AreEqual("OpenAI", note.KAttributesDto.Single(a => a.Name == KntConst.AiProviderAttributeName).Value);
        Assert.AreEqual("gpt-test", note.KAttributesDto.Single(a => a.Name == KntConst.AiModelAttributeName).Value);

        var tasks = (await service.Repository.Notes.GetNoteTasksAsync(note.NoteId)).Entity;
        Assert.AreEqual(1, tasks.Count);
        Assert.AreEqual(user.UserId, tasks[0].UserId);
        Assert.IsNotNull(tasks[0].StartDate, "The task starts when the session is created.");
        Assert.IsFalse(tasks[0].Resolved);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task Save_ThenGet_RoundTripsTheTurnsProviderAndModel(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var service = await StaffServiceAsync(db, orm, "staff1");

        var session = NewSession("First question", "First answer\r\nwith two lines");
        session.Turns[0].InputTokens = 10;
        session.Turns[0].OutputTokens = 20;
        session.Turns[0].TotalTokens = 30;
        session.Turns[0].ProcessingTime = TimeSpan.FromSeconds(1.5);
        session.Turns.Add(new AiChatTurnDto { Prompt = "Second question", Answer = "Cut answer", TotalTokens = 7, TokensEstimated = true, Truncated = true });
        var saved = await service.AiSessions.SaveAsync(session);
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);

        var loaded = await service.AiSessions.GetAsync(saved.Entity.NoteId);

        Assert.IsTrue(loaded.IsValid, loaded.ErrorMessage);
        Assert.AreEqual("OpenAI", loaded.Entity.Provider);
        Assert.AreEqual("gpt-test", loaded.Entity.Model);
        Assert.AreEqual(2, loaded.Entity.Turns.Count);
        Assert.AreEqual("First answer\r\nwith two lines", loaded.Entity.Turns[0].Answer);
        Assert.AreEqual(30, loaded.Entity.Turns[0].TotalTokens);
        Assert.AreEqual(TimeSpan.FromSeconds(1.5), loaded.Entity.Turns[0].ProcessingTime);
        Assert.IsTrue(loaded.Entity.Turns[1].Truncated);
        Assert.IsTrue(loaded.Entity.Turns[1].TokensEstimated);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task Save_ExistingSession_UpdatesItsTranscriptAndAttributes_KeepingOneTask(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var service = await StaffServiceAsync(db, orm, "staff1");
        var saved = (await service.AiSessions.SaveAsync(NewSession("Question", "Answer"))).Entity;

        saved.Turns.Add(new AiChatTurnDto { Prompt = "Another question", Answer = "Another answer" });
        saved.Model = "gpt-other";
        var updated = await service.AiSessions.SaveAsync(saved);

        Assert.IsTrue(updated.IsValid, updated.ErrorMessage);
        Assert.AreEqual(saved.NoteId, updated.Entity.NoteId);
        var loaded = (await service.AiSessions.GetAsync(saved.NoteId)).Entity;
        Assert.AreEqual(2, loaded.Turns.Count);
        Assert.AreEqual("gpt-other", loaded.Model);
        Assert.AreEqual(1, (await service.Repository.Notes.GetNoteTasksAsync(saved.NoteId)).Entity.Count);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task GetUserSessions_ReturnsOnlyTheUsersSessions_MostRecentlyModifiedFirst(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var serviceA = await StaffServiceAsync(db, orm, "staffA");
        var serviceB = await StaffServiceAsync(db, orm, "staffB");

        var older = (await serviceA.AiSessions.SaveAsync(NewSession("Older", "1"))).Entity;
        await Task.Delay(50);
        var newer = (await serviceA.AiSessions.SaveAsync(NewSession("Newer", "2"))).Entity;
        await Task.Delay(50);
        await serviceB.AiSessions.SaveAsync(NewSession("Of B", "3"));
        await Task.Delay(50);
        // Continuing the older session makes it the most recent one.
        older.Turns.Add(new AiChatTurnDto { Prompt = "More", Answer = "4" });
        Assert.IsTrue((await serviceA.AiSessions.SaveAsync(older)).IsValid);

        var sessions = await serviceA.AiSessions.GetUserSessionsAsync();

        Assert.IsTrue(sessions.IsValid, sessions.ErrorMessage);
        CollectionAssert.AreEqual(new[] { older.NoteId, newer.NoteId }, sessions.Entity.Select(s => s.NoteId).ToList());
    }

    private static AiChatAttachmentUploadDto Upload(string name, Guid sessionNoteId = default, byte[] content = null) => new()
    {
        SessionNoteId = sessionNoteId,
        Provider = EnumAiProvider.Ollama,
        Model = "qwen",
        Topic = "What is in the photo?\r\nBe brief",
        FileName = name,
        ContentType = AiAttachmentRules.ContentTypeOf(name),
        ContentBase64 = Convert.ToBase64String(content ?? new byte[] { 1, 2, 3 })
    };

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task AddAttachment_WithoutASession_CreatesItsNote_AndTheNextOneGoesInTheSameNote(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var service = await StaffWithResourcesAsync(db, orm, "staff1");

        var first = await service.AiSessions.AddAttachmentAsync(Upload("photo.jpg"));
        Assert.IsTrue(first.IsValid, first.ErrorMessage);
        var noteId = first.Entity.SessionNoteId;
        var second = await service.AiSessions.AddAttachmentAsync(Upload("manual.pdf", noteId));

        Assert.IsTrue(second.IsValid, second.ErrorMessage);
        Assert.AreEqual(noteId, second.Entity.SessionNoteId);
        Assert.AreEqual("photo.jpg", first.Entity.FileName);
        Assert.AreEqual("image/jpeg", first.Entity.ContentType);
        var session = (await service.AiSessions.GetUserSessionsAsync()).Entity.Single();
        Assert.AreEqual(noteId, session.NoteId);
        Assert.AreEqual("What is in the photo?", session.Topic);
        Assert.AreEqual(2, (await service.Notes.GetResourcesAsync(noteId)).Entity.Count);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task AnAttachment_IsReadWithItsContent_AndSurvivesSavingTheSession(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var service = await StaffWithResourcesAsync(db, orm, "staff1");
        var attachment = (await service.AiSessions.AddAttachmentAsync(Upload("photo.jpg", content: new byte[] { 9, 8, 7 }))).Entity;

        var session = NewSession("Question", "Answer");
        session.NoteId = attachment.SessionNoteId;
        session.Turns[0].Attachments.Add(attachment);
        var saved = await service.AiSessions.SaveAsync(session);
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);

        var file = await service.AiSessions.GetAttachmentAsync(attachment.SessionNoteId, attachment.ResourceId);

        Assert.IsTrue(file.IsValid, file.ErrorMessage);
        CollectionAssert.AreEqual(new byte[] { 9, 8, 7 }, Convert.FromBase64String(file.Entity.ContentBase64));
        Assert.AreEqual("image/jpeg", file.Entity.ContentType);
        var read = (await service.AiSessions.GetAsync(attachment.SessionNoteId)).Entity;
        Assert.AreEqual(attachment.ResourceId, read.Turns.Single().Attachments.Single().ResourceId);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task AnAttachment_OfAnotherUsersSession_CannotBeReadNorAdded(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var owner = await StaffWithResourcesAsync(db, orm, "owner");
        var other = await StaffWithResourcesAsync(db, orm, "other");
        var attachment = (await owner.AiSessions.AddAttachmentAsync(Upload("private.jpg"))).Entity;

        var read = await other.AiSessions.GetAttachmentAsync(attachment.SessionNoteId, attachment.ResourceId);
        var add = await other.AiSessions.AddAttachmentAsync(Upload("mine.jpg", attachment.SessionNoteId));

        Assert.IsFalse(read.IsValid);
        StringAssert.Contains(read.ErrorMessage, "another user");
        Assert.IsFalse(add.IsValid);
        Assert.AreEqual(1, (await owner.Notes.GetResourcesAsync(attachment.SessionNoteId)).Entity.Count);
    }

    [TestMethod]
    public async Task GetAttachment_OnlyGivesTheResourcesOfThatSession()
    {
        using var db = new RepositoryTestDatabase();
        var service = await StaffWithResourcesAsync(db, "Dapper", "staff1");
        var mine = (await service.AiSessions.AddAttachmentAsync(Upload("a.jpg"))).Entity;
        var other = (await service.AiSessions.AddAttachmentAsync(Upload("b.jpg"))).Entity;

        var res = await service.AiSessions.GetAttachmentAsync(mine.SessionNoteId, other.ResourceId);

        Assert.AreNotEqual(mine.SessionNoteId, other.SessionNoteId);
        Assert.IsFalse(res.IsValid);
        StringAssert.Contains(res.ErrorMessage, "does not exist");
    }

    [TestMethod]
    public async Task AddAttachment_RefusesAnEmptyFile_OneTooBig_AndCreatesNoSession()
    {
        using var db = new RepositoryTestDatabase();
        var service = await StaffWithResourcesAsync(db, "Dapper", "staff1");

        var empty = await service.AiSessions.AddAttachmentAsync(Upload("a.jpg", content: Array.Empty<byte>()));
        var big = await service.AiSessions.AddAttachmentAsync(Upload("a.jpg", content: new byte[KntConst.MaxLenResourceFile + 1]));
        var notBase64 = Upload("a.jpg");
        notBase64.ContentBase64 = "not base64!";
        var broken = await service.AiSessions.AddAttachmentAsync(notBase64);

        Assert.IsFalse(empty.IsValid);
        Assert.IsFalse(big.IsValid);
        Assert.IsFalse(broken.IsValid);
        Assert.AreEqual(0, (await service.AiSessions.GetUserSessionsAsync()).Entity.Count);
    }

    [TestMethod]
    public async Task AddAttachment_KeepsNoFoldersInTheNameOfTheFile()
    {
        using var db = new RepositoryTestDatabase();
        var service = await StaffWithResourcesAsync(db, "Dapper", "staff1");

        var res = await service.AiSessions.AddAttachmentAsync(Upload("../../evil.png"));

        Assert.AreEqual("evil.png", res.Entity.FileName);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task AnotherUsersSession_CannotBeReadNorSaved(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var owner = await StaffServiceAsync(db, orm, "owner");
        var other = await StaffServiceAsync(db, orm, "other");
        var saved = (await owner.AiSessions.SaveAsync(NewSession("Private", "Answer"))).Entity;

        var read = await other.AiSessions.GetAsync(saved.NoteId);
        saved.Turns.Add(new AiChatTurnDto { Prompt = "Hijack", Answer = "No" });
        var write = await other.AiSessions.SaveAsync(saved);

        Assert.IsFalse(read.IsValid);
        Assert.IsFalse(write.IsValid);
        StringAssert.Contains(read.ErrorMessage, "another user");
        Assert.AreEqual(1, (await owner.AiSessions.GetAsync(saved.NoteId)).Entity.Turns.Count);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task Get_ANoteThatIsNotASession_IsRefused(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var service = await StaffServiceAsync(db, orm, "staff1");
        var anyNote = (await service.Repository.Notes.GetAllMinimalAsync()).Entity.First();

        var res = await service.AiSessions.GetAsync(anyNote.NoteId);

        Assert.IsFalse(res.IsValid);
        StringAssert.Contains(res.ErrorMessage, "does not exist");
    }

    [TestMethod]
    public async Task Save_WithoutTurnsOrModel_IsRejected()
    {
        using var db = new RepositoryTestDatabase();
        var service = await StaffServiceAsync(db, "Dapper", "staff1");

        var noTurns = await service.AiSessions.SaveAsync(new AiChatSessionDto { Provider = "OpenAI", Model = "gpt-test" });
        var noModel = NewSession("Question", "Answer");
        noModel.Model = "";
        var noModelRes = await service.AiSessions.SaveAsync(noModel);

        Assert.IsFalse(noTurns.IsValid);
        Assert.IsFalse(noModelRes.IsValid);
    }

    [TestMethod]
    public async Task AGuest_IsRefused()
    {
        using var db = new RepositoryTestDatabase();
        var service = new KntService(db.CreateRepository("Dapper")) { UserIdentityName = "guest1", EnforceAuthorization = true };
        await AddUserAsync(service, "guest1", nameof(EnumRoles.Guest));

        var res = await service.AiSessions.GetUserSessionsAsync();

        Assert.IsFalse(res.IsValid);
        Assert.IsTrue(res.NotAuthorized);
    }

    private static AiChatSessionDto NewSession(string prompt, string answer) => new()
    {
        Provider = "OpenAI",
        Model = "gpt-test",
        Turns = { new AiChatTurnDto { Prompt = prompt, Answer = answer } }
    };

    // The files of the attachments, in a folder of this database: every user of it has to find them.
    private async Task<KntService> StaffWithResourcesAsync(RepositoryTestDatabase db, string orm, string userName)
    {
        var service = await StaffServiceAsync(db, orm, userName);
        var root = Path.ChangeExtension(db.DatabaseFilePath, null) + "-resources";
        Directory.CreateDirectory(root);
        _resourceFolders.Add(root);
        service.Repository.RepositoryRef.ResourcesContainer = "AiSessionTestResources";
        service.Repository.RepositoryRef.ResourcesContainerRootPath = root;
        return service;
    }

    [TestCleanup]
    public void DeleteResourceFolders()
    {
        foreach (var folder in _resourceFolders.Where(Directory.Exists))
            Directory.Delete(folder, recursive: true);
        _resourceFolders.Clear();
    }

    private static async Task<KntService> StaffServiceAsync(RepositoryTestDatabase db, string orm, string userName)
    {
        var service = new KntService(db.CreateRepository(orm)) { UserIdentityName = userName, EnforceAuthorization = true };
        await AddUserAsync(service, userName, nameof(EnumRoles.Staff));
        return service;
    }

    // Straight through the repository: commands would need an Admin to create users.
    private static async Task AddUserAsync(KntService service, string userName, string roleDefinition)
    {
        var res = await service.Repository.Users.AddAsync(new UserDto
        {
            UserId = Guid.NewGuid(),
            UserName = userName,
            EMail = $"{userName}-{Guid.NewGuid():N}@knote.tests",
            FullName = userName,
            RoleDefinition = roleDefinition
        });
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
    }
}

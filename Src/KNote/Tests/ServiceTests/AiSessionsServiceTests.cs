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

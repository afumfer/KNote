using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// IKntChatRoomService: chat room threads persisted as notes of the KntConst.ChatRoomTag type, in the
/// KntConst.ChatRoomFolderName folder, with a NoteTask per participant. Run with authorization enforced (as
/// ClientWin does) and Staff users, so the infrastructure the commands create on the fly (type and folder -
/// Admin/ProjectManager operations on their own) is shown to work for the chat room's minimum role.
/// </summary>
[TestClass]
public class ChatRoomsServiceTests
{
    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task GetThreads_FirstTime_CreatesTheNoteTypeAndTheFolder_OnlyOnce(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var service = await StaffServiceAsync(db, orm, "staff1");

        var first = await service.ChatRooms.GetThreadsAsync();
        var second = await service.ChatRooms.GetThreadsAsync();

        Assert.IsTrue(first.IsValid, first.ErrorMessage);
        Assert.IsTrue(second.IsValid, second.ErrorMessage);
        Assert.AreEqual(0, first.Entity.Count);
        Assert.AreEqual(1, (await service.Repository.NoteTypes.GetAllAsync()).Entity.Count(t => t.Name == KntConst.ChatRoomTag));
        Assert.AreEqual(1, (await service.Repository.Folders.GetAllAsync()).Entity.Count(f => f.Name == KntConst.ChatRoomFolderName));

        // The bypass only covered that creation: the same Staff user still can't create a note type himself.
        var direct = await service.NoteTypes.SaveAsync(new NoteTypeDto { Name = $"type-{Guid.NewGuid():N}" });
        Assert.IsTrue(direct.NotAuthorized);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task Create_SavesTheNoteInTheFolder_WithATaskOfTheCreator(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var service = await StaffServiceAsync(db, orm, "staff1");
        var user = await service.GetCurrentUserAsync();

        var created = await service.ChatRooms.CreateAsync("  Release planning  ");

        Assert.IsTrue(created.IsValid, created.ErrorMessage);
        Assert.AreNotEqual(Guid.Empty, created.Entity.ThreadId);
        Assert.IsTrue(created.Entity.NoteNumber > 0);
        Assert.AreEqual("Release planning", created.Entity.Topic);
        Assert.IsTrue(created.Entity.IsParticipant);

        var note = (await service.Repository.Notes.GetAsync(created.Entity.ThreadId)).Entity;
        var noteType = (await service.Repository.NoteTypes.GetAllAsync()).Entity.Single(t => t.Name == KntConst.ChatRoomTag);
        var folder = (await service.Repository.Folders.GetAllAsync()).Entity.Single(f => f.Name == KntConst.ChatRoomFolderName);
        Assert.AreEqual(noteType.NoteTypeId, note.NoteTypeId);
        Assert.AreEqual(folder.FolderId, note.FolderId);

        var tasks = (await service.Repository.Notes.GetNoteTasksAsync(note.NoteId)).Entity;
        Assert.AreEqual(1, tasks.Count);
        Assert.AreEqual(user.UserId, tasks[0].UserId);
        Assert.IsNotNull(tasks[0].StartDate);
    }

    [TestMethod]
    public async Task Create_WithoutTopic_OrWithATooLongOne_IsRefused()
    {
        using var db = new RepositoryTestDatabase();
        var service = await StaffServiceAsync(db, "Dapper", "staff1");

        Assert.IsFalse((await service.ChatRooms.CreateAsync("  ")).IsValid);
        Assert.IsFalse((await service.ChatRooms.CreateAsync(new string('x', ChatRoomThreadInfoDto.MaxTopicLength + 1))).IsValid);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task AddMessage_FromSeveralUsers_ThenGet_ReturnsTheMessagesInOrderAndTheParticipants(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var serviceA = await StaffServiceAsync(db, orm, "staffA");
        var serviceB = await StaffServiceAsync(db, orm, "staffB");
        var thread = (await serviceA.ChatRooms.CreateAsync("Thread")).Entity;

        var first = await serviceA.ChatRooms.AddMessageAsync(new ChatRoomMessageDto { ThreadId = thread.ThreadId, Text = "Hello\nB" });
        var second = await serviceB.ChatRooms.AddMessageAsync(new ChatRoomMessageDto { ThreadId = thread.ThreadId, Text = "Hi **A**" });

        Assert.IsTrue(first.IsValid, first.ErrorMessage);
        Assert.IsTrue(second.IsValid, second.ErrorMessage);
        Assert.AreNotEqual(Guid.Empty, first.Entity.MessageId);
        Assert.AreEqual("staffA", first.Entity.UserName, "The user is the one of the service, not one given by the caller.");
        Assert.AreEqual("Hello\r\nB", first.Entity.Text);

        var loaded = await serviceB.ChatRooms.GetAsync(thread.ThreadId);
        Assert.IsTrue(loaded.IsValid, loaded.ErrorMessage);
        Assert.AreEqual(2, loaded.Entity.Messages.Count);
        Assert.AreEqual(first.Entity.MessageId, loaded.Entity.Messages[0].MessageId);
        Assert.AreEqual("Hello\r\nB", loaded.Entity.Messages[0].Text);
        Assert.AreEqual("staffB", loaded.Entity.Messages[1].UserName);
        Assert.AreEqual("Hi **A**", loaded.Entity.Messages[1].Text);
        Assert.IsTrue(loaded.Entity.Messages.All(m => m.ThreadId == thread.ThreadId));
        CollectionAssert.AreEqual(new[] { "staffA", "staffB" }, loaded.Entity.Participants);
        Assert.IsTrue(loaded.Entity.IsParticipant, "Writing in a thread joins it.");

        // The note is readable as any other: its description is the conversation.
        var note = (await serviceA.Repository.Notes.GetAsync(thread.ThreadId)).Entity;
        StringAssert.Contains(note.Description, "**staffB**");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task GetThreads_ListsEveryThread_MostRecentActivityFirst_AndWhichOnesTheUserTakesPartIn(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var serviceA = await StaffServiceAsync(db, orm, "staffA");
        var serviceB = await StaffServiceAsync(db, orm, "staffB");
        var older = (await serviceA.ChatRooms.CreateAsync("Older")).Entity;
        await Task.Delay(50);
        var ofB = (await serviceB.ChatRooms.CreateAsync("Of B")).Entity;
        await Task.Delay(50);
        Assert.IsTrue((await serviceA.ChatRooms.AddMessageAsync(new ChatRoomMessageDto { ThreadId = older.ThreadId, Text = "Up" })).IsValid);

        var threads = await serviceA.ChatRooms.GetThreadsAsync();

        Assert.IsTrue(threads.IsValid, threads.ErrorMessage);
        CollectionAssert.AreEqual(new[] { older.ThreadId, ofB.ThreadId }, threads.Entity.Select(t => t.ThreadId).ToList());
        Assert.IsTrue(threads.Entity[0].IsParticipant);
        Assert.IsFalse(threads.Entity[1].IsParticipant);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task Join_AddsTheUserOnce(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var serviceA = await StaffServiceAsync(db, orm, "staffA");
        var serviceB = await StaffServiceAsync(db, orm, "staffB");
        var thread = (await serviceA.ChatRooms.CreateAsync("Thread")).Entity;

        var first = await serviceB.ChatRooms.JoinAsync(thread.ThreadId);
        var second = await serviceB.ChatRooms.JoinAsync(thread.ThreadId);

        Assert.IsTrue(first.IsValid, first.ErrorMessage);
        Assert.IsTrue(second.IsValid, second.ErrorMessage);
        Assert.IsTrue(first.Entity.IsParticipant);
        Assert.AreEqual(2, (await serviceA.Repository.Notes.GetNoteTasksAsync(thread.ThreadId)).Entity.Count);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task GetGeneral_CreatesTheGeneralThreadOnce(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var serviceA = await StaffServiceAsync(db, orm, "staffA");
        var serviceB = await StaffServiceAsync(db, orm, "staffB");

        var created = await serviceA.ChatRooms.GetGeneralAsync();
        var found = await serviceB.ChatRooms.GetGeneralAsync();

        Assert.IsTrue(created.IsValid, created.ErrorMessage);
        Assert.IsTrue(found.IsValid, found.ErrorMessage);
        Assert.AreEqual(KntConst.ChatRoomGeneralThreadTopic, created.Entity.Topic);
        Assert.AreEqual(created.Entity.ThreadId, found.Entity.ThreadId);
        Assert.IsTrue(created.Entity.IsParticipant);
        Assert.IsFalse(found.Entity.IsParticipant, "Finding the general thread doesn't join it.");
        Assert.AreEqual(1, (await serviceA.ChatRooms.GetThreadsAsync()).Entity.Count);
    }

    [TestMethod]
    public async Task AnotherNote_IsNotAThread()
    {
        using var db = new RepositoryTestDatabase();
        var service = await StaffServiceAsync(db, "Dapper", "staff1");
        var anyNote = (await service.Repository.Notes.GetAllMinimalAsync()).Entity.First();

        var get = await service.ChatRooms.GetAsync(anyNote.NoteId);
        Assert.IsFalse(get.IsValid);
        StringAssert.Contains(get.ErrorMessage, "does not exist");
        Assert.IsFalse((await service.ChatRooms.JoinAsync(anyNote.NoteId)).IsValid);
        Assert.IsFalse((await service.ChatRooms.AddMessageAsync(new ChatRoomMessageDto { ThreadId = anyNote.NoteId, Text = "x" })).IsValid);    }

    [TestMethod]
    public async Task AddMessage_EmptyOrTooLong_IsRefused()
    {
        using var db = new RepositoryTestDatabase();
        var service = await StaffServiceAsync(db, "Dapper", "staff1");
        var thread = (await service.ChatRooms.CreateAsync("Thread")).Entity;

        Assert.IsFalse((await service.ChatRooms.AddMessageAsync(new ChatRoomMessageDto { ThreadId = thread.ThreadId, Text = " \r\n" })).IsValid);
        Assert.IsFalse((await service.ChatRooms.AddMessageAsync(new ChatRoomMessageDto
        {
            ThreadId = thread.ThreadId,
            Text = new string('x', ChatRoomMessageDto.MaxTextLength + 1)
        })).IsValid);
        Assert.IsFalse((await service.ChatRooms.AddMessageAsync(new ChatRoomMessageDto { Text = "No thread" })).IsValid);
    }

    [TestMethod]
    public async Task AGuest_IsRefused()
    {
        using var db = new RepositoryTestDatabase();
        var service = new KntService(db.CreateRepository("Dapper")) { UserIdentityName = "guest1", EnforceAuthorization = true };
        await AddUserAsync(service, "guest1", nameof(EnumRoles.Guest));

        var res = await service.ChatRooms.GetThreadsAsync();

        Assert.IsFalse(res.IsValid);
        Assert.IsTrue(res.NotAuthorized);
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

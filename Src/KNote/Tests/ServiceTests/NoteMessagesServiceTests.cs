using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Service.ServicesCommands;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// The alarms of a note in the Service: a new one (Notes.NewMessageAsync) comes with its default values, for the user
/// running the service, and is only saved by SaveMessageAsync.
/// </summary>
[TestClass]
public class NoteMessagesServiceTests
{
    private readonly List<RepositoryTestDatabase> _databases = new();

    [TestCleanup]
    public void Cleanup() => _databases.ForEach(db => db.Dispose());

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task NewMessage_ForTheCurrentUser_WithTheDefaultValues(string orm)
    {
        var service = CreateService(orm);
        var user = (await service.Users.CreateAsync(NewUser())).Entity;
        service.UserIdentityName = user.UserName;
        var noteId = Guid.NewGuid();
        var before = DateTime.Now.AddMinutes(-1);

        var res = await service.Notes.NewMessageAsync(noteId);

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        var message = res.Entity;
        Assert.AreEqual(Guid.Empty, message.KMessageId, "Not saved: SaveMessageAsync gives it its id.");
        Assert.IsTrue(message.IsNew());
        Assert.AreEqual(noteId, message.NoteId);
        Assert.AreEqual(user.UserId, message.UserId);
        Assert.AreEqual(user.FullName, message.UserFullName);
        Assert.IsTrue(message.AlarmActivated);
        Assert.IsTrue(message.AlarmDateTime >= before && message.AlarmDateTime <= DateTime.Now);
        Assert.AreEqual(0, message.AlarmDateTime!.Value.Second, "To the minute.");
        Assert.AreEqual(EnumAlarmType.Standard, message.AlarmType);
        Assert.AreEqual(EnumNotificationType.AppInfo, message.NotificationType);
        Assert.AreEqual(KntNotesNewMessageAsyncCommand.DefaultComment, message.Comment);
        Assert.IsTrue(message.IsValid(), message.GetErrorMessage());
        Assert.IsEmpty((await service.Notes.GetMessagesAsync(noteId)).Entity, "Nothing is saved.");
    }

    [TestMethod]
    public async Task NewMessage_WithoutARegisteredUser_HasNoUser()
    {
        var service = CreateService("Dapper");
        service.UserIdentityName = "nobody-registered";

        var res = await service.Notes.NewMessageAsync(Guid.NewGuid());

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        Assert.IsNull(res.Entity.UserId);
    }

    [TestMethod]
    public async Task NewMessage_WithAuthorizationEnforced_IsForStaff()
    {
        using var db = new RepositoryTestDatabase();
        var setup = new KntService(db.CreateRepository("Dapper"), activateMessageBroker: false);
        var guest = NewUser();
        guest.RoleDefinition = nameof(EnumRoles.Guest);
        Assert.IsTrue((await setup.Users.CreateAsync(guest)).IsValid);
        var service = new KntService(db.CreateRepository("Dapper"), activateMessageBroker: false) { UserIdentityName = guest.UserName, EnforceAuthorization = true };

        var res = await service.Notes.NewMessageAsync(Guid.NewGuid());

        Assert.IsFalse(res.IsValid);
        Assert.IsTrue(res.NotAuthorized);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow(0)]
    public void AnAlarmEveryFewMinutes_WithoutMinutes_IsNotValid(int? minutes)
    {
        var alarm = new KMessageDto { AlarmDateTime = DateTime.Now, AlarmType = EnumAlarmType.InMinutes, AlarmMinutes = minutes };

        Assert.IsFalse(alarm.IsValid());
        StringAssert.Contains(alarm.GetErrorMessage(), "minutes");
    }

    private KntService CreateService(string orm)
    {
        var db = new RepositoryTestDatabase();
        _databases.Add(db);
        return new KntService(db.CreateRepository(orm), activateMessageBroker: false);
    }

    private static UserRegisterDto NewUser()
    {
        var id = Guid.NewGuid();
        return new UserRegisterDto
        {
            UserName = $"msg-{id:N}"[..24],
            EMail = $"{id:N}@knote.tests",
            FullName = "Alarms test user",
            RoleDefinition = nameof(EnumRoles.Staff),
            Password = "Alarms-Password-1!"
        };
    }
}

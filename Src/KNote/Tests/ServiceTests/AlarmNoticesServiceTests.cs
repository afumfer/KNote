using KNote.Model;
using KNote.Model.Dto;
using KNote.Repository;
using KNote.Service.Core;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// The alarms that become due, in the Service: consumed by message (Notes.ConsumeDueAlarmsAsync, of the user running the
/// service; ConsumeAllUsersDueAlarmsAsync, of every user), each one rescheduled or deactivated and returned once; the
/// Application info notices of the alarms a panel keeps (Notes.GetAlarmNoticesAsync); and the email of an Email alarm,
/// the same for ClientWin and the Server (KntAlarmEmail).
/// </summary>
[TestClass]
public class AlarmNoticesServiceTests
{
    private readonly List<RepositoryTestDatabase> _databases = new();

    [TestCleanup]
    public void Cleanup() => _databases.ForEach(db => db.Dispose());

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ConsumeDueAlarms_TheDueOnesOfTheUserAndType_Once(string orm)
    {
        var (service, db) = await CreateAsync(orm);
        var (user, other) = (await NewUserAsync(service), await NewUserAsync(service));
        service.UserIdentityName = user.UserName;
        var noteId = await NewNoteAsync(service);
        var due = await AddAlarmAsync(service, noteId, user.UserId, EnumNotificationType.AppInfo, DateTime.Now.AddMinutes(-5));
        var daily = await AddAlarmAsync(service, noteId, user.UserId, EnumNotificationType.AppInfo, DateTime.Now.AddDays(-1).AddMinutes(-1), EnumAlarmType.Daily);
        var notYet = await AddAlarmAsync(service, noteId, user.UserId, EnumNotificationType.AppInfo, DateTime.Now.AddHours(1));
        var otherType = await AddAlarmAsync(service, noteId, user.UserId, EnumNotificationType.Email, DateTime.Now.AddMinutes(-5));
        var otherUser = await AddAlarmAsync(service, noteId, other.UserId, EnumNotificationType.AppInfo, DateTime.Now.AddMinutes(-5));

        var consumed = await service.Notes.ConsumeDueAlarmsAsync(EnumNotificationType.AppInfo);

        Assert.IsTrue(consumed.IsValid, consumed.ErrorMessage);
        CollectionAssert.AreEquivalent(new[] { due.KMessageId, daily.KMessageId }, consumed.Entity.Select(m => m.KMessageId).ToArray());
        Assert.IsTrue(consumed.Entity.All(m => m.AlarmDateTime <= DateTime.Now && m.AlarmActivated == true), "Returned as they were when due.");
        Assert.AreEqual(user.FullName, consumed.Entity.First().UserFullName);
        Assert.IsFalse((await service.Notes.GetMessageAsync(due.KMessageId)).Entity.AlarmActivated, "Once: deactivated.");
        Assert.IsTrue((await service.Notes.GetMessageAsync(daily.KMessageId)).Entity.AlarmDateTime > DateTime.Now, "Daily: the next day.");
        foreach (var untouched in new[] { notYet, otherType, otherUser })
            Assert.IsTrue((await service.Notes.GetMessageAsync(untouched.KMessageId)).Entity.AlarmActivated);

        Assert.IsEmpty((await service.Notes.ConsumeDueAlarmsAsync(EnumNotificationType.AppInfo)).Entity, "Consumed once.");
    }

    [TestMethod]
    [DataRow("Dapper", "EntityFramework")]
    [DataRow("EntityFramework", "Dapper")]
    public async Task ConsumeDueAlarms_ByTwoConsumersAtOnce_EachAlarmOnce(string orm, string otherOrm)
    {
        var (service, db) = await CreateAsync(orm);
        var user = await NewUserAsync(service);
        var noteId = await NewNoteAsync(service);
        var alarms = new List<Guid>();
        for (var i = 0; i < 20; i++)
            alarms.Add((await AddAlarmAsync(service, noteId, user.UserId, EnumNotificationType.AppInfo, DateTime.Now.AddMinutes(-i - 1))).KMessageId);
        using var one = new KntService(db.CreateRepository(orm)) { UserIdentityName = user.UserName };
        using var two = new KntService(db.CreateRepository(otherOrm)) { UserIdentityName = user.UserName };

        var results = await Task.WhenAll(one.Notes.ConsumeDueAlarmsAsync(EnumNotificationType.AppInfo), two.Notes.ConsumeDueAlarmsAsync(EnumNotificationType.AppInfo));

        var returned = results.SelectMany(r => r.Entity).Select(m => m.KMessageId).ToList();
        CollectionAssert.AreEquivalent(alarms, returned, "Each one returned, and only once.");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ConsumeAllUsersDueAlarms_TheDueOnesOfEveryUser(string orm)
    {
        var (service, _) = await CreateAsync(orm);
        var (user, other) = (await NewUserAsync(service), await NewUserAsync(service));
        var noteId = await NewNoteAsync(service);
        var mine = await AddAlarmAsync(service, noteId, user.UserId, EnumNotificationType.Email, DateTime.Now.AddMinutes(-5));
        var theirs = await AddAlarmAsync(service, noteId, other.UserId, EnumNotificationType.Email, DateTime.Now.AddMinutes(-5));
        await AddAlarmAsync(service, noteId, other.UserId, EnumNotificationType.AppInfo, DateTime.Now.AddMinutes(-5));

        var consumed = await service.Notes.ConsumeAllUsersDueAlarmsAsync(EnumNotificationType.Email);

        Assert.IsTrue(consumed.IsValid, consumed.ErrorMessage);
        CollectionAssert.AreEquivalent(new[] { mine.KMessageId, theirs.KMessageId }, consumed.Entity.Select(m => m.KMessageId).ToArray());
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task GetAlarmNotices_OnlyTheOnesStillNoticesOfTheUser_ReadNow(string orm)
    {
        var (service, _) = await CreateAsync(orm);
        var (user, other) = (await NewUserAsync(service), await NewUserAsync(service));
        service.UserIdentityName = user.UserName;
        var noteId = await NewNoteAsync(service, "Notice note");
        var notice = await AddAlarmAsync(service, noteId, user.UserId, EnumNotificationType.AppInfo, DateTime.Now, comment: "Call");
        var deleted = await AddAlarmAsync(service, noteId, user.UserId, EnumNotificationType.AppInfo, DateTime.Now);
        var ofOther = await AddAlarmAsync(service, noteId, other.UserId, EnumNotificationType.AppInfo, DateTime.Now);
        var email = await AddAlarmAsync(service, noteId, user.UserId, EnumNotificationType.Email, DateTime.Now);
        Assert.IsTrue((await service.Notes.DeleteMessageAsync(deleted.KMessageId)).IsValid);

        var notices = await service.Notes.GetAlarmNoticesAsync(new List<Guid> { notice.KMessageId, deleted.KMessageId, ofOther.KMessageId, email.KMessageId, Guid.NewGuid() });

        Assert.IsTrue(notices.IsValid, notices.ErrorMessage);
        var only = notices.Entity.Single();
        Assert.AreEqual(notice.KMessageId, only.KMessageId);
        Assert.AreEqual(noteId, only.NoteId);
        Assert.AreEqual("Notice note", only.NoteTopic);
        Assert.AreEqual("Call", only.Comment);
    }

    [TestMethod]
    public void AlarmEmail_TheNotesTopic_AndTheCommentBeforeTheDescription()
    {
        var (subject, body) = KntAlarmEmail.Compose(new NoteDto { Topic = "Renew", Description = "The contract." }, new KMessageDto { Comment = "Today" });

        Assert.AreEqual("Renew", subject);
        Assert.AreEqual("Today\r\n\r\nThe contract.", body);
        StringAssert.StartsWith(KntAlarmEmail.FailedComment(new KMessageDto { Comment = "Today" }, "no email"), "[Email failed ");
    }

    private async Task<(KntService Service, RepositoryTestDatabase Db)> CreateAsync(string orm)
    {
        var db = new RepositoryTestDatabase();
        _databases.Add(db);
        await Task.CompletedTask;
        return (new KntService(db.CreateRepository(orm), activateMessageBroker: false), db);
    }

    private static async Task<UserDto> NewUserAsync(KntService service)
    {
        var id = Guid.NewGuid();
        var res = await service.Users.CreateAsync(new UserRegisterDto
        {
            UserName = $"alm-{id:N}"[..24],
            EMail = $"{id:N}@knote.tests",
            FullName = $"Alarms user {id:N}"[..24],
            RoleDefinition = nameof(EnumRoles.Staff),
            Password = "Alarms-Password-1!"
        });
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        return res.Entity;
    }

    private static async Task<Guid> NewNoteAsync(KntService service, string topic = "Alarms note")
    {
        var folder = await service.Repository.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "Alarms folder" });
        var noteId = Guid.NewGuid();
        var note = await service.Repository.Notes.AddAsync(new NoteDto { NoteId = noteId, Topic = topic, FolderId = folder.Entity.FolderId, CreationDateTime = DateTime.Now, ModificationDateTime = DateTime.Now });
        Assert.IsTrue(note.IsValid, note.ErrorMessage);
        return noteId;
    }

    private static async Task<KMessageDto> AddAlarmAsync(KntService service, Guid noteId, Guid userId, EnumNotificationType notificationType,
        DateTime alarmDateTime, EnumAlarmType alarmType = EnumAlarmType.Standard, string comment = "Alarm")
    {
        var alarm = new KMessageDto
        {
            KMessageId = Guid.NewGuid(), NoteId = noteId, UserId = userId, NotificationType = notificationType,
            AlarmType = alarmType, AlarmActivated = true, AlarmDateTime = alarmDateTime, Comment = comment
        };
        var res = await service.Repository.Notes.AddMessageAsync(alarm);
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        return alarm;
    }
}

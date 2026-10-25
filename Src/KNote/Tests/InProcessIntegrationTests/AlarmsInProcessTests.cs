using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Server.Helpers;
using KNote.Service.Core;
using KNote.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace KNote.Tests.InProcessIntegrationTests;

/// <summary>
/// The alarms that become due, in the Server: the Application info notices of the web app (api/alarms/check, consumed
/// once, with the interval of the Server's configuration; api/alarms/notices, the ones the panel keeps that are still
/// the user's notices), and the Email alarms sent by EmailAlarmsService (every repository; the SMTP server down puts the
/// alarm back; no email is written in the alarm's comment). Two repositories; the service's rounds are run by the tests
/// (Alarms:Email:Enabled is off, so it doesn't run by itself).
/// </summary>
[TestClass]
public class AlarmsInProcessTests
{
    private const string Password = "Alarms-InProcess-1!";

    private static KNoteWebApplicationFactory _factory = null!;
    private static readonly FakeEmailSender EmailSender = new();

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        _factory = new KNoteWebApplicationFactory(repositoryCount: 2);
        _factory.AppConfiguration["RateLimits:SignInPerMinute"] = "1000";
        _factory.AppConfiguration["Alarms:AppInfoCheckSeconds"] = "30";
        _factory.AppConfiguration["Email:Host"] = "smtp.knote.tests";
        _factory.AppConfiguration["Email:FromAddress"] = "knote@knote.tests";
        _factory.TestServices = services => services.AddSingleton<IEmailSender>(EmailSender);

        using var client = _factory.CreateClient();
        await client.GetAsync("api/systemvalues/repositoryname");
    }

    [ClassCleanup]
    public static async Task ClassCleanup() => await _factory.DisposeAsync();

    [TestMethod]
    public async Task Check_TheDueAppInfoAlarmsOfTheUser_Once_WithTheConfiguredInterval()
    {
        var (user, client) = await NewUserAsync();
        var (other, _) = await NewUserAsync();
        using var service = _factory.CreateServiceForRepository(0);
        var noteId = await NewNoteAsync(service, "Alarm notice note");
        var due = await AddAlarmAsync(service, noteId, user.UserId, EnumNotificationType.AppInfo, DateTime.Now.AddMinutes(-1), "Call the customer");
        await AddAlarmAsync(service, noteId, user.UserId, EnumNotificationType.AppInfo, DateTime.Now.AddHours(1));
        await AddAlarmAsync(service, noteId, user.UserId, EnumNotificationType.Email, DateTime.Now.AddMinutes(-1));
        await AddAlarmAsync(service, noteId, other.UserId, EnumNotificationType.AppInfo, DateTime.Now.AddMinutes(-1));

        var first = await ReadAsync<AlarmCheckDto>(await client.PostAsync("api/alarms/check", null));
        var second = await ReadAsync<AlarmCheckDto>(await client.PostAsync("api/alarms/check", null));

        Assert.AreEqual(30, first.CheckSeconds);
        var notice = first.Notices.Single();
        Assert.AreEqual(due.KMessageId, notice.KMessageId);
        Assert.AreEqual(noteId, notice.NoteId);
        Assert.AreEqual("Alarm notice note", notice.NoteTopic);
        Assert.AreEqual("Call the customer", notice.Comment);
        Assert.IsEmpty(second.Notices, "Once.");
        Assert.IsFalse((await service.Notes.GetMessageAsync(due.KMessageId)).Entity.AlarmActivated);
    }

    [TestMethod]
    public async Task Notices_OnlyTheOnesStillNoticesOfTheUser()
    {
        var (user, client) = await NewUserAsync();
        var (other, _) = await NewUserAsync();
        using var service = _factory.CreateServiceForRepository(0);
        var noteId = await NewNoteAsync(service, "Kept notices note");
        var kept = await AddAlarmAsync(service, noteId, user.UserId, EnumNotificationType.AppInfo, DateTime.Now.AddMinutes(-1));
        var deleted = await AddAlarmAsync(service, noteId, user.UserId, EnumNotificationType.AppInfo, DateTime.Now.AddMinutes(-1));
        var ofOther = await AddAlarmAsync(service, noteId, other.UserId, EnumNotificationType.AppInfo, DateTime.Now.AddMinutes(-1));
        Assert.IsTrue((await service.Notes.DeleteMessageAsync(deleted.KMessageId)).IsValid);

        var notices = await ReadAsync<List<AlarmNoticeDto>>(await client.PostAsJsonAsync("api/alarms/notices",
            new List<Guid> { kept.KMessageId, deleted.KMessageId, ofOther.KMessageId }));

        Assert.AreEqual(kept.KMessageId, notices.Single().KMessageId);
    }

    [TestMethod]
    public async Task Alarms_AGuestCanNotCheckThem()
    {
        var (_, client) = await NewUserAsync(nameof(EnumRoles.Guest));

        var check = await client.PostAsync("api/alarms/check", null);
        var notices = await client.PostAsJsonAsync("api/alarms/notices", new List<Guid>());

        Assert.AreEqual(HttpStatusCode.Forbidden, check.StatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden, notices.StatusCode);
    }

    [TestMethod]
    public async Task EmailAlarms_SentOnceInEveryRepository_PutBackWhenTheSmtpServerFails_FailureWhenNoEmail()
    {
        var alarms = _factory.Services.GetRequiredService<EmailAlarmsService>();
        await alarms.SendDueAlarmsAsync();
        var (userA, _) = await NewUserAsync();
        var (failing, _) = await NewUserAsync();
        using var serviceA = _factory.CreateServiceForRepository(0);
        using var serviceB = _factory.CreateServiceForRepository(1);
        var userB = await NewUserInAsync(serviceB);
        var noteA = await NewNoteAsync(serviceA, "Email alarm A", "The description A.");
        var noteB = await NewNoteAsync(serviceB, "Email alarm B");
        var dueA = await AddAlarmAsync(serviceA, noteA, userA.UserId, EnumNotificationType.Email, DateTime.Now.AddMinutes(-1), "Renew");
        await AddAlarmAsync(serviceB, noteB, userB.UserId, EnumNotificationType.Email, DateTime.Now.AddMinutes(-1));
        var notSent = await AddAlarmAsync(serviceA, noteA, failing.UserId, EnumNotificationType.Email, DateTime.Now.AddMinutes(-2));
        var noEmail = await AddAlarmAsync(serviceA, noteA, null, EnumNotificationType.Email, DateTime.Now.AddMinutes(-1), "Nobody");
        var appInfo = await AddAlarmAsync(serviceA, noteA, userA.UserId, EnumNotificationType.AppInfo, DateTime.Now.AddMinutes(-1));
        EmailSender.FailTo = failing.EMail;
        var before = EmailSender.Sent.Count;

        var sent = await alarms.SendDueAlarmsAsync();
        var again = await alarms.SendDueAlarmsAsync();

        Assert.AreEqual(2, sent);
        var emails = EmailSender.Sent.Skip(before).ToList();
        var toA = emails.Single(e => e.To == userA.EMail);
        Assert.AreEqual("Email alarm A", toA.Subject);
        Assert.AreEqual("Renew\r\n\r\nThe description A.", toA.Body);
        Assert.AreEqual("Email alarm B", emails.Single(e => e.To == userB.EMail).Subject);
        Assert.AreEqual(0, again, "Once; the failing one is still failing.");
        Assert.IsFalse((await serviceA.Notes.GetMessageAsync(dueA.KMessageId)).Entity.AlarmActivated);

        var putBack = (await serviceA.Notes.GetMessageAsync(notSent.KMessageId)).Entity;
        Assert.IsTrue(putBack.AlarmActivated, "Put back, to be sent the next time.");
        Assert.IsLessThan(1, Math.Abs((notSent.AlarmDateTime!.Value - putBack.AlarmDateTime!.Value).TotalSeconds), "As it was when due.");
        var failed = (await serviceA.Notes.GetMessageAsync(noEmail.KMessageId)).Entity;
        Assert.IsFalse(failed.AlarmActivated);
        StringAssert.StartsWith(failed.Comment, "[Email failed ");
        StringAssert.EndsWith(failed.Comment, "Nobody");
        Assert.IsTrue((await serviceA.Notes.GetMessageAsync(appInfo.KMessageId)).Entity.AlarmActivated, "Only the Email alarms.");

        EmailSender.FailTo = null;
        Assert.AreEqual(1, await alarms.SendDueAlarmsAsync(), "Sent when the SMTP server is back.");
        Assert.IsTrue(EmailSender.Sent.Any(e => e.To == failing.EMail));
    }

    private static async Task<(UserDto User, HttpClient Client)> NewUserAsync(string role = nameof(EnumRoles.Staff))
    {
        UserDto user;
        using (var service = _factory.CreateServiceForRepository(0))
            user = await NewUserInAsync(service, role);

        var client = _factory.CreateClient();
        var token = await (await client.PostAsJsonAsync("api/users/login", new UserCredentialsDto { UserName = user.UserName, Password = Password }))
            .Content.ReadFromJsonAsync<UserTokenDto>();
        Assert.IsTrue(token!.success, token.error);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token.token);
        return (user, client);
    }

    private static async Task<UserDto> NewUserInAsync(KntService service, string role = nameof(EnumRoles.Staff))
    {
        var id = Guid.NewGuid();
        var res = await service.Users.CreateAsync(new UserRegisterDto
        {
            UserName = $"alm-{id:N}"[..24],
            EMail = $"{id:N}@knote.tests",
            FullName = $"Alarms user {id:N}"[..24],
            RoleDefinition = role,
            Password = Password
        });
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        return res.Entity;
    }

    private static async Task<Guid> NewNoteAsync(KntService service, string topic, string description = "")
    {
        var folder = await service.Repository.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "Alarms folder" });
        var noteId = Guid.NewGuid();
        var note = await service.Repository.Notes.AddAsync(new NoteDto
        {
            NoteId = noteId, Topic = topic, Description = description, FolderId = folder.Entity.FolderId,
            CreationDateTime = DateTime.Now, ModificationDateTime = DateTime.Now
        });
        Assert.IsTrue(note.IsValid, note.ErrorMessage);
        return noteId;
    }

    private static async Task<KMessageDto> AddAlarmAsync(KntService service, Guid noteId, Guid? userId, EnumNotificationType notificationType,
        DateTime alarmDateTime, string comment = "Alarm")
    {
        var alarm = new KMessageDto
        {
            KMessageId = Guid.NewGuid(), NoteId = noteId, UserId = userId, NotificationType = notificationType,
            AlarmType = EnumAlarmType.Standard, AlarmActivated = true, AlarmDateTime = alarmDateTime, Comment = comment
        };
        var res = await service.Repository.Notes.AddMessageAsync(alarm);
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        return alarm;
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage httpRes)
    {
        var body = await httpRes.Content.ReadAsStringAsync();
        Assert.IsTrue(httpRes.IsSuccessStatusCode, $"{httpRes.StatusCode}: {body}");
        var res = System.Text.Json.JsonSerializer.Deserialize<Result<T>>(body, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));
        Assert.IsTrue(res!.IsValid, res.ErrorMessage);
        return res.Entity!;
    }
}

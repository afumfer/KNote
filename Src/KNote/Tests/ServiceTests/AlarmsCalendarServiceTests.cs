using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;
using Microsoft.Data.Sqlite;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// The alarms calendar of the web (Notes.GetAlarmsCalendarAsync): the alarms of a filter that fire in a range, with
/// their note, and their occurrences (periodic ones from the creation of the note on, see AlarmOccurrencesTests), in
/// both ORMs.
/// </summary>
[TestClass]
public class AlarmsCalendarServiceTests
{
    private readonly List<RepositoryTestDatabase> _databases = new();
    private static readonly DateTime NoteCreation = new(2026, 9, 3, 16, 20, 0);

    [TestCleanup]
    public void Cleanup() => _databases.ForEach(db => db.Dispose());

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task Calendar_TheAlarmsOfTheRange_WithTheirNoteAndOccurrences(string orm)
    {
        var service = Create(orm);
        var user = await NewUserAsync(service);
        var (noteId, noteNumber) = await NewNoteAsync(service, "Calendar note");
        var standard = await AddAlarmAsync(service, noteId, user.UserId, new DateTime(2026, 10, 20, 9, 0, 0));
        var weekly = await AddAlarmAsync(service, noteId, user.UserId, new DateTime(2026, 10, 20, 9, 0, 0), EnumAlarmType.Weekly);
        await AddAlarmAsync(service, noteId, user.UserId, new DateTime(2026, 12, 20, 9, 0, 0));

        var res = await service.Notes.GetAlarmsCalendarAsync(Filter(new DateTime(2026, 10, 1), new DateTime(2026, 11, 1)));

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        CollectionAssert.AreEquivalent(new[] { standard.KMessageId, weekly.KMessageId }, res.Entity.Alarms.Select(a => a.KMessageId).ToArray());
        var alarm = res.Entity.Alarms.First();
        Assert.AreEqual(noteNumber, alarm.NoteNumber);
        Assert.AreEqual("Calendar note", alarm.NoteTopic);
        Assert.AreEqual(user.FullName, alarm.UserFullName);
        Assert.AreEqual(1, res.Entity.Occurrences.Count(o => o.KMessageId == standard.KMessageId));
        Assert.AreEqual(4, res.Entity.Occurrences.Count(o => o.KMessageId == weekly.KMessageId));
        CollectionAssert.AreEqual(res.Entity.Occurrences.OrderBy(o => o.DateTime).ToList(), res.Entity.Occurrences);
        Assert.IsFalse(res.Entity.Truncated);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task Calendar_PeriodicAlarm_FromTheCreationOfItsNote(string orm)
    {
        var service = Create(orm);
        var user = await NewUserAsync(service);
        var (noteId, _) = await NewNoteAsync(service);
        var weekly = await AddAlarmAsync(service, noteId, user.UserId, new DateTime(2026, 10, 20, 9, 0, 0), EnumAlarmType.Weekly);

        var september = await service.Notes.GetAlarmsCalendarAsync(Filter(new DateTime(2026, 8, 1), new DateTime(2026, 10, 1)));
        var future = await service.Notes.GetAlarmsCalendarAsync(Filter(new DateTime(2030, 1, 1), new DateTime(2030, 2, 1)));

        CollectionAssert.AreEqual(new[] { 8, 15, 22, 29 }.Select(d => new DateTime(2026, 9, d, 9, 0, 0)).ToList(),
            september.Entity.Occurrences.Select(o => o.DateTime).ToList());
        Assert.AreEqual(5, future.Entity.Occurrences.Count);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task Calendar_Filters(string orm)
    {
        var service = Create(orm);
        var (user, other) = (await NewUserAsync(service), await NewUserAsync(service));
        var (noteId, _) = await NewNoteAsync(service);
        var date = new DateTime(2026, 10, 20, 9, 0, 0);
        var mine = await AddAlarmAsync(service, noteId, user.UserId, date);
        var email = await AddAlarmAsync(service, noteId, user.UserId, date, notificationType: EnumNotificationType.Email);
        var daily = await AddAlarmAsync(service, noteId, user.UserId, date, EnumAlarmType.Daily);
        var others = await AddAlarmAsync(service, noteId, other.UserId, date);
        var off = await AddAlarmAsync(service, noteId, user.UserId, date, EnumAlarmType.Weekly, activated: false);
        var (start, end) = (new DateTime(2026, 10, 1), new DateTime(2026, 11, 1));

        async Task<Guid[]> Ids(AlarmsCalendarFilterDto filter)
            => (await service.Notes.GetAlarmsCalendarAsync(filter)).Entity.Alarms.Select(a => a.KMessageId).ToArray();

        CollectionAssert.AreEquivalent(new[] { mine.KMessageId, email.KMessageId, daily.KMessageId, others.KMessageId },
            await Ids(Filter(start, end)));
        CollectionAssert.AreEquivalent(new[] { mine.KMessageId, email.KMessageId, daily.KMessageId },
            await Ids(Filter(start, end, f => f.UserId = user.UserId)));
        CollectionAssert.AreEquivalent(new[] { daily.KMessageId },
            await Ids(Filter(start, end, f => f.AlarmTypes = new() { EnumAlarmType.Daily, EnumAlarmType.Weekly })));
        CollectionAssert.AreEquivalent(new[] { email.KMessageId },
            await Ids(Filter(start, end, f => f.NotificationTypes = new() { EnumNotificationType.Email })));

        var withOff = await service.Notes.GetAlarmsCalendarAsync(Filter(start, end, f => f.IncludeDeactivated = true));
        CollectionAssert.Contains(withOff.Entity.Alarms.Select(a => a.KMessageId).ToList(), off.KMessageId);
        Assert.AreEqual(1, withOff.Entity.Occurrences.Count(o => o.KMessageId == off.KMessageId), "Deactivated: only at its date.");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task Calendar_EveryFewMinutes_GroupedByDayOrTruncated(string orm)
    {
        var service = Create(orm);
        var user = await NewUserAsync(service);
        var (noteId, _) = await NewNoteAsync(service);
        var frequent = await AddAlarmAsync(service, noteId, user.UserId, new DateTime(2026, 10, 20, 9, 10, 0), EnumAlarmType.InMinutes, minutes: 10);
        var (start, end) = (new DateTime(2026, 10, 19), new DateTime(2026, 10, 26));

        var grouped = await service.Notes.GetAlarmsCalendarAsync(Filter(start, end, f => f.GroupFrequentByDay = true));
        var all = await service.Notes.GetAlarmsCalendarAsync(Filter(new DateTime(2026, 10, 1), new DateTime(2026, 11, 1)));

        Assert.AreEqual(7, grouped.Entity.Occurrences.Count);
        Assert.AreEqual(144, grouped.Entity.Occurrences[1].Count);
        Assert.IsFalse(grouped.Entity.Truncated);
        Assert.AreEqual(KntConst.MaxAlarmsCalendarOccurrences, all.Entity.Occurrences.Count);
        Assert.IsTrue(all.Entity.Truncated);
        Assert.AreEqual(frequent.KMessageId, all.Entity.Alarms.Single().KMessageId);
    }

    [TestMethod]
    [DataRow("Dapper")]
    public async Task Calendar_WrongRange_IsNotValid(string orm)
    {
        var service = Create(orm);

        var res = await service.Notes.GetAlarmsCalendarAsync(Filter(new DateTime(2026, 11, 1), new DateTime(2026, 10, 1)));

        Assert.IsFalse(res.IsValid);
    }

    private static AlarmsCalendarFilterDto Filter(DateTime start, DateTime end, Action<AlarmsCalendarFilterDto>? change = null)
    {
        var filter = new AlarmsCalendarFilterDto { Start = start, End = end };
        change?.Invoke(filter);
        return filter;
    }

    private KntService Create(string orm)
    {
        var db = new RepositoryTestDatabase();
        _databases.Add(db);
        return new KntService(db.CreateRepository(orm), activateMessageBroker: false);
    }

    // The repositories give a new note the current date; these tests need it to have been created on NoteCreation.
    private void SetNoteCreation(Guid noteId)
    {
        using var connection = new SqliteConnection($"Data Source={_databases[^1].DatabaseFilePath}");
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "UPDATE Notes SET CreationDateTime = @creation WHERE NoteId = @noteId;";
        cmd.Parameters.AddWithValue("@creation", NoteCreation);
        cmd.Parameters.AddWithValue("@noteId", noteId.ToString().ToUpperInvariant());
        Assert.AreEqual(1, cmd.ExecuteNonQuery());
    }

    private static async Task<UserDto> NewUserAsync(KntService service)
    {
        var id = Guid.NewGuid();
        var res = await service.Users.CreateAsync(new UserRegisterDto
        {
            UserName = $"cal-{id:N}"[..24],
            EMail = $"{id:N}@knote.tests",
            FullName = $"Calendar user {id:N}"[..24],
            RoleDefinition = nameof(EnumRoles.Staff),
            Password = "Calendar-Password-1!"
        });
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        return res.Entity;
    }

    private async Task<(Guid NoteId, int NoteNumber)> NewNoteAsync(KntService service, string topic = "Alarms note")
    {
        var folder = await service.Repository.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "Alarms folder" });
        var noteId = Guid.NewGuid();
        var note = await service.Repository.Notes.AddAsync(new NoteDto { NoteId = noteId, Topic = topic, FolderId = folder.Entity.FolderId, CreationDateTime = NoteCreation, ModificationDateTime = NoteCreation });
        Assert.IsTrue(note.IsValid, note.ErrorMessage);
        SetNoteCreation(noteId);
        return (noteId, note.Entity.NoteNumber);
    }

    private static async Task<KMessageDto> AddAlarmAsync(KntService service, Guid noteId, Guid userId, DateTime alarmDateTime,
        EnumAlarmType alarmType = EnumAlarmType.Standard, EnumNotificationType notificationType = EnumNotificationType.AppInfo,
        bool activated = true, int? minutes = null)
    {
        var alarm = new KMessageDto
        {
            KMessageId = Guid.NewGuid(), NoteId = noteId, UserId = userId, NotificationType = notificationType,
            AlarmType = alarmType, AlarmActivated = activated, AlarmDateTime = alarmDateTime, AlarmMinutes = minutes, Comment = "Alarm"
        };
        var res = await service.Repository.Notes.AddMessageAsync(alarm);
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        return alarm;
    }
}

using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// AlarmOccurrences: the dates an alarm fires on, for the alarms calendar. Its AlarmDateTime is the next activation;
/// a periodic alarm fires every period from the creation of its note on, with the steps of ApplyAlarmControl.
/// </summary>
[TestClass]
public class AlarmOccurrencesTests
{
    private static readonly DateTime NoteCreation = new(2026, 9, 3, 16, 20, 0);

    private static KMessageDto Alarm(EnumAlarmType type, DateTime alarmDateTime, bool activated = true, int? minutes = null)
        => new() { AlarmType = type, AlarmDateTime = alarmDateTime, AlarmActivated = activated, AlarmMinutes = minutes };

    private static List<DateTime> Expand(KMessageDto alarm, DateTime start, DateTime end)
        => AlarmOccurrences.Expand(alarm, NoteCreation, start, end).ToList();

    [TestMethod]
    public void Standard_OnlyAtItsDate()
    {
        var alarm = Alarm(EnumAlarmType.Standard, new DateTime(2026, 10, 20, 9, 0, 0));

        CollectionAssert.AreEqual(new[] { alarm.AlarmDateTime!.Value }, Expand(alarm, new DateTime(2026, 10, 1), new DateTime(2026, 11, 1)));
        Assert.AreEqual(0, Expand(alarm, new DateTime(2026, 11, 1), new DateTime(2026, 12, 1)).Count);
    }

    [TestMethod]
    public void Weekly_FromTheNoteCreationOn_BeforeAndAfterItsDate()
    {
        var alarm = Alarm(EnumAlarmType.Weekly, new DateTime(2026, 10, 20, 9, 0, 0));

        var dates = Expand(alarm, new DateTime(2026, 9, 1), new DateTime(2026, 11, 1));

        // 3 Sep (16:20) is the creation: the first one is on Tuesday 8 Sep.
        CollectionAssert.AreEqual(new[] { 8, 15, 22, 29 }.Select(d => new DateTime(2026, 9, d, 9, 0, 0))
            .Concat(new[] { 6, 13, 20, 27 }.Select(d => new DateTime(2026, 10, d, 9, 0, 0))).ToList(), dates);
    }

    [TestMethod]
    public void Weekly_GoesOnInTheFuture()
    {
        var alarm = Alarm(EnumAlarmType.Weekly, new DateTime(2026, 10, 20, 9, 0, 0));

        var dates = Expand(alarm, new DateTime(2030, 1, 1), new DateTime(2030, 2, 1));

        Assert.AreEqual(5, dates.Count);
        Assert.IsTrue(dates.All(d => d.DayOfWeek == DayOfWeek.Tuesday && d.Hour == 9));
    }

    [TestMethod]
    public void Daily_InADay_OnlyOne()
    {
        var alarm = Alarm(EnumAlarmType.Daily, new DateTime(2026, 10, 20, 9, 0, 0));

        CollectionAssert.AreEqual(new[] { new DateTime(2026, 10, 5, 9, 0, 0) }, Expand(alarm, new DateTime(2026, 10, 5), new DateTime(2026, 10, 6)));
    }

    [TestMethod]
    public void Monthly_FollowsTheDriftOfApplyAlarmControl()
    {
        var alarm = Alarm(EnumAlarmType.Monthly, new DateTime(2027, 1, 31, 8, 0, 0));

        var dates = Expand(alarm, new DateTime(2027, 1, 1), new DateTime(2027, 5, 1));

        CollectionAssert.AreEqual(new[]
        {
            new DateTime(2027, 1, 31, 8, 0, 0), new DateTime(2027, 2, 28, 8, 0, 0),
            new DateTime(2027, 3, 28, 8, 0, 0), new DateTime(2027, 4, 28, 8, 0, 0)
        }, dates);

        // What ApplyAlarmControl does when it fires: the same next date.
        var fired = Alarm(EnumAlarmType.Monthly, new DateTime(2027, 1, 31, 8, 0, 0));
        while (fired.AlarmDateTime < new DateTime(2027, 3, 1))
            fired.AlarmDateTime = fired.AlarmDateTime.Value.AddMonths(1);
        Assert.AreEqual(dates[2], fired.AlarmDateTime);
    }

    [TestMethod]
    public void Annual_BeforeTheNoteCreation_Nothing()
    {
        var alarm = Alarm(EnumAlarmType.Annual, new DateTime(2027, 5, 10, 8, 0, 0));

        CollectionAssert.AreEqual(new[] { new DateTime(2028, 5, 10, 8, 0, 0) }, Expand(alarm, new DateTime(2028, 1, 1), new DateTime(2029, 1, 1)));
        Assert.AreEqual(0, Expand(alarm, new DateTime(2026, 1, 1), new DateTime(2027, 1, 1)).Count);
    }

    [TestMethod]
    public void InMinutes_EveryFewMinutes()
    {
        var alarm = Alarm(EnumAlarmType.InMinutes, new DateTime(2026, 10, 20, 9, 10, 0), minutes: 45);

        var dates = Expand(alarm, new DateTime(2026, 10, 21), new DateTime(2026, 10, 22));

        Assert.AreEqual(32, dates.Count);
        Assert.IsTrue(dates.Zip(dates.Skip(1)).All(p => p.Second - p.First == TimeSpan.FromMinutes(45)));
    }

    [TestMethod]
    public void Deactivated_OnlyAtItsDate()
    {
        var alarm = Alarm(EnumAlarmType.Daily, new DateTime(2026, 10, 20, 9, 0, 0), activated: false);

        CollectionAssert.AreEqual(new[] { alarm.AlarmDateTime!.Value }, Expand(alarm, new DateTime(2026, 10, 1), new DateTime(2026, 11, 1)));
    }

    [TestMethod]
    public void AlarmDateBeforeTheNoteCreation_StillShown()
    {
        var alarm = Alarm(EnumAlarmType.Weekly, new DateTime(2026, 8, 25, 9, 0, 0));

        var dates = Expand(alarm, new DateTime(2026, 8, 1), new DateTime(2026, 9, 9));

        CollectionAssert.AreEqual(new[] { new DateTime(2026, 8, 25, 9, 0, 0), new DateTime(2026, 9, 1, 9, 0, 0), new DateTime(2026, 9, 8, 9, 0, 0) }, dates);
    }

    [TestMethod]
    public void WithoutDate_Nothing()
    {
        Assert.AreEqual(0, AlarmOccurrences.Expand(new KMessageDto { AlarmType = EnumAlarmType.Daily, AlarmActivated = true },
            NoteCreation, DateTime.MinValue.AddYears(1), DateTime.MaxValue.AddYears(-1)).Count());
    }
}

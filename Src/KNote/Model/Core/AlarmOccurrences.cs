using System;
using System.Collections.Generic;

namespace KNote.Model;

/// <summary>
/// The dates on which an alarm fires, for the alarms calendar of the web. An alarm's AlarmDateTime is the next
/// activation (the reference); a periodic alarm also fires every period before and after it, from the start of its
/// series (the creation of its note) on. The steps are the ones ApplyAlarmControl uses to reschedule it after it fires,
/// one period at a time, so the dates after the reference are the real next ones (with the same drift of the monthly
/// alarms: 31 Jan, 28 Feb, 28 Mar...); before it, they go back one period at a time from the reference.
/// </summary>
public static class AlarmOccurrences
{
    /// <summary>
    /// The occurrences of the alarm in [rangeStart, rangeEnd), in ascending order. A Standard alarm (or one of an
    /// unknown type), and a deactivated one, only at its AlarmDateTime. A periodic one, at every period from
    /// seriesStart (or from its AlarmDateTime, if that is earlier) on. Lazy: the caller takes as many as it wants.
    /// </summary>
    public static IEnumerable<DateTime> Expand(IAlarmSchedule alarm, DateTime seriesStart, DateTime rangeStart, DateTime rangeEnd)
    {
        if (alarm.AlarmDateTime == null || rangeEnd <= rangeStart)
            yield break;

        var reference = alarm.AlarmDateTime.Value;
        var from = seriesStart < reference ? seriesStart : reference;
        if (rangeStart > from)
            from = rangeStart;

        if (!IsPeriodic(alarm) || alarm.AlarmActivated != true)
        {
            if (reference >= rangeStart && reference < rangeEnd)
                yield return reference;
            yield break;
        }

        var step = FixedStep(alarm);
        if (step != null)
        {
            // Daily, Weekly and InMinutes: reference + k * step, with k computed (an alarm every few minutes has
            // hundreds of thousands of occurrences in a few years).
            var ticks = step.Value.Ticks;
            var k = (long)Math.Ceiling((from - reference).Ticks / (double)ticks);
            for (var date = reference.AddTicks(k * ticks); date < rangeEnd; date = date.AddTicks(ticks))
                if (date >= from)
                    yield return date;
            yield break;
        }

        // Monthly and Annual, one period at a time (a few per year): first back from the reference, then forward.
        var before = new List<DateTime>();
        for (var date = Step(alarm, reference, -1); date >= from; date = Step(alarm, date, -1))
            if (date < rangeEnd)
                before.Add(date);
        for (var i = before.Count - 1; i >= 0; i--)
            yield return before[i];

        for (var date = reference; date < rangeEnd; date = Step(alarm, date, 1))
            if (date >= from)
                yield return date;
    }

    /// <summary>Whether the alarm repeats (every type but Standard).</summary>
    public static bool IsPeriodic(IAlarmSchedule alarm) => alarm.AlarmType switch
    {
        EnumAlarmType.Daily or EnumAlarmType.Weekly or EnumAlarmType.Monthly or EnumAlarmType.Annual => true,
        EnumAlarmType.InMinutes => (alarm.AlarmMinutes ?? 0) >= 1,
        _ => false
    };

    private static TimeSpan? FixedStep(IAlarmSchedule alarm) => alarm.AlarmType switch
    {
        EnumAlarmType.Daily => TimeSpan.FromDays(1),
        EnumAlarmType.Weekly => TimeSpan.FromDays(7),
        EnumAlarmType.InMinutes => TimeSpan.FromMinutes(alarm.AlarmMinutes.Value),
        _ => null
    };

    private static DateTime Step(IAlarmSchedule alarm, DateTime date, int periods)
        => alarm.AlarmType == EnumAlarmType.Annual ? date.AddYears(periods) : date.AddMonths(periods);
}

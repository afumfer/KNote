using System;

namespace KNote.Model;

/// <summary>
/// The time window of the tasks panel: N whole units (days, weeks from Monday to Sunday, or months) around a
/// reference date, centered on the reference's unit; with an even N the unit left over goes to the past (N/2 before,
/// N/2 - 1 after). Dates only (no time): Start and End are the first and last day of the window, both included.
/// Shared by the Server, which reads the tasks of the window, and the web, which moves the reference.
/// </summary>
public static class TasksWindow
{
    public const int DefaultSize = 3;

    public static int MaxSize(EnumTasksWindowUnit unit) => unit == EnumTasksWindowUnit.Days ? 31 : 12;

    public static (DateTime Start, DateTime End) Compute(EnumTasksWindowUnit unit, int size, DateTime reference)
    {
        size = Math.Clamp(size, 1, MaxSize(unit));
        var day = reference.Date;
        var before = size / 2;

        switch (unit)
        {
            case EnumTasksWindowUnit.Months:
                var start = new DateTime(day.Year, day.Month, 1).AddMonths(-before);
                return (start, start.AddMonths(size).AddDays(-1));
            case EnumTasksWindowUnit.Weeks:
                var monday = day.AddDays(-(((int)day.DayOfWeek + 6) % 7)).AddDays(-7 * before);
                return (monday, monday.AddDays(7 * size - 1));
            default:
                var first = day.AddDays(-before);
                return (first, first.AddDays(size - 1));
        }
    }

    /// <summary>
    /// The reference moved the given number of units (negative, to the past). Moving months keeps the day of the
    /// month, or the last day of a shorter month.
    /// </summary>
    public static DateTime Shift(EnumTasksWindowUnit unit, DateTime reference, int units) => unit switch
    {
        EnumTasksWindowUnit.Months => reference.Date.AddMonths(units),
        EnumTasksWindowUnit.Weeks => reference.Date.AddDays(7 * units),
        _ => reference.Date.AddDays(units)
    };

    /// <summary>
    /// The cut-off date D the tasks are classified on: today, or the last day of the window if it ends before today
    /// (the state the tasks had then, without a history of changes).
    /// </summary>
    public static DateTime CutOff(DateTime windowEnd, DateTime today)
        => windowEnd.Date < today.Date ? windowEnd.Date : today.Date;
}

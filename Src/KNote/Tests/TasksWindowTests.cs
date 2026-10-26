using KNote.Model;

namespace KNote.Tests;

/// <summary>
/// TasksWindow: the time window of the tasks panel. The examples are the table of the use case (CU-TAR-01), with today
/// = Saturday 24 Oct 2026.
/// </summary>
[TestClass]
public class TasksWindowTests
{
    private static readonly DateTime Today = new(2026, 10, 24);

    [TestMethod]
    [DataRow(EnumTasksWindowUnit.Months, 3, "2026-09-01", "2026-11-30")]
    [DataRow(EnumTasksWindowUnit.Months, 4, "2026-08-01", "2026-11-30")]
    [DataRow(EnumTasksWindowUnit.Months, 5, "2026-08-01", "2026-12-31")]
    [DataRow(EnumTasksWindowUnit.Months, 6, "2026-07-01", "2026-12-31")]
    [DataRow(EnumTasksWindowUnit.Weeks, 3, "2026-10-12", "2026-11-01")]
    [DataRow(EnumTasksWindowUnit.Weeks, 4, "2026-10-05", "2026-11-01")]
    [DataRow(EnumTasksWindowUnit.Weeks, 5, "2026-10-05", "2026-11-08")]
    [DataRow(EnumTasksWindowUnit.Weeks, 6, "2026-09-28", "2026-11-08")]
    [DataRow(EnumTasksWindowUnit.Days, 3, "2026-10-23", "2026-10-25")]
    [DataRow(EnumTasksWindowUnit.Days, 4, "2026-10-22", "2026-10-25")]
    [DataRow(EnumTasksWindowUnit.Days, 5, "2026-10-22", "2026-10-26")]
    [DataRow(EnumTasksWindowUnit.Days, 6, "2026-10-21", "2026-10-26")]
    public void Compute_MatchesTheExamplesOfTheUseCase(EnumTasksWindowUnit unit, int size, string start, string end)
    {
        var window = TasksWindow.Compute(unit, size, Today.AddHours(17));

        Assert.AreEqual(DateTime.Parse(start), window.Start);
        Assert.AreEqual(DateTime.Parse(end), window.End);
    }

    [TestMethod]
    public void Compute_OneUnit_IsTheUnitOfTheReference()
    {
        Assert.AreEqual((new DateTime(2026, 10, 1), new DateTime(2026, 10, 31)), TasksWindow.Compute(EnumTasksWindowUnit.Months, 1, Today));
        Assert.AreEqual((new DateTime(2026, 10, 19), new DateTime(2026, 10, 25)), TasksWindow.Compute(EnumTasksWindowUnit.Weeks, 1, Today));
        Assert.AreEqual((Today, Today), TasksWindow.Compute(EnumTasksWindowUnit.Days, 1, Today));
    }

    [TestMethod]
    public void Compute_ClampsTheSize()
    {
        var window = TasksWindow.Compute(EnumTasksWindowUnit.Months, 40, Today);

        Assert.AreEqual(TasksWindow.MaxSize(EnumTasksWindowUnit.Months), (window.End.Year - window.Start.Year) * 12 + window.End.Month - window.Start.Month + 1);
    }

    [TestMethod]
    public void Compute_WeeksAcrossTheYear_StartOnMonday()
    {
        var window = TasksWindow.Compute(EnumTasksWindowUnit.Weeks, 1, new DateTime(2027, 1, 1));

        Assert.AreEqual(new DateTime(2026, 12, 28), window.Start);
        Assert.AreEqual(DayOfWeek.Monday, window.Start.DayOfWeek);
        Assert.AreEqual(new DateTime(2027, 1, 3), window.End);
    }

    [TestMethod]
    public void Shift_MonthsKeepsTheDayOrTheLastDayOfTheMonth()
    {
        Assert.AreEqual(new DateTime(2026, 2, 28), TasksWindow.Shift(EnumTasksWindowUnit.Months, new DateTime(2026, 1, 31), 1));
        Assert.AreEqual(new DateTime(2026, 7, 24), TasksWindow.Shift(EnumTasksWindowUnit.Months, Today, -3));
        Assert.AreEqual(new DateTime(2026, 11, 7), TasksWindow.Shift(EnumTasksWindowUnit.Weeks, Today, 2));
        Assert.AreEqual(new DateTime(2026, 10, 21), TasksWindow.Shift(EnumTasksWindowUnit.Days, Today, -3));
    }

    [TestMethod]
    public void CutOff_IsTodayOrTheEndOfAPastWindow()
    {
        Assert.AreEqual(Today, TasksWindow.CutOff(new DateTime(2026, 11, 30), Today.AddHours(9)));
        Assert.AreEqual(new DateTime(2026, 8, 31), TasksWindow.CutOff(new DateTime(2026, 8, 31), Today));
    }
}

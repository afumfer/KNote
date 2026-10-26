using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// TaskPanelRules: which tasks a window of the tasks panel shows, their column on the cut-off date, their indicators
/// and the inconsistent data they are shown with.
/// </summary>
[TestClass]
public class TaskPanelRulesTests
{
    private static readonly DateTime Today = new(2026, 10, 24);

    private static NoteTaskDto Task(string start, string? end = null, string? expectedEnd = null, bool? resolved = null)
    {
        var endDate = end == null ? (DateTime?)null : DateTime.Parse(end).AddHours(18);
        return new NoteTaskDto
        {
            CreationDateTime = DateTime.Parse(start).AddHours(-30),
            StartDate = DateTime.Parse(start).AddHours(9),
            EndDate = endDate,
            Resolved = resolved ?? endDate != null,
            ExpectedEndDate = expectedEnd == null ? null : DateTime.Parse(expectedEnd)
        };
    }

    [TestMethod]
    [DataRow("2026-10-30", null, EnumTaskPanelColumn.Pending)]
    [DataRow("2026-10-24", null, EnumTaskPanelColumn.InProgress)]
    [DataRow("2026-01-10", null, EnumTaskPanelColumn.InProgress)]
    [DataRow("2026-10-01", "2026-10-24", EnumTaskPanelColumn.Resolved)]
    [DataRow("2026-10-01", "2026-10-25", EnumTaskPanelColumn.InProgress)]
    public void Classify_OnTheCutOffDate(string start, string? end, EnumTaskPanelColumn expected)
    {
        Assert.AreEqual(expected, TaskPanelRules.Classify(Task(start, end), Today));
    }

    [TestMethod]
    public void Classify_InAPastWindow_RebuildsTheStateOfThen()
    {
        var cutOff = TasksWindow.CutOff(new DateTime(2026, 8, 31), Today);
        var task = Task("2026-08-10", "2026-09-15");

        Assert.AreEqual(EnumTaskPanelColumn.InProgress, TaskPanelRules.Classify(task, cutOff));
        CollectionAssert.Contains(TaskPanelRules.Indicators(task, EnumTaskPanelColumn.InProgress, cutOff),
            new TaskPanelIndicator("Resolved later", EnumTaskPanelIndicatorKind.Good));
    }

    [TestMethod]
    [DataRow("2026-09-15", null, true)]           // open: goes on until today
    [DataRow("2026-08-01", "2026-09-01", true)]   // resolved on the first day of the window
    [DataRow("2026-08-01", "2026-08-31", false)]  // resolved the day before
    [DataRow("2026-11-30", null, true)]           // starts on the last day
    [DataRow("2026-12-01", null, false)]          // starts after it
    public void Overlaps_TheWindow(string start, string? end, bool expected)
    {
        Assert.AreEqual(expected, TaskPanelRules.Overlaps(Task(start, end), new DateTime(2026, 9, 1), new DateTime(2026, 11, 30), Today));
    }

    [TestMethod]
    public void Overlaps_AFutureWindow_OpenTasksDoNotReachIt()
    {
        var future = TasksWindow.Compute(EnumTasksWindowUnit.Months, 1, new DateTime(2027, 3, 1));

        Assert.IsFalse(TaskPanelRules.Overlaps(Task("2026-01-10"), future.Start, future.End, Today));
        Assert.IsTrue(TaskPanelRules.Overlaps(Task("2027-03-10"), future.Start, future.End, Today));
    }

    [TestMethod]
    public void WithoutStartDate_StartsOnItsCreationDate_AndIsInconsistent()
    {
        var task = new NoteTaskDto { CreationDateTime = new DateTime(2026, 10, 2, 12, 0, 0) };

        Assert.AreEqual(new DateTime(2026, 10, 2), TaskPanelRules.EffectiveStart(task));
        Assert.AreEqual(1, TaskPanelRules.InconsistencyReasons(task).Count);
    }

    [TestMethod]
    public void ResolvedFlagWithoutEndDate_IsOpen_AndInconsistent()
    {
        var task = Task("2026-10-01", resolved: true);

        Assert.AreEqual(EnumTaskPanelColumn.InProgress, TaskPanelRules.Classify(task, Today));
        Assert.AreEqual(1, TaskPanelRules.InconsistencyReasons(task).Count);
    }

    [TestMethod]
    public void EndDateWithoutResolvedFlag_IsResolved_AndInconsistent()
    {
        var task = Task("2026-10-01", "2026-10-05", resolved: false);

        Assert.AreEqual(EnumTaskPanelColumn.Resolved, TaskPanelRules.Classify(task, Today));
        Assert.AreEqual(1, TaskPanelRules.InconsistencyReasons(task).Count);
    }

    [TestMethod]
    public void EndBeforeStart_HasAnInvalidDuration()
    {
        var task = Task("2026-10-10", "2026-10-05");

        Assert.AreEqual(EnumTaskPanelColumn.Resolved, TaskPanelRules.Classify(task, Today));
        var indicators = TaskPanelRules.Indicators(task, EnumTaskPanelColumn.Resolved, Today);
        Assert.AreEqual("Invalid duration", indicators[0].Text);
        Assert.AreEqual(EnumTaskPanelIndicatorKind.Inconsistent, indicators[^1].Kind);
    }

    [TestMethod]
    public void Indicators_OfEachColumn()
    {
        Assert.AreEqual("Starts in 6 d", TaskPanelRules.Indicators(Task("2026-10-30"), EnumTaskPanelColumn.Pending, Today)[0].Text);
        Assert.AreEqual("Opened today", TaskPanelRules.Indicators(Task("2026-10-24"), EnumTaskPanelColumn.InProgress, Today)[0].Text);
        Assert.AreEqual("Open 14 d", TaskPanelRules.Indicators(Task("2026-10-10"), EnumTaskPanelColumn.InProgress, Today)[0].Text);
        Assert.AreEqual("Took 4 d", TaskPanelRules.Indicators(Task("2026-10-01", "2026-10-05"), EnumTaskPanelColumn.Resolved, Today)[0].Text);
        Assert.AreEqual("Same day", TaskPanelRules.Indicators(Task("2026-10-01", "2026-10-01"), EnumTaskPanelColumn.Resolved, Today)[0].Text);
    }

    [TestMethod]
    public void Indicators_WithExpectedDates_LateAndDeviation()
    {
        var late = TaskPanelRules.Indicators(Task("2026-10-01", expectedEnd: "2026-10-20"), EnumTaskPanelColumn.InProgress, Today);
        CollectionAssert.Contains(late, new TaskPanelIndicator("Late 4 d", EnumTaskPanelIndicatorKind.Late));

        var over = TaskPanelRules.Indicators(Task("2026-10-01", "2026-10-12", "2026-10-10"), EnumTaskPanelColumn.Resolved, Today);
        CollectionAssert.Contains(over, new TaskPanelIndicator("2 d over plan", EnumTaskPanelIndicatorKind.Late));

        var ahead = TaskPanelRules.Indicators(Task("2026-10-01", "2026-10-08", "2026-10-10"), EnumTaskPanelColumn.Resolved, Today);
        CollectionAssert.Contains(ahead, new TaskPanelIndicator("2 d ahead of plan", EnumTaskPanelIndicatorKind.Good));
    }

    [TestMethod]
    public void Indicators_WithoutExpectedDates_NeverLate()
    {
        var indicators = TaskPanelRules.Indicators(Task("2025-01-01"), EnumTaskPanelColumn.InProgress, Today);

        Assert.IsFalse(indicators.Any(i => i.Kind == EnumTaskPanelIndicatorKind.Late));
    }
}

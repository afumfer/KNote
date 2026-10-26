using System;
using System.Collections.Generic;
using KNote.Model.Dto;

namespace KNote.Model;

/// <summary>
/// The rules of the tasks panel (which tasks a window shows, the column of each one, its indicators and why its data
/// is inconsistent), on whole days. A task is resolved if and only if it has an EndDate; one without StartDate starts
/// on its creation date. The repositories' query of the panel (IKntNoteRepository.GetTasksPanel*) is the SQL version
/// of Overlaps and Classify: keep them in step.
/// </summary>
public static class TaskPanelRules
{
    public static DateTime EffectiveStart(NoteTaskDto task) => (task.StartDate ?? task.CreationDateTime).Date;

    public static bool IsResolved(NoteTaskDto task) => task.EndDate != null;

    /// <summary>
    /// The last day of the task: its EndDate if it is resolved and, if not, today (it goes on), but never before its
    /// start.
    /// </summary>
    public static DateTime EffectiveEnd(NoteTaskDto task, DateTime today)
    {
        var start = EffectiveStart(task);
        var end = IsResolved(task) ? task.EndDate.Value.Date : today.Date;
        return end > start ? end : start;
    }

    /// <summary>Whether the task is in the window: its days [start, effective end] overlap the window's.</summary>
    public static bool Overlaps(NoteTaskDto task, DateTime windowStart, DateTime windowEnd, DateTime today)
        => EffectiveStart(task) <= windowEnd.Date && EffectiveEnd(task, today) >= windowStart.Date;

    /// <summary>The column of the task on the cut-off date D (TasksWindow.CutOff).</summary>
    public static EnumTaskPanelColumn Classify(NoteTaskDto task, DateTime cutOff)
    {
        if (EffectiveStart(task) > cutOff.Date)
            return EnumTaskPanelColumn.Pending;
        if (IsResolved(task) && task.EndDate.Value.Date <= cutOff.Date)
            return EnumTaskPanelColumn.Resolved;
        return EnumTaskPanelColumn.InProgress;
    }

    /// <summary>Why the task's data breaks the rules of the panel (empty if it doesn't); it is still shown.</summary>
    public static List<string> InconsistencyReasons(NoteTaskDto task)
    {
        var reasons = new List<string>();
        if (task.StartDate == null)
            reasons.Add("It has no start date: its creation date is used.");
        if (task.Resolved && task.EndDate == null)
            reasons.Add("It is marked as resolved but has no end date: it is treated as open.");
        if (!task.Resolved && task.EndDate != null)
            reasons.Add("It has an end date but is not marked as resolved: it is treated as resolved.");
        if (task.EndDate != null && task.EndDate.Value.Date < EffectiveStart(task))
            reasons.Add("Its end date is earlier than its start date: its duration is not valid.");
        return reasons;
    }

    /// <summary>
    /// The indicators of the task in its column on the cut-off date: how long it has been open, how long it took, when
    /// it starts, and, only if it has expected dates, its delay or deviation (a task without them is never late).
    /// </summary>
    public static List<TaskPanelIndicator> Indicators(NoteTaskDto task, EnumTaskPanelColumn column, DateTime cutOff)
    {
        var d = cutOff.Date;
        var start = EffectiveStart(task);
        var expectedEnd = task.ExpectedEndDate?.Date;
        var list = new List<TaskPanelIndicator>();

        switch (column)
        {
            case EnumTaskPanelColumn.Pending:
                list.Add(new($"Starts in {Days(start - d)}"));
                break;
            case EnumTaskPanelColumn.InProgress:
                var open = (d - start).Days;
                list.Add(new(open == 0 ? "Opened today" : $"Open {Days(d - start)}"));
                if (expectedEnd != null && expectedEnd < d)
                    list.Add(new($"Late {Days(d - expectedEnd.Value)}", EnumTaskPanelIndicatorKind.Late));
                if (IsResolved(task))
                    list.Add(new("Resolved later", EnumTaskPanelIndicatorKind.Good));
                break;
            case EnumTaskPanelColumn.Resolved:
                var end = task.EndDate.Value.Date;
                if (end < start)
                    list.Add(new("Invalid duration", EnumTaskPanelIndicatorKind.Inconsistent));
                else
                    list.Add(new(end == start ? "Same day" : $"Took {Days(end - start)}"));
                if (expectedEnd != null)
                {
                    var deviation = (end - expectedEnd.Value).Days;
                    if (deviation > 0)
                        list.Add(new($"{deviation} d over plan", EnumTaskPanelIndicatorKind.Late));
                    else if (deviation < 0)
                        list.Add(new($"{-deviation} d ahead of plan", EnumTaskPanelIndicatorKind.Good));
                    else
                        list.Add(new("On time", EnumTaskPanelIndicatorKind.Good));
                }
                break;
        }

        if (InconsistencyReasons(task).Count > 0)
            list.Add(new("Inconsistent data", EnumTaskPanelIndicatorKind.Inconsistent));
        return list;
    }

    private static string Days(TimeSpan span) => $"{span.Days} d";
}

public enum EnumTaskPanelIndicatorKind
{
    Neutral,
    Late,
    Good,
    Inconsistent
}

public record TaskPanelIndicator(string Text, EnumTaskPanelIndicatorKind Kind = EnumTaskPanelIndicatorKind.Neutral);

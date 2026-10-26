using System;
using System.Collections.Generic;

namespace KNote.Model.Dto;

/// <summary>
/// What the tasks panel of the web asks for: its time window (TasksWindow: unit, size and reference date), the user's
/// today (the browser's date: the Server classifies with it) and the filters, which are combined with AND. With
/// Column, only the next page of that column ("Load more").
/// </summary>
public class TasksPanelFilterDto
{
    public EnumTasksWindowUnit Unit { get; set; } = EnumTasksWindowUnit.Months;
    public int Size { get; set; } = TasksWindow.DefaultSize;
    // Default (no date): today.
    public DateTime ReferenceDate { get; set; }
    public DateTime Today { get; set; }

    // The type of the task's note, the task's user.
    public Guid? NoteTypeId { get; set; }
    public Guid? UserId { get; set; }
    // The note's tags contain any of these words.
    public List<string> Tags { get; set; } = new();
    // In the task's topic and description, and the note's topic; a number, the note's number.
    public string Text { get; set; }
    public Guid? FolderId { get; set; }
    public bool IncludeChildFolders { get; set; } = true;

    public EnumTaskPanelColumn? Column { get; set; }
    public int Skip { get; set; }
    public int PageSize { get; set; } = 40;
}

/// <summary>
/// The query of the tasks panel the Service gives the repositories: the window and cut-off date already computed and the
/// filters, plus the note types left out (the system notes: AI assistant sessions and chat room threads). Dates only
/// (no time); WindowEnd and CutOff are included.
/// </summary>
public class TasksPanelQueryDto
{
    public DateTime WindowStart { get; set; }
    public DateTime WindowEnd { get; set; }
    public DateTime CutOff { get; set; }
    public DateTime Today { get; set; }

    public Guid? NoteTypeId { get; set; }
    public Guid? UserId { get; set; }
    public List<string> Tags { get; set; } = new();
    public string Text { get; set; }
    public Guid? FolderId { get; set; }
    public bool IncludeChildFolders { get; set; }
    public List<Guid> ExcludeNoteTypeIds { get; set; } = new();
}

/// <summary>How many tasks of the query each column has, and how many there are without the window.</summary>
public class TasksPanelCountsDto
{
    public int Resolved { get; set; }
    public int InProgress { get; set; }
    public int Pending { get; set; }
    public int TotalWithoutWindow { get; set; }

    public int this[EnumTaskPanelColumn column] => column switch
    {
        EnumTaskPanelColumn.Resolved => Resolved,
        EnumTaskPanelColumn.InProgress => InProgress,
        _ => Pending
    };
}

/// <summary>
/// The dates around a window without tasks where the nearest ones are: the last start or end date before it and the
/// first start after it (null: none).
/// </summary>
public class TasksPanelNearestDto
{
    public DateTime? LastBefore { get; set; }
    public DateTime? FirstAfter { get; set; }
}

/// <summary>A task of the panel, with its note and the column it is in on the cut-off date.</summary>
public class TaskPanelItemDto : NoteTaskDto
{
    public int NoteNumber { get; set; }
    public string NoteTopic { get; set; }
    public string NoteTags { get; set; }
    public Guid FolderId { get; set; }
    public Guid? NoteTypeId { get; set; }
    public string NoteTypeName { get; set; }
    public EnumTaskPanelColumn Column { get; set; }
}

public class TasksPanelColumnDto
{
    public EnumTaskPanelColumn Column { get; set; }
    // Every task of the column, not only those in Tasks (a page).
    public int TotalCount { get; set; }
    public List<TaskPanelItemDto> Tasks { get; set; } = new();
}

public class TasksPanelDto
{
    public DateTime WindowStart { get; set; }
    public DateTime WindowEnd { get; set; }
    public DateTime CutOff { get; set; }
    public DateTime Today { get; set; }

    // The three columns in order (Resolved, In progress, Pending), or only the one asked for (Load more).
    public List<TasksPanelColumnDto> Columns { get; set; } = new();
    // Tasks of the filters out of the window.
    public int OutsideCount { get; set; }
    // The window has no tasks but the filters do: a date that shows the nearest one (TasksWindow reference).
    public DateTime? NearestTaskDate { get; set; }
}

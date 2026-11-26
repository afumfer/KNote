using System;
using System.Collections.Generic;

namespace KNote.Model.Dto;

/// <summary>A page of the history of a note (the «History» tab, H.3): its rows, the newest first, without their copies.</summary>
public class NoteHistoryPageDto
{
    public const int PageSize = 100;

    public Guid NoteId { get; set; }
    public List<KLogDto> Rows { get; set; } = new();
    /// <summary>How many rows the note has.</summary>
    public int Total { get; set; }
    public int Skip { get; set; }
    /// <summary>The rows of this page whose version can be restored (H23, HistoryRules.Restorable).</summary>
    public List<Guid> Restorable { get; set; } = new();

    public bool HasMore => Skip + Rows.Count < Total;

    public bool CanRestore(Guid klogId) => Restorable?.Contains(klogId) == true;
}

/// <summary>
/// A note in «Recover deleted notes» (H24): what its row Deleted says (its number and topic as they were, the folder it was deleted
/// from, when and by whom), without its version. Row is that row, for «See this version».
/// </summary>
public class DeletedNoteDto
{
    public Guid NoteId { get; set; }
    public int NoteNumber { get; set; }
    public string Topic { get; set; }
    public string FolderPath { get; set; }
    public DateTime DeletedAt { get; set; }
    public string DeletedBy { get; set; }
    public string Origin { get; set; }
    public KLogDto Row { get; set; }
}

/// <summary>What recovering a deleted note did (H24): its number (another one if its own was taken) and what didn't come back.</summary>
public class NoteRecoverResultDto
{
    public Guid NoteId { get; set; }
    public int NoteNumber { get; set; }
    public string Topic { get; set; }
    public List<string> Notices { get; set; } = new();
}

/// <summary>The retention of the history (H25) and the day the history started (SYSTEM/HISTORY_START), for its editor.</summary>
public class HistoryRetentionInfoDto
{
    public HistoryRetentionDto Retention { get; set; } = new();
    public DateTime? Since { get; set; }
}

/// <summary>What restoring a version did (H23): Changed false if the note was already so; Notices, what couldn't be restored.</summary>
public class NoteRestoreResultDto
{
    public Guid NoteId { get; set; }
    public bool Changed { get; set; }
    public List<string> Notices { get; set; } = new();
}

/// <summary>
/// Two versions of a note side by side («Compare», H12, H22): what the change did (the note before and after that row) or that
/// version against the note now (WithCurrentNote). Its fields that differ, with their names resolved, and its text by lines, with
/// the equal lines far from a change folded and the words that changed in each line replaced (NoteComparer).
/// </summary>
public class NoteComparisonDto
{
    public Guid NoteId { get; set; }
    public Guid KLogId { get; set; }
    public int NoteNumber { get; set; }
    public bool WithCurrentNote { get; set; }
    /// <summary>The note before that change is still kept (or it is the first one): «what the change did» can be compared.</summary>
    public bool CanCompareChange { get; set; } = true;
    public string BeforeLabel { get; set; }
    public string AfterLabel { get; set; }
    public string Notice { get; set; }

    public List<NoteComparisonFieldDto> Fields { get; set; } = new();
    public NoteTextComparisonDto Text { get; set; } = new();

    public bool Same => Fields.Count == 0 && Text.Same;
}

public class NoteComparisonFieldDto
{
    public string Name { get; set; }
    public string Before { get; set; }
    public string After { get; set; }
}

public enum EnumComparisonLine
{
    Equal,
    Removed,
    Added,
    /// <summary>Equal lines folded: Skipped of them.</summary>
    Folded
}

public class NoteTextComparisonDto
{
    public bool Same { get; set; } = true;
    /// <summary>Too many changes to compare them line by line: all of it removed and added.</summary>
    public bool Replaced { get; set; }
    public int Added { get; set; }
    public int Removed { get; set; }
    /// <summary>Not every line is here (NoteComparer.MaxLinesShown).</summary>
    public bool Truncated { get; set; }
    public List<NoteComparisonLineDto> Lines { get; set; } = new();
}

public class NoteComparisonLineDto
{
    public EnumComparisonLine Kind { get; set; }
    /// <summary>Its number in the text before and after (1 the first; 0 if it isn't there).</summary>
    public int BeforeNumber { get; set; }
    public int AfterNumber { get; set; }
    public string Text { get; set; }
    /// <summary>The words that changed, when its pair (the line it replaces or that replaces it) is like it; null: the whole line.</summary>
    public List<NoteComparisonSegmentDto> Segments { get; set; }
    public int Skipped { get; set; }
}

public class NoteComparisonSegmentDto
{
    public string Text { get; set; }
    public bool Changed { get; set; }
}

/// <summary>
/// The note as it was after a change of its history («See this version», H18), ready to be shown: the names already resolved
/// (the path of its folder, its type, its attributes), not its script (H21). Its description as it was kept (each app shows it
/// as it shows the note's: DescriptionFormat). Exact false: it includes later changes, up to IncludesUntil (Notice says so).
/// </summary>
public class NoteVersionViewDto
{
    public Guid NoteId { get; set; }
    public Guid KLogId { get; set; }
    public int NoteNumber { get; set; }
    /// <summary>When the change happened (the last one of its session).</summary>
    public DateTime AsOf { get; set; }
    public string UserName { get; set; }
    public string Change { get; set; }

    public bool Exact { get; set; } = true;
    public DateTime? IncludesUntil { get; set; }
    public string Notice { get; set; }

    public string Topic { get; set; }
    public string FolderPath { get; set; }
    public string NoteTypeName { get; set; }
    public string Tags { get; set; }
    public int Priority { get; set; }
    /// <summary>How the description is shown: markdown, html or navigation (ContentTypeExt.ForDescription).</summary>
    public string DescriptionFormat { get; set; }
    public string Description { get; set; }
    public List<NoteVersionAttributeViewDto> Attributes { get; set; } = new();
}

public class NoteVersionAttributeViewDto
{
    public string Name { get; set; }
    public string Value { get; set; }
}

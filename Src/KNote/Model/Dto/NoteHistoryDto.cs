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

    public bool HasMore => Skip + Rows.Count < Total;
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

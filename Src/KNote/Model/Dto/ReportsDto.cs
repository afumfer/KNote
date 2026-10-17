using System;

namespace KNote.Model.Dto;

// A notes book reads and renders every note of the list: above this many notes, both apps ask the user first.
public static class LargeNotesBook
{
    public const int Notes = 100;

    public static bool Is(int notesCount) => notesCount > Notes;

    public static string Warning(int notesCount)
        => $"The book will have {notesCount} notes (one chapter each): generating it may take a long time.";
}

public enum NotesListReportSource
{
    Folder,
    Search,
    Filter
}

// A notes list of the web app to print or export (api/reports): the notes of a folder, a quick search or a
// filter, in the order the list shows them, with the columns of its view.
public class NotesListReportRequestDto
{
    public NotesListReportSource Source { get; set; }

    // Folder.
    public Guid? FolderId { get; set; }

    // Search.
    public string TextSearch { get; set; }

    // Filter (its paging and order are ignored: the whole list is printed, in OrderColumn/OrderAscending).
    public NotesFilterDto NotesFilter { get; set; }

    // The list's order: one of NoteOrderCriteria.SortableColumns, or null for the query's own order.
    public string OrderColumn { get; set; }

    public bool OrderAscending { get; set; } = true;

    // Compact view of the list: number, topic, priority and tags.
    public bool Compact { get; set; } = true;

    // The user's culture (e.g. "es-ES"): dates and numbers as the list shows them, the CSV separator.
    public string Culture { get; set; }
}

// A printable report: a standalone HTML page (A4, with its own styles) to show and print.
public class ReportDto
{
    public string Title { get; set; }

    public string Html { get; set; }

    // Proposed name (without extension) for the PDF saved from it.
    public string FileName { get; set; }

    public int NotesCount { get; set; }
}

// An exported file (e.g. a notes list as CSV) for the client to save.
public class ReportFileDto
{
    public string FileName { get; set; }

    public string ContentType { get; set; }

    public string Content { get; set; }
}

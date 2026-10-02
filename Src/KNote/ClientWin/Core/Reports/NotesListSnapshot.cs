namespace KNote.ClientWin.Core.Reports;

// A visible column of the notes list, in the order it is shown. Name is the bound property
// (NoteNumber, Topic, Tags...), Header the caption the user sees, Width its on-screen width (only its
// proportion to the other columns matters).
public record NotesListColumn(string Name, string Header, bool AlignRight, int Width);

// The notes list exactly as the user is seeing it: visible columns in display order and rows in display
// order (after sorting and the in-memory text filter), each cell as the text the grid shows for it.
// Taken by the view (see INotesListSnapshotProvider), since column layout, sort order and the text filter
// only live there; the print and CSV export use cases work from this, never from the raw loaded list.
public class NotesListSnapshot
{
    public List<NotesListColumn> Columns { get; } = new();

    // One entry per row, one cell per column of Columns (same order).
    public List<string[]> Rows { get; } = new();

    // The note of each row (same order as Rows), for the use cases that need more than the cell texts
    // (e.g. the notes book, which prints each note's description).
    public List<Guid> NoteIds { get; } = new();

    // Text of the in-memory filter applied over the loaded list (NotesSelectorCtrl.EnableTextFilter), or
    // empty when none is applied.
    public string TextFilter { get; set; } = "";

    // Notes loaded for the folder/search/filter, before the text filter.
    public int LoadedCount { get; set; }
}

namespace KNote.Client.AppStoreService;

/// <summary>
/// Where the user was in a notes list (Pages/Notes/NotesGrid), so the list can be shown again at the same place
/// when the user comes back to the page (saved by the grid when the page is left). ListKey tells which list it belongs to (e.g.
/// the folder of the tree panel); LastNoteId is the note that was selected.
/// </summary>
public class NotesListPosition
{
    public string ListKey { get; init; } = "";

    public double ScrollTop { get; set; }

    public Guid? LastNoteId { get; set; }

    // The order chosen in the list (null: the list's own order).
    public NotesOrder? Order { get; set; }
}

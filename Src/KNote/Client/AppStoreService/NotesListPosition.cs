namespace KNote.Client.AppStoreService;

/// <summary>
/// Where the user was in a notes list (Pages/Notes/NotesGrid), so the list can be shown again at the same place
/// after opening a note. ListKey tells which list it belongs to (e.g. the folder of the tree panel).
/// </summary>
public class NotesListPosition
{
    public string ListKey { get; init; } = "";

    public double ScrollTop { get; set; }

    public Guid? LastNoteId { get; set; }
}

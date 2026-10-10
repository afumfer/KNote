using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.Ai;

// What KNoteAiTools needs from the application it runs in: the parts of create_task that differ between
// ClientWin (default folder from its Store, the note is opened in the note editor) and Server (the
// repository's home folder, the Web client is told about the new note).
public interface IKNoteAiToolsHost
{
    // Where create_task saves the new note, or null when there is no such place configured.
    Task<AiNoteDestination> GetNewNoteDestinationAsync();

    // Called once the new note is saved, to show it to the user. Must not wait for the user: the tool
    // call returns right after it.
    void OnNoteCreated(IKntService service, NoteExtendedDto note);
}

public sealed record AiNoteDestination(IKntService Service, FolderInfoDto Folder);

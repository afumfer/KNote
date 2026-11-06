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

    // False if changing the note now would be lost: saving an editor open with it would overwrite the change.
    bool CanChangeNote(Guid noteId) => true;

    // An action of the assistant changed the note, to show it changed where it is seen.
    void OnNoteChanged(IKntService service, Guid noteId) { }
}

public sealed record AiNoteDestination(IKntService Service, FolderInfoDto Folder);

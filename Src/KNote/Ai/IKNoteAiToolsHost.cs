using KNote.Model;
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
    // call returns right after it. openEditor: create_task opens it; create_note does not, because a note open in an
    // editor cannot be changed and the model may go on completing it (open_note_editor does it when asked).
    void OnNoteCreated(IKntService service, NoteExtendedDto note, bool openEditor = true);

    // The user asked to see a note in its edit form. Must not wait for the user, like OnNoteCreated.
    void OpenNoteEditor(IKntService service, NoteMinimalDto note) { }

    // False if changing the note now would be lost: saving an editor open with it would overwrite the change.
    bool CanChangeNote(Guid noteId) => true;

    // An action of the assistant changed the note, to show it changed where it is seen.
    void OnNoteChanged(IKntService service, Guid noteId) { }

    // KNote Atlas 2.1b2b (process_document, D22): whether this application runs the extended summaries and the translations of the
    // documents in the background. Without it the tool is not offered.
    bool CanProcessDocuments => false;

    // The model of the repository for them (its recommended LLM, as the window «Extended summary»), or null if there is none.
    Task<AiProviderRef> GetDocumentModelAsync(IKntService service) => Task.FromResult<AiProviderRef>(null);

    // Starts one in the background, with that model: null if it started, or why not (another one going on...). Must not wait for it.
    Task<string> StartDocumentProcessAsync(IKntService service, AiProviderRef provider, DocumentProcessRequestDto request) =>
        Task.FromResult("This application can't summarize or translate documents from the assistant.");
}

public sealed record AiNoteDestination(IKntService Service, FolderInfoDto Folder);

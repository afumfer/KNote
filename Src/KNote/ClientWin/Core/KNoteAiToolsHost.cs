using KNote.Ai;
using KNote.ClientWin.Controllers;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Core;

// ClientWin's side of KNoteAiTools.create_task: the new note goes to Store.DefaultFolderWithServiceRef and,
// once saved, is opened in the note editor. A rare Core -> Controllers reference, the opposite of this
// codebase's usual direction, justified by needing to launch a full Ctrl+View pair, not just call a service
// method.
public class KNoteAiToolsHost : IKNoteAiToolsHost
{
    private readonly Store _store;

    // Captured at construction time, which always happens on the UI thread (KNoteAIAssistantCtrl builds it
    // when a provider is applied, only ever reached from UI event handlers). OnNoteCreated uses it to marshal
    // NoteEditorCtrl/Form construction back onto the UI thread, since by the time a tool call runs - deep
    // inside the OpenAI/Anthropic/Ollama SDK's own async internals - the SynchronizationContext may already
    // have been lost to a ConfigureAwait(false) somewhere in that chain.
    private readonly SynchronizationContext _uiContext;

    public KNoteAiToolsHost(Store store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _uiContext = SynchronizationContext.Current;
    }

    public Task<AiNoteDestination> GetNewNoteDestinationAsync()
    {
        var defaultFolderWithServiceRef = _store.DefaultFolderWithServiceRef;
        if (defaultFolderWithServiceRef?.ServiceRef == null || defaultFolderWithServiceRef.FolderInfo == null)
            return Task.FromResult<AiNoteDestination>(null);

        return Task.FromResult(new AiNoteDestination(
            defaultFolderWithServiceRef.ServiceRef.Service, defaultFolderWithServiceRef.FolderInfo));
    }

    // Fire-and-forget: the note is already saved by the time this runs, so the tool doesn't need to
    // wait for the user to close the editor - it only needs to trigger showing it.
    public void OnNoteCreated(IKntService service, NoteExtendedDto note)
    {
        void Show() => _ = ShowNoteForEditingAsync(service, note.NoteId);

        // Marshal onto the UI thread before touching NoteEditorCtrl/Form - see the _uiContext comment
        // on the field for why this can't just call Show() directly.
        if (_uiContext != null)
            _uiContext.Post(_ => Show(), null);
        else
            Show();
    }

    private async Task ShowNoteForEditingAsync(IKntService service, Guid noteId)
    {
        // The same LoadModelById(service, id) + Run() the rest of the app uses to open an existing
        // note for editing (e.g. double-clicking a note in the tree) - NoteEditorCtrl/its view are
        // used exactly as designed, unmodified, with no direct access to view members from here.
        var noteEditor = new NoteEditorCtrl(_store);
        var loaded = await noteEditor.LoadModelById(service, noteId);
        if (loaded)
            noteEditor.Run();
    }
}

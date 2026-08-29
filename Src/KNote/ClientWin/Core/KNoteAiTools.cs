using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Threading;
using KNote.ClientWin.Controllers;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using Microsoft.Extensions.AI;

namespace KNote.ClientWin.Core;

// KNoteAIAssistant plan (Phase 5): function-calling surface exposed to the assistant's IChatClient
// (wired in AiChatClientFactory.Create). Wraps the Service layer - never Repository directly, per
// ClientWin convention - so the model can act on the user's active KNote repository (the same
// service the assistant was opened against, not the separate "assistant" repository used for
// the prompt/system-prompt catalog). Takes IKntService directly (not ServiceRef) so it can be
// exercised in ClientWin.Tests against the existing FakeKntService/FakeKntNoteService test doubles
// without a real database.
//
// create_task additionally needs Store (to reach Store.DefaultFolderWithServiceRef and to
// construct a NoteEditorCtrl) - a rare Core -> Controllers reference, the opposite of this
// codebase's usual direction, justified by needing to launch a full Ctrl+View pair, not just call
// a service method.
public class KNoteAiTools
{
    private const int MaxResults = 20;

    private readonly IKntService _service;
    private readonly Store _store;

    // Captured at construction time, which always happens on the UI thread (AiChatClientFactory.Create
    // is only ever called from KNoteAIAssistantCtrl.SetProvider, itself only reached from UI event
    // handlers). create_task uses it to marshal NoteEditorCtrl/Form construction back onto the UI
    // thread, since by the time a tool call runs - deep inside the OpenAI/Anthropic/Ollama SDK's own
    // async internals - the SynchronizationContext may already have been lost to a ConfigureAwait(false)
    // somewhere in that chain.
    private readonly SynchronizationContext _uiContext;

    public KNoteAiTools(IKntService service, Store store)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _uiContext = SynchronizationContext.Current;
    }

    public IEnumerable<AITool> GetTools()
    {
        yield return AIFunctionFactory.Create(SearchNotesAsync, new AIFunctionFactoryOptions
        {
            Name = "search_notes",
            Description = "Full-text search over the notes in the user's active KNote repository. " +
                "Matches are made against each note's topic and tags by default, and additionally " +
                "against the note's body/content when includeContent is true. Returns, as one JSON " +
                "object per line, each matching note's full metadata: NoteId (Guid, use it with other " +
                "KNote tools that need to identify a specific note), NoteNumber (the short numeric id " +
                "users refer to as \"#123\"), Topic, Tags, InternalTags, Priority, CreationDateTime, " +
                "ModificationDateTime and FolderId - not just topic and tags. Use it to find notes the " +
                "user refers to before answering questions about their content, to check whether a note " +
                "about something already exists, or to answer follow-up questions about a note's number, " +
                "dates, priority or folder without a further lookup."
        });

        yield return AIFunctionFactory.Create(GetNoteDetailsAsync, new AIFunctionFactoryOptions
        {
            Name = "get_note_details",
            Description = "Gets the full detail of a single note from the user's active KNote repository, " +
                "including its Description (the note's full body/content), Tags, InternalTags, Priority, " +
                "dates, FolderDto, NoteTypeDto and any KAttributesDto (custom attributes) - everything " +
                "search_notes does not already return. Look the note up first with search_notes to get its " +
                "NoteId or NoteNumber, then call this to read what's actually inside it."
        });

        yield return AIFunctionFactory.Create(CreateTaskAsync, new AIFunctionFactoryOptions
        {
            Name = "create_task",
            Description = "Creates and saves a new KNote note/task from something the user asked to " +
                "remember, note down, or turn into a task/reminder, then opens it in KNote's note editor " +
                "for the user to see. Unlike a draft, this note is already persisted when the editor opens " +
                "- from there it's entirely up to the user to modify and re-save it, leave it as-is, or " +
                "delete it. Always determine a short topic and a full description from the user's request " +
                "before calling this. The note is created in the application's default repository and " +
                "folder (Store.DefaultFolderWithServiceRef), which may be different from whatever " +
                "repository search_notes/get_note_details are currently operating on."
        });
    }

    [Description("Full-text search over the notes in the user's active KNote repository (topic, tags, and optionally the note body).")]
    private async Task<string> SearchNotesAsync(
        [Description(
            "The search query: one or more space-separated words. A note matches only if it contains ALL " +
            "of the words (logical AND across words), each looked up as a substring across the searched " +
            "fields (topic, tags, and content when includeContent is true) with a logical OR between those " +
            "fields for that word. Prefix a word with '!' to instead require that it is NOT present " +
            "(exclusion), e.g. \"invoice !draft\" finds notes mentioning 'invoice' but not 'draft'. Wrap a " +
            "multi-word phrase in double quotes to search for it as a unit, e.g. \"\\\"project plan\\\"\". " +
            "To look up a single note by its number instead of doing a text search, pass \"#\" followed by " +
            "the number, e.g. \"#123\".")]
        string textSearch,
        [Description("Also search inside the note's body/content, not just its topic and tags. Defaults to false (topic and tags only).")]
        bool includeContent = false)
    {
        var search = new NotesSearchDto
        {
            TextSearch = textSearch,
            SearchInDescription = includeContent
        };

        var response = await _service.Notes.GetSearchMinimalAsync(search);

        if (!response.IsValid)
            return $"Error searching notes: {response.ErrorMessage}";

        if (response.Entity == null || response.Entity.Count == 0)
            return "No notes found matching the given search.";

        // One JSON object per line (NoteMinimalDto's full set of fields) rather than a hand-picked
        // subset, so the model has NoteNumber/dates/priority/folder available for follow-up
        // questions without another round trip.
        var sb = new StringBuilder();
        foreach (var note in response.Entity.Take(MaxResults))
            sb.AppendLine(JsonSerializer.Serialize(note));

        if (response.Entity.Count > MaxResults)
            sb.AppendLine($"... and {response.Entity.Count - MaxResults} more, not shown.");

        return sb.ToString();
    }

    [Description("Gets the full detail of a single note (including its Description/content) by NoteId or NoteNumber.")]
    private async Task<string> GetNoteDetailsAsync(
        [Description("The note's NoteId (a GUID), as returned by search_notes. Provide either noteId or noteNumber, not both.")]
        string noteId = null,
        [Description("The note's NoteNumber (a short integer, e.g. 123), as returned by search_notes. Provide either noteId or noteNumber, not both.")]
        int? noteNumber = null)
    {
        if (string.IsNullOrEmpty(noteId) && noteNumber == null)
            return "Error: provide either noteId or noteNumber.";

        var response = !string.IsNullOrEmpty(noteId)
            ? Guid.TryParse(noteId, out var id)
                ? await _service.Notes.GetAsync(id)
                : null
            : await _service.Notes.GetAsync(noteNumber.Value);

        if (response == null)
            return $"Error: '{noteId}' is not a valid noteId (expected a GUID).";

        if (!response.IsValid)
            return $"Error getting note: {response.ErrorMessage}";

        if (response.Entity == null)
            return "Note not found.";

        return JsonSerializer.Serialize(response.Entity);
    }

    [Description("Creates and saves a new note/task with the given topic and description, then opens it in the note editor for the user to see - already persisted, not a draft.")]
    private async Task<string> CreateTaskAsync(
        [Description("Short title for the note/task, determined from the user's request.")]
        string topic,
        [Description("The task/note body: the content the user asked to note down, written out in full.")]
        string description)
    {
        if (string.IsNullOrWhiteSpace(topic))
            return "Error: topic is required and cannot be empty.";

        var defaultFolderWithServiceRef = _store.DefaultFolderWithServiceRef;
        if (defaultFolderWithServiceRef?.ServiceRef == null || defaultFolderWithServiceRef.FolderInfo == null)
            return "Error: no default repository/folder is configured in this KNote instance.";

        var service = defaultFolderWithServiceRef.ServiceRef.Service;

        // Persisted through the Service layer only - same as search_notes/get_note_details - never
        // through NoteEditorCtrl or its view. NewExtendedAsync gives the same defaults (note type,
        // attribute completion) NoteEditorCtrl.NewModel itself gets from the same call - but, like
        // NoteEditorCtrl.NewModel, it still has to fill in Tags itself: NewExtendedAsync leaves it
        // null by design, and KntNotesSaveExtendedAsyncCommand.Execute unconditionally calls
        // Param.Tags.Contains(...) - a null Tags throws a NullReferenceException on save.
        var newNoteResponse = await service.Notes.NewExtendedAsync();
        if (!newNoteResponse.IsValid)
            return $"Error creating note: {newNoteResponse.ErrorMessage}";

        var note = newNoteResponse.Entity;
        note.Topic = topic;
        note.Description = description ?? "";
        note.Tags = "";
        note.FolderId = defaultFolderWithServiceRef.FolderInfo.FolderId;
        note.FolderDto = defaultFolderWithServiceRef.FolderInfo.GetSimpleDto<FolderDto>();

        var saveResponse = await service.Notes.SaveExtendedAsync(note);
        if (!saveResponse.IsValid)
            return $"Error saving note: {saveResponse.ErrorMessage}";

        ShowNoteForEditing(service, saveResponse.Entity.NoteId);

        return $"Created and saved a new note titled \"{topic}\" (note #{saveResponse.Entity.NoteNumber}), " +
            "and opened it in the KNote editor for the user to see. From here it is entirely the user's " +
            "responsibility to modify and re-save it, leave it as it is, or delete it.";
    }

    // Fire-and-forget: the note is already saved by the time this runs, so the tool doesn't need to
    // wait for the user to close the editor - it only needs to trigger showing it.
    private void ShowNoteForEditing(IKntService service, Guid noteId)
    {
        void Show() => _ = ShowNoteForEditingAsync(service, noteId);

        // Marshal onto the UI thread before touching NoteEditorCtrl/Form - see the _uiContext comment
        // on the constructor for why this can't just call Show() directly.
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

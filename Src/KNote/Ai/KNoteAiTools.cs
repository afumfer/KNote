using System.ComponentModel;
using System.Text;
using System.Text.Json;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using Microsoft.Extensions.AI;

namespace KNote.Ai;

// Function-calling surface exposed to the assistant's IChatClient (passed to AiChatClientFactory.Create).
// Wraps the Service layer - never Repository directly - so the model can act on the user's KNote
// repository (the same service the assistant works against). Takes IKntService directly (not ServiceRef)
// so it can be exercised in tests against the existing FakeKntService/FakeKntNoteService test doubles
// without a real database. What create_task does that depends on the application (where the note goes,
// how it is shown) comes from the IKNoteAiToolsHost.
public partial class KNoteAiTools
{
    private const int MaxResults = 20;

    // The text of a note get_note_details gives at most: one note of hundreds of thousands of characters would fill the
    // context of a local model, which then loses part of it without saying so. A notice tells the model the note goes on.
    public const int MaxNoteTextLength = 40000;

    private readonly IKntService _service;
    private readonly IKNoteAiToolsHost _host;

    public KNoteAiTools(IKntService service, IKNoteAiToolsHost host)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _host = host ?? throw new ArgumentNullException(nameof(host));
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
                "dates, priority or folder without a further lookup. KNote's own notes (such as the saved " +
                "chat sessions) are not included."
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
                "remember, note down, or turn into a task/reminder, then shows it to the user. Unlike a " +
                "draft, this note is already persisted when it is shown - from there it's entirely up to " +
                "the user to modify and re-save it, leave it as-is, or delete it. Always determine a short " +
                "topic and a full description from the user's request before calling this. The note is " +
                "created in the application's default repository and folder, which may be different from " +
                "whatever repository search_notes/get_note_details are currently operating on."
        });

        foreach (var tool in GetQueryTools())
            yield return tool;

        foreach (var tool in GetActionTools())
            yield return tool;
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
            SearchInDescription = includeContent,
            ExcludeNoteTypeIds = await GetExcludedNoteTypeIdsAsync()
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

    // The system notes (KntConst.SystemNoteTypeNames) are left out of the tools: they are not the user's notes, and other
    // users' AI sessions must not reach the model.
    private async Task<List<Guid>> GetExcludedNoteTypeIdsAsync()
    {
        var response = await _service.NoteTypes.GetAllAsync();
        if (!response.IsValid)
            return null;

        var excluded = response.Entity?
            .Where(t => KntConst.IsSystemNoteType(t.Name))
            .Select(t => t.NoteTypeId)
            .ToList();
        return excluded?.Count > 0 ? excluded : null;
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

        if (response.Entity == null || KntConst.IsSystemNoteType(response.Entity.NoteTypeDto?.Name))
            return "Note not found.";

        var note = response.Entity;
        var fullLength = note.Description?.Length ?? 0;
        if (fullLength > MaxNoteTextLength)
            note.Description = note.Description.Substring(0, MaxNoteTextLength);

        // Before and after the text: at the end only of a text so long, small models overlooked it.
        var notice = fullLength > MaxNoteTextLength
            ? $"[IMPORTANT: this note was cut: only the first {MaxNoteTextLength} of its {fullLength} characters are shown. " +
              "Tell the user that you have read only the beginning of the note.]"
            : null;

        var details = new StringBuilder();
        if (notice != null)
            details.Append(notice).Append('\n');
        details.Append(JsonSerializer.Serialize(note));
        if (notice != null)
            details.Append('\n').Append(notice);

        var terms = await FindGlossaryTermsAsync(note);
        if (terms.Count > 0)
            details.Append("\nTerms of the glossary in this note:\n").Append(GlossaryLines(terms));

        return details.ToString();
    }

    // D7: the terms of the glossary in what the model reads. Without them the note is still read.
    private async Task<List<GlossaryTermDto>> FindGlossaryTermsAsync(NoteDto note)
    {
        try
        {
            var response = await _service.AtlasContext.FindGlossaryTermsAsync(new List<string> { note.Topic, note.Description });
            return response.IsValid && response.Entity != null ? response.Entity : new List<GlossaryTermDto>();
        }
        catch (Exception)
        {
            return new List<GlossaryTermDto>();
        }
    }

    [Description("Creates and saves a new note/task with the given topic and description, then shows it to the user - already persisted, not a draft.")]
    private async Task<string> CreateTaskAsync(
        [Description("Short title for the note/task, determined from the user's request.")]
        string topic,
        [Description("The task/note body: the content the user asked to note down, written out in full.")]
        string description)
    {
        if (string.IsNullOrWhiteSpace(topic))
            return "Error: topic is required and cannot be empty.";

        var destination = await _host.GetNewNoteDestinationAsync();
        if (destination?.Service == null || destination.Folder == null)
            return "Error: no default repository/folder is configured in this KNote instance.";

        var service = destination.Service;

        // Persisted through the Service layer only - same as search_notes/get_note_details. NewExtendedAsync
        // gives the same defaults (note type, attribute completion) the note editors get from the same call -
        // but, like them, Tags still has to be filled in here: NewExtendedAsync leaves it null by design, and
        // KntNotesSaveExtendedAsyncCommand.Execute unconditionally calls Param.Tags.Contains(...) - a null
        // Tags throws a NullReferenceException on save.
        var newNoteResponse = await service.Notes.NewExtendedAsync();
        if (!newNoteResponse.IsValid)
            return $"Error creating note: {newNoteResponse.ErrorMessage}";

        var note = newNoteResponse.Entity;
        note.Topic = topic;
        note.Description = description ?? "";
        note.Tags = "";
        note.FolderId = destination.Folder.FolderId;
        note.FolderDto = destination.Folder.GetSimpleDto<FolderDto>();

        var saveResponse = await service.Notes.SaveExtendedAsync(note);
        if (!saveResponse.IsValid)
            return $"Error saving note: {saveResponse.ErrorMessage}";

        _host.OnNoteCreated(service, saveResponse.Entity);

        TurnActions.Add(new AiActionDto
        {
            Kind = AiActionRules.CreateTask,
            Status = EnumAiActionStatus.Done,
            Text = $"Created the note #{saveResponse.Entity.NoteNumber} ({topic})",
            NoteId = saveResponse.Entity.NoteId,
            NoteNumber = saveResponse.Entity.NoteNumber
        });
        _newNoteIds.Add(saveResponse.Entity.NoteId);

        return $"Created and saved a new note titled \"{topic}\" (note #{saveResponse.Entity.NoteNumber}), " +
            "and showed it to the user. From here it is entirely the user's responsibility to modify and " +
            "re-save it, leave it as it is, or delete it.";
    }
}

using System.ComponentModel;
using System.Text;
using System.Text.Json;
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
public class KNoteAiTools
{
    private const int MaxResults = 20;

    private readonly IKntService _service;

    public KNoteAiTools(IKntService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
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
}

using System.ComponentModel;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using Microsoft.Extensions.AI;

namespace KNote.Ai;

// The tools that act on the repository (KNote Atlas, D2, D10). What needs the user's confirmation does nothing here: the tool
// adds it to the actions of the turn (TurnActions) as proposed and tells the model so; when the turn ends the app shows the
// card, and what is accepted is run by ExecuteActionsAsync, not by the model. Creating something new is done at once, and so
// is what the user approved for this conversation or a change to a note created in it.
public partial class KNoteAiTools
{
    private const int MaxCardText = 500;

    private readonly HashSet<string> _autoApprovedKinds = new();
    private readonly HashSet<Guid> _newNoteIds = new();
    private List<AiActionDto> _earlierActions = new();

    // The actions of the running turn, for the turn that the app keeps.
    public List<AiActionDto> TurnActions { get; private set; } = new();

    // Before each turn: the Server keeps nothing between them, so all it depends on comes with the conversation.
    public void BeginTurn(IEnumerable<AiChatTurnDto> history, IEnumerable<string> autoApprovedKinds)
    {
        TurnActions = new List<AiActionDto>();
        _earlierActions = (history ?? Enumerable.Empty<AiChatTurnDto>()).SelectMany(t => t.Actions).ToList();
        _autoApprovedKinds.Clear();
        _autoApprovedKinds.UnionWith(autoApprovedKinds ?? Enumerable.Empty<string>());
        _newNoteIds.Clear();
        _newNoteIds.UnionWith(AiActionRules.NewNoteIds(history));
    }

    // What the user accepted in the card, one by one: one that fails does not stop the others.
    public async Task ExecuteActionsAsync(IEnumerable<AiActionDto> actions)
    {
        foreach (var action in actions)
            await ExecuteActionAsync(_service, action);
    }

    private async Task ExecuteActionAsync(IKntService service, AiActionDto action)
    {
        if (action.NoteId is Guid noteId && !_host.CanChangeNote(noteId))
        {
            action.Status = EnumAiActionStatus.Failed;
            action.Result = "The note is open in an editor: close it and ask again.";
            return;
        }

        await KntAiActionExecutor.ExecuteAsync(service, action);

        if (action.Status == EnumAiActionStatus.Done && action.Kind != AiActionRules.CreateNote && action.NoteId is Guid changed)
            _host.OnNoteChanged(service, changed);
    }

    private IEnumerable<AITool> GetActionTools()
    {
        yield return AIFunctionFactory.Create(CreateNoteAsync, new AIFunctionFactoryOptions
        {
            Name = AiActionRules.CreateNote,
            Description = "Creates and saves a new note, in the folder asked for or else in the default one, then shows it to the " +
                "user. Use it when the user asks to write something down in a note of a folder; for a quick task or reminder, " +
                "create_task. It is done at once."
        });

        yield return AIFunctionFactory.Create(AppendToNoteAsync, new AIFunctionFactoryOptions
        {
            Name = AiActionRules.AppendToNote,
            Description = "Adds text at the end of an existing note, without touching what it has. It is proposed to the user, who " +
                "has to confirm it: until then it is not done."
        });

        yield return AIFunctionFactory.Create(SetTagsAsync, new AIFunctionFactoryOptions
        {
            Name = AiActionRules.SetTags,
            Description = "Adds tags to an existing note or removes them. It is proposed to the user, who has to confirm it: until " +
                "then it is not done."
        });
    }

    private async Task<bool> CanActAsync()
    {
        try
        {
            return await _service.GetCurrentUserRoleAsync() >= AiActionRules.MinRole;
        }
        catch (Exception)
        {
            return false;
        }
    }

    [Description("Creates and saves a new note.")]
    private async Task<string> CreateNoteAsync(
        [Description("Short title of the note.")]
        string topic,
        [Description("The text of the note, in Markdown, written out in full.")]
        string content,
        [Description("The folder of the note: its name, its path or its number. By default, the default folder.")]
        string folder = null,
        [Description("Tags of the note.")]
        string[] tags = null)
    {
        if (string.IsNullOrWhiteSpace(topic))
            return "Error: topic is required and cannot be empty.";

        IKntService service;
        FolderInfoDto target;
        if (!string.IsNullOrWhiteSpace(folder))
        {
            var (folders, foldersError) = await GetFoldersAsync();
            if (foldersError != null)
                return foldersError;
            var (found, findError) = folders.Find(folder);
            if (findError != null)
                return findError;
            (service, target) = (_service, found);
        }
        else
        {
            var destination = await _host.GetNewNoteDestinationAsync();
            if (destination?.Service == null || destination.Folder == null)
                return "Error: no default repository/folder is configured in this KNote instance.";
            (service, target) = (destination.Service, destination.Folder);
        }

        var action = new AiActionDto
        {
            Kind = AiActionRules.CreateNote,
            Text = $"Create the note ({topic})",
            Topic = topic,
            Content = content,
            FolderId = target.FolderId,
            AddTags = tags?.ToList()
        };
        TurnActions.Add(action);
        await ExecuteActionAsync(service, action);
        if (action.Status != EnumAiActionStatus.Done)
            return $"Error: {action.Result}";

        _newNoteIds.Add(action.NoteId.Value);
        _host.OnNoteCreated(service, new NoteExtendedDto { NoteId = action.NoteId.Value, NoteNumber = action.NoteNumber.Value, Topic = topic });
        return $"Done: created the note #{action.NoteNumber}, and showed it to the user.";
    }

    [Description("Proposes adding text at the end of a note.")]
    private async Task<string> AppendToNoteAsync(
        [Description("The note's NoteNumber (a short integer, e.g. 123).")]
        int noteNumber,
        [Description("The text to add, in Markdown, written out in full.")]
        string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return "Error: content is required and cannot be empty.";

        var (note, error) = await FindChangeableNoteAsync(noteNumber);
        if (error != null)
            return error;

        var shown = content.Length > MaxCardText ? content[..MaxCardText] + "…" : content;
        return await ProposeAsync(new AiActionDto
        {
            Kind = AiActionRules.AppendToNote,
            Text = $"Add to #{noteNumber} ({note.Topic}): {shown}",
            NoteId = note.NoteId,
            NoteNumber = noteNumber,
            Content = content
        });
    }

    [Description("Proposes adding or removing tags of a note.")]
    private async Task<string> SetTagsAsync(
        [Description("The note's NoteNumber (a short integer, e.g. 123).")]
        int noteNumber,
        [Description("The tags to add.")]
        string[] add = null,
        [Description("The tags to remove.")]
        string[] remove = null)
    {
        var (note, error) = await FindChangeableNoteAsync(noteNumber);
        if (error != null)
            return error;

        if (AiActionRules.ChangeTags(note.Tags, add, remove) == AiActionRules.ChangeTags(note.Tags, null, null))
            return "Error: that does not change the tags of the note.";

        var changes = new List<string>();
        if (add?.Length > 0)
            changes.Add($"add {string.Join(", ", add.Select(t => $"*{t}*"))}");
        if (remove?.Length > 0)
            changes.Add($"remove {string.Join(", ", remove.Select(t => $"*{t}*"))}");

        return await ProposeAsync(new AiActionDto
        {
            Kind = AiActionRules.SetTags,
            Text = $"Tags of #{noteNumber} ({note.Topic}): {string.Join(" and ", changes)}",
            NoteId = note.NoteId,
            NoteNumber = noteNumber,
            AddTags = add?.ToList(),
            RemoveTags = remove?.ToList()
        });
    }

    private async Task<(NoteDto Note, string Error)> FindChangeableNoteAsync(int noteNumber)
    {
        var cited = await _service.Notes.GetCitedNoteAsync(noteNumber);
        if (!cited.IsValid)
            return (null, $"Error getting note: {cited.ErrorMessage}");
        if (cited.Entity == null)
            return (null, $"Error: {KntAiActionExecutor.NoteNotFound}");

        var (note, error) = await KntAiActionExecutor.GetChangeableNoteAsync(_service, cited.Entity.NoteId);
        return error != null ? (null, $"Error: {error}") : (note, null);
    }

    private async Task<string> ProposeAsync(AiActionDto action)
    {
        if (AiActionRules.IsRepeated(action, TurnActions, _earlierActions))
            return $"Nothing to do, it was already proposed or done: {action.Text}. Do not call this tool again for it.";

        TurnActions.Add(action);

        if (AiActionRules.NeedsConfirmation(action.Kind) && !_autoApprovedKinds.Contains(action.Kind)
            && !(action.NoteId is Guid noteId && _newNoteIds.Contains(noteId)))
            return $"Proposed, NOT done yet: {action.Text}. KNote is showing the user a card with Accept and Reject buttons for it. " +
                "Do not ask anything about it: just say, in one short line, that it is waiting in the card.";

        await ExecuteActionAsync(_service, action);
        return action.Status == EnumAiActionStatus.Done ? $"Done: {action.Text}" : $"Error: {action.Result}";
    }
}

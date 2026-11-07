using System.ComponentModel;
using System.Globalization;
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
    private const string EmailAlarmNotice = " (sent only if KNote's email alarms are on)";

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
            Description = "Creates and saves a new note, in the folder asked for or else in the default one. Use it when the user " +
                "asks to write something down in a note of a folder; for a quick task or reminder, create_task. It is done at once."
        });

        yield return AIFunctionFactory.Create(AppendToNoteAsync, new AIFunctionFactoryOptions
        {
            Name = AiActionRules.AppendToNote,
            Description = "Adds text at the end of an existing note, without touching what it has. To change what it has, " +
                AiActionRules.ReplaceNoteText + ". It is proposed to the user, who has to confirm it: until then it is not done."
        });

        yield return AIFunctionFactory.Create(ReplaceNoteTextAsync, new AIFunctionFactoryOptions
        {
            Name = AiActionRules.ReplaceNoteText,
            Description = "Replaces the whole text of an existing note with a new one: what it has now is lost. Only when the user " +
                "asks to replace or rewrite the text of a note. It is proposed to the user, who has to confirm it: until then it " +
                "is not done."
        });

        yield return AIFunctionFactory.Create(MoveNoteAsync, new AIFunctionFactoryOptions
        {
            Name = AiActionRules.MoveNote,
            Description = "Moves an existing note to another folder. It is proposed to the user, who has to confirm it: until then " +
                "it is not done."
        });

        yield return AIFunctionFactory.Create(OpenNoteEditorAsync, new AIFunctionFactoryOptions
        {
            Name = AiActionRules.OpenNoteEditor,
            Description = "Opens the edit form of an existing note, for the user to see it or change it by hand. Use it when the " +
                "user asks to open a note or to see it in its form; to open a new one, create it first with create_note. It " +
                "changes nothing and it is done at once."
        });

        yield return AIFunctionFactory.Create(SetTagsAsync, new AIFunctionFactoryOptions
        {
            Name = AiActionRules.SetTags,
            Description = "Adds tags to an existing note or removes them. It is proposed to the user, who has to confirm it: until " +
                "then it is not done."
        });

        yield return AIFunctionFactory.Create(AddTaskAsync, new AIFunctionFactoryOptions
        {
            Name = AiActionRules.AddTask,
            Description = "Adds a task to an existing note, for the user or for somebody else. For a task that belongs to no note, " +
                "create_task. It is proposed to the user, who has to confirm it: until then it is not done."
        });

        yield return AIFunctionFactory.Create(AddAlarmAsync, new AIFunctionFactoryOptions
        {
            Name = AiActionRules.AddAlarm,
            Description = "Adds to a note an alarm that KNote shows to the user at a date and time. To send it by email, " +
                AiActionRules.AddEmailAlarm + ". It is proposed to the user, who has to confirm it: until then it is not done."
        });

        yield return AIFunctionFactory.Create(AddEmailAlarmAsync, new AIFunctionFactoryOptions
        {
            Name = AiActionRules.AddEmailAlarm,
            Description = "Adds to a note an alarm that is sent to the user by email at a date and time. Only when the user asks for " +
                "an email. It is proposed to the user, who has to confirm it: until then it is not done."
        });

        yield return AIFunctionFactory.Create(SetAttributeAsync, new AIFunctionFactoryOptions
        {
            Name = AiActionRules.SetAttribute,
            Description = "Changes the value of an attribute of an existing note. It is proposed to the user, who has to confirm it: " +
                "until then it is not done."
        });

        yield return AIFunctionFactory.Create(RelateNotesAsync, new AIFunctionFactoryOptions
        {
            Name = AiActionRules.RelateNotes,
            Description = "Relates a note to another one. It is proposed to the user, who has to confirm it: until then it is not done."
        });

        yield return AIFunctionFactory.Create(CreateFolderAsync, new AIFunctionFactoryOptions
        {
            Name = AiActionRules.CreateFolder,
            Description = "Creates a new folder, inside the one asked for or else inside Home. It is proposed to the user, who has to " +
                "confirm it: until then it is not done, and a note cannot go in it yet."
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
        if (AiActionRules.IsRepeated(action, TurnActions, _earlierActions))
            return Repeated(action);

        TurnActions.Add(action);
        await ExecuteActionAsync(service, action);
        if (action.Status != EnumAiActionStatus.Done)
            return $"Error: {action.Result}";

        _newNoteIds.Add(action.NoteId.Value);
        _host.OnNoteCreated(service, new NoteExtendedDto { NoteId = action.NoteId.Value, NoteNumber = action.NoteNumber.Value, Topic = topic }, openEditor: false);
        return $"Done: created the note #{action.NoteNumber}. It is saved, and its edit form is not open: call {AiActionRules.OpenNoteEditor} " +
            "only if the user asked to open or see it.";
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

        return await ProposeAsync(new AiActionDto
        {
            Kind = AiActionRules.AppendToNote,
            Text = $"Add to #{noteNumber} ({note.Topic}): {Shown(content)}",
            NoteId = note.NoteId,
            NoteNumber = noteNumber,
            Content = content
        });
    }

    [Description("Proposes replacing the whole text of a note.")]
    private async Task<string> ReplaceNoteTextAsync(
        [Description("The note's NoteNumber (a short integer, e.g. 123).")]
        int noteNumber,
        [Description("The whole new text of the note, in Markdown, written out in full: what is not here is lost.")]
        string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return "Error: content is required and cannot be empty.";

        var (note, error) = await FindChangeableNoteAsync(noteNumber);
        if (error != null)
            return error;

        var stamp = AiActionRules.TextStamp(note.Description);
        var text = KntAiActionExecutor.NoteText(_service, note, content);
        if (AiActionRules.TextStamp(text) == stamp)
            return $"Error: the note #{noteNumber} already has that text.";

        return await ProposeAsync(new AiActionDto
        {
            Kind = AiActionRules.ReplaceNoteText,
            Text = $"Replace the whole text of #{noteNumber} ({note.Topic}), {note.Description?.Length ?? 0} → {text.Length} characters",
            NoteId = note.NoteId,
            NoteNumber = noteNumber,
            Content = content,
            TextStamp = stamp
        });
    }

    [Description("Proposes moving a note to another folder.")]
    private async Task<string> MoveNoteAsync(
        [Description("The note's NoteNumber (a short integer, e.g. 123).")]
        int noteNumber,
        [Description("The folder it goes to: its name, its path or its number.")]
        string folder)
    {
        var (note, error) = await FindChangeableNoteAsync(noteNumber);
        if (error != null)
            return error;

        var (folders, foldersError) = await GetFoldersAsync();
        if (foldersError != null)
            return foldersError;

        var (target, findError) = folders.Find(folder);
        if (findError != null)
            return findError;
        if (target.FolderId == note.FolderId)
            return $"Error: the note #{noteNumber} is already in {folders.Path(target.FolderId)}.";

        return await ProposeAsync(new AiActionDto
        {
            Kind = AiActionRules.MoveNote,
            Text = $"Move #{noteNumber} ({note.Topic}) from {folders.Path(note.FolderId)} to {folders.Path(target.FolderId)}",
            NoteId = note.NoteId,
            NoteNumber = noteNumber,
            FolderId = target.FolderId
        });
    }

    [Description("Opens the edit form of a note.")]
    private async Task<string> OpenNoteEditorAsync(
        [Description("The note's NoteNumber (a short integer, e.g. 123).")]
        int noteNumber)
    {
        var (note, error) = await FindCitedNoteAsync(noteNumber);
        if (error != null)
            return error;

        _host.OpenNoteEditor(_service, note);
        return $"Done: the edit form of the note #{noteNumber} ({note.Topic}) is open for the user. Just say so, in one short line.";
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
        if (!(add?.Length > 0) && !(remove?.Length > 0))
            return "Error: this call has no tags. Say which ones in add or in remove.";

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

    [Description("Proposes adding a task to a note.")]
    private async Task<string> AddTaskAsync(
        [Description("The note's NoteNumber (a short integer, e.g. 123).")]
        int noteNumber,
        [Description("What has to be done.")]
        string description,
        [Description("Who does it: a user name or a full name. By default, the user of this conversation.")]
        string user = null,
        [Description("When it should be finished, as yyyy-MM-dd.")]
        string dueDate = null)
    {
        if (string.IsNullOrWhiteSpace(description))
            return "Error: description is required and cannot be empty.";

        var (note, error) = await FindChangeableNoteAsync(noteNumber);
        if (error != null)
            return error;

        var (assignee, userError) = await FindAssigneeAsync(user);
        if (userError != null)
            return userError;

        DateTime? due = null;
        if (!string.IsNullOrWhiteSpace(dueDate))
        {
            if (!DateTime.TryParse(dueDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return $"Error: '{dueDate}' is not a date. Use yyyy-MM-dd.";
            due = date;
        }

        var by = due != null ? $", by {AiActionRules.DateText(due.Value, withTime: false)}" : "";
        return await ProposeAsync(new AiActionDto
        {
            Kind = AiActionRules.AddTask,
            Text = $"Task on #{noteNumber} ({note.Topic}) for {UserText(assignee)}{by}: {Shown(description)}",
            NoteId = note.NoteId,
            NoteNumber = noteNumber,
            UserId = assignee.UserId,
            Content = description,
            Date = due
        });
    }

    [Description("Proposes adding an alarm to a note.")]
    private Task<string> AddAlarmAsync(
        [Description("The note's NoteNumber (a short integer, e.g. 123).")]
        int noteNumber,
        [Description("The text of the alarm.")]
        string message,
        [Description("When it goes off, as yyyy-MM-dd HH:mm. Work out the date from today's.")]
        string when = null,
        [Description("Instead of when: in how many minutes from now it goes off, for \"in two hours\".")]
        int? minutesFromNow = null,
        [Description("How it repeats: none, daily, weekly, monthly or yearly. By default, none.")]
        string repeat = null) =>
        ProposeAlarmAsync(AiActionRules.AddAlarm, "Alarm", "", noteNumber, message, when, minutesFromNow, repeat);

    [Description("Proposes adding an alarm sent by email to a note.")]
    private Task<string> AddEmailAlarmAsync(
        [Description("The note's NoteNumber (a short integer, e.g. 123).")]
        int noteNumber,
        [Description("The text of the alarm.")]
        string message,
        [Description("When it is sent, as yyyy-MM-dd HH:mm. Work out the date from today's.")]
        string when = null,
        [Description("Instead of when: in how many minutes from now it is sent, for \"in two hours\".")]
        int? minutesFromNow = null,
        [Description("How it repeats: none, daily, weekly, monthly or yearly. By default, none.")]
        string repeat = null) =>
        ProposeAlarmAsync(AiActionRules.AddEmailAlarm, "Email alarm", EmailAlarmNotice, noteNumber, message, when, minutesFromNow, repeat);

    private async Task<string> ProposeAlarmAsync(string kind, string name, string notice, int noteNumber, string message, string when,
        int? minutesFromNow, string repeat)
    {
        if (string.IsNullOrWhiteSpace(message))
            return "Error: message is required and cannot be empty.";

        var (note, error) = await FindChangeableNoteAsync(noteNumber);
        if (error != null)
            return error;

        var (date, dateError) = AiActionRules.AlarmDate(when, minutesFromNow, DateTime.Now);
        if (dateError != null)
            return dateError;

        var (type, repeatError) = AiActionRules.AlarmRepeat(repeat);
        if (repeatError != null)
            return repeatError;

        return await ProposeAsync(new AiActionDto
        {
            Kind = kind,
            Text = $"{name} on #{noteNumber} ({note.Topic}), {AiActionRules.DateText(date.Value)}{AiActionRules.AlarmRepeatText(type.Value)}: {Shown(message)}{notice}",
            NoteId = note.NoteId,
            NoteNumber = noteNumber,
            Content = message,
            Date = date,
            AlarmType = type
        });
    }

    [Description("Proposes changing the value of an attribute of a note.")]
    private async Task<string> SetAttributeAsync(
        [Description("The note's NoteNumber (a short integer, e.g. 123).")]
        int noteNumber,
        [Description("The name of the attribute.")]
        string attribute,
        [Description("Its new value. A date as yyyy-MM-dd HH:mm, and yes or no for a boolean.")]
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Error: value is required and cannot be empty.";

        var (note, error) = await FindChangeableNoteAsync(noteNumber);
        if (error != null)
            return error;

        var attributes = (note.KAttributesDto ?? new List<NoteKAttributeDto>()).Where(a => !a.Disabled).ToList();
        var wanted = GlossaryTable.Normalize(attribute);
        var found = attributes.FirstOrDefault(a => GlossaryTable.Normalize(a.Name) == wanted);
        if (found == null)
            return attributes.Count == 0
                ? $"Error: the note #{noteNumber} has no attributes."
                : $"Error: the note #{noteNumber} has no attribute '{attribute}'. Its attributes are: {Options(attributes.Select(a => a.Name))}.";

        List<string> values = null;
        if (found.KAttributeDataType is EnumKAttributeDataType.TabulatedValue or EnumKAttributeDataType.TagsValue)
        {
            var response = await _service.KAttributes.GetKAttributeTabulatedValuesAsync(found.KAttributeId);
            if (!response.IsValid)
                return $"Error reading the values of the attribute: {response.ErrorMessage}";
            values = (response.Entity ?? new List<KAttributeTabulatedValueDto>()).Select(v => v.Value).ToList();
        }

        var (newValue, valueError) = AiActionRules.AttributeValue(found.KAttributeDataType, value, values);
        if (valueError != null)
            return valueError;
        if (newValue == found.Value)
            return $"Error: {found.Name} of the note #{noteNumber} already has that value.";

        var before = string.IsNullOrEmpty(found.Value) ? "(empty)" : found.Value;
        return await ProposeAsync(new AiActionDto
        {
            Kind = AiActionRules.SetAttribute,
            Text = $"{found.Name} of #{noteNumber} ({note.Topic}): {Shown(before)} → {Shown(newValue)}",
            NoteId = note.NoteId,
            NoteNumber = noteNumber,
            AttributeId = found.KAttributeId,
            Value = newValue
        });
    }

    [Description("Proposes relating a note to another one.")]
    private async Task<string> RelateNotesAsync(
        [Description("The NoteNumber of the note the relation goes from.")]
        int noteNumber,
        [Description("The NoteNumber of the note it goes to.")]
        int relatedNoteNumber,
        [Description("The type of the relation, only if the user says one.")]
        string relationType = null)
    {
        if (noteNumber == relatedNoteNumber)
            return "Error: a note cannot be related to itself.";

        var (note, error) = await FindChangeableNoteAsync(noteNumber);
        if (error != null)
            return error;

        var (target, targetError) = await FindCitedNoteAsync(relatedNoteNumber);
        if (targetError != null)
            return targetError;

        var (type, typeError) = await FindTraceNoteTypeAsync(relationType);
        if (typeError != null)
            return typeError;

        var asType = type != null ? $", as {type.Name}" : "";
        return await ProposeAsync(new AiActionDto
        {
            Kind = AiActionRules.RelateNotes,
            Text = $"Relate #{noteNumber} ({note.Topic}) to #{relatedNoteNumber} ({target.Topic}){asType}",
            NoteId = note.NoteId,
            NoteNumber = noteNumber,
            TargetNoteId = target.NoteId,
            TraceNoteTypeId = type?.TraceNoteTypeId
        });
    }

    [Description("Proposes creating a folder.")]
    private async Task<string> CreateFolderAsync(
        [Description("The name of the new folder.")]
        string name,
        [Description("The folder it goes in: its name, its path or its number. By default, Home.")]
        string parent = null)
    {
        var (folders, foldersError) = await GetFoldersAsync();
        if (foldersError != null)
            return foldersError;

        Guid parentId;
        if (!string.IsNullOrWhiteSpace(parent))
        {
            var (found, findError) = folders.Find(parent);
            if (findError != null)
                return findError;
            parentId = found.FolderId;
        }
        else
        {
            var home = await _service.Folders.GetHomeAsync();
            if (!home.IsValid || home.Entity == null)
                return $"Error reading the Home folder: {home.ErrorMessage}";
            parentId = home.Entity.FolderId;
        }

        var error = await KntAiActionExecutor.GetNewFolderErrorAsync(_service, parentId, name);
        if (error != null)
            return $"Error: {error}";

        return await ProposeAsync(new AiActionDto
        {
            Kind = AiActionRules.CreateFolder,
            Text = $"Create the folder {name.Trim()} in {folders.Path(parentId)}",
            FolderId = parentId,
            Topic = name.Trim()
        });
    }

    private static string Shown(string text) => text.Length > MaxCardText ? text[..MaxCardText] + "…" : text;

    private static string Repeated(AiActionDto action) =>
        $"Nothing to do, it was already proposed or done: {action.Text}. Do not call this tool again for it.";

    // The user of a task: the one named or, without a name, the user of the conversation.
    private async Task<(UserDto User, string Error)> FindAssigneeAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            var current = await _service.GetCurrentUserAsync();
            return current != null ? (current, null) : (null, "Error: the user of this conversation is not known. Say who does the task.");
        }

        var (userId, error) = await FindUserAsync(name);
        if (error != null)
            return (null, error);

        var user = await _service.Users.GetAsync(userId.Value);
        return user.IsValid ? (user.Entity, null) : (null, $"Error reading the user: {user.ErrorMessage}");
    }

    private async Task<(TraceNoteTypeDto Type, string Error)> FindTraceNoteTypeAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return (null, null);

        var response = await _service.TraceNoteTypes.GetAllAsync();
        if (!response.IsValid)
            return (null, $"Error reading the types of relation: {response.ErrorMessage}");

        var types = response.Entity ?? new List<TraceNoteTypeDto>();
        var wanted = GlossaryTable.Normalize(name);
        var found = types.FirstOrDefault(t => GlossaryTable.Normalize(t.Name) == wanted);
        return found != null
            ? (found, null)
            : (null, $"Error: there is no type of relation '{name}'. The types are: {Options(types.Select(t => t.Name))}.");
    }

    private async Task<(NoteMinimalDto Note, string Error)> FindCitedNoteAsync(int noteNumber)
    {
        var cited = await _service.Notes.GetCitedNoteAsync(noteNumber);
        if (!cited.IsValid)
            return (null, $"Error getting note: {cited.ErrorMessage}");
        return cited.Entity != null ? (cited.Entity, null) : (null, $"Error: {KntAiActionExecutor.NoteNotFound}");
    }

    private async Task<(NoteDto Note, string Error)> FindChangeableNoteAsync(int noteNumber)
    {
        var (cited, citedError) = await FindCitedNoteAsync(noteNumber);
        if (citedError != null)
            return (null, citedError);

        var (note, error) = await KntAiActionExecutor.GetChangeableNoteAsync(_service, cited.NoteId);
        return error != null ? (null, $"Error: {error}") : (note, null);
    }

    private async Task<string> ProposeAsync(AiActionDto action)
    {
        if (AiActionRules.IsRepeated(action, TurnActions, _earlierActions))
            return Repeated(action);

        var minRole = AiActionRules.MinRoleOf(action.Kind);
        if (!(await _service.GetCurrentUserRoleAsync() >= minRole))
            return $"Error: the role of the user does not allow it, it needs {minRole}. Tell the user so.";

        TurnActions.Add(action);

        var onNewNote = action.NoteId is Guid noteId && _newNoteIds.Contains(noteId);
        if (AiActionRules.NeedsCard(action.Kind, _autoApprovedKinds.Contains(action.Kind), onNewNote))
            return $"Proposed, NOT done yet: {action.Text}. KNote is showing the user a card with Accept and Reject buttons for it. " +
                "Do not ask anything about it: just say, in one short line, that it is waiting in the card.";

        await ExecuteActionAsync(_service, action);
        // The history shows the model saying that an accepted action was waiting in its card, so it has to be told that this one is not.
        return action.Status == EnumAiActionStatus.Done
            ? $"Done: {action.Text}. It is already done, without a card: nothing is waiting for the user. Do not mention any card " +
                "or confirmation: just say, in one short line, that it is done."
            : $"Error: {action.Result}";
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Service.Core;

/// <summary>
/// Runs an action of the AI assistant (KNote Atlas, D2) with the commands of always, so with the permissions of the user of
/// the service and with what those commands do after saving (the semantic index marked, the rules of the system notes). It
/// never trusts what it receives: it looks at the note again. Each action on its own: one that fails leaves it Failed and
/// says why, and the others go on. Only the kinds of AiActionRules.Kinds run.
/// </summary>
public static class KntAiActionExecutor
{
    public const string NoteNotFound = "Note not found.";
    public const string NoteLocked = "The note is locked and cannot be changed.";
    public const string NoteChanged = "The note has changed since this was proposed, so nothing was replaced. Ask again.";

    public static async Task ExecuteAsync(IKntService service, AiActionDto action)
    {
        try
        {
            var error = action.Kind switch
            {
                AiActionRules.CreateNote => await CreateNoteAsync(service, action),
                AiActionRules.AppendToNote => await AppendToNoteAsync(service, action),
                AiActionRules.ReplaceNoteText => await ReplaceNoteTextAsync(service, action),
                AiActionRules.MoveNote => await MoveNoteAsync(service, action),
                AiActionRules.SetTags => await SetTagsAsync(service, action),
                AiActionRules.AddTask => await AddTaskAsync(service, action),
                AiActionRules.AddAlarm => await AddAlarmAsync(service, action, EnumNotificationType.AppInfo),
                AiActionRules.AddEmailAlarm => await AddAlarmAsync(service, action, EnumNotificationType.Email),
                AiActionRules.SetAttribute => await SetAttributeAsync(service, action),
                AiActionRules.RelateNotes => await RelateNotesAsync(service, action),
                AiActionRules.CreateFolder => await CreateFolderAsync(service, action),
                _ => $"Unknown action: {action.Kind}."
            };
            action.Status = error == null ? EnumAiActionStatus.Done : EnumAiActionStatus.Failed;
            action.Result = error;
        }
        catch (Exception ex)
        {
            action.Status = EnumAiActionStatus.Failed;
            action.Result = ex.Message;
        }
    }

    /// <summary>The note an action can change, or the reason why not.</summary>
    public static async Task<(NoteDto Note, string Error)> GetChangeableNoteAsync(IKntService service, Guid noteId)
    {
        var (note, error) = await GetVisibleNoteAsync(service, noteId);
        if (error != null)
            return (null, error);
        return note.GetContentTypeExt().DescriptionBlocked ? (null, NoteLocked) : (note, null);
    }

    // A note an action can point to, without changing it: the destination of a relation.
    private static async Task<(NoteDto Note, string Error)> GetVisibleNoteAsync(IKntService service, Guid noteId)
    {
        var res = await service.Notes.GetAsync(noteId);
        return !res.IsValid || res.Entity == null || KntConst.IsSystemNoteType(res.Entity.NoteTypeDto?.Name)
            ? (null, NoteNotFound)
            : (res.Entity, null);
    }

    /// <summary>A text of the assistant, in Markdown, as the note keeps it: in an HTML note, in HTML.</summary>
    public static string NoteText(IKntService service, NoteDto note, string markdown) =>
        note.GetContentTypeExt().ForDescription == "html" ? service.Notes.UtilMarkdownToHtml(markdown ?? "") : markdown ?? "";

    /// <summary>Why a folder cannot be created there, or null: in the folders of the system, or with the name of a sister.</summary>
    public static async Task<string> GetNewFolderErrorAsync(IKntService service, Guid parentId, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "The name of the folder is required.";

        if (!await IsUserFolderAsync(service, parentId))
            return "The parent folder was not found.";

        var folders = await service.Folders.GetAllAsync();
        if (!folders.IsValid)
            return folders.ErrorMessage;

        return folders.Entity.Any(f => f.ParentId == parentId && string.Equals(f.Name, name.Trim(), StringComparison.OrdinalIgnoreCase))
            ? $"There is already a folder {name.Trim()} there."
            : null;
    }

    // A folder that exists and is not inside one of the system.
    private static async Task<bool> IsUserFolderAsync(IKntService service, Guid folderId)
    {
        var path = await service.Folders.GetPathAsync(folderId);
        return path.IsValid && !string.IsNullOrEmpty(path.Entity) && !path.Entity.Split('\\').Any(KntConst.SystemFolderNames.Contains);
    }

    private static async Task<string> CreateNoteAsync(IKntService service, AiActionDto action)
    {
        var folder = await service.Folders.GetAsync(action.FolderId ?? Guid.Empty);
        if (!folder.IsValid || folder.Entity == null)
            return "The folder of the note was not found.";

        var newNote = await service.Notes.NewExtendedAsync();
        if (!newNote.IsValid)
            return newNote.ErrorMessage;

        var note = newNote.Entity;
        note.Topic = action.Topic;
        note.Description = action.Content ?? "";
        note.Tags = AiActionRules.ChangeTags("", action.AddTags, null);
        note.FolderId = folder.Entity.FolderId;
        note.FolderDto = folder.Entity;

        var saved = await service.Notes.SaveExtendedAsync(note);
        if (!saved.IsValid)
            return saved.ErrorMessage;

        action.NoteId = saved.Entity.NoteId;
        action.NoteNumber = saved.Entity.NoteNumber;
        action.Text = $"Created the note #{saved.Entity.NoteNumber} ({action.Topic})";
        return null;
    }

    private static async Task<string> AppendToNoteAsync(IKntService service, AiActionDto action)
    {
        var (note, error) = await GetChangeableNoteAsync(service, action.NoteId ?? Guid.Empty);
        if (error != null)
            return error;

        var text = NoteText(service, note, action.Content);
        note.Description = string.IsNullOrEmpty(note.Description) ? text : $"{note.Description.TrimEnd()}\r\n\r\n{text}";

        var saved = await service.Notes.SaveAsync(note);
        return saved.IsValid ? null : saved.ErrorMessage;
    }

    // Only the text the card was shown for, because what is replaced cannot be recovered: what somebody wrote in the note
    // while the card was waiting stays. Emptying a note is not replacing it.
    private static async Task<string> ReplaceNoteTextAsync(IKntService service, AiActionDto action)
    {
        var (note, error) = await GetChangeableNoteAsync(service, action.NoteId ?? Guid.Empty);
        if (error != null)
            return error;

        if (string.IsNullOrWhiteSpace(action.Content))
            return "The new text of the note is required.";
        if (action.TextStamp != AiActionRules.TextStamp(note.Description))
            return NoteChanged;

        note.Description = NoteText(service, note, action.Content);

        var saved = await service.Notes.SaveAsync(note);
        return saved.IsValid ? null : saved.ErrorMessage;
    }

    private static async Task<string> MoveNoteAsync(IKntService service, AiActionDto action)
    {
        var (note, error) = await GetChangeableNoteAsync(service, action.NoteId ?? Guid.Empty);
        if (error != null)
            return error;

        var folderId = action.FolderId ?? Guid.Empty;
        if (!await IsUserFolderAsync(service, folderId))
            return "The folder was not found.";

        var moved = await service.Notes.UtilPatchFolderAsync(note.NoteId, folderId);
        return moved.IsValid ? null : moved.ErrorMessage;
    }

    private static async Task<string> SetTagsAsync(IKntService service, AiActionDto action)
    {
        var (note, error) = await GetChangeableNoteAsync(service, action.NoteId ?? Guid.Empty);
        if (error != null)
            return error;

        note.Tags = AiActionRules.ChangeTags(note.Tags, action.AddTags, action.RemoveTags);

        var saved = await service.Notes.SaveAsync(note);
        return saved.IsValid ? null : saved.ErrorMessage;
    }

    private static async Task<string> AddTaskAsync(IKntService service, AiActionDto action)
    {
        var (note, error) = await GetChangeableNoteAsync(service, action.NoteId ?? Guid.Empty);
        if (error != null)
            return error;

        var user = await service.Users.GetAsync(action.UserId ?? Guid.Empty);
        if (!user.IsValid || user.Entity == null)
            return "The user of the task was not found.";

        var tasks = await service.Notes.GetNoteTasksAsync(note.NoteId);
        if (!tasks.IsValid)
            return tasks.ErrorMessage;

        // As the editors of both apps start one: after the other tasks, started now.
        var now = DateTime.Now;
        var saved = await service.Notes.SaveNoteTaskAsync(new NoteTaskDto
        {
            NoteId = note.NoteId,
            UserId = user.Entity.UserId,
            Description = action.Content,
            Tags = "",
            Priority = tasks.Entity.Count + 1,
            CreationDateTime = now,
            ModificationDateTime = now,
            StartDate = now,
            ExpectedEndDate = action.Date
        });
        if (!saved.IsValid)
            return saved.ErrorMessage;

        await UpdateNoteStatusAsync(service, note);
        return null;
    }

    // Only what the kind says: the type of alarm never comes from outside, so it cannot run a script. It is for the user who
    // runs it (NewMessageAsync).
    private static async Task<string> AddAlarmAsync(IKntService service, AiActionDto action, EnumNotificationType notification)
    {
        var (note, error) = await GetChangeableNoteAsync(service, action.NoteId ?? Guid.Empty);
        if (error != null)
            return error;

        var message = await service.Notes.NewMessageAsync(note.NoteId);
        if (!message.IsValid)
            return message.ErrorMessage;

        message.Entity.NotificationType = notification;
        message.Entity.AlarmType = action.AlarmType ?? EnumAlarmType.Standard;
        message.Entity.AlarmDateTime = action.Date;
        message.Entity.Comment = action.Content;

        var saved = await service.Notes.SaveMessageAsync(message.Entity);
        if (!saved.IsValid)
            return saved.ErrorMessage;

        await UpdateNoteStatusAsync(service, note);
        return null;
    }

    // The status of a note comes from its tasks and alarms, and is written when the note is saved. If this fails, what was
    // added is there all the same: the next save of the note puts it right.
    private static Task UpdateNoteStatusAsync(IKntService service, NoteDto note) => service.Notes.SaveAsync(note);

    private static async Task<string> SetAttributeAsync(IKntService service, AiActionDto action)
    {
        var (note, error) = await GetChangeableNoteAsync(service, action.NoteId ?? Guid.Empty);
        if (error != null)
            return error;

        var attribute = note.KAttributesDto?.FirstOrDefault(a => a.KAttributeId == action.AttributeId);
        if (attribute == null)
            return "The note does not have that attribute.";

        attribute.Value = action.Value;

        var saved = await service.Notes.SaveAsync(note);
        return saved.IsValid ? null : saved.ErrorMessage;
    }

    private static async Task<string> RelateNotesAsync(IKntService service, AiActionDto action)
    {
        var (note, error) = await GetChangeableNoteAsync(service, action.NoteId ?? Guid.Empty);
        if (error != null)
            return error;

        var (target, targetError) = await GetVisibleNoteAsync(service, action.TargetNoteId ?? Guid.Empty);
        if (targetError != null)
            return targetError;

        var saved = await service.Notes.SaveTraceNoteAsync(new TraceNoteDto
        {
            FromId = note.NoteId,
            ToId = target.NoteId,
            TraceNoteTypeId = action.TraceNoteTypeId
        });
        return saved.IsValid ? null : saved.ErrorMessage;
    }

    private static async Task<string> CreateFolderAsync(IKntService service, AiActionDto action)
    {
        var parentId = action.FolderId ?? Guid.Empty;
        var error = await GetNewFolderErrorAsync(service, parentId, action.Topic);
        if (error != null)
            return error;

        var saved = await service.Folders.SaveAsync(new FolderDto { Name = action.Topic.Trim(), ParentId = parentId });
        return saved.IsValid ? null : saved.ErrorMessage;
    }
}

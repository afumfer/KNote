using System;
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

    public static async Task ExecuteAsync(IKntService service, AiActionDto action)
    {
        try
        {
            var error = action.Kind switch
            {
                AiActionRules.CreateNote => await CreateNoteAsync(service, action),
                AiActionRules.AppendToNote => await AppendToNoteAsync(service, action),
                AiActionRules.SetTags => await SetTagsAsync(service, action),
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
        var res = await service.Notes.GetAsync(noteId);
        if (!res.IsValid || res.Entity == null || KntConst.IsSystemNoteType(res.Entity.NoteTypeDto?.Name))
            return (null, NoteNotFound);
        return res.Entity.GetContentTypeExt().DescriptionBlocked ? (null, NoteLocked) : (res.Entity, null);
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

        var text = note.GetContentTypeExt().ForDescription == "html"
            ? service.Notes.UtilMarkdownToHtml(action.Content ?? "")
            : action.Content ?? "";
        note.Description = string.IsNullOrEmpty(note.Description) ? text : $"{note.Description.TrimEnd()}\r\n\r\n{text}";

        var saved = await service.Notes.SaveAsync(note);
        return saved.IsValid ? null : saved.ErrorMessage;
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
}

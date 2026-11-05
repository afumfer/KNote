using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Service.Core;

/// <summary>
/// The notes of the context KNote Atlas gives the AI (stage 1, chapters 19–21), in the folder «KNote Atlas»
/// (KntSystemNotes): the repository's profile and glossary (one of each: with several, the oldest one) and the
/// instructions of each task (@KNoteTaskPrompt, with the attributes Task, Mode and Active). They are found without being
/// created (the windows are for any user), and created with their templates by the commands of ProjectManager.
/// <para>
/// The rule of who writes them (Notas-plan-tecnico, 03-Etapa-1 § 3.1): the notes of these types that people write
/// (ProtectedNoteTypeNames) are only saved, deleted, moved or retagged by ProjectManager and Admin. It is a domain rule of
/// the note commands, applied even with the authorization of commands off (the Server); without a user (the system's
/// own processes), allowed.
/// </para>
/// Errors are added to the command's result.
/// </summary>
internal static class KntAtlasNotes
{
    private const string BypassReason = "KNote Atlas: the notes of the context of the AI";

    public static readonly IReadOnlyList<string> ProtectedNoteTypeNames = new[]
    {
        KntConst.ProfileTag, KntConst.GlossaryTag, KntConst.TaskPromptTag, KntConst.TestQueriesTag
    };

    public const string NotAllowedMessage =
        "Only a ProjectManager or an Admin can change the notes of KNote Atlas (profile, glossary, task instructions and test queries).";

    public static readonly IReadOnlyList<string> TaskPromptModes = new[] { "Add", "Replace" };

    #region The rule of who writes them

    /// <summary>False when a note of one of these types (its new one or the one it had) is written by a user below ProjectManager.</summary>
    public static async Task<bool> CanWriteAsync(IKntService service, params Guid?[] noteTypeIds)
    {
        var ids = noteTypeIds.Where(id => id != null).Select(id => id.Value).Distinct().ToList();
        if (ids.Count == 0 || string.IsNullOrWhiteSpace(service.UserIdentityName))
            return true;

        var types = (await service.Repository.NoteTypes.GetAllAsync()).Entity;
        if (types == null || !types.Any(t => ids.Contains(t.NoteTypeId) && ProtectedNoteTypeNames.Contains(t.Name)))
            return true;

        return await service.GetCurrentUserRoleAsync() >= EnumRoles.ProjectManager;
    }

    /// <summary>The same for a note that already exists (to delete, move or retag it); true if it does not exist.</summary>
    public static async Task<bool> CanWriteNoteAsync(IKntService service, Guid noteId)
    {
        var note = (await service.Repository.Notes.GetAsync(noteId)).Entity;
        return note == null || await CanWriteAsync(service, note.NoteTypeId);
    }

    #endregion

    #region Finding them

    /// <summary>The notes of a type, the oldest first; none if the type does not exist yet (nothing is created).</summary>
    public static async Task<List<NoteMinimalDto>> FindAsync(IKntService service, string noteTypeName, ResultBase result)
    {
        var noteTypeId = await FindNoteTypeIdAsync(service, noteTypeName, result);
        if (noteTypeId == null)
            return new List<NoteMinimalDto>();

        var resNotes = await service.Notes.GetFilterMinimalAsync(new NotesFilterDto { NoteTypeId = noteTypeId });
        if (!resNotes.IsValid)
        {
            result.AddListErrorMessage(resNotes.ListErrorMessage);
            return new List<NoteMinimalDto>();
        }
        return resNotes.Entity.OrderBy(n => n.NoteNumber).ToList();
    }

    /// <summary>The oldest note of a type, whole (with its attributes); null, none.</summary>
    public static async Task<NoteDto> FindFirstAsync(IKntService service, string noteTypeName, ResultBase result)
    {
        var first = (await FindAsync(service, noteTypeName, result)).FirstOrDefault();
        return first == null ? null : await GetAsync(service, first.NoteId, result);
    }

    /// <summary>The notes of instructions of a task (by the attribute Task, its key or name), the oldest first.</summary>
    public static async Task<List<TaskPromptNote>> FindTaskPromptsAsync(IKntService service, AiTask task, ResultBase result)
    {
        var prompts = new List<TaskPromptNote>();
        foreach (var minimal in await FindAsync(service, KntConst.TaskPromptTag, result))
        {
            var note = await GetAsync(service, minimal.NoteId, result);
            if (note == null)
                continue;
            if (AiTaskCatalog.Find(AttributeValue(note, KntConst.TaskPromptTaskAttributeName)) != task)
                continue;
            prompts.Add(new TaskPromptNote(note,
                string.Equals(AttributeValue(note, KntConst.TaskPromptModeAttributeName)?.Trim(), "Replace", StringComparison.OrdinalIgnoreCase),
                !bool.TryParse(AttributeValue(note, KntConst.TaskPromptActiveAttributeName), out var active) || active));
        }
        return prompts;
    }

    public sealed record TaskPromptNote(NoteDto Note, bool Replaces, bool Active);

    /// <summary>The text of a note as the AI reads it: Markdown (an HTML note converted).</summary>
    public static string Text(IKntService service, NoteDto note)
    {
        var description = note?.Description ?? "";
        return string.Equals(note?.GetContentTypeExt().ForDescription, "html", StringComparison.OrdinalIgnoreCase)
            ? service.Notes.UtilHtmlToMarkdown(description)
            : description;
    }

    public static AtlasNoteRefDto Ref(NoteInfoDto note) => note == null ? null : new AtlasNoteRefDto
    {
        NoteId = note.NoteId,
        NoteNumber = note.NoteNumber,
        Topic = note.Topic,
        ModificationDateTime = note.ModificationDateTime
    };

    public static AtlasNoteRefDto Ref(NoteMinimalDto note) => note == null ? null : new AtlasNoteRefDto
    {
        NoteId = note.NoteId,
        NoteNumber = note.NoteNumber,
        Topic = note.Topic,
        ModificationDateTime = note.ModificationDateTime
    };

    #endregion

    #region Creating them

    /// <summary>The profile or the glossary: the oldest one or, without any, a new one with its template.</summary>
    public static async Task<NoteDto> EnsureSingleAsync(IKntService service, string noteTypeName, string typeDescription,
        string topic, string template, ResultBase result)
    {
        var noteTypeId = await KntSystemNotes.EnsureNoteTypeAsync(service, noteTypeName, typeDescription, BypassReason, result);
        if (!result.IsValid)
            return null;
        var folder = await KntSystemNotes.EnsureFolderAsync(service, KntConst.AtlasFolderName, BypassReason, result);
        if (!result.IsValid)
            return null;

        var noteId = await KntSystemNotes.FindOrCreateAsync(service, result,
            find: async () => (await FindAsync(service, noteTypeName, result)).FirstOrDefault()?.NoteId ?? Guid.Empty,
            create: async () => (await SaveNewAsync(service, noteTypeId, folder, topic, template, null, result))?.NoteId ?? Guid.Empty);
        return result.IsValid ? await GetAsync(service, noteId, result) : null;
    }

    /// <summary>The note of instructions of a task: its active one, else its oldest one, else a new one (Mode Add, active).</summary>
    public static async Task<NoteDto> EnsureTaskPromptAsync(IKntService service, AiTask task, ResultBase result)
    {
        var noteTypeId = await KntSystemNotes.EnsureNoteTypeAsync(service, KntConst.TaskPromptTag,
            "KNote Atlas: the instructions of a task of the AI, added to KNote's or instead of them.", BypassReason, result);
        if (!result.IsValid)
            return null;
        await KntSystemNotes.EnsureAttributeAsync(service, noteTypeId, KntConst.TaskPromptTaskAttributeName,
            "The task of KNote Atlas", 1, BypassReason, result);
        await KntSystemNotes.EnsureAttributeAsync(service, noteTypeId, KntConst.TaskPromptModeAttributeName,
            "Add: added to KNote's instructions; Replace: instead of them", 2, BypassReason, result, tabulatedValues: TaskPromptModes);
        await KntSystemNotes.EnsureAttributeAsync(service, noteTypeId, KntConst.TaskPromptActiveAttributeName,
            "Whether KNote uses it", 3, BypassReason, result, EnumKAttributeDataType.Bool);
        var folder = await KntSystemNotes.EnsureFolderAsync(service, KntConst.AtlasFolderName, BypassReason, result);
        if (!result.IsValid)
            return null;

        var noteId = await KntSystemNotes.FindOrCreateAsync(service, result,
            find: async () =>
            {
                var prompts = await FindTaskPromptsAsync(service, task, result);
                return (prompts.FirstOrDefault(p => p.Active) ?? prompts.FirstOrDefault())?.Note.NoteId ?? Guid.Empty;
            },
            create: async () => (await SaveNewAsync(service, noteTypeId, folder, $"Instructions: {task.Name}", AtlasTemplates.TaskPrompt(task),
                new Dictionary<string, string>
                {
                    [KntConst.TaskPromptTaskAttributeName] = task.Name,
                    [KntConst.TaskPromptModeAttributeName] = "Add",
                    [KntConst.TaskPromptActiveAttributeName] = true.ToString()
                }, result))?.NoteId ?? Guid.Empty);
        return result.IsValid ? await GetAsync(service, noteId, result) : null;
    }

    private static async Task<NoteDto> SaveNewAsync(IKntService service, Guid noteTypeId, FolderInfoDto folder, string topic,
        string description, IDictionary<string, string> attributes, ResultBase result)
    {
        // With the NoteType, KAttributesDto comes completed with the type's attributes, as for any new note.
        var resNew = await service.Notes.NewAsync(new NoteInfoDto { NoteTypeId = noteTypeId });
        if (!resNew.IsValid)
        {
            result.AddListErrorMessage(resNew.ListErrorMessage);
            return null;
        }
        var note = resNew.Entity;
        note.Topic = topic;
        note.Description = AtlasTemplates.NoteText(description);
        note.NoteTypeId = noteTypeId;
        note.FolderId = folder.FolderId;
        note.Tags ??= "";
        foreach (var (name, value) in attributes ?? new Dictionary<string, string>())
        {
            var attribute = note.KAttributesDto?.FirstOrDefault(a => a.Name == name);
            if (attribute != null)
                attribute.Value = value;
        }

        var resSave = await service.Notes.SaveAsync(note);
        if (!resSave.IsValid)
        {
            result.AddListErrorMessage(resSave.ListErrorMessage);
            return null;
        }
        return resSave.Entity;
    }

    #endregion

    #region Private methods

    private static async Task<Guid?> FindNoteTypeIdAsync(IKntService service, string noteTypeName, ResultBase result)
    {
        var resTypes = await service.NoteTypes.GetAllAsync();
        if (!resTypes.IsValid)
        {
            result.AddListErrorMessage(resTypes.ListErrorMessage);
            return null;
        }
        return resTypes.Entity?.FirstOrDefault(t => t.Name == noteTypeName)?.NoteTypeId;
    }

    private static async Task<NoteDto> GetAsync(IKntService service, Guid noteId, ResultBase result)
    {
        var resNote = await service.Notes.GetAsync(noteId);
        if (!resNote.IsValid)
        {
            result.AddListErrorMessage(resNote.ListErrorMessage);
            return null;
        }
        return resNote.Entity;
    }

    private static string AttributeValue(NoteDto note, string name) =>
        note.KAttributesDto?.FirstOrDefault(a => a.Name == name)?.Value;

    #endregion
}

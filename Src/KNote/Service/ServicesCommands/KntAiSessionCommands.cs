using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.Service.ServicesCommands;

// AI assistant sessions (see IKntAiSessionService). Staff, like the assistant itself: a session is a note and a
// task the user creates. Everything goes through the other domains' services (notes, tasks, note types,
// attributes, folders), so their rules and command events apply. Creating the note type, its attributes and the
// folder needs Admin/ProjectManager on its own: once this command is authorized, that is done inside a
// KntAuthorizationBypass scope, so the assistant works for any Staff user.

[KntAuthorize(EnumRoles.Staff)]
public class KntAiSessionsGetUserSessionsAsyncCommand : KntCommandServiceBase<Result<List<AiChatSessionInfoDto>>>
{
    public KntAiSessionsGetUserSessionsAsyncCommand(IKntService service) : base(service)
    {

    }

    public override async Task<Result<List<AiChatSessionInfoDto>>> Execute()
    {
        var result = new Result<List<AiChatSessionInfoDto>>();

        var user = await AiSessions.GetCurrentUserAsync(Service, result);
        var infrastructure = await AiSessions.EnsureInfrastructureAsync(Service, result);
        if (!result.IsValid)
            return result;

        var resNotes = await Service.Notes.GetFilterMinimalAsync(new NotesFilterDto
        {
            NoteTypeId = infrastructure.NoteTypeId,
            TaskUserId = user.UserId
        });
        if (!resNotes.IsValid)
        {
            result.AddListErrorMessage(resNotes.ListErrorMessage);
            return result;
        }

        result.Entity = resNotes.Entity
            .OrderByDescending(n => n.ModificationDateTime)
            .Select(n => new AiChatSessionInfoDto
            {
                NoteId = n.NoteId,
                NoteNumber = n.NoteNumber,
                Topic = n.Topic,
                CreationDateTime = n.CreationDateTime,
                ModificationDateTime = n.ModificationDateTime
            })
            .ToList();

        return result;
    }
}

[KntAuthorize(EnumRoles.Staff)]
public class KntAiSessionsGetAsyncCommand : KntCommandServiceBase<Guid, Result<AiChatSessionDto>>
{
    public KntAiSessionsGetAsyncCommand(IKntService service, Guid noteId) : base(service, noteId)
    {

    }

    public override async Task<Result<AiChatSessionDto>> Execute()
    {
        var result = new Result<AiChatSessionDto>();

        var user = await AiSessions.GetCurrentUserAsync(Service, result);
        var infrastructure = await AiSessions.EnsureInfrastructureAsync(Service, result);
        if (!result.IsValid)
            return result;

        var note = await AiSessions.GetUserSessionNoteAsync(Service, Param, infrastructure, user, result);
        if (!result.IsValid)
            return result;

        result.Entity = new AiChatSessionDto
        {
            NoteId = note.NoteId,
            NoteNumber = note.NoteNumber,
            Topic = note.Topic,
            CreationDateTime = note.CreationDateTime,
            ModificationDateTime = note.ModificationDateTime,
            Provider = AiSessions.GetAttributeValue(note, infrastructure.ProviderAttributeId),
            Model = AiSessions.GetAttributeValue(note, infrastructure.ModelAttributeId),
            Turns = AiChatSessionTranscript.Parse(note.Description)
        };

        return result;
    }
}

[KntAuthorize(EnumRoles.Staff)]
public class KntAiSessionsSaveAsyncCommand : KntCommandServiceBase<AiChatSessionDto, Result<AiChatSessionDto>>
{
    private const int MaxTopicLength = 80;

    public KntAiSessionsSaveAsyncCommand(IKntService service, AiChatSessionDto session) : base(service, session)
    {

    }

    public override Result ValidateParam()
    {
        var result = new Result();
        if (Param == null)
            result.AddErrorMessage("The AI session is required.");
        else
        {
            if (Param.Turns == null || Param.Turns.Count == 0)
                result.AddErrorMessage("An AI session needs at least one message.");
            if (string.IsNullOrWhiteSpace(Param.Provider) || string.IsNullOrWhiteSpace(Param.Model))
                result.AddErrorMessage("The AI provider and model of the session are required.");
        }
        return result;
    }

    public override async Task<Result<AiChatSessionDto>> Execute()
    {
        var result = new Result<AiChatSessionDto>();

        var user = await AiSessions.GetCurrentUserAsync(Service, result);
        var infrastructure = await AiSessions.EnsureInfrastructureAsync(Service, result);
        if (!result.IsValid)
            return result;

        NoteDto saved = Param.NoteId == Guid.Empty
            ? await AddSessionAsync(user, infrastructure, result)
            : await UpdateSessionAsync(user, infrastructure, result);
        if (!result.IsValid)
            return result;

        result.Entity = new AiChatSessionDto
        {
            NoteId = saved.NoteId,
            NoteNumber = saved.NoteNumber,
            Topic = saved.Topic,
            CreationDateTime = saved.CreationDateTime,
            ModificationDateTime = saved.ModificationDateTime,
            Provider = Param.Provider,
            Model = Param.Model,
            Turns = Param.Turns
        };

        return result;
    }

    private async Task<NoteDto> AddSessionAsync(UserDto user, AiSessionsInfrastructure infrastructure, Result<AiChatSessionDto> result)
    {
        // With the NoteType, KAttributesDto comes completed with the type's attributes, as for any new note.
        var resNew = await Service.Notes.NewExtendedAsync(new NoteInfoDto { NoteTypeId = infrastructure.NoteTypeId });
        if (!resNew.IsValid)
        {
            result.AddListErrorMessage(resNew.ListErrorMessage);
            return null;
        }

        var note = resNew.Entity;
        note.Topic = string.IsNullOrWhiteSpace(Param.Topic) ? TopicFromFirstPrompt() : Param.Topic;
        note.Description = AiChatSessionTranscript.Write(Param.Turns);
        // NewExtendedAsync leaves Tags null, and KntNotesSaveExtendedAsyncCommand needs it.
        note.Tags = "";
        // FolderDto too: it is validated with the note.
        note.FolderId = infrastructure.Folder.FolderId;
        note.FolderDto = infrastructure.Folder.GetSimpleDto<FolderDto>();
        note.NoteTypeId = infrastructure.NoteTypeId;
        SetSessionAttributes(note, infrastructure);

        var now = DateTime.Now;
        note.Tasks.Add(new NoteTaskDto
        {
            UserId = user.UserId,
            Description = "AI Assistant session",
            Tags = "",
            CreationDateTime = now,
            ModificationDateTime = now,
            StartDate = now
        });

        var resSave = await Service.Notes.SaveExtendedAsync(note);
        if (!resSave.IsValid)
        {
            result.AddListErrorMessage(resSave.ListErrorMessage);
            return null;
        }
        return resSave.Entity;
    }

    private async Task<NoteDto> UpdateSessionAsync(UserDto user, AiSessionsInfrastructure infrastructure, Result<AiChatSessionDto> result)
    {
        var note = await AiSessions.GetUserSessionNoteAsync(Service, Param.NoteId, infrastructure, user, result);
        if (!result.IsValid)
            return null;

        if (!string.IsNullOrWhiteSpace(Param.Topic))
            note.Topic = Param.Topic;
        note.Description = AiChatSessionTranscript.Write(Param.Turns);
        SetSessionAttributes(note, infrastructure);

        var resSave = await Service.Notes.SaveAsync(note);
        if (!resSave.IsValid)
        {
            result.AddListErrorMessage(resSave.ListErrorMessage);
            return null;
        }
        return resSave.Entity;
    }

    private void SetSessionAttributes(NoteDto note, AiSessionsInfrastructure infrastructure)
    {
        AiSessions.SetAttributeValue(note, infrastructure.ProviderAttributeId, Param.Provider);
        AiSessions.SetAttributeValue(note, infrastructure.ModelAttributeId, Param.Model);
    }

    // The first line of the first prompt, shortened; a dated title when there is none.
    private string TopicFromFirstPrompt()
    {
        var firstLine = (Param.Turns.FirstOrDefault()?.Prompt ?? "")
            .ReplaceLineEndings("\n")
            .Split('\n')
            .Select(l => l.Trim())
            .FirstOrDefault(l => l.Length > 0);

        if (string.IsNullOrEmpty(firstLine))
            return $"AI session {DateTime.Now:yyyy-MM-dd HH:mm}";

        return firstLine.Length <= MaxTopicLength ? firstLine : firstLine.Substring(0, MaxTopicLength - 1) + "…";
    }
}

internal sealed record AiSessionsInfrastructure(Guid NoteTypeId, Guid ProviderAttributeId, Guid ModelAttributeId, FolderInfoDto Folder);

// What the AI session commands share. Errors are added to the command's result instead of thrown, as every
// command reports them.
internal static class AiSessions
{
    private const string InfrastructureBypassReason = "AI assistant sessions: note type, attributes and folder";

    public static async Task<UserDto> GetCurrentUserAsync(IKntService service, ResultBase result)
    {
        var user = await service.GetCurrentUserAsync();
        if (user == null)
            result.AddErrorMessage($"The user '{service.UserIdentityName}' is not registered in the repository '{service.RepositoryRef?.Alias}'.");
        return user;
    }

    // The note type, its two attributes and the folder of the sessions, created when missing (found by name, so
    // a folder moved elsewhere in the tree is still found). Only their creation runs in the bypass scope.
    public static async Task<AiSessionsInfrastructure> EnsureInfrastructureAsync(IKntService service, ResultBase result)
    {
        var noteTypeId = await EnsureNoteTypeAsync(service, result);
        if (!result.IsValid)
            return null;

        var providerAttributeId = await EnsureAttributeAsync(service, noteTypeId, KntConst.AiProviderAttributeName, "AI provider of the session", 1, result);
        var modelAttributeId = await EnsureAttributeAsync(service, noteTypeId, KntConst.AiModelAttributeName, "AI model of the session", 2, result);
        var folder = await EnsureFolderAsync(service, result);
        if (!result.IsValid)
            return null;

        return new AiSessionsInfrastructure(noteTypeId, providerAttributeId, modelAttributeId, folder);
    }

    // The session note, only if it is one (of the sessions' note type) and belongs to the user.
    public static async Task<NoteDto> GetUserSessionNoteAsync(IKntService service, Guid noteId,
        AiSessionsInfrastructure infrastructure, UserDto user, ResultBase result)
    {
        var resNote = await service.Notes.GetAsync(noteId);
        if (!resNote.IsValid)
        {
            result.AddListErrorMessage(resNote.ListErrorMessage);
            return null;
        }

        var note = resNote.Entity;
        if (note == null || note.NoteTypeId != infrastructure.NoteTypeId)
        {
            result.AddErrorMessage("The AI session does not exist.");
            return null;
        }

        var resTasks = await service.Notes.GetNoteTasksAsync(noteId);
        if (!resTasks.IsValid)
        {
            result.AddListErrorMessage(resTasks.ListErrorMessage);
            return null;
        }
        if (resTasks.Entity == null || !resTasks.Entity.Any(t => t.UserId == user.UserId))
        {
            result.AddErrorMessage("The AI session belongs to another user.");
            return null;
        }

        return note;
    }

    public static string GetAttributeValue(NoteDto note, Guid kattributeId) =>
        note.KAttributesDto?.FirstOrDefault(a => a.KAttributeId == kattributeId)?.Value;

    public static void SetAttributeValue(NoteDto note, Guid kattributeId, string value)
    {
        var attribute = note.KAttributesDto?.FirstOrDefault(a => a.KAttributeId == kattributeId);
        if (attribute != null)
            attribute.Value = value;
    }

    private static async Task<Guid> EnsureNoteTypeAsync(IKntService service, ResultBase result)
    {
        var resTypes = await service.NoteTypes.GetAllAsync();
        if (!resTypes.IsValid)
        {
            result.AddListErrorMessage(resTypes.ListErrorMessage);
            return Guid.Empty;
        }

        var noteType = resTypes.Entity?.FirstOrDefault(t => t.Name == KntConst.ChatSessionsTag);
        if (noteType != null)
            return noteType.NoteTypeId;

        Result<NoteTypeDto> resSave;
        using (KntAuthorizationBypass.Begin(service, InfrastructureBypassReason))
        {
            resSave = await service.NoteTypes.SaveAsync(new NoteTypeDto
            {
                Name = KntConst.ChatSessionsTag,
                Description = "AI Assistant sessions"
            });
        }
        if (!resSave.IsValid)
            result.AddListErrorMessage(resSave.ListErrorMessage);
        return resSave.Entity?.NoteTypeId ?? Guid.Empty;
    }

    private static async Task<Guid> EnsureAttributeAsync(IKntService service, Guid noteTypeId, string name,
        string description, int order, ResultBase result)
    {
        var resAttributes = await service.KAttributes.GetAllAsync(noteTypeId);
        if (!resAttributes.IsValid)
        {
            result.AddListErrorMessage(resAttributes.ListErrorMessage);
            return Guid.Empty;
        }

        var attribute = resAttributes.Entity?.FirstOrDefault(a => a.NoteTypeId == noteTypeId && a.Name == name);
        if (attribute != null)
            return attribute.KAttributeId;

        Result<KAttributeDto> resSave;
        using (KntAuthorizationBypass.Begin(service, InfrastructureBypassReason))
        {
            resSave = await service.KAttributes.SaveAsync(new KAttributeDto
            {
                Name = name,
                Description = description,
                KAttributeDataType = EnumKAttributeDataType.String,
                NoteTypeId = noteTypeId,
                Order = order
            });
        }
        if (!resSave.IsValid)
            result.AddListErrorMessage(resSave.ListErrorMessage);
        return resSave.Entity?.KAttributeId ?? Guid.Empty;
    }

    private static async Task<FolderInfoDto> EnsureFolderAsync(IKntService service, ResultBase result)
    {
        var resFolders = await service.Folders.GetAllAsync();
        if (!resFolders.IsValid)
        {
            result.AddListErrorMessage(resFolders.ListErrorMessage);
            return null;
        }

        var folder = resFolders.Entity?.FirstOrDefault(f => f.Name == KntConst.AiSessionsFolderName);
        if (folder != null)
            return folder;

        Result<FolderDto> resSave;
        using (KntAuthorizationBypass.Begin(service, InfrastructureBypassReason))
        {
            resSave = await service.Folders.SaveAsync(new FolderDto { Name = KntConst.AiSessionsFolderName });
        }
        if (!resSave.IsValid)
            result.AddListErrorMessage(resSave.ListErrorMessage);
        return resSave.Entity;
    }
}

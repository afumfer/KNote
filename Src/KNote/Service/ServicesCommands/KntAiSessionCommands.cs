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

    private Task<NoteDto> AddSessionAsync(UserDto user, AiSessionsInfrastructure infrastructure, Result<AiChatSessionDto> result) =>
        AiSessions.AddNoteAsync(Service, user, infrastructure,
            string.IsNullOrWhiteSpace(Param.Topic) ? AiSessions.TopicFromPrompt(Param.Turns.FirstOrDefault()?.Prompt) : Param.Topic,
            AiChatSessionTranscript.Write(Param.Turns), Param.Provider, Param.Model, result);

    private async Task<NoteDto> UpdateSessionAsync(UserDto user, AiSessionsInfrastructure infrastructure, Result<AiChatSessionDto> result)
    {
        var note = await AiSessions.GetUserSessionNoteAsync(Service, Param.NoteId, infrastructure, user, result);
        if (!result.IsValid)
            return null;

        if (!string.IsNullOrWhiteSpace(Param.Topic))
            note.Topic = Param.Topic;
        note.Description = AiChatSessionTranscript.Write(Param.Turns);
        AiSessions.SetSessionAttributes(note, infrastructure, Param.Provider, Param.Model);

        var resSave = await Service.Notes.SaveAsync(note);
        if (!resSave.IsValid)
        {
            result.AddListErrorMessage(resSave.ListErrorMessage);
            return null;
        }
        return resSave.Entity;
    }
}

[KntAuthorize(EnumRoles.Staff)]
public class KntAiSessionsAddAttachmentAsyncCommand : KntCommandServiceBase<AiChatAttachmentUploadDto, Result<AiChatAttachmentDto>>
{
    private const string AttachmentDescription = "AI assistant attachment";

    private byte[] _content;

    public KntAiSessionsAddAttachmentAsyncCommand(IKntService service, AiChatAttachmentUploadDto attachment) : base(service, attachment)
    {

    }

    public override Result ValidateParam()
    {
        var result = new Result();
        if (Param == null)
        {
            result.AddErrorMessage("The file is required.");
            return result;
        }

        var sizeError = AiAttachmentRules.SizeError(Name(), Content().LongLength);
        if (sizeError != null)
            result.AddErrorMessage(sizeError);
        if (Param.SessionNoteId == Guid.Empty && (string.IsNullOrWhiteSpace(Param.Provider) || string.IsNullOrWhiteSpace(Param.Model)))
            result.AddErrorMessage("The AI provider and model of the session are required.");
        return result;
    }

    public override async Task<Result<AiChatAttachmentDto>> Execute()
    {
        var result = new Result<AiChatAttachmentDto>();

        var user = await AiSessions.GetCurrentUserAsync(Service, result);
        var infrastructure = await AiSessions.EnsureInfrastructureAsync(Service, result);
        if (!result.IsValid)
            return result;

        var note = Param.SessionNoteId == Guid.Empty
            ? await AiSessions.AddNoteAsync(Service, user, infrastructure, AiSessions.TopicFromPrompt(Param.Topic), "", Param.Provider, Param.Model, result)
            : await AiSessions.GetUserSessionNoteAsync(Service, Param.SessionNoteId, infrastructure, user, result);
        if (!result.IsValid)
            return result;

        // A guid before the name, as the other resources of a note have it: two files with the same name don't share a file.
        var name = Name();
        var contentType = string.IsNullOrWhiteSpace(Param.ContentType) ? AiAttachmentRules.ContentTypeOf(name) : Param.ContentType;
        var saved = await Service.Notes.SaveResourceAsync(new ResourceDto
        {
            NoteId = note.NoteId,
            Name = $"{Guid.NewGuid()}_{name}",
            Description = AttachmentDescription,
            FileType = contentType,
            ContentArrayBytes = Content()
        });
        if (!saved.IsValid)
        {
            result.AddListErrorMessage(saved.ListErrorMessage);
            return result;
        }

        result.Entity = new AiChatAttachmentDto
        {
            ResourceId = saved.Entity.ResourceId,
            FileName = name,
            ContentType = contentType,
            SessionNoteId = note.NoteId
        };
        return result;
    }

    private string Name() => AiAttachmentRules.SafeFileName(Param.FileName);

    private byte[] Content()
    {
        if (_content == null)
        {
            try
            {
                _content = Convert.FromBase64String(Param.ContentBase64 ?? "");
            }
            catch (FormatException)
            {
                _content = Array.Empty<byte>();
            }
        }
        return _content;
    }
}

[KntAuthorize(EnumRoles.Staff)]
public class KntAiSessionsGetAttachmentAsyncCommand : KntCommandServiceBase<AiChatAttachmentDto, Result<AiChatAttachmentContentDto>>
{
    public KntAiSessionsGetAttachmentAsyncCommand(IKntService service, AiChatAttachmentDto attachment) : base(service, attachment)
    {

    }

    public override async Task<Result<AiChatAttachmentContentDto>> Execute()
    {
        var result = new Result<AiChatAttachmentContentDto>();

        var user = await AiSessions.GetCurrentUserAsync(Service, result);
        var infrastructure = await AiSessions.EnsureInfrastructureAsync(Service, result);
        if (!result.IsValid)
            return result;

        var note = await AiSessions.GetUserSessionNoteAsync(Service, Param.SessionNoteId, infrastructure, user, result);
        if (!result.IsValid)
            return result;

        var resource = await Service.Notes.GetResourceAsync(Param.ResourceId);
        if (!resource.IsValid || resource.Entity == null || resource.Entity.NoteId != note.NoteId)
        {
            result.AddErrorMessage("The attachment does not exist.");
            return result;
        }

        // Its file may be in the folder of another installation, if the repository does not keep the content in the database.
        var content = resource.Entity.ContentArrayBytes;
        if (content == null || content.Length == 0)
        {
            result.AddErrorMessage("The file of the attachment is not available here.");
            return result;
        }

        result.Entity = new AiChatAttachmentContentDto
        {
            FileName = resource.Entity.NameOut,
            ContentType = resource.Entity.FileType,
            ContentBase64 = Convert.ToBase64String(content)
        };
        return result;
    }
}

internal sealed record AiSessionsInfrastructure(Guid NoteTypeId, Guid ProviderAttributeId, Guid ModelAttributeId, FolderInfoDto Folder);

// What the AI session commands share (the infrastructure, with KntSystemNotes). Errors are added to the command's
// result instead of thrown, as every command reports them.
internal static class AiSessions
{
    private const string InfrastructureBypassReason = "AI assistant sessions: note type, attributes and folder";
    private const int MaxTopicLength = 80;

    public static Task<UserDto> GetCurrentUserAsync(IKntService service, ResultBase result) =>
        KntSystemNotes.GetCurrentUserAsync(service, result);

    // The note type, its two attributes and the folder of the sessions, created when missing.
    public static async Task<AiSessionsInfrastructure> EnsureInfrastructureAsync(IKntService service, ResultBase result)
    {
        var noteTypeId = await KntSystemNotes.EnsureNoteTypeAsync(service, KntConst.ChatSessionsTag, "AI Assistant sessions",
            InfrastructureBypassReason, result);
        if (!result.IsValid)
            return null;

        var providerAttributeId = await KntSystemNotes.EnsureAttributeAsync(service, noteTypeId, KntConst.AiProviderAttributeName,
            "AI provider of the session", 1, InfrastructureBypassReason, result);
        var modelAttributeId = await KntSystemNotes.EnsureAttributeAsync(service, noteTypeId, KntConst.AiModelAttributeName,
            "AI model of the session", 2, InfrastructureBypassReason, result);
        var folder = await KntSystemNotes.EnsureFolderAsync(service, KntConst.AiSessionsFolderName, InfrastructureBypassReason, result);
        if (!result.IsValid)
            return null;

        return new AiSessionsInfrastructure(noteTypeId, providerAttributeId, modelAttributeId, folder);
    }

    // A new session note, with the task that links it to its user.
    public static async Task<NoteDto> AddNoteAsync(IKntService service, UserDto user, AiSessionsInfrastructure infrastructure, string topic,
        string description, string provider, string model, ResultBase result)
    {
        // With the NoteType, KAttributesDto comes completed with the type's attributes, as for any new note.
        var resNew = await service.Notes.NewExtendedAsync(new NoteInfoDto { NoteTypeId = infrastructure.NoteTypeId });
        if (!resNew.IsValid)
        {
            result.AddListErrorMessage(resNew.ListErrorMessage);
            return null;
        }

        var note = resNew.Entity;
        note.Topic = topic;
        note.Description = description;
        // NewExtendedAsync leaves Tags null, and KntNotesSaveExtendedAsyncCommand needs it.
        note.Tags = "";
        // FolderDto too: it is validated with the note.
        note.FolderId = infrastructure.Folder.FolderId;
        note.FolderDto = infrastructure.Folder.GetSimpleDto<FolderDto>();
        note.NoteTypeId = infrastructure.NoteTypeId;
        SetSessionAttributes(note, infrastructure, provider, model);

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

        var resSave = await service.Notes.SaveExtendedAsync(note);
        if (!resSave.IsValid)
        {
            result.AddListErrorMessage(resSave.ListErrorMessage);
            return null;
        }
        return resSave.Entity;
    }

    public static void SetSessionAttributes(NoteDto note, AiSessionsInfrastructure infrastructure, string provider, string model)
    {
        SetAttributeValue(note, infrastructure.ProviderAttributeId, provider);
        SetAttributeValue(note, infrastructure.ModelAttributeId, model);
    }

    // The first line of the first prompt, shortened; a dated title when there is none.
    public static string TopicFromPrompt(string prompt)
    {
        var firstLine = (prompt ?? "")
            .ReplaceLineEndings("\n")
            .Split('\n')
            .Select(l => l.Trim())
            .FirstOrDefault(l => l.Length > 0);

        if (string.IsNullOrEmpty(firstLine))
            return $"AI session {DateTime.Now:yyyy-MM-dd HH:mm}";

        return firstLine.Length <= MaxTopicLength ? firstLine : firstLine.Substring(0, MaxTopicLength - 1) + "…";
    }

    // The session note, only if it is one (of the sessions' note type) and belongs to the user.
    public static async Task<NoteDto> GetUserSessionNoteAsync(IKntService service, Guid noteId,
        AiSessionsInfrastructure infrastructure, UserDto user, ResultBase result)
    {
        var note = await KntSystemNotes.GetNoteOfTypeAsync(service, noteId, infrastructure.NoteTypeId,
            "The AI session does not exist.", result);
        if (note == null)
            return null;

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
}

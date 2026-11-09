using System;
using System.Linq;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.Service.ServicesCommands;

[KntAuthorize(EnumRoles.Staff)]
public class KntResourceContentGetPhotoContextAsyncCommand : KntCommandServiceBase<Guid, Result<PhotoDescriptionContextDto>>
{
    public KntResourceContentGetPhotoContextAsyncCommand(IKntService service, Guid resourceId) : base(service, resourceId)
    {

    }

    public override async Task<Result<PhotoDescriptionContextDto>> Execute()
    {
        var result = new Result<PhotoDescriptionContextDto>();

        var (resource, note) = await KntResourceContent.GetDescribableAsync(Service, Param, result);
        if (!result.IsValid)
            return result;
        if (!AiPhotoRules.IsPhoto(resource))
        {
            result.AddErrorMessage("It is not a photo KNote can describe (PNG, JPEG, GIF or WEBP).");
            return result;
        }
        if (resource.DescriptionByUser)
        {
            result.AddErrorMessage("Its description is locked: the AI doesn't change it.");
            return result;
        }

        // Its file: read where it is (ContentArrayBytes, from the database or the folder of the resources).
        Service.Notes.UtilManageResourceContent(resource);
        if (resource.ContentArrayBytes == null || resource.ContentArrayBytes.Length == 0)
        {
            result.AddErrorMessage("The file of the photo is not available in this installation.");
            return result;
        }
        resource.LoadedDescription = resource.Description;

        var text = note.Description ?? "";
        if (string.Equals(note.GetContentTypeExt().ForDescription, "html", StringComparison.OrdinalIgnoreCase))
            text = Service.Notes.UtilHtmlToMarkdown(text);

        var others = await Repository.Notes.GetResourcesAsync(note.NoteId);
        result.Entity = new PhotoDescriptionContextDto
        {
            Resource = resource,
            NoteTopic = note.Topic,
            NoteText = AiPhotoRules.NoteText(text),
            PersonDescriptions = AiPhotoRules.PersonDescriptions((others.Entity ?? new()).Where(r => r.ResourceId != resource.ResourceId)),
            PhotoDescription = AiPhotoRules.PhotoDescription(resource)
        };
        return result;
    }
}

[KntAuthorize(EnumRoles.Staff)]
public class KntResourceContentSaveAiDescriptionAsyncCommand : KntCommandServiceBase<AiDescriptionSaveDto, Result<ResourceInfoDto>>
{
    public KntResourceContentSaveAiDescriptionAsyncCommand(IKntService service, AiDescriptionSaveDto description) : base(service, description)
    {

    }

    public override Result ValidateParam()
    {
        var result = new Result();
        if (Param == null || Param.ResourceId == Guid.Empty)
            result.AddErrorMessage("The resource is required.");
        else if (string.IsNullOrWhiteSpace(Param.Model))
            result.AddErrorMessage("The model that wrote the description is required.");
        else if (string.IsNullOrEmpty(AiPhotoRules.CleanDescription(Param.Description)))
            result.AddErrorMessage("The AI gave no description.");
        return result;
    }

    public override async Task<Result<ResourceInfoDto>> Execute()
    {
        var result = new Result<ResourceInfoDto>();

        var (_, note) = await KntResourceContent.GetDescribableAsync(Service, Param.ResourceId, result);
        if (!result.IsValid)
            return result;

        var written = await Repository.Notes.SaveResourceAiDescriptionAsync(Param.ResourceId,
            AiPhotoRules.CleanDescription(Param.Description), Param.Model.Trim(), Param.ExpectedDescription, Param.ExpectedName);
        if (!written.IsValid)
        {
            result.AddListErrorMessage(written.ListErrorMessage);
            return result;
        }
        if (!written.Entity)
        {
            result.AddErrorMessage("Its description changed or was locked while the AI was writing it: it was not written.");
            return result;
        }

        // Its fragment of the semantic index changes; the note itself doesn't (nor its date).
        await KntSemanticIndexer.MarkStaleAsync(Service, note.NoteId);

        var saved = await Repository.Notes.GetResourceAsync(Param.ResourceId);
        var info = saved.Entity.GetSimpleDto<ResourceInfoDto>();
        info.ContentBase64 = null;
        info.LoadedDescription = info.Description;
        (info.RelativeUrl, info.FullUrl) = Service.Notes.UtilGetResourceUrls(saved.Entity);
        result.Entity = info;
        return result;
    }
}

// What both commands check: the resource exists and is not of one of KNote's own notes (sessions, chat room...).
internal static class KntResourceContent
{
    public static async Task<(ResourceDto Resource, NoteDto Note)> GetDescribableAsync(IKntService service, Guid resourceId, ResultBase result)
    {
        var resource = await service.Repository.Notes.GetResourceAsync(resourceId);
        if (!resource.IsValid || resource.Entity == null)
        {
            result.AddErrorMessage("The resource does not exist.");
            return (null, null);
        }

        var note = await service.Notes.GetAsync(resource.Entity.NoteId);
        if (!note.IsValid || note.Entity == null)
        {
            result.AddErrorMessage("The note of the resource does not exist.");
            return (null, null);
        }
        if (KntConst.IsSystemNoteType(note.Entity.NoteTypeDto?.Name))
        {
            result.AddErrorMessage("The resources of KNote's own notes are not described.");
            return (null, null);
        }

        return (resource.Entity, note.Entity);
    }
}

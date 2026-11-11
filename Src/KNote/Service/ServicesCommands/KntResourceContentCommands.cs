using System;
using System.Collections.Generic;
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
            result.AddErrorMessage(AiPhotoRules.FileNotAvailableMessage);
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

[KntAuthorize(EnumRoles.Guest)]
public class KntResourceContentGetProcessingStatusAsyncCommand : KntCommandServiceBase<Result<ResourceProcessingStatusDto>>
{
    public KntResourceContentGetProcessingStatusAsyncCommand(IKntService service) : base(service)
    {

    }

    public override async Task<Result<ResourceProcessingStatusDto>> Execute()
    {
        var result = new Result<ResourceProcessingStatusDto>();

        var resources = await KntResourceContent.GetResourcesToDescribeAsync(Service, result);
        if (!result.IsValid)
            return result;

        var (pending, person) = ResourceProcessingRules.Count(resources);
        var state = await KntResourceProcessingState.ReadAsync(Service);
        result.Entity = new ResourceProcessingStatusDto
        {
            PendingPhotos = pending,
            PersonPhotos = person,
            LastRunDateTime = state?.LastRunDateTime,
            SecondsPerPhoto = state?.SecondsPerPhoto
        };
        return result;
    }
}

[KntAuthorize(EnumRoles.Admin)]
public class KntResourceContentPrepareProcessingAsyncCommand : KntCommandServiceBase<ResourceProcessingOptionsDto, Result<ResourceProcessingPlanDto>>
{
    public KntResourceContentPrepareProcessingAsyncCommand(IKntService service, ResourceProcessingOptionsDto options) : base(service, options)
    {

    }

    public override Result ValidateParam()
    {
        var result = new Result();
        if (Param == null || !Param.Photos)
            result.AddErrorMessage("Choose what to process.");
        return result;
    }

    public override async Task<Result<ResourceProcessingPlanDto>> Execute()
    {
        var result = new Result<ResourceProcessingPlanDto>();

        var resources = await KntResourceContent.GetResourcesToDescribeAsync(Service, result);
        if (!result.IsValid)
            return result;

        var (describe, lockThem) = ResourceProcessingRules.Plan(resources, Param);
        var plan = new ResourceProcessingPlanDto
        {
            PhotosToDescribe = describe,
            ReplacePersonDescriptions = Param.PersonDescriptions == EnumPersonDescriptions.Replace
        };

        // Kept as they are: only if nobody changed them meanwhile (a conditional write, as the AI's descriptions).
        foreach (var resource in lockThem)
        {
            var locked = await Repository.Notes.LockResourceDescriptionAsync(resource.ResourceId, resource.Description);
            if (locked.IsValid && locked.Entity)
                plan.Locked++;
        }

        result.Entity = plan;
        return result;
    }
}

[KntAuthorize(EnumRoles.Admin)]
public class KntResourceContentSaveProcessingRunAsyncCommand : KntCommandServiceBase<ResourceProcessingRunDto, Result>
{
    public KntResourceContentSaveProcessingRunAsyncCommand(IKntService service, ResourceProcessingRunDto run) : base(service, run)
    {

    }

    public override Result ValidateParam()
    {
        var result = new Result();
        if (Param == null || Param.PhotosDescribed < 0 || Param.Seconds < 0)
            result.AddErrorMessage("The run is not valid.");
        return result;
    }

    public override async Task<Result> Execute()
    {
        // The pace changes only with a run that described something: a canceled one at the first photo says nothing.
        var state = await KntResourceProcessingState.ReadAsync(Service) ?? new ResourceProcessingStateDto();
        state.LastRunDateTime = DateTime.Now;
        state.SecondsPerPhoto = ResourceProcessingRules.SecondsPerPhoto(Param.PhotosDescribed, Param.Seconds) ?? state.SecondsPerPhoto;
        return await KntResourceProcessingState.WriteAsync(Service, state);
    }
}

[KntAuthorize(EnumRoles.Staff)]
public class KntResourceContentReadDocumentAsyncCommand : KntCommandServiceBase<Guid, Result<DocumentReadDto>>
{
    public KntResourceContentReadDocumentAsyncCommand(IKntService service, Guid resourceId) : base(service, resourceId)
    {

    }

    public override async Task<Result<DocumentReadDto>> Execute()
    {
        var result = new Result<DocumentReadDto>();

        var (resource, note) = await KntResourceContent.GetDescribableAsync(Service, Param, result);
        if (!result.IsValid)
            return result;
        if (!DocumentTextRules.IsDocument(resource))
        {
            result.AddErrorMessage("It is not a document KNote reads (PDF, DOCX, ODT, TXT or MD).");
            return result;
        }

        var (outcome, read) = await KntDocuments.ReadAndSaveAsync(Service, Param);
        switch (outcome)
        {
            case EnumDocumentReadOutcome.NoReader:
                result.AddErrorMessage("This installation can't read documents.");
                return result;
            case EnumDocumentReadOutcome.NotAvailable:
                result.AddErrorMessage("The file of the document is not available in this installation.");
                return result;
            case EnumDocumentReadOutcome.Changed:
                result.AddErrorMessage("Its file was replaced while it was being read: read it again.");
                return result;
            case EnumDocumentReadOutcome.NotADocument:
                result.AddErrorMessage("It is not a document KNote reads (PDF, DOCX, ODT, TXT or MD).");
                return result;
        }

        // Its fragments of the semantic index change; the note itself doesn't (nor its date).
        await KntSemanticIndexer.MarkStaleAsync(Service, note.NoteId);

        var pages = DocumentTextRules.Pages(read.Text);
        result.Entity = new DocumentReadDto
        {
            ResourceId = Param,
            State = read.State,
            Pages = pages.Count,
            PagesToTranscribe = pages.Count(p => p.NeedsTranscription)
        };
        return result;
    }
}

/// <summary>The text read of a resource, its beginning only with a MaxLength (DocumentTextRules.Cut; 0: all of it).</summary>
public record DocumentTextRequest(Guid ResourceId, int MaxLength);

[KntAuthorize(EnumRoles.Guest)]
public class KntResourceContentGetDocumentTextAsyncCommand : KntCommandServiceBase<DocumentTextRequest, Result<ResourceTextDto>>
{
    public KntResourceContentGetDocumentTextAsyncCommand(IKntService service, DocumentTextRequest request) : base(service, request)
    {

    }

    public override async Task<Result<ResourceTextDto>> Execute()
    {
        var result = new Result<ResourceTextDto>();

        await KntResourceContent.GetDescribableAsync(Service, Param.ResourceId, result);
        if (!result.IsValid)
            return result;

        var text = await Repository.Notes.GetResourceTextAsync(Param.ResourceId);
        if (!text.IsValid)
        {
            result.AddListErrorMessage(text.ListErrorMessage);
            return result;
        }

        var entity = text.Entity;
        // Not read: no text (null), and no pages.
        if (entity?.Text != null)
            (entity.Text, entity.PagesShown, entity.PagesCount) = DocumentTextRules.Cut(entity.Text, Param.MaxLength);
        result.Entity = entity;
        return result;
    }
}

// What both commands check: the resource exists and is not of one of KNote's own notes (sessions, chat room...).
internal static class KntResourceContent
{
    // The resources the AI could describe, of the notes that are not KNote's own.
    public static async Task<List<ResourceInfoDto>> GetResourcesToDescribeAsync(IKntService service, ResultBase result)
    {
        var excluded = await KntSystemNotes.GetSystemNoteTypeIdsAsync(service.Repository);
        var resources = await service.Repository.Notes.GetResourcesToDescribeAsync(excluded);
        if (!resources.IsValid)
            result.AddListErrorMessage(resources.ListErrorMessage);
        return resources.Entity ?? new List<ResourceInfoDto>();
    }

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

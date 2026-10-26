using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using KNote.Model.Dto;
using KNote.Model;
using KNote.Service.Core;
using Microsoft.Extensions.Logging;

namespace KNote.Service.ServicesCommands;

[KntAuthorize(EnumRoles.Guest)]
public class KntNotesGetAllAsyncCommand : KntCommandServiceBase<Result<List<NoteInfoDto>>>
{
    public KntNotesGetAllAsyncCommand(IKntService service) : base(service)
    {

    }

    public override async Task<Result<List<NoteInfoDto>>> Execute()
    {
        return await Repository.Notes.GetAllAsync();
    }
}

[KntAuthorize(EnumRoles.Guest)]
public class KntNotesGetMinimalAllAsyncCommand : KntCommandServiceBase<Result<List<NoteMinimalDto>>>
{
    public KntNotesGetMinimalAllAsyncCommand(IKntService service) : base(service)
    {

    }

    public override async Task<Result<List<NoteMinimalDto>>> Execute()
    {
        return await Repository.Notes.GetAllMinimalAsync();
    }
}

[KntAuthorize(EnumRoles.Guest)]
public class KntNotesHomeAllAsyncCommand : KntCommandServiceBase<Result<List<NoteInfoDto>>>
{
    public KntNotesHomeAllAsyncCommand(IKntService service) : base(service)
    {

    }

    public override async Task<Result<List<NoteInfoDto>>> Execute()
    {
        return await Repository.Notes.HomeNotesAsync();
    }
}

[KntAuthorize(EnumRoles.Guest)]
public class KntNotesHomeFolderAsyncCommand : KntCommandServiceBase<Guid, Result<List<NoteInfoDto>>>
{
    public KntNotesHomeFolderAsyncCommand(IKntService service, Guid folderId) : base(service, folderId)
    {

    }

    public override async Task<Result<List<NoteInfoDto>>> Execute()
    {
        // Home or one of the folders directly inside it: the Web shows them without signing in, so this must
        // not read any other folder.
        var resHome = await Repository.Folders.GetHomeAsync();
        var homeId = resHome.Entity?.FolderId;
        if (homeId == null || (Param != homeId && (await Repository.Folders.GetAsync(Param)).Entity?.ParentId != homeId))
        {
            var result = new Result<List<NoteInfoDto>>();
            result.AddErrorMessage("The folder is not one of the folders of Home.");
            return result;
        }

        return await Repository.Notes.HomeNotesAsync(Param);
    }
}

[KntAuthorize(EnumRoles.Guest)]
public class KntNotesGetAsyncCommand : KntCommandServiceBase<Guid, Result<NoteDto>>
{
    public KntNotesGetAsyncCommand(IKntService service, Guid id) : base(service, id)
    {

    }

    public override async Task<Result<NoteDto>> Execute()
    {
        return await Repository.Notes.GetAsync(Param);
    }
}

[KntAuthorize(EnumRoles.Guest)]
public class KntNotesGetByNumberAsyncCommand : KntCommandServiceBase<int, Result<NoteDto>>
{
    public KntNotesGetByNumberAsyncCommand(IKntService service, int id) : base(service, id)
    {

    }

    public override async Task<Result<NoteDto>> Execute()
    {
        return await Repository.Notes.GetAsync(Param);
    }
}

[KntAuthorize(EnumRoles.Guest)]
public class KntNotesGetExtendedAsyncCommand : KntCommandServiceBase<Guid, Result<NoteExtendedDto>>
{
    public KntNotesGetExtendedAsyncCommand(IKntService service, Guid id) : base(service, id)
    {

    }

    public override async Task<Result<NoteExtendedDto>> Execute()
    {
        var result = new Result<NoteExtendedDto>();

        // None of these six depend on each other's result - each only needs Param (the note id) -
        // so they run concurrently instead of one round trip at a time. Safe because every
        // repository call opens (and closes) its own connection/DbContext: KntRepositoryFactory.Create
        // is the only place IKntRepository is constructed anywhere in the app, and it never uses the
        // singleton-connection constructor, so there is no shared mutable connection/context across
        // these calls to race on.
        var noteTask = Service.Notes.GetAsync(Param);
        var resourcesTask = Service.Notes.GetResourcesAsync(Param);
        var tasksTask = Service.Notes.GetNoteTasksAsync(Param);
        var messagesTask = Service.Notes.GetMessagesAsync(Param);
        var traceNotesFromTask = Service.Notes.GetTraceNotesFromAsync(Param);
        var traceNotesToTask = Service.Notes.GetTraceNotesToAsync(Param);

        await Task.WhenAll(noteTask, resourcesTask, tasksTask, messagesTask, traceNotesFromTask, traceNotesToTask);

        var entity = noteTask.Result.Entity.GetSimpleDto<NoteExtendedDto>();
        entity.Resources = resourcesTask.Result.Entity;
        entity.Tasks = tasksTask.Result.Entity;
        entity.Messages = messagesTask.Result.Entity;
        entity.TraceNotesFrom = traceNotesFromTask.Result.Entity;
        entity.TraceNotesTo = traceNotesToTask.Result.Entity;

        result.Entity = entity;
        return result;
    }
}

[KntAuthorize(EnumRoles.Guest)]
public class KntNotesGetByFolderAsyncCommand : KntCommandServiceBase<Guid, Result<List<NoteInfoDto>>>
{
    public KntNotesGetByFolderAsyncCommand(IKntService service, Guid folderId) : base(service, folderId)
    {

    }

    public override async Task<Result<List<NoteInfoDto>>> Execute()
    {
        return await Repository.Notes.GetByFolderAsync(Param);
    }
}

[KntAuthorize(EnumRoles.Guest)]
public class KntNotesGetMinimalByFolderAsyncCommand : KntCommandServiceBase<Guid, Result<List<NoteMinimalDto>>>
{
    public KntNotesGetMinimalByFolderAsyncCommand(IKntService service, Guid folderId) : base(service, folderId)
    {

    }

    public override async Task<Result<List<NoteMinimalDto>>> Execute()
    {
        return await Repository.Notes.GetByFolderMinimalAsync(Param);
    }
}


[KntAuthorize(EnumRoles.Guest)]
public class KntNotesGetFilterAsyncCommand : KntCommandServiceBase<NotesFilterDto, Result<List<NoteInfoDto>>>
{
    public KntNotesGetFilterAsyncCommand(IKntService service, NotesFilterDto notesFilter) : base(service, notesFilter)
    {

    }

    public override async Task<Result<List<NoteInfoDto>>> Execute()
    {
        return await Repository.Notes.GetFilterAsync(Param);
    }
}

[KntAuthorize(EnumRoles.Guest)]
public class KntNotesGetMinimalFilterAsyncCommand : KntCommandServiceBase<NotesFilterDto, Result<List<NoteMinimalDto>>>
{
    public KntNotesGetMinimalFilterAsyncCommand(IKntService service, NotesFilterDto notesFilter) : base(service, notesFilter)
    {

    }

    public override async Task<Result<List<NoteMinimalDto>>> Execute()
    {
        return await Repository.Notes.GetFilterMinimalAsync(Param);
    }
}

[KntAuthorize(EnumRoles.Guest)]
public class KntNotesGetSearchAsyncCommand : KntCommandServiceBase<NotesSearchDto, Result<List<NoteInfoDto>>>
{
    public KntNotesGetSearchAsyncCommand(IKntService service, NotesSearchDto notesSearch) : base(service, notesSearch)
    {

    }

    public override async Task<Result<List<NoteInfoDto>>> Execute()
    {
        return await Repository.Notes.GetSearchAsync(Param);
    }
}

[KntAuthorize(EnumRoles.Guest)]
public class KntNotesGetMinimalSearchAsyncCommand : KntCommandServiceBase<NotesSearchDto, Result<List<NoteMinimalDto>>>
{
    public KntNotesGetMinimalSearchAsyncCommand(IKntService service, NotesSearchDto notesSearch) : base(service, notesSearch)
    {

    }

    public override async Task<Result<List<NoteMinimalDto>>> Execute()
    {
        return await Repository.Notes.GetSearchMinimalAsync(Param);
    }
}

[KntAuthorize(EnumRoles.Staff)]
public class KntNotesNewAsyncCommand : KntCommandServiceBase<NoteInfoDto, Result<NoteDto>>
{
    public KntNotesNewAsyncCommand(IKntService service, NoteInfoDto entityInfo) : base(service, entityInfo)
    {

    }

    public override async Task<Result<NoteDto>> Execute()
    {
        return await Repository.Notes.NewAsync(Param);
    }
}

[KntAuthorize(EnumRoles.Staff)]
public class KntNotesNewExtendedAsyncCommand : KntCommandServiceBase<NoteInfoDto, Result<NoteExtendedDto>>
{
    public KntNotesNewExtendedAsyncCommand(IKntService service, NoteInfoDto entityInfo) : base(service, entityInfo)
    {

    }

    public override async Task<Result<NoteExtendedDto>> Execute()
    {
        var result = new Result<NoteExtendedDto>();

        var entity = (await Repository.Notes.NewAsync(Param)).Entity.GetSimpleDto<NoteExtendedDto>();

        result.Entity = entity;
        return result;
    }
}

[KntAuthorize(EnumRoles.Staff)]
public class KntNotesSaveAsyncCommand : KntCommandSaveServiceBase<NoteDto, Result<NoteDto>>
{
    private readonly bool UpdateStatus;
    public KntNotesSaveAsyncCommand(IKntService service, NoteDto entity, bool updateStatus = true) : base(service, entity)
    {
        UpdateStatus = updateStatus;
    }

    public override async Task<Result<NoteDto>> Execute()
    {
        Result<NoteDto> res;

        if (Param.NoteId == Guid.Empty)
        {
            Param.NoteId = Guid.NewGuid();
            res = await Repository.Notes.AddAsync(Param);
            return res;
        }
        else
        {
            if (UpdateStatus)
            {
                Param.InternalTags = Service.Notes.UtilGetNoteStatus((await Service.Notes.GetNoteTasksAsync(Param.NoteId)).Entity, (await Service.Notes.GetMessagesAsync(Param.NoteId)).Entity);
            }

            var resExist = await Repository.Notes.GetAsync(Param.NoteId);
            if (resExist.Entity == null)
            {
                res = await Repository.Notes.AddAsync(Param);
            }
            else
            {
                res = await Repository.Notes.UpdateAsync(Param);
            }

            return res;
        }
    }
}

[KntAuthorize(EnumRoles.Staff)]
public class KntNotesSaveExtendedAsyncCommand : KntCommandSaveServiceBase<NoteExtendedDto, Result<NoteExtendedDto>>
{    
    public KntNotesSaveExtendedAsyncCommand(IKntService service, NoteExtendedDto entity) : base(service, entity)
    {        
    }

    public override async Task<Result<NoteExtendedDto>> Execute()
    {
        var result = new Result<NoteExtendedDto>();

        Param.InternalTags = Service.Notes.UtilGetNoteStatus(Param.Tasks, Param.Messages);

        if (Param.IsDirty())
        {
            var resNote = await Service.Notes.SaveAsync(Param.GetSimpleDto<NoteDto>(), false);
            result.Entity = resNote.Entity.GetSimpleDto<NoteExtendedDto>();
        }
        else
            result.Entity = Param;

        var noteEdited = result.Entity;

        foreach (var item in Param.Messages)
        {
            if (item.IsDeleted())
            {
                var res = await Service.Notes.DeleteMessageAsync(item.KMessageId);
                if (!res.IsValid)
                    result.AddListErrorMessage(res.ListErrorMessage);
            }
            else if (item.IsDirty())
            {
                if (item.NoteId == Guid.Empty)
                    item.NoteId = noteEdited.NoteId;
                var res = await Service.Notes.SaveMessageAsync(item, true);
                if (!res.IsValid)
                    result.AddListErrorMessage(res.ListErrorMessage);
                else
                    result.Entity.Messages.Add(res.Entity);
            }
            else
                result.Entity.Messages.Add(item);
        }
        Param.Messages.RemoveAll(m => m.IsDeleted());

        foreach (var item in Param.Resources)
        {
            if (item.IsDeleted())
            {
                var res = await Service.Notes.DeleteResourceAsync(item.ResourceId);
                if (!res.IsValid)
                    result.AddListErrorMessage(res.ListErrorMessage);
            }
            else if (item.IsDirty())
            {
                if (item.NoteId == Guid.Empty)
                    item.NoteId = noteEdited.NoteId;
                var res = await Service.Notes.SaveResourceAsync(item, true);
                if (!res.IsValid)
                    result.AddListErrorMessage(res.ListErrorMessage);
                else
                    result.Entity.Resources.Add(res.Entity);
            }
            else
                result.Entity.Resources.Add(item);
        }
        Param.Resources.RemoveAll(r => r.IsDeleted());

        foreach (var item in Param.Tasks)
        {
            if (item.IsDeleted())
            {
                var res = await Service.Notes.DeleteNoteTaskAsync(item.NoteTaskId);
                if (!res.IsValid)
                    result.AddListErrorMessage(res.ListErrorMessage);
            }
            else if (item.IsDirty())
            {
                if (item.NoteId == Guid.Empty)
                    item.NoteId = noteEdited.NoteId;
                var res = await Service.Notes.SaveNoteTaskAsync(item, true);
                if (!res.IsValid)
                    result.AddListErrorMessage(res.ListErrorMessage);
                else
                    result.Entity.Tasks.Add(res.Entity);
            }
            else
                result.Entity.Tasks.Add(item);

        }
        Param.Tasks.RemoveAll(t => t.IsDeleted());

        foreach (var item in Param.TraceNotesFrom) // incoming relations: this note is ToId
        {
            if (item.IsDeleted())
            {
                var res = await Service.Notes.DeleteTraceNoteAsync(item.TraceNoteId);
                if (!res.IsValid)
                    result.AddListErrorMessage(res.ListErrorMessage);
            }
            else if (item.IsDirty())
            {
                if (item.ToId == Guid.Empty)
                    item.ToId = noteEdited.NoteId;
                var res = await Service.Notes.SaveTraceNoteAsync(item, true);
                if (!res.IsValid)
                    result.AddListErrorMessage(res.ListErrorMessage);
                else
                    result.Entity.TraceNotesFrom.Add(res.Entity);
            }
            else
                result.Entity.TraceNotesFrom.Add(item);
        }
        Param.TraceNotesFrom.RemoveAll(t => t.IsDeleted());

        foreach (var item in Param.TraceNotesTo) // outgoing relations: this note is FromId
        {
            if (item.IsDeleted())
            {
                var res = await Service.Notes.DeleteTraceNoteAsync(item.TraceNoteId);
                if (!res.IsValid)
                    result.AddListErrorMessage(res.ListErrorMessage);
            }
            else if (item.IsDirty())
            {
                if (item.FromId == Guid.Empty)
                    item.FromId = noteEdited.NoteId;
                var res = await Service.Notes.SaveTraceNoteAsync(item, true);
                if (!res.IsValid)
                    result.AddListErrorMessage(res.ListErrorMessage);
                else
                    result.Entity.TraceNotesTo.Add(res.Entity);
            }
            else
                result.Entity.TraceNotesTo.Add(item);
        }
        Param.TraceNotesTo.RemoveAll(t => t.IsDeleted());

        // Send note to message Broker

        if (Param.Tags?.Contains(KntConst.TagForMerging) == true)
        {
            Service.PublishNoteInMessageBroker(Param);
        }

        return result;
    }
}

[KntAuthorize(EnumRoles.Staff)]
public class KntNotesDeleteAsyncCommand : KntCommandServiceBase<Guid, Result<NoteDto>>
{
    public KntNotesDeleteAsyncCommand(IKntService service, Guid id) : base(service, id)
    {

    }

    public override async Task<Result<NoteDto>> Execute()
    {
        var result = new Result<NoteDto>();

        var resGetEntity = await Service.Notes.GetAsync(Param);

        if (resGetEntity.IsValid)
        {
            var resDelEntity = await Repository.Notes.DeleteAsync(Param);
            if (resDelEntity.IsValid)
                result.Entity = resGetEntity.Entity;
            else
                result.AddListErrorMessage(resDelEntity.ListErrorMessage);
        }
        else
        {
            result.AddListErrorMessage(resGetEntity.ListErrorMessage);
        }

        return result;
    }
}

[KntAuthorize(EnumRoles.Staff)]
public class KntNotesDeleteExtendedAsyncCommand : KntCommandServiceBase<Guid, Result<NoteExtendedDto>>
{
    public KntNotesDeleteExtendedAsyncCommand(IKntService service, Guid id) : base(service, id)
    {

    }

    public override async Task<Result<NoteExtendedDto>> Execute()
    {
        var result = new Result<NoteExtendedDto>();

        var neForDelete = (await Service.Notes.GetExtendedAsync(Param)).Entity;

        foreach (var item in neForDelete.Messages)
        {
            var res = await Service.Notes.DeleteMessageAsync(item.KMessageId);
            if (!res.IsValid)
                result.AddListErrorMessage(res.ListErrorMessage);
        }
        foreach (var item in neForDelete.Resources)
        {
            var res = await Service.Notes.DeleteResourceAsync(item.ResourceId);
            if (!res.IsValid)
                result.AddListErrorMessage(res.ListErrorMessage);
        }
        foreach (var item in neForDelete.Tasks)
        {
            var res = await Service.Notes.DeleteNoteTaskAsync(item.NoteTaskId);
            if (!res.IsValid)
                result.AddListErrorMessage(res.ListErrorMessage);
        }
        // TraceNotes' FromId/ToId FKs use DeleteBehavior.Restrict (see ModelBuilderExtensions),
        // deliberately - not Cascade, because both FKs reference the same Notes table and EF Core/
        // SQL Server disallow cascading both. So this note's trace relations - in both directions -
        // must be deleted explicitly here before the note itself, or the FK would reject the delete.
        foreach (var item in neForDelete.TraceNotesFrom)
        {
            var res = await Service.Notes.DeleteTraceNoteAsync(item.TraceNoteId);
            if (!res.IsValid)
                result.AddListErrorMessage(res.ListErrorMessage);
        }
        foreach (var item in neForDelete.TraceNotesTo)
        {
            var res = await Service.Notes.DeleteTraceNoteAsync(item.TraceNoteId);
            if (!res.IsValid)
                result.AddListErrorMessage(res.ListErrorMessage);
        }

        var resNote = await Service.Notes.DeleteAsync(Param);
        if (!resNote.IsValid)
            result.AddListErrorMessage(resNote.ListErrorMessage);

        result.Entity = neForDelete;

        return result;
    }
}


[KntAuthorize(EnumRoles.Guest)]
public class KntNotesGetResourcesAsyncCommand : KntCommandServiceBase<Guid, Result<List<ResourceDto>>>
{
    public KntNotesGetResourcesAsyncCommand(IKntService service, Guid id) : base(service, id)
    {

    }

    public override async Task<Result<List<ResourceDto>>> Execute()
    {
        var res = await Repository.Notes.GetResourcesAsync(Param);
        if (res.IsValid)
            foreach (var r in res.Entity)
                Service.Notes.UtilManageResourceContent(r);
        return res;
    }
}

[KntAuthorize(EnumRoles.Guest)]
public class KntNotesGetResourcesInfoAsyncCommand : KntCommandServiceBase<Guid, Result<List<ResourceInfoDto>>>
{
    public KntNotesGetResourcesInfoAsyncCommand(IKntService service, Guid id) : base(service, id)
    {

    }

    public override async Task<Result<List<ResourceInfoDto>>> Execute()
    {
        var res = new Result<List<ResourceInfoDto>>();

        var resGetResources = await Service.Notes.GetResourcesAsync(Param);

        res.AddListErrorMessage(resGetResources.ListErrorMessage);
        res.Entity = new List<ResourceInfoDto>();
        foreach (var r in resGetResources.Entity)
            res.Entity.Add(r.GetSimpleDto<ResourceInfoDto>());

        return res;
    }
}

[KntAuthorize(EnumRoles.Guest)]
public class KntNotesGetResourceAsyncCommand : KntCommandServiceBase<Guid, Result<ResourceDto>>
{
    public KntNotesGetResourceAsyncCommand(IKntService service, Guid id) : base(service, id)
    {

    }

    public override async Task<Result<ResourceDto>> Execute()
    {
        var res = await Repository.Notes.GetResourceAsync(Param);
        if (res.IsValid)
            Service.Notes.UtilManageResourceContent(res.Entity);
        return res;
    }
}

[KntAuthorize(EnumRoles.Staff)]
public class KntNotesSaveResourceAsyncCommand : KntCommandSaveServiceBase<ResourceDto, Result<ResourceDto>>
{
    private readonly bool forceNew;
    public KntNotesSaveResourceAsyncCommand(IKntService service, ResourceDto resource, bool forceNew = false) : base(service, resource)
    {
        this.forceNew = forceNew;
    }

    public override async Task<Result<ResourceDto>> Execute()
    {
        Result<ResourceDto> result;

        var hasNewContent = Param.ContentArrayBytes != null;
        var fileManaged = Service.Notes.UtilManageResourceContent(Param);

        // Without its file (and its content not in the database) the resource would be saved empty.
        if (hasNewContent && !fileManaged && !Param.ContentInDB)
        {
            result = new Result<ResourceDto>();
            result.AddErrorMessage("The resource file couldn't be saved on the server (see the server log: the folder of the resources may lack permissions).");
            return result;
        }

        var tmpContent = Param.ContentArrayBytes;

        if (!Param.ContentInDB)
            Param.ContentArrayBytes = null;

        if (Param.ResourceId == Guid.Empty)
        {
            Param.ResourceId = Guid.NewGuid();
            result = await Repository.Notes.AddResourceAsync(Param);
        }
        else
        {
            var checkExist = await Service.Notes.GetResourceAsync(Param.ResourceId);

            // Delete old resource file
            if (checkExist.IsValid)
            {
                var oldResource = checkExist.Entity;

                if (oldResource.Name != Param.Name && oldResource.ContentInDB == false)
                {
                    var oldFile = Service.Notes.UtilGetResourceFilePath(oldResource);
                    try
                    {
                        if (File.Exists(oldFile))
                            File.Delete(oldFile);
                    }
                    catch (Exception ex)
                    {
                        Service.Logger?.LogError(ex, "Resource {name}: the old file {file} couldn't be deleted.", Param.Name, oldFile);
                    }
                }
            }

            if (!forceNew)
            {
                result = await Repository.Notes.UpdateResourceAsync(Param);
            }
            else
            {

                if (checkExist.IsValid)
                    result = await Repository.Notes.UpdateResourceAsync(Param);
                else
                    result = await Repository.Notes.AddResourceAsync(Param);
            }
        }

        if (!result.Entity.ContentInDB)
            result.Entity.ContentArrayBytes = tmpContent;

        (result.Entity.RelativeUrl, result.Entity.FullUrl) = Service.Notes.UtilGetResourceUrls(result.Entity);

        return result;
    }
}

[KntAuthorize(EnumRoles.Staff)]
public class KntNotesSaveResourceInfoAsyncCommand : KntCommandSaveServiceBase<ResourceInfoDto, Result<ResourceInfoDto>>
{
    private readonly bool forceNew;
    public KntNotesSaveResourceInfoAsyncCommand(IKntService service, ResourceInfoDto resource, bool forceNew = false) : base(service, resource)
    {
        this.forceNew = forceNew;
    }

    public override async Task<Result<ResourceInfoDto>> Execute()
    {
        if (Param == null)
            throw new ArgumentException("Resource can't be null");

        var res = new Result<ResourceInfoDto>();

        ResourceDto resource;
        if (Param.ResourceId == Guid.Empty || forceNew)
            resource = Param.GetSimpleDto<ResourceDto>();
        else
        {
            // An existing resource: only its description and order change. Its file, and its content when it is kept
            // in the database (ResourceInfoDto doesn't say so), stay as they are.
            var resGetResource = await Service.Notes.GetResourceAsync(Param.ResourceId);
            if (!resGetResource.IsValid)
            {
                res.AddListErrorMessage(resGetResource.ListErrorMessage);
                return res;
            }
            resource = resGetResource.Entity;
            resource.Description = Param.Description;
            resource.Order = Param.Order;
        }

        var resSaveResource = await Service.Notes.SaveResourceAsync(resource, forceNew);

        res.AddListErrorMessage(resSaveResource.ListErrorMessage);
        res.Entity = resSaveResource.Entity?.GetSimpleDto<ResourceInfoDto>();

        return res;
    }
}

[KntAuthorize(EnumRoles.Staff)]
public class KntNotesDeleteResourceAsyncCommand : KntCommandServiceBase<Guid, Result<ResourceDto>>
{    
    public KntNotesDeleteResourceAsyncCommand(IKntService service, Guid id) : base(service, id)
    {      
        
    }

    public override async Task<Result<ResourceDto>> Execute()
    {
        var result = new Result<ResourceDto>();

        var resGetEntity = await Repository.Notes.GetResourceAsync(Param);
        if (resGetEntity.IsValid)
        {
            var resDelEntity = await Repository.Notes.DeleteResourceAsync(Param);
            if (resDelEntity.IsValid)
            {
                result.Entity = resGetEntity.Entity;
                try
                {
                    var fullPathRec = Service.Notes.UtilGetResourceFilePath(result.Entity);
                    if (!string.IsNullOrEmpty(fullPathRec) && File.Exists(fullPathRec))
                        File.Delete(fullPathRec);
                }
                catch (Exception ex)
                {
                    // The resource is already deleted: its file is left behind.
                    Service.Logger?.LogError(ex, "Resource {name}: its file couldn't be deleted.", result.Entity.Name);
                }
            }
            else
                result.AddListErrorMessage(resDelEntity.ListErrorMessage);
        }
        else
        {
            result.AddListErrorMessage(resGetEntity.ListErrorMessage);
        }

        return result;
    }
}

[KntAuthorize(EnumRoles.Staff)]
public class KntNotesDeleteResourceInfoAsyncCommand : KntCommandServiceBase<Guid, Result<ResourceInfoDto>>
{
    public KntNotesDeleteResourceInfoAsyncCommand(IKntService service, Guid id) : base(service, id)
    {

    }

    public override async Task<Result<ResourceInfoDto>> Execute()
    {
        var res = new Result<ResourceInfoDto>();

        var resDelete = await Service.Notes.DeleteResourceAsync(Param);
        res.AddListErrorMessage(resDelete.ListErrorMessage);
        res.Entity = resDelete.Entity.GetSimpleDto<ResourceInfoDto>();
        return res;
    }
}

[KntAuthorize(EnumRoles.Guest)]
public class KntNotesGetNoteTasksAsyncCommand : KntCommandServiceBase<Guid, Result<List<NoteTaskDto>>>
{
    public KntNotesGetNoteTasksAsyncCommand(IKntService service, Guid id) : base(service, id)
    {

    }

    public override async Task<Result<List<NoteTaskDto>>> Execute()
    {
        return await Repository.Notes.GetNoteTasksAsync(Param);
    }
}

[KntAuthorize(EnumRoles.Guest)]
public class KntNotesGetStartedTasksByDateTimeRageAsyncCommand : KntCommandServiceBase< Result<List<NoteTaskDto>>>
{
    private readonly DateTime startDateTime;
    private readonly DateTime endDateTime;

    public KntNotesGetStartedTasksByDateTimeRageAsyncCommand(IKntService service, DateTime startDateTime, DateTime endDateTime) : base(service)
    {
        this.startDateTime = startDateTime;
        this.endDateTime = endDateTime;
    }

    public override async Task<Result<List<NoteTaskDto>>> Execute()
    {
        return await Repository.Notes.GetStartedTasksByDateTimeRageAsync(startDateTime, endDateTime);
    }
}

[KntAuthorize(EnumRoles.Guest)]
public class KntNotesGetEstimatedTasksByDateTimeRageAsyncCommand : KntCommandServiceBase<Result<List<NoteTaskDto>>>
{
    private readonly DateTime startDateTime;
    private readonly DateTime endDateTime;

    public KntNotesGetEstimatedTasksByDateTimeRageAsyncCommand(IKntService service, DateTime startDateTime, DateTime endDateTime) : base(service)
    {
        this.startDateTime = startDateTime;
        this.endDateTime = endDateTime;
    }

    public override async Task<Result<List<NoteTaskDto>>> Execute()
    {
        return await Repository.Notes.GetEstimatedTasksByDateTimeRageAsync(startDateTime, endDateTime);
    }
}

[KntAuthorize(EnumRoles.Guest)]
public class KntNotesGetNoteTaskAsyncCommand : KntCommandServiceBase<Guid, Result<NoteTaskDto>>
{
    public KntNotesGetNoteTaskAsyncCommand(IKntService service, Guid id) : base(service, id)
    {

    }

    public override async Task<Result<NoteTaskDto>> Execute()
    {
        return await Repository.Notes.GetNoteTaskAsync(Param);
    }
}

[KntAuthorize(EnumRoles.Staff)]
public class KntNotesSaveNoteTaskAsyncCommand : KntCommandSaveServiceBase<NoteTaskDto, Result<NoteTaskDto>>
{
    private readonly bool forceNew;
    public KntNotesSaveNoteTaskAsyncCommand(IKntService service, NoteTaskDto entity, bool forceNew = false) : base(service, entity)
    {
        this.forceNew = forceNew;
    }

    public override async Task<Result<NoteTaskDto>> Execute()
    {
        if (Param.NoteTaskId == Guid.Empty)
        {
            Param.NoteTaskId = Guid.NewGuid();
            return await Repository.Notes.AddNoteTaskAsync(Param);
        }
        else
        {
            if (!forceNew)
            {
                return await Repository.Notes.UpdateNoteTaskAsync(Param);
            }
            else
            {
                var checkExist = await Service.Notes.GetNoteTaskAsync(Param.NoteTaskId);
                if (checkExist.IsValid)
                    return await Repository.Notes.UpdateNoteTaskAsync(Param);
                else
                    return await Repository.Notes.AddNoteTaskAsync(Param);
            }
        }
    }
}

[KntAuthorize(EnumRoles.Staff)]
public class KntNotesDeleteNoteTaskAsyncCommand : KntCommandServiceBase<Guid, Result<NoteTaskDto>>
{    
    public KntNotesDeleteNoteTaskAsyncCommand(IKntService service, Guid id) : base(service, id)
    {
        
    }

    public override async Task<Result<NoteTaskDto>> Execute()
    {
        var result = new Result<NoteTaskDto>();

        var resGetEntity = await Repository.Notes.GetNoteTaskAsync(Param);
        if (resGetEntity.IsValid)
        {
            var resDelEntity = await Repository.Notes.DeleteNoteTaskAsync(Param);
            if (resDelEntity.IsValid)
                result.Entity = resGetEntity.Entity;
            else
                result.AddListErrorMessage(resDelEntity.ListErrorMessage);
        }
        else
        {
            result.AddListErrorMessage(resGetEntity.ListErrorMessage);
        }

        return result;
    }
}

[KntAuthorize(EnumRoles.Guest)]
public class KntNotesGetMessagesAsyncCommand : KntCommandServiceBase<Guid, Result<List<KMessageDto>>>
{
    public KntNotesGetMessagesAsyncCommand(IKntService service, Guid id) : base(service, id)
    {

    }

    public override async Task<Result<List<KMessageDto>>> Execute()
    {
        return await Repository.Notes.GetMessagesAsync(Param);
    }
}

[KntAuthorize(EnumRoles.Guest)]
public class KntNotesGetMessageAsyncCommand : KntCommandServiceBase<Guid, Result<KMessageDto>>
{
    public KntNotesGetMessageAsyncCommand(IKntService service, Guid id) : base(service, id)
    {

    }

    public override async Task<Result<KMessageDto>> Execute()
    {
        return await Repository.Notes.GetMessageAsync(Param);
    }
}

/// <summary>
/// A new alarm of the note, not saved yet: for the user running the service, activated, for now (to the minute), shown
/// as Application info (the notification the web app gives) and with a comment to replace.
/// </summary>
[KntAuthorize(EnumRoles.Staff)]
public class KntNotesNewMessageAsyncCommand : KntCommandServiceBase<Guid, Result<KMessageDto>>
{
    public const string DefaultComment = "(Additional text for the alarm)";

    public KntNotesNewMessageAsyncCommand(IKntService service, Guid noteId) : base(service, noteId)
    {

    }

    public override async Task<Result<KMessageDto>> Execute()
    {
        var user = await Service.GetCurrentUserAsync();
        var now = DateTime.Now;

        var message = new KMessageDto
        {
            NoteId = Param,
            UserId = user?.UserId,
            UserFullName = user?.FullName,
            AlarmActivated = true,
            AlarmDateTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Kind),
            AlarmType = EnumAlarmType.Standard,
            NotificationType = EnumNotificationType.AppInfo,
            Comment = DefaultComment
        };
        message.SetIsNew(true);

        return new Result<KMessageDto>(message);
    }
}

[KntAuthorize(EnumRoles.Staff)]
public class KntNotesSaveMessageAsyncCommand : KntCommandSaveServiceBase<KMessageDto, Result<KMessageDto>>
{
    private readonly bool forceNew;
    public KntNotesSaveMessageAsyncCommand(IKntService service, KMessageDto entity, bool forceNew = false) : base(service, entity)
    {
        this.forceNew = forceNew;
    }

    public override async Task<Result<KMessageDto>> Execute()
    {
        Result<KMessageDto> resSavedEntity;

        if (Param.KMessageId == Guid.Empty)
        {
            Param.KMessageId = Guid.NewGuid();
            resSavedEntity = await Repository.Notes.AddMessageAsync(Param);
        }
        else
        {
            if (!forceNew)
            {
                resSavedEntity = await Repository.Notes.UpdateMessageAsync(Param);
            }
            else
            {
                var checkExist = await Service.Notes.GetMessageAsync(Param.KMessageId);
                if (checkExist.IsValid)
                    resSavedEntity = await Repository.Notes.UpdateMessageAsync(Param);
                else
                    resSavedEntity = await Repository.Notes.AddMessageAsync(Param);
            }
        }

        resSavedEntity.Entity.UserFullName = Param.UserFullName;
        return resSavedEntity;
    }
}

[KntAuthorize(EnumRoles.Staff)]
public class KntNotesDeleteMessageAsyncCommand : KntCommandServiceBase<Guid, Result<KMessageDto>>
{
    public KntNotesDeleteMessageAsyncCommand(IKntService service, Guid id) : base(service, id)
    {

    }

    public override async Task<Result<KMessageDto>> Execute()
    {
        var result = new Result<KMessageDto>();

        var resGetEntity = await Service.Notes.GetMessageAsync(Param);

        if (resGetEntity.IsValid)
        {
            var resDelEntity = await Repository.Notes.DeleteMessageAsync(Param);
            if (resDelEntity.IsValid)
                result.Entity = resGetEntity.Entity;
            else
                result.AddListErrorMessage(resDelEntity.ListErrorMessage);
        }
        else
        {
            result.AddListErrorMessage(resGetEntity.ListErrorMessage);
        }

        return result;
    }
}

[KntAuthorize(EnumRoles.Guest)]
public class KntNotesGetTraceNotesFromAsyncCommand : KntCommandServiceBase<Guid, Result<List<TraceNoteDto>>>
{
    public KntNotesGetTraceNotesFromAsyncCommand(IKntService service, Guid id) : base(service, id)
    {

    }

    // "From" list (incoming relations, ToId == Param) - Fase 0, decision 4.
    public override async Task<Result<List<TraceNoteDto>>> Execute()
    {
        return await Repository.TraceNotes.GetAllByToIdAsync(Param);
    }
}

[KntAuthorize(EnumRoles.Guest)]
public class KntNotesGetTraceNotesToAsyncCommand : KntCommandServiceBase<Guid, Result<List<TraceNoteDto>>>
{
    public KntNotesGetTraceNotesToAsyncCommand(IKntService service, Guid id) : base(service, id)
    {

    }

    // "To" list (outgoing relations, FromId == Param) - Fase 0, decision 4.
    public override async Task<Result<List<TraceNoteDto>>> Execute()
    {
        return await Repository.TraceNotes.GetAllByFromIdAsync(Param);
    }
}

[KntAuthorize(EnumRoles.Staff)]
public class KntNotesSaveTraceNoteAsyncCommand : KntCommandSaveServiceBase<TraceNoteDto, Result<TraceNoteDto>>
{
    private readonly bool forceNew;
    public KntNotesSaveTraceNoteAsyncCommand(IKntService service, TraceNoteDto entity, bool forceNew = false) : base(service, entity)
    {
        this.forceNew = forceNew;
    }

    public override async Task<Result<TraceNoteDto>> Execute()
    {
        // The same two notes, in the same direction, can only be traced once per type. The database unique index
        // (FromId, ToId, TraceNoteTypeId) does not block the untyped relations (NULL type), so it is checked here,
        // for every caller (ClientWin's note editor, the web, KntScript) and both ORMs.
        var resDuplicate = await CheckNotDuplicatedAsync();
        if (!resDuplicate.IsValid)
            return resDuplicate;

        if (Param.TraceNoteId == Guid.Empty)
        {
            Param.TraceNoteId = Guid.NewGuid();
            return await Repository.TraceNotes.AddAsync(Param);
        }
        else
        {
            if (!forceNew)
            {
                return await Repository.TraceNotes.UpdateAsync(Param);
            }
            else
            {
                var checkExist = await Repository.TraceNotes.GetAsync(Param.TraceNoteId);
                if (checkExist.IsValid)
                    return await Repository.TraceNotes.UpdateAsync(Param);
                else
                    return await Repository.TraceNotes.AddAsync(Param);
            }
        }
    }

    private async Task<Result<TraceNoteDto>> CheckNotDuplicatedAsync()
    {
        var result = new Result<TraceNoteDto>();

        var resExisting = await Repository.TraceNotes.GetAllByFromIdAsync(Param.FromId);
        if (!resExisting.IsValid)
        {
            result.AddListErrorMessage(resExisting.ListErrorMessage);
            return result;
        }

        if (resExisting.Entity.Any(t => t.ToId == Param.ToId && t.TraceNoteTypeId == Param.TraceNoteTypeId && t.TraceNoteId != Param.TraceNoteId))
            result.AddErrorMessage("These notes are already traced with the same type (a relation can only be added once).");

        return result;
    }
}

[KntAuthorize(EnumRoles.Staff)]
public class KntNotesDeleteTraceNoteAsyncCommand : KntCommandServiceBase<Guid, Result<TraceNoteDto>>
{
    public KntNotesDeleteTraceNoteAsyncCommand(IKntService service, Guid id) : base(service, id)
    {

    }

    public override async Task<Result<TraceNoteDto>> Execute()
    {
        var result = new Result<TraceNoteDto>();

        var resGetEntity = await Repository.TraceNotes.GetAsync(Param);
        if (resGetEntity.IsValid)
        {
            var resDelEntity = await Repository.TraceNotes.DeleteAsync(Param);
            if (resDelEntity.IsValid)
                result.Entity = resGetEntity.Entity;
            else
                result.AddListErrorMessage(resDelEntity.ListErrorMessage);
        }
        else
        {
            result.AddListErrorMessage(resGetEntity.ListErrorMessage);
        }

        return result;
    }
}

[KntAuthorize(EnumRoles.Guest)]
public class KntNotesGetWindowAsyncCommand : KntCommandServiceBase<Result<WindowDto>>
{
    private readonly Guid noteId;
    private readonly Guid userId;
    public KntNotesGetWindowAsyncCommand(IKntService service, Guid noteId, Guid userId) : base(service)
    {
        this.noteId = noteId;
        this.userId = userId;
    }

    public override async Task<Result<WindowDto>> Execute()
    {
        return await Repository.Notes.GetWindowAsync(noteId, userId);
    }
}

[KntAuthorize(EnumRoles.Staff)]
public class KntNotesSaveWindowAsyncCommand : KntCommandSaveServiceBase<WindowDto, Result<WindowDto>>
{
    private readonly bool forceNew;
    public KntNotesSaveWindowAsyncCommand(IKntService service, WindowDto entity, bool forceNew = false) : base(service, entity)
    {
        this.forceNew = forceNew;
    }

    public override async Task<Result<WindowDto>> Execute()
    {
        if (Param.WindowId == Guid.Empty)
        {
            Param.WindowId = Guid.NewGuid();
            return await Repository.Notes.AddWindowAsync(Param);
        }
        else
        {
            if (!forceNew)
            {
                return await Repository.Notes.UpdateWindowAsync(Param);
            }
            else
            {
                var checkExist = await Service.Notes.GetWindowAsync(Param.NoteId, Param.UserId);
                if (checkExist.IsValid)
                    return await Repository.Notes.UpdateWindowAsync(Param);
                else
                    return await Repository.Notes.AddWindowAsync(Param);
            }
        }
    }
}

[KntAuthorize(EnumRoles.Guest)]
public class KntNotesGetVisibleNotesIdAsyncCommand : KntCommandServiceBase<Result<List<Guid>>>
{
    private readonly string userName;
    
    public KntNotesGetVisibleNotesIdAsyncCommand(IKntService service, string userName) : base(service)
    {
        this.userName = userName;
    }

    public override async Task<Result<List<Guid>>> Execute()
    {
        var userId = Guid.Empty;

        var userDto = (await Repository.Users.GetByUserNameAsync(userName)).Entity;
        if (userDto != null)
            userId = userDto.UserId;

        return await Repository.Notes.GetVisibleNotesIdAsync(userId);
    }
}

[KntAuthorize(EnumRoles.Staff)]
public class KntNotesGetAlarmNotesIdAsyncCommand : KntCommandServiceBase<Result<List<Guid>>>
{
    private readonly string userName;
    private readonly EnumNotificationType? notificationType;

    public KntNotesGetAlarmNotesIdAsyncCommand(IKntService service, string userName, EnumNotificationType? notificationType = null) : base(service)
    {
        this.userName = userName;
        this.notificationType = notificationType;
    }

    public override async Task<Result<List<Guid>>> Execute()
    {
        var userId = Guid.Empty;

        var userDto = (await Repository.Users.GetByUserNameAsync(userName)).Entity;
        if (userDto != null)
            userId = userDto.UserId;

        return await Repository.Notes.GetAlarmNotesIdAsync(userId, notificationType);
    }
}

/// <summary>
/// The due alarms of a type of the user running the service, consumed: each one returned once, however many consumers
/// check at the same time (see IKntNoteRepository.ConsumeDueMessagesAsync). None for a user not registered here.
/// </summary>
[KntAuthorize(EnumRoles.Staff)]
public class KntNotesConsumeDueAlarmsAsyncCommand : KntCommandServiceBase<EnumNotificationType, Result<List<KMessageDto>>>
{
    public KntNotesConsumeDueAlarmsAsyncCommand(IKntService service, EnumNotificationType notificationType) : base(service, notificationType)
    {
    }

    public override async Task<Result<List<KMessageDto>>> Execute()
    {
        var user = await Service.GetCurrentUserAsync();
        if (user == null)
            return new Result<List<KMessageDto>>(new List<KMessageDto>());

        return await Repository.Notes.ConsumeDueMessagesAsync(user.UserId, Param);
    }
}

/// <summary>
/// The due alarms of a type of every user, consumed (as KntNotesConsumeDueAlarmsAsyncCommand): for an alarms service
/// that works for everybody, such as the Server's EmailAlarmsService (no user; the Server runs with authorization off).
/// </summary>
[KntAuthorize(EnumRoles.Admin)]
public class KntNotesConsumeAllUsersDueAlarmsAsyncCommand : KntCommandServiceBase<EnumNotificationType, Result<List<KMessageDto>>>
{
    public KntNotesConsumeAllUsersDueAlarmsAsyncCommand(IKntService service, EnumNotificationType notificationType) : base(service, notificationType)
    {
    }

    public override async Task<Result<List<KMessageDto>>> Execute()
    {
        return await Repository.Notes.ConsumeDueMessagesAsync(null, Param);
    }
}

/// <summary>
/// The Application info notices of the user running the service for the alarms given (the ones its alarms panel keeps):
/// only those that still are (KMessageDto.IsAppInfoNoticeFor the user, of a note that exists), with their note's number
/// and topic and their comment read now. The others are left out, so the panel drops them.
/// </summary>
[KntAuthorize(EnumRoles.Staff)]
public class KntNotesGetAlarmNoticesAsyncCommand : KntCommandServiceBase<List<Guid>, Result<List<AlarmNoticeDto>>>
{
    public KntNotesGetAlarmNoticesAsyncCommand(IKntService service, List<Guid> messageIds) : base(service, messageIds)
    {
    }

    public override async Task<Result<List<AlarmNoticeDto>>> Execute()
    {
        var result = new Result<List<AlarmNoticeDto>>(new List<AlarmNoticeDto>());

        var user = await Service.GetCurrentUserAsync();
        if (user == null)
            return result;

        foreach (var messageId in (Param ?? new List<Guid>()).Distinct())
        {
            var message = (await Repository.Notes.GetMessageAsync(messageId)).Entity;
            if (message?.NoteId == null || !message.IsAppInfoNoticeFor(user.UserId))
                continue;

            var note = (await Repository.Notes.GetAsync(message.NoteId.Value)).Entity;
            if (note == null)
                continue;

            result.Entity.Add(new AlarmNoticeDto
            {
                KMessageId = message.KMessageId,
                NoteId = note.NoteId,
                NoteNumber = note.NoteNumber,
                NoteTopic = note.Topic,
                Comment = message.Comment,
                AlarmDateTime = message.AlarmDateTime
            });
        }

        return result;
    }
}

/// <summary>
/// The alarms calendar of the web: the alarms of the filter (read-only, any user's, like the alarms of a note) and the
/// dates they fire on in its range. A periodic alarm fires from the creation of its note on (AlarmOccurrences); with
/// GroupFrequentByDay, an alarm of every few minutes is one occurrence per day. At most
/// KntConst.MaxAlarmsCalendarOccurrences occurrences, the first ones (Truncated says that there were more).
/// </summary>
[KntAuthorize(EnumRoles.Guest)]
public class KntNotesGetAlarmsCalendarAsyncCommand : KntCommandServiceBase<AlarmsCalendarFilterDto, Result<AlarmsCalendarDto>>
{
    public KntNotesGetAlarmsCalendarAsyncCommand(IKntService service, AlarmsCalendarFilterDto filter) : base(service, filter)
    {
    }

    public override async Task<Result<AlarmsCalendarDto>> Execute()
    {
        var result = new Result<AlarmsCalendarDto>(new AlarmsCalendarDto());
        if (Param == null || Param.End <= Param.Start)
        {
            result.AddErrorMessage("The range of dates of the calendar is not valid.");
            return result;
        }

        var resAlarms = await Repository.Notes.GetCalendarAlarmsAsync(Param);
        if (!resAlarms.IsValid)
        {
            result.AddListErrorMessage(resAlarms.ListErrorMessage);
            return result;
        }

        var occurrences = new List<AlarmOccurrenceDto>();
        foreach (var alarm in resAlarms.Entity)
        {
            var dates = AlarmOccurrences.Expand(alarm, alarm.NoteCreationDateTime, Param.Start, Param.End);
            if (Param.GroupFrequentByDay && alarm.AlarmType == EnumAlarmType.InMinutes)
                occurrences.AddRange(dates.GroupBy(d => d.Date)
                    .Select(day => new AlarmOccurrenceDto { KMessageId = alarm.KMessageId, DateTime = day.First(), Count = day.Count() }));
            else
                occurrences.AddRange(dates.Take(KntConst.MaxAlarmsCalendarOccurrences + 1)
                    .Select(d => new AlarmOccurrenceDto { KMessageId = alarm.KMessageId, DateTime = d }));
        }

        var calendar = result.Entity;
        calendar.Occurrences = occurrences.OrderBy(o => o.DateTime).ToList();
        if (calendar.Occurrences.Count > KntConst.MaxAlarmsCalendarOccurrences)
        {
            calendar.Occurrences = calendar.Occurrences.Take(KntConst.MaxAlarmsCalendarOccurrences).ToList();
            calendar.Truncated = true;
        }

        var shown = calendar.Occurrences.Select(o => o.KMessageId).ToHashSet();
        calendar.Alarms = resAlarms.Entity.Where(a => shown.Contains(a.KMessageId)).ToList();

        return result;
    }
}

[KntAuthorize(EnumRoles.Staff)]
public class KntNotesPatchFolderAsyncCommand : KntCommandServiceBase<Result<bool>>
{
    private readonly Guid noteId;
    private readonly Guid folderId;

    public KntNotesPatchFolderAsyncCommand(IKntService service, Guid noteId, Guid folderId) : base(service)
    {
        this.noteId = noteId;
        this.folderId = folderId;
    }

    public override async Task<Result<bool>> Execute()
    {
        return await Repository.Notes.PatchFolderAsync(noteId, folderId);
    }
}

[KntAuthorize(EnumRoles.Staff)]
public class KntNotesPatchChangeTagsAsyncCommand : KntCommandServiceBase<Result<bool>>
{
    private readonly Guid noteId;
    private readonly string oldTag;
    private readonly string newTag;

    public KntNotesPatchChangeTagsAsyncCommand(IKntService service, Guid noteId, string oldTag, string newTag) : base(service)
    {
        this.noteId = noteId;
        this.oldTag = oldTag;
        this.newTag = newTag;
    }

    public override async Task<Result<bool>> Execute()
    {
        return await Repository.Notes.PatchChangeTagsAsync(noteId, oldTag, newTag);
    }
}
using KNote.Model.Dto;
using KNote.Model;
using KNote.Service.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNote.Repository.EntityFramework.Entities;
using static Dapper.SqlMapper;
using System.Data;
using System.IO;

namespace KNote.Service.ServicesCommands;

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

public class KntNotesGetExtendedAsyncCommand : KntCommandServiceBase<Guid, Result<NoteExtendedDto>>
{
    public KntNotesGetExtendedAsyncCommand(IKntService service, Guid id) : base(service, id)
    {

    }

    public override async Task<Result<NoteExtendedDto>> Execute()
    {
        var result = new Result<NoteExtendedDto>();

        var entity = (await Service.Notes.GetAsync(Param)).Entity.GetSimpleDto<NoteExtendedDto>();
        entity.Resources = (await Service.Notes.GetResourcesAsync(Param)).Entity;
        entity.Tasks = (await Service.Notes.GetNoteTasksAsync(Param)).Entity;
        entity.Messages = (await Service.Notes.GetMessagesAsync(Param)).Entity;

        result.Entity = entity;
        return result;
    }
}

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

public class KntNotesGetFilterAsyncCommand : KntCommandServiceBase<NotesFilterDto, Result<List<NoteInfoDto>>>
{
    public KntNotesGetFilterAsyncCommand(IKntService service, NotesFilterDto notesFilter) : base(service, notesFilter)
    {

    }

    public override async Task<Result<List<NoteInfoDto>>> Execute()
    {
        return await Repository.Notes.GetFilter(Param);
    }
}

public class KntNotesGetSearchAsyncCommand : KntCommandServiceBase<NotesSearchDto, Result<List<NoteInfoDto>>>
{
    public KntNotesGetSearchAsyncCommand(IKntService service, NotesSearchDto notesSearch) : base(service, notesSearch)
    {

    }

    public override async Task<Result<List<NoteInfoDto>>> Execute()
    {
        return await Repository.Notes.GetSearch(Param);
    }
}

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

public class KntNotesSaveAsyncCommand : KntCommandServiceBase<NoteDto, Result<NoteDto>>
{
    private readonly bool UpdateStatus;
    public KntNotesSaveAsyncCommand(IKntService service, NoteDto entity, bool updateStatus = true) : base(service, entity)
    {
        UpdateStatus = updateStatus;
    }

    public override async Task<Result<NoteDto>> Execute()
    {
        if (Param.NoteId == Guid.Empty)
        {
            Param.NoteId = Guid.NewGuid();
            var res = await Repository.Notes.AddAsync(Param);
            return res;
        }
        else
        {
            if (UpdateStatus)
                Param.InternalTags = GetNoteStatus((await Service.Notes.GetNoteTasksAsync(Param.NoteId)).Entity, (await Service.Notes.GetMessagesAsync(Param.NoteId)).Entity);
            var res = await Repository.Notes.UpdateAsync(Param);
            return res;
        }
    }
}

public class KntNotesSaveExtendedAsyncCommand : KntCommandServiceBase<NoteExtendedDto, Result<NoteExtendedDto>>
{    
    public KntNotesSaveExtendedAsyncCommand(IKntService service, NoteExtendedDto entity) : base(service, entity)
    {        
    }

    public override async Task<Result<NoteExtendedDto>> Execute()
    {
        var result = new Result<NoteExtendedDto>();

        Param.InternalTags = GetNoteStatus(Param.Tasks, Param.Messages);

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

        return result;
    }
}

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

        var resNote = await Service.Notes.DeleteAsync(Param);
        if (!resNote.IsValid)
            result.AddListErrorMessage(resNote.ListErrorMessage);

        result.Entity = neForDelete;

        return result;
    }
}


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
                Service.Notes.ManageResourceContent(r);
        return res;
    }
}

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

public class KntNotesGetResourceAsyncCommand : KntCommandServiceBase<Guid, Result<ResourceDto>>
{
    public KntNotesGetResourceAsyncCommand(IKntService service, Guid id) : base(service, id)
    {

    }

    public override async Task<Result<ResourceDto>> Execute()
    {
        var res = await Repository.Notes.GetResourceAsync(Param);
        if (res.IsValid)
            Service.Notes.ManageResourceContent(res.Entity);
        return res;
    }
}

public class KntNotesSaveResourceAsyncCommand : KntCommandServiceBase<ResourceDto, Result<ResourceDto>>
{
    private readonly bool forceNew;
    public KntNotesSaveResourceAsyncCommand(IKntService service, ResourceDto resource, bool forceNew = false) : base(service, resource)
    {
        this.forceNew = forceNew;
    }

    public override async Task<Result<ResourceDto>> Execute()
    {
        Result<ResourceDto> result;

        Service.Notes.ManageResourceContent(Param);

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
                    var oldFile = Service.Notes.GetResourcePath(oldResource);
                    try
                    {
                        if (File.Exists(oldFile))
                            File.Delete(oldFile);
                    }
                    catch (Exception ex)
                    {
                        // TODO: anotate this meesage in log
                        var errMsg = ex.ToString();
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

        (result.Entity.RelativeUrl, result.Entity.FullUrl) = GetResourceUrls(result.Entity);

        return result;
    }
}

public class KntNotesSaveResourceInfoAsyncCommand : KntCommandServiceBase<ResourceInfoDto, Result<ResourceInfoDto>>
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

        var resource = Param.GetSimpleDto<ResourceDto>();

        var resSaveResource = await Service.Notes.SaveResourceAsync(resource, forceNew);

        res.AddListErrorMessage(resSaveResource.ListErrorMessage);
        res.Entity = resSaveResource.Entity.GetSimpleDto<ResourceInfoDto>();

        return res;
    }
}

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
                    var repRef = Repository.RespositoryRef;
                    var fullPathRec = Path.Combine(repRef.ResourcesContainerCacheRootPath, result.Entity.Container, result.Entity.Name);
                    if (File.Exists(fullPathRec))
                        File.Delete(fullPathRec);
                }
                catch (Exception ex)
                {
                    // TODO: anotate this meesage in log
                    var errMsg = ex.ToString();
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

public class KntNotesSaveNoteTaskAsyncCommand : KntCommandServiceBase<NoteTaskDto, Result<NoteTaskDto>>
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

public class KntNotesSaveMessageAsyncCommand : KntCommandServiceBase<KMessageDto, Result<KMessageDto>>
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

public class KntNotesSaveWindowAsyncCommand : KntCommandServiceBase<WindowDto, Result<WindowDto>>
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
        return await Repository.Notes.PatchFolder(noteId, folderId);
    }
}

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
        return await Repository.Notes.PatchChangeTags(noteId, oldTag, newTag);
    }
}
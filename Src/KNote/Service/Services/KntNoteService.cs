using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using System.Linq.Expressions;
using KNote.Repository;
using System.IO;
using KNote.Service.Interfaces;
using KNote.Service.Core;

namespace KNote.Service.Services
{
    public class KntNoteService : KntServiceBase, IKntNoteService
    {
        #region Fields

        // private readonly IKntRepository Repository;

        #endregion

        #region Constructor

        //protected internal KntNoteService(IKntRepository repository)
        //{
        //    Repository = repository;
        //}
        public KntNoteService(IKntService service) : base(service)
        {

        }


        #endregion

        #region IKntNoteService

        public async Task<Result<List<NoteInfoDto>>> GetAllAsync()
        {
            return await Repository.Notes.GetAllAsync();
        }
        
        public async Task<Result<List<NoteInfoDto>>> HomeNotesAsync()
        {
            return await Repository.Notes.HomeNotesAsync();
        }

        public async Task <Result<NoteDto>> GetAsync(Guid noteId)
        {
            return await Repository.Notes.GetAsync(noteId);
        }

        public async Task<Result<NoteDto>> GetAsync(int noteNumber)
        {
            return await Repository.Notes.GetAsync(noteNumber);
        }

        public async Task<Result<NoteExtendedDto>> GetExtendedAsync(Guid noteId)
        {
            var result = new Result<NoteExtendedDto>();            

            var entity = (await GetAsync(noteId)).Entity.GetSimpleDto<NoteExtendedDto>();
            entity.Resources = (await GetResourcesAsync(noteId)).Entity;
            entity.Tasks = (await GetNoteTasksAsync(noteId)).Entity;
            entity.Messages = (await GetMessagesAsync(noteId)).Entity;

            result.Entity = entity;
            return result;
        }

        public async Task<Result<List<NoteInfoDto>>> GetByFolderAsync(Guid folderId)
        {
            return await Repository.Notes.GetByFolderAsync(folderId);
        }

        public async Task<Result<List<NoteInfoDto>>> GetFilter(NotesFilterDto notesFilter)
        {
            return await Repository.Notes.GetFilter(notesFilter);
        }

        public async Task<Result<List<NoteInfoDto>>> GetSearch(NotesSearchDto notesSearch)
        {
            return await Repository.Notes.GetSearch(notesSearch);
        }

        public async Task<Result<NoteDto>> NewAsync(NoteInfoDto entityInfo = null)
        {
            return await Repository.Notes.NewAsync(entityInfo);
        }

        public async Task<Result<NoteExtendedDto>> NewExtendedAsync(NoteInfoDto entityInfo = null)
        {
            var result = new Result<NoteExtendedDto>();
            
            var entity = (await Repository.Notes.NewAsync(entityInfo)).Entity.GetSimpleDto<NoteExtendedDto>();
            
            result.Entity = entity;
            return result;
        }

        public async Task<Result<NoteDto>> SaveAsync(NoteDto entity, bool updateStatus = true)
        {
            if (entity.NoteId == Guid.Empty)
            {
                entity.NoteId = Guid.NewGuid();
                var res = await Repository.Notes.AddAsync(entity);                
                return res;
            }
            else
            {
                if (updateStatus)
                    entity.InternalTags = GetNoteStatus((await GetNoteTasksAsync(entity.NoteId)).Entity, (await GetMessagesAsync(entity.NoteId)).Entity);
                var res =  await Repository.Notes.UpdateAsync(entity);                
                return res;
            }
        }
        
        public async Task<Result<NoteExtendedDto>> SaveExtendedAsync(NoteExtendedDto entity)
        {            
            var result = new Result<NoteExtendedDto>();
            
            entity.InternalTags = GetNoteStatus(entity.Tasks, entity.Messages);

            if (entity.IsDirty())
            {
                var resNote = await SaveAsync(entity.GetSimpleDto<NoteDto>(), false);
                result.Entity = resNote.Entity.GetSimpleDto<NoteExtendedDto>();
            }
            else
                result.Entity = entity;

            var noteEdited = result.Entity;

            foreach (var item in entity.Messages)
            {
                if (item.IsDeleted())
                {
                    var res = await DeleteMessageAsync(item.KMessageId);
                    if (!res.IsValid)
                        result.AddListErrorMessage(res.ListErrorMessage);
                }
                else if (item.IsDirty())
                {
                    if (item.NoteId == Guid.Empty)
                        item.NoteId = noteEdited.NoteId;
                    var res = await SaveMessageAsync(item, true);
                    if (!res.IsValid)
                        result.AddListErrorMessage(res.ListErrorMessage);
                    else
                        result.Entity.Messages.Add(res.Entity);
                }
                else
                    result.Entity.Messages.Add(item);
            }
            entity.Messages.RemoveAll( m => m.IsDeleted());

            foreach (var item in entity.Resources)
            {
                if (item.IsDeleted())
                {
                    var res = await DeleteResourceAsync(item.ResourceId);
                    if (!res.IsValid)
                        result.AddListErrorMessage(res.ListErrorMessage);
                }
                else if (item.IsDirty())
                {
                    if (item.NoteId == Guid.Empty)
                        item.NoteId = noteEdited.NoteId;
                    var res = await SaveResourceAsync(item, true);
                    if (!res.IsValid)
                        result.AddListErrorMessage(res.ListErrorMessage);
                    else
                        result.Entity.Resources.Add(res.Entity);
                }
                else
                    result.Entity.Resources.Add(item);
            }
            entity.Resources.RemoveAll(r => r.IsDeleted());

            foreach (var item in entity.Tasks)
            {
                if (item.IsDeleted())
                {
                    var res = await DeleteNoteTaskAsync(item.NoteTaskId);
                    if (!res.IsValid)
                        result.AddListErrorMessage(res.ListErrorMessage);
                }
                else if (item.IsDirty())
                {
                    if (item.NoteId == Guid.Empty)
                        item.NoteId = noteEdited.NoteId;
                    var res = await SaveNoteTaskAsync(item, true);
                    if (!res.IsValid)
                        result.AddListErrorMessage(res.ListErrorMessage);
                    else
                        result.Entity.Tasks.Add(res.Entity);
                }
                else
                    result.Entity.Tasks.Add(item);

            }
            entity.Tasks.RemoveAll(t => t.IsDeleted());

            return result;
        }

        public async Task<Result<NoteDto>> DeleteAsync(Guid id)
        {            
            var result = new Result<NoteDto>();

            var resGetEntity = await GetAsync(id);

            if (resGetEntity.IsValid)
            {
                var resDelEntity = await Repository.Notes.DeleteAsync(id);
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

        public async Task<Result<NoteExtendedDto>> DeleteExtendedAsync(Guid id)
        {            
            var result = new Result<NoteExtendedDto>();
            
            var neForDelete = (await GetExtendedAsync(id)).Entity;

            foreach (var item in neForDelete.Messages)
            {
                var res = await DeleteMessageAsync(item.KMessageId);
                if (!res.IsValid)
                    result.AddListErrorMessage(res.ListErrorMessage);
            }
            foreach (var item in neForDelete.Resources)
            {
                var res = await DeleteResourceAsync(item.ResourceId);
                if (!res.IsValid)
                    result.AddListErrorMessage(res.ListErrorMessage);
            }
            foreach (var item in neForDelete.Tasks)
            {
                var res = await DeleteNoteTaskAsync(item.NoteTaskId);
                if (!res.IsValid)
                    result.AddListErrorMessage(res.ListErrorMessage);
            }

            var resNote = await DeleteAsync(id);
            if (!resNote.IsValid)
                result.AddListErrorMessage(resNote.ListErrorMessage);

            result.Entity = neForDelete;

            return result;
        }

        public async Task<List<NoteKAttributeDto>> CompleteNoteAttributes(List<NoteKAttributeDto> attributesNotes, Guid noteId, Guid? noteTypeId = null)
        {
            return await Repository.Notes.CompleteNoteAttributes(attributesNotes, noteId, noteTypeId);
        }

        public async Task<Result<List<ResourceDto>>> GetResourcesAsync(Guid noteId)
        {
            var res = await Repository.Notes.GetResourcesAsync(noteId);         
            if(res.IsValid)
                foreach(var r in res.Entity)                    
                    ManageResourceContent(r);
            return res;
        }

        public async Task<Result<List<ResourceInfoDto>>> GetResourcesInfoAsync(Guid noteId)
        {
            var res = new Result<List<ResourceInfoDto>>();

            var resGetResources = await GetResourcesAsync(noteId);
                        
            res.AddListErrorMessage(resGetResources.ListErrorMessage);
            res.Entity = new List<ResourceInfoDto>();
            foreach (var r in resGetResources.Entity)
                res.Entity.Add(r.GetSimpleDto<ResourceInfoDto>());

            return res;
        }

        public async Task<Result<ResourceDto>> GetResourceAsync(Guid resourceId)
        {            
            var res = await Repository.Notes.GetResourceAsync(resourceId);
            if(res.IsValid)
                ManageResourceContent(res.Entity);
            return res;
        }

        public async Task<Result<ResourceDto>> SaveResourceAsync(ResourceDto resource, bool forceNew = false)
        {
            Result<ResourceDto> result;

            ManageResourceContent(resource);

            var tmpContent = resource.ContentArrayBytes;

            if (!resource.ContentInDB)                            
                resource.ContentArrayBytes = null;
                                   
            if (resource.ResourceId == Guid.Empty)
            {
                resource.ResourceId = Guid.NewGuid();
                result = await Repository.Notes.AddResourceAsync(resource);
            }
            else
            {
                var checkExist = await GetResourceAsync(resource.ResourceId);

                // Delete old resource file
                if (checkExist.IsValid)
                {
                    var oldResource = checkExist.Entity;

                    if (oldResource.Name != resource.Name && oldResource.ContentInDB == false)
                    {
                        var oldFile = GetResourcePath(oldResource);
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
                    result = await Repository.Notes.UpdateResourceAsync(resource);
                }
                else
                {
                    
                    if(checkExist.IsValid)
                        result = await Repository.Notes.UpdateResourceAsync(resource);
                    else
                        result = await Repository.Notes.AddResourceAsync(resource);
                }
            }

            if (!result.Entity.ContentInDB)
                result.Entity.ContentArrayBytes = tmpContent;

            (result.Entity.RelativeUrl, result.Entity.FullUrl) = GetResourceUrls(result.Entity);

            return result;
        }

        public async Task<Result<ResourceInfoDto>> SaveResourceAsync(ResourceInfoDto resourceInfo, bool forceNew = false)
        {
            if (resourceInfo == null)
                throw new ArgumentException("Resource can't be null");

            var res = new Result<ResourceInfoDto>();

            var resource = resourceInfo.GetSimpleDto<ResourceDto>();

            var resSaveResource = await SaveResourceAsync(resource, forceNew);
            
            res.AddListErrorMessage(resSaveResource.ListErrorMessage);
            res.Entity = resSaveResource.Entity.GetSimpleDto<ResourceInfoDto>();

            return res;
        }

        public bool ManageResourceContent(ResourceDto resource, bool forceUpdateDto = true)
        {
            if (resource == null)
                return false;

            string rootCacheResource = Repository.RespositoryRef.ResourcesContainerCacheRootPath;
            if (string.IsNullOrEmpty(resource.Container))
            {
                if (forceUpdateDto)
                {
                    resource.Container = Repository.RespositoryRef.ResourcesContainer + @"\" + DateTime.Now.Year.ToString();
                    resource.ContentInDB = Repository.RespositoryRef.ResourceContentInDB;
                }
            }
                        
            if (rootCacheResource == null || resource.Container == null || resource.Name == null)
                return false;

            try
            {                
                string dirPath = Path.Combine(new string[] { rootCacheResource, resource.Container });                
                string file = GetResourcePath(resource);

                if(resource.ContentArrayBytes != null)
                {
                    if (!Directory.Exists(dirPath))
                        Directory.CreateDirectory(dirPath);
                    if (!File.Exists(file))
                        File.WriteAllBytes(file, resource.ContentArrayBytes);
                }

                if (forceUpdateDto)
                {
                    if (!resource.ContentInDB)
                        resource.ContentArrayBytes = File.ReadAllBytes(file);

                    (resource.RelativeUrl, resource.FullUrl) = GetResourceUrls(resource);
                }
            }
            catch (Exception ex)
            {
                // TODO: anotate this meesage in log
                var errMsg = ex.ToString();
                return false;
            }
                                               
            return true;
        }

        public string GetResourcePath(ResourceDto resource)
        {
            string rootPath = Repository.RespositoryRef.ResourcesContainerCacheRootPath;
            string relativePath;
            string fullPath;

            if (string.IsNullOrEmpty(rootPath) || string.IsNullOrEmpty(resource.Container) || string.IsNullOrEmpty(resource.Name))
                return null;

            relativePath = Path.Combine(resource.Container, resource.Name);
            fullPath = Path.Combine(rootPath, relativePath);

            return fullPath;
        }

        public async Task<Result<ResourceDto>> DeleteResourceAsync(Guid id)
        {            
            var result = new Result<ResourceDto>();

            var resGetEntity = await Repository.Notes.GetResourceAsync(id);
            if (resGetEntity.IsValid)
            {
                var resDelEntity = await Repository.Notes.DeleteResourceAsync(id);
                if (resDelEntity.IsValid)
                {
                    result.Entity = resGetEntity.Entity;
                    try
                    {
                        var repRef = Repository.RespositoryRef;
                        var fullPathRec  = Path.Combine(repRef.ResourcesContainerCacheRootPath, result.Entity.Container, result.Entity.Name);
                        if(File.Exists(fullPathRec))
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

        public async Task<Result<ResourceInfoDto>> DeleteResourceInfoAsync(Guid id)
        {
            var res = new Result<ResourceInfoDto>();

            var resDelete = await DeleteResourceAsync(id);            
            res.AddListErrorMessage(resDelete.ListErrorMessage);
            res.Entity = resDelete.Entity.GetSimpleDto<ResourceInfoDto>();
            return res;
        }

        public async Task<Result<List<NoteTaskDto>>> GetNoteTasksAsync(Guid idNote)
        {
            return await Repository.Notes.GetNoteTasksAsync(idNote);
        }

        public async Task<Result<List<NoteTaskDto>>> GetStartedTasksByDateTimeRageAsync(DateTime startDateTime, DateTime endDateTime)
        {
            return await Repository.Notes.GetStartedTasksByDateTimeRageAsync(startDateTime, endDateTime);
        }

        public async Task<Result<List<NoteTaskDto>>> GetEstimatedTasksByDateTimeRageAsync(DateTime startDateTime, DateTime endDateTime)
        {
            return await Repository.Notes.GetEstimatedTasksByDateTimeRageAsync(startDateTime, endDateTime);
        }


        public async Task<Result<NoteTaskDto>> GetNoteTaskAsync(Guid noteTaskId)
        {            
            return await Repository.Notes.GetNoteTaskAsync(noteTaskId);
        }

        public async Task<Result<NoteTaskDto>> SaveNoteTaskAsync(NoteTaskDto entity, bool forceNew = false)
        {            
            if (entity.NoteTaskId == Guid.Empty)
            {
                entity.NoteTaskId = Guid.NewGuid();
                return await Repository.Notes.AddNoteTaskAsync(entity);
            }
            else
            {
                if (!forceNew)
                {
                    return await Repository.Notes.UpdateNoteTaskAsync(entity);
                }
                else
                {
                    var checkExist = await GetNoteTaskAsync(entity.NoteTaskId);
                    if (checkExist.IsValid)
                        return await Repository.Notes.UpdateNoteTaskAsync(entity);
                    else
                        return await Repository.Notes.AddNoteTaskAsync(entity);
                }
            }
        }

        public async Task<Result<NoteTaskDto>> DeleteNoteTaskAsync(Guid id)
        {         
            var result = new Result<NoteTaskDto>();

            var resGetEntity = await Repository.Notes.GetNoteTaskAsync(id);
            if (resGetEntity.IsValid)
            {
                var resDelEntity = await Repository.Notes.DeleteNoteTaskAsync(id);
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

        public async Task<Result<List<KMessageDto>>> GetMessagesAsync(Guid noteId)
        {
            return await Repository.Notes.GetMessagesAsync(noteId);
        }

        public async Task<Result<KMessageDto>> GetMessageAsync(Guid messageId)
        {
            return await Repository.Notes.GetMessageAsync(messageId);
        }

        public async Task<Result<KMessageDto>> SaveMessageAsync(KMessageDto entity, bool forceNew = false)
        {
            Result<KMessageDto> resSavedEntity;

            if (entity.KMessageId == Guid.Empty)
            {
                entity.KMessageId = Guid.NewGuid();                
                resSavedEntity = await Repository.Notes.AddMessageAsync(entity);
            }
            else
            {
                if (!forceNew)
                {                    
                    resSavedEntity = await Repository.Notes.UpdateMessageAsync(entity);
                }
                else
                {
                    var checkExist = await GetMessageAsync(entity.KMessageId);
                    if (checkExist.IsValid)                        
                        resSavedEntity = await Repository.Notes.UpdateMessageAsync(entity);
                    else                        
                        resSavedEntity = await Repository.Notes.AddMessageAsync(entity);
                }
            }

            resSavedEntity.Entity.UserFullName = entity.UserFullName;
            return resSavedEntity;
        }

        public async Task<Result<KMessageDto>> DeleteMessageAsync(Guid messageId)
        {
            var result = new Result<KMessageDto>();

            var resGetEntity = await GetMessageAsync(messageId);

            if (resGetEntity.IsValid)
            {
                var resDelEntity = await Repository.Notes.DeleteMessageAsync(messageId);
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

        public async Task<Result<WindowDto>> GetWindowAsync(Guid noteId, Guid userId)
        {
            return await Repository.Notes.GetWindowAsync(noteId, userId);
        }

        public async Task<Result<WindowDto>> SaveWindowAsync(WindowDto entity, bool forceNew = false)
        {
            if (entity.WindowId == Guid.Empty)
            {
                entity.WindowId = Guid.NewGuid();
                return await Repository.Notes.AddWindowAsync(entity);
            }
            else
            {
                if (!forceNew)
                {
                    return await Repository.Notes.UpdateWindowAsync(entity);
                }
                else
                {
                    var checkExist = await GetWindowAsync(entity.NoteId, entity.UserId);
                    if (checkExist.IsValid)
                        return await Repository.Notes.UpdateWindowAsync(entity);
                    else
                        return await Repository.Notes.AddWindowAsync(entity);
                }
            }
        }

        public async Task<Result<List<Guid>>> GetVisibleNotesIdAsync(string userName)
        {
            var userId = Guid.Empty;

            var userDto = (await Repository.Users.GetByUserNameAsync(userName)).Entity;
            if (userDto != null)
                userId = userDto.UserId;

            return await Repository.Notes.GetVisibleNotesIdAsync(userId);
        }

        public async Task<Result<List<Guid>>> GetAlarmNotesIdAsync(string userName, EnumNotificationType? notificationType = null)
        {
            var userId = Guid.Empty;

            var userDto = (await Repository.Users.GetByUserNameAsync(userName)).Entity;
            if (userDto != null)
                userId = userDto.UserId;

            return await Repository.Notes.GetAlarmNotesIdAsync(userId, notificationType);
        }

        public async Task<Result<bool>> PatchFolder(Guid noteId, Guid folderId)
        {
            return await Repository.Notes.PatchFolder(noteId, folderId);
        }

        public async Task<Result<bool>> PatchChangeTags(Guid noteId, string oldTag, string newTag)
        {                        
            return await Repository.Notes.PatchChangeTags(noteId, oldTag, newTag);
        }

        #endregion

        #region Private methods

        private string GetNoteStatus(List<NoteTaskDto> tasks, List<KMessageDto> messages)
        {
            string status = "";

            var tasksValid = tasks.Where(t => t.IsDeleted() == false).Select(t => t).ToList();
            var messagesValid = messages.Where(m => m.IsDeleted() == false).Select(m => m).ToList();

            bool allTaskResolved = true;
            if (tasksValid?.Count > 0)
            {
                foreach (var item in tasks)
                {
                    if (item.Resolved == false)
                    {
                        allTaskResolved = false;
                        break;
                    }
                }
            }
            else
            {
                allTaskResolved = false;
            }

            bool alarmsPending = false;
            foreach (var item in messagesValid)
            {
                if (item.AlarmActivated == true)
                {
                    alarmsPending = true;
                    break;
                }

            }

            if (allTaskResolved == true)
                status = KntConst.Status[EnumStatus.Resolved];

            if (alarmsPending == true)
            {
                if (!string.IsNullOrEmpty(status))
                    status += "; ";
                status += KntConst.Status[EnumStatus.AlarmsPending];
            }

            return status;
        }

        private (string, string) GetResourceUrls(ResourceDto resource)
        {
            string rootUrl = Repository.RespositoryRef.ResourcesContainerCacheRootUrl;
            string relativeUrl;
            string fullUrl;

            if (string.IsNullOrEmpty(resource.Container))
            {
                resource.Container = Repository.RespositoryRef.ResourcesContainer + @"\" + DateTime.Now.Year.ToString();
                resource.ContentInDB = Repository.RespositoryRef.ResourceContentInDB;
            }

            if (string.IsNullOrEmpty(rootUrl) || string.IsNullOrEmpty(resource.Container) || string.IsNullOrEmpty(resource.Name))
                return (null, null);

            relativeUrl = (Path.Combine(resource.Container, resource.Name)).Replace(@"\", @"/");
            fullUrl = (Path.Combine(rootUrl, relativeUrl)).Replace(@"\", @"/");

            return (relativeUrl, fullUrl);
        }

        #endregion
    }
}

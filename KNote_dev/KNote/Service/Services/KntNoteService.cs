using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using System.Linq.Expressions;
using KNote.Service;
using KNote.Repository;

namespace KNote.Service.Services
{
    public class KntNoteService : DomainActionBase, IKntNoteService
    {
        #region Fields

        private readonly IKntRepository _repository;

        #endregion

        #region Constructor

        protected internal KntNoteService(IKntRepository repository)
        {
            _repository = repository;
        }

        #endregion

        #region IKntNoteService

        public async Task<Result<List<NoteInfoDto>>> GetAllAsync()
        {
            return await _repository.Notes.GetAllAsync();
        }
        
        public async Task<Result<List<NoteInfoDto>>> HomeNotesAsync()
        {
            return await _repository.Notes.HomeNotesAsync();
        }

        public async Task <Result<NoteDto>> GetAsync(Guid noteId)
        {
            return await _repository.Notes.GetAsync(noteId);
        }

        public async Task<Result<NoteExtendedDto>> GetExtendedAsync(Guid noteId)
        {
            var result = new Result<NoteExtendedDto>();            

            var entity = (await _repository.Notes.GetAsync(noteId)).Entity.GetSimpleDto<NoteExtendedDto>();
            entity.Resources = (await _repository.Notes.GetResourcesAsync(noteId)).Entity;
            entity.Tasks = (await _repository.Notes.GetNoteTasksAsync(noteId)).Entity;
            entity.Messages = (await _repository.Notes.GetMessagesAsync(noteId)).Entity;

            result.Entity = entity;
            return result;
        }

        public async Task<Result<List<NoteInfoDto>>> GetByFolderAsync(Guid folderId)
        {
            return await _repository.Notes.GetByFolderAsync(folderId);
        }

        public async Task<Result<List<NoteInfoDto>>> GetFilter(NotesFilterDto notesFilter)
        {
            return await _repository.Notes.GetFilter(notesFilter);
        }

        public async Task<Result<List<NoteInfoDto>>> GetSearch(NotesSearchDto notesSearch)
        {
            return await _repository.Notes.GetSearch(notesSearch);
        }

        public async Task<Result<NoteDto>> NewAsync(NoteInfoDto entityInfo = null)
        {
            return await _repository.Notes.NewAsync(entityInfo);
        }

        public async Task<Result<NoteExtendedDto>> NewExtendedAsync(NoteInfoDto entityInfo = null)
        {
            var result = new Result<NoteExtendedDto>();
            
            var entity = (await _repository.Notes.NewAsync(entityInfo)).Entity.GetSimpleDto<NoteExtendedDto>();
            
            result.Entity = entity;
            return result;
        }

        public async Task<Result<NoteDto>> SaveAsync(NoteDto entity)
        {
            if (entity.NoteId == Guid.Empty)
            {
                entity.NoteId = Guid.NewGuid();
                var res = await _repository.Notes.AddAsync(entity);                
                return res;
            }
            else
            {
                var res =  await _repository.Notes.UpdateAsync(entity);                
                return res;
            }
        }

        public async Task<Result<NoteExtendedDto>> SaveExtendedAsync(NoteExtendedDto entity)
        {            
            var result = new Result<NoteExtendedDto>();
            
            if (entity.IsDirty())
            {
                var resNote = await SaveAsync(entity.GetSimpleDto<NoteDto>());
                result.Entity = resNote.Entity.GetSimpleDto<NoteExtendedDto>();
            }
            else
                result.Entity = entity;

            var noteEdited = result.Entity;

            foreach (var item in entity.Messages)
            {
                if (item.IsDirty())
                {
                    if (item.NoteId == Guid.Empty)
                        item.NoteId = noteEdited.NoteId;
                    var res = await SaveMessageAsync(item, true);
                    if (!res.IsValid)
                        CopyErrorList(res.ErrorList, result.ErrorList);
                    else
                        result.Entity.Messages.Add(res.Entity);
                }
                else
                    result.Entity.Messages.Add(item);
            }
            foreach (var item in entity.MessagesDeleted)
            {
                var res = await DeleteMessageAsync(item);
                if (!res.IsValid)
                    CopyErrorList(res.ErrorList, result.ErrorList);
            }
            entity.MessagesDeleted.Clear();

            foreach (var item in entity.Resources)
            {
                if (item.IsDirty())
                {
                    if (item.NoteId == Guid.Empty)
                        item.NoteId = noteEdited.NoteId;
                    var res = await SaveResourceAsync(item, true);
                    if (!res.IsValid)
                        CopyErrorList(res.ErrorList, result.ErrorList);
                    else 
                        result.Entity.Resources.Add(res.Entity);
                }
                else
                
                    result.Entity.Resources.Add(item);                
            }
            foreach (var item in entity.ResourcesDeleted)
            {
                var res = await DeleteResourceAsync(item);
                if (!res.IsValid)
                    CopyErrorList(res.ErrorList, result.ErrorList);
            }
            entity.Resources.Clear();

            foreach (var item in entity.Tasks)
            {
                if (item.IsDirty())
                {
                    if (item.NoteId == Guid.Empty)
                        item.NoteId = noteEdited.NoteId;
                    var res = await SaveNoteTaskAsync(item, true);
                    if (!res.IsValid)
                        CopyErrorList(res.ErrorList, result.ErrorList);
                    else 
                        result.Entity.Tasks.Add(res.Entity);
                }
                else
                    result.Entity.Tasks.Add(item);

            }
            foreach (var item in entity.TasksDeleted)
            {
                var res = await DeleteNoteTaskAsync(item);
                if (!res.IsValid)
                    CopyErrorList(res.ErrorList, result.ErrorList);
            }
            entity.TasksDeleted.Clear();

            return result;
        }

        public async Task<Result<NoteDto>> DeleteAsync(Guid id)
        {            
            var result = new Result<NoteDto>();

            var resGetEntity = await GetAsync(id);

            if (resGetEntity.IsValid)
            {
                var resDelEntity = await _repository.Notes.DeleteAsync(id);
                if (resDelEntity.IsValid)
                    result.Entity = resGetEntity.Entity;
                else
                    result.ErrorList = resDelEntity.ErrorList;
            }
            else
            {
                result.ErrorList = resGetEntity.ErrorList;
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
                    CopyErrorList(res.ErrorList, result.ErrorList);
            }
            foreach (var item in neForDelete.Resources)
            {
                var res = await DeleteResourceAsync(item.ResourceId);
                if (!res.IsValid)
                    CopyErrorList(res.ErrorList, result.ErrorList);
            }
            foreach (var item in neForDelete.Tasks)
            {
                var res = await DeleteNoteTaskAsync(item.NoteTaskId);
                if (!res.IsValid)
                    CopyErrorList(res.ErrorList, result.ErrorList);
            }

            var resNote = await DeleteAsync(id);
            if (!resNote.IsValid)
                CopyErrorList(resNote.ErrorList, result.ErrorList);

            result.Entity = neForDelete;

            return result;
        }

        public async Task<List<NoteKAttributeDto>> CompleteNoteAttributes(List<NoteKAttributeDto> attributesNotes, Guid noteId, Guid? noteTypeId = null)
        {
            return await _repository.Notes.CompleteNoteAttributes(attributesNotes, noteId, noteTypeId);
        }

        public async Task<Result<List<ResourceDto>>> GetResourcesAsync(Guid idNote)
        {
            return await _repository.Notes.GetResourcesAsync(idNote);
        }

        public async Task<Result<ResourceDto>> GetResourceAsync(Guid resourceId)
        {
            return await _repository.Notes.GetResourceAsync(resourceId);
        }

        public async Task<Result<ResourceDto>> SaveResourceAsync(ResourceDto entity, bool forceNew = false)
        {            
            if (entity.ResourceId == Guid.Empty)
            {
                entity.ResourceId = Guid.NewGuid();
                return await _repository.Notes.AddResourceAsync(entity);
            }
            else
            {
                if (!forceNew)
                {
                    return await _repository.Notes.UpdateResourceAsync(entity);
                }
                else
                {
                    var checkExist = await GetResourceAsync(entity.ResourceId);
                    if(checkExist.IsValid)
                        return await _repository.Notes.UpdateResourceAsync(entity);
                    else
                        return await _repository.Notes.AddResourceAsync(entity);
                }
            }
        }

        public async Task<Result<ResourceDto>> DeleteResourceAsync(Guid id)
        {            
            var result = new Result<ResourceDto>();

            var resGetEntity = await _repository.Notes.GetResourceAsync(id);
            if (resGetEntity.IsValid)
            {
                var resDelEntity = await _repository.Notes.DeleteResourceAsync(id);
                if (resDelEntity.IsValid)
                    result.Entity = resGetEntity.Entity;
                else
                    result.ErrorList = resDelEntity.ErrorList;
            }
            else
            {
                result.ErrorList = resGetEntity.ErrorList;
            }

            return result;
        }

        public async Task<Result<List<NoteTaskDto>>> GetNoteTasksAsync(Guid idNote)
        {
            return await _repository.Notes.GetNoteTasksAsync(idNote);
        }

        public async Task<Result<NoteTaskDto>> GetNoteTaskAsync(Guid noteTaskId)
        {            
            return await _repository.Notes.GetNoteTaskAsync(noteTaskId);
        }

        public async Task<Result<NoteTaskDto>> SaveNoteTaskAsync(NoteTaskDto entity, bool forceNew = false)
        {            
            if (entity.NoteTaskId == Guid.Empty)
            {
                entity.NoteTaskId = Guid.NewGuid();
                return await _repository.Notes.AddNoteTaskAsync(entity);
            }
            else
            {
                if (!forceNew)
                {
                    return await _repository.Notes.UpdateNoteTaskAsync(entity);
                }
                else
                {
                    var checkExist = await GetNoteTaskAsync(entity.NoteTaskId);
                    if (checkExist.IsValid)
                        return await _repository.Notes.UpdateNoteTaskAsync(entity);
                    else
                        return await _repository.Notes.AddNoteTaskAsync(entity);
                }
            }
        }

        public async Task<Result<NoteTaskDto>> DeleteNoteTaskAsync(Guid id)
        {         
            var result = new Result<NoteTaskDto>();

            var resGetEntity = await _repository.Notes.GetNoteTaskAsync(id);
            if (resGetEntity.IsValid)
            {
                var resDelEntity = await _repository.Notes.DeleteNoteTaskAsync(id);
                if (resDelEntity.IsValid)
                    result.Entity = resGetEntity.Entity;
                else
                    result.ErrorList = resDelEntity.ErrorList;
            }
            else
            {
                result.ErrorList = resGetEntity.ErrorList;
            }

            return result;
        }

        public async Task<Result<List<KMessageDto>>> GetMessagesAsync(Guid noteId)
        {
            return await _repository.Notes.GetMessagesAsync(noteId);
        }

        public async Task<Result<KMessageDto>> GetMessageAsync(Guid messageId)
        {
            return await _repository.Notes.GetMessageAsync(messageId);
        }

        public async Task<Result<KMessageDto>> SaveMessageAsync(KMessageDto entity, bool forceNew = false)
        {
            if (entity.KMessageId == Guid.Empty)
            {
                entity.KMessageId = Guid.NewGuid();
                return await _repository.Notes.AddMessageAsync(entity);
            }
            else
            {
                if (!forceNew)
                {
                    return await _repository.Notes.UpdateMessageAsync(entity);
                }
                else
                {
                    var checkExist = await GetMessageAsync(entity.KMessageId);
                    if (checkExist.IsValid)
                        return await _repository.Notes.UpdateMessageAsync(entity);
                    else
                        return await _repository.Notes.AddMessageAsync(entity);
                }
            }
        }

        public async Task<Result<KMessageDto>> DeleteMessageAsync(Guid messageId)
        {
            var result = new Result<KMessageDto>();

            var resGetEntity = await GetMessageAsync(messageId);

            if (resGetEntity.IsValid)
            {
                var resDelEntity = await _repository.Notes.DeleteMessageAsync(messageId);
                if (resDelEntity.IsValid)
                    result.Entity = resGetEntity.Entity;
                else
                    result.ErrorList = resDelEntity.ErrorList;
            }
            else
            {
                result.ErrorList = resGetEntity.ErrorList;
            }

            return result;
        }

        public async Task<Result<WindowDto>> GetWindowAsync(Guid noteId, Guid userId)
        {
            return await _repository.Notes.GetWindowAsync(noteId, userId);
        }

        public async Task<Result<WindowDto>> SaveWindowAsync(WindowDto entity, bool forceNew = false)
        {
            if (entity.WindowId == Guid.Empty)
            {
                entity.WindowId = Guid.NewGuid();
                return await _repository.Notes.AddWindowAsync(entity);
            }
            else
            {
                if (!forceNew)
                {
                    return await _repository.Notes.UpdateWindowAsync(entity);
                }
                else
                {
                    var checkExist = await GetWindowAsync(entity.NoteId, entity.UserId);
                    if (checkExist.IsValid)
                        return await _repository.Notes.UpdateWindowAsync(entity);
                    else
                        return await _repository.Notes.AddWindowAsync(entity);
                }
            }
        }

        public async Task<Result<List<Guid>>> GetVisibleNotesIdAsync(string userName)
        {
            var userId = Guid.Empty;

            var userDto = (await _repository.Users.GetByUserNameAsync(userName)).Entity;
            if (userDto != null)
                userId = userDto.UserId;

            return await _repository.Notes.GetVisibleNotesIdAsync(userId);
        }

        public async Task<Result<List<Guid>>> GetAlarmNotesIdAsync(string userName, EnumNotificationType? notificationType = null)
        {
            var userId = Guid.Empty;

            var userDto = (await _repository.Users.GetByUserNameAsync(userName)).Entity;
            if (userDto != null)
                userId = userDto.UserId;

            return await _repository.Notes.GetAlarmNotesIdAsync(userId, notificationType);
        }

        public async Task<Result<bool>> PatchFolder(Guid noteId, Guid folderId)
        {
            return await _repository.Notes.PatchFolder(noteId, folderId);
        }

        #endregion

    }
}

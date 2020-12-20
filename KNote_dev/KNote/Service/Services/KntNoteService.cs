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
            var entity = new NoteExtendedDto();

            entity.Note = (await _repository.Notes.GetAsync(noteId)).Entity;
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
            var entity = new NoteExtendedDto();

            entity.Note = (await _repository.Notes.NewAsync(entityInfo)).Entity;

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
            result.Entity = new NoteExtendedDto();

            if (entity.Note.IsDirty())
            {
                var resNote = await SaveAsync(entity.Note);
                result.Entity.Note = resNote.Entity;
            }
            else
                result.Entity.Note = entity.Note;

            var noteEdited = result.Entity.Note;

            foreach (var item in entity.Messages)
            {
                if (item.IsDirty())
                {
                    if (item.NoteId == Guid.Empty)
                        item.NoteId = noteEdited.NoteId;
                    var res = await SaveMessageAsync(item);
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
                    var res = await SaveResourceAsync(item);
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
                    var res = await SaveNoteTaskAsync(item);
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
            result.Entity = new NoteExtendedDto();

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

        public async Task<Result<List<ResourceDto>>> GetResourcesAsync(Guid idNote)
        {
            return await _repository.Notes.GetResourcesAsync(idNote);
        }

        public async Task<Result<ResourceDto>> SaveResourceAsync(ResourceDto entity)
        {            
            if (entity.ResourceId == Guid.Empty)
            {
                entity.ResourceId = Guid.NewGuid();
                return await _repository.Notes.AddResourceAsync(entity);
            }
            else
            {
                return await _repository.Notes.UpdateResourceAsync(entity);
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

        public async Task<Result<NoteTaskDto>> SaveNoteTaskAsync(NoteTaskDto entity)
        {            
            if (entity.NoteTaskId == Guid.Empty)
            {
                entity.NoteTaskId = Guid.NewGuid();
                return await _repository.Notes.AddNoteTaskAsync(entity);
            }
            else
            {
                return await _repository.Notes.UpdateNoteTaskAsync(entity);
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

        public async Task<Result<KMessageDto>> SaveMessageAsync(KMessageDto entity)
        {
            if (entity.KMessageId == Guid.Empty)
            {
                entity.KMessageId = Guid.NewGuid();
                return await _repository.Notes.AddMessageAsync(entity);
            }
            else
            {
                return await _repository.Notes.UpdateMessageAsync(entity);
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

        #endregion

    }
}

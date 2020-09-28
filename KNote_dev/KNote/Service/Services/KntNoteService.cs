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

        public async Task<Result<NoteDto>> SaveAsync(NoteDto entity)
        {
            if (entity.NoteId == Guid.Empty)
            {
                entity.NoteId = Guid.NewGuid();
                return await _repository.Notes.AddAsync(entity);
            }
            else
            {
                return await _repository.Notes.UpdateAsync(entity);
            }
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

        public async Task<Result<List<ResourceDto>>> GetNoteResourcesAsync(Guid idNote)
        {
            return await _repository.Notes.GetNoteResourcesAsync(idNote);
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

            //var resGetEntity = await GetResourceAsync(id);
            //if (resGetEntity.IsValid)
            //{
            //    var resDelEntity = await _repository.Notes.DeleteResourceAsync(id);
            //    if (resDelEntity.IsValid)
            //        result.Entity = resGetEntity.Entity;
            //    else
            //        result.ErrorList = resDelEntity.ErrorList;
            //}
            //else
            //{
            //    result.ErrorList = resGetEntity.ErrorList;
            //}

            // TODO: Implementación provisional al bloque anterior
            var resDelEntity = await _repository.Notes.DeleteResourceAsync(id);
            if (resDelEntity.IsValid)
                result.Entity = null;
            else
                result.ErrorList = resDelEntity.ErrorList;

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

            //var resGetEntity = await GetNoteTaskAsync(id);
            //if (resGetEntity.IsValid)
            //{
            //    var resDelEntity = await _repository.Notes.DeleteNoteTaskAsync(id);
            //    if (resDelEntity.IsValid)
            //        result.Entity = resGetEntity.Entity;
            //    else
            //        result.ErrorList = resDelEntity.ErrorList;
            //}
            //else
            //{
            //    result.ErrorList = resGetEntity.ErrorList;
            //}
            
            // TODO: Implementación provisional al bloque anterior
            var resDelEntity = await _repository.Notes.DeleteNoteTaskAsync(id);
            if (resDelEntity.IsValid)
                result.Entity = null;
            else
                result.ErrorList = resDelEntity.ErrorList;

            return result;
        }


        #endregion

    }
}

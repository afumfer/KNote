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
            return await _repository.Notes.SaveAsync(entity);
        }

        public async Task<Result<NoteInfoDto>> DeleteAsync(Guid id)
        {
            return await _repository.Notes.DeleteAsync(id);
        }

        public async Task<Result<NoteKAttributeDto>> SaveAttrtibuteAsync(NoteKAttributeDto entity)
        {
            return await _repository.Notes.SaveAttrtibuteAsync(entity);
        }

        public async Task<Result<ResourceDto>> SaveResourceAsync(ResourceDto entity)
        {
            return await _repository.Notes.SaveResourceAsync(entity);
        }

        public async Task<Result<List<ResourceDto>>> GetNoteResourcesAsync(Guid idNote)
        {
            return await _repository.Notes.GetNoteResourcesAsync(idNote);
        }

        public async Task<Result<ResourceDto>> DeleteResourceAsync(Guid id)
        {
            return await _repository.Notes.DeleteResourceAsync(id);
        }
        
        public async Task<Result<NoteTaskDto>> SaveNoteTaskAsync(NoteTaskDto entityInfo)
        {
            return await _repository.Notes.SaveNoteTaskAsync(entityInfo);
        }

        public async Task<Result<List<NoteTaskDto>>> GetNoteTasksAsync(Guid idNote)
        {
            return await _repository.Notes.GetNoteTasksAsync(idNote);
        }

        public async Task<Result<NoteTaskDto>> DeleteNoteTaskAsync(Guid id)
        {
            return await _repository.Notes.DeleteNoteTaskAsync(id);
        }


        #endregion

    }
}

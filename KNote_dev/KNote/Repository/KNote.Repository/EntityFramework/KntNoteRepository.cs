using KNote.Model;
using KNote.Model.Dto;
using KNote.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Repository.EntityFramework
{
    public class KntNoteRepository: DomainActionBase, IKntNoteRepository
    {
        private IGenericRepositoryEF<KntDbContext, Note> _notes;

        public KntNoteRepository(KntDbContext context, bool throwKntException)
        {
            _notes = new GenericRepositoryEF<KntDbContext, Note>(context, throwKntException);
        }

        public Task<Result<NoteInfoDto>> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<NoteTaskDto>> DeleteNoteTaskAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<ResourceDto>> DeleteResourceAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<List<NoteInfoDto>>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Result<NoteDto>> GetAsync(Guid noteId)
        {
            throw new NotImplementedException();
        }

        public Task<Result<List<NoteInfoDto>>> GetByFolderAsync(Guid folderId)
        {
            throw new NotImplementedException();
        }

        public Task<Result<List<NoteInfoDto>>> GetFilter(NotesFilterDto notesFilter)
        {
            throw new NotImplementedException();
        }

        public int GetNextNoteNumber()
        {
            throw new NotImplementedException();
        }

        public Task<Result<List<ResourceDto>>> GetNoteResourcesAsync(Guid idNote)
        {
            throw new NotImplementedException();
        }

        public Task<Result<List<NoteTaskDto>>> GetNoteTasksAsync(Guid idNote)
        {
            throw new NotImplementedException();
        }

        public Task<Result<List<NoteInfoDto>>> GetSearch(NotesSearchDto notesSearch)
        {
            throw new NotImplementedException();
        }

        public Task<Result<List<NoteInfoDto>>> HomeNotesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Result<NoteDto>> NewAsync(NoteInfoDto entity = null)
        {
            throw new NotImplementedException();
        }

        public Task<Result<NoteDto>> SaveAsync(NoteDto entityInfo)
        {
            throw new NotImplementedException();
        }

        public Task<Result<NoteKAttributeDto>> SaveAttrtibuteAsync(NoteKAttributeDto entity)
        {
            throw new NotImplementedException();
        }

        public Task<Result<NoteTaskDto>> SaveNoteTaskAsync(NoteTaskDto entityInfo)
        {
            throw new NotImplementedException();
        }

        public Task<Result<ResourceDto>> SaveResourceAsync(ResourceDto entity)
        {
            throw new NotImplementedException();
        }

        #region  IDisposable

        public virtual void Dispose()
        {
            _notes.Dispose();
        }

        #endregion
    }
}

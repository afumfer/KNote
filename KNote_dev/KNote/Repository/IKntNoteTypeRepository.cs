using KNote.Model;
using KNote.Model.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KNote.Repository
{
    public interface IKntNoteTypeRepository: IDisposable
    {
        Task<Result<List<NoteTypeDto>>> GetAllAsync();
        Task<Result<NoteTypeDto>> GetAsync(Guid id);
        //Task<Result<NoteTypeDto>> SaveAsync(NoteTypeDto entity);
        Task<Result<NoteTypeDto>> AddAsync(NoteTypeDto entity);
        Task<Result<NoteTypeDto>> UpdateAsync(NoteTypeDto entity);
        //Task<Result<NoteTypeDto>> DeleteAsync(Guid id);
        //Task<Result<bool>> DeleteAsync(Guid id);
        Task<Result> DeleteAsync(Guid id);
    }
}

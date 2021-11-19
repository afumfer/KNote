using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Repository;

public interface IKntNoteTypeRepository: IDisposable
{
    Task<Result<List<NoteTypeDto>>> GetAllAsync();
    Task<Result<NoteTypeDto>> GetAsync(Guid id);        
    Task<Result<NoteTypeDto>> AddAsync(NoteTypeDto entity);
    Task<Result<NoteTypeDto>> UpdateAsync(NoteTypeDto entity);
    Task<Result> DeleteAsync(Guid id);
}

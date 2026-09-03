using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Repository;

public interface IKntTraceNoteTypeRepository : IDisposable
{
    Task<Result<List<TraceNoteTypeDto>>> GetAllAsync();
    Task<Result<TraceNoteTypeDto>> GetAsync(Guid id);
    Task<Result<TraceNoteTypeDto>> AddAsync(TraceNoteTypeDto entity);
    Task<Result<TraceNoteTypeDto>> UpdateAsync(TraceNoteTypeDto entity);
    Task<Result> DeleteAsync(Guid id);
}

using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Repository;

public interface IKntTraceNoteRepository : IDisposable
{
    Task<Result<List<TraceNoteDto>>> GetAllByFromIdAsync(Guid fromId);
    Task<Result<List<TraceNoteDto>>> GetAllByToIdAsync(Guid toId);
    Task<Result<TraceNoteDto>> GetAsync(Guid id);
    Task<Result<TraceNoteDto>> AddAsync(TraceNoteDto entity);
    Task<Result<TraceNoteDto>> UpdateAsync(TraceNoteDto entity);
    Task<Result> DeleteAsync(Guid id);
}

using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Repository;

public interface IKntTraceNoteRepository : IDisposable
{
    Task<Result<List<TraceNoteDto>>> GetAllByFromIdAsync(Guid fromId);
    Task<Result<List<TraceNoteDto>>> GetAllByToIdAsync(Guid toId);
    Task<Result<TraceNoteDto>> GetAsync(Guid id);
    // With the rows of the history of its two notes (KntHistory), in the same transaction (H10). Without them, as always.
    Task<Result<TraceNoteDto>> AddAsync(TraceNoteDto entity, IReadOnlyList<KLogWriteDto> history = null);
    Task<Result<TraceNoteDto>> UpdateAsync(TraceNoteDto entity, IReadOnlyList<KLogWriteDto> history = null);
    Task<Result> DeleteAsync(Guid id, IReadOnlyList<KLogWriteDto> history = null);
}

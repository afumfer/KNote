using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

// cancellationToken (all of them): the caller no longer wants the answer; see BaseService.SendAsync.
public interface ITraceNoteTypeWebApiService
{
    Task<Result<List<TraceNoteTypeDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<TraceNoteTypeDto>> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<TraceNoteTypeDto>> SaveAsync(TraceNoteTypeDto traceNoteType, CancellationToken cancellationToken = default);
    Task<Result<TraceNoteTypeDto>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

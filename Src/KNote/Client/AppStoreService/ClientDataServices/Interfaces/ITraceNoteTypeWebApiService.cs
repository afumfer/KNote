using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

public interface ITraceNoteTypeWebApiService
{
    Task<Result<List<TraceNoteTypeDto>>> GetAllAsync();
    Task<Result<TraceNoteTypeDto>> GetAsync(Guid id);
    Task<Result<TraceNoteTypeDto>> SaveAsync(TraceNoteTypeDto traceNoteType);
    Task<Result<TraceNoteTypeDto>> DeleteAsync(Guid id);
}

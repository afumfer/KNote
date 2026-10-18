using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

// cancellationToken (all of them): the caller no longer wants the answer; see BaseService.SendAsync.
public interface INoteTypeWebApiService
{
    Task<Result<List<NoteTypeDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<NoteTypeDto>> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<NoteTypeDto>> SaveAsync(NoteTypeDto noteType, CancellationToken cancellationToken = default);
    Task<Result<NoteTypeDto>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

// cancellationToken (all of them): the caller no longer wants the answer; see BaseService.SendAsync.
public interface IFolderWebApiService
{
    Task<Result<List<FolderInfoDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<List<FolderDto>>> GetTreeAsync(CancellationToken cancellationToken = default);
    // The folders directly inside Home (readable without signing in), shown beside its notes in Home.
    Task<Result<List<FolderInfoDto>>> GetHomeChildrenAsync(CancellationToken cancellationToken = default);
    Task<Result<FolderDto>> GetAsync(Guid folderId, CancellationToken cancellationToken = default);
    Task<Result<string>> GetPathAsync(Guid folderId, CancellationToken cancellationToken = default);
    Task<Result<FolderDto>> SaveAsync(FolderDto folder, CancellationToken cancellationToken = default);
    Task<Result<FolderDto>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> UpdateOrderNotesAsync(Guid folderId, string orderNotes, CancellationToken cancellationToken = default);
}

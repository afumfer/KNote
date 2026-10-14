using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

public interface IFolderWebApiService
{
    Task<Result<List<FolderInfoDto>>> GetAllAsync();
    Task<Result<List<FolderDto>>> GetTreeAsync();
    // The folders directly inside Home (readable without signing in), shown beside its notes in Home.
    Task<Result<List<FolderInfoDto>>> GetHomeChildrenAsync();
    Task<Result<FolderDto>> GetAsync(Guid folderId);
    Task<Result<string>> GetPathAsync(Guid folderId);
    Task<Result<FolderDto>> SaveAsync(FolderDto folder);
    Task<Result<FolderDto>> DeleteAsync(Guid id);
    Task<Result> UpdateOrderNotesAsync(Guid folderId, string orderNotes);
}


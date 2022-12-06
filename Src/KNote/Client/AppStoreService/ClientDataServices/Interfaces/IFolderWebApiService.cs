using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

public interface IFolderWebApiService
{
    Task<Result<List<FolderInfoDto>>> GetAllAsync();
    Task<Result<List<FolderDto>>> GetTreeAsync();
    Task<Result<FolderDto>> GetAsync(Guid folderId);
    Task<Result<FolderDto>> SaveAsync(FolderDto folder);
    Task<Result<FolderDto>> DeleteAsync(Guid id);
    //Task<Result<List<NoteInfoDto>>> GetNotes(Guid folderId);
}


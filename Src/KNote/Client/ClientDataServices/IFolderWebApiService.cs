using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.ClientDataServices;

public interface IFolderWebApiService
{
    Task<Result<List<FolderDto>>> GetAllAsync();
    Task<Result<List<FolderDto>>> GetTreeAsync();        
    Task<Result<FolderDto>> GetAsync(Guid folderId);        
    Task<Result<FolderDto>> SaveAsync(FolderDto folder);
    Task<Result<FolderDto>> DeleteAsync(Guid id);
    Task<Result<List<NoteInfoDto>>> GetNotes(Guid folderId);
}


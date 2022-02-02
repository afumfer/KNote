using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KNote.Model.Dto;
using KNote.Model;

namespace KNote.Service
{
    public interface IKntFolderService
    {        
        Task<Result<List<FolderInfoDto>>> GetAllAsync();        
        Task<Result<List<FolderDto>>> GetTreeAsync();
        Task<Result<FolderDto>> GetHomeAsync();
        Task<Result<FolderDto>> GetAsync(Guid folderId);
        Task<Result<FolderDto>> GetAsync(int folderNumber);
        Task<Result<FolderDto>> SaveAsync(FolderDto entityInfo);                        
        Task<Result<FolderDto>> DeleteAsync(Guid id);        
    }
}

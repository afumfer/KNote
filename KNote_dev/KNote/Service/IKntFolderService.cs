using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KNote.Model.Dto;
using KNote.Model;

namespace KNote.Service
{
    public interface IKntFolderService
    {        
        Task<Result<List<FolderDto>>> GetAllAsync();        
        Task<Result<List<FolderDto>>> GetTreeAsync();
        Task<Result<FolderDto>> GetAsync(Guid folderId);        
        Task<Result<FolderDto>> SaveAsync(FolderDto entityInfo);                        
        Task<Result<FolderDto>> DeleteAsync(Guid id);        
    }
}

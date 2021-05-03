using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KNote.Model.Dto;
using KNote.Model;

namespace KNote.Repository
{
    public interface IKntFolderRepository : IDisposable
    {        
        Task<Result<List<FolderDto>>> GetAllAsync();        
        Task<Result<List<FolderDto>>> GetTreeAsync(Guid? parentId = null);
        Task<Result<FolderDto>> GetAsync(Guid folderId);
        Task<Result<FolderDto>> GetHomeAsync();        
        Task<Result<FolderDto>> AddAsync(FolderDto entityInfo);
        Task<Result<FolderDto>> UpdateAsync(FolderDto entityInfo);
        Task<Result> DeleteAsync(Guid id);
    }
}

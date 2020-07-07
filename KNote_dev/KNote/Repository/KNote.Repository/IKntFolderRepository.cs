using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KNote.Model.Dto;
using KNote.Model;

namespace KNote.Repository
{
    public interface IKntFolderRepository : IDisposable
    {
        int GetNextFolderNumber();
        Task<Result<List<FolderDto>>> GetAllAsync();
        Task<Result<List<FolderDto>>> GetRootsAsync();
        Task<Result<List<FolderDto>>> GetTreeAsync();
        Task<Result<FolderDto>> GetAsync(Guid folderId);
        Task<Result<FolderDto>> SaveAsync(FolderDto entityInfo);
        Task<Result<FolderDto>> DeleteAsync(Guid id);
    }
}

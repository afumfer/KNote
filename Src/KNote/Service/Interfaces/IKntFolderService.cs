using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KNote.Model.Dto;
using KNote.Model;

namespace KNote.Service.Interfaces;

public interface IKntFolderService
{
    Task<Result<List<FolderInfoDto>>> GetAllAsync();
    Task<Result<List<FolderDto>>> GetTreeAsync();
    Task<Result<FolderDto>> GetHomeAsync();
    Task<Result<FolderDto>> GetAsync(Guid folderId);
    Task<Result<FolderDto>> GetAsync(int folderNumber);
    // The folder's path in the folders tree, from the root (e.g. "Home\Projects\KNote").
    Task<Result<string>> GetPathAsync(Guid folderId);
    Task<Result<FolderDto>> SaveAsync(FolderDto entityInfo);
    Task<Result> UpdateOrderNotesAsync(Guid folderId, string orderNotes);
    Task<Result<FolderDto>> DeleteAsync(Guid id);
}

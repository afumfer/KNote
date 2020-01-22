using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using KNote.Shared.Dto;
using KNote.Shared;

namespace KNote.DomainModel.Services
{
    public interface IKntFolderService
    {
        Result<List<FolderInfoDto>> GetAll();
        Result<List<FolderInfoDto>> GetRoots();
        Result<FolderInfoDto> Get(int folerNumber);
        Result<FolderInfoDto> Get(Guid folderId);
        Result<FolderDto> New(FolderInfoDto entity = null);
        Task<Result<FolderInfoDto>> SaveAsync(FolderInfoDto entityInfo);
        Result<FolderInfoDto> Save(FolderInfoDto entityInfo);                
        int GetNextFolderNumber();
        Result<List<FolderInfoDto>> GetTree();
        Result<FolderInfoDto> Delete(Guid id);
        Task<Result<FolderInfoDto>> DeleteAsync(Guid id);

        // TODO: 
        // Result<List<Folder>> GetAllFull(Expression<Func<Folder, bool>> predicate)
    }
}

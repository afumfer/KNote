using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using KNote.Shared.Dto.Info;
using KNote.Shared.Dto;
using KNote.Shared;

namespace KNote.DomainModel.Services
{
    public interface IKntFolderService
    {
        Result<List<FolderInfoDto>> GetAll();
        Result<List<FolderInfoDto>> GetRoots();
        Result<List<FolderInfoDto>> GetTree();
        Result<FolderDto> Get(int folerNumber);
        Result<FolderDto> Get(Guid folderId);
        Result<FolderDto> New(FolderInfoDto entity = null);
        Result<FolderDto> Save(FolderDto entityInfo);
        Task<Result<FolderDto>> SaveAsync(FolderDto entityInfo);
                
        int GetNextFolderNumber();
        
        Result<FolderInfoDto> Delete(Guid id);
        Task<Result<FolderInfoDto>> DeleteAsync(Guid id);

        // TODO: 
        // Result<List<Folder>> GetAllFull(Expression<Func<Folder, bool>> predicate)
    }
}

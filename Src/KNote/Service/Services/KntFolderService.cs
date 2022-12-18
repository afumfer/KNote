using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNote.Model.Dto;
using KNote.Model;
using System.Linq.Expressions;
using KNote.Repository;
using KNote.Service.Interfaces;
using KNote.Service.Core;

namespace KNote.Service.Services
{
    public class KntFolderService : KntServiceBase, IKntFolderService
    {
        #region Constructor

        public KntFolderService(IKntService service) : base(service)
        {
            
        }

        #endregion

        #region IKntFolderService

        public async Task<Result<List<FolderInfoDto>>> GetAllAsync()
        {
            return await Repository.Folders.GetAllAsync();
        }
        
        public async Task<Result<FolderDto>> GetAsync(Guid folderId)
        {
            return await Repository.Folders.GetAsync(folderId);
        }

        public async Task<Result<FolderDto>> GetAsync(int folderNumber)
        {
            return await Repository.Folders.GetAsync(folderNumber);
        }

        public async Task<Result<List<FolderDto>>> GetTreeAsync()
        {
            return await Repository.Folders.GetTreeAsync();
        }

        public async Task<Result<FolderDto>> GetHomeAsync()
        {
            return await Repository.Folders.GetHomeAsync();
        }


        public async Task<Result<FolderDto>> SaveAsync(FolderDto entity)
        {            
            if (entity.FolderId == Guid.Empty)
            {
                entity.FolderId = Guid.NewGuid();
                var res = await Repository.Folders.AddAsync(entity);                
                return res;
            }
            else
            {
                var res = await Repository.Folders.UpdateAsync(entity);                
                return res;
            }
        }

        public async Task<Result<FolderDto>> DeleteAsync(Guid id)
        {            
            var result = new Result<FolderDto>();

            var resGetEntity = await GetAsync(id);

            if (resGetEntity.IsValid)
            {
                result.Entity = resGetEntity.Entity;

                // Check rules
                if(result.Entity.FolderNumber == 1)
                    result.AddErrorMessage($"{result.Entity.Name} is the default folder. It cannot be erased.");

                if (resGetEntity.Entity.ChildFolders.Count > 0)                
                    result.AddErrorMessage("This folder has child folders. Delete is not possible.");
                                                        
                if ((await Repository.Notes.CountNotesInFolder(id)).Entity > 0)
                    result.AddErrorMessage("This folder has notes. Delete is not possible.");

                if(!result.IsValid)
                    return result;

                // Is OK then delete entity
                var resDelEntity = await Repository.Folders.DeleteAsync(id);
                if (resDelEntity.IsValid)
                    result.Entity = resGetEntity.Entity;
                else
                    result.AddListErrorMessage(resDelEntity.ListErrorMessage);                
            }
            else
            {
                result.AddListErrorMessage(resGetEntity.ListErrorMessage);                
            }

            return result;
        }

        #endregion
    }
}

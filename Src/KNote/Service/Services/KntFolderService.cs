using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNote.Model.Dto;
using KNote.Model;
using System.Linq.Expressions;
using KNote.Service;
using KNote.Repository;

namespace KNote.Service.Services
{
    public class KntFolderService : DomainActionBase, IKntFolderService
    {
        #region Fields

        private readonly IKntRepository _repository;

        #endregion

        #region Constructor

        protected internal KntFolderService(IKntRepository repository)
        {
            _repository = repository;
        }

        #endregion

        #region IKntFolderService

        public async Task<Result<List<FolderDto>>> GetAllAsync()
        {
            return await _repository.Folders.GetAllAsync();
        }
        
        public async Task<Result<FolderDto>> GetAsync(Guid folderId)
        {
            return await _repository.Folders.GetAsync(folderId);
        }

        public async Task<Result<FolderDto>> GetAsync(int folderNumber)
        {
            return await _repository.Folders.GetAsync(folderNumber);
        }

        public async Task<Result<List<FolderDto>>> GetTreeAsync()
        {
            return await _repository.Folders.GetTreeAsync();
        }

        public async Task<Result<FolderDto>> GetHomeAsync()
        {
            return await _repository.Folders.GetHomeAsync();
        }


        public async Task<Result<FolderDto>> SaveAsync(FolderDto entity)
        {            
            if (entity.FolderId == Guid.Empty)
            {
                entity.FolderId = Guid.NewGuid();
                var res = await _repository.Folders.AddAsync(entity);                
                return res;
            }
            else
            {
                var res = await _repository.Folders.UpdateAsync(entity);                
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
                                                        
                if ((await _repository.Notes.CountNotesInFolder(id)).Entity > 0)
                    result.AddErrorMessage("This folder has notes. Delete is not possible.");

                if(!result.IsValid)
                    return result;

                // Is OK then delete entity
                var resDelEntity = await _repository.Folders.DeleteAsync(id);
                if (resDelEntity.IsValid)
                    result.Entity = resGetEntity.Entity;
                else
                    result.ErrorList = resDelEntity.ErrorList;
            }
            else
            {
                result.ErrorList = resGetEntity.ErrorList;
            }

            return result;
        }

        #endregion
    }
}

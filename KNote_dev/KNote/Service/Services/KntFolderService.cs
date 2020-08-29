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

        public async Task<Result<List<FolderDto>>> GetTreeAsync()
        {
            return await _repository.Folders.GetTreeAsync();
        }

        public async Task<Result<FolderDto>> SaveAsync(FolderDto entity)
        {            
            if (entity.FolderId == Guid.Empty)
            {
                entity.FolderId = Guid.NewGuid();
                return await _repository.Folders.AddAsync(entity);
            }
            else
            {
                return await _repository.Folders.UpdateAsync(entity);
            }
        }

        public async Task<Result<FolderDto>> DeleteAsync(Guid id)
        {            
            var result = new Result<FolderDto>();

            var resGetEntity = await GetAsync(id);

            if (resGetEntity.IsValid)
            {
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

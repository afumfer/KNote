using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNote.Repository.EntityFramework;
using KNote.Repository.Entities;
using KNote.Model.Dto;
using KNote.Model;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
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

        public async Task <Result<List<FolderDto>>> GetRootsAsync()
        {
            return await _repository.Folders.GetRootsAsync();
        }
        
        public async Task<Result<FolderDto>> GetAsync(Guid folderId)
        {
            return await _repository.Folders.GetAsync(folderId);
        }

        public int GetNextFolderNumber()
        {
            return  _repository.Folders.GetNextFolderNumber();
        }

        public async Task<Result<List<FolderDto>>> GetTreeAsync()
        {
            return await _repository.Folders.GetTreeAsync();
        }

        public async Task<Result<FolderDto>> SaveAsync(FolderDto entity)
        {
            return await _repository.Folders.SaveAsync(entity);
        }

        public async Task<Result<FolderDto>> DeleteAsync(Guid id)
        {
            return await _repository.Folders.DeleteAsync(id);
        }

        #endregion

    }
}

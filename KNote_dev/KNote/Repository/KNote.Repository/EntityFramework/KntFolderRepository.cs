using KNote.Model;
using KNote.Model.Dto;
using KNote.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Repository.EntityFramework
{
    public class KntFolderRepository: DomainActionBase, IKntFolderRepository
    {
        private IGenericRepositoryEF<KntDbContext, Folder> _folders;

        public KntFolderRepository(KntDbContext context, bool throwKntException)
        {
            _folders = new GenericRepositoryEF<KntDbContext, Folder>(context, throwKntException);
        }

        public Task<Result<FolderDto>> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<List<FolderDto>>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Result<FolderDto>> GetAsync(Guid folderId)
        {
            throw new NotImplementedException();
        }

        public int GetNextFolderNumber()
        {
            throw new NotImplementedException();
        }

        public Task<Result<List<FolderDto>>> GetRootsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Result<List<FolderDto>>> GetTreeAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Result<FolderDto>> SaveAsync(FolderDto entityInfo)
        {
            throw new NotImplementedException();
        }

        #region  IDisposable

        public virtual void Dispose()
        {
            _folders.Dispose();
        }

        #endregion
    }
}

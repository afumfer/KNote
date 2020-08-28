using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

using Microsoft.Data.SqlClient;
using Dapper;

using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Repository.Dapper
{
    public class KntFolderRepository : DomainActionBase, IKntFolderRepository
    {
        protected SqlConnection _db;

        public KntFolderRepository(SqlConnection db, bool throwKntException)
        {
            _db = db;
            ThrowKntException = throwKntException;
        }

        public Task<Result<List<FolderDto>>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Result<FolderDto>> GetAsync(Guid folderId)
        {
            throw new NotImplementedException();
        }

        public Task<Result<FolderDto>> GetHomeAsync()
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

        public Task<Result<FolderDto>> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

    }
}

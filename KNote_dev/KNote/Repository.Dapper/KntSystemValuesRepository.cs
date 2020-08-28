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
    public class KntSystemValuesRepository : DomainActionBase, IKntSystemValuesRepository
    {
        protected SqlConnection _db;

        public KntSystemValuesRepository(SqlConnection db, bool throwKntException)
        {
            _db = db;
            ThrowKntException = throwKntException;
        }

        public Task<Result<List<SystemValueDto>>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Result<SystemValueDto>> GetAsync(string key)
        {
            throw new NotImplementedException();
        }

        public Task<Result<SystemValueDto>> GetAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<SystemValueDto>> SaveAsync(SystemValueDto entityInfo)
        {
            throw new NotImplementedException();
        }

        public Task<Result<SystemValueDto>> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

    }
}

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
    public class KntKAttributeRepository : DomainActionBase, IKntKAttributeRepository
    {
        protected SqlConnection _db;

        public KntKAttributeRepository(SqlConnection db, bool throwKntException)
        {
            _db = db;
            ThrowKntException = throwKntException;
        }

        public Task<Result<List<KAttributeDto>>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Result<List<KAttributeInfoDto>>> GetAllAsync(Guid? typeId)
        {
            throw new NotImplementedException();
        }

        public Task<Result<List<KAttributeInfoDto>>> GetAllIncludeNullTypeAsync(Guid? typeId)
        {
            throw new NotImplementedException();
        }

        public Task<Result<KAttributeDto>> GetAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<List<KAttributeTabulatedValueDto>>> GetKAttributeTabulatedValuesAsync(Guid attributeId)
        {
            throw new NotImplementedException();
        }

        public Task<Result<KAttributeDto>> SaveAsync(KAttributeDto entityInfo)
        {
            throw new NotImplementedException();
        }

        public Task<Result<KAttributeTabulatedValueDto>> SaveTabulateValueAsync(Guid attributeId, KAttributeTabulatedValueDto entityInfo)
        {
            throw new NotImplementedException();
        }

        public Task<Result<KAttributeTabulatedValueDto>> AddNewKAttributeTabulatedValueAsync(Guid id, KAttributeTabulatedValueDto entityInfo)
        {
            throw new NotImplementedException();
        }

        public Task<Result<KAttributeInfoDto>> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<KAttributeTabulatedValueDto>> DeleteKAttributeTabulatedValueAsync(Guid id)
        {
            throw new NotImplementedException();
        }


        public void Dispose()
        {
            throw new NotImplementedException();
        }

    }
}

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
    public class KntKAttributeRepository : DomainActionBase, IKntKAttributeRepository
    {
        private IGenericRepositoryEF<KntDbContext, KAttribute > _kattributes;

        public KntKAttributeRepository(KntDbContext context, bool throwKntException)
        {
            _kattributes = new GenericRepositoryEF<KntDbContext, KAttribute>(context, throwKntException);
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

        public Task<Result<List<KAttributeDto>>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Result<List<KAttributeInfoDto>>> GetAllAsync(Guid? typeId)
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

        #region  IDisposable

        public virtual void Dispose()
        {
            _kattributes.Dispose();
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using System.Linq.Expressions;
using KNote.Service;
using KNote.Repository;

namespace KNote.Service.Services
{
    public class KntKAttributeService : DomainActionBase, IKntKAttributeService
    {
        #region Fields

        private readonly IKntRepository _repository;

        #endregion

        #region Constructor

        protected internal KntKAttributeService(IKntRepository repository)
        {
            _repository = repository;
        }

        #endregion

        #region IKntAttributeService

        public async Task<Result<List<KAttributeInfoDto>>> GetAllAsync()
        {
            return await _repository.KAttributes.GetAllAsync();
        }

        public async Task<Result<List<KAttributeInfoDto>>> GetAllAsync(Guid? typeId)
        {
            return await _repository.KAttributes.GetAllAsync(typeId);
        }

        public async Task<Result<KAttributeDto>> GetAsync(Guid id)
        {
            return await _repository.KAttributes.GetAsync(id);
        }

        public async Task<Result<KAttributeDto>> SaveAsync(KAttributeDto entity)
        {            
            if (entity.KAttributeId == Guid.Empty)
            {
                entity.KAttributeId = Guid.NewGuid();
                return await _repository.KAttributes.AddAsync(entity);
            }
            else
            {                
                return await _repository.KAttributes.UpdateAsync(entity);
            }
        }

        public async Task<Result<KAttributeInfoDto>> DeleteAsync(Guid id)
        {           
            var result = new Result<KAttributeInfoDto>();

            var resGetEntity = await GetAsync(id);

            if (resGetEntity.IsValid)
            {
                var resDelEntity = await _repository.KAttributes.DeleteAsync(id);
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

        public async Task<Result<List<KAttributeTabulatedValueDto>>> GetKAttributeTabulatedValuesAsync(Guid attributeId)
        {
            return await _repository.KAttributes.GetKAttributeTabulatedValuesAsync(attributeId);
        }

        public async Task<Result<KAttributeTabulatedValueDto>> DeleteKAttributeTabulatedValueAsync(Guid id)
        {
            var result = new Result<KAttributeTabulatedValueDto>();

            var resGetEntity = await _repository.KAttributes.GetKAttributeTabulatedValueAsync(id);

            if (resGetEntity.IsValid)
            {
                var resDelEntity = await _repository.KAttributes.DeleteKAttributeTabulatedValueAsync(id);
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

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
            return await _repository.KAttributes.SaveAsync(entity);
        }

        public async Task<Result<KAttributeInfoDto>> DeleteAsync(Guid id)
        {
            return await _repository.KAttributes.DeleteAsync(id);
        }

        public async Task<Result<KAttributeTabulatedValueDto>> SaveTabulateValueAsync(Guid attributeId, KAttributeTabulatedValueDto entity)
        {
            return await _repository.KAttributes.SaveTabulateValueAsync(attributeId, entity);
        }

        public async Task<Result<KAttributeTabulatedValueDto>> AddNewKAttributeTabulatedValueAsync(Guid attributeId, KAttributeTabulatedValueDto entity)
        {
            return await _repository.KAttributes.SaveTabulateValueAsync(attributeId, entity);
        }

        public async Task<Result<KAttributeTabulatedValueDto>> DeleteKAttributeTabulatedValueAsync(Guid id)
        {
            return await _repository.KAttributes.DeleteKAttributeTabulatedValueAsync(id);
        }

        public async Task<Result<List<KAttributeTabulatedValueDto>>> GetKAttributeTabulatedValuesAsync(Guid attributeId)
        {
            return await _repository.KAttributes.GetKAttributeTabulatedValuesAsync(attributeId);
        }


        #endregion 
    }
}

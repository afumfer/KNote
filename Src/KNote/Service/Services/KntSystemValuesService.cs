using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service;
using KNote.Repository;

namespace KNote.Service.Services
{
    public class KntSystemValuesService : DomainActionBase, IKntSystemValuesService
    {
        #region Fields

        private readonly IKntRepository _repository;

        #endregion

        #region Constructor

        protected internal KntSystemValuesService(IKntRepository repository)
        {
            _repository = repository;
        }

        #endregion

        #region IKntSystemValuesService

        public async Task<Result<List<SystemValueDto>>> GetAllAsync()
        {
            return await _repository.SystemValues.GetAllAsync();
        }

        public async Task<Result<SystemValueDto>> GetAsync(string scope, string key)
        {
            return await _repository.SystemValues.GetAsync(scope, key);
        }

        public async Task<Result<SystemValueDto>> GetAsync(Guid id)
        {
            return await _repository.SystemValues.GetAsync(id);
        }

        public async Task<Result<SystemValueDto>> SaveAsync(SystemValueDto entity)
        {
            if (entity.SystemValueId == Guid.Empty)
            {
                entity.SystemValueId = Guid.NewGuid();
                return await _repository.SystemValues.AddAsync(entity);
            }
            else
            {
                return await _repository.SystemValues.UpdateAsync(entity);
            }
        }

        public async Task<Result<SystemValueDto>> DeleteAsync(Guid id)
        {
            var result = new Result<SystemValueDto>();

            var resGetEntity = await GetAsync(id);

            if (resGetEntity.IsValid)
            {
                var resDelEntity = await _repository.SystemValues.DeleteAsync(id);
                if (resDelEntity.IsValid)
                    result.Entity = resGetEntity.Entity;
                else
                    result.AddListErrorMessage(resDelEntity.ListErrorMessage);
            }
            else
            {
                result.AddListErrorMessage(resGetEntity.ListErrorMessage);
            }

            return result;
        }

        #endregion
    }
}

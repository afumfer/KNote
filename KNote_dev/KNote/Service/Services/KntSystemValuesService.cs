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

        public async Task<Result<SystemValueDto>> GetAsync(string key)
        {
            return await _repository.SystemValues.GetAsync(key);
        }

        public async Task<Result<SystemValueDto>> GetAsync(Guid id)
        {
            return await _repository.SystemValues.GetAsync(id);
        }

        public async Task<Result<SystemValueDto>> SaveAsync(SystemValueDto entity)
        {
            return await _repository.SystemValues.SaveAsync(entity);
        }

        public async Task<Result<SystemValueDto>> DeleteAsync(Guid id)
        {
            return await _repository.SystemValues.DeleteAsync(id);
        }

        #endregion
    }
}

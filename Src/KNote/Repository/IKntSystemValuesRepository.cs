using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Repository
{
    public interface IKntSystemValuesRepository : IDisposable
    {
        Task<Result<List<SystemValueDto>>> GetAllAsync();
        Task<Result<SystemValueDto>> GetAsync(string scope, string key);
        Task<Result<SystemValueDto>> GetAsync(Guid id);
        Task<Result<SystemValueDto>> AddAsync(SystemValueDto entityInfo);
        Task<Result<SystemValueDto>> UpdateAsync(SystemValueDto entityInfo);
        Task<Result> DeleteAsync(Guid id);
    }
}

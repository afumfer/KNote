using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KNote.Model;
using KNote.Model.Dto;
using KNote.Repository.Entities;


namespace KNote.Service
{
    public interface IKntSystemValuesService
    {
        Task<Result<List<SystemValueDto>>> GetAllAsync();
        Task<Result<SystemValueDto>> GetAsync(string key);
        Task<Result<SystemValueDto>> GetAsync(Guid id);
        Task<Result<SystemValueDto>> SaveAsync(SystemValueDto entityInfo);
        Task<Result<SystemValueDto>> DeleteAsync(Guid id);
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Service.Interfaces;

public interface IKntSystemValuesService
{
    Task<Result<List<SystemValueDto>>> GetAllAsync();
    Task<Result<SystemValueDto>> GetAsync(KeyValuePair<string, string> scopeKey);
    Task<Result<SystemValueDto>> GetAsync(Guid id);
    Task<Result<SystemValueDto>> SaveAsync(SystemValueDto entityInfo);
    Task<Result<SystemValueDto>> DeleteAsync(Guid id);
}

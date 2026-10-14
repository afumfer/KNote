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

    // The repository's id and name (KntConst.RepositoryIdKey/RepositoryNameKey). Read without signing in:
    // ClientWin needs them before the user is registered in the repository, and the Web shows the name in
    // its public Home. The id never changes; the name only through SaveRepositoryNameAsync (Admin).
    Task<Result<RepositoryInfoDto>> GetRepositoryInfoAsync();
    Task<Result<RepositoryInfoDto>> SaveRepositoryNameAsync(string name);
}

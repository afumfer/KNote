using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Repository;
using KNote.Service.Core;
using KNote.Service.Interfaces;
using KNote.Service.ServicesCommands;
using static Dapper.SqlMapper;

namespace KNote.Service.Services;

public class KntSystemValuesService : KntServiceBase, IKntSystemValuesService
{
    #region Constructor

    public KntSystemValuesService(IKntService service) : base(service)
    {

    }

    #endregion

    #region IKntSystemValuesService

    public async Task<Result<List<SystemValueDto>>> GetAllAsync()
    {        
        var command = new KntSystemValueGetAllAsyncCommand(Service);
        return await ExecuteCommand(command);
    }

    public async Task<Result<SystemValueDto>> GetAsync(KeyValuePair<string, string> scopeKey) // string scope, string key
    {
        var command = new KntSystemValueGetByScopeKeyAsyncCommand(Service, scopeKey);
        return await ExecuteCommand(command);
    }

    public async Task<Result<SystemValueDto>> GetAsync(Guid id)
    {
        var command = new KntSystemValueGetAsyncCommand(Service, id);
        return await ExecuteCommand(command);
    }

    public async Task<Result<SystemValueDto>> SaveAsync(SystemValueDto entity)
    {
        var command = new KntSystemValueSaveAsyncCommand(Service, entity);
        return await ExecuteCommand(command);
    }

    public async Task<Result<SystemValueDto>> DeleteAsync(Guid id)
    {
        var command = new KntSystemValueDeleteAsyncCommand(Service, id);
        return await ExecuteCommand(command);
    }

    #endregion
}

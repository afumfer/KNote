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
        //return await Repository.SystemValues.GetAllAsync();
        var command = new KntSystemValueGetAllAsyncCommand(Service);
        return await ExecuteCommand(command);
    }

    public async Task<Result<SystemValueDto>> GetAsync(KeyValuePair<string, string> scopeKey) // string scope, string key
    {

        //return await Repository.SystemValues.GetAsync(scope, key);
        var command = new KntSystemValueGetByScopeKeyAsyncCommand(Service, scopeKey);
        return await ExecuteCommand(command);

    }

    public async Task<Result<SystemValueDto>> GetAsync(Guid id)
    {
        //return await Repository.SystemValues.GetAsync(id);
        var command = new KntSystemValueGetAsyncCommand(Service, id);
        return await ExecuteCommand(command);
    }

    public async Task<Result<SystemValueDto>> SaveAsync(SystemValueDto entity)
    {
        //if (entity.SystemValueId == Guid.Empty)
        //{
        //    entity.SystemValueId = Guid.NewGuid();
        //    return await Repository.SystemValues.AddAsync(entity);
        //}
        //else
        //{
        //    return await Repository.SystemValues.UpdateAsync(entity);
        //}
        var command = new KntSystemValueSaveAsyncCommand(Service, entity);
        return await ExecuteCommand(command);
    }

    public async Task<Result<SystemValueDto>> DeleteAsync(Guid id)
    {
        //var result = new Result<SystemValueDto>();

        //var resGetEntity = await GetAsync(id);

        //if (resGetEntity.IsValid)
        //{
        //    var resDelEntity = await Repository.SystemValues.DeleteAsync(id);
        //    if (resDelEntity.IsValid)
        //        result.Entity = resGetEntity.Entity;
        //    else
        //        result.AddListErrorMessage(resDelEntity.ListErrorMessage);
        //}
        //else
        //{
        //    result.AddListErrorMessage(resGetEntity.ListErrorMessage);
        //}

        //return result;
        var command = new KntSystemValueDeleteAsyncCommand(Service, id);
        return await ExecuteCommand(command);
    }

    #endregion
}

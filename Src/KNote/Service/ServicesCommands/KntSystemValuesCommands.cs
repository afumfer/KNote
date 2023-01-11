using KNote.Model.Dto;
using KNote.Model;
using KNote.Service.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KNote.Service.ServicesCommands;

public class KntSystemValueGetAllAsyncCommand : KntCommandServiceBase<Result<List<SystemValueDto>>>
{
    public KntSystemValueGetAllAsyncCommand(IKntService service) : base(service)
    {

    }

    public override async Task<Result<List<SystemValueDto>>> Execute()
    {
        return await Repository.SystemValues.GetAllAsync();
    }
}

public class KntSystemValueGetByScopeKeyAsyncCommand : KntCommandServiceBase<KeyValuePair<string, string>, Result<SystemValueDto>>
{
    public KntSystemValueGetByScopeKeyAsyncCommand(IKntService service, KeyValuePair<string, string> scopeKey) : base(service, scopeKey)
    {
    }

    public override async Task<Result<SystemValueDto>> Execute()
    {
        return await Repository.SystemValues.GetAsync(Param.Key, Param.Value);
    }
}

public class KntSystemValueGetAsyncCommand : KntCommandServiceBase<Guid, Result<SystemValueDto>>
{
    public KntSystemValueGetAsyncCommand(IKntService service,Guid id) : base(service, id)
    {
    }

    public override async Task<Result<SystemValueDto>> Execute()
    {
        return await Repository.SystemValues.GetAsync(Param);
    }
}

public class KntSystemValueSaveAsyncCommand : KntCommandSaveServiceBase<SystemValueDto, Result<SystemValueDto>>
{
    public KntSystemValueSaveAsyncCommand(IKntService service, SystemValueDto entity) : base(service, entity)
    {

    }

    public override async Task<Result<SystemValueDto>> Execute()
    {
        if (Param.SystemValueId == Guid.Empty)
        {
            Param.SystemValueId = Guid.NewGuid();
            return await Repository.SystemValues.AddAsync(Param);
        }
        else
        {
            return await Repository.SystemValues.UpdateAsync(Param);
        }
    }
}

public class KntSystemValueDeleteAsyncCommand : KntCommandServiceBase<Guid, Result<SystemValueDto>>
{
    public KntSystemValueDeleteAsyncCommand(IKntService service, Guid id) : base(service, id)
    {

    }

    public override async Task<Result<SystemValueDto>> Execute()
    {
        var result = new Result<SystemValueDto>();

        var resGetEntity = await Repository.SystemValues.GetAsync(Param);

        if (resGetEntity.IsValid)
        {
            var resDelEntity = await Repository.SystemValues.DeleteAsync(Param);
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
}
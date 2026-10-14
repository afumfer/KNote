using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KNote.Model.Dto;
using KNote.Model;
using KNote.Service.Core;

namespace KNote.Service.ServicesCommands;

[KntAuthorize(EnumRoles.Guest)]
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

[KntAuthorize(EnumRoles.Guest)]
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

[KntAuthorize(EnumRoles.Guest)]
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

[KntAuthorize(EnumRoles.Admin)]
public class KntSystemValueSaveAsyncCommand : KntCommandSaveServiceBase<SystemValueDto, Result<SystemValueDto>>
{
    public KntSystemValueSaveAsyncCommand(IKntService service, SystemValueDto entity) : base(service, entity)
    {

    }

    public override async Task<Result<SystemValueDto>> Execute()
    {
        var result = new Result<SystemValueDto>();

        if (KntSystemValueProtectedKeys.IsProtected(Param))
        {
            result.AddErrorMessage(KntSystemValueProtectedKeys.ErrorMessage(Param));
            return result;
        }

        if (Param.SystemValueId != Guid.Empty)
        {
            // Not by turning a protected row into another one either.
            var resStored = await Repository.SystemValues.GetAsync(Param.SystemValueId);
            if (resStored.IsValid && KntSystemValueProtectedKeys.IsProtected(resStored.Entity))
            {
                result.AddErrorMessage(KntSystemValueProtectedKeys.ErrorMessage(resStored.Entity));
                return result;
            }
        }

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

[KntAuthorize(EnumRoles.Admin)]
public class KntSystemValueDeleteAsyncCommand : KntCommandServiceBase<Guid, Result<SystemValueDto>>
{
    public KntSystemValueDeleteAsyncCommand(IKntService service, Guid id) : base(service, id)
    {

    }

    public override async Task<Result<SystemValueDto>> Execute()
    {
        var result = new Result<SystemValueDto>();

        var resGetEntity = await Repository.SystemValues.GetAsync(Param);

        if (resGetEntity.IsValid && KntSystemValueProtectedKeys.IsProtected(resGetEntity.Entity))
        {
            result.AddErrorMessage(KntSystemValueProtectedKeys.ErrorMessage(resGetEntity.Entity));
        }
        else if (resGetEntity.IsValid)
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

[KntAllowAnonymous]
public class KntSystemValueGetRepositoryInfoAsyncCommand : KntCommandServiceBase<Result<RepositoryInfoDto>>
{
    public KntSystemValueGetRepositoryInfoAsyncCommand(IKntService service) : base(service)
    {

    }

    public override async Task<Result<RepositoryInfoDto>> Execute()
    {
        var result = new Result<RepositoryInfoDto>();

        var resId = await Repository.SystemValues.GetAsync(KntConst.SystemScope, KntConst.RepositoryIdKey);
        var resName = await Repository.SystemValues.GetAsync(KntConst.SystemScope, KntConst.RepositoryNameKey);

        // Both are added by KntSchemaUpdater when the repository is opened, so a missing one means a
        // damaged database.
        if (!resId.IsValid || !Guid.TryParse(resId.Entity?.Value, out var repositoryId))
            result.AddErrorMessage($"The repository has no valid {KntConst.RepositoryIdKey}.");
        else if (!resName.IsValid || string.IsNullOrEmpty(resName.Entity?.Value))
            result.AddErrorMessage($"The repository has no {KntConst.RepositoryNameKey}.");
        else
            result.Entity = new RepositoryInfoDto { RepositoryId = repositoryId, Name = resName.Entity.Value };

        return result;
    }
}

[KntAuthorize(EnumRoles.Admin)]
public class KntSystemValueSaveRepositoryNameAsyncCommand : KntCommandServiceBase<string, Result<RepositoryInfoDto>>
{
    public const int MaxNameLength = 256;

    public KntSystemValueSaveRepositoryNameAsyncCommand(IKntService service, string name) : base(service, name?.Trim())
    {

    }

    public override Result ValidateParam()
    {
        var result = new Result();
        if (string.IsNullOrEmpty(Param))
            result.AddErrorMessage("The repository name is required.");
        else if (Param.Length > MaxNameLength)
            result.AddErrorMessage($"The repository name cannot be longer than {MaxNameLength} characters.");
        return result;
    }

    public override async Task<Result<RepositoryInfoDto>> Execute()
    {
        var resStored = await Repository.SystemValues.GetAsync(KntConst.SystemScope, KntConst.RepositoryNameKey);

        var resSave = resStored.IsValid && resStored.Entity != null
            ? await Repository.SystemValues.UpdateAsync(new SystemValueDto
            {
                SystemValueId = resStored.Entity.SystemValueId, Scope = KntConst.SystemScope, Key = KntConst.RepositoryNameKey, Value = Param
            })
            : await Repository.SystemValues.AddAsync(new SystemValueDto
            {
                SystemValueId = Guid.NewGuid(), Scope = KntConst.SystemScope, Key = KntConst.RepositoryNameKey, Value = Param
            });

        if (!resSave.IsValid)
        {
            var result = new Result<RepositoryInfoDto>();
            result.AddListErrorMessage(resSave.ListErrorMessage);
            return result;
        }

        return await new KntSystemValueGetRepositoryInfoAsyncCommand(Service).Execute();
    }
}

// SystemValues rows that only KntSchemaUpdater and the repository identity commands above may change: the
// generic save/delete commands (also reachable from KntScript) leave them alone.
internal static class KntSystemValueProtectedKeys
{
    public static bool IsProtected(SystemValueDto value) =>
        value != null
        && string.Equals(value.Scope, KntConst.SystemScope, StringComparison.OrdinalIgnoreCase)
        && (string.Equals(value.Key, KntConst.DbVersionKey, StringComparison.OrdinalIgnoreCase)
            || string.Equals(value.Key, KntConst.RepositoryIdKey, StringComparison.OrdinalIgnoreCase)
            || string.Equals(value.Key, KntConst.RepositoryNameKey, StringComparison.OrdinalIgnoreCase));

    public static string ErrorMessage(SystemValueDto value) =>
        $"The system value {value.Scope}/{value.Key} is managed by {KntConst.AppName} and cannot be changed or deleted this way.";
}

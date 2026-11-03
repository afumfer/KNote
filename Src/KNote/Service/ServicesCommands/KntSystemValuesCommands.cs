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

[KntAuthorize(EnumRoles.Guest)]
public class KntSystemValueGetRepositoryAiModelsAsyncCommand : KntCommandServiceBase<Result<RepositoryAiModelsDto>>
{
    public KntSystemValueGetRepositoryAiModelsAsyncCommand(IKntService service) : base(service)
    {

    }

    public override async Task<Result<RepositoryAiModelsDto>> Execute()
    {
        var models = new RepositoryAiModelsDto
        {
            EmbeddingModel = await ReadAsync(KntConst.AiEmbedModelKey),
            EmbeddingDimensions = int.TryParse(await ReadAsync(KntConst.AiEmbedDimsKey), out var dimensions) ? dimensions : null,
            QueryInstruction = await ReadAsync(KntConst.AiEmbedQueryInstructionKey),
            LlmModel = await ReadAsync(KntConst.AiLlmModelKey)
        };
        models.EmbeddingModelAvailable = models.EmbeddingModel != null && Service.EmbeddingGenerators?.Find(models.EmbeddingModel) != null;
        models.SetIsDirty(false);
        return new Result<RepositoryAiModelsDto>(models);
    }

    // A missing row is an empty value.
    private async Task<string> ReadAsync(string key)
    {
        var res = await Repository.SystemValues.GetAsync(KntConst.AiScope, key);
        return res.IsValid ? RepositoryAiModelsDto.Normalize(res.Entity?.Value) : null;
    }
}

[KntAuthorize(EnumRoles.Admin)]
public class KntSystemValueSaveRepositoryAiModelsAsyncCommand : KntCommandSaveServiceBase<RepositoryAiModelsDto, Result<RepositoryAiModelsDto>>
{
    public KntSystemValueSaveRepositoryAiModelsAsyncCommand(IKntService service, RepositoryAiModelsDto models) : base(service, models)
    {

    }

    public override Result ValidateParam()
    {
        if (Param != null)
            return base.ValidateParam();

        var result = new Result();
        result.AddErrorMessage("The repository's AI models are required.");
        return result;
    }

    public override async Task<Result<RepositoryAiModelsDto>> Execute()
    {
        var values = new (string Key, string Value)[]
        {
            (KntConst.AiEmbedModelKey, RepositoryAiModelsDto.Normalize(Param.EmbeddingModel)),
            (KntConst.AiEmbedDimsKey, Param.EmbeddingDimensions?.ToString()),
            (KntConst.AiEmbedQueryInstructionKey, RepositoryAiModelsDto.Normalize(Param.QueryInstruction)),
            (KntConst.AiLlmModelKey, RepositoryAiModelsDto.Normalize(Param.LlmModel))
        };

        foreach (var (key, value) in values)
        {
            var resSave = await WriteAsync(key, value);
            if (!resSave.IsValid)
            {
                var result = new Result<RepositoryAiModelsDto>();
                result.AddListErrorMessage(resSave.ListErrorMessage);
                return result;
            }
        }

        return await new KntSystemValueGetRepositoryAiModelsAsyncCommand(Service).Execute();
    }

    // An empty value has no row.
    private async Task<Result> WriteAsync(string key, string value)
    {
        var resStored = await Repository.SystemValues.GetAsync(KntConst.AiScope, key);
        var stored = resStored.IsValid ? resStored.Entity : null;

        if (value == null)
            return stored == null ? new Result() : await Repository.SystemValues.DeleteAsync(stored.SystemValueId);

        if (stored != null && stored.Value == value)
            return new Result();

        return stored != null
            ? await Repository.SystemValues.UpdateAsync(new SystemValueDto { SystemValueId = stored.SystemValueId, Scope = KntConst.AiScope, Key = key, Value = value })
            : await Repository.SystemValues.AddAsync(new SystemValueDto { SystemValueId = Guid.NewGuid(), Scope = KntConst.AiScope, Key = key, Value = value });
    }
}

// SystemValues rows that only KntSchemaUpdater and the repository commands above may change (the identity, and the AI
// models: changing the embedding model must go through its command): the generic save/delete commands (also
// reachable from KntScript) leave them alone.
internal static class KntSystemValueProtectedKeys
{
    public static bool IsProtected(SystemValueDto value) =>
        value != null
        && ((string.Equals(value.Scope, KntConst.SystemScope, StringComparison.OrdinalIgnoreCase)
            && (string.Equals(value.Key, KntConst.DbVersionKey, StringComparison.OrdinalIgnoreCase)
                || string.Equals(value.Key, KntConst.RepositoryIdKey, StringComparison.OrdinalIgnoreCase)
                || string.Equals(value.Key, KntConst.RepositoryNameKey, StringComparison.OrdinalIgnoreCase)))
            || string.Equals(value.Scope, KntConst.AiScope, StringComparison.OrdinalIgnoreCase));

    public static string ErrorMessage(SystemValueDto value) =>
        $"The system value {value.Scope}/{value.Key} is managed by {KntConst.AppName} and cannot be changed or deleted this way.";
}

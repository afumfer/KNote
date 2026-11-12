using System;
using System.Text.Json;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Service.Core;

// What KNote keeps of the runs of «Process pending resources» of a repository (SystemValues AI / AI_RESOURCES_STATE, a
// protected key, as all of the scope AI), as JSON. A value that cannot be read is no state: the estimate waits for a run.
internal static class KntResourceProcessingState
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task<ResourceProcessingStateDto> ReadAsync(IKntService service)
    {
        var res = await service.Repository.SystemValues.GetAsync(KntConst.AiScope, KntConst.AiResourcesStateKey);
        var json = res.IsValid ? res.Entity?.Value : null;
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<ResourceProcessingStateDto>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static async Task<Result> WriteAsync(IKntService service, ResourceProcessingStateDto state)
    {
        var value = JsonSerializer.Serialize(state, JsonOptions);
        var resStored = await service.Repository.SystemValues.GetAsync(KntConst.AiScope, KntConst.AiResourcesStateKey);
        var stored = resStored.IsValid ? resStored.Entity : null;

        return stored != null
            ? await service.Repository.SystemValues.UpdateAsync(new SystemValueDto
            {
                SystemValueId = stored.SystemValueId, Scope = KntConst.AiScope, Key = KntConst.AiResourcesStateKey, Value = value
            })
            : await service.Repository.SystemValues.AddAsync(new SystemValueDto
            {
                SystemValueId = Guid.NewGuid(), Scope = KntConst.AiScope, Key = KntConst.AiResourcesStateKey, Value = value
            });
    }
}

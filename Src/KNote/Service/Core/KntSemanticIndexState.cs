using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Service.Core;

// What KNote keeps of the semantic index of a repository (SystemValues AI / AI_INDEX_STATE, a protected key: only the
// indexer writes it), as JSON. A value that cannot be read is no state (the index is then created again from what the
// rows say: the notes with rows are not pending).
internal static class KntSemanticIndexState
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public static async Task<SemanticIndexStateDto> ReadAsync(IKntService service)
    {
        var res = await service.Repository.SystemValues.GetAsync(KntConst.AiScope, KntConst.AiIndexStateKey);
        var json = res.IsValid ? res.Entity?.Value : null;
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<SemanticIndexStateDto>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static async Task<Result> WriteAsync(IKntService service, SemanticIndexStateDto state)
    {
        var value = JsonSerializer.Serialize(state, JsonOptions);
        var resStored = await service.Repository.SystemValues.GetAsync(KntConst.AiScope, KntConst.AiIndexStateKey);
        var stored = resStored.IsValid ? resStored.Entity : null;

        return stored != null
            ? await service.Repository.SystemValues.UpdateAsync(new SystemValueDto
            {
                SystemValueId = stored.SystemValueId, Scope = KntConst.AiScope, Key = KntConst.AiIndexStateKey, Value = value
            })
            : await service.Repository.SystemValues.AddAsync(new SystemValueDto
            {
                SystemValueId = Guid.NewGuid(), Scope = KntConst.AiScope, Key = KntConst.AiIndexStateKey, Value = value
            });
    }
}

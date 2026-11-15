using System;
using System.Text.Json;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Service.Core;

/// <summary>What KNote keeps of the summaries (KNote Atlas, stage 2, D18): AI_SUMMARY_STATE, in SystemValues (scope AI).</summary>
internal static class KntSummaryState
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task<SummaryStateDto> ReadAsync(IKntService service)
    {
        var res = await service.Repository.SystemValues.GetAsync(KntConst.AiScope, KntConst.AiSummaryStateKey);
        var json = res.IsValid ? res.Entity?.Value : null;
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            var state = JsonSerializer.Deserialize<SummaryStateDto>(json, JsonOptions);
            if (state != null)
                state.SecondsPer1000Characters ??= new();
                state.TranslationSecondsPer1000Characters ??= new();
            return state;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static async Task<Result> WriteAsync(IKntService service, SummaryStateDto state)
    {
        var value = JsonSerializer.Serialize(state, JsonOptions);
        var resStored = await service.Repository.SystemValues.GetAsync(KntConst.AiScope, KntConst.AiSummaryStateKey);
        var stored = resStored.IsValid ? resStored.Entity : null;

        return stored != null
            ? await service.Repository.SystemValues.UpdateAsync(new SystemValueDto
            {
                SystemValueId = stored.SystemValueId, Scope = KntConst.AiScope, Key = KntConst.AiSummaryStateKey, Value = value
            })
            : await service.Repository.SystemValues.AddAsync(new SystemValueDto
            {
                SystemValueId = Guid.NewGuid(), Scope = KntConst.AiScope, Key = KntConst.AiSummaryStateKey, Value = value
            });
    }
}

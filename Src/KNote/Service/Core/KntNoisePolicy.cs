using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;

namespace KNote.Service.Core;

/// <summary>
/// The noise policy of a repository (KNote Atlas, stage 2, 2.2, D3): AI_NOISE_POLICY in SystemValues (scope AI, protected from the
/// generic commands of SystemValues), a JSON with the ids of the note types and of the folders left out; and what NoiseRules needs
/// of the repository (the ids that exist, and the ones of KNote's own notes).
/// </summary>
internal static class KntNoisePolicy
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>The ids of the note types and folders of a repository, and which are KNote's own (always out).</summary>
    public sealed record Context(ISet<Guid> NoteTypeIds, ISet<Guid> FolderIds, ISet<Guid> KNoteNoteTypeIds, ISet<Guid> KNoteFolderIds)
    {
        public NoisePolicyDto Clean(NoisePolicyDto policy) => NoiseRules.Clean(policy, NoteTypeIds, FolderIds, KNoteNoteTypeIds, KNoteFolderIds);
    }

    public static async Task<Context> LoadContextAsync(IKntService service)
    {
        var types = await service.Repository.NoteTypes.GetAllAsync();
        var folders = await service.Repository.Folders.GetAllAsync();
        var typeList = types.IsValid ? types.Entity ?? new() : new();
        var folderList = folders.IsValid ? folders.Entity ?? new() : new();
        return new Context(
            typeList.Select(t => t.NoteTypeId).ToHashSet(),
            folderList.Select(f => f.FolderId).ToHashSet(),
            typeList.Where(t => NoiseRules.KNoteNoteTypeNames.Contains(t.Name)).Select(t => t.NoteTypeId).ToHashSet(),
            folderList.Where(f => KntConst.SystemFolderNames.Contains(f.Name)).Select(f => f.FolderId).ToHashSet());
    }

    public static async Task<NoisePolicyDto> ReadAsync(IKntService service)
    {
        var res = await service.Repository.SystemValues.GetAsync(KntConst.AiScope, KntConst.AiNoisePolicyKey);
        var json = res.IsValid ? res.Entity?.Value : null;
        if (string.IsNullOrWhiteSpace(json))
            return new NoisePolicyDto();

        try
        {
            var policy = JsonSerializer.Deserialize<NoisePolicyDto>(json, JsonOptions) ?? new NoisePolicyDto();
            policy.ExcludedNoteTypeIds ??= new();
            policy.ExcludedFolderIds ??= new();
            return policy;
        }
        catch (JsonException)
        {
            return new NoisePolicyDto();
        }
    }

    public static async Task<Result> WriteAsync(IKntService service, NoisePolicyDto policy)
    {
        var value = JsonSerializer.Serialize(policy, JsonOptions);
        var resStored = await service.Repository.SystemValues.GetAsync(KntConst.AiScope, KntConst.AiNoisePolicyKey);
        var stored = resStored.IsValid ? resStored.Entity : null;

        return stored != null
            ? await service.Repository.SystemValues.UpdateAsync(new SystemValueDto
            {
                SystemValueId = stored.SystemValueId, Scope = KntConst.AiScope, Key = KntConst.AiNoisePolicyKey, Value = value
            })
            : await service.Repository.SystemValues.AddAsync(new SystemValueDto
            {
                SystemValueId = Guid.NewGuid(), Scope = KntConst.AiScope, Key = KntConst.AiNoisePolicyKey, Value = value
            });
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Service.Core;

/// <summary>
/// What the searches by meaning of KNote Atlas share (the search and the filter of delivery 0.4, the similar notes of
/// 0.5): the index of the repository's model with this installation's generator of it, the notes that can be found and
/// the result with the fragments of the notes found.
/// </summary>
internal static class KntSemanticSearcher
{
    public sealed record Index(RepositoryAiModelsDto Models, string Key, IKntEmbeddingGenerator Generator);

    /// <summary>
    /// The index to search, or null with the reason in result: without the repository's model, without its generator
    /// in this installation or without any row (a search never creates the index: its first rows come from an Admin's
    /// run, so the whole repository is not sent to the model without asking).
    /// </summary>
    public static async Task<Index> OpenAsync(IKntService service, ResultBase result)
    {
        var resModels = await service.SystemValues.GetRepositoryAiModelsAsync();
        if (!resModels.IsValid)
        {
            result.AddListErrorMessage(resModels.ListErrorMessage);
            return null;
        }
        var models = resModels.Entity;
        var indexKey = models.GetIndexKey();
        if (indexKey == null)
        {
            result.AddErrorMessage("The repository has no embedding model (KNote Atlas, repository's AI models).");
            return null;
        }
        var generator = service.EmbeddingGenerators?.Find(models.EmbeddingModel);
        if (generator == null)
        {
            result.AddErrorMessage($"This installation has no embeddings provider with the model {models.EmbeddingModel} (AI providers).");
            return null;
        }
        var resWatermark = await service.Repository.SemanticIndex.GetIndexWatermarkAsync(indexKey);
        if (!resWatermark.IsValid)
        {
            result.AddListErrorMessage(resWatermark.ListErrorMessage);
            return null;
        }
        if (resWatermark.Entity.Count == 0)
        {
            result.AddErrorMessage("The repository has no semantic index yet (KNote Atlas, semantic index).");
            return null;
        }
        return new Index(models, indexKey, generator);
    }

    /// <summary>The vectors of the index, as they are now (KntSemanticIndexCache).</summary>
    public static Task<KntSemanticIndexCache.Snapshot> VectorsAsync(IKntService service, Index index, CancellationToken cancellationToken) =>
        KntSemanticIndexCache.For(service.RepositoryRef?.ConnectionString ?? "", index.Key)
            .GetAsync(service.Repository.SemanticIndex, index.Key, cancellationToken);

    /// <summary>
    /// The notes that can be found: the ones the filter of always gives (all, without a filter), read through the
    /// service, so with its rules of who sees what, and with their data for the lists. Null with the reason in result.
    /// </summary>
    public static async Task<Dictionary<Guid, NoteMinimalDto>> AllowedNotesAsync(IKntService service, NotesFilterDto filter, ResultBase result)
    {
        filter = filter?.GetSimpleDto<NotesFilterDto>() ?? new NotesFilterDto();
        filter.PageIdentifier = new PageIdentifier();
        filter.OrderColumn = null;
        var resNotes = await service.Notes.GetFilterMinimalAsync(filter);
        if (!resNotes.IsValid)
        {
            result.AddListErrorMessage(resNotes.ListErrorMessage);
            return null;
        }
        return resNotes.Entity.GroupBy(n => n.NoteId).ToDictionary(g => g.Key, g => g.First());
    }

    /// <summary>
    /// The notes found, in the order of the matches, each with its similarity and the start of its fragment, and the
    /// notes of the index still pending. Null with the reason in result.
    /// </summary>
    public static async Task<SemanticSearchResultDto> ResultAsync(IKntService service, Index index, IReadOnlyList<SemanticSearchRules.NoteMatch> matches,
        IReadOnlyDictionary<Guid, NoteMinimalDto> notes, ResultBase result)
    {
        var contents = await ChunksAsync(service, matches.Select(m => m.NoteChunkId), result);
        if (contents == null)
            return null;
        // KNote Atlas, stage 4 (D14): a fragment of a resource says which one, and its page.
        var resources = await ResourcesAsync(service, contents.Values.Where(IsOfResource).Select(c => c.SourceId), result);
        if (resources == null)
            return null;

        var found = new SemanticSearchResultDto();
        foreach (var match in matches)
        {
            var hit = notes[match.NoteId].GetSimpleDto<NoteSearchHitDto>();
            var chunk = contents.GetValueOrDefault(match.NoteChunkId);
            hit.Similarity = match.Similarity;
            hit.Fragment = SemanticSearchRules.Fragment(chunk?.Content);
            hit.FragmentSource = match.SourceType;
            if (chunk != null && IsOfResource(chunk) && resources.TryGetValue(chunk.SourceId, out var resource))
            {
                hit.FragmentResource = resource.NameOut;
                hit.FragmentPage = chunk.Page;
            }
            found.Notes.Add(hit);
        }

        await PendingNotesAsync(service, index, found);
        return found;
    }

    /// <summary>
    /// The photos or the documents found (KNote Atlas, stage 4, D14), in the order of the matches, each with its note, its similarity,
    /// the start of its fragment and its page, and the notes of the index still pending. Null with the reason in result.
    /// </summary>
    public static async Task<SemanticSearchResultDto> ResourceResultAsync(IKntService service, Index index,
        IReadOnlyList<SemanticSearchRules.ResourceMatch> matches, IReadOnlyDictionary<Guid, NoteMinimalDto> notes, ResultBase result)
    {
        var contents = await ChunksAsync(service, matches.Select(m => m.NoteChunkId), result);
        if (contents == null)
            return null;
        var resources = await ResourcesAsync(service, matches.Select(m => m.ResourceId), result);
        if (resources == null)
            return null;

        var found = new SemanticSearchResultDto();
        foreach (var match in matches)
        {
            // Deleted meanwhile: the index is not up to date yet.
            if (!resources.TryGetValue(match.ResourceId, out var resource))
                continue;
            var chunk = contents.GetValueOrDefault(match.NoteChunkId);
            var hit = notes[match.NoteId].GetSimpleDto<ResourceSearchHitDto>();
            hit.Similarity = match.Similarity;
            hit.Fragment = SemanticSearchRules.Fragment(chunk?.Content);
            hit.FragmentSource = match.SourceType;
            hit.FragmentResource = resource.NameOut;
            hit.FragmentPage = chunk?.Page;
            hit.Resource = resource;
            found.Resources.Add(hit);
        }

        await PendingNotesAsync(service, index, found);
        return found;
    }

    /// <summary>
    /// What each resource of the notes that are not KNote's own is, for the search by kind: a photo or a document (the other
    /// files, none). Null with the reason in result.
    /// </summary>
    public static async Task<Dictionary<Guid, EnumResourceKind>> ResourceKindsAsync(IKntService service, ResultBase result)
    {
        var excluded = await KntSystemNotes.GetSystemNoteTypeIdsAsync(service.Repository);
        var states = await service.Repository.Notes.GetResourceTextStatesAsync(excluded);
        if (!states.IsValid)
        {
            result.AddListErrorMessage(states.ListErrorMessage);
            return null;
        }
        var kinds = new Dictionary<Guid, EnumResourceKind>();
        foreach (var state in states.Entity ?? new())
        {
            if (AiPhotoRules.IsPhoto(state.FileType, state.Name))
                kinds[state.ResourceId] = EnumResourceKind.Photos;
            else if (DocumentTextRules.IsDocument(state.FileType, state.Name))
                kinds[state.ResourceId] = EnumResourceKind.Documents;
        }
        return kinds;
    }

    private static bool IsOfResource(NoteChunkDto chunk) =>
        chunk.SourceType is NoteChunkSourceType.Resource or NoteChunkSourceType.ResourceText;

    private static async Task<Dictionary<Guid, NoteChunkDto>> ChunksAsync(IKntService service, IEnumerable<Guid> noteChunkIds, ResultBase result)
    {
        var ids = noteChunkIds.ToList();
        if (ids.Count == 0)
            return new Dictionary<Guid, NoteChunkDto>();
        var resContents = await service.Repository.SemanticIndex.GetChunkContentsAsync(ids);
        if (!resContents.IsValid)
        {
            result.AddListErrorMessage(resContents.ListErrorMessage);
            return null;
        }
        return resContents.Entity.ToDictionary(c => c.NoteChunkId);
    }

    // The resources, without their file, with their URLs (to open them), as the lists of the resources of a note give them.
    private static async Task<Dictionary<Guid, ResourceInfoDto>> ResourcesAsync(IKntService service, IEnumerable<Guid> resourceIds, ResultBase result)
    {
        var ids = resourceIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<Guid, ResourceInfoDto>();
        var resResources = await service.Repository.Notes.GetResourcesWithoutContentAsync(ids);
        if (!resResources.IsValid)
        {
            result.AddListErrorMessage(resResources.ListErrorMessage);
            return null;
        }
        var resources = new Dictionary<Guid, ResourceInfoDto>();
        foreach (var resource in resResources.Entity ?? new())
        {
            var info = resource.GetSimpleDto<ResourceInfoDto>();
            info.ContentBase64 = null;
            info.LoadedDescription = resource.Description ?? "";
            (info.RelativeUrl, info.FullUrl) = service.Notes.UtilGetResourceUrls(resource);
            resources[resource.ResourceId] = info;
        }
        return resources;
    }

    // The notes not up to date and, of them (4.6b), the ones left for a full run and the ones that can't be indexed whole here.
    private static async Task PendingNotesAsync(IKntService service, Index index, SemanticSearchResultDto found)
    {
        var excluded = await KntSystemNotes.GetSystemNoteTypeIdsAsync(service.Repository);
        var resSummary = await service.Repository.SemanticIndex.GetIndexSummaryAsync(index.Key, excluded);
        if (!resSummary.IsValid)
            return;
        found.PendingNotes = resSummary.Entity.PendingNotes;
        if (found.PendingNotes == 0)
            return;

        var pending = await service.Repository.SemanticIndex.GetPendingNoteIdsAsync(index.Key, MaxPendingClassified, excluded);
        foreach (var noteId in pending.Entity ?? new())
        {
            if (await HasDocumentsWithoutFileAsync(service, noteId))
                found.NotesWithoutFiles++;
            else if (KntSemanticIndexer.IsLeftForFullRun(service, index.Key, noteId))
                found.NotesForFullRun++;
        }
    }

    // The pending notes looked at to say why they are pending (more are counted as pending only).
    private const int MaxPendingClassified = 500;

    // A document of the note not read yet whose file is not in this installation (each one has its folder of resources): a full run
    // can't read it, and the note stays pending.
    private static async Task<bool> HasDocumentsWithoutFileAsync(IKntService service, Guid noteId)
    {
        foreach (var document in await KntDocuments.GetDocumentsAsync(service, noteId))
        {
            if (document.State != EnumExtractedState.NotRead)
                continue;
            var resource = await service.Repository.Notes.GetResourceAsync(document.ResourceId);
            if (resource.Entity == null)
                continue;
            using var content = KntDocuments.OpenContent(service, resource.Entity);
            if (content == null)
                return true;
        }
        return false;
    }
}

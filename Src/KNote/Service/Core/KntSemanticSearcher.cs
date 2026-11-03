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
        var repository = service.Repository.SemanticIndex;
        var contents = new Dictionary<Guid, string>();
        if (matches.Count > 0)
        {
            var resContents = await repository.GetChunkContentsAsync(matches.Select(m => m.NoteChunkId).ToList());
            if (!resContents.IsValid)
            {
                result.AddListErrorMessage(resContents.ListErrorMessage);
                return null;
            }
            contents = resContents.Entity.ToDictionary(c => c.NoteChunkId, c => c.Content);
        }

        var found = new SemanticSearchResultDto();
        foreach (var match in matches)
        {
            var hit = notes[match.NoteId].GetSimpleDto<NoteSearchHitDto>();
            hit.Similarity = match.Similarity;
            hit.Fragment = SemanticSearchRules.Fragment(contents.GetValueOrDefault(match.NoteChunkId));
            hit.FragmentSource = match.SourceType;
            found.Notes.Add(hit);
        }

        var excluded = await KntSystemNotes.GetSystemNoteTypeIdsAsync(service.Repository);
        var resSummary = await repository.GetIndexSummaryAsync(index.Key, excluded);
        if (resSummary.IsValid)
            found.PendingNotes = resSummary.Entity.PendingNotes;
        return found;
    }
}

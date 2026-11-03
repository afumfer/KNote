using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Service.Interfaces;

namespace KNote.Service.ServicesCommands;

// The semantic index of KNote Atlas (see IKntSemanticSearchService and KntSemanticIndexer). Reading its status,
// bringing it up to date before a search and searching are for any user (the index is derived data; the search applies
// the permissions); creating, rebuilding and removing indexes are Admin.

[KntAuthorize(EnumRoles.Guest)]
public class KntSemanticSearchGetIndexStatusAsyncCommand : KntCommandServiceBase<Result<SemanticIndexStatusDto>>
{
    public KntSemanticSearchGetIndexStatusAsyncCommand(IKntService service) : base(service)
    {

    }

    public override async Task<Result<SemanticIndexStatusDto>> Execute()
    {
        var result = new Result<SemanticIndexStatusDto>();

        var resModels = await Service.SystemValues.GetRepositoryAiModelsAsync();
        if (!resModels.IsValid)
        {
            result.AddListErrorMessage(resModels.ListErrorMessage);
            return result;
        }
        var models = resModels.Entity;
        var indexKey = models.GetIndexKey();

        var resIndexes = await Repository.SemanticIndex.GetIndexModelsAsync();
        if (!resIndexes.IsValid)
        {
            result.AddListErrorMessage(resIndexes.ListErrorMessage);
            return result;
        }

        var status = new SemanticIndexStatusDto
        {
            EmbeddingModel = models.EmbeddingModel,
            IndexKey = indexKey,
            EmbeddingModelAvailable = models.EmbeddingModelAvailable,
            OtherIndexes = resIndexes.Entity.Where(m => m.IndexKey != indexKey).ToList()
        };
        result.Entity = status;
        if (indexKey == null)
            return result;

        var excluded = await KntSystemNotes.GetSystemNoteTypeIdsAsync(Repository);
        var resSummary = await Repository.SemanticIndex.GetIndexSummaryAsync(indexKey, excluded);
        if (!resSummary.IsValid)
        {
            result.AddListErrorMessage(resSummary.ListErrorMessage);
            return result;
        }
        status.TotalNotes = resSummary.Entity.TotalNotes;
        status.IndexedNotes = resSummary.Entity.IndexedNotes;
        status.PendingNotes = resSummary.Entity.PendingNotes;
        status.Chunks = resSummary.Entity.Chunks;

        var state = await KntSemanticIndexState.ReadAsync(Service);
        if (state?.IndexKey == indexKey)
        {
            status.Status = state.Status;
            status.Rebuilding = state.RebuildSince != null;
            status.CompletedDateTime = state.CompletedDateTime;
            status.LastBuildSeconds = state.LastBuildSeconds;
            status.NotesPerMinute = state.NotesPerMinute;
        }
        if (status.Status == EnumSemanticIndexStatus.None && status.IndexedNotes > 0)
            status.Status = EnumSemanticIndexStatus.Building;

        return result;
    }
}

[KntAuthorize(EnumRoles.Admin)]
public class KntSemanticSearchRunIndexAsyncCommand : KntCommandServiceBase<SemanticIndexRunRequest, Result<SemanticIndexProgressDto>>
{
    public KntSemanticSearchRunIndexAsyncCommand(IKntService service, SemanticIndexRunRequest request) : base(service, request)
    {

    }

    public override Result ValidateParam()
    {
        var result = new Result();
        if (Param == null)
            result.AddErrorMessage("The run of the semantic index is required.");
        return result;
    }

    public override Task<Result<SemanticIndexProgressDto>> Execute() =>
        KntSemanticIndexer.RunAsync(Service, new KntSemanticIndexer.Options
        {
            Rebuild = Param.Rebuild,
            KeepState = true,
            Progress = Param.Progress,
            CancellationToken = Param.CancellationToken
        });
}

[KntAuthorize(EnumRoles.Guest)]
public class KntSemanticSearchRefreshPendingAsyncCommand : KntCommandServiceBase<SemanticIndexRefreshRequest, Result<SemanticIndexProgressDto>>
{
    public KntSemanticSearchRefreshPendingAsyncCommand(IKntService service, SemanticIndexRefreshRequest request) : base(service, request)
    {

    }

    public override Result ValidateParam()
    {
        var result = new Result();
        if (Param == null || Param.MaxNotes <= 0 || Param.MaxDuration <= TimeSpan.Zero)
            result.AddErrorMessage("The limits of the update of the semantic index are required.");
        return result;
    }

    public override Task<Result<SemanticIndexProgressDto>> Execute() =>
        KntSemanticIndexer.RunAsync(Service, new KntSemanticIndexer.Options
        {
            SkipIfBusy = true,
            MaxNotes = Param.MaxNotes,
            MaxDuration = Param.MaxDuration,
            CancellationToken = Param.CancellationToken
        });
}

[KntAuthorize(EnumRoles.Admin)]
public class KntSemanticSearchDeleteModelIndexAsyncCommand : KntCommandServiceBase<string, Result>
{
    public KntSemanticSearchDeleteModelIndexAsyncCommand(IKntService service, string indexKey) : base(service, indexKey)
    {

    }

    public override Result ValidateParam()
    {
        var result = new Result();
        if (string.IsNullOrWhiteSpace(Param))
            result.AddErrorMessage("The index to remove is required.");
        return result;
    }

    public override async Task<Result> Execute()
    {
        var resModels = await Service.SystemValues.GetRepositoryAiModelsAsync();
        if (!resModels.IsValid)
            return resModels;

        if (string.Equals(resModels.Entity.GetIndexKey(), Param, StringComparison.Ordinal))
        {
            var result = new Result();
            result.AddErrorMessage($"The index {Param} is the one of the repository's embedding model: it is rebuilt, not removed.");
            return result;
        }

        return await Repository.SemanticIndex.DeleteModelAsync(Param);
    }
}

// The search by meaning (D8, D4 and D6 of stage 0): it brings the index up to date (some pending notes, nothing while
// another run goes on), calculates the vector of the text and ranks the notes of the cache of vectors by their best
// fragment. The notes that can be found are the ones the filter of always gives (all, without a filter), read with
// it through the service: the same rules of who sees what as the search by words, and their data for the list. Never
// before the index is created (its first rows come from an Admin's run: a search does not send the whole repository to
// the model).
[KntAuthorize(EnumRoles.Guest)]
public class KntSemanticSearchSearchAsyncCommand : KntCommandServiceBase<SemanticSearchRequestDto, Result<SemanticSearchResultDto>>
{
    private readonly CancellationToken _cancellationToken;

    public KntSemanticSearchSearchAsyncCommand(IKntService service, SemanticSearchRequestDto request, CancellationToken cancellationToken)
        : base(service, request)
    {
        _cancellationToken = cancellationToken;
    }

    public override Result ValidateParam()
    {
        var result = new Result();
        if (string.IsNullOrWhiteSpace(Param?.Text))
            result.AddErrorMessage("The text to search is required.");
        else if (Param.Text.Length > SemanticSearchRequestDto.MaxTextLength)
            result.AddErrorMessage($"The text to search is longer than {SemanticSearchRequestDto.MaxTextLength} characters.");
        return result;
    }

    public override async Task<Result<SemanticSearchResultDto>> Execute()
    {
        var result = new Result<SemanticSearchResultDto>();
        var index = Repository.SemanticIndex;

        var resModels = await Service.SystemValues.GetRepositoryAiModelsAsync();
        if (!resModels.IsValid)
        {
            result.AddListErrorMessage(resModels.ListErrorMessage);
            return result;
        }
        var models = resModels.Entity;
        var indexKey = models.GetIndexKey();
        if (indexKey == null)
        {
            result.AddErrorMessage("The repository has no embedding model (KNote Atlas, repository's AI models).");
            return result;
        }
        var generator = Service.EmbeddingGenerators?.Find(models.EmbeddingModel);
        if (generator == null)
        {
            result.AddErrorMessage($"This installation has no embeddings provider with the model {models.EmbeddingModel} (AI providers).");
            return result;
        }
        var resWatermark = await index.GetIndexWatermarkAsync(indexKey);
        if (!resWatermark.IsValid)
        {
            result.AddListErrorMessage(resWatermark.ListErrorMessage);
            return result;
        }
        if (resWatermark.Entity.Count == 0)
        {
            result.AddErrorMessage("The repository has no semantic index yet (KNote Atlas, semantic index).");
            return result;
        }

        // D8. Its failures (a note, the provider) do not stop the search: the vectors there are serve.
        await KntSemanticIndexer.RunAsync(Service, new KntSemanticIndexer.Options
        {
            SkipIfBusy = true,
            MaxNotes = SemanticIndexRefreshRequest.DefaultMaxNotes,
            MaxDuration = SemanticIndexRefreshRequest.DefaultMaxDuration,
            CancellationToken = _cancellationToken
        });

        float[] query;
        try
        {
            var vectors = await generator.GenerateAsync(new[] { SemanticSearchRules.QueryText(models.QueryInstruction, Param.Text) }, _cancellationToken);
            if (vectors == null || vectors.Count != 1)
                throw new KntServiceException($"The model gave {vectors?.Count ?? 0} vectors for 1 text.");
            query = EmbeddingVector.Normalize(vectors[0], models.EmbeddingDimensions);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            result.AddErrorMessage($"The model {models.EmbeddingModel} could not calculate the vector of the search: {ex.Message}");
            return result;
        }

        var snapshot = await KntSemanticIndexCache.For(Service.RepositoryRef?.ConnectionString ?? "", indexKey)
            .GetAsync(index, indexKey, _cancellationToken);

        var filter = Param.Filter?.GetSimpleDto<NotesFilterDto>() ?? new NotesFilterDto();
        filter.PageIdentifier = new PageIdentifier();
        filter.OrderColumn = null;
        var resNotes = await Service.Notes.GetFilterMinimalAsync(filter);
        if (!resNotes.IsValid)
        {
            result.AddListErrorMessage(resNotes.ListErrorMessage);
            return result;
        }
        var notes = resNotes.Entity.GroupBy(n => n.NoteId).ToDictionary(g => g.Key, g => g.First());

        var matches = SemanticSearchRules.Rank(snapshot.Chunks, query, SemanticSearchRules.MinSimilarity, notes.ContainsKey)
            .Take(SemanticSearchRules.MaxResults)
            .ToList();

        var contents = new Dictionary<Guid, string>();
        if (matches.Count > 0)
        {
            var resContents = await index.GetChunkContentsAsync(matches.Select(m => m.NoteChunkId).ToList());
            if (!resContents.IsValid)
            {
                result.AddListErrorMessage(resContents.ListErrorMessage);
                return result;
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

        var excluded = await KntSystemNotes.GetSystemNoteTypeIdsAsync(Repository);
        var resSummary = await index.GetIndexSummaryAsync(indexKey, excluded);
        if (resSummary.IsValid)
            found.PendingNotes = resSummary.Entity.PendingNotes;

        result.Entity = found;
        return result;
    }
}

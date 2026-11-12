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
            MaxChunksPerNote = KntSemanticIndexer.QuickRefreshMaxChunks,
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

        var index = await KntSemanticSearcher.OpenAsync(Service, result);
        if (index == null)
            return result;

        // D8. Its failures (a note, the provider) do not stop the search: the vectors there are serve.
        await KntSemanticIndexer.RunAsync(Service, new KntSemanticIndexer.Options
        {
            SkipIfBusy = true,
            MaxNotes = SemanticIndexRefreshRequest.DefaultMaxNotes,
            MaxDuration = SemanticIndexRefreshRequest.DefaultMaxDuration,
            MaxChunksPerNote = KntSemanticIndexer.QuickRefreshMaxChunks,
            CancellationToken = _cancellationToken
        });

        float[] query;
        try
        {
            var models = index.Models;
            var vectors = await index.Generator.GenerateAsync(new[] { SemanticSearchRules.QueryText(models.QueryInstruction, Param.Text) }, _cancellationToken);
            if (vectors == null || vectors.Count != 1)
                throw new KntServiceException($"The model gave {vectors?.Count ?? 0} vectors for 1 text.");
            query = EmbeddingVector.Normalize(vectors[0], models.EmbeddingDimensions);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            result.AddErrorMessage($"The model {index.Models.EmbeddingModel} could not calculate the vector of the search: {ex.Message}");
            return result;
        }

        var snapshot = await KntSemanticSearcher.VectorsAsync(Service, index, _cancellationToken);
        var notes = await KntSemanticSearcher.AllowedNotesAsync(Service, Param.Filter, result);
        if (notes == null)
            return result;

        // KNote Atlas, stage 4 (D14): the photos or the documents, by their own fragments.
        if (Param.ResourceKind != EnumResourceKind.All)
        {
            var kinds = await KntSemanticSearcher.ResourceKindsAsync(Service, result);
            if (!result.IsValid)
                return result;
            var resourceMatches = SemanticSearchRules.RankResources(snapshot.Chunks, query,
                    (resourceId, noteId) => notes.ContainsKey(noteId) && kinds.GetValueOrDefault(resourceId) == Param.ResourceKind)
                .Take(SemanticSearchRules.MaxResults)
                .ToList();
            result.Entity = await KntSemanticSearcher.ResourceResultAsync(Service, index, resourceMatches, notes, result);
            return result;
        }

        var matches = SemanticSearchRules.Rank(snapshot.Chunks, query, SemanticSearchRules.MinSimilarity, notes.ContainsKey)
            .Take(SemanticSearchRules.MaxResults)
            .ToList();

        result.Entity = await KntSemanticSearcher.ResultAsync(Service, index, matches, notes, result);
        return result;
    }
}

// The notes similar to a note (delivery 0.5, decisions D1-D4): its fragments compared with the other notes' ones in the
// cache of vectors (SemanticSearchRules.RankSimilar), at most MaxSimilarNotes from MinSimilarNotes. Before, only this
// note is brought up to date if it is pending (not 50 others: the Trace notes tab opens fast). The note itself, the
// notes already related to it (both ways) and the system ones never come out; the notes that can come out are the
// filter's of always, as in the search.
[KntAuthorize(EnumRoles.Guest)]
public class KntSemanticSearchGetSimilarNotesAsyncCommand : KntCommandServiceBase<Guid, Result<SemanticSearchResultDto>>
{
    private readonly CancellationToken _cancellationToken;

    public KntSemanticSearchGetSimilarNotesAsyncCommand(IKntService service, Guid noteId, CancellationToken cancellationToken)
        : base(service, noteId)
    {
        _cancellationToken = cancellationToken;
    }

    public override Result ValidateParam()
    {
        var result = new Result();
        if (Param == Guid.Empty)
            result.AddErrorMessage("The note is required.");
        return result;
    }

    public override async Task<Result<SemanticSearchResultDto>> Execute()
    {
        var result = new Result<SemanticSearchResultDto>();

        var index = await KntSemanticSearcher.OpenAsync(Service, result);
        if (index == null)
            return result;

        var resNote = await Service.Notes.GetAsync(Param);
        if (resNote.Entity == null)
        {
            result.AddErrorMessage("The note does not exist.");
            return result;
        }

        await KntSemanticIndexer.RefreshNoteAsync(Service, Param, _cancellationToken);

        var snapshot = await KntSemanticSearcher.VectorsAsync(Service, index, _cancellationToken);
        if (!snapshot.Notes.TryGetValue(Param, out var noteChunks))
        {
            result.AddErrorMessage("This note is not in the semantic index yet.");
            return result;
        }

        var related = new HashSet<Guid>();
        foreach (var traceNotes in new[] { await Service.Notes.GetTraceNotesFromAsync(Param), await Service.Notes.GetTraceNotesToAsync(Param) })
            foreach (var traceNote in traceNotes.Entity ?? new List<TraceNoteDto>())
                related.Add(traceNote.FromId == Param ? traceNote.ToId : traceNote.FromId);

        var notes = await KntSemanticSearcher.AllowedNotesAsync(Service, null, result);
        if (notes == null)
            return result;

        var matches = SemanticSearchRules.RankSimilar(noteChunks, snapshot.Chunks, SemanticSearchRules.MinSimilarNotes,
                id => notes.ContainsKey(id) && !related.Contains(id))
            .Take(SemanticSearchRules.MaxSimilarNotes)
            .ToList();

        result.Entity = await KntSemanticSearcher.ResultAsync(Service, index, matches, notes, result);
        return result;
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Service.Interfaces;

namespace KNote.Service.ServicesCommands;

// The semantic index of KNote Atlas (see IKntSemanticSearchService and KntSemanticIndexer). Reading its status and
// bringing it up to date before a search are for any user (the index is derived data; the search applies the
// permissions); creating, rebuilding and removing indexes are Admin.

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

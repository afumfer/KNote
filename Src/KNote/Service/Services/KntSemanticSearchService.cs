using System.Threading;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Service.Interfaces;
using KNote.Service.ServicesCommands;

namespace KNote.Service.Services;

public class KntSemanticSearchService : KntServiceBase, IKntSemanticSearchService
{
    #region Constructor

    public KntSemanticSearchService(IKntService service) : base(service)
    {

    }

    #endregion

    #region IKntSemanticSearchService

    public async Task<Result<SemanticIndexStatusDto>> GetIndexStatusAsync()
    {
        var command = new KntSemanticSearchGetIndexStatusAsyncCommand(Service);
        return await ExecuteCommand(command);
    }

    public async Task<Result<SemanticIndexProgressDto>> RunIndexAsync(SemanticIndexRunRequest request)
    {
        var command = new KntSemanticSearchRunIndexAsyncCommand(Service, request);
        return await ExecuteCommand(command);
    }

    public async Task<Result<SemanticIndexProgressDto>> RefreshPendingAsync(SemanticIndexRefreshRequest request)
    {
        var command = new KntSemanticSearchRefreshPendingAsyncCommand(Service, request);
        return await ExecuteCommand(command);
    }

    public async Task<Result> DeleteModelIndexAsync(string indexKey)
    {
        var command = new KntSemanticSearchDeleteModelIndexAsyncCommand(Service, indexKey);
        return await ExecuteCommand(command);
    }

    public async Task<Result<SemanticSearchResultDto>> SearchAsync(SemanticSearchRequestDto request, CancellationToken cancellationToken = default)
    {
        var command = new KntSemanticSearchSearchAsyncCommand(Service, request, cancellationToken);
        return await ExecuteCommand(command);
    }

    #endregion
}

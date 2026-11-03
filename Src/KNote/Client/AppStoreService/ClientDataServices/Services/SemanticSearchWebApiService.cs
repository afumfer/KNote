using System.Net.Http.Json;
using KNote.Client.AppStoreService.ClientDataServices.Base;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

public class SemanticSearchWebApiService : BaseService, ISemanticSearchWebApiService
{
    public SemanticSearchWebApiService(AppState appState, HttpClient httpClient) : base(appState, httpClient)
    {

    }

    public Task<Result<RepositoryAiModelsDto>> GetModelsAsync(CancellationToken cancellationToken = default) =>
        SendAsync<RepositoryAiModelsDto>(ct => _httpClient.GetAsync("api/semantic/models", ct), "Get repository models", cancellationToken: cancellationToken);

    public Task<Result<RepositoryAiModelsDto>> SaveModelsAsync(RepositoryAiModelsDto models) =>
        SendAsync<RepositoryAiModelsDto>(ct => _httpClient.PostAsJsonAsync("api/semantic/models", models, ct), "Save repository models", true);

    public Task<Result<AiInstallationModelsDto>> GetInstallationModelsAsync(CancellationToken cancellationToken = default) =>
        SendAsync<AiInstallationModelsDto>(ct => _httpClient.GetAsync("api/semantic/installation-models", ct), "Get server models", cancellationToken: cancellationToken);

    public Task<Result<SemanticIndexStatusDto>> GetIndexStatusAsync(CancellationToken cancellationToken = default) =>
        SendAsync<SemanticIndexStatusDto>(ct => _httpClient.GetAsync("api/semantic/status", ct), "Get semantic index status", cancellationToken: cancellationToken);

    public Task<Result<SemanticIndexProgressDto>> RunIndexAsync(bool rebuild) =>
        SendAsync<SemanticIndexProgressDto>(ct => _httpClient.PostAsJsonAsync("api/semantic/index", new SemanticIndexRunDto { Rebuild = rebuild }, ct),
            "Semantic index");

    public Task<Result<SemanticIndexProgressDto>> GetIndexProgressAsync(CancellationToken cancellationToken = default) =>
        SendAsync<SemanticIndexProgressDto>(ct => _httpClient.GetAsync("api/semantic/index/progress", ct), "Semantic index progress",
            cancellationToken: cancellationToken, notifyErrors: false);

    public Task<Result<bool>> CancelIndexAsync() =>
        SendAsync<bool>(ct => _httpClient.PostAsync("api/semantic/index/cancel", null, ct), "Cancel the semantic index");

    public Task<Result<bool>> DeleteModelIndexAsync(string indexKey) =>
        SendAsync<bool>(ct => _httpClient.DeleteAsync($"api/semantic/index?key={Uri.EscapeDataString(indexKey)}", ct), "Remove a semantic index", true);

    public Task<Result<SemanticSearchResultDto>> SearchAsync(SemanticSearchRequestDto request, CancellationToken cancellationToken = default) =>
        SendAsync<SemanticSearchResultDto>(ct => _httpClient.PostAsJsonAsync("api/semantic/search", request, ct), "Search by meaning",
            cancellationToken: cancellationToken);
}

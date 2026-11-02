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
}

using System.Net.Http.Json;
using KNote.Client.AppStoreService.ClientDataServices.Base;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

public class LocalWebApiService : BaseService, ILocalWebApiService
{
    public LocalWebApiService(AppState appState, HttpClient httpClient) : base(appState, httpClient)
    {

    }

    public Task<Result<List<LocalRepositoryDto>>> GetRepositoriesAsync(CancellationToken cancellationToken = default) =>
        SendAsync<List<LocalRepositoryDto>>(ct => _httpClient.GetAsync("api/local/repositories", ct), "Get repositories", cancellationToken: cancellationToken);

    public Task<Result<List<LocalRepositoryDto>>> AddRepositoryAsync(LocalRepositoryAddDto repository) =>
        SendAsync<List<LocalRepositoryDto>>(ct => _httpClient.PostAsJsonAsync("api/local/repositories", repository, ct), "Add repository", true);

    public Task<Result<List<LocalRepositoryDto>>> SetDefaultRepositoryAsync(int index) =>
        SendAsync<List<LocalRepositoryDto>>(ct => _httpClient.PutAsync($"api/local/repositories/{index}/default", null, ct), "Default repository", true);

    public Task<Result<List<LocalRepositoryDto>>> RenameRepositoryAsync(int index, string name) =>
        SendAsync<List<LocalRepositoryDto>>(ct => _httpClient.PutAsJsonAsync($"api/local/repositories/{index}/name", name, ct), "Rename repository", true);

    public Task<Result<List<LocalRepositoryDto>>> UnlinkRepositoryAsync(int index) =>
        SendAsync<List<LocalRepositoryDto>>(ct => _httpClient.DeleteAsync($"api/local/repositories/{index}", ct), "Unlink repository", true);

    public Task<Result<List<LocalAiProviderDto>>> GetAiProvidersAsync(CancellationToken cancellationToken = default) =>
        SendAsync<List<LocalAiProviderDto>>(ct => _httpClient.GetAsync("api/local/ai", ct), "Get AI providers", cancellationToken: cancellationToken);

    public Task<Result<List<LocalAiProviderDto>>> SaveAiProvidersAsync(List<LocalAiProviderDto> providers) =>
        SendAsync<List<LocalAiProviderDto>>(ct => _httpClient.PutAsJsonAsync("api/local/ai", providers, ct), "Save AI providers", true);

    public Task<Result<LocalEmailSettingsDto>> GetEmailAsync(CancellationToken cancellationToken = default) =>
        SendAsync<LocalEmailSettingsDto>(ct => _httpClient.GetAsync("api/local/email", ct), "Get e-mail settings", cancellationToken: cancellationToken);

    public Task<Result<LocalEmailSettingsDto>> SaveEmailAsync(LocalEmailSettingsDto email) =>
        SendAsync<LocalEmailSettingsDto>(ct => _httpClient.PutAsJsonAsync("api/local/email", email, ct), "Save e-mail settings", true);

    public Task<Result<LocalServerSettingsDto>> GetServerAsync(CancellationToken cancellationToken = default) =>
        SendAsync<LocalServerSettingsDto>(ct => _httpClient.GetAsync("api/local/server", ct), "Get server settings", cancellationToken: cancellationToken);

    public Task<Result<LocalServerSettingsDto>> SavePortAsync(int port) =>
        SendAsync<LocalServerSettingsDto>(ct => _httpClient.PutAsJsonAsync("api/local/server/port", port, ct), "Save port", true);

    public Task<Result<bool>> StopAsync() =>
        SendAsync<bool>(ct => _httpClient.PostAsync("api/local/stop", null, ct), "Stop local server");
}

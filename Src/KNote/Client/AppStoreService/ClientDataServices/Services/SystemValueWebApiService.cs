using KNote.Client.AppStoreService.ClientDataServices.Base;
using KNote.Model;

namespace KNote.Client.AppStoreService.ClientDataServices;

public class SystemValueWebApiService : BaseService, ISystemValueWebApiService
{
    public SystemValueWebApiService(AppState appState, HttpClient httpClient) : base(appState, httpClient)
    {

    }

    public Task<Result<string>> GetRepositoryNameAsync(CancellationToken cancellationToken = default) =>
        SendAsync<string>(ct => _httpClient.GetAsync($"api/systemvalues/repositoryname", ct), "Get repository name",
            cancellationToken: cancellationToken);
}

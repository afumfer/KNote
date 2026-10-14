using KNote.Client.AppStoreService.ClientDataServices.Base;
using KNote.Model;

namespace KNote.Client.AppStoreService.ClientDataServices;

public class SystemValueWebApiService : BaseService, ISystemValueWebApiService
{
    public SystemValueWebApiService(AppState appState, HttpClient httpClient) : base(appState, httpClient)
    {

    }

    public async Task<Result<string>> GetRepositoryNameAsync()
    {
        var httpRes = await _httpClient.GetAsync($"api/systemvalues/repositoryname");
        return await ProcessResultFromHttpResponse<string>(httpRes, "Get repository name");
    }
}

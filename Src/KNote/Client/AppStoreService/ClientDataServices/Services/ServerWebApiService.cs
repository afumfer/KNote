using KNote.Client.AppStoreService.ClientDataServices.Base;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

public class ServerWebApiService : BaseService, IServerWebApiService
{
    public ServerWebApiService(AppState appState, HttpClient httpClient) : base(appState, httpClient)
    {

    }

    public Task<Result<ServerCapabilitiesDto>> GetCapabilitiesAsync(CancellationToken cancellationToken = default) =>
        SendAsync<ServerCapabilitiesDto>(ct => _httpClient.GetAsync("api/server/capabilities", ct), "Server capabilities",
            cancellationToken: cancellationToken, notifyErrors: false);
}

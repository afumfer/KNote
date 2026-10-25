using System.Net.Http.Json;
using KNote.Client.AppStoreService.ClientDataServices.Base;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

public class AlarmWebApiService : BaseService, IAlarmWebApiService
{
    public AlarmWebApiService(AppState appState, HttpClient httpClient) : base(appState, httpClient)
    {

    }

    public Task<Result<AlarmCheckDto>> CheckAsync(CancellationToken cancellationToken = default) =>
        SendAsync<AlarmCheckDto>(ct => _httpClient.PostAsync("api/alarms/check", null, ct), "Check alarms",
            cancellationToken: cancellationToken, notifyErrors: false);

    public Task<Result<List<AlarmNoticeDto>>> GetNoticesAsync(List<Guid> messageIds, CancellationToken cancellationToken = default) =>
        SendAsync<List<AlarmNoticeDto>>(ct => _httpClient.PostAsJsonAsync("api/alarms/notices", messageIds, ct), "Alarm notices",
            cancellationToken: cancellationToken, notifyErrors: false);
}

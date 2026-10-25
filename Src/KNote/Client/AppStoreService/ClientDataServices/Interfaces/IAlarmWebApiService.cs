using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

// The Application info alarms of the user (api/alarms), for the alarms panel (AlarmNotices). Asked by the app by
// itself, so their errors are not notified.
public interface IAlarmWebApiService
{
    // The user's alarms that have just become due (each one is given once), and when to check again.
    Task<Result<AlarmCheckDto>> CheckAsync(CancellationToken cancellationToken = default);

    // The current data of the notices kept, leaving out those that no longer are the user's notices.
    Task<Result<List<AlarmNoticeDto>>> GetNoticesAsync(List<Guid> messageIds, CancellationToken cancellationToken = default);
}

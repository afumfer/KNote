using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

// What the server can do (api/server), to show or hide parts of the app. Its errors are not notified: without an answer,
// it can do nothing extra.
public interface IServerWebApiService
{
    Task<Result<ServerCapabilitiesDto>> GetCapabilitiesAsync(CancellationToken cancellationToken = default);
}

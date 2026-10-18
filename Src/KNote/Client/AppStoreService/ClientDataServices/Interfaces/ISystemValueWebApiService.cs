using KNote.Model;

namespace KNote.Client.AppStoreService.ClientDataServices;

public interface ISystemValueWebApiService
{
    // The name of the repository (DB_REPOSITORYNAME), readable without signing in.
    // cancellationToken: the caller no longer wants the answer; see BaseService.SendAsync.
    Task<Result<string>> GetRepositoryNameAsync(CancellationToken cancellationToken = default);
}

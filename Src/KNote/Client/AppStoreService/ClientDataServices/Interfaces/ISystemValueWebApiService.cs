using KNote.Model;

namespace KNote.Client.AppStoreService.ClientDataServices;

public interface ISystemValueWebApiService
{
    // The name of the repository (DB_REPOSITORYNAME), readable without signing in.
    Task<Result<string>> GetRepositoryNameAsync();
}

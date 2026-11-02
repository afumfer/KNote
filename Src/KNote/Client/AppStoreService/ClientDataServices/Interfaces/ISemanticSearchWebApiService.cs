using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

// KNote Atlas (api/semantic): the AI models of the session's repository and, in the next deliveries, its semantic index
// and the search by meaning.
public interface ISemanticSearchWebApiService
{
    // The repository's models, with whether the server has its embedding model (any user).
    Task<Result<RepositoryAiModelsDto>> GetModelsAsync(CancellationToken cancellationToken = default);
    // Admin.
    Task<Result<RepositoryAiModelsDto>> SaveModelsAsync(RepositoryAiModelsDto models);
    // The embedding and chat models of the server's AI providers, to choose the repository's ones (Admin).
    Task<Result<AiInstallationModelsDto>> GetInstallationModelsAsync(CancellationToken cancellationToken = default);
}

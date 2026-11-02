using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

// KNote Atlas (api/semantic): the AI models of the session's repository, its semantic index and, in the next deliveries,
// the search by meaning.
public interface ISemanticSearchWebApiService
{
    // The repository's models, with whether the server has its embedding model (any user).
    Task<Result<RepositoryAiModelsDto>> GetModelsAsync(CancellationToken cancellationToken = default);
    // Admin.
    Task<Result<RepositoryAiModelsDto>> SaveModelsAsync(RepositoryAiModelsDto models);
    // The embedding and chat models of the server's AI providers, to choose the repository's ones (Admin).
    Task<Result<AiInstallationModelsDto>> GetInstallationModelsAsync(CancellationToken cancellationToken = default);
    // How the repository's semantic index is (any user).
    Task<Result<SemanticIndexStatusDto>> GetIndexStatusAsync(CancellationToken cancellationToken = default);
    // Creates, updates or rebuilds the index in the server's queue: the progress of its run (Admin).
    Task<Result<SemanticIndexProgressDto>> RunIndexAsync(bool rebuild);
    // The progress of the repository's run, or none; errors not notified, it is asked every second (Admin).
    Task<Result<SemanticIndexProgressDto>> GetIndexProgressAsync(CancellationToken cancellationToken = default);
    Task<Result<bool>> CancelIndexAsync();
    // Removes the index of another model (Admin).
    Task<Result<bool>> DeleteModelIndexAsync(string indexKey);
}

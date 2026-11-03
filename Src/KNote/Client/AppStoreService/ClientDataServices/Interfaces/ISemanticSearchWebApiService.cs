using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

// KNote Atlas (api/semantic): the AI models of the session's repository, its semantic index and the search by meaning.
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
    // The notes most similar to a text, among all of them or the ones of a filter (any user): not paged.
    Task<Result<SemanticSearchResultDto>> SearchAsync(SemanticSearchRequestDto request, CancellationToken cancellationToken = default);
    // The notes most similar to a note, not related to it yet (any user). Errors not notified: the list shows them.
    Task<Result<SemanticSearchResultDto>> GetSimilarNotesAsync(Guid noteId, CancellationToken cancellationToken = default);
    // The search evaluation (ProjectManager): its test queries and indexes, one query evaluated, the report saved.
    Task<Result<SearchEvaluationSetupDto>> GetSearchEvaluationAsync(CancellationToken cancellationToken = default);
    Task<Result<SearchEvaluationQueryResultDto>> EvaluateQueryAsync(SearchEvaluationQueryRequestDto request, CancellationToken cancellationToken = default);
    Task<Result<NoteDto>> SaveSearchEvaluationReportAsync(SearchEvaluationReportDto report);
}

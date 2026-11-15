using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

/// <summary>The summaries of KNote Atlas (stage 2, api/summaries): for now, of a long document of a note (CU-43), and its translations.</summary>
public interface ISummaryWebApiService
{
    Task<Result<DocumentProcessEstimateDto>> EstimateDocumentAsync(DocumentProcessRequestDto request, CancellationToken cancellationToken = default);
    Task<Result<DocumentProcessProgressDto>> SummarizeDocumentAsync(DocumentProcessRequestDto request, CancellationToken cancellationToken = default);
    // The user's summary of a document in the repository of the session (null, none). Without notifying errors: it is asked every second.
    Task<Result<DocumentProcessProgressDto>> GetDocumentProgressAsync(CancellationToken cancellationToken = default);
    Task<Result<bool>> CancelDocumentAsync(CancellationToken cancellationToken = default);
    // The translations of a document, the last one first (2.1b2: «Continue» goes on with one not finished).
    Task<Result<List<DocumentTranslationDto>>> GetDocumentTranslationsAsync(Guid resourceId, CancellationToken cancellationToken = default);
}

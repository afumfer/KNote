using System.Net.Http.Json;
using KNote.Client.AppStoreService.ClientDataServices.Base;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

public class SummaryWebApiService : BaseService, ISummaryWebApiService
{
    public SummaryWebApiService(AppState appState, HttpClient httpClient) : base(appState, httpClient)
    {

    }

    public Task<Result<DocumentProcessEstimateDto>> EstimateDocumentAsync(DocumentProcessRequestDto request, CancellationToken cancellationToken = default) =>
        SendAsync<DocumentProcessEstimateDto>(ct => _httpClient.PostAsJsonAsync("api/summaries/documents/estimate", request, ct),
            "Estimate the summary", cancellationToken: cancellationToken);

    public Task<Result<DocumentProcessProgressDto>> SummarizeDocumentAsync(DocumentProcessRequestDto request, CancellationToken cancellationToken = default) =>
        SendAsync<DocumentProcessProgressDto>(ct => _httpClient.PostAsJsonAsync("api/summaries/documents", request, ct),
            "Summarize document", cancellationToken: cancellationToken);

    public Task<Result<DocumentProcessProgressDto>> GetDocumentProgressAsync(CancellationToken cancellationToken = default) =>
        SendAsync<DocumentProcessProgressDto>(ct => _httpClient.GetAsync("api/summaries/documents/progress", ct),
            "Progress of the summary", cancellationToken: cancellationToken, notifyErrors: false);

    public Task<Result<bool>> CancelDocumentAsync(CancellationToken cancellationToken = default) =>
        SendAsync<bool>(ct => _httpClient.PostAsync("api/summaries/documents/cancel", null, ct),
            "Cancel the summary", cancellationToken: cancellationToken);
}

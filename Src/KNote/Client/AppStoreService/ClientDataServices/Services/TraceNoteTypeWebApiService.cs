using System.Net.Http.Json;
using KNote.Client.AppStoreService.ClientDataServices.Base;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

public class TraceNoteTypeWebApiService : BaseService, ITraceNoteTypeWebApiService
{
    public TraceNoteTypeWebApiService(AppState appState, HttpClient httpClient) : base(appState, httpClient)
    {

    }

    public Task<Result<List<TraceNoteTypeDto>>> GetAllAsync(CancellationToken cancellationToken = default) =>
        SendAsync<List<TraceNoteTypeDto>>(ct => _httpClient.GetAsync($"api/tracenotetypes", ct), "Get trace note types",
            cancellationToken: cancellationToken);

    public Task<Result<TraceNoteTypeDto>> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        SendAsync<TraceNoteTypeDto>(ct => _httpClient.GetAsync($"api/tracenotetypes/{id}", ct), "Get trace note type",
            cancellationToken: cancellationToken);

    public Task<Result<TraceNoteTypeDto>> SaveAsync(TraceNoteTypeDto traceNoteType, CancellationToken cancellationToken = default) =>
        SendAsync<TraceNoteTypeDto>(ct => traceNoteType.TraceNoteTypeId == Guid.Empty
                ? _httpClient.PostAsJsonAsync($"api/tracenotetypes", traceNoteType, ct)
                : _httpClient.PutAsJsonAsync($"api/tracenotetypes", traceNoteType, ct),
            "Save trace note type", true, cancellationToken);

    public Task<Result<TraceNoteTypeDto>> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        SendAsync<TraceNoteTypeDto>(ct => _httpClient.DeleteAsync($"api/tracenotetypes/{id}", ct), "Delete trace note type", true,
            cancellationToken);
}

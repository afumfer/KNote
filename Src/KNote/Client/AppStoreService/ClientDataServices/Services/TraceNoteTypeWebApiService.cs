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

    public async Task<Result<List<TraceNoteTypeDto>>> GetAllAsync()
    {
        var httpRes = await _httpClient.GetAsync($"api/tracenotetypes");
        return await ProcessResultFromHttpResponse<List<TraceNoteTypeDto>>(httpRes, "Get trace note types");
    }

    public async Task<Result<TraceNoteTypeDto>> GetAsync(Guid id)
    {
        var httpRes = await _httpClient.GetAsync($"api/tracenotetypes/{id}");
        return await ProcessResultFromHttpResponse<TraceNoteTypeDto>(httpRes, "Get trace note type");
    }

    public async Task<Result<TraceNoteTypeDto>> SaveAsync(TraceNoteTypeDto traceNoteType)
    {
        HttpResponseMessage httpRes;

        if (traceNoteType.TraceNoteTypeId == Guid.Empty)
            httpRes = await _httpClient.PostAsJsonAsync($"api/tracenotetypes", traceNoteType);
        else
            httpRes = await _httpClient.PutAsJsonAsync($"api/tracenotetypes", traceNoteType);

        return await ProcessResultFromHttpResponse<TraceNoteTypeDto>(httpRes, "Save trace note type", true);
    }

    public async Task<Result<TraceNoteTypeDto>> DeleteAsync(Guid id)
    {
        var httpRes = await _httpClient.DeleteAsync($"api/tracenotetypes/{id}");
        return await ProcessResultFromHttpResponse<TraceNoteTypeDto>(httpRes, "Delete trace note type", true);
    }
}

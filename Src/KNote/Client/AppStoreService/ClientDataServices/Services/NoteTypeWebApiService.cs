using System.Net.Http.Json;
using KNote.Client.AppStoreService.ClientDataServices.Base;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

public class NoteTypeWebApiService : BaseService, INoteTypeWebApiService
{
    public NoteTypeWebApiService(AppState appState, HttpClient httpClient) : base(appState, httpClient)
    {

    }

    public async Task<Result<List<NoteTypeDto>>> GetAllAsync()
    {        
        var httpRes = await _httpClient.GetAsync($"api/notetypes");
        return await ProcessResultFromHttpResponse<List<NoteTypeDto>>(httpRes, "Get note types");
    }

    public async Task<Result<NoteTypeDto>> GetAsync(Guid id)
    {        
        var httpRes = await _httpClient.GetAsync($"api/notetypes/{id}");
        return await ProcessResultFromHttpResponse<NoteTypeDto>(httpRes, "Get note type");
    }

    public async Task<Result<NoteTypeDto>> SaveAsync(NoteTypeDto noteType)
    {
        HttpResponseMessage httpRes;

        if (noteType.NoteTypeId == Guid.Empty)
            httpRes = await _httpClient.PostAsJsonAsync($"api/notetypes", noteType);
        else
            httpRes = await _httpClient.PutAsJsonAsync($"api/notetypes", noteType);

        return await ProcessResultFromHttpResponse<NoteTypeDto>(httpRes, "Save note type", true);
    }

    public async Task<Result<NoteTypeDto>> DeleteAsync(Guid id)
    {
        var httpRes = await _httpClient.DeleteAsync($"api/notetypes/{id}");
        return await ProcessResultFromHttpResponse<NoteTypeDto>(httpRes, "Delete note type", true);
    }
}


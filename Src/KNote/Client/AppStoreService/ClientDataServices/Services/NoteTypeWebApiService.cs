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
        return await httpClient.GetFromJsonAsync<Result<List<NoteTypeDto>>>($"api/notetypes");
    }

    public async Task<Result<NoteTypeDto>> GetAsync(Guid id)
    {
        return await httpClient.GetFromJsonAsync<Result<NoteTypeDto>>($"api/notetypes/{id}");
    }

    public async Task<Result<NoteTypeDto>> SaveAsync(NoteTypeDto noteType)
    {
        HttpResponseMessage httpRes;

        if (noteType.NoteTypeId == Guid.Empty)
            httpRes = await httpClient.PostAsJsonAsync($"api/notetypes", noteType);
        else
            httpRes = await httpClient.PutAsJsonAsync($"api/notetypes", noteType);

        var res = await httpRes.Content.ReadFromJsonAsync<Result<NoteTypeDto>>();

        return res;
    }

    public async Task<Result<NoteTypeDto>> DeleteAsync(Guid id)
    {
        var httpRes = await httpClient.DeleteAsync($"api/notetypes/{id}");

        var res = await httpRes.Content.ReadFromJsonAsync<Result<NoteTypeDto>>();

        return res;
    }
}


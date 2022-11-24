using System.Net.Http.Json;
using KNote.Client.AppStoreService.ClientDataServices.Base;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

public class FolderWebApiService : BaseService, IFolderWebApiService
{
    public FolderWebApiService(AppState appState, HttpClient httpClient) : base(appState, httpClient)
    {

    }

    public async Task<Result<List<FolderInfoDto>>> GetAllAsync()
    {
        return await _httpClient.GetFromJsonAsync<Result<List<FolderInfoDto>>>("api/folders");
    }

    public async Task<Result<List<FolderDto>>> GetTreeAsync()
    {        
        return await _httpClient.GetFromJsonAsync<Result<List<FolderDto>>>("api/folders/gettree");
    }

    public async Task<Result<FolderDto>> GetAsync(Guid folderId)
    {
        return await _httpClient.GetFromJsonAsync<Result<FolderDto>>($"api/folders/{folderId}");
    }

    public async Task<Result<FolderDto>> SaveAsync(FolderDto folder)
    {
        HttpResponseMessage httpRes;

        if (folder.FolderId == Guid.Empty)
            httpRes = await _httpClient.PostAsJsonAsync($"api/folders", folder);
        else
            httpRes = await _httpClient.PutAsJsonAsync($"api/folders", folder);

        var res = await httpRes.Content.ReadFromJsonAsync<Result<FolderDto>>();

        return res;
    }

    public async Task<Result<FolderDto>> DeleteAsync(Guid id)
    {
        var httpRes = await _httpClient.DeleteAsync($"api/folders/{id}");

        var res = await httpRes.Content.ReadFromJsonAsync<Result<FolderDto>>();

        return res;
    }

    public async Task<Result<List<NoteInfoDto>>> GetNotes(Guid folderId)
    {
        return await _httpClient.GetFromJsonAsync<Result<List<NoteInfoDto>>>($"api/folders/{folderId}/getnotes");
    }
}


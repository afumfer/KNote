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
        //return await httpClient.GetFromJsonAsync<Result<List<FolderInfoDto>>>("api/folders");
        var httpRes = await httpClient.GetAsync("api/folders");
        return await ProcessResultFromHttpResponse<List<FolderInfoDto>>(httpRes, "Get folders");

    }

    public async Task<Result<List<FolderDto>>> GetTreeAsync()
    {        
        //return await httpClient.GetFromJsonAsync<Result<List<FolderDto>>>("api/folders/gettree");
        var httpRes = await httpClient.GetAsync("api/folders/tree");
        return await ProcessResultFromHttpResponse<List<FolderDto>>(httpRes, "Get folders");

    }

    public async Task<Result<FolderDto>> GetAsync(Guid folderId)
    {
        //return await httpClient.GetFromJsonAsync<Result<FolderDto>>($"api/folders/{folderId}");
        var httpRes = await httpClient.GetAsync($"api/folders/{folderId}");
        return await ProcessResultFromHttpResponse<FolderDto>(httpRes, "Get folder");
    }

    public async Task<Result<FolderDto>> SaveAsync(FolderDto folder)
    {
        HttpResponseMessage httpRes;

        if (folder.FolderId == Guid.Empty)
            httpRes = await httpClient.PostAsJsonAsync($"api/folders", folder);
        else
            httpRes = await httpClient.PutAsJsonAsync($"api/folders", folder);

        //var res = await httpRes.Content.ReadFromJsonAsync<Result<FolderDto>>();
        //return res;
        
        return await ProcessResultFromHttpResponse<FolderDto>(httpRes, "Save folder", true);
    }

    public async Task<Result<FolderDto>> DeleteAsync(Guid id)
    {
        var httpRes = await httpClient.DeleteAsync($"api/folders/{id}");

        //var res = await httpRes.Content.ReadFromJsonAsync<Result<FolderDto>>();
        //return res;
        return await ProcessResultFromHttpResponse<FolderDto>(httpRes, "Delete folder", true);
    }

    //public async Task<Result<List<NoteInfoDto>>> GetNotes(Guid folderId)
    //{
    //    return await httpClient.GetFromJsonAsync<Result<List<NoteInfoDto>>>($"api/folders/{folderId}/getnotes");
    //}
}


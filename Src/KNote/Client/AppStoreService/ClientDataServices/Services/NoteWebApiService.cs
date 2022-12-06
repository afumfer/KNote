using System.Net.Http.Json;
using KNote.Client.AppStoreService.ClientDataServices.Base;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

public class NoteWebApiService : BaseService, INoteWebApiService
{

    public NoteWebApiService(AppState appState, HttpClient httpClient) : base(appState, httpClient)
    {

    }

    public async Task<Result<List<NoteInfoDto>>> GetHomeNotesAsync()
    {
        //return await httpClient.GetFromJsonAsync<Result<List<NoteInfoDto>>>("api/notes/homenotes");

        var httpRes = await httpClient.GetAsync("api/notes/homenotes");        
        return await ProcessResultFromHttpResponse<List<NoteInfoDto>>(httpRes, "Get home notes");
    }

    public async Task<Result<NoteDto>> GetAsync(Guid noteId)
    {
        //return await httpClient.GetFromJsonAsync<Result<NoteDto>>($"api/notes/{noteId}");

        var httpRes = await httpClient.GetAsync($"api/notes/{noteId}");
        return await ProcessResultFromHttpResponse<NoteDto>(httpRes, "Get note");

    }

    public async Task<Result<NoteDto>> NewAsync(NoteInfoDto entity = null)
    {
        //return await httpClient.GetFromJsonAsync<Result<NoteDto>>($"api/notes/new");

        var httpRes = await httpClient.GetAsync($"api/notes/new");
        return await ProcessResultFromHttpResponse<NoteDto>(httpRes, "New note");
    }

    public async Task<Result<NoteDto>> SaveAsync(NoteDto note, bool updateStatus = true)
    {
        HttpResponseMessage httpRes;

        if (note.NoteId == Guid.Empty)
            httpRes = await httpClient.PostAsJsonAsync($"api/notes", note);
        else
            httpRes = await httpClient.PutAsJsonAsync($"api/notes", note);

        return await ProcessResultFromHttpResponse<NoteDto>(httpRes, "Save note", true);

    }

    public async Task<Result<NoteDto>> DeleteAsync(Guid noteId)
    {
        //var httpRes = await httpClient.DeleteAsync($"api/notes/{noteId}");
        //var res = await httpRes.Content.ReadFromJsonAsync<Result<NoteDto>>();
        //return res;

        var httpRes = await httpClient.DeleteAsync($"api/notes/{noteId}");
        return await ProcessResultFromHttpResponse<NoteDto>(httpRes, "Delete note", true);
    }

    public async Task<Result<List<ResourceInfoDto>>> GetResourcesAsync(Guid noteId)
    {
        //return await httpClient.GetFromJsonAsync<Result<List<ResourceInfoDto>>>($"api/notes/{noteId}/resources");
        var httpRes = await httpClient.GetAsync($"api/notes/{noteId}/resources");
        return await ProcessResultFromHttpResponse<List<ResourceInfoDto>>(httpRes, "New note");

    }

    public async Task<Result<ResourceInfoDto>> SaveResourceAsync(ResourceInfoDto resource)
    {
        HttpResponseMessage httpRes;

        if (resource.ResourceId == Guid.Empty)
            httpRes = await httpClient.PostAsJsonAsync($"api/notes/resources", resource);
        else
            httpRes = await httpClient.PutAsJsonAsync($"api/notes/resources", resource);

        //var res = await httpRes.Content.ReadFromJsonAsync<Result<ResourceInfoDto>>();
        //return res;

        return await ProcessResultFromHttpResponse<ResourceInfoDto>(httpRes, "Save resouce", true);
    }

    public async Task<Result<ResourceInfoDto>> DeleteResourceAsync(Guid resourceId)
    {
        //var httpRes = await httpClient.DeleteAsync($"api/notes/deleteresource/{resourceId}");
        //var res = await httpRes.Content.ReadFromJsonAsync<Result<ResourceInfoDto>>();
        //return res;

        var httpRes = await httpClient.DeleteAsync($"api/notes/resources/{resourceId}");
        return await ProcessResultFromHttpResponse<ResourceInfoDto>(httpRes, "Delete resource", true);
    }

    public async Task<Result<List<NoteTaskDto>>> GetNoteTasksAsync(Guid noteId)
    {
        //return await httpClient.GetFromJsonAsync<Result<List<NoteTaskDto>>>($"api/notes/{noteId}/GetNoteTasks");

        var httpRes = await httpClient.GetAsync($"api/notes/{noteId}/tasks");
        return await ProcessResultFromHttpResponse<List<NoteTaskDto>>(httpRes, "Get note tasks");
    }

    public async Task<Result<List<NoteTaskDto>>> GetStartedTasksByDateTimeAsync(DateTime startDateTime, DateTime endDateTime)
    {
        var url = $"api/notes/GetStartedTasksByDateTimeRage?start={startDateTime.ToString("yyyyMMddHHmmss")}&end={endDateTime.ToString("yyyyMMddHHmmss")}";
        //return await httpClient.GetFromJsonAsync<Result<List<NoteTaskDto>>>(url);

        var httpRes = await httpClient.GetAsync(url);
        return await ProcessResultFromHttpResponse<List<NoteTaskDto>>(httpRes, "Get note tasks by datetime.");
    }

    public async Task<Result<List<NoteTaskDto>>> GetEstimatedTasksByDateTimeAsync(DateTime startDateTime, DateTime endDateTime)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<NoteTaskDto>> SaveNoteTaskAsync(NoteTaskDto noteTask)
    {
        HttpResponseMessage httpRes;

        if (noteTask.NoteTaskId == Guid.Empty)
            httpRes = await httpClient.PostAsJsonAsync($"api/notes/tasks", noteTask);
        else
            httpRes = await httpClient.PutAsJsonAsync($"api/notes/tasks", noteTask);

        //var res = await httpRes.Content.ReadFromJsonAsync<Result<NoteTaskDto>>();
        //return res;
        return await ProcessResultFromHttpResponse<NoteTaskDto>(httpRes, "Save note task", true);
    }

    public async Task<Result<NoteTaskDto>> DeleteNoteTaskAsync(Guid noteTaskId)
    {
        var httpRes = await httpClient.DeleteAsync($"api/notes/tasks/{noteTaskId}");

        //var res = await httpRes.Content.ReadFromJsonAsync<Result<NoteTaskDto>>();
        //return res;
        return await ProcessResultFromHttpResponse<NoteTaskDto>(httpRes, "Delete note task", true);
    }

    public async Task<Result<List<NoteInfoDto>>> GetSearch(string queryString)
    {
        //var res = await httpClient.GetFromJsonAsync<Result<List<NoteInfoDto>>>($"api/notes/getsearch?{queryString}");
        //return res;
        var httpRes = await httpClient.GetAsync($"api/notes/search?{queryString}");
        return await ProcessResultFromHttpResponse<List<NoteInfoDto>>(httpRes, "Get notes from search string");

    }
    
    public async Task<Result<List<NoteInfoDto>>> GetFilter(NotesFilterDto notesFilter)
    {
        //var res = new Result<List<NoteInfoDto>>();

        //try
        //{
        //    HttpResponseMessage httpRes;
        //    httpRes = await httpClient.PostAsJsonAsync($"api/notes/getfilter", notesFilter);
        //    if (httpRes.IsSuccessStatusCode)
        //        res = await httpRes.Content.ReadFromJsonAsync<Result<List<NoteInfoDto>>>();
        //    else
        //        res.AddErrorMessage($"Error. The web server has responded with the following message: {httpRes.ReasonPhrase}");
        //    return res;
        //}
        //catch (Exception ex)
        //{
        //    res.AddErrorMessage(ex.Message);
        //    return res;
        //}

        var httpRes = await httpClient.PostAsJsonAsync($"api/notes/filter", notesFilter);
        return await ProcessResultFromHttpResponse<List<NoteInfoDto>>(httpRes, "Get notes from filter");
    }

}


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
        return await httpClient.GetFromJsonAsync<Result<List<NoteInfoDto>>>("api/notes/homenotes");
    }

    public async Task<Result<NoteDto>> GetAsync(Guid noteId)
    {
        return await httpClient.GetFromJsonAsync<Result<NoteDto>>($"api/notes/{noteId}");
    }

    public async Task<Result<NoteDto>> NewAsync(NoteInfoDto entity = null)
    {
        return await httpClient.GetFromJsonAsync<Result<NoteDto>>($"api/notes/new");
    }

    public async Task<Result<NoteDto>> SaveAsync(NoteDto note, bool updateStatus = true)
    {
        HttpResponseMessage httpRes;

        if (note.NoteId == Guid.Empty)
            httpRes = await httpClient.PostAsJsonAsync($"api/notes", note);
        else
            httpRes = await httpClient.PutAsJsonAsync($"api/notes", note);

        var res = await httpRes.Content.ReadFromJsonAsync<Result<NoteDto>>();

        return res;
    }

    public async Task<Result<NoteDto>> DeleteAsync(Guid noteId)
    {
        var httpRes = await httpClient.DeleteAsync($"api/notes/{noteId}");

        var res = await httpRes.Content.ReadFromJsonAsync<Result<NoteDto>>();

        return res;
    }

    public async Task<Result<List<ResourceInfoDto>>> GetResourcesAsync(Guid noteId)
    {
        return await httpClient.GetFromJsonAsync<Result<List<ResourceInfoDto>>>($"api/notes/{noteId}/getresources");
    }

    public async Task<Result<ResourceInfoDto>> SaveResourceAsync(ResourceInfoDto resource)
    {
        HttpResponseMessage httpRes;

        if (resource.ResourceId == Guid.Empty)
            httpRes = await httpClient.PostAsJsonAsync($"api/notes/saveresource", resource);
        else
            httpRes = await httpClient.PutAsJsonAsync($"api/notes/saveresource", resource);

        var res = await httpRes.Content.ReadFromJsonAsync<Result<ResourceInfoDto>>();

        return res;
    }

    public async Task<Result<ResourceInfoDto>> DeleteResourceAsync(Guid resourceId)
    {
        var httpRes = await httpClient.DeleteAsync($"api/notes/deleteresource/{resourceId}");
        var res = await httpRes.Content.ReadFromJsonAsync<Result<ResourceInfoDto>>();
        return res;
    }

    public async Task<Result<List<NoteTaskDto>>> GetNoteTasksAsync(Guid noteId)
    {
        return await httpClient.GetFromJsonAsync<Result<List<NoteTaskDto>>>($"api/notes/{noteId}/GetNoteTasks");
    }

    public async Task<Result<List<NoteTaskDto>>> GetStartedTasksByDateTimeAsync(DateTime startDateTime, DateTime endDateTime)
    {
        var url = $"api/notes/GetStartedTasksByDateTimeRage?start={startDateTime.ToString("yyyyMMddHHmmss")}&end={endDateTime.ToString("yyyyMMddHHmmss")}";
        return await httpClient.GetFromJsonAsync<Result<List<NoteTaskDto>>>(url);
    }

    public async Task<Result<List<NoteTaskDto>>> GetEstimatedTasksByDateTimeAsync(DateTime startDateTime, DateTime endDateTime)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<NoteTaskDto>> SaveNoteTaskAsync(NoteTaskDto noteTask)
    {
        HttpResponseMessage httpRes;

        if (noteTask.NoteTaskId == Guid.Empty)
            httpRes = await httpClient.PostAsJsonAsync($"api/notes/savenotetask", noteTask);
        else
            httpRes = await httpClient.PutAsJsonAsync($"api/notes/savenotetask", noteTask);

        var res = await httpRes.Content.ReadFromJsonAsync<Result<NoteTaskDto>>();

        return res;
    }

    public async Task<Result<NoteTaskDto>> DeleteNoteTaskAsync(Guid noteTaskId)
    {
        var httpRes = await httpClient.DeleteAsync($"api/notes/deletenotetask/{noteTaskId}");
        var res = await httpRes.Content.ReadFromJsonAsync<Result<NoteTaskDto>>();
        return res;
    }

    public async Task<Result<List<NoteInfoDto>>> GetSearch(string queryString)
    {
        return await httpClient.GetFromJsonAsync<Result<List<NoteInfoDto>>>($"api/notes/getsearch?{queryString}");
    }

    // TODO: !!! Apply the following strategy to the rest of the ClientDataService methods. (Command pattern ?) 
    public async Task<Result<List<NoteInfoDto>>> GetFilter(NotesFilterDto notesFilter)
    {
        var res = new Result<List<NoteInfoDto>>();

        try
        {
            HttpResponseMessage httpRes;
            httpRes = await httpClient.PostAsJsonAsync($"api/notes/getfilter", notesFilter);
            if (httpRes.IsSuccessStatusCode)
                res = await httpRes.Content.ReadFromJsonAsync<Result<List<NoteInfoDto>>>();
            else
                res.AddErrorMessage($"Error. The web server has responded with the following message: {httpRes.ReasonPhrase.ToString()}");
            return res;
        }
        catch (Exception ex)
        {
            res.AddErrorMessage(ex.Message);
            return res;
        }
    }

}


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

    public Task<Result<List<NoteInfoDto>>> GetHomeNotesAsync(Guid? folderId = null, CancellationToken cancellationToken = default) =>
        SendAsync<List<NoteInfoDto>>(ct => _httpClient.GetAsync(folderId == null ? "api/notes/homenotes" : $"api/notes/homenotes?folderId={folderId}", ct),
            "Get home notes", cancellationToken: cancellationToken);

    public Task<Result<List<NoteMinimalDto>>> GetSearch(string queryString, CancellationToken cancellationToken = default) =>
        SendAsync<List<NoteMinimalDto>>(ct => _httpClient.GetAsync($"api/notes/search?{queryString}", ct),
            "Get notes from search string", cancellationToken: cancellationToken);

    public Task<Result<List<NoteMinimalDto>>> GetFilter(NotesFilterDto notesFilter, CancellationToken cancellationToken = default) =>
        SendAsync<List<NoteMinimalDto>>(ct => _httpClient.PostAsJsonAsync($"api/notes/filter", notesFilter, ct),
            "Get notes from filter", cancellationToken: cancellationToken);

    public Task<Result<NoteDto>> GetAsync(Guid noteId, CancellationToken cancellationToken = default) =>
        SendAsync<NoteDto>(ct => _httpClient.GetAsync($"api/notes/{noteId}", ct), "Get note", cancellationToken: cancellationToken);

    // The HTML of a note's description made safe to show (by Server, the same rules as ClientWin).
    public Task<Result<string>> HtmlAsync(string markdown, CancellationToken cancellationToken = default) =>
        SendAsync<string>(ct => _httpClient.PostAsJsonAsync("api/notes/html", markdown, ct), "Show note", cancellationToken: cancellationToken);

    public Task<Result<NoteDto>> NewAsync(CancellationToken cancellationToken = default) =>
        SendAsync<NoteDto>(ct => _httpClient.GetAsync($"api/notes/new", ct), "New note", cancellationToken: cancellationToken);

    public Task<Result<NoteDto>> SaveAsync(NoteDto note, bool updateStatus = true, CancellationToken cancellationToken = default) =>
        SendAsync<NoteDto>(ct => note.NoteId == Guid.Empty
                ? _httpClient.PostAsJsonAsync($"api/notes", note, ct)
                : _httpClient.PutAsJsonAsync($"api/notes", note, ct),
            "Save note", true, cancellationToken);

    public Task<Result<NoteDto>> DeleteAsync(Guid noteId, CancellationToken cancellationToken = default) =>
        SendAsync<NoteDto>(ct => _httpClient.DeleteAsync($"api/notes/{noteId}", ct), "Delete note", true, cancellationToken);

    public Task<Result<List<ResourceInfoDto>>> GetResourcesAsync(Guid noteId, CancellationToken cancellationToken = default) =>
        SendAsync<List<ResourceInfoDto>>(ct => _httpClient.GetAsync($"api/notes/{noteId}/resources", ct),
            "Get note resources", cancellationToken: cancellationToken);

    public Task<Result<ResourceInfoDto>> SaveResourceAsync(ResourceInfoDto resource, CancellationToken cancellationToken = default) =>
        SendAsync<ResourceInfoDto>(ct => resource.ResourceId == Guid.Empty
                ? _httpClient.PostAsJsonAsync($"api/notes/resources", resource, ct)
                : _httpClient.PutAsJsonAsync($"api/notes/resources", resource, ct),
            "Save resouce", true, cancellationToken);

    public Task<Result<ResourceInfoDto>> DeleteResourceAsync(Guid resourceId, CancellationToken cancellationToken = default) =>
        SendAsync<ResourceInfoDto>(ct => _httpClient.DeleteAsync($"api/notes/resources/{resourceId}", ct),
            "Delete resource", true, cancellationToken);

    public Task<Result<List<NoteTaskDto>>> GetNoteTasksAsync(Guid noteId, CancellationToken cancellationToken = default) =>
        SendAsync<List<NoteTaskDto>>(ct => _httpClient.GetAsync($"api/notes/{noteId}/tasks", ct),
            "Get note tasks", cancellationToken: cancellationToken);

    public Task<Result<List<NoteTaskDto>>> GetStartedTasksByDateTimeAsync(DateTime startDateTime, DateTime endDateTime,
        CancellationToken cancellationToken = default)
    {
        var url = $"api/notes/GetStartedTasksByDateTimeRage?start={startDateTime.ToString("yyyyMMddHHmmss")}&end={endDateTime.ToString("yyyyMMddHHmmss")}";
        return SendAsync<List<NoteTaskDto>>(ct => _httpClient.GetAsync(url, ct), "Get note tasks by datetime.", cancellationToken: cancellationToken);
    }

    public Task<Result<List<NoteTaskDto>>> GetEstimatedTasksByDateTimeAsync(DateTime startDateTime, DateTime endDateTime,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<NoteTaskDto>> SaveNoteTaskAsync(NoteTaskDto noteTask, CancellationToken cancellationToken = default) =>
        SendAsync<NoteTaskDto>(ct => noteTask.NoteTaskId == Guid.Empty
                ? _httpClient.PostAsJsonAsync($"api/notes/tasks", noteTask, ct)
                : _httpClient.PutAsJsonAsync($"api/notes/tasks", noteTask, ct),
            "Save note task", true, cancellationToken);

    public Task<Result<NoteTaskDto>> DeleteNoteTaskAsync(Guid noteTaskId, CancellationToken cancellationToken = default) =>
        SendAsync<NoteTaskDto>(ct => _httpClient.DeleteAsync($"api/notes/tasks/{noteTaskId}", ct),
            "Delete note task", true, cancellationToken);

    public Task<Result<NoteTraceNotesDto>> GetTraceNotesAsync(Guid noteId, CancellationToken cancellationToken = default) =>
        SendAsync<NoteTraceNotesDto>(ct => _httpClient.GetAsync($"api/notes/{noteId}/tracenotes", ct),
            "Get trace notes", cancellationToken: cancellationToken);

    public Task<Result<TraceNoteDto>> SaveTraceNoteAsync(TraceNoteDto traceNote, CancellationToken cancellationToken = default) =>
        SendAsync<TraceNoteDto>(ct => traceNote.TraceNoteId == Guid.Empty
                ? _httpClient.PostAsJsonAsync($"api/notes/tracenotes", traceNote, ct)
                : _httpClient.PutAsJsonAsync($"api/notes/tracenotes", traceNote, ct),
            "Save trace note", true, cancellationToken);

    public Task<Result<TraceNoteDto>> DeleteTraceNoteAsync(Guid traceNoteId, CancellationToken cancellationToken = default) =>
        SendAsync<TraceNoteDto>(ct => _httpClient.DeleteAsync($"api/notes/tracenotes/{traceNoteId}", ct),
            "Delete trace note", true, cancellationToken);
}

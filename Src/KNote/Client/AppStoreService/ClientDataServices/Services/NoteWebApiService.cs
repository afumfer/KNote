using System.Net;
using System.Net.Http.Json;
using System.Text;
using KNote.Client.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
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
            "Get home notes", cancellationToken: cancellationToken, timeout: ListReadTimeout);

    public Task<Result<List<NoteMinimalDto>>> GetSearch(string queryString, CancellationToken cancellationToken = default) =>
        SendAsync<List<NoteMinimalDto>>(ct => _httpClient.GetAsync($"api/notes/search?{queryString}", ct),
            "Get notes from search string", cancellationToken: cancellationToken, timeout: ListReadTimeout);

    public Task<Result<List<NoteMinimalDto>>> GetFilter(NotesFilterDto notesFilter, CancellationToken cancellationToken = default) =>
        SendAsync<List<NoteMinimalDto>>(ct => _httpClient.PostAsJsonAsync($"api/notes/filter", notesFilter, ct),
            "Get notes from filter", cancellationToken: cancellationToken, timeout: ListReadTimeout);

    public Task<Result<NoteDto>> GetAsync(Guid noteId, CancellationToken cancellationToken = default) =>
        SendAsync<NoteDto>(ct => _httpClient.GetAsync($"api/notes/{noteId}", ct), "Get note", cancellationToken: cancellationToken);

    public Task<Result<NoteMinimalDto>> GetCitedAsync(int noteNumber, CancellationToken cancellationToken = default) =>
        SendAsync<NoteMinimalDto>(ct => _httpClient.GetAsync($"api/notes/cited/{noteNumber}", ct), "Get note", cancellationToken: cancellationToken);

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

    public Task<Result<ResourceDescriberDto>> GetPhotoDescriberAsync(CancellationToken cancellationToken = default) =>
        SendAsync<ResourceDescriberDto>(ct => _httpClient.GetAsync("api/resources/describer", ct),
            "Describe photos", cancellationToken: cancellationToken);

    public async Task<Result<ResourceInfoDto>> UploadResourceAsync(IJSRuntime js, ElementReference input, ResourceInfoDto resource,
        IProgress<(long Sent, long Total)>? progress = null, CancellationToken cancellationToken = default)
    {
        const string action = "Upload resource";
        var module = await js.ImportModuleAsync("./js/kntUpload.js", _appState);
        if (module == null)
        {
            var notLoaded = new Result<ResourceInfoDto>();
            notLoaded.AddErrorMessage("The upload could not start: part of the page could not be loaded.");
            return notLoaded;
        }

        await using (module)
        {
            using var reporter = DotNetObjectReference.Create(new UploadProgress(progress));
            var url = new Uri(_httpClient.BaseAddress!, $"api/notes/{resource.NoteId}/resources/upload").ToString();
            var fields = new Dictionary<string, string?>
            {
                ["description"] = resource.Description,
                ["loadedDescription"] = resource.LoadedDescription,
                ["descriptionByUser"] = resource.DescriptionByUser ? "true" : "false",
                ["order"] = resource.Order.ToString(System.Globalization.CultureInfo.InvariantCulture)
            };

            UploadAnswer answer;
            using (cancellationToken.Register(() => _ = module.InvokeVoidAsync("cancel").AsTask()))
                answer = await module.InvokeAsync<UploadAnswer>("upload", input, url,
                    _httpClient.DefaultRequestHeaders.Authorization?.Parameter, fields, reporter);

            if (answer.Canceled || cancellationToken.IsCancellationRequested)
                return CancelledResult<ResourceInfoDto>();
            if (answer.Status == 0)
            {
                var res = new Result<ResourceInfoDto>();
                res.AddErrorMessage($"Error. {answer.Error ?? "The web server could not be reached."}");
                _appState.NotifyError(action, res.ErrorMessage);
                return res;
            }
            if (answer.Status == (int)HttpStatusCode.RequestEntityTooLarge)
            {
                // Refused by the web server before KNote (IIS without the limit of its web.config, a proxy).
                var res = new Result<ResourceInfoDto>();
                res.AddErrorMessage($"The web server refused the file for its size (KNote admits {ResourceFileRules.MaxSizeText}).");
                _appState.NotifyError(action, res.ErrorMessage);
                return res;
            }

            using var response = new HttpResponseMessage((HttpStatusCode)answer.Status)
            {
                Content = new StringContent(answer.Body ?? "", Encoding.UTF8, "application/json")
            };
            return await ProcessResultFromHttpResponse<ResourceInfoDto>(response, action);
        }
    }

    public Task<Result<DocumentReadDto>> ReadDocumentAsync(Guid resourceId, CancellationToken cancellationToken = default) =>
        SendAsync<DocumentReadDto>(ct => _httpClient.PostAsync($"api/resources/{resourceId}/read", null, ct),
            DocumentTexts.ReadDocument, cancellationToken: cancellationToken);

    public Task<Result<ResourceTextDto>> GetDocumentTextAsync(Guid resourceId, CancellationToken cancellationToken = default) =>
        SendAsync<ResourceTextDto>(ct => _httpClient.GetAsync($"api/resources/{resourceId}/text?maxLength={DocumentTextRules.MaxViewerLength}", ct),
            "Text of the document", cancellationToken: cancellationToken);

    // What js/kntUpload.js answers.
    private sealed record UploadAnswer(int Status, string? Body, string? Error, bool Canceled);

    // The progress of an upload, from js/kntUpload.js.
    private sealed class UploadProgress(IProgress<(long Sent, long Total)>? progress)
    {
        [JSInvokable]
        public void OnUploadProgress(long sent, long total) => progress?.Report((sent, total));
    }

    // Its errors are not notified one by one: «Describe photos» gives a summary at the end.
    public Task<Result<ResourceInfoDto>> DescribePhotoAsync(Guid resourceId, bool replace, CancellationToken cancellationToken = default) =>
        SendAsync<ResourceInfoDto>(ct => _httpClient.PostAsync($"api/resources/{resourceId}/describe?replace={(replace ? "true" : "false")}", null, ct),
            "Describe photo", cancellationToken: cancellationToken, notifyErrors: false);

    // As DescribePhotoAsync: a summary at the end of «Describe with AI».
    public Task<Result<ResourceInfoDto>> SummarizeDocumentAsync(Guid resourceId, bool replace, CancellationToken cancellationToken = default) =>
        SendAsync<ResourceInfoDto>(ct => _httpClient.PostAsync($"api/resources/{resourceId}/summarize?replace={(replace ? "true" : "false")}", null, ct),
            "Summarize document", cancellationToken: cancellationToken, notifyErrors: false);

    public Task<Result<ScannedDocumentDto>> GetScannedPagesAsync(Guid resourceId, CancellationToken cancellationToken = default) =>
        SendAsync<ScannedDocumentDto>(ct => _httpClient.GetAsync($"api/resources/{resourceId}/scanned", ct),
            DocumentTexts.TranscribePages, cancellationToken: cancellationToken);

    public Task<Result<DocumentReadDto>> TranscribePageAsync(Guid resourceId, int page, CancellationToken cancellationToken = default) =>
        SendAsync<DocumentReadDto>(ct => _httpClient.PostAsync($"api/resources/{resourceId}/transcribe?page={page}", null, ct),
            DocumentTexts.TranscribePages, cancellationToken: cancellationToken, notifyErrors: false);

    public Task<Result<ResourceProcessingStatusDto>> GetResourceProcessingStatusAsync(CancellationToken cancellationToken = default) =>
        SendAsync<ResourceProcessingStatusDto>(ct => _httpClient.GetAsync("api/resources/status", ct),
            "Pending resources", cancellationToken: cancellationToken, notifyErrors: false);

    public Task<Result<ResourceProcessingProgressDto>> ProcessResourcesAsync(ResourceProcessingOptionsDto options) =>
        SendAsync<ResourceProcessingProgressDto>(ct => _httpClient.PostAsJsonAsync("api/resources/process", options, ct),
            "Process pending resources");

    public Task<Result<ResourceProcessingProgressDto>> GetResourceProcessingProgressAsync(CancellationToken cancellationToken = default) =>
        SendAsync<ResourceProcessingProgressDto>(ct => _httpClient.GetAsync("api/resources/process/progress", ct),
            "Process pending resources progress", cancellationToken: cancellationToken, notifyErrors: false);

    public Task<Result<bool>> CancelResourceProcessingAsync() =>
        SendAsync<bool>(ct => _httpClient.PostAsync("api/resources/process/cancel", null, ct), "Cancel process pending resources");

    public Task<Result<List<NoteTaskDto>>> GetNoteTasksAsync(Guid noteId, CancellationToken cancellationToken = default) =>
        SendAsync<List<NoteTaskDto>>(ct => _httpClient.GetAsync($"api/notes/{noteId}/tasks", ct),
            "Get note tasks", cancellationToken: cancellationToken);

    public Task<Result<TasksPanelDto>> GetTasksPanelAsync(TasksPanelFilterDto filter, CancellationToken cancellationToken = default) =>
        SendAsync<TasksPanelDto>(ct => _httpClient.PostAsJsonAsync("api/notes/tasks/panel", filter, ct),
            "Get tasks panel", cancellationToken: cancellationToken, timeout: ListReadTimeout);

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

    public Task<Result<List<KMessageDto>>> GetAlarmsAsync(Guid noteId, CancellationToken cancellationToken = default) =>
        SendAsync<List<KMessageDto>>(ct => _httpClient.GetAsync($"api/notes/{noteId}/alarms", ct),
            "Get note alarms", cancellationToken: cancellationToken);

    public Task<Result<KMessageDto>> NewAlarmAsync(Guid noteId, CancellationToken cancellationToken = default) =>
        SendAsync<KMessageDto>(ct => _httpClient.GetAsync($"api/notes/{noteId}/alarms/new", ct),
            "New alarm", cancellationToken: cancellationToken);

    public Task<Result<KMessageDto>> SaveAlarmAsync(KMessageDto alarm, CancellationToken cancellationToken = default) =>
        SendAsync<KMessageDto>(ct => alarm.KMessageId == Guid.Empty
                ? _httpClient.PostAsJsonAsync("api/notes/alarms", alarm, ct)
                : _httpClient.PutAsJsonAsync("api/notes/alarms", alarm, ct),
            "Save alarm", true, cancellationToken);

    public Task<Result<KMessageDto>> DeleteAlarmAsync(Guid alarmId, CancellationToken cancellationToken = default) =>
        SendAsync<KMessageDto>(ct => _httpClient.DeleteAsync($"api/notes/alarms/{alarmId}", ct),
            "Delete alarm", true, cancellationToken);

    public Task<Result<AlarmsCalendarDto>> GetAlarmsCalendarAsync(AlarmsCalendarFilterDto filter, CancellationToken cancellationToken = default) =>
        SendAsync<AlarmsCalendarDto>(ct => _httpClient.PostAsJsonAsync("api/notes/alarms/calendar", filter, ct),
            "Get alarms calendar", cancellationToken: cancellationToken, timeout: ListReadTimeout);
}

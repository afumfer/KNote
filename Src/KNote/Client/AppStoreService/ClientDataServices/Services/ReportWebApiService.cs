using System.Globalization;
using System.Net.Http.Json;
using KNote.Client.AppStoreService.ClientDataServices.Base;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

public class ReportWebApiService : BaseService, IReportWebApiService
{
    public ReportWebApiService(AppState appState, HttpClient httpClient) : base(appState, httpClient)
    {

    }

    public async Task<Result<ReportDto>> GetNotesListAsync(NotesListReportRequestDto request)
    {
        var httpRes = await _httpClient.PostAsJsonAsync("api/reports/noteslist", WithCulture(request));
        return await ProcessResultFromHttpResponse<ReportDto>(httpRes, "Print notes list");
    }

    // It can take long (one chapter per note): cancelled when the user closes its preview.
    public async Task<Result<ReportDto>> GetNotesBookAsync(NotesListReportRequestDto request, CancellationToken cancellationToken = default)
    {
        var httpRes = await _httpClient.PostAsJsonAsync("api/reports/notesbook", WithCulture(request), cancellationToken);
        return await ProcessResultFromHttpResponse<ReportDto>(httpRes, "Print notes list as book");
    }

    public async Task<Result<ReportFileDto>> GetNotesListCsvAsync(NotesListReportRequestDto request)
    {
        var httpRes = await _httpClient.PostAsJsonAsync("api/reports/noteslistcsv", WithCulture(request));
        return await ProcessResultFromHttpResponse<ReportFileDto>(httpRes, "Export notes list to CSV");
    }

    public async Task<Result<ReportDto>> GetNoteDetailAsync(Guid noteId)
    {
        var httpRes = await _httpClient.GetAsync($"api/reports/notes/{noteId}?culture={Uri.EscapeDataString(CultureInfo.CurrentCulture.Name)}");
        return await ProcessResultFromHttpResponse<ReportDto>(httpRes, "Print note details");
    }

    // The browser's culture (Blazor WebAssembly starts with it).
    private static NotesListReportRequestDto WithCulture(NotesListReportRequestDto request)
    {
        request.Culture ??= CultureInfo.CurrentCulture.Name;
        return request;
    }
}

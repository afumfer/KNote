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

    public Task<Result<ReportDto>> GetNotesListAsync(NotesListReportRequestDto request, CancellationToken cancellationToken = default) =>
        SendAsync<ReportDto>(ct => _httpClient.PostAsJsonAsync("api/reports/noteslist", WithCulture(request), ct), "Print notes list",
            cancellationToken: cancellationToken);

    // It can take long (one chapter per note): cancelled when the user closes its preview.
    public Task<Result<ReportDto>> GetNotesBookAsync(NotesListReportRequestDto request, CancellationToken cancellationToken = default) =>
        SendAsync<ReportDto>(ct => _httpClient.PostAsJsonAsync("api/reports/notesbook", WithCulture(request), ct), "Print notes list as book",
            cancellationToken: cancellationToken);

    public Task<Result<ReportFileDto>> GetNotesListCsvAsync(NotesListReportRequestDto request, CancellationToken cancellationToken = default) =>
        SendAsync<ReportFileDto>(ct => _httpClient.PostAsJsonAsync("api/reports/noteslistcsv", WithCulture(request), ct), "Export notes list to CSV",
            cancellationToken: cancellationToken);

    public Task<Result<ReportDto>> GetNoteDetailAsync(Guid noteId, CancellationToken cancellationToken = default) =>
        SendAsync<ReportDto>(ct => _httpClient.GetAsync($"api/reports/notes/{noteId}?culture={Uri.EscapeDataString(CultureInfo.CurrentCulture.Name)}", ct),
            "Print note details", cancellationToken: cancellationToken);

    // The browser's culture (Blazor WebAssembly starts with it).
    private static NotesListReportRequestDto WithCulture(NotesListReportRequestDto request)
    {
        request.Culture ??= CultureInfo.CurrentCulture.Name;
        return request;
    }
}

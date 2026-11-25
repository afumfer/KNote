using KNote.Client.AppStoreService.ClientDataServices.Base;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

public class HistoryWebApiService : BaseService, IHistoryWebApiService
{
    public HistoryWebApiService(AppState appState, HttpClient httpClient) : base(appState, httpClient)
    {

    }

    public Task<Result<NoteHistoryPageDto>> GetNoteHistoryAsync(Guid noteId, int skip = 0, CancellationToken cancellationToken = default) =>
        SendAsync<NoteHistoryPageDto>(ct => _httpClient.GetAsync($"api/history/notes/{noteId}?skip={skip}", ct), "Get the history of the note",
            cancellationToken: cancellationToken);

    public Task<Result<NoteVersionViewDto>> GetNoteVersionAsync(Guid noteId, Guid klogId, CancellationToken cancellationToken = default) =>
        SendAsync<NoteVersionViewDto>(ct => _httpClient.GetAsync($"api/history/notes/{noteId}/versions/{klogId}", ct), "Get a version of the note",
            cancellationToken: cancellationToken);
}

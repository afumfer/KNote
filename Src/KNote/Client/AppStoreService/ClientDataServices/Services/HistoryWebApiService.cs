using System.Net.Http.Json;
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

    public Task<Result<NoteComparisonDto>> GetNoteComparisonAsync(Guid noteId, Guid klogId, bool withCurrentNote, CancellationToken cancellationToken = default) =>
        SendAsync<NoteComparisonDto>(ct => _httpClient.GetAsync(
                $"api/history/notes/{noteId}/compare/{klogId}?withCurrentNote={(withCurrentNote ? "true" : "false")}", ct),
            "Compare the versions of the note", cancellationToken: cancellationToken);

    public Task<Result<List<DeletedNoteDto>>> GetDeletedNotesAsync(CancellationToken cancellationToken = default) =>
        SendAsync<List<DeletedNoteDto>>(ct => _httpClient.GetAsync("api/history/deleted", ct), "Get the deleted notes",
            cancellationToken: cancellationToken);

    public Task<Result<NoteRecoverResultDto>> RecoverNoteAsync(Guid noteId) =>
        SendAsync<NoteRecoverResultDto>(ct => _httpClient.PostAsync($"api/history/deleted/{noteId}/recover", null, ct), "Recover the deleted note");

    // The server answers a Result without an entity.
    public async Task<Result> RemoveDeletedNoteAsync(Guid noteId) =>
        await SendAsync<bool>(ct => _httpClient.DeleteAsync($"api/history/deleted/{noteId}", ct), "Remove the deleted note permanently");

    public Task<Result<HistoryRetentionInfoDto>> GetRetentionAsync(CancellationToken cancellationToken = default) =>
        SendAsync<HistoryRetentionInfoDto>(ct => _httpClient.GetAsync("api/history/retention", ct), "Get the retention of the history",
            cancellationToken: cancellationToken);

    public Task<Result<HistoryRetentionDto>> SaveRetentionAsync(HistoryRetentionDto retention) =>
        SendAsync<HistoryRetentionDto>(ct => _httpClient.PutAsJsonAsync("api/history/retention", retention, ct), "Save the retention of the history", true);

    // Not canceled with the window: a restore that started finishes.
    public Task<Result<NoteRestoreResultDto>> RestoreNoteVersionAsync(Guid noteId, Guid klogId) =>
        SendAsync<NoteRestoreResultDto>(ct => _httpClient.PostAsync($"api/history/notes/{noteId}/versions/{klogId}/restore", null, ct),
            "Restore a version of the note");
}

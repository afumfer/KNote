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

    public Task<Result<List<FolderInfoDto>>> GetAllAsync(CancellationToken cancellationToken = default) =>
        SendAsync<List<FolderInfoDto>>(ct => _httpClient.GetAsync("api/folders", ct), "Get folders", cancellationToken: cancellationToken);

    public Task<Result<List<FolderDto>>> GetTreeAsync(CancellationToken cancellationToken = default) =>
        SendAsync<List<FolderDto>>(ct => _httpClient.GetAsync("api/folders/tree", ct), "Get folders",
            cancellationToken: cancellationToken, timeout: ListReadTimeout);

    public Task<Result<List<FolderInfoDto>>> GetHomeChildrenAsync(CancellationToken cancellationToken = default) =>
        SendAsync<List<FolderInfoDto>>(ct => _httpClient.GetAsync("api/folders/homechildren", ct), "Get Home folders",
            cancellationToken: cancellationToken, timeout: ListReadTimeout);

    public Task<Result<FolderDto>> GetAsync(Guid folderId, CancellationToken cancellationToken = default) =>
        SendAsync<FolderDto>(ct => _httpClient.GetAsync($"api/folders/{folderId}", ct), "Get folder", cancellationToken: cancellationToken);

    // The folder's path in the folders tree, from the root (Service: Folders.GetPathAsync, as ClientWin shows it).
    public Task<Result<string>> GetPathAsync(Guid folderId, CancellationToken cancellationToken = default) =>
        SendAsync<string>(ct => _httpClient.GetAsync($"api/folders/{folderId}/path", ct), "Get folder path", cancellationToken: cancellationToken);

    public Task<Result<FolderDto>> SaveAsync(FolderDto folder, CancellationToken cancellationToken = default) =>
        SendAsync<FolderDto>(ct => folder.FolderId == Guid.Empty
                ? _httpClient.PostAsJsonAsync($"api/folders", folder, ct)
                : _httpClient.PutAsJsonAsync($"api/folders", folder, ct),
            "Save folder", true, cancellationToken);

    public Task<Result<FolderDto>> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        SendAsync<FolderDto>(ct => _httpClient.DeleteAsync($"api/folders/{id}", ct), "Delete folder", true, cancellationToken);

    // The order of the notes the folder remembers (Folder.OrderNotes, in NoteOrderCriteria's format).
    public async Task<Result> UpdateOrderNotesAsync(Guid folderId, string orderNotes, CancellationToken cancellationToken = default) =>
        await SendAsync<object>(ct => _httpClient.PutAsJsonAsync($"api/folders/{folderId}/ordernotes", orderNotes, ct),
            "Save order of notes", cancellationToken: cancellationToken);
}

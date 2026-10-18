using System.Net.Http.Json;
using KNote.Client.AppStoreService.ClientDataServices.Base;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

public class NoteTypeWebApiService : BaseService, INoteTypeWebApiService
{
    public NoteTypeWebApiService(AppState appState, HttpClient httpClient) : base(appState, httpClient)
    {

    }

    public Task<Result<List<NoteTypeDto>>> GetAllAsync(CancellationToken cancellationToken = default) =>
        SendAsync<List<NoteTypeDto>>(ct => _httpClient.GetAsync($"api/notetypes", ct), "Get note types", cancellationToken: cancellationToken);

    public Task<Result<NoteTypeDto>> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        SendAsync<NoteTypeDto>(ct => _httpClient.GetAsync($"api/notetypes/{id}", ct), "Get note type", cancellationToken: cancellationToken);

    public Task<Result<NoteTypeDto>> SaveAsync(NoteTypeDto noteType, CancellationToken cancellationToken = default) =>
        SendAsync<NoteTypeDto>(ct => noteType.NoteTypeId == Guid.Empty
                ? _httpClient.PostAsJsonAsync($"api/notetypes", noteType, ct)
                : _httpClient.PutAsJsonAsync($"api/notetypes", noteType, ct),
            "Save note type", true, cancellationToken);

    public Task<Result<NoteTypeDto>> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        SendAsync<NoteTypeDto>(ct => _httpClient.DeleteAsync($"api/notetypes/{id}", ct), "Delete note type", true, cancellationToken);
}

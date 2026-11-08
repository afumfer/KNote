using System.Net.Http.Json;
using KNote.Client.AppStoreService.ClientDataServices.Base;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

public class AtlasWebApiService : BaseService, IAtlasWebApiService
{
    public AtlasWebApiService(AppState appState, HttpClient httpClient) : base(appState, httpClient)
    {

    }

    public Task<Result<AtlasProfileInfoDto>> GetProfileAsync(CancellationToken cancellationToken = default) =>
        SendAsync<AtlasProfileInfoDto>(ct => _httpClient.GetAsync("api/atlas/profile", ct), "Repository profile", cancellationToken: cancellationToken);

    public Task<Result<NoteDto>> CreateProfileAsync() =>
        SendAsync<NoteDto>(ct => _httpClient.PostAsync("api/atlas/profile", null, ct), "Create the profile", true);

    public Task<Result<NoteDto>> FillProfileAsync() =>
        SendAsync<NoteDto>(ct => _httpClient.PostAsync("api/atlas/profile/fill", null, ct), "Fill in folders and types", true);

    public Task<Result<AtlasGlossaryInfoDto>> GetGlossaryAsync(CancellationToken cancellationToken = default) =>
        SendAsync<AtlasGlossaryInfoDto>(ct => _httpClient.GetAsync("api/atlas/glossary", ct), "Glossary", cancellationToken: cancellationToken);

    public Task<Result<NoteDto>> CreateGlossaryAsync() =>
        SendAsync<NoteDto>(ct => _httpClient.PostAsync("api/atlas/glossary", null, ct), "Create the glossary", true);

    public Task<Result<List<AtlasTaskPromptStatusDto>>> GetTaskPromptsAsync(CancellationToken cancellationToken = default) =>
        SendAsync<List<AtlasTaskPromptStatusDto>>(ct => _httpClient.GetAsync("api/atlas/tasks", ct), "Task instructions", cancellationToken: cancellationToken);

    public Task<Result<NoteDto>> CustomizeTaskAsync(string taskKey) =>
        SendAsync<NoteDto>(ct => _httpClient.PostAsync($"api/atlas/tasks/{Uri.EscapeDataString(taskKey)}/customize", null, ct), "Customize the task");

    public Task<Result<AiComposedPromptDto>> ComposeAsync(AiComposeRequestDto request, CancellationToken cancellationToken = default) =>
        SendAsync<AiComposedPromptDto>(ct => _httpClient.PostAsJsonAsync("api/atlas/compose", request, ct), "What the AI receives",
            cancellationToken: cancellationToken);
}

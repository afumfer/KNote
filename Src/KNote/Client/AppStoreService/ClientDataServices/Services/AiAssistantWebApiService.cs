using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using KNote.Client.AppStoreService.ClientDataServices.Base;
using KNote.Model;
using KNote.Model.Dto;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace KNote.Client.AppStoreService.ClientDataServices;

public class AiAssistantWebApiService : BaseService, IAiAssistantWebApiService
{
    private const string ChatAction = "AI assistant";

    public AiAssistantWebApiService(AppState appState, HttpClient httpClient) : base(appState, httpClient)
    {

    }

    public async Task<Result<List<AiProviderInfoDto>>> GetProvidersAsync()
    {
        var httpRes = await _httpClient.GetAsync("api/aiassistant/providers");
        return await ProcessResultFromHttpResponse<List<AiProviderInfoDto>>(httpRes, "Get AI providers");
    }

    // api/aiassistant/chat answers with server-sent events: an "event:" and a "data:" line (AiChatStreamEventDto
    // as JSON) per event, and a blank line after each one.
    public async IAsyncEnumerable<AiChatStreamEventDto> ChatAsync(AiChatRequestDto request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/aiassistant/chat")
        {
            Content = JsonContent.Create(request)
        };
        // Read the answer as it arrives, not once it is complete.
        httpRequest.SetBrowserResponseStreamingEnabled(true);

        using var httpRes = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!httpRes.IsSuccessStatusCode)
        {
            var res = await ProcessResultFromHttpResponse<AiChatTurnDto>(httpRes, ChatAction);
            yield return new AiChatStreamEventDto { Type = AiChatStreamEventTypes.Error, Text = res.ErrorMessage };
            yield break;
        }

        using var stream = await httpRes.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);
        var data = new StringBuilder();

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            if (line.StartsWith("data:"))
            {
                if (data.Length > 0)
                    data.Append('\n');
                data.Append(line.AsSpan(line.Length > 5 && line[5] == ' ' ? 6 : 5));
            }
            else if (line.Length == 0 && data.Length > 0)
            {
                var e = ReadEvent(data.ToString());
                data.Clear();
                if (e != null)
                    yield return e;
            }
        }

        if (data.Length > 0 && ReadEvent(data.ToString()) is { } last)
            yield return last;
    }

    public async Task<Result<List<AiChatSessionInfoDto>>> GetSessionsAsync()
    {
        var httpRes = await _httpClient.GetAsync("api/aiassistant/sessions");
        return await ProcessResultFromHttpResponse<List<AiChatSessionInfoDto>>(httpRes, "Get AI sessions");
    }

    public async Task<Result<AiChatSessionDto>> GetSessionAsync(Guid noteId)
    {
        var httpRes = await _httpClient.GetAsync($"api/aiassistant/sessions/{noteId}");
        return await ProcessResultFromHttpResponse<AiChatSessionDto>(httpRes, "Get AI session");
    }

    public async Task<Result<AiChatSessionDto>> SaveSessionAsync(AiChatSessionDto session)
    {
        var httpRes = await _httpClient.PostAsJsonAsync("api/aiassistant/sessions", session);
        return await ProcessResultFromHttpResponse<AiChatSessionDto>(httpRes, "Save AI session");
    }

    private AiChatStreamEventDto? ReadEvent(string json)
    {
        var e = JsonSerializer.Deserialize<AiChatStreamEventDto>(json, JsonSerializerOptions.Web);
        if (e?.Type == AiChatStreamEventTypes.Error)
            _appState.NotifyError(ChatAction, e.Text ?? "");
        return e;
    }
}

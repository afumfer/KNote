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

    public Task<Result<List<AiProviderInfoDto>>> GetProvidersAsync(CancellationToken cancellationToken = default) =>
        SendAsync<List<AiProviderInfoDto>>(ct => _httpClient.GetAsync("api/aiassistant/providers", ct), "Get AI providers",
            cancellationToken: cancellationToken);

    // api/aiassistant/chat answers with server-sent events: an "event:" and a "data:" line (AiChatStreamEventDto
    // as JSON) per event, and a blank line after each one. The server not reached, or the answer cut (no network),
    // ends it with an Error event as the server's own; cancelled (cancellationToken), OperationCanceledException.
    public async IAsyncEnumerable<AiChatStreamEventDto> ChatAsync(AiChatRequestDto request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/aiassistant/chat")
        {
            Content = JsonContent.Create(request)
        };
        // Read the answer as it arrives, not once it is complete.
        httpRequest.SetBrowserResponseStreamingEnabled(true);

        var (response, sendError) = await SendChatAsync(httpRequest, cancellationToken);
        if (response == null)
        {
            yield return ErrorEvent(sendError!);
            yield break;
        }

        using var httpRes = response;
        if (!httpRes.IsSuccessStatusCode)
        {
            var res = await ProcessResultFromHttpResponse<AiChatTurnDto>(httpRes, ChatAction, cancellationToken: cancellationToken);
            yield return new AiChatStreamEventDto { Type = AiChatStreamEventTypes.Error, Text = res.ErrorMessage };
            yield break;
        }

        using var stream = await httpRes.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);
        var data = new StringBuilder();

        while (true)
        {
            var (line, readError) = await ReadLineAsync(reader, cancellationToken);
            if (readError != null)
            {
                yield return ErrorEvent(readError);
                yield break;
            }
            if (line == null)
                break;

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

    public Task<Result<List<AiChatSessionInfoDto>>> GetSessionsAsync(CancellationToken cancellationToken = default) =>
        SendAsync<List<AiChatSessionInfoDto>>(ct => _httpClient.GetAsync("api/aiassistant/sessions", ct), "Get AI sessions",
            cancellationToken: cancellationToken);

    public Task<Result<AiChatSessionDto>> GetSessionAsync(Guid noteId, CancellationToken cancellationToken = default) =>
        SendAsync<AiChatSessionDto>(ct => _httpClient.GetAsync($"api/aiassistant/sessions/{noteId}", ct), "Get AI session",
            cancellationToken: cancellationToken);

    public Task<Result<AiChatSessionDto>> SaveSessionAsync(AiChatSessionDto session, CancellationToken cancellationToken = default) =>
        SendAsync<AiChatSessionDto>(ct => _httpClient.PostAsJsonAsync("api/aiassistant/sessions", session, ct), "Save AI session",
            cancellationToken: cancellationToken);

    // An iterator can't yield from a catch: these return the failure, and ChatAsync turns it into an Error event.
    private async Task<(HttpResponseMessage? Response, string? Error)> SendChatAsync(HttpRequestMessage httpRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            return (await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken), null);
        }
        catch (Exception ex) when (ex is HttpRequestException || (ex is OperationCanceledException && !cancellationToken.IsCancellationRequested))
        {
            return (null, $"Error. The web server could not be reached: {ex.Message}");
        }
    }

    private static async Task<(string? Line, string? Error)> ReadLineAsync(StreamReader reader, CancellationToken cancellationToken)
    {
        try
        {
            return (await reader.ReadLineAsync(cancellationToken), null);
        }
        catch (Exception ex) when ((ex is HttpRequestException or IOException) && !cancellationToken.IsCancellationRequested)
        {
            return (null, $"Error. The answer was interrupted: {ex.Message}");
        }
    }

    private AiChatStreamEventDto ErrorEvent(string error)
    {
        _appState.NotifyError(ChatAction, error);
        return new AiChatStreamEventDto { Type = AiChatStreamEventTypes.Error, Text = error };
    }

    private AiChatStreamEventDto? ReadEvent(string json)
    {
        var e = JsonSerializer.Deserialize<AiChatStreamEventDto>(json, JsonSerializerOptions.Web);
        if (e?.Type == AiChatStreamEventTypes.Error)
            _appState.NotifyError(ChatAction, e.Text ?? "");
        return e;
    }
}

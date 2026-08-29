using Microsoft.Extensions.AI;

namespace KNote.ClientWin.Tests.Fakes;

/// <summary>
/// Minimal IChatClient test double: GetResponseAsync/GetStreamingResponseAsync only work when
/// their Impl delegate is set; everything else throws - same convention as the other Fakes/ in
/// this project. Lets KNoteAIAssistantCtrlTests exercise the controller's own logic (message
/// history bookkeeping, error rollback) without any real provider/network/API key.
/// </summary>
internal class FakeChatClient : IChatClient
{
    public Func<IEnumerable<ChatMessage>, ChatOptions, CancellationToken, Task<ChatResponse>> GetResponseImpl { get; set; }
    public Func<IEnumerable<ChatMessage>, ChatOptions, CancellationToken, IAsyncEnumerable<ChatResponseUpdate>> GetStreamingResponseImpl { get; set; }

    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions options = null, CancellationToken cancellationToken = default) =>
        (GetResponseImpl ?? throw new NotSupportedException($"{nameof(GetResponseAsync)} not configured for this test"))(messages, options, cancellationToken);

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions options = null, CancellationToken cancellationToken = default) =>
        (GetStreamingResponseImpl ?? throw new NotSupportedException($"{nameof(GetStreamingResponseAsync)} not configured for this test"))(messages, options, cancellationToken);

    public object GetService(Type serviceType, object serviceKey = null) => null;

    public void Dispose() { }
}

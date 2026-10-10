using System.Runtime.CompilerServices;
using KNote.Model;
using KNote.Server.Ai;
using Microsoft.Extensions.AI;

namespace KNote.Tests.Helpers;

/// <summary>
/// IAiChatClientProvider for the in-process tests: instead of a real provider, each call to the model streams the
/// next response of Script (a list of ChatResponseUpdate per call), wrapped like AiChatClientFactory does - the
/// request's tools and function invocation - so a scripted FunctionCallContent really runs the KNote tool.
/// </summary>
public class ScriptedAiChatClientProvider : IAiChatClientProvider
{
    // One list of updates per call to the model, in order; an exception thrown by Script fails the call.
    public Func<List<List<ChatResponseUpdate>>> Script { get; set; } = () => [];

    public IChatClient Create(AiProviderRef providerRef, IEnumerable<AITool> tools)
    {
        var toolList = tools.ToList();
        return new ScriptedChatClient(Script)
            .AsBuilder()
            .ConfigureOptions(o => o.Tools = [.. toolList])
            .UseFunctionInvocation()
            .Build();
    }

    private sealed class ScriptedChatClient(Func<List<List<ChatResponseUpdate>>> script) : IChatClient
    {
        private List<List<ChatResponseUpdate>>? _responses;
        private int _calls;

        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("The Web assistant always streams.");

        public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages,
            ChatOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            _responses ??= script();
            if (_calls >= _responses.Count)
                throw new InvalidOperationException("The test script has no more responses.");

            foreach (var update in _responses[_calls++])
            {
                await Task.Yield();
                yield return update;
            }
        }

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose() { }
    }
}

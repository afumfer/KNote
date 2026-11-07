using Microsoft.Extensions.AI;

namespace KNote.Ai;

// The images that view_note_images has looked up in a turn (KNote Atlas, D12). A tool result cannot carry them the same way to
// every provider, and Ollama's client would turn them into text; a message with images reaches them all, as an attachment does.
public sealed class AiToolImages
{
    // However many times the model asks in a turn: its context is not for this.
    public const int MaxPerTurn = 9;

    private readonly Dictionary<Guid, DataContent> _images = new();

    public int Count => _images.Count;

    public bool IsFull => _images.Count >= MaxPerTurn;

    public void Add(Guid id, byte[] content, string contentType) => _images.TryAdd(id, new DataContent(content, contentType));

    public ChatMessage ToMessage() => new(ChatRole.User, [new TextContent("[The images of view_note_images, in the order it gave.]"), .. _images.Values]);
}

// Right above the provider, under the function invocation: each call to the model of a turn gets the images found so far, after
// the result of the tool that asked for them. They are not kept in the conversation.
public sealed class AiToolImagesChatClient(IChatClient innerClient) : DelegatingChatClient(innerClient)
{
    public const string PropertyName = "knt-tool-images";

    public override Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions options = null, CancellationToken cancellationToken = default)
    {
        var (withImages, forwarded) = Prepare(messages, options);
        return base.GetResponseAsync(withImages, forwarded, cancellationToken);
    }

    public override IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions options = null, CancellationToken cancellationToken = default)
    {
        var (withImages, forwarded) = Prepare(messages, options);
        return base.GetStreamingResponseAsync(withImages, forwarded, cancellationToken);
    }

    // The property stays here: a provider would not know what to do with it.
    private static (IEnumerable<ChatMessage> Messages, ChatOptions Options) Prepare(IEnumerable<ChatMessage> messages, ChatOptions options)
    {
        if (options?.AdditionalProperties == null || !options.AdditionalProperties.TryGetValue(PropertyName, out var value))
            return (messages, options);

        var forwarded = options.Clone();
        forwarded.AdditionalProperties = new AdditionalPropertiesDictionary(options.AdditionalProperties);
        forwarded.AdditionalProperties.Remove(PropertyName);

        return value is AiToolImages { Count: > 0 } images
            ? (messages.Append(images.ToMessage()), forwarded)
            : (messages, forwarded);
    }
}

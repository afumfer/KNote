using KNote.Model;

namespace KNote.Ai;

/// <summary>
/// The context KNote fixes in the calls of the summaries (KNote Atlas, stage 2, D11): without num_ctx, Ollama doesn't use the
/// model's context but its own, which its server decides and may be much smaller, and it cuts what doesn't fit without saying it.
/// Ollama: the provider's «Context size» or, without it, the smaller of the model's longest one (/api/show) and 16,384 (measured in 2.1a1). OpenAI and
/// Anthropic: a fixed one, kept small for its cost (2.1a). The assistant doesn't change.
/// </summary>
public static class AiSummaryContext
{
    public const int OllamaDefaultTokens = 16_384;
    public const int PaidTokens = 32_000;

    public static async Task<int> TokensAsync(AiProviderRef provider, CancellationToken cancellationToken = default)
    {
        if (provider?.Provider != EnumAiProvider.Ollama)
            return PaidTokens;
        if (provider.ContextWindow is int contextWindow && contextWindow > 0)
            return contextWindow;

        var longest = await AiModelCapabilities.ContextLengthAsync(provider, cancellationToken);
        return longest is int length ? Math.Min(length, OllamaDefaultTokens) : OllamaDefaultTokens;
    }

    /// <summary>The provider with that context, for its chat client (AiChatClientFactory sends it to Ollama as num_ctx).</summary>
    public static AiProviderRef With(AiProviderRef provider, int tokens)
    {
        if (provider?.Provider != EnumAiProvider.Ollama)
            return provider;
        var copy = provider.GetSimpleDto<AiProviderRef>();
        copy.ContextWindow = tokens;
        return copy;
    }
}

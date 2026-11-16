using System.Collections.Concurrent;
using KNote.Model;
using OllamaSharp;

namespace KNote.Ai;

// What a model can do, for what the apps offer (KNote Atlas, D12). Ollama says it (/api/show); OpenAI and Anthropic do not, and
// their current models see images, so they are taken as able. If Ollama cannot be asked, the model is too: the provider's error
// says more than a notice that it may not see images.
public static class AiModelCapabilities
{
    private const string VisionCapability = "vision";

    private static readonly ConcurrentDictionary<string, bool> Known = new();

    public static async Task<bool> SupportsImagesAsync(AiProviderRef provider, CancellationToken cancellationToken = default)
    {
        if (provider?.Provider != EnumAiProvider.Ollama)
            return true;

        var key = $"{provider.Host}|{provider.Model}";
        if (Known.TryGetValue(key, out var known))
            return known;

        try
        {
            using var client = new OllamaApiClient(provider.Host);
            var model = await client.ShowModelAsync(provider.Model, cancellationToken);
            if (model?.Capabilities == null)
                return true;

            return Known[key] = model.Capabilities.Contains(VisionCapability, StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return true;
        }
    }

    private static readonly ConcurrentDictionary<string, int?> ContextLengths = new();

    /// <summary>
    /// The longest context of an Ollama model, as /api/show says it ({architecture}.context_length; KNote Atlas, stage 2, D11), or
    /// null if Ollama doesn't say it or can't be asked.
    /// </summary>
    public static async Task<int?> ContextLengthAsync(AiProviderRef provider, CancellationToken cancellationToken = default)
    {
        if (provider?.Provider != EnumAiProvider.Ollama)
            return null;

        var key = $"{provider.Host}|{provider.Model}";
        if (ContextLengths.TryGetValue(key, out var known))
            return known;

        try
        {
            using var client = new OllamaApiClient(provider.Host);
            var model = await client.ShowModelAsync(provider.Model, cancellationToken);
            var info = model?.Info;
            if (info?.ExtraInfo == null)
                return null;

            var entry = info.ExtraInfo.FirstOrDefault(e => e.Key.EndsWith(".context_length", StringComparison.Ordinal));
            return ContextLengths[key] = entry.Value != null && int.TryParse(entry.Value.ToString(), out var length) && length > 0 ? length : null;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return null;
        }
    }
}

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
}

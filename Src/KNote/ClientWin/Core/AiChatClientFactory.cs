using Anthropic;
using KNote.Model;
using Microsoft.Extensions.AI;
using OllamaSharp;

namespace KNote.ClientWin.Core;

// KNoteAIAssistant plan (Phase 2): builds the Microsoft.Extensions.AI IChatClient for a given
// AiProviderRef, dispatching over the fixed provider set (EnumAiProvider). No DI container here -
// ClientWin has none - so this mirrors the manual switch used by the PrimerChatbotSimple PoC.
public static class AiChatClientFactory
{
    public static IChatClient Create(AiProviderRef providerRef)
    {
        if (providerRef is null)
            throw new ArgumentNullException(nameof(providerRef));

        return providerRef.Provider switch
        {
            EnumAiProvider.OpenAI => new OpenAI.Chat.ChatClient(
                providerRef.Model,
                ResolveApiKey(providerRef, "OPENAI_API_KEY"))
                .AsIChatClient(),

            EnumAiProvider.Anthropic => new AnthropicClient
            {
                ApiKey = ResolveApiKey(providerRef, "ANTHROPIC_API_KEY")
            }
            .AsIChatClient()
            .AsBuilder()
            .ConfigureOptions(o => o.ModelId = providerRef.Model)
            .Build(),

            EnumAiProvider.Ollama => new OllamaApiClient(providerRef.Host, providerRef.Model),

            _ => throw new ArgumentException($"Unknown AI provider: {providerRef.Provider}", nameof(providerRef))
        };
    }

    // KNoteData.config (AiProviderRef.ApiKey) takes precedence; the environment variable is only
    // a fallback for local/manual testing when the config hasn't been filled in yet. Not used for
    // Ollama, which authenticates the local/remote server by host instead of an API key.
    private static string ResolveApiKey(AiProviderRef providerRef, string environmentVariableName)
    {
        if (!string.IsNullOrEmpty(providerRef.ApiKey))
            return providerRef.ApiKey;

        return Environment.GetEnvironmentVariable(environmentVariableName) ?? "";
    }
}

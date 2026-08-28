using Anthropic;
using KNote.Model;
using KNote.Service.Core;
using Microsoft.Extensions.AI;
using OllamaSharp;

namespace KNote.ClientWin.Core;

// KNoteAIAssistant plan (Phase 2): builds the Microsoft.Extensions.AI IChatClient for a given
// AiProviderRef, dispatching over the fixed provider set (EnumAiProvider). No DI container here -
// ClientWin has none - so this mirrors the manual switch used by the PrimerChatbotSimple PoC.
// Phase 5 adds KNoteAiTools (search_notes) uniformly to all three providers via
// UseFunctionInvocation() - tool-calling support then depends on the chosen model, not on this
// wiring (e.g. it requires an Ollama model that supports function calling).
public static class AiChatClientFactory
{
    public static IChatClient Create(AiProviderRef providerRef, ServiceRef serviceRef)
    {
        if (providerRef is null)
            throw new ArgumentNullException(nameof(providerRef));

        IChatClient baseClient = providerRef.Provider switch
        {
            EnumAiProvider.OpenAI => new OpenAI.Chat.ChatClient(
                providerRef.Model,
                ResolveApiKey(providerRef, "OPENAI_API_KEY"))
                .AsIChatClient(),

            EnumAiProvider.Anthropic => new AnthropicClient
            {
                ApiKey = ResolveApiKey(providerRef, "ANTHROPIC_API_KEY")
            }.AsIChatClient(),

            EnumAiProvider.Ollama => new OllamaApiClient(providerRef.Host, providerRef.Model),

            _ => throw new ArgumentException($"Unknown AI provider: {providerRef.Provider}", nameof(providerRef))
        };

        var tools = new KNoteAiTools(serviceRef);

        return baseClient.AsBuilder()
            .ConfigureOptions(o =>
            {
                // OpenAI/Ollama already bake the model into the client at construction above;
                // only the Anthropic bridge needs it set through ChatOptions.
                if (providerRef.Provider == EnumAiProvider.Anthropic)
                    o.ModelId = providerRef.Model;

                // OpenAI's reasoning models (e.g. gpt-5.x) reject function tools on
                // /v1/chat/completions unless reasoning_effort is explicitly "none" (HTTP 400
                // invalid_request_error otherwise). Non-reasoning OpenAI models ignore this.
                if (providerRef.Provider == EnumAiProvider.OpenAI)
                    o.Reasoning = new ReasoningOptions { Effort = ReasoningEffort.None };

                o.Tools = [.. tools.GetTools()];
            })
            .UseFunctionInvocation()
            .Build();
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

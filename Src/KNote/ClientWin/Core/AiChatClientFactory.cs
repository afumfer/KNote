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

        var tools = new KNoteAiTools(serviceRef.Service);

        return baseClient.AsBuilder()
            .ConfigureOptions(o =>
            {
                // OpenAI/Ollama already bake the model into the client at construction above;
                // only the Anthropic bridge needs it set through ChatOptions.
                if (providerRef.Provider == EnumAiProvider.Anthropic)
                    o.ModelId = providerRef.Model;

                // OpenAI's reasoning models (o1/o3/o4/gpt-5.x) reject function tools on
                // /v1/chat/completions unless reasoning_effort is explicitly "none" (HTTP 400
                // invalid_request_error otherwise) - but older/non-reasoning models (gpt-4o,
                // gpt-4o-mini, ...) reject the reasoning_effort argument outright as unrecognized
                // (a different HTTP 400). Confirmed by ClientWin.Tests/OpenAiProviderSmokeTests -
                // only set this for models that actually need it.
                if (providerRef.Provider == EnumAiProvider.OpenAI && IsReasoningModel(providerRef.Model))
                    o.Reasoning = new ReasoningOptions { Effort = ReasoningEffort.None };

                o.Tools = [.. tools.GetTools()];
            })
            .UseFunctionInvocation()
            .Build();
    }

    // There's no API to ask "does this model support reasoning_effort", so this is a name-based
    // heuristic - update it if OpenAI (or a compatible gateway/proxy exposing custom model
    // aliases) ships a new reasoning-model family name.
    // Internal so ClientWin.Tests can verify the heuristic directly for known model names.
    internal static bool IsReasoningModel(string model) =>
        !string.IsNullOrEmpty(model) &&
        (model.StartsWith("o1", StringComparison.OrdinalIgnoreCase) ||
         model.StartsWith("o3", StringComparison.OrdinalIgnoreCase) ||
         model.StartsWith("o4", StringComparison.OrdinalIgnoreCase) ||
         model.Contains("gpt-5", StringComparison.OrdinalIgnoreCase));

    // KNoteData.config (AiProviderRef.ApiKey) takes precedence; the environment variable is only
    // a fallback for local/manual testing when the config hasn't been filled in yet. Not used for
    // Ollama, which authenticates the local/remote server by host instead of an API key.
    // Internal (not private) so ClientWin.Tests/AiChatClientFactoryTests.cs can exercise the
    // precedence logic directly, without a real network call.
    internal static string ResolveApiKey(AiProviderRef providerRef, string environmentVariableName)
    {
        if (!string.IsNullOrEmpty(providerRef.ApiKey))
            return providerRef.ApiKey;

        return Environment.GetEnvironmentVariable(environmentVariableName) ?? "";
    }
}

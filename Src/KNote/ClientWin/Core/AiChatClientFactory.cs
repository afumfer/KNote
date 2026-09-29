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
    public static IChatClient Create(AiProviderRef providerRef, ServiceRef serviceRef, Store store)
    {
        if (providerRef is null)
            throw new ArgumentNullException(nameof(providerRef));

        IChatClient baseClient = providerRef.Provider switch
        {
            // Responses API (/v1/responses), not Chat Completions: OpenAI's reasoning models reject
            // function tools on /v1/chat/completions unless reasoning_effort is "none", and some of
            // them (gpt-6-astra) don't accept "none" either - so tools can't work there at all.
            // /v1/responses accepts tools with any reasoning effort, and non-reasoning models
            // (gpt-4o, ...) work unchanged. OPENAI001: the SDK still flags this API as
            // evaluation-only - re-run OpenAiProviderSmokeTests after bumping OpenAI or
            // Microsoft.Extensions.AI.OpenAI.
#pragma warning disable OPENAI001
            EnumAiProvider.OpenAI => new OpenAI.Responses.ResponsesClient(
                ResolveApiKey(providerRef, "OPENAI_API_KEY"))
                .AsIChatClient(providerRef.Model),
#pragma warning restore OPENAI001

            EnumAiProvider.Anthropic => new AnthropicClient
            {
                ApiKey = ResolveApiKey(providerRef, "ANTHROPIC_API_KEY")
            }.AsIChatClient(),

            EnumAiProvider.Ollama => new OllamaApiClient(providerRef.Host, providerRef.Model),

            _ => throw new ArgumentException($"Unknown AI provider: {providerRef.Provider}", nameof(providerRef))
        };

        var tools = new KNoteAiTools(serviceRef.Service, store);

        return baseClient.AsBuilder()
            .ConfigureOptions(o =>
            {
                // OpenAI/Ollama already bake the model into the client at construction above;
                // only the Anthropic bridge needs it set through ChatOptions.
                if (providerRef.Provider == EnumAiProvider.Anthropic)
                    o.ModelId = providerRef.Model;

                // Unlike Chat Completions, the Responses API stores responses server-side by
                // default (store=true) - keep them off OpenAI's servers, since tool results carry
                // note contents. No reasoning_effort is sent: each model uses its own default.
#pragma warning disable OPENAI001
                if (providerRef.Provider == EnumAiProvider.OpenAI)
                    o.RawRepresentationFactory = _ => new OpenAI.Responses.CreateResponseOptions { StoredOutputEnabled = false };
#pragma warning restore OPENAI001

                o.Tools = [.. tools.GetTools()];
            })
            .UseFunctionInvocation()
            .Build();
    }

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

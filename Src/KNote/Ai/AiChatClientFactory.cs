using Anthropic;
using KNote.Model;
using Microsoft.Extensions.AI;
using OllamaSharp;

namespace KNote.Ai;

// Builds the Microsoft.Extensions.AI IChatClient for a given AiProviderRef, dispatching over the fixed
// provider set (EnumAiProvider). Shared by ClientWin and Server: each one passes the tools it wants the
// model to have (see KNoteAiTools), attached uniformly to all three providers via UseFunctionInvocation() -
// tool-calling support then depends on the chosen model, not on this wiring (e.g. it requires an Ollama
// model that supports function calling).
public static class AiChatClientFactory
{
    public const int AnthropicMaxOutputTokens = 64000;

    public static IChatClient Create(AiProviderRef providerRef, IEnumerable<AITool> tools = null)
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

            // The Messages API requires max_tokens and the SDK sends 1024 when none is given, which cuts long
            // answers. It is only a ceiling (neither billed nor counted against the rate limits) and fits
            // every model from Claude Haiku 4.5 on (64K output; the rest of the current models allow 128K).
            EnumAiProvider.Anthropic => new AnthropicClient
            {
                ApiKey = ResolveApiKey(providerRef, "ANTHROPIC_API_KEY")
            }.AsIChatClient(providerRef.Model, AnthropicMaxOutputTokens),

            EnumAiProvider.Ollama => new OllamaApiClient(providerRef.Host, providerRef.Model),

            _ => throw new ArgumentException($"Unknown AI provider: {providerRef.Provider}", nameof(providerRef))
        };

        var toolList = tools?.ToList() ?? new List<AITool>();

        return baseClient.AsBuilder()
            .ConfigureOptions(o =>
            {
                // Unlike Chat Completions, the Responses API stores responses server-side by
                // default (store=true) - keep them off OpenAI's servers, since tool results carry
                // note contents. No reasoning_effort is sent: each model uses its own default.
#pragma warning disable OPENAI001
                if (providerRef.Provider == EnumAiProvider.OpenAI)
                    o.RawRepresentationFactory = _ => new OpenAI.Responses.CreateResponseOptions { StoredOutputEnabled = false };
#pragma warning restore OPENAI001

                if (toolList.Count > 0)
                    o.Tools = [.. toolList];
            })
            .UseFunctionInvocation()
            .Build();
    }

    // The configured key (AiProviderRef.ApiKey: KNoteData.config in ClientWin, appsettings/user-secrets in
    // Server) takes precedence; the environment variable is only a fallback for local/manual testing when
    // the configuration hasn't been filled in yet. Not used for Ollama, which authenticates the
    // local/remote server by host instead of an API key.
    internal static string ResolveApiKey(AiProviderRef providerRef, string environmentVariableName)
    {
        if (!string.IsNullOrEmpty(providerRef.ApiKey))
            return providerRef.ApiKey;

        return Environment.GetEnvironmentVariable(environmentVariableName) ?? "";
    }
}

using KNote.Model;
using Microsoft.Extensions.Configuration;

namespace KNote.ClientWin.Tests.Helpers;

/// <summary>
/// Resolves the AiProviderRef used by the "RequiresRealAiProvider" smoke tests
/// (OpenAiProviderSmokeTests / AnthropicProviderSmokeTests / OllamaProviderSmokeTests), one per
/// provider. Same precedence as production (AiChatClientFactory.ResolveApiKey): the config file
/// (appsettings.json, overridable via `dotnet user-secrets` on this project) wins; falling back to
/// the same environment variables AiChatClientFactory itself falls back to
/// (OPENAI_API_KEY/ANTHROPIC_API_KEY) means a dev who already has those set for manual ClientWin
/// testing gets the smoke tests "for free", no extra setup. Returns null when a provider isn't
/// configured at all - callers are expected to Assert.Inconclusive(...) in that case rather than
/// fail or silently pass.
/// </summary>
internal static class AiTestConfig
{
    private static readonly IConfiguration Configuration = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
        .AddUserSecrets(typeof(AiTestConfig).Assembly, optional: true)
        .Build();

    public static AiProviderRef TryGetOpenAiProviderRef()
    {
        var section = Configuration.GetSection("AiProviderSmokeTests:OpenAI");
        var apiKey = FirstNonEmpty(section["ApiKey"], Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
        if (string.IsNullOrEmpty(apiKey))
            return null;

        return new AiProviderRef
        {
            Alias = "Test OpenAI",
            Provider = EnumAiProvider.OpenAI,
            Model = string.IsNullOrEmpty(section["Model"]) ? "gpt-4o-mini" : section["Model"],
            ApiKey = apiKey
        };
    }

    public static AiProviderRef TryGetAnthropicProviderRef()
    {
        var section = Configuration.GetSection("AiProviderSmokeTests:Anthropic");
        var apiKey = FirstNonEmpty(section["ApiKey"], Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY"));
        if (string.IsNullOrEmpty(apiKey))
            return null;

        return new AiProviderRef
        {
            Alias = "Test Anthropic",
            Provider = EnumAiProvider.Anthropic,
            Model = string.IsNullOrEmpty(section["Model"]) ? "claude-haiku-4-5" : section["Model"],
            ApiKey = apiKey
        };
    }

    public static AiProviderRef TryGetOllamaProviderRef()
    {
        var section = Configuration.GetSection("AiProviderSmokeTests:Ollama");
        var host = FirstNonEmpty(section["Host"], Environment.GetEnvironmentVariable("OLLAMA_HOST"));
        if (string.IsNullOrEmpty(host))
            return null;

        return new AiProviderRef
        {
            Alias = "Test Ollama",
            Provider = EnumAiProvider.Ollama,
            Model = string.IsNullOrEmpty(section["Model"]) ? "llama3.1" : section["Model"],
            Host = host
        };
    }

    /// <summary>
    /// Ollama has no API key to "not be configured" - a Host can be set but point at nothing
    /// listening. Checked separately (with a short timeout) so an unreachable/not-installed local
    /// Ollama reports as Inconclusive ("not available"), not a hard test failure.
    /// </summary>
    public static async Task<bool> IsOllamaReachableAsync(string host)
    {
        try
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            var response = await httpClient.GetAsync($"{host.TrimEnd('/')}/api/tags");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static string FirstNonEmpty(string a, string b) => string.IsNullOrEmpty(a) ? b : a;
}

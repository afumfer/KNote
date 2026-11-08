using KNote.ClientWin.Tests.Helpers;
using Microsoft.Extensions.AI;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Real-call smoke tests against a local/reachable Ollama server. Excluded from routine runs
/// (`dotnet test --filter "TestCategory!=RequiresRealAiProvider"`); run explicitly after bumping
/// OllamaSharp/Microsoft.Extensions.AI. Unlike OpenAI/Anthropic, "not configured" isn't just a
/// missing API key - the configured Host might not have anything listening (Ollama not installed,
/// not running, wrong port), so reachability is checked first and reported as Inconclusive too,
/// distinct from an actual regression. Tool-calling support additionally depends on the chosen
/// model (not every Ollama model supports function calling) - a failure there may mean "pick a
/// different model", not "the wiring broke". See ClientWin.Tests/CLAUDE.md.
/// </summary>
[TestClass]
[TestCategory("RequiresRealAiProvider")]
public class OllamaProviderSmokeTests
{
    // KNote Atlas 1.3: a model that reasons sends it apart from the answer, without asking for it. A model that does not
    // reason says nothing about the wiring.
    [TestMethod]
    public async Task Reasoning_ComesApartFromTheAnswer()
    {
        var providerRef = await GetReachableProviderRefOrInconclusiveAsync();
        if (providerRef == null)
            return;

        var client = TestAiChatClientFactory.Create(providerRef);
        KNote.Model.Dto.AiChatTurnDto turn = null;
        await foreach (var e in KNote.Ai.AiChatTurnStreamer.StreamAsync(client, null, null, "How much is 128 + 512? Think it through."))
            if (e.Type == KNote.Model.Dto.AiChatStreamEventTypes.Completed)
                turn = e.Turn;

        Assert.IsNotNull(turn);
        Assert.IsFalse(string.IsNullOrWhiteSpace(turn.Answer), "Expected an answer besides the reasoning.");
        if (turn.Reasoning == null)
            Assert.Inconclusive($"{providerRef.Model} did not reason: try with a model that does.");
        Assert.IsNotNull(turn.ReasoningTime);
    }

    // KNote Atlas 1.5. With a model that does not see images, say so: it is not the wiring.
    [TestMethod]
    public async Task WebSearch_KnoteSearchesTheInternet_WithOllamasWebSearch()
    {
        var providerRef = await GetReachableProviderRefOrInconclusiveAsync();
        if (providerRef == null)
            return;

        if (string.IsNullOrWhiteSpace(providerRef.WebSearchApiKey))
            Assert.Inconclusive("No web search key of ollama.com (AiProviderSmokeTests:Ollama:WebSearchApiKey or OLLAMA_API_KEY) - skipping.");

        await AiWebSearchSmokeCheck.AssertTheModelSearchesTheInternetAsync(providerRef);
    }

    [TestMethod]
    public async Task Image_IsReadByTheModel()
    {
        var providerRef = await GetReachableProviderRefOrInconclusiveAsync();
        if (providerRef == null)
            return;

        if (!await KNote.Ai.AiModelCapabilities.SupportsImagesAsync(providerRef))
            Assert.Inconclusive($"{providerRef.Model} does not see images: try with a model that does.");

        await AiImageSmokeCheck.AssertTheModelReadsAnImageAsync(TestAiChatClientFactory.Create(providerRef));
    }

    private static async Task<KNote.Model.AiProviderRef> GetReachableProviderRefOrInconclusiveAsync()
    {
        var providerRef = AiTestConfig.TryGetOllamaProviderRef();
        if (providerRef == null)
        {
            Assert.Inconclusive("No Ollama host configured (OLLAMA_HOST env var or ClientWin.Tests user-secrets/appsettings.json) - skipping Ollama smoke test.");
            return null;
        }

        if (!await AiTestConfig.IsOllamaReachableAsync(providerRef.Host))
        {
            Assert.Inconclusive($"Ollama host '{providerRef.Host}' is not reachable - skipping Ollama smoke test.");
            return null;
        }

        return providerRef;
    }

    [TestMethod]
    public async Task Completion_ReturnsNonEmptyResponse()
    {
        var providerRef = await GetReachableProviderRefOrInconclusiveAsync();
        if (providerRef == null)
            return;

        var client = TestAiChatClientFactory.Create(providerRef);
        var response = await client.GetResponseAsync([new ChatMessage(ChatRole.User, "Reply with exactly one word: OK.")]);

        Assert.IsFalse(string.IsNullOrWhiteSpace(response.Text), "Expected a non-empty response from Ollama.");
    }

    [TestMethod]
    public async Task Streaming_ReturnsNonEmptyResponse()
    {
        var providerRef = await GetReachableProviderRefOrInconclusiveAsync();
        if (providerRef == null)
            return;

        var client = TestAiChatClientFactory.Create(providerRef);
        var text = new System.Text.StringBuilder();
        await foreach (var update in client.GetStreamingResponseAsync([new ChatMessage(ChatRole.User, "Reply with exactly one word: OK.")]))
            text.Append(update.Text);

        Assert.IsFalse(string.IsNullOrWhiteSpace(text.ToString()), "Expected a non-empty streamed response from Ollama.");
    }

    [TestMethod]
    public async Task ToolCalling_SearchNotesRoundTripCompletesWithoutError()
    {
        var providerRef = await GetReachableProviderRefOrInconclusiveAsync();
        if (providerRef == null)
            return;

        var client = TestAiChatClientFactory.Create(providerRef);
        var response = await client.GetResponseAsync([
            new ChatMessage(ChatRole.User, "Use the search_notes tool to search for the word \"test\", then summarize in one sentence what you found (even if nothing was found).")
        ]);

        Assert.IsFalse(string.IsNullOrWhiteSpace(response.Text), "Expected a non-empty response after the tool round trip. Note: not every Ollama model supports function calling - if this fails, try a model known to support tools (e.g. llama3.1, qwen2.5) before assuming the wiring is broken.");
    }
}

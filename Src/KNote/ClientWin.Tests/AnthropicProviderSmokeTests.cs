using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Helpers;
using Microsoft.Extensions.AI;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Real-call smoke tests against the actual Anthropic API. Excluded from routine runs
/// (`dotnet test --filter "TestCategory!=RequiresRealAiProvider"`); run explicitly
/// (`dotnet test --filter "TestCategory=RequiresRealAiProvider"`) after bumping
/// Anthropic/Microsoft.Extensions.AI to confirm the wiring in AiChatClientFactory still works
/// end-to-end. See ClientWin.Tests/CLAUDE.md for how to configure ANTHROPIC_API_KEY.
/// </summary>
[TestClass]
[TestCategory("RequiresRealAiProvider")]
public class AnthropicProviderSmokeTests
{
    [TestMethod]
    public async Task Completion_ReturnsNonEmptyResponse()
    {
        var providerRef = AiTestConfig.TryGetAnthropicProviderRef();
        if (providerRef == null)
        {
            Assert.Inconclusive("ANTHROPIC_API_KEY is not configured (env var or ClientWin.Tests user-secrets/appsettings.json) - skipping Anthropic smoke test.");
            return;
        }

        var client = AiChatClientFactory.Create(providerRef, TestServiceRefFactory.CreateInMemorySqlite());
        var response = await client.GetResponseAsync([new ChatMessage(ChatRole.User, "Reply with exactly one word: OK.")]);

        Assert.IsFalse(string.IsNullOrWhiteSpace(response.Text), "Expected a non-empty response from Anthropic.");
    }

    [TestMethod]
    public async Task Streaming_ReturnsNonEmptyResponse()
    {
        var providerRef = AiTestConfig.TryGetAnthropicProviderRef();
        if (providerRef == null)
        {
            Assert.Inconclusive("ANTHROPIC_API_KEY is not configured (env var or ClientWin.Tests user-secrets/appsettings.json) - skipping Anthropic smoke test.");
            return;
        }

        var client = AiChatClientFactory.Create(providerRef, TestServiceRefFactory.CreateInMemorySqlite());
        var text = new System.Text.StringBuilder();
        await foreach (var update in client.GetStreamingResponseAsync([new ChatMessage(ChatRole.User, "Reply with exactly one word: OK.")]))
            text.Append(update.Text);

        Assert.IsFalse(string.IsNullOrWhiteSpace(text.ToString()), "Expected a non-empty streamed response from Anthropic.");
    }

    [TestMethod]
    public async Task ToolCalling_SearchNotesRoundTripCompletesWithoutError()
    {
        var providerRef = AiTestConfig.TryGetAnthropicProviderRef();
        if (providerRef == null)
        {
            Assert.Inconclusive("ANTHROPIC_API_KEY is not configured (env var or ClientWin.Tests user-secrets/appsettings.json) - skipping Anthropic smoke test.");
            return;
        }

        var client = AiChatClientFactory.Create(providerRef, TestServiceRefFactory.CreateInMemorySqlite());
        var response = await client.GetResponseAsync([
            new ChatMessage(ChatRole.User, "Use the search_notes tool to search for the word \"test\", then summarize in one sentence what you found (even if nothing was found).")
        ]);

        Assert.IsFalse(string.IsNullOrWhiteSpace(response.Text), "Expected a non-empty response after the tool round trip.");
    }
}

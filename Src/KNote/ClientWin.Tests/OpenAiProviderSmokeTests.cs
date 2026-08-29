using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Helpers;
using Microsoft.Extensions.AI;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Real-call smoke tests against the actual OpenAI API. Excluded from routine runs
/// (`dotnet test --filter "TestCategory!=RequiresRealAiProvider"`); run explicitly
/// (`dotnet test --filter "TestCategory=RequiresRealAiProvider"`) after bumping
/// OpenAI/Microsoft.Extensions.AI(.OpenAI) to confirm the wiring in AiChatClientFactory still works
/// end-to-end - this is exactly the kind of regression a NuGet bump can introduce silently (see the
/// gpt-5.x "reasoning_effort" incompatibility with function tools fixed in AiChatClientFactory).
/// See ClientWin.Tests/CLAUDE.md for how to configure OPENAI_API_KEY.
/// </summary>
[TestClass]
[TestCategory("RequiresRealAiProvider")]
public class OpenAiProviderSmokeTests
{
    [TestMethod]
    public async Task Completion_ReturnsNonEmptyResponse()
    {
        var providerRef = AiTestConfig.TryGetOpenAiProviderRef();
        if (providerRef == null)
        {
            Assert.Inconclusive("OPENAI_API_KEY is not configured (env var or ClientWin.Tests user-secrets/appsettings.json) - skipping OpenAI smoke test.");
            return;
        }

        var client = AiChatClientFactory.Create(providerRef, TestServiceRefFactory.CreateInMemorySqlite());
        var response = await client.GetResponseAsync([new ChatMessage(ChatRole.User, "Reply with exactly one word: OK.")]);

        Assert.IsFalse(string.IsNullOrWhiteSpace(response.Text), "Expected a non-empty response from OpenAI.");
    }

    [TestMethod]
    public async Task Streaming_ReturnsNonEmptyResponse()
    {
        var providerRef = AiTestConfig.TryGetOpenAiProviderRef();
        if (providerRef == null)
        {
            Assert.Inconclusive("OPENAI_API_KEY is not configured (env var or ClientWin.Tests user-secrets/appsettings.json) - skipping OpenAI smoke test.");
            return;
        }

        var client = AiChatClientFactory.Create(providerRef, TestServiceRefFactory.CreateInMemorySqlite());
        var text = new System.Text.StringBuilder();
        await foreach (var update in client.GetStreamingResponseAsync([new ChatMessage(ChatRole.User, "Reply with exactly one word: OK.")]))
            text.Append(update.Text);

        Assert.IsFalse(string.IsNullOrWhiteSpace(text.ToString()), "Expected a non-empty streamed response from OpenAI.");
    }

    [TestMethod]
    public async Task ToolCalling_SearchNotesRoundTripCompletesWithoutError()
    {
        var providerRef = AiTestConfig.TryGetOpenAiProviderRef();
        if (providerRef == null)
        {
            Assert.Inconclusive("OPENAI_API_KEY is not configured (env var or ClientWin.Tests user-secrets/appsettings.json) - skipping OpenAI smoke test.");
            return;
        }

        // AiChatClientFactory.Create always attaches KNoteAiTools' tools (search_notes/get_note_details)
        // and enables function invocation, exactly like production - this is the request shape that
        // triggered the "reasoning_effort" HTTP 400 with gpt-5.x models when it wasn't yet handled.
        var client = AiChatClientFactory.Create(providerRef, TestServiceRefFactory.CreateInMemorySqlite());
        var response = await client.GetResponseAsync([
            new ChatMessage(ChatRole.User, "Use the search_notes tool to search for the word \"test\", then summarize in one sentence what you found (even if nothing was found).")
        ]);

        Assert.IsFalse(string.IsNullOrWhiteSpace(response.Text), "Expected a non-empty response after the tool round trip.");
    }
}

using KNote.Ai;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests.Helpers;

// KNote Atlas 1.6: a question only the Internet answers, with the search of a turn as the assistant builds it
// (AiWebSearch): the check of the RequiresRealAiProvider suites that each adapter still turns HostedWebSearchTool into the
// provider's own search, and that KNote's web_search reaches ollama.com with the key. Every run makes real searches
// (Anthropic bills each one).
internal static class AiWebSearchSmokeCheck
{
    public static async Task AssertTheModelSearchesTheInternetAsync(AiProviderRef providerRef)
    {
        var tools = AiWebSearch.ToolsFor(providerRef);
        Assert.AreNotEqual(0, tools.Count, $"{providerRef.Alias} cannot search the Internet.");
        var client = AiChatClientFactory.Create(providerRef, tools);

        var events = new List<AiChatStreamEventDto>();
        await foreach (var e in AiChatTurnStreamer.StreamAsync(client, AiWebSearch.SystemPrompt("You are a useful assistant.", providerRef, true), [],
            "Search the Internet: in which year was the Amstrad CPC 6128 released? Answer with the year and the link of your source.", tools))
            events.Add(e);

        var answer = events.Last(e => e.Type == AiChatStreamEventTypes.Completed).Turn!.Answer;
        Assert.IsTrue(events.Any(e => e.Type == AiChatStreamEventTypes.Tool && e.Text == AiWebSearchRules.WebSearchName),
            $"It did not search. It answered: {answer}");
        StringAssert.Contains(answer, "1985");
        StringAssert.Contains(answer, "http");
    }
}

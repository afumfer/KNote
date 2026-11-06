using System.Text.Json;
using KNote.Ai;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.Model.Dto;
using Microsoft.Extensions.AI;

namespace KNote.ClientWin.Tests;

/// <summary>
/// The reasoning of the model (KNote Atlas, delivery 1.3): apart from the answer, with its events and its time, shown folded
/// as plain text, never saved nor sent back. Its tokens, only when the provider tells them apart.
/// </summary>
[TestClass]
public class AiReasoningTests
{
    private static ChatResponseUpdate Update(params AIContent[] contents) => new(ChatRole.Assistant, contents);

    private static async Task<List<AiChatStreamEventDto>> StreamAsync(params ChatResponseUpdate[] updates)
    {
        async IAsyncEnumerable<ChatResponseUpdate> Stream()
        {
            foreach (var update in updates)
            {
                await Task.Yield();
                yield return update;
            }
        }

        var chatClient = new FakeChatClient { GetStreamingResponseImpl = (_, _, _) => Stream() };
        var events = new List<AiChatStreamEventDto>();
        await foreach (var e in AiChatTurnStreamer.StreamAsync(chatClient, "system", null, "prompt"))
            events.Add(e);
        return events;
    }

    private static AiChatTurnDto Turn(List<AiChatStreamEventDto> events) => events.Single(e => e.Type == AiChatStreamEventTypes.Completed).Turn;

    [TestMethod]
    public async Task Streamer_SeparatesTheReasoningFromTheAnswer()
    {
        var events = await StreamAsync(
            Update(new TextReasoningContent("First ")), Update(new TextReasoningContent("thought")), Update(new TextContent("The answer")));

        var reasoning = events.Where(e => e.Type == AiChatStreamEventTypes.Reasoning).Select(e => e.Text).ToList();
        CollectionAssert.AreEqual(new[] { "First ", "thought" }, reasoning);
        var turn = Turn(events);
        Assert.AreEqual("The answer", turn.Answer);
        Assert.AreEqual("First thought", turn.Reasoning);
        Assert.IsNotNull(turn.ReasoningTime);
        Assert.IsNull(turn.ReasoningTokens, "Not reported by this provider, and not estimated.");
    }

    [TestMethod]
    public async Task Streamer_ReasoningAgainAfterATool_IsPutAfterWhatItThoughtBefore()
    {
        var events = await StreamAsync(
            Update(new TextReasoningContent("Before the tool")),
            Update(new FunctionCallContent("1", "search_notes")),
            Update(new TextReasoningContent("After the tool")),
            Update(new TextContent("Done")));

        Assert.AreEqual("Before the tool\n\nAfter the tool", Turn(events).Reasoning);
        CollectionAssert.AreEqual(
            new[] { AiChatStreamEventTypes.Reasoning, AiChatStreamEventTypes.Tool, AiChatStreamEventTypes.Reasoning, AiChatStreamEventTypes.Delta, AiChatStreamEventTypes.Completed },
            events.Select(e => e.Type).ToList());
    }

    [TestMethod]
    public async Task Streamer_WithoutReasoning_TheTurnIsAsAlways()
    {
        var turn = Turn(await StreamAsync(Update(new TextContent("Hello"))));

        Assert.IsNull(turn.Reasoning);
        Assert.IsNull(turn.ReasoningTime);
        Assert.IsNull(turn.ReasoningTokens);
    }

    [TestMethod]
    public async Task Streamer_ReasoningWithoutText_IsIgnored()
    {
        var events = await StreamAsync(Update(new TextReasoningContent("") { ProtectedData = "encrypted" }), Update(new TextContent("Hello")));

        Assert.IsFalse(events.Any(e => e.Type == AiChatStreamEventTypes.Reasoning));
        Assert.IsNull(Turn(events).Reasoning);
    }

    [TestMethod]
    public async Task Streamer_ReasoningTokens_AddUpWhenTheProviderGivesThem()
    {
        var usage = new UsageContent(new UsageDetails { InputTokenCount = 3, OutputTokenCount = 10, TotalTokenCount = 13, ReasoningTokenCount = 7 });

        var turn = Turn(await StreamAsync(Update(usage), Update(new UsageContent(new UsageDetails { ReasoningTokenCount = 1 })), Update(new TextContent("Hi"))));

        Assert.AreEqual(8, turn.ReasoningTokens);
        StringAssert.Contains(turn.UsageSummary(), "8 reasoning");
    }

    [TestMethod]
    public void TheReasoning_IsNotSentWithTheTurn_NorSaved_AndItsTokensAre()
    {
        var turn = new AiChatTurnDto { Prompt = "Q", Answer = "A", Reasoning = "Secret thoughts", ReasoningTokens = 5, TotalTokens = 9 };

        var json = JsonSerializer.Serialize(turn);
        var transcript = AiChatSessionTranscript.Write(new[] { turn });
        var parsed = AiChatSessionTranscript.Parse(transcript).Single();

        Assert.IsFalse(json.Contains("Secret thoughts"), json);
        Assert.IsFalse(transcript.Contains("Secret thoughts"), transcript);
        Assert.AreEqual(5, parsed.ReasoningTokens);
        Assert.IsNull(parsed.Reasoning);
    }

    [TestMethod]
    public void AConversationSavedBefore_OpensAsItWas()
    {
        var turn = new AiChatTurnDto { Prompt = "Q", Answer = "A", TotalTokens = 9 };

        var transcript = AiChatSessionTranscript.Write(new[] { turn });
        var parsed = AiChatSessionTranscript.Parse(transcript).Single();

        Assert.IsFalse(transcript.Contains("reasoning", StringComparison.OrdinalIgnoreCase), transcript);
        Assert.IsNull(parsed.ReasoningTokens);
        Assert.AreEqual("A", parsed.Answer);
    }

    [TestMethod]
    public void ReasoningTitle_SaysHowLong()
    {
        Assert.AreEqual("Thought for 63 s", new AiChatTurnDto { ReasoningTime = TimeSpan.FromSeconds(63.2) }.ReasoningTitle());
        Assert.AreEqual("Thought for 1 s", new AiChatTurnDto { ReasoningTime = TimeSpan.FromMilliseconds(100) }.ReasoningTitle());
        Assert.AreEqual(AiChatTurnDto.ReasoningText, new AiChatTurnDto().ReasoningTitle());
    }

    [TestMethod]
    public void AiChatHtml_ShowsItFoldedAsPlainText()
    {
        var turn = new AiChatTurnDto { Answer = "Answer", Reasoning = "<b>raw</b> **text** #123", ReasoningTime = TimeSpan.FromSeconds(5) };

        var html = AiChatHtml.AssistantMessage(turn, "Provider");

        StringAssert.Contains(html, "<details class=\"reasoning\"><summary>Thought for 5 s</summary>");
        StringAssert.Contains(html, "&lt;b&gt;raw&lt;/b&gt; **text** #123");
        Assert.IsTrue(html.IndexOf("reasoning-text", StringComparison.Ordinal) < html.IndexOf("Answer", StringComparison.Ordinal), "Above the answer.");
        Assert.IsFalse(html.Contains("knt-note:"), "No links in it.");
    }

    [TestMethod]
    public void AiChatHtml_WhileItArrives_AndWithoutReasoning()
    {
        var pending = AiChatHtml.PendingAnswer("Provider", null, AiChatTurnDto.ThinkingText, "So far");
        var none = AiChatHtml.AssistantMessage(new AiChatTurnDto { Answer = "Answer" }, "Provider");

        StringAssert.Contains(pending, "<summary>Reasoning</summary>");
        StringAssert.Contains(pending, "So far");
        StringAssert.Contains(pending, AiChatTurnDto.ThinkingText);
        Assert.IsFalse(none.Contains("<details"), none);
    }
}

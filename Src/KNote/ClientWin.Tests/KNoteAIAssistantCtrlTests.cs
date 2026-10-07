using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.Model;
using Microsoft.Extensions.AI;

namespace KNote.ClientWin.Tests;

[TestClass]
public class KNoteAIAssistantCtrlTests
{
    private static KNoteAIAssistantCtrl CreateCtrl(FakeChatClient chatClient)
    {
        var store = new Store(new TestFactoryViews());
        var ctrl = new KNoteAIAssistantCtrl(store);
        ctrl.SetChatClientForTesting(chatClient);
        // Mirrors SetProvider(...): seeds ChatMessages with the system prompt. SetChatClientForTesting
        // itself doesn't do this, since it's meant to only swap the client, not force a reset.
        ctrl.RestartAIAssistant();
        return ctrl;
    }

    [TestMethod]
    public async Task GetCompletionAsync_Success_AppendsUserAndAssistantMessages()
    {
        var chatClient = new FakeChatClient
        {
            // Completion streams too (see GetCompletionAsync), and puts the answer together.
            GetStreamingResponseImpl = (messages, options, ct) => AnswerStream("Hi there",
                new UsageDetails { InputTokenCount = 3, OutputTokenCount = 2, TotalTokenCount = 5 })
        };
        var ctrl = CreateCtrl(chatClient);

        await ctrl.GetCompletionAsync("Hello");

        Assert.AreEqual("Hi there", ctrl.Result);
        Assert.AreEqual(5, ctrl.TotalTokens);
        // System + User + Assistant
        Assert.AreEqual(3, ctrl.ChatMessages.Count);
        StringAssert.Contains(ctrl.ChatTextMessages.ToString(), "Hi there");
    }

    [TestMethod]
    public async Task GetCompletionAsync_ProviderThrows_RollsBackTheUnansweredUserTurn()
    {
        var chatClient = new FakeChatClient
        {
            GetStreamingResponseImpl = (messages, options, ct) => throw new InvalidOperationException("simulated provider failure")
        };
        var ctrl = CreateCtrl(chatClient);
        var messagesBeforeSend = ctrl.ChatMessages.Count;

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => ctrl.GetCompletionAsync("Hello"));

        // No orphaned "User" message left over: a retry (or a provider switch) must not resend it
        // without a matching assistant reply.
        Assert.AreEqual(messagesBeforeSend, ctrl.ChatMessages.Count);
        Assert.AreEqual("", ctrl.ChatTextMessages.ToString());
    }

    [TestMethod]
    public async Task StreamCompletionAsync_Success_AppendsUserAndAssistantMessages()
    {
        var chatClient = new FakeChatClient
        {
            GetStreamingResponseImpl = (messages, options, ct) => StreamOf("Hi", " there")
        };
        var ctrl = CreateCtrl(chatClient);

        await ctrl.StreamCompletionAsync("Hello");

        Assert.AreEqual("Hi there", ctrl.Result);
        Assert.AreEqual(3, ctrl.ChatMessages.Count);
        StringAssert.Contains(ctrl.ChatTextMessages.ToString(), "Hi there");
    }

    [TestMethod]
    public async Task StreamCompletionAsync_ProviderThrowsMidStream_RollsBackUserTurnAndDanglingIntro()
    {
        var chatClient = new FakeChatClient
        {
            GetStreamingResponseImpl = (messages, options, ct) => StreamThatThrowsAfterOneChunk()
        };
        var ctrl = CreateCtrl(chatClient);
        var messagesBeforeSend = ctrl.ChatMessages.Count;
        var transcriptBeforeSend = ctrl.ChatTextMessages.ToString();

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => ctrl.StreamCompletionAsync("Hello"));

        Assert.AreEqual(messagesBeforeSend, ctrl.ChatMessages.Count);
        Assert.AreEqual(transcriptBeforeSend, ctrl.ChatTextMessages.ToString());
    }

    [TestMethod]
    public async Task GetCompletionAsync_Success_AddsATurnWithItsUsage()
    {
        var chatClient = new FakeChatClient
        {
            // Completion streams too (see GetCompletionAsync), and puts the answer together.
            GetStreamingResponseImpl = (messages, options, ct) => AnswerStream("Hi there",
                new UsageDetails { InputTokenCount = 3, OutputTokenCount = 2, TotalTokenCount = 5 })
        };
        var ctrl = CreateCtrl(chatClient);

        await ctrl.GetCompletionAsync("Hello");

        Assert.AreEqual(1, ctrl.ChatTurns.Count);
        var turn = ctrl.ChatTurns[0];
        Assert.AreEqual("Hello", turn.Prompt);
        Assert.AreEqual("Hi there", turn.Answer);
        Assert.AreEqual(3L, turn.InputTokens);
        Assert.AreEqual(2L, turn.OutputTokens);
        Assert.AreEqual(5L, turn.TotalTokens);
        Assert.IsFalse(turn.TokensEstimated);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task GetCompletionAsync_AnswerStoppedByTheTokenLimit_MarksTheTurnAsTruncated(bool cut)
    {
        var chatClient = new FakeChatClient
        {
            GetStreamingResponseImpl = (messages, options, ct) => AnswerStream("Once upon a",
                finishReason: cut ? ChatFinishReason.Length : ChatFinishReason.Stop)
        };
        var ctrl = CreateCtrl(chatClient);

        await ctrl.GetCompletionAsync("Tell me a long story");

        Assert.AreEqual(cut, ctrl.ChatTurns[0].Truncated);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task StreamCompletionAsync_AnswerStoppedByTheTokenLimit_MarksTheTurnAsTruncated(bool cut)
    {
        var chatClient = new FakeChatClient
        {
            GetStreamingResponseImpl = (messages, options, ct) => AnswerStream("Once upon a",
                finishReason: cut ? ChatFinishReason.Length : ChatFinishReason.Stop)
        };
        var ctrl = CreateCtrl(chatClient);

        await ctrl.StreamCompletionAsync("Tell me a long story");

        Assert.AreEqual(cut, ctrl.ChatTurns[0].Truncated);
    }

    [TestMethod]
    public async Task GetCompletionAsync_ProviderThrows_AddsNoTurn()
    {
        var chatClient = new FakeChatClient
        {
            GetStreamingResponseImpl = (messages, options, ct) => throw new InvalidOperationException("simulated provider failure")
        };
        var ctrl = CreateCtrl(chatClient);

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => ctrl.GetCompletionAsync("Hello"));

        Assert.AreEqual(0, ctrl.ChatTurns.Count);
    }

    [TestMethod]
    public async Task StreamCompletionAsync_Success_AddsATurnWithEstimatedTokens()
    {
        var chatClient = new FakeChatClient
        {
            GetStreamingResponseImpl = (messages, options, ct) => StreamOf("Hi", " there")
        };
        var ctrl = CreateCtrl(chatClient);

        await ctrl.StreamCompletionAsync("Hello");
        await ctrl.StreamCompletionAsync("Again");

        Assert.AreEqual(2, ctrl.ChatTurns.Count);
        Assert.AreEqual("Hello", ctrl.ChatTurns[0].Prompt);
        Assert.AreEqual("Again", ctrl.ChatTurns[1].Prompt);
        Assert.AreEqual("Hi there", ctrl.ChatTurns[1].Answer);
        Assert.IsTrue(ctrl.ChatTurns[1].TokensEstimated);
    }

    [TestMethod]
    public async Task StreamCompletionAsync_WhileStreaming_StreamingResultHasTheAnswerSoFar()
    {
        var chatClient = new FakeChatClient
        {
            GetStreamingResponseImpl = (messages, options, ct) => StreamOf("Hi", " there")
        };
        var ctrl = CreateCtrl(chatClient);
        var seen = new List<string>();
        ctrl.StreamToken += (s, e) => seen.Add(ctrl.StreamingResult);

        await ctrl.StreamCompletionAsync("Hello");

        // First the intro (nothing received yet), then each chunk, then the closing line break.
        CollectionAssert.AreEqual(new[] { "", "Hi", "Hi there", "Hi there" }, seen);
    }

    [TestMethod]
    public async Task StreamCompletionAsync_NextStream_StartsFromAnEmptyStreamingResult()
    {
        var chatClient = new FakeChatClient
        {
            GetStreamingResponseImpl = (messages, options, ct) => StreamOf("Hi", " there")
        };
        var ctrl = CreateCtrl(chatClient);
        await ctrl.StreamCompletionAsync("Hello");
        string atStart = null;
        ctrl.StreamToken += (s, e) => atStart ??= ctrl.StreamingResult;

        await ctrl.StreamCompletionAsync("Again");

        Assert.AreEqual("", atStart);
    }

    [TestMethod]
    public async Task StreamCompletionAsync_ProviderThrowsMidStream_AddsNoTurnButKeepsThePartialAnswer()
    {
        var chatClient = new FakeChatClient
        {
            GetStreamingResponseImpl = (messages, options, ct) => StreamThatThrowsAfterOneChunk()
        };
        var ctrl = CreateCtrl(chatClient);

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => ctrl.StreamCompletionAsync("Hello"));

        Assert.AreEqual(0, ctrl.ChatTurns.Count);
        // What the view shows of the failed answer.
        Assert.AreEqual("partial", ctrl.StreamingResult);
    }

    [TestMethod]
    public void RestartAIAssistant_ClearsHistoryAndCounters()
    {
        var ctrl = CreateCtrl(new FakeChatClient());
        ctrl.RootSystemChat = "custom system prompt";

        ctrl.RestartAIAssistant();

        Assert.AreEqual(1, ctrl.ChatMessages.Count); // just the system message
        Assert.AreEqual("", ctrl.ChatTextMessages.ToString());
        Assert.AreEqual(0, ctrl.TotalTokens);
        Assert.AreEqual(TimeSpan.Zero, ctrl.TotalProcessingTime);
    }

    [TestMethod]
    public async Task RestartAIAssistant_ClearsTurnsAndStreamingResult()
    {
        var chatClient = new FakeChatClient
        {
            GetStreamingResponseImpl = (messages, options, ct) => StreamOf("Hi", " there")
        };
        var ctrl = CreateCtrl(chatClient);
        await ctrl.StreamCompletionAsync("Hello");

        ctrl.RestartAIAssistant();

        Assert.AreEqual(0, ctrl.ChatTurns.Count);
        Assert.AreEqual("", ctrl.StreamingResult);
    }

    [TestMethod]
    [DataRow(false, false)]
    [DataRow(true, true)]
    public void MarkdownResultView_ReadsTheRememberedMode(bool remembered, bool expected)
    {
        // Only read here: setting it saves the real config files of the user (see CreateCtrlWithProviders).
        var store = new Store(new TestFactoryViews());
        store.State.Session.AiAssistantMarkdownView = remembered;

        Assert.AreEqual(expected, new KNoteAIAssistantCtrl(store).MarkdownResultView);
    }

    // GetPreferredProvider only reads the config; SetProvider is deliberately not exercised here because it
    // persists the choice with Store.SaveConfig(), which writes the real KNoteData.config of the user.
    private static KNoteAIAssistantCtrl CreateCtrlWithProviders(string lastAlias, params string[] aliases)
    {
        var store = new Store(new TestFactoryViews());
        foreach (var alias in aliases)
            store.Settings.Ai.Providers.Add(new AiProviderRef { Alias = alias, Provider = EnumAiProvider.Ollama, Model = "m", Host = "http://localhost" });
        store.State.Session.LastAiProviderAlias = lastAlias;
        return new KNoteAIAssistantCtrl(store);
    }

    [TestMethod]
    public void GetPreferredProvider_LastAliasStillConfigured_ReturnsIt()
    {
        var ctrl = CreateCtrlWithProviders("second", "first", "second");

        Assert.AreEqual("second", ctrl.GetPreferredProvider().Alias);
    }

    [TestMethod]
    public void GetPreferredProvider_AliasComparisonIgnoresCase()
    {
        var ctrl = CreateCtrlWithProviders("SECOND", "first", "second");

        Assert.AreEqual("second", ctrl.GetPreferredProvider().Alias);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("removed provider")]
    public void GetPreferredProvider_NoUsableLastAlias_FallsBackToTheFirstProvider(string lastAlias)
    {
        var ctrl = CreateCtrlWithProviders(lastAlias, "first", "second");

        Assert.AreEqual("first", ctrl.GetPreferredProvider().Alias);
    }

    [TestMethod]
    public void GetPreferredProvider_NoProviders_ReturnsNull()
    {
        var ctrl = CreateCtrlWithProviders("anything");

        Assert.IsNull(ctrl.GetPreferredProvider());
    }

    // An answer as providers stream it: its text, then a last update with why it stopped and its usage.
    private static async IAsyncEnumerable<ChatResponseUpdate> AnswerStream(string text, UsageDetails usage = null,
        ChatFinishReason? finishReason = null)
    {
        await Task.Yield();
        yield return new ChatResponseUpdate(ChatRole.Assistant, text);
        yield return new ChatResponseUpdate
        {
            Role = ChatRole.Assistant,
            FinishReason = finishReason ?? ChatFinishReason.Stop,
            Contents = usage == null ? [] : [new UsageContent(usage)]
        };
    }

    private static async IAsyncEnumerable<ChatResponseUpdate> StreamOf(params string[] chunks)
    {
        foreach (var chunk in chunks)
        {
            await Task.Yield();
            yield return new ChatResponseUpdate(ChatRole.Assistant, chunk);
        }
    }

    private static async IAsyncEnumerable<ChatResponseUpdate> StreamThatThrowsAfterOneChunk()
    {
        await Task.Yield();
        yield return new ChatResponseUpdate(ChatRole.Assistant, "partial");
        throw new InvalidOperationException("simulated provider failure mid-stream");
    }
}

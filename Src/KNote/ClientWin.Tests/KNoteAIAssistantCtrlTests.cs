using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
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
            GetResponseImpl = (messages, options, ct) =>
                Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, "Hi there"))
                {
                    Usage = new UsageDetails { InputTokenCount = 3, OutputTokenCount = 2, TotalTokenCount = 5 }
                })
        };
        var ctrl = CreateCtrl(chatClient);

        await ctrl.GetCompletionAsync("Hello");

        Assert.AreEqual("Hi there", ctrl.Result);
        Assert.AreEqual(5, ctrl.TotalTokens);
        // System + User + Assistant
        Assert.AreEqual(3, ctrl.ChatMessages.Count);
        StringAssert.Contains(ctrl.ChatTextMessasges.ToString(), "Hi there");
    }

    [TestMethod]
    public async Task GetCompletionAsync_ProviderThrows_RollsBackTheUnansweredUserTurn()
    {
        var chatClient = new FakeChatClient
        {
            GetResponseImpl = (messages, options, ct) => throw new InvalidOperationException("simulated provider failure")
        };
        var ctrl = CreateCtrl(chatClient);
        var messagesBeforeSend = ctrl.ChatMessages.Count;

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => ctrl.GetCompletionAsync("Hello"));

        // No orphaned "User" message left over: a retry (or a provider switch) must not resend it
        // without a matching assistant reply.
        Assert.AreEqual(messagesBeforeSend, ctrl.ChatMessages.Count);
        Assert.AreEqual("", ctrl.ChatTextMessasges.ToString());
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
        StringAssert.Contains(ctrl.ChatTextMessasges.ToString(), "Hi there");
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
        var transcriptBeforeSend = ctrl.ChatTextMessasges.ToString();

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => ctrl.StreamCompletionAsync("Hello"));

        Assert.AreEqual(messagesBeforeSend, ctrl.ChatMessages.Count);
        Assert.AreEqual(transcriptBeforeSend, ctrl.ChatTextMessasges.ToString());
    }

    [TestMethod]
    public void RestartAIAssistant_ClearsHistoryAndCounters()
    {
        var ctrl = CreateCtrl(new FakeChatClient());
        ctrl.RootSystemChat = "custom system prompt";

        ctrl.RestartAIAssistant();

        Assert.AreEqual(1, ctrl.ChatMessages.Count); // just the system message
        Assert.AreEqual("", ctrl.ChatTextMessasges.ToString());
        Assert.AreEqual(0, ctrl.TotalTokens);
        Assert.AreEqual(TimeSpan.Zero, ctrl.TotalProcessingTime);
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

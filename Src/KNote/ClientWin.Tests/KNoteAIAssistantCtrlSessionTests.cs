using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.ClientWin.Tests.Helpers;
using KNote.Model;
using KNote.Model.Dto;
using Microsoft.Extensions.AI;

namespace KNote.ClientWin.Tests;

/// <summary>
/// KNoteAIAssistantCtrl's sessions (IKntService.AiSessions of its ServiceRef): saved after each answer only when
/// PersistSession is on, saved again before leaving them when that failed, and resumed with their provider.
/// </summary>
[TestClass]
public class KNoteAIAssistantCtrlSessionTests
{
    private static readonly AiProviderRef OpenAi = new() { Alias = "OpenAI test", Provider = EnumAiProvider.OpenAI, Model = "gpt-test", ApiKey = "placeholder" };
    private static readonly AiProviderRef Claude = new() { Alias = "Claude test", Provider = EnumAiProvider.Anthropic, Model = "claude-test", ApiKey = "placeholder" };

    private static Result<T> Valid<T>(T entity) => new() { Entity = entity };

    private static Result<T> Invalid<T>(string message)
    {
        var result = new Result<T>();
        result.AddErrorMessage(message);
        return result;
    }

    private sealed record Setup(KNoteAIAssistantCtrl Ctrl, FakeKntAiSessionService Sessions, FakeAIAssistantView View);

    private static Setup CreateCtrl(bool persistSession)
    {
        var store = new Store(new TestFactoryViews());
        store.Settings.Ai.Providers.Add(OpenAi);
        store.Settings.Ai.Providers.Add(Claude);
        var view = new FakeAIAssistantView();
        store.FactoryViews.Registry.Register<KNoteAIAssistantCtrl, IViewBase>(_ => view);
        var service = new FakeKntService();
        store.AddServiceRef(TestServiceRefFactory.CreateWithFakeService(service));

        var ctrl = new KNoteAIAssistantCtrl(store) { PersistSession = persistSession };
        ctrl.SetChatClientForTesting(new FakeChatClient
        {
            GetStreamingResponseImpl = (messages, options, ct) => StreamOf("An answer")
        }, OpenAi);
        ctrl.RestartAIAssistant();
        return new Setup(ctrl, service.AiSessionsFake, view);
    }

    [TestMethod]
    public async Task PersistSessionOff_NothingIsSaved()
    {
        var (ctrl, sessions, _) = CreateCtrl(persistSession: false);
        // SaveAsyncImpl left unset: any save would throw NotSupportedException.

        await ctrl.GetCompletionAsync("Hello");
        await ctrl.StreamCompletionAsync("Again");

        Assert.AreEqual(2, ctrl.ChatTurns.Count);
        Assert.AreEqual(Guid.Empty, ctrl.SessionNoteId);
    }

    [TestMethod]
    public async Task PersistSessionOn_EachAnswerSavesTheSession_WithItsTurnsAndProvider()
    {
        var (ctrl, sessions, _) = CreateCtrl(persistSession: true);
        var noteId = Guid.NewGuid();
        var saved = new List<AiChatSessionDto>();
        sessions.SaveAsyncImpl = session =>
        {
            saved.Add(new AiChatSessionDto { NoteId = session.NoteId, Provider = session.Provider, Model = session.Model, Turns = session.Turns.ToList() });
            return Task.FromResult(Valid(new AiChatSessionDto { NoteId = noteId, NoteNumber = 7, Topic = "Hello" }));
        };

        await ctrl.GetCompletionAsync("Hello");
        await ctrl.StreamCompletionAsync("Again");

        Assert.AreEqual(2, saved.Count);
        Assert.AreEqual(Guid.Empty, saved[0].NoteId, "The first answer creates the session.");
        Assert.AreEqual(noteId, saved[1].NoteId, "The next ones update it.");
        CollectionAssert.AreEqual(new[] { "Hello", "Again" }, saved[1].Turns.Select(t => t.Prompt).ToList());
        Assert.AreEqual(EnumAiProvider.OpenAI, saved[1].Provider);
        Assert.AreEqual("gpt-test", saved[1].Model);
        Assert.AreEqual(noteId, ctrl.SessionNoteId);
        Assert.AreEqual("Hello", ctrl.SessionTopic);
        Assert.IsFalse(ctrl.SessionPendingSave);
    }

    [TestMethod]
    public async Task ASaveThatFails_IsRetriedBeforeLeaving_AndTheUserDecidesWhetherToDiscard()
    {
        var (ctrl, sessions, view) = CreateCtrl(persistSession: true);
        sessions.SaveAsyncImpl = _ => Task.FromResult(Invalid<AiChatSessionDto>("database down"));

        await ctrl.GetCompletionAsync("Hello");

        Assert.IsTrue(ctrl.SessionPendingSave);
        StringAssert.Contains(view.LastShownInfo, "database down");

        // Kept: the user doesn't want to lose it.
        view.NextShowInfoResult = DialogResult.No;
        Assert.IsFalse(await ctrl.NewSessionAsync());
        Assert.AreEqual(1, ctrl.ChatTurns.Count);

        // Saved this time: a new conversation starts.
        sessions.SaveAsyncImpl = _ => Task.FromResult(Valid(new AiChatSessionDto { NoteId = Guid.NewGuid() }));
        Assert.IsTrue(await ctrl.NewSessionAsync());
        Assert.AreEqual(0, ctrl.ChatTurns.Count);
        Assert.AreEqual(Guid.Empty, ctrl.SessionNoteId);
    }

    [TestMethod]
    public async Task OpenSession_ResumesItsTurns_WithItsProvider()
    {
        var (ctrl, sessions, _) = CreateCtrl(persistSession: true);
        var noteId = Guid.NewGuid();
        sessions.GetAsyncImpl = id => Task.FromResult(Valid(new AiChatSessionDto
        {
            NoteId = id,
            Topic = "Old session",
            Provider = EnumAiProvider.Anthropic,
            Model = "claude-test",
            Turns =
            [
                new AiChatTurnDto { Prompt = "First", Answer = "One", TotalTokens = 4 },
                new AiChatTurnDto { Prompt = "Second", Answer = "Two", TotalTokens = 6 }
            ]
        }));

        Assert.IsTrue(await ctrl.OpenSessionAsync(noteId));

        Assert.AreSame(Claude, ctrl.CurrentProviderRef);
        Assert.AreEqual(noteId, ctrl.SessionNoteId);
        Assert.AreEqual("Old session", ctrl.SessionTopic);
        CollectionAssert.AreEqual(new[] { "First", "Second" }, ctrl.ChatTurns.Select(t => t.Prompt).ToList());
        Assert.AreEqual(10, ctrl.TotalTokens);
        // System + two questions and answers: what is sent with the next prompt.
        Assert.AreEqual(5, ctrl.ChatMessages.Count);
        Assert.IsFalse(ctrl.SessionPendingSave);
    }

    [TestMethod]
    public async Task OpenSession_ModelNoLongerConfigured_ContinuesWithThePreferredProvider_AndSaysSo()
    {
        var (ctrl, sessions, view) = CreateCtrl(persistSession: true);
        sessions.GetAsyncImpl = id => Task.FromResult(Valid(new AiChatSessionDto
        {
            NoteId = id,
            Provider = EnumAiProvider.Ollama,
            Model = "retired-model",
            Turns = [new AiChatTurnDto { Prompt = "First", Answer = "One" }]
        }));

        Assert.IsTrue(await ctrl.OpenSessionAsync(Guid.NewGuid()));

        Assert.AreSame(ctrl.GetPreferredProvider(), ctrl.CurrentProviderRef);
        StringAssert.Contains(view.LastShownInfo, "retired-model");
        Assert.AreEqual(1, ctrl.ChatTurns.Count);
    }

    private static async IAsyncEnumerable<ChatResponseUpdate> StreamOf(params string[] chunks)
    {
        foreach (var chunk in chunks)
        {
            await Task.Yield();
            yield return new ChatResponseUpdate(ChatRole.Assistant, chunk);
        }
    }
}

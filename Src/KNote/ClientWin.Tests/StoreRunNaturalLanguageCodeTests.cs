using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Store.RunNaturalLanguageCode ("ln" script engine): sends the note's Script as a prompt through
/// KNoteAIAssistantCtrl. Its GetCompletionAsync/rollback behavior is already fully covered by
/// KNoteAIAssistantCtrlTests via SetChatClientForTesting - that seam isn't reachable from here, since
/// RunNaturalLanguageCode constructs its own KNoteAIAssistantCtrl internally and always goes through
/// the real SetProvider/AiChatClientFactory path (no AiProviderRefs configured -> OnInitialized fails
/// before ever touching a chat client, real or fake). So this test only covers what actually is
/// reachable from here: the no-provider-configured guard that keeps GetCompletionAsync from being
/// called against a null chat client. The happy path (a real provider answering and the assistant
/// view opening) has no fake-able seam from Store and must be verified by hand - see
/// ClientWin.Tests/CLAUDE.md's note on KNoteAiTools.CreateTaskAsync for the same kind of gap.
/// </summary>
[TestClass]
public class StoreRunNaturalLanguageCodeTests
{
    [TestMethod]
    public async Task RunNaturalLanguageCode_NoProvidersConfigured_ReturnsWithoutShowingAssistantView()
    {
        var store = new Store(new TestFactoryViews());
        var fakeView = new FakeAIAssistantView();
        store.FactoryViews.Registry.Register<KNoteAIAssistantCtrl, IViewBase>(_ => fakeView);

        await store.RunNaturalLanguageCode("Summarize this note.");

        Assert.AreEqual(0, fakeView.ShowViewCallCount, "The assistant chat view must not open when no AI provider is configured.");
        StringAssert.Contains(fakeView.LastShownInfo, "No AI providers are configured");
    }

    [TestMethod]
    public async Task RunNaturalLanguageCode_EmptyPrompt_DoesNothing()
    {
        var store = new Store(new TestFactoryViews());
        var fakeView = new FakeAIAssistantView();
        store.FactoryViews.Registry.Register<KNoteAIAssistantCtrl, IViewBase>(_ => fakeView);

        await store.RunNaturalLanguageCode("");

        Assert.AreEqual(0, fakeView.ShowViewCallCount);
        Assert.IsNull(fakeView.LastShownInfo);
    }
}

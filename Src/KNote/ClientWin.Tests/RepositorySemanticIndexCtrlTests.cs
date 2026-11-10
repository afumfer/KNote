using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.ClientWin.Tests.Helpers;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Interfaces;

namespace KNote.ClientWin.Tests;

/// <summary>
/// RepositorySemanticIndexCtrl (Â«Semantic indexÂ» of the KNote Atlas window) and the Store's run of the index
/// (SemanticIndexJob): the status it shows, a run started after saying what it does, one run at a time in the app, its
/// cancellation, the status read again when it ends, and removing the index of another model.
/// </summary>
[TestClass]
public class RepositorySemanticIndexCtrlTests
{
    private static SemanticIndexStatusDto Status(EnumSemanticIndexStatus status = EnumSemanticIndexStatus.Ready, int pending = 3,
        bool available = true, string model = "qwen3-embedding:0.6b") => new()
    {
        EmbeddingModel = model,
        IndexKey = model,
        EmbeddingModelAvailable = available,
        Status = status,
        TotalNotes = 10,
        IndexedNotes = 10 - pending,
        PendingNotes = pending,
        Chunks = 25,
        NotesPerMinute = 60,
        OtherIndexes = new() { new SemanticIndexModelDto { IndexKey = "old-model", Chunks = 40, Notes = 10 } }
    };

    private static (RepositorySemanticIndexCtrl ctrl, FakeSemanticIndexView view, FakeKntService service, Store store) CreateCtrl(
        SemanticIndexStatusDto? status = null, Store? sharedStore = null)
    {
        var store = sharedStore;
        var view = new FakeSemanticIndexView();
        if (store == null)
        {
            var factoryViews = new TestFactoryViews();
            store = new Store(factoryViews);
            store.Settings.Ai.Providers.Add(new AiProviderRef { Alias = "Paid embeddings", Provider = EnumAiProvider.OpenAI,
                Model = "text-embedding-3-small", Kind = EnumAiProviderKind.Embeddings });
            // Every controller of this store gets this view.
            store.FactoryViews.Registry.Register<RepositorySemanticIndexCtrl, IViewSemanticIndex>(c => view);
        }

        var service = new FakeKntService { RepositoryRef = new RepositoryRef { Name = "Notes" } };
        var current = status ?? Status();
        service.SemanticSearchFake.GetIndexStatusAsyncImpl = () => Task.FromResult(new Result<SemanticIndexStatusDto>(current));
        store.AddServiceRef(TestServiceRefFactory.CreateWithFakeService(service));
        store.GrantAdmin(service);

        return (new RepositorySemanticIndexCtrl(store), view, service, store);
    }

    [TestMethod]
    public async Task Load_ReadsTheStatus_AndShowsIt()
    {
        var (ctrl, view, service, _) = CreateCtrl(Status(pending: 3));

        Assert.IsTrue(await ctrl.LoadAsync(service, "Notes"));

        Assert.AreEqual("Ready · model qwen3-embedding:0.6b", ctrl.StatusText);
        Assert.AreEqual("7 of 10 notes indexed · 3 pending · 25 fragments", ctrl.CountsText);
        Assert.AreEqual("&Update now", ctrl.RunText);
        Assert.IsTrue(ctrl.CanUpdate);
        Assert.IsTrue(ctrl.CanRebuild);
        Assert.AreEqual("", ctrl.WarningText);
        Assert.IsTrue(view.RefreshViewCount > 0);
    }

    [TestMethod]
    public async Task WithoutTheModelHere_OrWithoutPendingNotes_NothingToRun()
    {
        var (ctrl, _, service, _) = CreateCtrl(Status(available: false));
        await ctrl.LoadAsync(service, "Notes");
        Assert.IsFalse(ctrl.CanUpdate);
        Assert.IsFalse(ctrl.CanRebuild);
        StringAssert.Contains(ctrl.WarningText, "no embeddings provider");
        Assert.IsFalse(ctrl.Run(rebuild: false));

        var (upToDate, _, upToDateService, _) = CreateCtrl(Status(pending: 0));
        await upToDate.LoadAsync(upToDateService, "Notes");
        Assert.IsFalse(upToDate.CanUpdate);
        Assert.IsTrue(upToDate.CanRebuild);
    }

    [TestMethod]
    public async Task Run_SaysWhatItDoes_AndCanceledStartsNothing()
    {
        var (ctrl, view, service, store) = CreateCtrl(Status(EnumSemanticIndexStatus.None, pending: 10, model: "text-embedding-3-small"));
        await ctrl.LoadAsync(service, "Notes");
        view.ShowInfoImpl = (_, _) => DialogResult.Cancel;

        Assert.IsFalse(ctrl.Run(rebuild: false));

        StringAssert.Contains(view.ShownInfo.Single(), "10 notes with the model text-embedding-3-small");
        StringAssert.Contains(view.ShownInfo.Single(), "paid model");
        StringAssert.Contains(view.ShownInfo.Single(), "about 10 s");
        Assert.IsFalse(store.SemanticIndex.IsRunning);
    }

    [TestMethod]
    public async Task Run_StartsTheStoresRun_WhichReadsTheStatusAgainWhenItEnds()
    {
        var (ctrl, view, service, store) = CreateCtrl();
        await ctrl.LoadAsync(service, "Notes");
        var release = new TaskCompletionSource<Result<SemanticIndexProgressDto>>(TaskCreationOptions.RunContinuationsAsynchronously);
        SemanticIndexRunRequest? request = null;
        service.SemanticSearchFake.RunIndexAsyncImpl = r =>
        {
            request = r;
            r.Progress.Report(new SemanticIndexProgressDto { Phase = EnumSemanticIndexPhase.Indexing, NotesTotal = 3, NotesDone = 1 });
            return release.Task;
        };
        var statusReads = 0;
        service.SemanticSearchFake.GetIndexStatusAsyncImpl = () =>
        {
            Interlocked.Increment(ref statusReads);
            return Task.FromResult(new Result<SemanticIndexStatusDto>(Status(pending: 0)));
        };

        Assert.IsTrue(ctrl.Run(rebuild: true));
        await WaitForAsync(() => request != null);

        Assert.IsTrue(request!.Rebuild);
        Assert.IsTrue(ctrl.IsRunning);
        Assert.IsFalse(ctrl.CanUpdate, "one run at a time");
        StringAssert.StartsWith(store.SemanticIndex.StatusBarText, "Semantic index of Notes");

        release.SetResult(new Result<SemanticIndexProgressDto>(new SemanticIndexProgressDto { Phase = EnumSemanticIndexPhase.Completed, NotesDone = 3 }));
        await store.SemanticIndex.Completion;
        await WaitForAsync(() => statusReads > 0);

        Assert.IsFalse(ctrl.IsRunning);
        Assert.IsNull(store.SemanticIndex.StatusBarText);
        Assert.AreEqual(EnumSemanticIndexPhase.Completed, ctrl.Progress!.Phase);
        Assert.AreEqual("Done: 3 notes in 1 s.", ctrl.ProgressText);
        Assert.IsTrue(view.RefreshProgressCount > 0);
    }

    [TestMethod]
    public async Task AnotherRepositorysRun_LeavesThisOneWaiting()
    {
        var (first, _, firstService, store) = CreateCtrl();
        var (second, _, secondService, _) = CreateCtrl(sharedStore: store);
        await first.LoadAsync(firstService, "First");
        await second.LoadAsync(secondService, "Second");
        var release = new TaskCompletionSource<Result<SemanticIndexProgressDto>>(TaskCreationOptions.RunContinuationsAsynchronously);
        firstService.SemanticSearchFake.RunIndexAsyncImpl = _ => release.Task;

        Assert.IsTrue(first.Run(rebuild: false));

        Assert.IsTrue(second.OtherRepositoryRunning);
        Assert.IsFalse(second.CanUpdate);
        StringAssert.Contains(second.WarningText, "The index of First is being created");
        Assert.IsNull(second.Progress);
        Assert.IsFalse(second.Run(rebuild: false));

        release.SetResult(new Result<SemanticIndexProgressDto>(new SemanticIndexProgressDto { Phase = EnumSemanticIndexPhase.Completed }));
        await store.SemanticIndex.Completion;
        Assert.IsFalse(second.OtherRepositoryRunning);
    }

    [TestMethod]
    public async Task CancelRun_CancelsTheStoresRun()
    {
        var (ctrl, _, service, store) = CreateCtrl();
        await ctrl.LoadAsync(service, "Notes");
        service.SemanticSearchFake.RunIndexAsyncImpl = async r =>
        {
            try
            {
                await Task.Delay(Timeout.Infinite, r.CancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
            return new Result<SemanticIndexProgressDto>(new SemanticIndexProgressDto { Phase = EnumSemanticIndexPhase.Canceled, NotesDone = 2 });
        };

        ctrl.Run(rebuild: false);
        ctrl.CancelRun();
        await store.SemanticIndex.Completion;

        Assert.AreEqual(EnumSemanticIndexPhase.Canceled, ctrl.Progress!.Phase);
        StringAssert.Contains(ctrl.ProgressText, "Canceled after 2 notes");
    }

    [TestMethod]
    public async Task AFailedRun_SaysWhy()
    {
        var (ctrl, _, service, store) = CreateCtrl();
        await ctrl.LoadAsync(service, "Notes");
        service.SemanticSearchFake.RunIndexAsyncImpl = _ => throw new InvalidOperationException("Ollama is not running.");

        ctrl.Run(rebuild: false);
        await store.SemanticIndex.Completion;

        Assert.AreEqual("Failed: Ollama is not running.", ctrl.ProgressText);
    }

    [TestMethod]
    [DataRow(DialogResult.Yes, true)]
    [DataRow(DialogResult.No, false)]
    public async Task DeleteIndex_AsksFirst(DialogResult answer, bool removed)
    {
        var (ctrl, view, service, _) = CreateCtrl();
        await ctrl.LoadAsync(service, "Notes");
        view.ShowInfoImpl = (_, _) => answer;
        string? deleted = null;
        service.SemanticSearchFake.DeleteModelIndexAsyncImpl = key =>
        {
            deleted = key;
            return Task.FromResult(new Result());
        };

        Assert.AreEqual(removed, await ctrl.DeleteIndexAsync("old-model"));

        Assert.AreEqual(removed ? "old-model" : null, deleted);
        StringAssert.Contains(view.ShownInfo.Single(), "old-model (40 fragments)");
    }

    private static async Task WaitForAsync(Func<bool> condition)
    {
        for (var i = 0; i < 200 && !condition(); i++)
            await Task.Delay(10);
        Assert.IsTrue(condition(), "timed out");
    }
}

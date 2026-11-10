using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.ClientWin.Tests.Helpers;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Interfaces;

namespace KNote.ClientWin.Tests;

/// <summary>
/// RepositoryResourcesCtrl (Â«Process pending resourcesÂ» of the KNote Atlas window, KNote Atlas 4.3) and the Store's run of «Process pending
/// resources» (ResourceProcessingJob): what it shows, a run started after saying what it does (with which model, whether it is
/// paid), the choice for the descriptions written by people, one run of KNote Atlas at a time in the app (also with the index),
/// and the update of the semantic index when a run describes something.
/// </summary>
[TestClass]
public class RepositoryResourcesCtrlTests
{
    private static ResourceProcessingStatusDto Status(int pending = 3, int person = 2, double? pace = 3) =>
        new() { PendingPhotos = pending, PersonPhotos = person, SecondsPerPhoto = pace, LastRunDateTime = pace == null ? null : new DateTime(2026, 11, 9, 10, 0, 0) };

    private static (RepositoryResourcesCtrl ctrl, FakeResourceProcessingView view, FakeKntService service, Store store) CreateCtrl(
        ResourceProcessingStatusDto? status = null)
    {
        var view = new FakeResourceProcessingView();
        var store = new Store(new TestFactoryViews());
        store.Settings.Ai.Providers.Add(new AiProviderRef { Alias = "Paid chat", Provider = EnumAiProvider.OpenAI, Model = "gpt-test", ApiKey = "key" });
        store.FactoryViews.Registry.Register<RepositoryResourcesCtrl, IViewResourceProcessing>(c => view);
        store.FactoryViews.Registry.Register<RepositorySemanticIndexCtrl, IViewSemanticIndex>(c => new FakeSemanticIndexView());

        var service = new FakeKntService { RepositoryRef = new RepositoryRef { Name = "Notes" } };
        var current = status ?? Status();
        service.ResourceContentFake.GetProcessingStatusAsyncImpl = () => Task.FromResult(new Result<ResourceProcessingStatusDto>(current));
        service.SystemValuesFake.GetRepositoryAiModelsAsyncImpl = () => Task.FromResult(new Result<RepositoryAiModelsDto>(new RepositoryAiModelsDto()));
        service.SemanticSearchFake.GetIndexStatusAsyncImpl = () => Task.FromResult(new Result<SemanticIndexStatusDto>(new SemanticIndexStatusDto
        {
            EmbeddingModel = "qwen3-embedding:0.6b", IndexKey = "qwen3-embedding:0.6b", EmbeddingModelAvailable = true,
            Status = EnumSemanticIndexStatus.Ready, TotalNotes = 5, IndexedNotes = 5
        }));
        store.AddServiceRef(TestServiceRefFactory.CreateWithFakeService(service));
        store.GrantAdmin(service);

        return (new RepositoryResourcesCtrl(store), view, service, store);
    }

    [TestMethod]
    public async Task Load_ShowsWhatIsPending_AndTheDescriptionsOfPeople()
    {
        var (ctrl, view, service, _) = CreateCtrl();

        Assert.IsTrue(await ctrl.LoadAsync(service, "Notes"));

        Assert.AreEqual("3 photos without a description of its own · 2 with a description written by a person, not locked", ctrl.CountsText);
        StringAssert.StartsWith(ctrl.LastRunText, "Last run: ");
        Assert.IsTrue(ctrl.HasPersonPhotos);
        Assert.IsTrue(ctrl.CanRun);
        Assert.IsTrue(view.RefreshViewCount > 0);
    }

    [TestMethod]
    public async Task NothingPending_NothingToRun_ButLockingIsSomething()
    {
        var (ctrl, _, service, _) = CreateCtrl(Status(pending: 0, person: 2));
        await ctrl.LoadAsync(service, "Notes");

        Assert.IsFalse(ctrl.CanRun);
        ctrl.PersonDescriptions = EnumPersonDescriptions.Lock;
        Assert.IsTrue(ctrl.CanRun);
        ctrl.PersonDescriptions = EnumPersonDescriptions.Replace;
        Assert.IsTrue(ctrl.CanRun);
    }

    [TestMethod]
    public async Task Run_SaysWhatItDoes_AndCanceledStartsNothing()
    {
        var (ctrl, view, service, store) = CreateCtrl();
        await ctrl.LoadAsync(service, "Notes");
        ctrl.PersonDescriptions = EnumPersonDescriptions.Replace;
        view.ShowInfoImpl = (_, _) => DialogResult.Cancel;

        Assert.IsFalse(await ctrl.RunAsync());

        var question = view.ShownInfo.Single();
        StringAssert.Contains(question, "The AI describes 5 photos with the model gpt-test.");
        StringAssert.Contains(question, "It should take about 15 s.");
        StringAssert.Contains(question, "paid model");
        Assert.IsFalse(store.ResourceProcessing.IsRunning);
    }

    [TestMethod]
    public async Task Run_StartsTheStoresRun_WithTheChoice_AndThenUpdatesTheIndex()
    {
        var (ctrl, view, service, store) = CreateCtrl();
        await ctrl.LoadAsync(service, "Notes");
        ctrl.PersonDescriptions = EnumPersonDescriptions.Lock;
        var release = new TaskCompletionSource<Result<ResourceProcessingProgressDto>>(TaskCreationOptions.RunContinuationsAsynchronously);
        ResourceProcessingOptionsDto? options = null;
        AiProviderRef? provider = null;
        store.ResourceProcessing.Runner = (_, p, o, progress, _) =>
        {
            provider = p;
            options = o;
            progress.Report(new ResourceProcessingProgressDto { Phase = EnumResourceProcessingPhase.Processing, PhotosTotal = 3, PhotosDone = 1 });
            return release.Task;
        };
        var indexRuns = 0;
        service.SemanticSearchFake.RunIndexAsyncImpl = _ =>
        {
            Interlocked.Increment(ref indexRuns);
            return Task.FromResult(new Result<SemanticIndexProgressDto>(new SemanticIndexProgressDto { Phase = EnumSemanticIndexPhase.Completed }));
        };

        Assert.IsTrue(await ctrl.RunAsync());
        await WaitForAsync(() => options != null);

        Assert.AreEqual(EnumPersonDescriptions.Lock, options!.PersonDescriptions);
        Assert.AreEqual("gpt-test", provider!.Model);
        Assert.IsTrue(ctrl.IsRunning);
        Assert.IsFalse(ctrl.CanRun, "one run at a time");
        StringAssert.StartsWith(store.ResourceProcessing.StatusBarText, "Describing photos of Notes");

        release.SetResult(new Result<ResourceProcessingProgressDto>(new ResourceProcessingProgressDto
        {
            Phase = EnumResourceProcessingPhase.Completed, PhotosTotal = 3, PhotosDone = 3, PhotosDescribed = 3, PhotosLocked = 2, ElapsedSeconds = 9
        }));
        await store.ResourceProcessing.Completion;
        await WaitForAsync(() => indexRuns > 0);

        Assert.IsFalse(ctrl.IsRunning);
        Assert.AreEqual("Done: 3 photos described in 9 s · 2 locked.", ctrl.ProgressText);
        Assert.IsTrue(view.RefreshProgressCount > 0);
        await store.SemanticIndex.Completion;
    }

    [TestMethod]
    public async Task TheIndexAndTheResources_RunOneAtATime()
    {
        var (resources, _, service, store) = CreateCtrl();
        await resources.LoadAsync(service, "Notes");
        var index = new RepositorySemanticIndexCtrl(store);
        await index.LoadAsync(service, "Notes");
        var release = new TaskCompletionSource<Result<SemanticIndexProgressDto>>(TaskCreationOptions.RunContinuationsAsynchronously);
        service.SemanticSearchFake.RunIndexAsyncImpl = _ => release.Task;

        Assert.IsTrue(store.SemanticIndex.Start(service, "Notes", rebuild: true));

        Assert.IsFalse(resources.CanRun);
        StringAssert.Contains(resources.WarningText, "The semantic index of Notes is being created");
        Assert.IsFalse(store.ResourceProcessing.Start(service, "Notes", new AiProviderRef(), new ResourceProcessingOptionsDto(), false));

        release.SetResult(new Result<SemanticIndexProgressDto>(new SemanticIndexProgressDto { Phase = EnumSemanticIndexPhase.Completed }));
        await store.SemanticIndex.Completion;
        Assert.IsTrue(resources.CanRun);

        var held = new TaskCompletionSource<Result<ResourceProcessingProgressDto>>(TaskCreationOptions.RunContinuationsAsynchronously);
        store.ResourceProcessing.Runner = (_, _, _, _, _) => held.Task;
        Assert.IsTrue(store.ResourceProcessing.Start(service, "Notes", new AiProviderRef(), new ResourceProcessingOptionsDto(), false));
        Assert.IsFalse(index.CanUpdate || index.CanRebuild, "the index waits for the resources");
        StringAssert.Contains(index.WarningText, "The resources of Notes are being processed");
        held.SetResult(new Result<ResourceProcessingProgressDto>(new ResourceProcessingProgressDto { Phase = EnumResourceProcessingPhase.Completed }));
        await store.ResourceProcessing.Completion;
    }

    private static async Task WaitForAsync(Func<bool> condition)
    {
        for (var i = 0; i < 200 && !condition(); i++)
            await Task.Delay(25);
        Assert.IsTrue(condition(), "timed out");
    }
}

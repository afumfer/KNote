using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.ClientWin.Tests.Helpers;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// LongDocumentSummaryCtrl (KNote Atlas, stage 2, 2.1a2): the model and the language it proposes, the estimate (and why it can't be
/// done), starting the summary in the Store with what was chosen (and remembering the language of the repository), refusing it
/// while another run of KNote Atlas goes on, its progress only for its document, and the role it needs.
/// </summary>
[TestClass]
public class LongDocumentSummaryCtrlTests
{
    private static readonly AiProviderRef Local = new() { Alias = "Local", Provider = EnumAiProvider.OpenAI, Model = "small-model", Kind = EnumAiProviderKind.Chat };
    private static readonly AiProviderRef Recommended = new() { Alias = "Good", Provider = EnumAiProvider.OpenAI, Model = "gpt-6-luna", Kind = EnumAiProviderKind.Chat };

    private static (LongDocumentSummaryCtrl Ctrl, FakeLongDocumentSummaryView View, FakeKntService Service, Store Store, ResourceDto Document) Create()
    {
        var factoryViews = new TestFactoryViews();
        var view = new FakeLongDocumentSummaryView();
        factoryViews.Registry.Register<LongDocumentSummaryCtrl, IViewLongDocumentSummary>(_ => view);
        var service = new FakeKntService();
        var store = new Store(factoryViews).GrantAdmin(service);
        store.Settings.Ai.Providers.Add(Local);
        store.Settings.Ai.Providers.Add(Recommended);
        service.SystemValuesFake.GetRepositoryAiModelsAsyncImpl = () => Task.FromResult(new Result<RepositoryAiModelsDto>(new RepositoryAiModelsDto { LlmModel = "gpt-6-luna" }));
        var ctrl = new LongDocumentSummaryCtrl(store) { SaveSettings = () => { } };
        var document = new ResourceDto { ResourceId = Guid.NewGuid(), NoteId = Guid.NewGuid(), Name = $"{Guid.NewGuid()}_manual.pdf", FileType = "application/pdf" };
        return (ctrl, view, service, store, document);
    }

    [TestMethod]
    public async Task ItProposes_TheRepositorysModel_AndTheLastLanguageOfTheRepository()
    {
        var (ctrl, _, service, store, document) = Create();
        var serviceRef = TestServiceRefFactory.CreateWithFakeService(service);
        store.State.Session.SummaryLanguages.Add(new RepositoryLanguageState { RepositoryId = serviceRef.RepositoryId, Language = "Spanish" });

        await ctrl.LoadAsync(serviceRef, document);

        Assert.AreEqual("Good", ctrl.Provider.Alias);
        Assert.AreEqual("Spanish", ctrl.Language);
        Assert.AreEqual("manual.pdf", ctrl.FileName);
        CollectionAssert.AreEqual(new[] { "Local", "Good" }, ctrl.Providers.Select(p => p.Alias).ToArray());
    }

    [TestMethod]
    public async Task TheEstimate_IsShown_OrWhyItCantBeDone()
    {
        var (ctrl, view, service, _, document) = Create();
        await ctrl.LoadAsync(TestServiceRefFactory.CreateWithFakeService(service), document);
        service.AtlasContextFake.ComposeAsyncImpl = _ => Task.FromResult(new Result<AiComposedPromptDto>(new AiComposedPromptDto { Text = "System" }));
        service.SummariesFake.GetStateAsyncImpl = () => Task.FromResult(new Result<SummaryStateDto>(new SummaryStateDto()));
        service.SummariesFake.PrepareDocumentAsyncImpl = _ => Task.FromResult(new Result<DocumentToSummarizeDto>(new DocumentToSummarizeDto
        {
            ResourceId = document.ResourceId, FileName = "manual.pdf", Paged = true, PagesCount = 3, MissingPages = [2],
            Text = $"One{DocumentTextRules.PageSeparator}{DocumentTextRules.PendingMark}{DocumentTextRules.PageSeparator}Three"
        }));

        await ctrl.EstimateAsync();

        Assert.IsNull(ctrl.EstimateError);
        Assert.AreEqual(3, ctrl.Estimate.Pages);
        Assert.AreEqual(2, ctrl.Estimate.Calls);
        CollectionAssert.AreEqual(new[] { 2 }, ctrl.Estimate.MissingPages);
        Assert.IsGreaterThanOrEqualTo(1, view.RefreshViewCallCount);

        var failed = new Result<DocumentToSummarizeDto>();
        failed.AddErrorMessage("The document has no text to summarize.");
        service.SummariesFake.PrepareDocumentAsyncImpl = _ => Task.FromResult(failed);
        await ctrl.EstimateAsync();

        Assert.IsNull(ctrl.Estimate);
        Assert.AreEqual("The document has no text to summarize.", ctrl.EstimateError);
    }

    [TestMethod]
    public async Task Generate_StartsTheSummaryInTheStore_AndRemembersTheLanguage()
    {
        var (ctrl, view, service, store, document) = Create();
        var serviceRef = TestServiceRefFactory.CreateWithFakeService(service);
        await ctrl.LoadAsync(serviceRef, document);
        ctrl.Language = "French ";
        ctrl.Length = EnumSummaryLength.Long;
        service.SemanticSearchFake.GetIndexStatusAsyncImpl = () => Task.FromResult(new Result<SemanticIndexStatusDto>(new SemanticIndexStatusDto { Status = EnumSemanticIndexStatus.None }));
        DocumentProcessRequestDto? asked = null;
        AiProviderRef? used = null;
        var release = new TaskCompletionSource<Result<DocumentProcessProgressDto>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        store.DocumentProcessing.Runner = (_, provider, request, _, _) =>
        {
            asked = request;
            used = provider;
            started.TrySetResult();
            return release.Task;
        };

        Assert.IsTrue(await ctrl.GenerateAsync());
        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.IsTrue(ctrl.IsRunning);
        Assert.AreEqual(document.ResourceId, asked!.ResourceId);
        Assert.AreEqual("French", asked.Language);
        Assert.AreEqual(EnumSummaryLength.Long, asked.Length);
        Assert.AreEqual("Good", used!.Alias);
        Assert.AreEqual("French", store.State.Session.SummaryLanguages.Single(l => l.RepositoryId == serviceRef.RepositoryId).Language);

        release.SetResult(new Result<DocumentProcessProgressDto>(new DocumentProcessProgressDto { Phase = EnumDocumentProcessPhase.Completed, NoteId = Guid.NewGuid(), NoteNumber = 9 }));
        await store.DocumentProcessing.Completion;
        Assert.IsFalse(ctrl.IsRunning);
        Assert.AreEqual(9, ctrl.Progress.NoteNumber, "its document's run, after it ended");
    }

    [TestMethod]
    public async Task Generate_IsRefused_WhileAnotherRunOfKNoteAtlasGoesOn()
    {
        var (ctrl, view, service, store, document) = Create();
        await ctrl.LoadAsync(TestServiceRefFactory.CreateWithFakeService(service), document);
        service.SemanticSearchFake.GetIndexStatusAsyncImpl = () => Task.FromResult(new Result<SemanticIndexStatusDto>(new SemanticIndexStatusDto()));
        var held = new TaskCompletionSource<Result<ResourceProcessingProgressDto>>(TaskCreationOptions.RunContinuationsAsynchronously);
        store.ResourceProcessing.Runner = (_, _, _, _, _) => held.Task;
        Assert.IsTrue(store.ResourceProcessing.Start(service, "Notes", new AiProviderRef(), new ResourceProcessingOptionsDto(), false));

        Assert.IsTrue(ctrl.OtherRunning);
        Assert.IsFalse(await ctrl.GenerateAsync());
        Assert.AreEqual(SummaryTexts.OtherRunning, view.LastShownInfo);
        Assert.IsNull(ctrl.Progress, "nothing of its document");

        held.SetResult(new Result<ResourceProcessingProgressDto>(new ResourceProcessingProgressDto { Phase = EnumResourceProcessingPhase.Completed }));
        await store.ResourceProcessing.Completion;
    }

    // Found by hand (2.1b2): after a translation canceled, the job updates the index with its note; «Continue» stayed disabled until
    // another control refreshed the window, because it didn't hear when that run ended.
    [TestMethod]
    public async Task TheWindow_IsRefreshed_WhenAnotherRunOfKNoteAtlasEnds()
    {
        var (ctrl, view, service, store, document) = Create();
        await ctrl.LoadAsync(TestServiceRefFactory.CreateWithFakeService(service), document);
        ctrl.Run();
        var held = new TaskCompletionSource<Result<ResourceProcessingProgressDto>>(TaskCreationOptions.RunContinuationsAsynchronously);
        store.ResourceProcessing.Runner = (_, _, _, _, _) => held.Task;
        Assert.IsTrue(store.ResourceProcessing.Start(service, "Notes", new AiProviderRef(), new ResourceProcessingOptionsDto(), false));
        var whileRunning = view.RefreshProgressCallCount;
        Assert.IsTrue(ctrl.OtherRunning);

        held.SetResult(new Result<ResourceProcessingProgressDto>(new ResourceProcessingProgressDto { Phase = EnumResourceProcessingPhase.Completed }));
        await store.ResourceProcessing.Completion;

        Assert.IsFalse(ctrl.OtherRunning);
        Assert.IsGreaterThan(whileRunning, view.RefreshProgressCallCount, "the window hears that it ended");
    }

    // Found by hand (2.1a2): the window opened without its models, its lengths or its language.
    [TestMethod]
    public async Task Run_ShowsWhatLoadPrepared_InTheView()
    {
        var (ctrl, view, service, _, document) = Create();
        await ctrl.LoadAsync(TestServiceRefFactory.CreateWithFakeService(service), document);

        ctrl.Run();

        Assert.AreEqual(1, view.ShowViewCallCount);
        Assert.IsGreaterThanOrEqualTo(1, view.RefreshViewCallCount, "the view shows the models, the length and the language");
    }

    // 2.1b2: «Translate document», the same window: its own language, no length, «Continue» and the question before another one.
    [TestMethod]
    public async Task ATranslation_ProposesItsOwnLanguage_AndContinue_GoesOnWithTheOneNotFinished()
    {
        var (ctrl, view, service, store, document) = Create();
        var serviceRef = TestServiceRefFactory.CreateWithFakeService(service);
        store.State.Session.SummaryLanguages.Add(new RepositoryLanguageState { RepositoryId = serviceRef.RepositoryId, Language = "" });
        store.State.Session.TranslationLanguages.Add(new RepositoryLanguageState { RepositoryId = serviceRef.RepositoryId, Language = "French" });
        var unfinished = new DocumentTranslationDto
        {
            ResourceId = document.ResourceId, NoteId = Guid.NewGuid(), NoteNumber = 123, Language = "Spanish", Paged = true, PagesCount = 138,
            Parts = 150, NextPart = 45, NextPage = 41
        };
        service.SummariesFake.GetDocumentTranslationsAsyncImpl = _ => Task.FromResult(new Result<List<DocumentTranslationDto>>(new() { unfinished }));
        service.SemanticSearchFake.GetIndexStatusAsyncImpl = () => Task.FromResult(new Result<SemanticIndexStatusDto>(new SemanticIndexStatusDto { Status = EnumSemanticIndexStatus.None }));
        DocumentProcessRequestDto? asked = null;
        var release = new TaskCompletionSource<Result<DocumentProcessProgressDto>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        store.DocumentProcessing.Runner = (_, _, request, _, _) =>
        {
            asked = request;
            started.TrySetResult();
            return release.Task;
        };

        await ctrl.LoadAsync(serviceRef, document, EnumDocumentProcessKind.Translate);

        Assert.AreEqual("French", ctrl.Language, "the language of the last translation, not of the last summary");
        Assert.AreSame(unfinished, ctrl.Unfinished);
        StringAssert.Contains(SummaryTexts.Unfinished(ctrl.Unfinished), "Spanish, pages 1–40 of 138 (note #123)");

        Assert.IsTrue(await ctrl.ContinueAsync());
        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.AreEqual(EnumDocumentProcessKind.Translate, asked!.Kind);
        Assert.AreEqual(unfinished.NoteId, asked.ContinueNoteId);
        Assert.AreEqual("Spanish", store.State.Session.TranslationLanguages.Single().Language, "in its language");

        release.SetResult(new Result<DocumentProcessProgressDto>(new DocumentProcessProgressDto
        {
            Kind = EnumDocumentProcessKind.Translate, Phase = EnumDocumentProcessPhase.Canceled, NoteId = unfinished.NoteId, NoteNumber = 123
        }));
        await store.DocumentProcessing.Completion;
        Assert.IsTrue(SummaryTexts.HasNote(ctrl.Progress), "what was translated can be opened");
    }

    [TestMethod]
    public async Task ATranslation_NeedsALanguage_AndAsksBeforeAnotherOneIntoTheLanguageOfOneNotFinished()
    {
        var (ctrl, view, service, store, document) = Create();
        var serviceRef = TestServiceRefFactory.CreateWithFakeService(service);
        var unfinished = new DocumentTranslationDto { ResourceId = document.ResourceId, NoteId = Guid.NewGuid(), NoteNumber = 7, Language = "Spanish", Parts = 10, NextPart = 3 };
        service.SummariesFake.GetDocumentTranslationsAsyncImpl = _ => Task.FromResult(new Result<List<DocumentTranslationDto>>(new() { unfinished }));
        service.SemanticSearchFake.GetIndexStatusAsyncImpl = () => Task.FromResult(new Result<SemanticIndexStatusDto>(new SemanticIndexStatusDto { Status = EnumSemanticIndexStatus.None }));
        var requests = new List<DocumentProcessRequestDto>();
        store.DocumentProcessing.Runner = (_, _, request, _, _) =>
        {
            requests.Add(request);
            return Task.FromResult(new Result<DocumentProcessProgressDto>(new DocumentProcessProgressDto { Kind = request.Kind, Phase = EnumDocumentProcessPhase.Completed }));
        };
        await ctrl.LoadAsync(serviceRef, document, EnumDocumentProcessKind.Translate);

        ctrl.Language = " ";
        Assert.IsFalse(await ctrl.GenerateAsync());
        Assert.AreEqual(KNote.Service.ServicesCommands.KntSummaryTexts.LanguageRequired, view.LastShownInfo);

        ctrl.Language = "spanish";
        view.NextShowInfoResult = DialogResult.Cancel;
        Assert.IsFalse(await ctrl.GenerateAsync(), "canceled: nothing starts");
        Assert.AreEqual(SummaryTexts.ContinueInstead(unfinished), view.LastShownInfo);

        view.NextShowInfoResult = DialogResult.No;
        Assert.IsTrue(await ctrl.GenerateAsync());
        await store.DocumentProcessing.Completion;
        Assert.IsNull(requests.Single().ContinueNoteId, "No: another one");

        view.NextShowInfoResult = DialogResult.Yes;
        Assert.IsTrue(await ctrl.GenerateAsync());
        await store.DocumentProcessing.Completion;
        Assert.AreEqual(unfinished.NoteId, requests[1].ContinueNoteId, "Yes: that one goes on");
    }

    [TestMethod]
    public void OnlyStaff_OpensIt()
    {
        var factoryViews = new TestFactoryViews();
        var view = new FakeLongDocumentSummaryView();
        factoryViews.Registry.Register<LongDocumentSummaryCtrl, IViewLongDocumentSummary>(_ => view);
        var store = new Store(factoryViews);
        var notified = new List<string>();
        store.AccessDeniedNotifier = notified.Add;

        new LongDocumentSummaryCtrl(store).Run();

        Assert.AreEqual(0, view.ShowViewCallCount);
        Assert.HasCount(1, notified);
    }
}

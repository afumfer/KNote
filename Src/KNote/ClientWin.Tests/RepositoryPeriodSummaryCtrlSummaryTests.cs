using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.ClientWin.Tests.Helpers;
using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// The summary of the section «Period summary» (RepositoryPeriodSummaryCtrl, KNote Atlas 2.5b): the request with the selection, the
/// subject (empty: the default one), the words between their limits and the language of the repository remembered; the model, the
/// repository's recommended one until another is chosen, resolved when it is used; «Estimate» with it; without a chat provider, why;
/// and «Generate» only with notes that go in.
/// </summary>
[TestClass]
public class RepositoryPeriodSummaryCtrlSummaryTests
{
    private static readonly Guid StatusId = Guid.NewGuid();
    private static readonly Guid PlanningId = Guid.NewGuid();
    private static readonly AiProviderRef Recommended = new() { Alias = "Paid", Provider = EnumAiProvider.OpenAI, Model = "gpt-6-luna", ApiKey = "k" };

    private static async Task<(RepositoryPeriodSummaryCtrl Ctrl, FakeKntService Service, Store Store)> CreateAsync(bool withProvider = true, bool withNotes = true)
    {
        var factoryViews = new TestFactoryViews();
        factoryViews.Registry.Register<RepositoryPeriodSummaryCtrl, IViewPeriodSummary>(_ => new FakePeriodSummaryView());
        var store = new Store(factoryViews);
        if (withProvider)
            store.Settings.Ai.Providers.Add(Recommended);
        var repositoryId = Guid.NewGuid();
        store.State.Session.PeriodSummaryLanguages.Add(new RepositoryLanguageState { RepositoryId = repositoryId, Language = "Spanish" });
        store.State.Session.PeriodSummaries.Add(new RepositoryPeriodSummaryState
        {
            RepositoryId = repositoryId, Parameters = new PeriodSummaryParametersDto { ConceptAttributeIds = { StatusId, Guid.NewGuid() } }
        });
        var service = new FakeKntService { RepositoryRef = new RepositoryRef { Name = "Notes", RepositoryId = repositoryId } };
        var taskType = new NoteTypeDto { NoteTypeId = Guid.NewGuid(), Name = "Task" };
        service.KAttributesFake.GetAllAsyncImpl = () => Task.FromResult(new Result<List<KAttributeInfoDto>>(new List<KAttributeInfoDto>
        {
            new() { KAttributeId = StatusId, Name = "Status", KAttributeDataType = EnumKAttributeDataType.TabulatedValue, Order = 1 },
            new() { KAttributeId = PlanningId, Name = "Planning", KAttributeDataType = EnumKAttributeDataType.String, Order = 2,
                NoteTypeId = taskType.NoteTypeId, NoteTypeDto = taskType },
            new() { KAttributeId = Guid.NewGuid(), Name = "Due date", KAttributeDataType = EnumKAttributeDataType.DateTime, Order = 3 }
        }));
        store.AddServiceRef(TestServiceRefFactory.CreateWithFakeService(service));
        store.Security.SetRepositoryRole(service, EnumRoles.Staff);

        var note = new NoteSelectionDto { NoteId = Guid.NewGuid(), NoteNumber = 4, Decision = withNotes ? EnumSelectionDecision.In : EnumSelectionDecision.Out };
        service.SemanticSearchFake.GetIndexStatusAsyncImpl = () => Task.FromResult(new Result<SemanticIndexStatusDto>(new SemanticIndexStatusDto()));
        service.SummariesFake.PrepareSelectionAsyncImpl = (_, _) =>
            Task.FromResult(new Result<NoteSelectionResultDto>(new NoteSelectionResultDto { Notes = { note } }));
        service.SummariesFake.PreparePeriodAsyncImpl = (_, _) => Task.FromResult(new Result<PeriodSummaryPreparedDto>(new PeriodSummaryPreparedDto
        {
            Selection = new NoteSelectionResultDto { Notes = { note } },
            Notes = { new PeriodNoteDto { NoteId = note.NoteId, NoteNumber = 4, Topic = "Server purchase", Text = "We chose the R750." } }
        }));
        service.SummariesFake.GetStateAsyncImpl = () => Task.FromResult(new Result<SummaryStateDto>(new SummaryStateDto()));

        var ctrl = new RepositoryPeriodSummaryCtrl(store) { SaveSettings = () => { } };
        ctrl.ChooseModel = _ => Task.FromResult((Recommended, new ResourceDescriberDto { Model = Recommended.Model, Provider = Recommended.Provider }));
        await ctrl.LoadAsync(service);
        return (ctrl, service, store);
    }

    [TestMethod]
    public async Task TheRequest_HasTheSelection_TheDefaultSubject_AndTheLanguageOfTheRepository()
    {
        var (ctrl, _, _) = await CreateAsync();
        ctrl.Words = 50_000;

        var request = ctrl.SummaryRequest();

        Assert.AreEqual(EnumDocumentProcessKind.PeriodSummary, request.Kind);
        Assert.IsNull(request.Period.Subject, "empty: the default one");
        StringAssert.StartsWith(ctrl.DefaultSubject, "KNote summary: ");
        Assert.AreEqual(PeriodSummaryRules.MaxWords, request.Period.Words);
        Assert.AreEqual("Spanish", request.Language, "the last one of the repository");
        Assert.IsNull(ctrl.Provider, "the repository's recommended model");
        Assert.IsNull(request.ProviderAlias);
        Assert.AreEqual(ctrl.To, request.Period.Selection.To);
        Assert.IsTrue(ctrl.CanGenerate);
    }

    [TestMethod]
    public async Task TheConceptAttributes_AreTheOnesThatGroup_WithTheLastOnesOfTheRepository_AndAreRemembered()
    {
        var (ctrl, _, store) = await CreateAsync();

        CollectionAssert.AreEqual(new[] { "- · Status", "Task · Planning" }, ctrl.ConceptChoices.Select(c => c.Label).ToList(), "with the note type; not a date");
        CollectionAssert.AreEqual(new[] { StatusId }, ctrl.ConceptAttributeIds, "the last ones that still exist");

        ctrl.ConceptAttributeIds = new List<Guid> { PlanningId, StatusId };
        CollectionAssert.AreEqual(new[] { StatusId, PlanningId }, ctrl.SummaryRequest().Period.ConceptAttributeIds, "in the order of the choices");
        // A run that calls no model.
        DocumentProcessRequestDto? started = null;
        store.DocumentProcessing.Runner = (_, _, request, _, _) =>
        {
            started = request;
            return Task.FromResult(new Result<DocumentProcessProgressDto>(new DocumentProcessProgressDto { Kind = EnumDocumentProcessKind.PeriodSummary, Phase = EnumDocumentProcessPhase.Completed }));
        };
        Assert.IsTrue(await ctrl.GenerateAsync());
        await store.DocumentProcessing.Completion;

        CollectionAssert.AreEqual(new[] { StatusId, PlanningId }, started!.Period.ConceptAttributeIds);

        CollectionAssert.AreEqual(new[] { StatusId, PlanningId }, store.State.Session.PeriodSummaries.Single().Parameters.ConceptAttributeIds);
    }

    [TestMethod]
    public async Task EachField_IsKeptAsItChanges_ForTheUserAndTheRepository_AndComesBackTheNextTime()
    {
        var (ctrl, service, store) = await CreateAsync();
        var saves = 0;
        ctrl.SaveSettings = () => saves++;

        ctrl.Subject = "Server";
        ctrl.Words = 3000;
        ctrl.Language = "French";
        ctrl.Provider = Recommended;
        await ctrl.SetPeriodAsync(new DateTime(2026, 10, 1), new DateTime(2026, 10, 31), fromTheBeginning: false);
        await ctrl.SetKeywordsAsync("R750");

        var kept = store.State.Session.PeriodSummaries.Single().Parameters;
        Assert.AreEqual(("Server", 3000, "French", "Paid", "R750"), (kept.Subject, kept.Words, kept.Language, kept.ProviderAlias, kept.Keywords));
        Assert.AreEqual((new DateTime(2026, 10, 1), new DateTime(2026, 10, 31)), (kept.From!.Value, kept.To!.Value));
        Assert.AreEqual(0, saves, "kept in memory; written when it is estimated, generated or closed");

        // Another section of the same user and repository (the window opened again).
        var again = new RepositoryPeriodSummaryCtrl(store) { SaveSettings = () => { } };
        again.ChooseModel = ctrl.ChooseModel;
        await again.LoadAsync(service);

        Assert.AreEqual(("Server", 3000, "French", "R750"), (again.Subject, again.Words, again.Language, again.Keywords));
        Assert.AreSame(Recommended, again.Provider);
        Assert.AreEqual((new DateTime(2026, 10, 1), new DateTime(2026, 10, 31)), (again.From, again.To));
        ctrl.Dispose();
        Assert.AreEqual(1, saves, "closed, written");
    }

    [TestMethod]
    public async Task Estimate_WithTheRecommendedModel_SaysTheNotesAndThatItIsPaid()
    {
        var (ctrl, _, _) = await CreateAsync();

        await ctrl.EstimateAsync();

        Assert.IsNull(ctrl.EstimateError, ctrl.EstimateError);
        Assert.AreEqual(1, ctrl.Estimate.Notes);
        StringAssert.StartsWith(ctrl.EstimateText, "1 note, ");
        StringAssert.Contains(ctrl.EstimateText, "The notes are sent to OpenAI, which charges for it.");
    }

    [TestMethod]
    public async Task WithoutAChatProvider_TheEstimateSaysWhy()
    {
        var (ctrl, _, _) = await CreateAsync(withProvider: false);

        await ctrl.EstimateAsync();

        StringAssert.Contains(ctrl.EstimateText, "There is no AI provider of chat configured");
        Assert.IsFalse(ctrl.CanGenerate);
    }

    [TestMethod]
    public async Task WithoutNotesThatGoIn_ItIsNotGenerated()
    {
        var (ctrl, _, _) = await CreateAsync(withNotes: false);

        Assert.IsFalse(ctrl.CanGenerate);
        Assert.IsFalse(await ctrl.GenerateAsync());
    }
}

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
        var service = new FakeKntService { RepositoryRef = new RepositoryRef { Name = "Notes", RepositoryId = repositoryId } };
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

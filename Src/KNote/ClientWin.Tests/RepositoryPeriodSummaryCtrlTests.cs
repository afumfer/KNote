using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.ClientWin.Tests.Helpers;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// RepositoryPeriodSummaryCtrl («Period summary» of the KNote Atlas window, KNote Atlas 2.3b) and ReviewSelectionCtrl: the selection of
/// the last week prepared when the section loads, again with each field changed (the period, «From the beginning», the folder with its
/// path, the keywords, «Talks about…» only with an index), its counts and its failure; «Review selection» with its three lists, the
/// changes it keeps (a box back as KNote decides takes the correction away) and the selection prepared again after keeping them.
/// </summary>
[TestClass]
public class RepositoryPeriodSummaryCtrlTests
{
    private static readonly Guid FolderId = Guid.NewGuid();

    private sealed class Fixture
    {
        public RepositoryPeriodSummaryCtrl Ctrl { get; init; } = null!;
        public FakePeriodSummaryView View { get; init; } = null!;
        public FakeReviewSelectionView ReviewView { get; init; } = null!;
        public FakeKntService Service { get; init; } = null!;
        public Store Store { get; init; } = null!;
        public List<NoteSelectionRequestDto> Requests { get; } = new();
        public List<RelevanceCorrectionsSaveDto> Saved { get; } = new();
        public NoteSelectionResultDto Selection { get; set; } = SelectionOf();
        public bool Fails { get; set; }
    }

    private static NoteSelectionDto NoteOf(int number, EnumSelectionDecision decision, EnumSelectionDecision? computed = null) => new()
    {
        NoteId = Guid.NewGuid(), NoteNumber = number, Topic = $"Note {number}", Decision = decision, ComputedDecision = computed ?? decision,
        Reason = $"{decision}: reason"
    };

    private static NoteSelectionResultDto SelectionOf(params NoteSelectionDto[] notes) => new() { Notes = notes.ToList() };

    private static Fixture Create(bool withIndex = false)
    {
        var factoryViews = new TestFactoryViews();
        var view = new FakePeriodSummaryView();
        var reviewView = new FakeReviewSelectionView();
        factoryViews.Registry.Register<RepositoryPeriodSummaryCtrl, IViewPeriodSummary>(_ => view);
        factoryViews.Registry.Register<ReviewSelectionCtrl, IViewReviewSelection>(_ => reviewView);
        var store = new Store(factoryViews);
        var service = new FakeKntService();
        store.AddServiceRef(TestServiceRefFactory.CreateWithFakeService(service));
        store.Security.SetRepositoryRole(service, EnumRoles.Staff);

        var fixture = new Fixture { Ctrl = new RepositoryPeriodSummaryCtrl(store), View = view, ReviewView = reviewView, Service = service, Store = store };
        service.SemanticSearchFake.GetIndexStatusAsyncImpl = () => Task.FromResult(new Result<SemanticIndexStatusDto>(new SemanticIndexStatusDto
        {
            Status = withIndex ? EnumSemanticIndexStatus.Ready : EnumSemanticIndexStatus.None, IndexedNotes = withIndex ? 10 : 0,
            EmbeddingModelAvailable = withIndex
        }));
        service.SummariesFake.PrepareSelectionAsyncImpl = (request, _) =>
        {
            fixture.Requests.Add(request);
            if (fixture.Fails)
            {
                var error = new Result<NoteSelectionResultDto>();
                error.AddErrorMessage("No index.");
                return Task.FromResult(error);
            }
            return Task.FromResult(new Result<NoteSelectionResultDto>(fixture.Selection));
        };
        service.SummariesFake.SaveRelevanceCorrectionsAsyncImpl = changes =>
        {
            fixture.Saved.Add(changes);
            return Task.FromResult(new Result<List<RelevanceCorrectionDto>>(new List<RelevanceCorrectionDto>()));
        };
        service.FoldersFake.GetByIdAsyncImpl = id => Task.FromResult(new Result<FolderDto>(new FolderDto { FolderId = id, Name = "Projects" }));
        return fixture;
    }

    [TestMethod]
    public async Task Load_PreparesTheLastWeek_WithoutMeaningIfThereIsNoIndex()
    {
        var fixture = Create();
        fixture.Selection = SelectionOf(NoteOf(1, EnumSelectionDecision.In), NoteOf(2, EnumSelectionDecision.Doubtful));

        await fixture.Ctrl.LoadAsync(fixture.Service);
        await fixture.Ctrl.SetMeaningTextAsync("volcanoes");

        var request = fixture.Requests.First();
        Assert.AreEqual((DateTime.Today.AddDays(-6), DateTime.Today, null), (request.From, request.To, request.FolderId));
        Assert.IsFalse(fixture.Ctrl.MeaningAvailable);
        Assert.IsNull(fixture.Requests.Last().MeaningText, "without an index, «Talks about…» is not sent");
        Assert.AreEqual("1 in, 1 doubtful, 0 out.", fixture.Ctrl.CountText);
        Assert.IsTrue(fixture.Ctrl.CanReview);
    }

    [TestMethod]
    public async Task EachField_PreparesTheSelectionAgain()
    {
        var fixture = Create(withIndex: true);
        await fixture.Ctrl.LoadAsync(fixture.Service);

        await fixture.Ctrl.SetPeriodAsync(new DateTime(2026, 1, 1), new DateTime(2026, 1, 31), fromTheBeginning: true);
        fixture.Ctrl.ChooseFolder = () => new FolderInfoDto { FolderId = FolderId, Name = "Projects" };
        await fixture.Ctrl.ChooseFolderAsync();
        await fixture.Ctrl.SetKeywordsAsync(" QL ");
        await fixture.Ctrl.SetMeaningTextAsync("volcanoes");
        await fixture.Ctrl.SetKeywordsAsync(" QL ");

        Assert.HasCount(5, fixture.Requests, "the same text again prepares nothing");
        var last = fixture.Requests.Last();
        Assert.AreEqual((NoteSelectionRequestDto.Beginning, new DateTime(2026, 1, 31), FolderId, "QL", "volcanoes"),
            (last.From, last.To, last.FolderId, last.Keywords, last.MeaningText));
        Assert.AreEqual("Projects", fixture.Ctrl.FolderText);

        await fixture.Ctrl.ClearFolderAsync();
        Assert.IsNull(fixture.Requests.Last().FolderId);
        Assert.AreEqual("All folders", fixture.Ctrl.FolderText);
    }

    [TestMethod]
    public async Task TheLinkToNoteRelevance_IsShown_OnceItExists()
    {
        var fixture = Create();
        await fixture.Ctrl.LoadAsync(fixture.Service);
        Assert.AreEqual("", fixture.Ctrl.RelevanceNoteText);
        Assert.IsFalse(await fixture.Ctrl.OpenRelevanceNoteAsync());

        fixture.Selection = new NoteSelectionResultDto { RelevanceNote = new AtlasNoteRefDto { NoteId = Guid.NewGuid(), NoteNumber = 42 } };
        await fixture.Ctrl.PrepareAsync();

        Assert.AreEqual("Note relevance (#42)", fixture.Ctrl.RelevanceNoteText);
    }

    [TestMethod]
    public async Task ASelectionThatFails_SaysSo()
    {
        var fixture = Create();
        fixture.Fails = true;

        await fixture.Ctrl.LoadAsync(fixture.Service);

        Assert.IsTrue(fixture.Ctrl.LoadFailed);
        Assert.IsFalse(fixture.Ctrl.CanReview);
        Assert.AreEqual("The selection could not be prepared.", fixture.Ctrl.CountText);
        StringAssert.Contains(fixture.View.ShownInfo.Single(), "No index.");
    }

    [TestMethod]
    public async Task Review_WhatItKeeps_PreparesTheSelectionAgain()
    {
        var fixture = Create();
        fixture.Selection = SelectionOf(NoteOf(1, EnumSelectionDecision.In));
        await fixture.Ctrl.LoadAsync(fixture.Service);

        fixture.Ctrl.RunReview = _ => false;
        Assert.IsFalse(await fixture.Ctrl.ReviewAsync());
        fixture.Ctrl.RunReview = _ => true;
        Assert.IsTrue(await fixture.Ctrl.ReviewAsync());

        Assert.HasCount(2, fixture.Requests);
    }

    [TestMethod]
    public async Task ReviewSelection_KeepsTheBoxesChanged()
    {
        var fixture = Create();
        var inOne = NoteOf(1, EnumSelectionDecision.In);
        var doubtful = NoteOf(2, EnumSelectionDecision.Doubtful);
        var outOne = NoteOf(3, EnumSelectionDecision.Out);
        var corrected = NoteOf(4, EnumSelectionDecision.In, computed: EnumSelectionDecision.Out);
        var review = new ReviewSelectionCtrl(fixture.Store, fixture.Service, SelectionOf(inOne, doubtful, outOne, corrected));

        Assert.AreEqual(2, review.NotesOf(EnumSelectionDecision.In).Count);
        Assert.IsTrue(review.IsChecked(doubtful), "S5: a doubtful one starts checked");
        Assert.IsTrue(await review.AcceptAsync(), "nothing to keep");
        Assert.IsEmpty(fixture.Saved);

        review.SetChecked(doubtful.NoteId, false);
        review.SetChecked(outOne.NoteId, true);
        review.SetChecked(corrected.NoteId, false);
        review.SetChecked(inOne.NoteId, true);
        Assert.AreEqual("2 of 4 notes would go into the summary · 3 changes to keep.", review.ChangesText);

        Assert.IsTrue(await review.AcceptAsync());

        var changes = fixture.Saved.Single().Changes.ToDictionary(c => c.NoteId, c => c.Decision);
        Assert.HasCount(3, changes);
        Assert.AreEqual(EnumSelectionDecision.Out, changes[doubtful.NoteId]);
        Assert.AreEqual(EnumSelectionDecision.In, changes[outOne.NoteId]);
        Assert.IsNull(changes[corrected.NoteId], "back to KNote's: its correction goes away");
        Assert.IsTrue(review.Saved);
    }

    [TestMethod]
    public async Task ReviewSelection_ThatCannotKeepItsChanges_Stays()
    {
        var fixture = Create();
        var note = NoteOf(1, EnumSelectionDecision.In);
        fixture.Service.SummariesFake.SaveRelevanceCorrectionsAsyncImpl = _ =>
        {
            var error = new Result<List<RelevanceCorrectionDto>>();
            error.AddErrorMessage("Locked.");
            return Task.FromResult(error);
        };
        var review = new ReviewSelectionCtrl(fixture.Store, fixture.Service, SelectionOf(note));
        review.SetChecked(note.NoteId, false);

        Assert.IsFalse(await review.AcceptAsync());
        Assert.IsFalse(review.Saved);
        StringAssert.Contains(fixture.ReviewView.ShownInfo.Single(), "Locked.");
    }
}

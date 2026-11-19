using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.ClientWin.Tests.Helpers;
using KNote.Model;
using KNote.Model.Dto;
using Microsoft.Extensions.AI;

namespace KNote.ClientWin.Tests;

/// <summary>
/// «Classify doubtful notes with AI…» of the section «Period summary» (RepositoryPeriodSummaryCtrl, KNote Atlas 2.4b, C1-C6): it says
/// what it does first (and nothing without saying yes), asks the batches one by one with AiNoteClassifier, keeps what the AI answered,
/// stops between batches when cancelled (what was asked stays), says how it went and prepares the selection again.
/// </summary>
[TestClass]
public class RepositoryPeriodSummaryCtrlClassifyTests
{
    private static readonly AiProviderRef Provider = new() { Alias = "Local", Provider = EnumAiProvider.Ollama, Model = "qwen3.5:9b" };

    private sealed class Fixture
    {
        public RepositoryPeriodSummaryCtrl Ctrl { get; init; } = null!;
        public FakePeriodSummaryView View { get; init; } = null!;
        public FakeKntService Service { get; init; } = null!;
        public List<string> Questions { get; } = new();
        public List<RelevanceAiAnswersSaveDto> Saved { get; } = new();
        public int Prepared { get; set; }
        public DialogResult Answer { get; set; } = DialogResult.OK;
    }

    private static async Task<Fixture> CreateAsync(int doubtful)
    {
        var notes = Enumerable.Range(1, doubtful).Select(i => new NoteSelectionDto
        {
            NoteId = Guid.NewGuid(), NoteNumber = i, Topic = $"Note {i}", Decision = EnumSelectionDecision.Doubtful,
            ComputedDecision = EnumSelectionDecision.Doubtful, Source = EnumSelectionSource.Signals, Score = 0
        }).ToList();

        var factoryViews = new TestFactoryViews();
        var view = new FakeQuestionsView();
        factoryViews.Registry.Register<RepositoryPeriodSummaryCtrl, IViewPeriodSummary>(_ => view);
        var store = new Store(factoryViews);
        var service = new FakeKntService();
        store.AddServiceRef(TestServiceRefFactory.CreateWithFakeService(service));
        store.Security.SetRepositoryRole(service, EnumRoles.Staff);

        // SaveSettings: not the real Store.SaveConfig, which writes the user's config files.
        var fixture = new Fixture { Ctrl = new RepositoryPeriodSummaryCtrl(store) { SaveSettings = () => { } }, View = view, Service = service };
        view.Answer = (info, buttons) =>
        {
            fixture.Questions.Add(info);
            return buttons == MessageBoxButtons.OKCancel ? fixture.Answer : DialogResult.OK;
        };
        service.SemanticSearchFake.GetIndexStatusAsyncImpl = () => Task.FromResult(new Result<SemanticIndexStatusDto>(new SemanticIndexStatusDto()));
        service.SummariesFake.PrepareSelectionAsyncImpl = (_, _) =>
        {
            fixture.Prepared++;
            return Task.FromResult(new Result<NoteSelectionResultDto>(new NoteSelectionResultDto { Notes = notes }));
        };
        service.SummariesFake.GetNotesToClassifyAsyncImpl = ids => Task.FromResult(new Result<List<NoteToClassifyDto>>(
            notes.Where(n => ids.Contains(n.NoteId)).Select(n => new NoteToClassifyDto
            {
                NoteId = n.NoteId, NoteNumber = n.NoteNumber, Topic = n.Topic, Text = "A short text.", ContentHash = $"h{n.NoteNumber}"
            }).ToList()));
        service.SummariesFake.SaveRelevanceAiAnswersAsyncImpl = answers =>
        {
            fixture.Saved.Add(answers);
            return Task.FromResult(new Result<List<RelevanceAiAnswerDto>>(answers.Answers));
        };

        // Every note number answered: each batch reads only its own.
        var answer = string.Join("\n", notes.Select(n => $"#{n.NoteNumber} | Out | a reminder"));
        var model = new FakeChatClient { GetStreamingResponseImpl = (_, _, _) => Stream(answer) };
        fixture.Ctrl.ChooseModel = _ => Task.FromResult((Provider, new ResourceDescriberDto { Provider = Provider.Provider, Model = Provider.Model }));
        fixture.Ctrl.CreateChatClient = _ => model;

        await fixture.Ctrl.LoadAsync(service);
        return fixture;
    }

    [TestMethod]
    public async Task TheDoubtfulNotes_AreAskedByBatches_AndTheSelectionIsPreparedAgain()
    {
        var fixture = await CreateAsync(doubtful: 12);
        Assert.IsTrue(fixture.Ctrl.CanClassify);
        Assert.AreEqual("12 doubtful notes can be asked to the AI.", fixture.Ctrl.PendingText);

        Assert.IsTrue(await fixture.Ctrl.ClassifyAsync());

        StringAssert.Contains(fixture.Questions.First(), "12 notes in 2 calls to the model qwen3.5:9b");
        Assert.HasCount(2, fixture.Saved);
        Assert.AreEqual(12, fixture.Saved.Sum(s => s.Answers.Count));
        Assert.AreEqual("The AI classified 12 of 12 notes.", fixture.Questions.Last());
        Assert.AreEqual(2, fixture.Prepared, "on load and after");
        Assert.IsFalse(fixture.Ctrl.IsClassifying);
    }

    [TestMethod]
    public async Task WithoutSayingYes_NothingIsAsked()
    {
        var fixture = await CreateAsync(doubtful: 3);
        fixture.Answer = DialogResult.Cancel;

        Assert.IsFalse(await fixture.Ctrl.ClassifyAsync());

        Assert.IsEmpty(fixture.Saved);
        Assert.AreEqual(1, fixture.Prepared);
    }

    [TestMethod]
    public async Task Cancelled_ItStopsBetweenBatches_AndWhatWasAskedStays()
    {
        var fixture = await CreateAsync(doubtful: 25);
        var save = fixture.Service.SummariesFake.SaveRelevanceAiAnswersAsyncImpl!;
        fixture.Service.SummariesFake.SaveRelevanceAiAnswersAsyncImpl = answers =>
        {
            fixture.Ctrl.CancelClassification();
            return save(answers);
        };

        await fixture.Ctrl.ClassifyAsync();

        Assert.HasCount(1, fixture.Saved, "the first batch, then it stops");
        Assert.AreEqual("The AI classified 10 of 25 notes.", fixture.Questions.Last());
    }

    [TestMethod]
    public async Task APaidModel_IsSaidBeforeAsking()
    {
        var fixture = await CreateAsync(doubtful: 2);
        fixture.Ctrl.ChooseModel = _ => Task.FromResult((Provider, new ResourceDescriberDto { Provider = "OpenAI", Model = "gpt-6-luna", IsPaid = true }));
        fixture.Answer = DialogResult.Cancel;

        await fixture.Ctrl.ClassifyAsync();

        StringAssert.Contains(fixture.Questions.Single(), "gpt-6-luna is a paid model");
    }

    private static async IAsyncEnumerable<ChatResponseUpdate> Stream(string text)
    {
        await Task.Yield();
        yield return new ChatResponseUpdate(ChatRole.Assistant, text);
    }

    // The section's view that answers its questions.
    private sealed class FakeQuestionsView : FakePeriodSummaryView
    {
        public Func<string, MessageBoxButtons, DialogResult> Answer { get; set; } = (_, _) => DialogResult.OK;

        public override DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information) =>
            Answer(info, buttons);
    }
}

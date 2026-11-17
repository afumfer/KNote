using System.Runtime.CompilerServices;
using KNote.Ai;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Tests.Helpers;
using Microsoft.Extensions.AI;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// The summary of a period with a repository and a fake model (KNote Atlas, stage 2, 2.5, D5, D11, D17, P1-P7): what it reads of each
/// note that goes in (not of the ones out), its doubtful notes asked to the AI first, the summary kept in a new note of «KNote Summary»
/// with the original of the AI attached and without touching the notes of the period, the estimate, nothing kept when it is canceled,
/// and a selection without notes that go in.
/// </summary>
[TestClass]
public class PeriodSummaryServiceTests
{
    private static readonly DateTime From = new(2020, 3, 9);
    private static readonly DateTime To = new(2020, 3, 15);
    private static readonly DateTime InPeriod = new(2020, 3, 10, 12, 0, 0);
    private static readonly AiProviderRef Provider = new() { Alias = "Fake", Provider = EnumAiProvider.OpenAI, Model = "fake-llm", ApiKey = "k" };

    private static string Words(int count) => string.Join(" ", Enumerable.Range(1, count).Select(i => $"word{i}"));

    private static DocumentProcessRequestDto Request(int words = 300) => new()
    {
        Kind = EnumDocumentProcessKind.PeriodSummary, Language = "English",
        Period = new PeriodSummaryRequestDto { Selection = new NoteSelectionRequestDto { From = From, To = To }, Words = words }
    };

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ThePeriod_IsPrepared_WithWhatIsReadOfEachNoteThatGoesIn(string orm)
    {
        using var fixture = await SemanticIndexFixture.CreateAsync(orm);
        var work = await AddFolderAsync(fixture, "Work");
        var decision = await AddNoteAsync(fixture, "Server purchase", $"We chose the Dell R750. {Words(50)}", work);
        var tiny = await AddNoteAsync(fixture, "x", "y", fixture.HomeId, sameDay: true);
        await AddTaskAsync(fixture, decision, "Buy it", start: new DateTime(2020, 3, 11));
        await AddTaskAsync(fixture, decision, "Old one", start: new DateTime(2019, 1, 1));
        Assert.IsTrue((await fixture.Service.Repository.Notes.AddResourceAsync(new ResourceDto
        {
            ResourceId = Guid.NewGuid(), NoteId = decision, Name = $"{Guid.NewGuid()}_offer.pdf", Container = "R", Description = "The offer of Dell.",
            FileType = "application/pdf"
        })).IsValid);

        var prepared = await fixture.Service.Summaries.PreparePeriodAsync(Request().Period);

        Assert.IsTrue(prepared.IsValid, prepared.ErrorMessage);
        var note = prepared.Entity.Notes.Single();
        Assert.AreEqual(decision, note.NoteId);
        Assert.IsFalse(prepared.Entity.Notes.Any(n => n.NoteId == tiny), "out: not read");
        StringAssert.EndsWith(note.FolderPath, "Work");
        StringAssert.StartsWith(note.Text, "We chose the Dell R750.");
        StringAssert.Contains(note.Tasks.Single(), "Buy it");
        StringAssert.EndsWith(note.Resources.Single(), "offer.pdf: The offer of Dell.");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task TheSummary_IsKeptInANewNote_AfterAskingAboutTheDoubtfulNotes(string orm)
    {
        using var fixture = await SemanticIndexFixture.CreateAsync(orm);
        await SignInAsync(fixture);
        // The original of the AI is a resource: a folder of resources of the test's own.
        var root = Path.Combine(Path.GetTempPath(), $"knote-period-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        fixture.Service.Repository.RepositoryRef.ResourcesContainer = "KntResources";
        fixture.Service.Repository.RepositoryRef.ResourcesContainerRootPath = root;
        fixture.Service.Repository.RepositoryRef.ResourcesContainerRootUrl = "http://intranet/KNote";
        var decision = await AddNoteAsync(fixture, "Server purchase", $"We chose the Dell R750. {Words(50)}", fixture.HomeId);
        var doubtful = await AddNoteAsync(fixture, "Call", "Call the supplier about the delivery date.", fixture.HomeId);
        var decisionNumber = (await fixture.Service.Notes.GetAsync(decision)).Entity.NoteNumber;
        var doubtfulNumber = (await fixture.Service.Notes.GetAsync(doubtful)).Entity.NoteNumber;
        var before = (await fixture.Service.Notes.GetAsync(decision)).Entity.ModificationDateTime;
        var model = new FakeModel(request => request.Contains("<note number=")
            ? $"#{doubtfulNumber} | In | about the delivery"
            : request.Contains("Write the summary") ? $"The server was chosen (#{decisionNumber})." : $"- Chosen #{decisionNumber}");
        var phases = new List<EnumDocumentProcessPhase>();

        var estimate = await AiDocumentProcessor.EstimateAsync(fixture.Service, Provider, Request());
        var result = await AiDocumentProcessor.ProcessAsync(fixture.Service, _ => model, Provider, Request(),
            new SyncProgress<DocumentProcessProgressDto>(p => phases.Add(p.Phase)));

        Assert.IsTrue(estimate.IsValid, estimate.ErrorMessage);
        Assert.AreEqual((2, 1), (estimate.Entity.Notes, estimate.Entity.DoubtfulToClassify));
        Assert.AreEqual(3, estimate.Entity.Calls, "a classification, a batch and the final call");
        Assert.IsTrue(result.IsValid, result.ErrorMessage);
        Assert.AreEqual(EnumDocumentProcessPhase.Completed, result.Entity.Phase);
        CollectionAssert.IsSubsetOf(new[] { EnumDocumentProcessPhase.Classifying, EnumDocumentProcessPhase.Reading, EnumDocumentProcessPhase.Writing }, phases);

        var summary = (await fixture.Service.Notes.GetExtendedAsync(result.Entity.NoteId!.Value)).Entity;
        Assert.AreEqual(KntConst.SummaryTag, summary.NoteTypeDto.Name);
        Assert.AreEqual(KntConst.SummaryFolderName, (await fixture.Service.Folders.GetAsync(summary.FolderId)).Entity.Name);
        Assert.AreEqual("KNote summary: 09/03/2020 - 15/03/2020", summary.Topic);
        StringAssert.Contains(summary.Description, "2 notes read");
        StringAssert.EndsWith(summary.Description, $"The server was chosen (#{decisionNumber}).");
        StringAssert.EndsWith(summary.Resources.Single().NameOut, "(original).md");
        Assert.AreEqual(before, (await fixture.Service.Notes.GetAsync(decision)).Entity.ModificationDateTime, "the notes of the period don't change");
        var state = (await fixture.Service.Summaries.GetStateAsync()).Entity;
        Assert.IsTrue(state.PeriodSecondsPer1000Characters.ContainsKey("fake-llm"));
        try { Directory.Delete(root, recursive: true); } catch (IOException) { }
    }

    [TestMethod]
    public async Task Canceled_NothingIsKept()
    {
        using var fixture = await SemanticIndexFixture.CreateAsync();
        await SignInAsync(fixture);
        await AddNoteAsync(fixture, "Server purchase", Words(60), fixture.HomeId);
        using var cancellation = new CancellationTokenSource();
        var model = new FakeModel(_ =>
        {
            cancellation.Cancel();
            return "- notes";
        });

        var result = await AiDocumentProcessor.ProcessAsync(fixture.Service, _ => model, Provider, Request(), cancellationToken: cancellation.Token);

        Assert.AreEqual(EnumDocumentProcessPhase.Canceled, result.Entity.Phase);
        Assert.IsFalse((await fixture.Service.NoteTypes.GetAllAsync()).Entity.Any(t => t.Name == KntConst.SummaryTag), "no summary saved");
    }

    [TestMethod]
    public async Task WithoutNotesThatGoIn_ItSaysSo()
    {
        using var fixture = await SemanticIndexFixture.CreateAsync();
        await SignInAsync(fixture);
        await AddNoteAsync(fixture, "x", "y", fixture.HomeId, sameDay: true);

        var estimate = await AiDocumentProcessor.EstimateAsync(fixture.Service, Provider, Request());
        var result = await AiDocumentProcessor.ProcessAsync(fixture.Service, _ => new FakeModel(_ => "x"), Provider, Request());

        StringAssert.Contains(estimate.ErrorMessage, "No note of the selection goes into the summary");
        Assert.AreEqual(EnumDocumentProcessPhase.Failed, result.Entity.Phase);
        StringAssert.Contains(result.Entity.Message, "No note of the selection");
    }

    private static async Task SignInAsync(SemanticIndexFixture fixture) =>
        fixture.Service.UserIdentityName = (await fixture.Service.Users.GetAsync(fixture.UserId)).Entity.UserName;

    private static async Task<Guid> AddFolderAsync(SemanticIndexFixture fixture, string name)
    {
        var saved = await fixture.Service.Folders.SaveAsync(new FolderDto { Name = name, ParentId = fixture.HomeId });
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        return saved.Entity.FolderId;
    }

    // A note of the period; not left the same day unless said (a very short one left the same day is out).
    private static async Task<Guid> AddNoteAsync(SemanticIndexFixture fixture, string topic, string description, Guid folderId, bool sameDay = false)
    {
        var note = await fixture.Service.Repository.Notes.AddAsync(new NoteDto
        {
            NoteId = Guid.NewGuid(), Topic = topic, Description = description, FolderId = folderId,
            CreationDateTime = sameDay ? InPeriod : InPeriod.AddDays(-1), ModificationDateTime = InPeriod
        });
        Assert.IsTrue(note.IsValid, note.ErrorMessage);
        return note.Entity.NoteId;
    }

    private static async Task AddTaskAsync(SemanticIndexFixture fixture, Guid noteId, string description, DateTime start)
    {
        var task = await fixture.Service.Repository.Notes.AddNoteTaskAsync(new NoteTaskDto
        {
            NoteTaskId = Guid.NewGuid(), NoteId = noteId, UserId = fixture.UserId, Description = description, Tags = "", Priority = 1, StartDate = start
        });
        Assert.IsTrue(task.IsValid, task.ErrorMessage);
    }

    private sealed class SyncProgress<T>(Action<T> report) : IProgress<T>
    {
        public void Report(T value) => report(value);
    }

    // Answers by what it is asked (the last user message).
    private sealed class FakeModel(Func<string, string> answer) : IChatClient
    {
        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("The summaries stream.");

        public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var request = messages.Last(m => m.Role == ChatRole.User).Text;
            await Task.Yield();
            var text = answer(request);
            cancellationToken.ThrowIfCancellationRequested();
            yield return new ChatResponseUpdate(ChatRole.Assistant, text);
        }

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose() { }
    }
}

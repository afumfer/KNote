using System.Runtime.CompilerServices;
using KNote.Ai;
using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;
using KNote.Tests.Helpers;
using Microsoft.Extensions.AI;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// The doubtful notes asked to the AI with a repository (KNote Atlas, stage 2, 2.4, D9, C1-C6) and a fake model: the batch read as
/// the selection reads it (the same fingerprint), the answers kept in «Note relevance» and used by the next selection, asked again
/// once the note changes, below a person's correction; KNote's own notes never sent; and a model that answers nothing keeps nothing.
/// </summary>
[TestClass]
public class NoteClassificationServiceTests
{
    private static readonly DateTime From = new(2020, 3, 9);
    private static readonly DateTime To = new(2020, 3, 15);
    private static readonly DateTime InPeriod = new(2020, 3, 10, 12, 0, 0);
    private static readonly AiProviderRef Provider = new() { Alias = "Fake", Provider = EnumAiProvider.Ollama, Model = "fake-llm" };

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task TheAnswers_AreKept_AndDecideTheDoubtfulNotes_WhileTheyDontChange(string orm)
    {
        using var fixture = await SemanticIndexFixture.CreateAsync(orm);
        await SignInAsync(fixture);
        var decision = await AddNoteAsync(fixture, "Budget", "We decided to buy the new server.");
        var reminder = await AddNoteAsync(fixture, "Call", "Call Ana.");
        var selection = await SelectAsync(fixture);
        var doubtful = NoteClassificationRules.ToClassify(selection);
        CollectionAssert.AreEquivalent(new[] { decision.NoteId, reminder.NoteId }, doubtful.Select(n => n.NoteId).ToList());
        var model = new FakeModel(messages => $"#{decision.NoteNumber} | In | a decision\n#{reminder.NoteNumber} | Out | a reminder");

        var answers = await AiNoteClassifier.ClassifyAsync(fixture.Service, model, Provider, doubtful.Select(n => n.NoteId).ToList());

        Assert.IsTrue(answers.IsValid, answers.ErrorMessage);
        Assert.HasCount(2, answers.Entity);
        StringAssert.Contains(model.Request, "<note number=");
        StringAssert.Contains(model.Request, "We decided to buy the new server.");
        StringAssert.Contains(model.System, "#number | In | reason");
        var after = await SelectAsync(fixture);
        Assert.AreEqual((EnumSelectionDecision.In, EnumSelectionSource.Ai), Decision(after, decision.NoteId));
        Assert.AreEqual((EnumSelectionDecision.Out, EnumSelectionSource.Ai), Decision(after, reminder.NoteId));
        Assert.AreEqual("Out: according to the AI (fake-llm), a reminder", after.Notes.Single(n => n.NoteId == reminder.NoteId).Reason);
        Assert.AreEqual(0, NoteClassificationRules.PendingCount(after));

        // Changed, its answer is no longer valid: it is doubtful again (and asked again).
        var changed = (await fixture.Service.Repository.Notes.GetAsync(reminder.NoteId)).Entity;
        changed.Description = "Call Ana about the invoice.";
        Assert.IsTrue((await fixture.Service.Repository.Notes.UpdateAsync(changed, keepModificationDate: true)).IsValid);
        var again = await SelectAsync(fixture);
        Assert.AreEqual((EnumSelectionDecision.Doubtful, EnumSelectionSource.Signals), Decision(again, reminder.NoteId));
        Assert.AreEqual(EnumSelectionSource.Ai, Decision(again, decision.NoteId).Source);

        // A person's correction is above the AI.
        Assert.IsTrue((await fixture.Service.Summaries.SaveRelevanceCorrectionsAsync(new RelevanceCorrectionsSaveDto
        {
            Changes = { new() { NoteId = decision.NoteId, Decision = EnumSelectionDecision.Out } }
        })).IsValid);
        Assert.AreEqual((EnumSelectionDecision.Out, EnumSelectionSource.Correction), Decision(await SelectAsync(fixture), decision.NoteId));
    }

    [TestMethod]
    public async Task KNotesOwnNotes_AreNeverSent_AndAModelThatAnswersNothing_KeepsNothing()
    {
        using var fixture = await SemanticIndexFixture.CreateAsync();
        await SignInAsync(fixture);
        var sessions = (await fixture.Service.NoteTypes.SaveAsync(new NoteTypeDto { Name = KntConst.ChatSessionsTag })).Entity.NoteTypeId;
        var own = await AddNoteAsync(fixture, "A conversation", "Hello.", sessions);
        var note = await AddNoteAsync(fixture, "Note", "Short text.");

        var toClassify = await fixture.Service.Summaries.GetNotesToClassifyAsync(new List<Guid> { own.NoteId, note.NoteId });
        var answers = await AiNoteClassifier.ClassifyAsync(fixture.Service, new FakeModel(_ => "I can't decide."), Provider, new[] { note.NoteId });

        CollectionAssert.AreEqual(new[] { note.NoteId }, toClassify.Entity.Select(n => n.NoteId).ToList());
        Assert.AreEqual(EnumSelectionSource.Signals, Decision(await SelectAsync(fixture), note.NoteId).Source);
        Assert.IsTrue(answers.IsValid);
        Assert.IsEmpty(answers.Entity);
        Assert.IsEmpty((await fixture.Service.NoteTypes.GetAllAsync()).Entity.Where(t => t.Name == KntConst.RelevanceTag),
            "nothing to keep: «Note relevance» is not created");
    }

    [TestMethod]
    public async Task AGuest_DoesNotAskTheAi()
    {
        using var fixture = await SemanticIndexFixture.CreateAsync();
        var note = await AddNoteAsync(fixture, "Note", "Short text.");
        var user = await fixture.Service.Repository.Users.AddAsync(new UserDto
        {
            UserId = Guid.NewGuid(), UserName = "classify-guest", EMail = "classify-guest@knote.tests", FullName = "Guest", RoleDefinition = "Guest"
        });
        Assert.IsTrue(user.IsValid, user.ErrorMessage);
        var guest = new KNote.Service.Core.KntService(fixture.Db.CreateRepository("Dapper")) { UserIdentityName = "classify-guest", EnforceAuthorization = true };
        var model = new FakeModel(_ => "#1 | In | x");

        var answers = await AiNoteClassifier.ClassifyAsync(guest, model, Provider, new[] { note.NoteId });

        Assert.IsTrue(answers.NotAuthorized);
        Assert.AreEqual(0, model.Calls);
    }

    private static (EnumSelectionDecision Decision, EnumSelectionSource Source) Decision(NoteSelectionResultDto selection, Guid noteId)
    {
        var note = selection.Notes.Single(n => n.NoteId == noteId);
        return (note.Decision, note.Source);
    }

    private static async Task<NoteSelectionResultDto> SelectAsync(SemanticIndexFixture fixture)
    {
        var result = await fixture.Service.Summaries.PrepareSelectionAsync(new NoteSelectionRequestDto { From = From, To = To });
        Assert.IsTrue(result.IsValid, result.ErrorMessage);
        return result.Entity;
    }

    private static async Task SignInAsync(SemanticIndexFixture fixture) =>
        fixture.Service.UserIdentityName = (await fixture.Service.Users.GetAsync(fixture.UserId)).Entity.UserName;

    // A short note of the period, not left the same day: doubtful.
    private static async Task<NoteDto> AddNoteAsync(SemanticIndexFixture fixture, string topic, string description, Guid? noteTypeId = null)
    {
        var note = await fixture.Service.Repository.Notes.AddAsync(new NoteDto
        {
            NoteId = Guid.NewGuid(), Topic = topic, Description = description, FolderId = fixture.HomeId, NoteTypeId = noteTypeId,
            CreationDateTime = InPeriod.AddDays(-1), ModificationDateTime = InPeriod
        });
        Assert.IsTrue(note.IsValid, note.ErrorMessage);
        return note.Entity;
    }

    private sealed class FakeModel(Func<List<ChatMessage>, string> answer) : IChatClient
    {
        public int Calls { get; private set; }
        public string System { get; private set; } = "";
        public string Request { get; private set; } = "";

        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("The classifier streams.");

        public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            Calls++;
            var list = messages.ToList();
            System = list.First(m => m.Role == ChatRole.System).Text;
            Request = list.Last(m => m.Role == ChatRole.User).Text;
            await Task.Yield();
            yield return new ChatResponseUpdate(ChatRole.Assistant, answer(list));
        }

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose() { }
    }
}

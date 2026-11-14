using System.Runtime.CompilerServices;
using KNote.Ai;
using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Service.ServicesCommands;
using KNote.Tests.Helpers;
using Microsoft.Extensions.AI;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// The summary of a long document of a note (KNote Atlas, stage 2, 2.1a, CU-43, D21) with a repository and a fake model: the new
/// note (type, folder, relation, original attached), the note of the document untouched, the batches and the joins, the language,
/// canceling (nothing kept), a document replaced meanwhile, the scanned pages without text, KNote's own notes, the estimate and the
/// pace. Dapper and EF.
/// </summary>
[TestClass]
public class DocumentSummaryServiceTests
{
    private static readonly DateTime NoteDate = new(2026, 1, 2, 3, 4, 5);
    private const char Page = DocumentTextRules.PageSeparator;

    private RepositoryTestDatabase db = null!;
    private KntService service = null!;
    private string rootPath = null!;

    private static AiProviderRef Ollama(int context = 8_192) =>
        new() { Alias = "Local", Provider = EnumAiProvider.Ollama, Model = "qwen3.5:9b", Host = "http://localhost:11434", ContextWindow = context };

    private void Open(string orm)
    {
        db = new RepositoryTestDatabase();
        service = new KntService(db.CreateRepository(orm), activateMessageBroker: false);

        rootPath = Path.Combine(Path.GetTempPath(), $"knote-longdoc-{Guid.NewGuid():N}");
        Directory.CreateDirectory(rootPath);
        var repositoryRef = service.Repository.RepositoryRef;
        repositoryRef.ResourcesContainer = "KntResources";
        repositoryRef.ResourcesContainerRootPath = rootPath;
        repositoryRef.ResourcesContainerRootUrl = "http://intranet/KNote";
    }

    [TestCleanup]
    public void TestCleanup()
    {
        db?.Dispose();
        if (rootPath != null && Directory.Exists(rootPath))
            Directory.Delete(rootPath, true);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task TheSummary_GoesToANewNote_RelatedToTheDocumentsNote_WithTheOriginal(string orm)
    {
        Open(orm);
        var (noteId, folderId, resourceId) = await AddDocumentAsync("QL manual", $"The QL boots.{Page}It has two microdrives.");
        var model = new FakeModel(request => request.Contains("Write its summary") ? "The QL manual explains how it boots (p. 1)." : "- It boots (p. 1)");
        AiProviderRef? used = null;

        var run = await AiDocumentProcessor.SummarizeAsync(service, p => { used = p; return model; }, Ollama(),
            new DocumentProcessRequestDto { ResourceId = resourceId, Language = "Spanish", Length = EnumSummaryLength.Short });

        Assert.IsTrue(run.IsValid, run.ErrorMessage);
        Assert.AreEqual(EnumDocumentProcessPhase.Completed, run.Entity.Phase);
        Assert.AreEqual(8_192, used!.ContextWindow, "KNote fixes the context of the calls (D11)");
        var summary = (await service.Notes.GetExtendedAsync(run.Entity.NoteId!.Value)).Entity;
        Assert.AreEqual("Summary of manual.pdf", summary.Topic);
        Assert.AreEqual(KntConst.SummaryTag, summary.NoteTypeDto.Name);
        Assert.AreEqual(folderId, summary.FolderId, "in the folder of the note of the document");
        var source = (await service.Notes.GetAsync(noteId)).Entity;
        StringAssert.Contains(summary.Description, $"of the note #{source.NoteNumber}");
        StringAssert.EndsWith(summary.Description, "The QL manual explains how it boots (p. 1).");

        var relation = summary.TraceNotesTo.Single();
        Assert.AreEqual(noteId, relation.ToId);
        Assert.AreEqual(KntConst.DerivedFromTraceNoteType, (await service.TraceNoteTypes.GetAsync(relation.TraceNoteTypeId!.Value)).Entity.Name);

        var original = summary.Resources.Single();
        Assert.IsTrue(original.DescriptionByUser, "locked: the stage 4 doesn't describe it");
        StringAssert.EndsWith(original.Name, "Summary of manual.pdf (original).md");
        var file = (await service.Notes.GetResourceAsync(original.ResourceId)).Entity;
        Assert.AreEqual("The QL manual explains how it boots (p. 1).", System.Text.Encoding.UTF8.GetString(file.ContentArrayBytes));

        Assert.AreEqual(NoteDate, source.ModificationDateTime, "the note of the document is not modified");
        var last = model.Requests[^1];
        StringAssert.EndsWith(last.Text, "Write the summary in Spanish.");
        Assert.AreEqual(SummaryBatches.OutputTokens(300), last.Options!.MaxOutputTokens);
        Assert.IsNotNull((await service.Summaries.GetStateAsync()).Entity.SecondsPer1000Characters.GetValueOrDefault("qwen3.5:9b"), "its pace is kept");
    }

    [TestMethod]
    public async Task ALongDocument_IsReadInBatches_AndItsNotesJoinedWhenTheyDontFit()
    {
        Open("EntityFramework");
        var pages = string.Join(Page, Enumerable.Range(1, 30).Select(i => $"Page {i}. " + new string('x', 3_000)));
        var (_, _, resourceId) = await AddDocumentAsync("Long manual", pages);
        var model = new FakeModel(request => request.Contains("Write its summary") ? "The summary." : "- " + new string('n', 1_500));
        var phases = new List<EnumDocumentProcessPhase>();

        var run = await AiDocumentProcessor.SummarizeAsync(service, _ => model, Ollama(3_000),
            new DocumentProcessRequestDto { ResourceId = resourceId, Length = EnumSummaryLength.Short }, new SyncProgress(p => phases.Add(p.Phase)));

        Assert.AreEqual(EnumDocumentProcessPhase.Completed, run.Entity.Phase, run.ErrorMessage);
        var batches = model.Requests.Count(r => r.Text.StartsWith("Read this part"));
        var joins = model.Requests.Count(r => r.Text.StartsWith("These are notes of consecutive parts"));
        Assert.IsGreaterThan(1, batches);
        Assert.IsGreaterThan(0, joins);
        Assert.AreEqual(1, model.Requests.Count(r => r.Text.Contains("Write its summary")));
        Assert.IsTrue(model.Requests.All(r => r.Text.Length + r.System.Length <= 3_000 * SummaryBatches.CharactersPerToken), "every call fits in the context");
        CollectionAssert.IsSubsetOf(new[] { EnumDocumentProcessPhase.Reading, EnumDocumentProcessPhase.Joining, EnumDocumentProcessPhase.Writing,
            EnumDocumentProcessPhase.Saving, EnumDocumentProcessPhase.Completed }, phases);
        StringAssert.Contains(model.Requests[0].Text, "[p. 1]");
    }

    [TestMethod]
    public async Task Canceling_KeepsNothing()
    {
        Open("EntityFramework");
        var pages = string.Join(Page, Enumerable.Range(1, 30).Select(i => new string('x', 3_000)));
        var (_, folderId, resourceId) = await AddDocumentAsync("Long manual", pages);
        using var cancellation = new CancellationTokenSource();
        var model = new FakeModel(_ =>
        {
            cancellation.Cancel();
            return "- Notes";
        });

        var run = await AiDocumentProcessor.SummarizeAsync(service, _ => model, Ollama(3_000),
            new DocumentProcessRequestDto { ResourceId = resourceId, Length = EnumSummaryLength.Short }, cancellationToken: cancellation.Token);

        Assert.AreEqual(EnumDocumentProcessPhase.Canceled, run.Entity.Phase, run.ErrorMessage);
        Assert.HasCount(1, model.Requests, "it stops at once");
        Assert.HasCount(1, (await service.Notes.GetByFolderAsync(folderId)).Entity, "no new note");
    }

    [TestMethod]
    public async Task ADocumentReplacedMeanwhile_IsNotSummarized()
    {
        Open("Dapper");
        var (_, folderId, resourceId) = await AddDocumentAsync("QL manual", "The QL boots.");
        var model = new FakeModel(request =>
        {
            if (request.Contains("Write its summary"))
            {
                var resource = service.Repository.Notes.GetResourceAsync(resourceId).Result.Entity;
                resource.Name = $"{Guid.NewGuid()}_manual.pdf";
                Assert.IsTrue(service.Repository.Notes.UpdateResourceAsync(resource).Result.IsValid);
            }
            return "The summary.";
        });

        var run = await AiDocumentProcessor.SummarizeAsync(service, _ => model, Ollama(), new DocumentProcessRequestDto { ResourceId = resourceId });

        Assert.AreEqual(EnumDocumentProcessPhase.Failed, run.Entity.Phase);
        StringAssert.Contains(run.ErrorMessage, KntSummaryTexts.DocumentChanged);
        Assert.HasCount(1, (await service.Notes.GetByFolderAsync(folderId)).Entity);
    }

    [TestMethod]
    public async Task TheScannedPagesWithoutText_AreLeftOut_AndTheNoteSaysSo()
    {
        Open("EntityFramework");
        var (_, _, resourceId) = await AddDocumentAsync("Scanned manual",
            $"Page one.{Page}{DocumentTextRules.PendingMark}{Page}{DocumentTextRules.PendingMark}{Page}Page four.");
        var model = new FakeModel(request => request.Contains("Write its summary") ? "The summary." : "- Notes");

        var estimate = await AiDocumentProcessor.EstimateAsync(service, Ollama(), new DocumentProcessRequestDto { ResourceId = resourceId });
        var run = await AiDocumentProcessor.SummarizeAsync(service, _ => model, Ollama(), new DocumentProcessRequestDto { ResourceId = resourceId });

        CollectionAssert.AreEqual(new[] { 2, 3 }, estimate.Entity.MissingPages);
        Assert.AreEqual(4, estimate.Entity.Pages);
        Assert.IsFalse(model.Requests[0].Text.Contains("[p. 2]"));
        StringAssert.Contains((await service.Notes.GetAsync(run.Entity.NoteId!.Value)).Entity.Description, "without text yet: 2–3.");
    }

    [TestMethod]
    public async Task TheEstimate_GivesTheCalls_AndTheTimeOnceTheModelHasAPace()
    {
        Open("EntityFramework");
        var (_, _, resourceId) = await AddDocumentAsync("QL manual", string.Join(Page, Enumerable.Range(1, 20).Select(i => new string('x', 3_000))));
        var request = new DocumentProcessRequestDto { ResourceId = resourceId, Length = EnumSummaryLength.Medium };

        var before = (await AiDocumentProcessor.EstimateAsync(service, Ollama(), request)).Entity;
        await AiDocumentProcessor.SummarizeAsync(service, _ => new FakeModel(r => r.Contains("Write its summary") ? "The summary." : "- Notes"), Ollama(), request);
        var after = (await AiDocumentProcessor.EstimateAsync(service, Ollama(), request)).Entity;

        Assert.AreEqual(20, before.Pages);
        Assert.AreEqual(60_000, before.Characters);
        Assert.IsGreaterThanOrEqualTo(2, before.Batches);
        Assert.AreEqual(before.Batches + 1, before.Calls);
        Assert.AreEqual(8_192, before.ContextTokens);
        Assert.IsFalse(before.IsPaid);
        Assert.IsNull(before.Seconds, "no summary with this model yet");
        Assert.IsNotNull(after.Seconds);
    }

    [TestMethod]
    public async Task AContextTooSmallForTheLength_IsSaidBeforeStarting()
    {
        Open("EntityFramework");
        var (_, _, resourceId) = await AddDocumentAsync("QL manual", "The QL boots.");

        var longOne = await AiDocumentProcessor.EstimateAsync(service, Ollama(3_000), new DocumentProcessRequestDto { ResourceId = resourceId, Length = EnumSummaryLength.Long });
        var shortOne = await AiDocumentProcessor.EstimateAsync(service, Ollama(3_000), new DocumentProcessRequestDto { ResourceId = resourceId, Length = EnumSummaryLength.Short });

        Assert.IsFalse(longOne.IsValid);
        StringAssert.Contains(longOne.ErrorMessage, "too small for this summary");
        Assert.IsTrue(shortOne.IsValid, shortOne.ErrorMessage);
    }

    [TestMethod]
    public async Task TheDocumentsOfKNotesOwnNotes_AreLeftOut()
    {
        Open("EntityFramework");
        var noteTypeId = Guid.NewGuid();
        Assert.IsTrue((await service.Repository.NoteTypes.AddAsync(new NoteTypeDto { NoteTypeId = noteTypeId, Name = KntConst.ChatSessionsTag })).IsValid);
        var (_, _, resourceId) = await AddDocumentAsync("A session", "Its attachment.", noteTypeId);

        var prepared = await service.Summaries.PrepareDocumentAsync(resourceId);

        Assert.IsFalse(prepared.IsValid);
    }

    [TestMethod]
    public async Task OnlyStaff_SummarizesADocument()
    {
        Open("EntityFramework");
        var (_, _, resourceId) = await AddDocumentAsync("QL manual", "The QL boots.");
        service.EnforceAuthorization = true;
        service.UserIdentityName = "nobody";

        var prepared = await service.Summaries.PrepareDocumentAsync(resourceId);

        Assert.IsTrue(prepared.NotAuthorized);
    }

    #region Helpers

    private sealed record Request(string System, string Text, ChatOptions? Options);

    // A model that answers what the function gives for each request (the last message of the user).
    private sealed class FakeModel(Func<string, string> answer) : IChatClient
    {
        public List<Request> Requests { get; } = [];

        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("The summaries stream.");

        public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var list = messages.ToList();
            var text = list.Last(m => m.Role == ChatRole.User).Text;
            Requests.Add(new Request(list.First(m => m.Role == ChatRole.System).Text, text, options));
            var reply = answer(text);
            await Task.Yield();
            yield return new ChatResponseUpdate(ChatRole.Assistant, reply);
        }

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose() { }
    }

    // Progress reported at once, in the caller's thread (Progress<T> posts it).
    private sealed class SyncProgress(Action<DocumentProcessProgressDto> report) : IProgress<DocumentProcessProgressDto>
    {
        public void Report(DocumentProcessProgressDto value) => report(value);
    }

    private async Task<(Guid NoteId, Guid FolderId, Guid ResourceId)> AddDocumentAsync(string topic, string text, Guid? noteTypeId = null)
    {
        var folder = await service.Repository.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = $"Manuals {Guid.NewGuid():N}" });
        Assert.IsTrue(folder.IsValid, folder.ErrorMessage);

        var noteId = Guid.NewGuid();
        var note = await service.Repository.Notes.AddAsync(new NoteDto
        {
            NoteId = noteId, Topic = topic, Description = "", ContentType = "markdown", FolderId = folder.Entity.FolderId, NoteTypeId = noteTypeId,
            CreationDateTime = NoteDate, ModificationDateTime = NoteDate
        });
        Assert.IsTrue(note.IsValid, note.ErrorMessage);

        var resource = await service.Repository.Notes.AddResourceAsync(new ResourceDto
        {
            ResourceId = Guid.NewGuid(), NoteId = noteId, Name = $"{Guid.NewGuid()}_manual.pdf", Description = "manual.pdf", ContentInDB = true,
            ContentArrayBytes = [1], FileType = "application/pdf", Container = "KntResources"
        });
        Assert.IsTrue(resource.IsValid, resource.ErrorMessage);
        var state = DocumentTextRules.StateOf(DocumentTextRules.Pages(text));
        Assert.IsTrue((await service.Repository.Notes.SaveResourceTextAsync(resource.Entity.ResourceId, text, state, resource.Entity.Name)).Entity);
        return (noteId, folder.Entity.FolderId, resource.Entity.ResourceId);
    }

    #endregion
}

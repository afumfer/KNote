using System.Runtime.CompilerServices;
using System.Text;
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
/// The translation of a long document of a note (KNote Atlas, stage 2, 2.1b, CU-43, D21) with a repository and a fake model: the new
/// note (type, folder, relation, the translation attached with the pages of the document), the note of the document untouched,
/// canceling (what was translated stays) and «Continue», a part that comes back too short, a document replaced meanwhile, a document
/// without pages, the estimate and the pace, and the translation read again by pages. Dapper and EF.
/// </summary>
[TestClass]
public class DocumentTranslationServiceTests
{
    private static readonly DateTime NoteDate = new(2026, 1, 2, 3, 4, 5);
    private const char Page = DocumentTextRules.PageSeparator;

    private RepositoryTestDatabase db = null!;
    private KntService service = null!;
    private string rootPath = null!;

    private static AiProviderRef Ollama(int context = 16_384) =>
        new() { Alias = "Local", Provider = EnumAiProvider.Ollama, Model = "qwen3.5:9b", Host = "http://localhost:11434", ContextWindow = context };

    private void Open(string orm)
    {
        db = new RepositoryTestDatabase();
        service = new KntService(db.CreateRepository(orm), activateMessageBroker: false);

        rootPath = Path.Combine(Path.GetTempPath(), $"knote-translation-{Guid.NewGuid():N}");
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
    public async Task TheTranslation_GoesToANewNote_WithTheDocumentAttached_ByPages(string orm)
    {
        Open(orm);
        var (noteId, folderId, resourceId) = await AddDocumentAsync("QL manual", $"The QL boots.{Page}It has two microdrives.");
        var model = new FakeModel(request => request.Contains("(p. 1)") ? "El QL arranca." : "Tiene dos microdrives.");
        AiProviderRef? used = null;

        var run = await AiDocumentProcessor.ProcessAsync(service, p => { used = p; return model; }, Ollama(),
            new DocumentProcessRequestDto { ResourceId = resourceId, Kind = EnumDocumentProcessKind.Translate, Language = "Spanish" });

        Assert.IsTrue(run.IsValid, run.ErrorMessage);
        Assert.AreEqual(EnumDocumentProcessPhase.Completed, run.Entity.Phase);
        Assert.AreEqual(16_384, used!.ContextWindow, "KNote fixes the context of the calls (D11)");
        var translation = (await service.Notes.GetExtendedAsync(run.Entity.NoteId!.Value)).Entity;
        Assert.AreEqual("Translation of manual.pdf into Spanish", translation.Topic);
        Assert.AreEqual(KntConst.TranslationTag, translation.NoteTypeDto.Name);
        Assert.AreEqual(folderId, translation.FolderId, "in the folder of the note of the document");
        StringAssert.Contains(translation.Description, "Translated: all the 2 pages.");

        var relation = translation.TraceNotesTo.Single();
        Assert.AreEqual(noteId, relation.ToId);
        Assert.AreEqual(KntConst.DerivedFromTraceNoteType, (await service.TraceNoteTypes.GetAsync(relation.TraceNoteTypeId!.Value)).Entity.Name);

        var document = translation.Resources.Single();
        Assert.IsTrue(document.DescriptionByUser, "locked: the stage 4 doesn't describe it");
        StringAssert.EndsWith(document.Name, "manual (Spanish).md");
        Assert.AreEqual($"El QL arranca.{Page}Tiene dos microdrives.", await FileTextAsync(document.ResourceId));

        Assert.AreEqual(NoteDate, (await service.Notes.GetAsync(noteId)).Entity.ModificationDateTime, "the note of the document is not modified");
        Assert.HasCount(2, model.Requests, "a call per page");
        StringAssert.EndsWith(model.Requests[0].Text, "Translate the text between <text> and </text> into Spanish.");
        StringAssert.Contains(model.Requests[0].System, "You translate a long document");
        Assert.IsNotNull((await service.Summaries.GetStateAsync()).Entity.TranslationSecondsPer1000Characters.GetValueOrDefault("qwen3.5:9b"), "its pace is kept");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task Canceling_KeepsWhatWasTranslated_AndContinue_EndsIt(string orm)
    {
        Open(orm);
        var pages = string.Join(Page, Enumerable.Range(1, 6).Select(i => $"Page {i}. " + new string('x', 2_500)));
        var (_, folderId, resourceId) = await AddDocumentAsync("Long manual", pages);
        using var cancellation = new CancellationTokenSource();
        var first = new FakeModel(request =>
        {
            if (request.Contains("(p. 4)"))
                cancellation.Cancel();
            return Translated(request);
        });
        var request = new DocumentProcessRequestDto { ResourceId = resourceId, Kind = EnumDocumentProcessKind.Translate, Language = "Spanish" };

        var canceled = await AiDocumentProcessor.ProcessAsync(service, _ => first, Ollama(), request, cancellationToken: cancellation.Token);

        Assert.AreEqual(EnumDocumentProcessPhase.Canceled, canceled.Entity.Phase, canceled.ErrorMessage);
        Assert.IsNotNull(canceled.Entity.NoteId, "what was translated is kept");
        var left = (await service.Summaries.GetDocumentTranslationsAsync(resourceId)).Entity.Single();
        Assert.IsFalse(left.Finished);
        Assert.AreEqual(4, left.NextPart);
        Assert.AreEqual(5, left.NextPage);
        StringAssert.Contains((await service.Notes.GetAsync(left.NoteId)).Entity.Description, "Translated: pages 1–4 of 6. Not finished");
        Assert.HasCount(4, DocumentTextRules.Pages(await FileTextAsync(left.TranslationResourceId)));

        var estimate = await AiDocumentProcessor.EstimateAsync(service, Ollama(), new DocumentProcessRequestDto
        {
            ResourceId = resourceId, Kind = EnumDocumentProcessKind.Translate, ContinueNoteId = left.NoteId
        });
        Assert.AreEqual(5, estimate.Entity.FromPage);
        Assert.AreEqual(2, estimate.Entity.Calls, "what is left");

        var second = new FakeModel(Translated);
        var continued = await AiDocumentProcessor.ProcessAsync(service, _ => second, Ollama(),
            new DocumentProcessRequestDto { ResourceId = resourceId, Kind = EnumDocumentProcessKind.Translate, ContinueNoteId = left.NoteId });

        Assert.AreEqual(EnumDocumentProcessPhase.Completed, continued.Entity.Phase, continued.ErrorMessage);
        Assert.AreEqual(left.NoteId, continued.Entity.NoteId, "the same note");
        StringAssert.Contains(second.Requests[0].Text, "(p. 5)");
        Assert.HasCount(2, second.Requests);
        StringAssert.EndsWith(second.Requests[0].Text, "Translate the text between <text> and </text> into Spanish.", "in the language it was started with");
        var file = DocumentTextRules.Pages(await FileTextAsync(left.TranslationResourceId));
        CollectionAssert.AreEqual(Enumerable.Range(1, 6).Select(i => $"Página {i}").ToArray(), file.Select(p => p.Text[..p.Text.IndexOf('.')]).ToArray());
        Assert.HasCount(2, (await service.Notes.GetByFolderAsync(folderId)).Entity, "the document's note and its translation");
        Assert.IsTrue((await service.Summaries.GetDocumentTranslationsAsync(resourceId)).Entity.Single().Finished);
    }

    [TestMethod]
    public async Task APartThatComesBackTooShort_IsAskedAgain_AndSaidIfStillShort()
    {
        Open("EntityFramework");
        var (_, _, resourceId) = await AddDocumentAsync("QL manual", new string('x', 1_000));
        var model = new FakeModel(_ => "Corto.");

        var run = await AiDocumentProcessor.ProcessAsync(service, _ => model, Ollama(),
            new DocumentProcessRequestDto { ResourceId = resourceId, Kind = EnumDocumentProcessKind.Translate, Language = "Spanish" });

        Assert.AreEqual(EnumDocumentProcessPhase.Completed, run.Entity.Phase, run.ErrorMessage);
        Assert.HasCount(2, model.Requests, "asked again once");
        var translation = (await service.Summaries.GetDocumentTranslationsAsync(resourceId)).Entity.Single();
        Assert.AreEqual($"Corto.\n\n{DocumentTranslationRules.IncompleteLine}", await FileTextAsync(translation.TranslationResourceId));
    }

    [TestMethod]
    public async Task ADocumentReplacedMeanwhile_IsNotTranslated()
    {
        Open("Dapper");
        var (_, folderId, resourceId) = await AddDocumentAsync("QL manual", "The QL boots.");
        var model = new FakeModel(_ =>
        {
            var resource = service.Repository.Notes.GetResourceAsync(resourceId).Result.Entity;
            resource.Name = $"{Guid.NewGuid()}_manual.pdf";
            Assert.IsTrue(service.Repository.Notes.UpdateResourceAsync(resource).Result.IsValid);
            return "El QL arranca.";
        });

        var run = await AiDocumentProcessor.ProcessAsync(service, _ => model, Ollama(),
            new DocumentProcessRequestDto { ResourceId = resourceId, Kind = EnumDocumentProcessKind.Translate, Language = "Spanish" });

        Assert.AreEqual(EnumDocumentProcessPhase.Failed, run.Entity.Phase);
        StringAssert.Contains(run.ErrorMessage, KntSummaryTexts.DocumentChanged);
        Assert.HasCount(1, (await service.Notes.GetByFolderAsync(folderId)).Entity);
    }

    [TestMethod]
    public async Task ADocumentWithoutPages_IsTranslatedInPartsThatFit_AndKeepsNoPages()
    {
        Open("EntityFramework");
        var text = string.Join("\n\n", Enumerable.Range(1, 4).Select(i => $"Part {i}. " + new string('x', 2_800)));
        var (_, _, resourceId) = await AddDocumentAsync("Notes", text, fileName: "notes.txt", fileType: "text/plain");
        var model = new FakeModel(r => "Traducido " + new string('y', r.Length * 3 / 4));

        var run = await AiDocumentProcessor.ProcessAsync(service, _ => model, Ollama(),
            new DocumentProcessRequestDto { ResourceId = resourceId, Kind = EnumDocumentProcessKind.Translate, Language = "Spanish" });

        Assert.AreEqual(EnumDocumentProcessPhase.Completed, run.Entity.Phase, run.ErrorMessage);
        Assert.HasCount(2, model.Requests, "two parts of about 2,800 fit in a call of 8,000");
        Assert.IsFalse(model.Requests[0].Text.Contains("(p. "));
        var translation = (await service.Summaries.GetDocumentTranslationsAsync(resourceId)).Entity.Single();
        var file = await FileTextAsync(translation.TranslationResourceId);
        Assert.IsFalse(file.Contains(Page), "no pages");
        StringAssert.Contains((await service.Notes.GetAsync(translation.NoteId)).Entity.Description, "Translated: all of it.");
    }

    [TestMethod]
    public async Task TheEstimate_GivesACallPerPage_AndTheTimeOnceTheModelHasAPace()
    {
        Open("EntityFramework");
        var (_, _, resourceId) = await AddDocumentAsync("QL manual",
            $"One{Page}{DocumentTextRules.PendingMark}{Page}Three{Page}Four");
        var request = new DocumentProcessRequestDto { ResourceId = resourceId, Kind = EnumDocumentProcessKind.Translate, Language = "French" };

        var before = (await AiDocumentProcessor.EstimateAsync(service, Ollama(), request)).Entity;
        await AiDocumentProcessor.ProcessAsync(service, _ => new FakeModel(_ => "Un"), Ollama(), request);
        var after = (await AiDocumentProcessor.EstimateAsync(service, Ollama(), request)).Entity;

        Assert.AreEqual(EnumDocumentProcessKind.Translate, before.Kind);
        Assert.AreEqual(3, before.Calls, "the scanned page without text is not sent");
        CollectionAssert.AreEqual(new[] { 2 }, before.MissingPages);
        Assert.AreEqual(4, before.Pages);
        Assert.IsNull(before.Seconds, "no translation with this model yet");
        Assert.IsNull(before.FromPage);
        Assert.IsNotNull(after.Seconds);
    }

    [TestMethod]
    public async Task ATranslation_NeedsALanguage()
    {
        Open("EntityFramework");
        var (_, _, resourceId) = await AddDocumentAsync("QL manual", "The QL boots.");

        var estimate = await AiDocumentProcessor.EstimateAsync(service, Ollama(),
            new DocumentProcessRequestDto { ResourceId = resourceId, Kind = EnumDocumentProcessKind.Translate });

        Assert.IsFalse(estimate.IsValid);
        StringAssert.Contains(estimate.ErrorMessage, KntSummaryTexts.LanguageRequired);
    }

    // D21: the stage 4 reads the translation of a document with pages by its pages (the index cites them, the viewers show them).
    [TestMethod]
    public async Task TheTranslation_IsReadAgainByPages()
    {
        Open("EntityFramework");
        service.DocumentReader = new AiDocumentReader();
        var (_, _, resourceId) = await AddDocumentAsync("QL manual", $"The QL boots.{Page}It has two microdrives.");
        await AiDocumentProcessor.ProcessAsync(service, _ => new FakeModel(r => r.Contains("(p. 1)") ? "Uno." : "Dos."), Ollama(),
            new DocumentProcessRequestDto { ResourceId = resourceId, Kind = EnumDocumentProcessKind.Translate, Language = "Spanish" });
        var translation = (await service.Summaries.GetDocumentTranslationsAsync(resourceId)).Entity.Single();

        var read = await service.ResourceContent.ReadDocumentAsync(translation.TranslationResourceId);
        var text = await service.ResourceContent.GetDocumentTextAsync(translation.TranslationResourceId);

        Assert.IsTrue(read.IsValid, read.ErrorMessage);
        CollectionAssert.AreEqual(new[] { "Uno.", "Dos." }, DocumentTextRules.Pages(text.Entity.Text).Select(p => p.Text).ToArray());
    }

    [TestMethod]
    public async Task OnlyStaff_KeepsATranslation()
    {
        Open("EntityFramework");
        var (_, _, resourceId) = await AddDocumentAsync("QL manual", "The QL boots.");
        service.EnforceAuthorization = true;
        service.UserIdentityName = "nobody";

        var saved = await service.Summaries.SaveDocumentTranslationAsync(new DocumentTranslationSaveDto
        {
            ResourceId = resourceId, Model = "m", Language = "Spanish", Parts = 1, NextPart = 1, Text = "El QL arranca."
        });

        Assert.IsTrue(saved.NotAuthorized);
    }

    #region Helpers

    // A translation of a page of the requests: «Página N.» and as long as the page, so that it doesn't look incomplete.
    private static string Translated(string request)
    {
        var start = request.IndexOf("Page ", StringComparison.Ordinal);
        var number = request[(start + 5)..request.IndexOf('.', start)];
        return $"Página {number}. " + new string('y', 2_000);
    }

    private async Task<string> FileTextAsync(Guid resourceId)
    {
        var file = (await service.Notes.GetResourceAsync(resourceId)).Entity;
        return Encoding.UTF8.GetString(file.ContentArrayBytes);
    }

    private sealed record Request(string System, string Text, ChatOptions? Options);

    // A model that answers what the function gives for each request (the last message of the user).
    private sealed class FakeModel(Func<string, string> answer) : IChatClient
    {
        public List<Request> Requests { get; } = [];

        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("The translations stream.");

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

    private async Task<(Guid NoteId, Guid FolderId, Guid ResourceId)> AddDocumentAsync(string topic, string text,
        string fileName = "manual.pdf", string fileType = "application/pdf")
    {
        var folder = await service.Repository.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = $"Manuals {Guid.NewGuid():N}" });
        Assert.IsTrue(folder.IsValid, folder.ErrorMessage);

        var noteId = Guid.NewGuid();
        var note = await service.Repository.Notes.AddAsync(new NoteDto
        {
            NoteId = noteId, Topic = topic, Description = "", ContentType = "markdown", FolderId = folder.Entity.FolderId,
            CreationDateTime = NoteDate, ModificationDateTime = NoteDate
        });
        Assert.IsTrue(note.IsValid, note.ErrorMessage);

        var resource = await service.Repository.Notes.AddResourceAsync(new ResourceDto
        {
            ResourceId = Guid.NewGuid(), NoteId = noteId, Name = $"{Guid.NewGuid()}_{fileName}", Description = fileName, ContentInDB = true,
            ContentArrayBytes = [1], FileType = fileType, Container = "KntResources"
        });
        Assert.IsTrue(resource.IsValid, resource.ErrorMessage);
        var state = DocumentTextRules.StateOf(DocumentTextRules.Pages(text));
        Assert.IsTrue((await service.Repository.Notes.SaveResourceTextAsync(resource.Entity.ResourceId, text, state, resource.Entity.Name)).Entity);
        return (noteId, folder.Entity.FolderId, resource.Entity.ResourceId);
    }

    #endregion
}

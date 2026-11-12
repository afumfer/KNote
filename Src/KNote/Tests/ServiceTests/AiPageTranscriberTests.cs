using System.Runtime.CompilerServices;
using KNote.Ai;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;
using Microsoft.Extensions.AI;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// AiPageTranscriber and «Process pending resources» with scanned pages (KNote Atlas, stage 4, 4.5), with a repository, the reader
/// of the app (it draws the pages with PDFium) and a fake model: a scanned page is drawn and sent to the model with the task
/// TranscribePage (no tools), and its transcription kept in place of the OCR layer, marked as transcribed, without the images the
/// model invents and without touching the date of the note; what is not transcribed (a page not pending, a file not here, one
/// replaced meanwhile); reading the document again keeps the transcriptions; the batch transcribes the pages (also the ones of the
/// documents it reads in the run, with or without the OCR ones) and then summarizes the scanned documents. Dapper and EF.
/// </summary>
[TestClass]
public class AiPageTranscriberTests
{
    private static readonly AiProviderRef Ollama = new() { Alias = "Local", Provider = EnumAiProvider.Ollama, Model = "qwen3.5:9b", Host = "http://localhost:11434" };
    private const string Pdf = "application/pdf";

    private RepositoryTestDatabase db = null!;
    private KntService service = null!;
    private string rootPath = null!;

    private void Open(string orm = "EntityFramework")
    {
        db = new RepositoryTestDatabase();
        service = new KntService(db.CreateRepository(orm), activateMessageBroker: false) { DocumentReader = new AiDocumentReader() };

        rootPath = Path.Combine(Path.GetTempPath(), $"knote-pages-{Guid.NewGuid():N}");
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
    public async Task AScannedPage_IsDrawnTranscribedAndKept_WithoutTouchingTheNote(string orm)
    {
        Open(orm);
        var noteId = await AddNoteAsync("Manuales del QL");
        var pdfId = await AddResourceAsync(noteId, "ql.pdf", TestDocuments.MixedPdf("The first page.", "SCANNED PAGE"));
        Assert.IsTrue((await service.ResourceContent.ReadDocumentAsync(pdfId)).IsValid);
        var dateBefore = (await service.Repository.Notes.GetAsync(noteId)).Entity.ModificationDateTime;
        var model = new FakeModel(_ => "```markdown\n# SCANNED PAGE\n\n![Image](https://i.imgur.com/your_image_link_here)\nIts text.\n```");

        var scanned = (await service.ResourceContent.GetScannedDocumentsAsync(pdfId)).Entity.Single();
        var res = await AiPageTranscriber.TranscribeAsync(service, model, Ollama, pdfId, 2);

        CollectionAssert.AreEqual(new[] { 2 }, scanned.PendingPages);
        Assert.AreEqual(0, scanned.OcrPages.Count);
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        Assert.AreEqual((EnumExtractedState.Complete, 2, 0), (res.Entity.State, res.Entity.Pages, res.Entity.PagesToTranscribe));
        var pages = DocumentTextRules.Pages((await service.ResourceContent.GetDocumentTextAsync(pdfId)).Entity.Text);
        Assert.IsFalse(pages[0].Transcribed);
        Assert.IsTrue(pages[1].Transcribed);
        Assert.AreEqual("# SCANNED PAGE\n\nIts text.", pages[1].Text, "without the code block nor the image the model invented");
        Assert.AreEqual(dateBefore, (await service.Repository.Notes.GetAsync(noteId)).Entity.ModificationDateTime);
        Assert.AreEqual(0, (await service.ResourceContent.GetScannedDocumentsAsync()).Entity.Count);

        StringAssert.Contains(model.Messages[0].Text, "You transcribe a scanned page");
        var request = model.Messages[1];
        StringAssert.Contains(request.Text, "This is page 2 of 2 of the scanned document «ql.pdf».");
        StringAssert.EndsWith(request.Text, "without translating it.");
        var image = request.Contents.OfType<DataContent>().Single();
        Assert.AreEqual("image/jpeg", image.MediaType);
        Assert.IsTrue(image.Data.Length > 1000, "the page drawn");
        Assert.IsNull(model.Options!.Tools);
        Assert.AreEqual(AiTranscriptionRules.MaxOutputTokens, model.Options.MaxOutputTokens);
        Assert.AreEqual(ReasoningEffort.None, model.Options.Reasoning?.Effort);
    }

    [TestMethod]
    public async Task WhatIsNotTranscribed_SaysWhy()
    {
        Open();
        var noteId = await AddNoteAsync("Docs");
        var text = await AddResourceAsync(noteId, "text.pdf", TestDocuments.Pdf("Only text."));
        var away = await AddResourceAsync(noteId, "away.pdf", TestDocuments.ScannedPdf("FAR AWAY"));
        Assert.IsTrue((await service.ResourceContent.ReadDocumentAsync(text)).IsValid);
        Assert.IsTrue((await service.ResourceContent.ReadDocumentAsync(away)).IsValid);
        foreach (var file in Directory.GetFiles(rootPath, "*away.pdf", SearchOption.AllDirectories))
            File.Delete(file);
        var model = new FakeModel(_ => "Text.");

        async Task<string> ErrorOf(Guid id, int page) => (await AiPageTranscriber.TranscribeAsync(service, model, Ollama, id, page)).ErrorMessage;

        StringAssert.Contains(await ErrorOf(text, 1), AiTranscriptionRules.NotPendingMessage);
        StringAssert.Contains(await ErrorOf(text, 9), AiTranscriptionRules.NotPendingMessage);
        StringAssert.Contains(await ErrorOf(away, 1), AiDocumentRules.FileNotAvailableMessage);
        StringAssert.Contains(await ErrorOf(text, 0), "required");
        Assert.AreEqual(0, model.Calls, "the model is not called");
    }

    [TestMethod]
    public async Task ATranscription_IsNotKept_IfTheFileWasReplaced_OrThePageIsNotPending()
    {
        Open();
        var pdfId = await AddResourceAsync(await AddNoteAsync("Docs"), "scan.pdf", TestDocuments.ScannedPdf("ONE", "TWO"));
        Assert.IsTrue((await service.ResourceContent.ReadDocumentAsync(pdfId)).IsValid);
        var context = (await service.ResourceContent.GetPageContextAsync(pdfId, 1)).Entity;

        var replaced = await service.ResourceContent.SaveTranscriptionAsync(new PageTranscriptionSaveDto
        {
            ResourceId = pdfId, Page = 1, Text = "ONE", ExpectedName = "another_file.pdf"
        });
        var saved = await service.ResourceContent.SaveTranscriptionAsync(new PageTranscriptionSaveDto
        {
            ResourceId = pdfId, Page = 1, Text = AiTranscriptionRules.NoText, ExpectedName = context.ExpectedName
        });
        var again = await service.ResourceContent.SaveTranscriptionAsync(new PageTranscriptionSaveDto
        {
            ResourceId = pdfId, Page = 1, Text = "ONE", ExpectedName = context.ExpectedName
        });

        StringAssert.Contains(replaced.ErrorMessage, "replaced");
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        Assert.AreEqual((EnumExtractedState.Partial, 1), (saved.Entity.State, saved.Entity.PagesToTranscribe));
        StringAssert.Contains(again.ErrorMessage, AiTranscriptionRules.NotPendingMessage);
        var page = DocumentTextRules.Pages((await service.ResourceContent.GetDocumentTextAsync(pdfId)).Entity.Text)[0];
        Assert.AreEqual((true, ""), (page.Transcribed, page.Text), "a page without text: transcribed, empty");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ReadingADocumentAgain_KeepsItsTranscriptions(string orm)
    {
        Open(orm);
        var pdfId = await AddResourceAsync(await AddNoteAsync("Docs"), "scan.pdf", TestDocuments.ScannedPdf("ONE", "TWO"));
        Assert.IsTrue((await service.ResourceContent.ReadDocumentAsync(pdfId)).IsValid);
        Assert.IsTrue((await AiPageTranscriber.TranscribeAsync(service, new FakeModel(_ => "Page one."), Ollama, pdfId, 1)).IsValid);

        var read = await service.ResourceContent.ReadDocumentAsync(pdfId);

        Assert.IsTrue(read.IsValid, read.ErrorMessage);
        Assert.AreEqual(1, read.Entity.PagesToTranscribe);
        var pages = DocumentTextRules.Pages((await service.ResourceContent.GetDocumentTextAsync(pdfId)).Entity.Text);
        Assert.AreEqual((true, "Page one."), (pages[0].Transcribed, pages[0].Text));
        Assert.IsTrue(pages[1].NeedsTranscription);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ARun_TranscribesThePages_AlsoOfTheDocumentsItReads_AndThenSummarizesThem(string orm)
    {
        Open(orm);
        var noteId = await AddNoteAsync("Manuales");
        var read = await AddResourceAsync(noteId, "read.pdf", TestDocuments.ScannedPdf("FIRST", "OCR:SECOND"));
        Assert.IsTrue((await service.ResourceContent.ReadDocumentAsync(read)).IsValid);
        var notRead = await AddResourceAsync(noteId, "new.pdf", TestDocuments.ScannedPdf("THIRD"));
        var model = new FakeModel(messages => messages[0].Text.Contains("You transcribe") ? "A transcribed page." : "A summary.");

        var before = (await service.ResourceContent.GetProcessingStatusAsync()).Entity;
        var run = await AiResourceProcessor.ProcessAsync(service, model, Ollama, new ResourceProcessingOptionsDto { Photos = false, OcrPages = false });

        Assert.AreEqual((2, 1, 1, 1), (before.PendingPages, before.OcrPages, before.ScannedDocuments, before.DocumentsToRead));
        Assert.AreEqual(EnumResourceProcessingPhase.Completed, run.Entity.Phase, run.Entity.Message);
        Assert.AreEqual(0, run.Entity.ErrorsCount, string.Join(" ", run.Entity.Errors));
        Assert.AreEqual((2, 2, 2), (run.Entity.PagesTotal, run.Entity.PagesDone, run.Entity.PagesTranscribed), "the first one and the one read in the run, not the OCR one");
        Assert.AreEqual((2, 2, 0), (run.Entity.DocumentsTotal, run.Entity.DocumentsSummarized, run.Entity.DocumentsWaiting), "summarized once transcribed");
        Assert.AreEqual("A summary.", (await service.Notes.GetResourceAsync(notRead)).Entity.Description);

        var after = (await service.ResourceContent.GetProcessingStatusAsync()).Entity;
        Assert.AreEqual((1, 1, 1), (after.PendingPages, after.OcrPages, after.ScannedDocuments), "the OCR page stays pending");
        Assert.IsNotNull(after.SecondsPerPage);
        var pages = DocumentTextRules.Pages((await service.ResourceContent.GetDocumentTextAsync(read)).Entity.Text);
        Assert.AreEqual((true, false), (pages[0].Transcribed, pages[1].Transcribed));
        StringAssert.Contains(pages[1].Text, "SECOND", "its OCR layer, until it is transcribed");
    }

    [TestMethod]
    public async Task ARunWithoutScannedPages_LeavesThemPending()
    {
        Open();
        var pdfId = await AddResourceAsync(await AddNoteAsync("Docs"), "scan.pdf", TestDocuments.ScannedPdf("ONE"));
        var model = new FakeModel(_ => "Text.");

        var run = await AiResourceProcessor.ProcessAsync(service, model, Ollama, new ResourceProcessingOptionsDto { Photos = false, ScannedPages = false });

        Assert.AreEqual((0, 1, 1), (run.Entity.PagesTotal, run.Entity.DocumentsRead, run.Entity.DocumentsWaiting));
        Assert.AreEqual(0, model.Calls);
        Assert.AreEqual(1, (await service.ResourceContent.GetProcessingStatusAsync()).Entity.PendingPages);
    }

    #region Helpers

    private sealed class FakeModel(Func<List<ChatMessage>, string> answer) : IChatClient
    {
        public List<ChatMessage> Messages { get; private set; } = [];
        public ChatOptions? Options { get; private set; }
        public int Calls { get; private set; }

        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("The transcriber streams.");

        public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            Calls++;
            Messages = messages.ToList();
            Options = options;
            await Task.Yield();
            yield return new ChatResponseUpdate(ChatRole.Assistant, answer(Messages));
        }

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose() { }
    }

    private async Task<Guid> AddResourceAsync(Guid noteId, string name, byte[] content)
    {
        var saved = await service.Notes.SaveResourceAsync(new ResourceDto
        {
            NoteId = noteId,
            Name = $"{Guid.NewGuid()}_{name}",
            Description = name,
            FileType = Pdf,
            ContentArrayBytes = content
        });
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        return saved.Entity.ResourceId;
    }

    private async Task<Guid> AddNoteAsync(string topic)
    {
        var folder = await service.Repository.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "Documents folder" });
        Assert.IsTrue(folder.IsValid, folder.ErrorMessage);

        var noteId = Guid.NewGuid();
        var date = new DateTime(2026, 1, 2, 3, 4, 5);
        var note = await service.Repository.Notes.AddAsync(new NoteDto
        {
            NoteId = noteId, Topic = topic, FolderId = folder.Entity.FolderId, CreationDateTime = date, ModificationDateTime = date
        });
        Assert.IsTrue(note.IsValid, note.ErrorMessage);
        return noteId;
    }

    #endregion
}

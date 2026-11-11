using System.Runtime.CompilerServices;
using KNote.Ai;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;
using Microsoft.Extensions.AI;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// AiDocumentSummarizer and «Process pending resources» with documents (KNote Atlas, stage 4, 4.4c), with a repository, the
/// reader of the app and a fake model: a document not read is read first and summarized with the context of its note (no tools,
/// the topic named in the last line); what is not summarized (a locked one, a person's description without asking, a scanned one
/// without text, one that can't be read, one whose file is not here) and the date of the note, which never changes. The batch
/// reads the documents not read yet, also the locked ones, summarizes the pending ones and keeps its pace. Dapper and EF.
/// </summary>
[TestClass]
public class AiDocumentSummarizerTests
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

        rootPath = Path.Combine(Path.GetTempPath(), $"knote-summaries-{Guid.NewGuid():N}");
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
    public async Task ADocument_IsReadAndSummarized_WithTheContextOfItsNote_AndSavedWithItsModel(string orm)
    {
        Open(orm);
        var noteId = await AddNoteAsync("Manuales del QL", "Documentación de la ampliación.");
        var pdfId = await AddResourceAsync(noteId, "q68.pdf", TestDocuments.Pdf("The Q68 user manual.", "Chapter 2. SD cards."));
        var dateBefore = (await service.Repository.Notes.GetAsync(noteId)).Entity.ModificationDateTime;
        var model = new FakeModel("**Resumen:** Manual del Q68: tarjetas SD.");

        var res = await AiDocumentSummarizer.SummarizeAsync(service, model, Ollama, pdfId, replacePersonDescription: false);

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        var stored = (await service.Notes.GetResourceAsync(pdfId)).Entity;
        Assert.AreEqual("Manual del Q68: tarjetas SD.", stored.Description, "without its label");
        Assert.AreEqual("qwen3.5:9b", stored.DescriptionModel);
        Assert.AreEqual(dateBefore, (await service.Repository.Notes.GetAsync(noteId)).Entity.ModificationDateTime);
        Assert.AreEqual(EnumExtractedState.Complete, (await service.ResourceContent.GetDocumentTextAsync(pdfId)).Entity.State, "read first");

        StringAssert.Contains(model.Messages[0].Text, "You summarize a document attached to a note");
        var request = model.Messages[1].Text;
        StringAssert.Contains(request, "Topic: Manuales del QL");
        StringAssert.Contains(request, "The document: q68.pdf, 2 pages.");
        StringAssert.Contains(request, "[Page 2]");
        StringAssert.Contains(request, "SD cards.");
        StringAssert.EndsWith(request, "Write in the language of the topic of the note («Manuales del QL»).");
        Assert.IsNull(model.Options!.Tools);
        Assert.AreEqual(ReasoningEffort.None, model.Options.Reasoning?.Effort);
    }

    [TestMethod]
    public async Task APersonsDescription_IsOnlyReplacedWhenAsked_AndTheModelReceivesIt()
    {
        Open();
        var pdfId = await AddResourceAsync(await AddNoteAsync("Docs", ""), "letter.pdf", TestDocuments.Pdf("Dear friend."), "Carta de la abuela Carmen.");
        var model = new FakeModel("A letter of grandma Carmen.");

        var refused = await AiDocumentSummarizer.SummarizeAsync(service, model, Ollama, pdfId, replacePersonDescription: false);
        var replaced = await AiDocumentSummarizer.SummarizeAsync(service, model, Ollama, pdfId, replacePersonDescription: true);

        StringAssert.Contains(refused.ErrorMessage, "written by a person");
        Assert.IsTrue(replaced.IsValid, replaced.ErrorMessage);
        StringAssert.Contains(model.Messages[1].Text, "What a person wrote of this document (it is true: keep its names and facts):\nCarta de la abuela Carmen.");
    }

    [TestMethod]
    public async Task WhatIsNotSummarized_SaysWhy()
    {
        Open();
        var noteId = await AddNoteAsync("Docs", "");
        var locked = await AddResourceAsync(noteId, "locked.pdf", TestDocuments.Pdf("Mine."), "Mine.", locked: true);
        var scanned = await AddResourceAsync(noteId, "scan.pdf", TestDocuments.MixedPdf("", "SCANNED PAGE"));
        var damaged = await AddResourceAsync(noteId, "damaged.pdf", TestDocuments.Damaged);
        var away = await AddResourceAsync(noteId, "away.pdf", TestDocuments.Pdf("Far away."));
        foreach (var file in Directory.GetFiles(rootPath, "*away.pdf", SearchOption.AllDirectories))
            File.Delete(file);
        var model = new FakeModel("A summary.");

        async Task<string> ErrorOf(Guid id) => (await AiDocumentSummarizer.SummarizeAsync(service, model, Ollama, id, true)).ErrorMessage;

        StringAssert.Contains(await ErrorOf(locked), "locked");
        StringAssert.Contains(await ErrorOf(scanned), AiDocumentRules.NeedsTranscriptionMessage);
        StringAssert.Contains(await ErrorOf(damaged), AiDocumentRules.UnreadableMessage);
        StringAssert.Contains(await ErrorOf(away), AiDocumentRules.FileNotAvailableMessage);
        Assert.AreEqual(0, model.Calls, "the model is not called");
    }

    [TestMethod]
    public async Task ADocumentChangedWhileTheModelWrites_IsNotWritten()
    {
        Open();
        var pdfId = await AddResourceAsync(await AddNoteAsync("Docs", ""), "manual.pdf", TestDocuments.Pdf("A manual."));
        var model = new FakeModel("A summary.", async () =>
        {
            var resource = (await service.Notes.GetResourceAsync(pdfId)).Entity;
            resource.Description = "Written by a person meanwhile.";
            resource.DescriptionByUser = true;
            Assert.IsTrue((await service.Notes.SaveResourceAsync(resource)).IsValid);
        });

        var res = await AiDocumentSummarizer.SummarizeAsync(service, model, Ollama, pdfId, replacePersonDescription: true);

        Assert.IsFalse(res.IsValid);
        Assert.AreEqual("Written by a person meanwhile.", (await service.Notes.GetResourceAsync(pdfId)).Entity.Description);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ARun_ReadsTheDocuments_SummarizesThePending_AndKeepsItsPace(string orm)
    {
        Open(orm);
        var noteId = await AddNoteAsync("Docs", "");
        var manual = await AddResourceAsync(noteId, "manual.pdf", TestDocuments.Pdf("A manual."));
        var locked = await AddResourceAsync(noteId, "locked.pdf", TestDocuments.Pdf("Locked text."), "Mine.", locked: true);
        await AddResourceAsync(noteId, "scan.pdf", TestDocuments.MixedPdf("", "SCANNED PAGE"));
        await AddResourceAsync(noteId, "photo.png", Png(), fileType: "image/png");
        var model = new FakeModel("A summary.");

        var before = (await service.ResourceContent.GetProcessingStatusAsync()).Entity;
        var run = await AiResourceProcessor.ProcessAsync(service, model, Ollama, new ResourceProcessingOptionsDto { Photos = false });

        Assert.AreEqual((1, 3, 0), (before.PendingPhotos, before.DocumentsToRead, before.PersonDocuments));
        Assert.AreEqual(2, before.PendingDocuments, "the manual and the scan");
        Assert.AreEqual(EnumResourceProcessingPhase.Completed, run.Entity.Phase, run.Entity.Message);
        Assert.AreEqual((0, 3, 3), (run.Entity.PhotosTotal, run.Entity.DocumentsToReadTotal, run.Entity.DocumentsRead));
        Assert.AreEqual((2, 1, 1), (run.Entity.DocumentsTotal, run.Entity.DocumentsSummarized, run.Entity.DocumentsWaiting));
        Assert.AreEqual(0, run.Entity.ErrorsCount, string.Join(" ", run.Entity.Errors));
        Assert.AreEqual(1, model.Calls, "only the manual; the photo was not chosen");
        Assert.AreEqual("A summary.", (await service.Notes.GetResourceAsync(manual)).Entity.Description);
        Assert.AreEqual(EnumExtractedState.Complete, (await service.ResourceContent.GetDocumentTextAsync(locked)).Entity.State, "a locked one is read too");

        var after = (await service.ResourceContent.GetProcessingStatusAsync()).Entity;
        Assert.AreEqual((0, 1, 1), (after.DocumentsToRead, after.PendingDocuments, after.PendingPhotos), "the scan waits; the photo stays");
        Assert.IsNotNull(after.SecondsPerDocument);
        Assert.IsNull(after.SecondsPerPhoto);
    }

    #region Helpers

    private sealed class FakeModel(string answer, Func<Task>? during = null) : IChatClient
    {
        public List<ChatMessage> Messages { get; private set; } = [];
        public ChatOptions? Options { get; private set; }
        public int Calls { get; private set; }

        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("The summarizer streams.");

        public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            Calls++;
            Messages = messages.ToList();
            Options = options;
            if (during != null)
                await during();
            yield return new ChatResponseUpdate(ChatRole.Assistant, answer);
        }

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose() { }
    }

    private async Task<Guid> AddResourceAsync(Guid noteId, string name, byte[] content, string? description = null, bool locked = false, string fileType = Pdf)
    {
        var saved = await service.Notes.SaveResourceAsync(new ResourceDto
        {
            NoteId = noteId,
            Name = $"{Guid.NewGuid()}_{name}",
            Description = description ?? name,
            DescriptionByUser = locked,
            FileType = fileType,
            ContentArrayBytes = content
        });
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        return saved.Entity.ResourceId;
    }

    private static byte[] Png()
    {
        using var bitmap = new SkiaSharp.SKBitmap(64, 48);
        bitmap.Erase(SkiaSharp.SKColors.Blue);
        using var image = SkiaSharp.SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private async Task<Guid> AddNoteAsync(string topic, string description)
    {
        var folder = await service.Repository.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "Documents folder" });
        Assert.IsTrue(folder.IsValid, folder.ErrorMessage);

        var noteId = Guid.NewGuid();
        var date = new DateTime(2026, 1, 2, 3, 4, 5);
        var note = await service.Repository.Notes.AddAsync(new NoteDto
        {
            NoteId = noteId, Topic = topic, Description = description, FolderId = folder.Entity.FolderId,
            CreationDateTime = date, ModificationDateTime = date
        });
        Assert.IsTrue(note.IsValid, note.ErrorMessage);
        return noteId;
    }

    #endregion
}

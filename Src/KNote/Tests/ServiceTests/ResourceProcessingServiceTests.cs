using System.Runtime.CompilerServices;
using KNote.Ai;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;
using Microsoft.Extensions.AI;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// «Process pending resources» (KNote Atlas, stage 4, 4.3) with a repository and a fake model: what it counts (not KNote's own
/// notes), what it describes and locks as chosen, the pace it keeps for the estimates, the photos whose file is not here
/// (counted, not errors), the ones that fail (errors, tried again next time), canceling (what is done is kept) and the date
/// of the notes, which never changes. Dapper and EF.
/// </summary>
[TestClass]
public class ResourceProcessingServiceTests
{
    private static readonly AiProviderRef Ollama = new() { Alias = "Local", Provider = EnumAiProvider.Ollama, Model = "qwen3.5:9b", Host = "http://localhost:11434" };
    private static readonly DateTime NoteDate = new(2026, 1, 2, 3, 4, 5);

    private RepositoryTestDatabase db = null!;
    private KntService service = null!;
    private string rootPath = null!;

    private void Open(string orm)
    {
        db = new RepositoryTestDatabase();
        service = new KntService(db.CreateRepository(orm), activateMessageBroker: false);

        rootPath = Path.Combine(Path.GetTempPath(), $"knote-batch-{Guid.NewGuid():N}");
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
    public async Task TheStatus_CountsThePhotosOfTheRepository_NotOfKNotesOwnNotes(string orm)
    {
        Open(orm);
        var noteId = await AddNoteAsync("Trip");
        await AddResourceAsync(noteId, "beach.png");
        await AddResourceAsync(noteId, "grandma.png", "La abuela Carmen en Cádiz.");
        await AddResourceAsync(noteId, "locked.png", "Mine.", locked: true);
        await AddResourceAsync(noteId, "manual.pdf", fileType: "application/pdf");
        await AddResourceAsync(await AddSystemNoteAsync(), "session.png");

        var status = await service.ResourceContent.GetProcessingStatusAsync();

        Assert.IsTrue(status.IsValid, status.ErrorMessage);
        Assert.AreEqual(1, status.Entity.PendingPhotos);
        Assert.AreEqual(1, status.Entity.PersonPhotos);
        Assert.IsNull(status.Entity.SecondsPerPhoto, "no run yet");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ARun_DescribesThePending_LocksTheChosen_AndKeepsItsPace(string orm)
    {
        Open(orm);
        var noteId = await AddNoteAsync("Trip");
        var beach = await AddResourceAsync(noteId, "beach.png");
        var sea = await AddResourceAsync(noteId, "sea.png");
        var grandma = await AddResourceAsync(noteId, "grandma.png", "La abuela Carmen en Cádiz.");
        var model = new FakeModel("A blue photo.");
        // Saving a resource with Dapper changes the date of its note (not with EF): the run must not change it again.
        var noteDate = (await service.Repository.Notes.GetAsync(noteId)).Entity.ModificationDateTime;

        var run = await AiResourceProcessor.ProcessAsync(service, model, Ollama,
            new ResourceProcessingOptionsDto { PersonDescriptions = EnumPersonDescriptions.Lock });

        Assert.IsTrue(run.IsValid, run.ErrorMessage);
        Assert.AreEqual(EnumResourceProcessingPhase.Completed, run.Entity.Phase);
        Assert.AreEqual(2, run.Entity.PhotosTotal);
        Assert.AreEqual(2, run.Entity.PhotosDescribed);
        Assert.AreEqual(1, run.Entity.PhotosLocked);
        Assert.AreEqual(2, model.Calls);
        foreach (var photo in new[] { beach, sea })
        {
            var described = (await service.Notes.GetResourceAsync(photo)).Entity;
            Assert.AreEqual("A blue photo.", described.Description);
            Assert.AreEqual("qwen3.5:9b", described.DescriptionModel);
        }
        var kept = (await service.Notes.GetResourceAsync(grandma)).Entity;
        Assert.AreEqual("La abuela Carmen en Cádiz.", kept.Description);
        Assert.IsTrue(kept.DescriptionByUser);
        Assert.IsNull(kept.DescriptionModel);

        var status = (await service.ResourceContent.GetProcessingStatusAsync()).Entity;
        Assert.AreEqual(0, status.PendingPhotos);
        Assert.AreEqual(0, status.PersonPhotos);
        Assert.IsNotNull(status.LastRunDateTime);
        Assert.IsNotNull(status.SecondsPerPhoto);
        Assert.AreEqual(noteDate, (await service.Repository.Notes.GetAsync(noteId)).Entity.ModificationDateTime, "the note's date doesn't change");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task Replace_DescribesThePhotosAPersonDescribed_FromWhatTheyWrote(string orm)
    {
        Open(orm);
        var grandma = await AddResourceAsync(await AddNoteAsync("Birthday"), "grandma.png", "La abuela Carmen, a la derecha.");
        var model = new FakeModel("La abuela Carmen sopla las velas.");

        var run = await AiResourceProcessor.ProcessAsync(service, model, Ollama,
            new ResourceProcessingOptionsDto { PersonDescriptions = EnumPersonDescriptions.Replace });

        Assert.AreEqual(1, run.Entity.PhotosDescribed);
        StringAssert.Contains(model.Messages[1].Text, "La abuela Carmen, a la derecha.");
        Assert.AreEqual("La abuela Carmen sopla las velas.", (await service.Notes.GetResourceAsync(grandma)).Entity.Description);
    }

    [TestMethod]
    public async Task APhotoWhoseFileIsNotHere_IsCounted_AndOneThatFails_IsAnError()
    {
        Open("EntityFramework");
        var noteId = await AddNoteAsync("Trip");
        var away = await AddResourceAsync(noteId, "away.png");
        await AddResourceAsync(noteId, "empty.png");
        foreach (var file in Directory.GetFiles(rootPath, "*away.png", SearchOption.AllDirectories))
            File.Delete(file);
        // The model answers nothing: «gave no description».
        var model = new FakeModel("");

        var run = await AiResourceProcessor.ProcessAsync(service, model, Ollama, new ResourceProcessingOptionsDto());

        Assert.AreEqual(EnumResourceProcessingPhase.Completed, run.Entity.Phase);
        Assert.AreEqual(1, run.Entity.NotAvailable);
        Assert.AreEqual(1, run.Entity.ErrorsCount);
        StringAssert.StartsWith(run.Entity.Errors.Single(), "empty.png: ");
        Assert.AreEqual(0, run.Entity.PhotosDescribed);
        Assert.AreEqual(2, (await service.ResourceContent.GetProcessingStatusAsync()).Entity.PendingPhotos, "both stay pending");
        Assert.IsNull((await service.Notes.GetResourceAsync(away)).Entity.DescriptionModel);
    }

    [TestMethod]
    public async Task Canceling_KeepsWhatIsDone()
    {
        Open("EntityFramework");
        var noteId = await AddNoteAsync("Trip");
        await AddResourceAsync(noteId, "one.png");
        await AddResourceAsync(noteId, "two.png");
        await AddResourceAsync(noteId, "three.png");
        using var cancellation = new CancellationTokenSource();
        var model = new FakeModel("A photo.", () =>
        {
            cancellation.Cancel();
            return Task.CompletedTask;
        });

        var run = await AiResourceProcessor.ProcessAsync(service, model, Ollama, new ResourceProcessingOptionsDto(), cancellationToken: cancellation.Token);

        Assert.AreEqual(EnumResourceProcessingPhase.Canceled, run.Entity.Phase);
        Assert.AreEqual(1, model.Calls, "it stops at once");
        Assert.IsTrue((await service.ResourceContent.GetProcessingStatusAsync()).Entity.PendingPhotos >= 2, "the rest stays pending");
    }

    [TestMethod]
    public async Task OnlyAnAdmin_RunsIt()
    {
        Open("EntityFramework");
        service.EnforceAuthorization = true;
        service.UserIdentityName = "nobody";

        var prepared = await service.ResourceContent.PrepareProcessingAsync(new ResourceProcessingOptionsDto());

        Assert.IsFalse(prepared.IsValid);
        Assert.IsTrue(prepared.NotAuthorized);
    }

    #region Helpers

    // A model that answers a fixed text; during, what happens while it writes.
    private sealed class FakeModel(string answer, Func<Task>? during = null) : IChatClient
    {
        public List<ChatMessage> Messages { get; private set; } = [];
        public int Calls { get; private set; }

        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("The describer streams.");

        public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            Calls++;
            Messages = messages.ToList();
            if (during != null)
                await during();
            yield return new ChatResponseUpdate(ChatRole.Assistant, answer);
        }

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose() { }
    }

    private async Task<Guid> AddResourceAsync(Guid noteId, string name, string? description = null, bool locked = false, string fileType = "image/png")
    {
        var saved = await service.Notes.SaveResourceAsync(new ResourceDto
        {
            NoteId = noteId,
            Name = $"{Guid.NewGuid()}_{name}",
            Description = description ?? name,
            DescriptionByUser = locked,
            FileType = fileType,
            ContentArrayBytes = Png()
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

    private async Task<Guid> AddSystemNoteAsync()
    {
        var noteTypeId = Guid.NewGuid();
        Assert.IsTrue((await service.Repository.NoteTypes.AddAsync(new NoteTypeDto { NoteTypeId = noteTypeId, Name = KntConst.ChatSessionsTag })).IsValid);
        return await AddNoteAsync("A session", noteTypeId);
    }

    private async Task<Guid> AddNoteAsync(string topic, Guid? noteTypeId = null)
    {
        var folder = await service.Repository.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "Photos folder" });
        Assert.IsTrue(folder.IsValid, folder.ErrorMessage);

        var noteId = Guid.NewGuid();
        var note = await service.Repository.Notes.AddAsync(new NoteDto
        {
            NoteId = noteId,
            Topic = topic,
            Description = "",
            FolderId = folder.Entity.FolderId,
            NoteTypeId = noteTypeId,
            CreationDateTime = NoteDate,
            ModificationDateTime = NoteDate
        });
        Assert.IsTrue(note.IsValid, note.ErrorMessage);
        return noteId;
    }

    #endregion
}

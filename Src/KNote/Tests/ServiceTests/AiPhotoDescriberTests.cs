using System.Runtime.CompilerServices;
using KNote.Ai;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;
using Microsoft.Extensions.AI;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// AiPhotoDescriber with a repository and a fake model (KNote Atlas, stage 4, 4.2): what the model receives (the rules of the
/// task, the context of the note and the photo, no tools), what is saved and when nothing is: a locked photo, a description a
/// person wrote, one of KNote's own notes, or a resource that changed while the model was writing (D12).
/// </summary>
[TestClass]
public class AiPhotoDescriberTests
{
    private static readonly AiProviderRef Ollama = new() { Alias = "Local", Provider = EnumAiProvider.Ollama, Model = "qwen3.5:9b", Host = "http://localhost:11434" };
    private static readonly AiProviderRef OpenAi = new() { Alias = "Paid", Provider = EnumAiProvider.OpenAI, Model = "gpt-test", ApiKey = "test" };

    private RepositoryTestDatabase db = null!;
    private KntService service = null!;
    private string rootPath = null!;

    private void Open(string orm = "EntityFramework")
    {
        db = new RepositoryTestDatabase();
        service = new KntService(db.CreateRepository(orm), activateMessageBroker: false);

        rootPath = Path.Combine(Path.GetTempPath(), $"knote-photos-{Guid.NewGuid():N}");
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
    public async Task APhoto_IsDescribed_WithTheContextOfItsNote_AndSavedWithItsModel(string orm)
    {
        Open(orm);
        var noteId = await AddNoteAsync("Viaje a Cádiz", "Restauramos el ordenador de la abuela Carmen.");
        var photoId = await AddResourceAsync(noteId, "photo.png");
        await AddResourceAsync(noteId, "other.png", "Carmen y Lucía en la playa.", locked: true);
        var dateBefore = (await service.Notes.GetAsync(noteId)).Entity.ModificationDateTime;
        var model = new FakeModel("**Descripción:** Un ordenador Sinclair QL sobre una mesa blanca.");

        var res = await AiPhotoDescriber.DescribeAsync(service, model, Ollama, photoId, replacePersonDescription: false);

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        var stored = (await service.Notes.GetResourceAsync(photoId)).Entity;
        Assert.AreEqual("Un ordenador Sinclair QL sobre una mesa blanca.", stored.Description);
        Assert.AreEqual("qwen3.5:9b", stored.DescriptionModel);
        Assert.IsFalse(stored.DescriptionByUser);
        Assert.AreEqual(dateBefore, (await service.Notes.GetAsync(noteId)).Entity.ModificationDateTime);

        // What the model received: the rules of the task, the note and the other photo's description, the photo; no tools.
        StringAssert.Contains(model.Messages[0].Text, "You describe a photo attached to a note");
        var request = model.Messages[1];
        StringAssert.Contains(request.Text, "Topic: Viaje a Cádiz");
        StringAssert.Contains(request.Text, "Restauramos el ordenador de la abuela Carmen.");
        StringAssert.Contains(request.Text, "- Carmen y Lucía en la playa.");
        Assert.AreEqual("image/png", request.Contents.OfType<DataContent>().Single().MediaType);
        Assert.IsNull(model.Options!.Tools);
        Assert.AreEqual(ReasoningEffort.None, model.Options.Reasoning?.Effort);
        Assert.IsNotNull(model.Options.MaxOutputTokens);
        Assert.IsNotNull(model.Options.Temperature);
    }

    // OpenAI's reasoning models answer 400 to a temperature (found with a real one in 4.2).
    [TestMethod]
    public async Task AnotherProvider_KeepsItsReasoningItsOutputAndItsTemperature()
    {
        Open();
        var photoId = await AddResourceAsync(await AddNoteAsync("A note", ""), "photo.png");
        var model = new FakeModel("A QL.");

        await AiPhotoDescriber.DescribeAsync(service, model, OpenAi, photoId, replacePersonDescription: false);

        Assert.IsNull(model.Options!.Reasoning);
        Assert.IsNull(model.Options.MaxOutputTokens);
        Assert.IsNull(model.Options.Temperature);
    }

    [TestMethod]
    public async Task ADescriptionWrittenByAPerson_IsOnlyReplacedWhenAsked()
    {
        Open();
        var photoId = await AddResourceAsync(await AddNoteAsync("A note", ""), "photo.png", "Grandma Carmen in Cádiz.");

        var notAsked = await AiPhotoDescriber.DescribeAsync(service, new FakeModel("A beach."), Ollama, photoId, replacePersonDescription: false);
        Assert.IsFalse(notAsked.IsValid);
        Assert.AreEqual("Grandma Carmen in Cádiz.", (await service.Notes.GetResourceAsync(photoId)).Entity.Description);

        var model = new FakeModel("Grandma Carmen on a beach.");
        var asked = await AiPhotoDescriber.DescribeAsync(service, model, Ollama, photoId, replacePersonDescription: true);
        Assert.IsTrue(asked.IsValid, asked.ErrorMessage);
        Assert.AreEqual("Grandma Carmen on a beach.", (await service.Notes.GetResourceAsync(photoId)).Entity.Description);
        // What the person wrote is not lost: the model receives it, to say who is in the photo.
        StringAssert.Contains(model.Messages[1].Text, "What a person wrote of this photo (it is true: keep its names and facts):\nGrandma Carmen in Cádiz.");
    }

    [TestMethod]
    public async Task ALockedPhoto_AndAFileThatIsNotAPhoto_AreNotDescribed()
    {
        Open();
        var noteId = await AddNoteAsync("A note", "");
        var locked = await AddResourceAsync(noteId, "photo.png", "photo.png", locked: true);
        var pdf = await AddResourceAsync(noteId, "manual.pdf", fileType: "application/pdf");
        var model = new FakeModel("A QL.");

        Assert.IsFalse((await AiPhotoDescriber.DescribeAsync(service, model, Ollama, locked, true)).IsValid);
        Assert.IsFalse((await AiPhotoDescriber.DescribeAsync(service, model, Ollama, pdf, true)).IsValid);
        Assert.AreEqual(0, model.Calls, "The model is not called.");
    }

    [TestMethod]
    public async Task APhotoOfKNotesOwnNotes_IsNotDescribed()
    {
        Open();
        var noteTypeId = Guid.NewGuid();
        Assert.IsTrue((await service.Repository.NoteTypes.AddAsync(new NoteTypeDto { NoteTypeId = noteTypeId, Name = KntConst.ChatSessionsTag })).IsValid);
        var photoId = await AddResourceAsync(await AddNoteAsync("A session", "", noteTypeId), "photo.png");
        var model = new FakeModel("A QL.");

        var res = await AiPhotoDescriber.DescribeAsync(service, model, Ollama, photoId, true);

        Assert.IsFalse(res.IsValid);
        Assert.AreEqual(0, model.Calls);
    }

    [TestMethod]
    public async Task APhotoThatChangesWhileTheModelWrites_KeepsWhatAPersonWrote()
    {
        Open();
        var photoId = await AddResourceAsync(await AddNoteAsync("A note", ""), "photo.png");
        // A person edits the description while the model is writing.
        var model = new FakeModel("A QL.", async () =>
        {
            var editor = (await service.Notes.GetResourceAsync(photoId)).Entity;
            editor.Description = "My QL.";
            editor.DescriptionByUser = true;
            Assert.IsTrue((await service.Notes.SaveResourceAsync(editor)).IsValid);
        });

        var res = await AiPhotoDescriber.DescribeAsync(service, model, Ollama, photoId, true);

        Assert.IsFalse(res.IsValid);
        var stored = (await service.Notes.GetResourceAsync(photoId)).Entity;
        Assert.AreEqual("My QL.", stored.Description);
        Assert.IsTrue(stored.DescriptionByUser);
        Assert.IsNull(stored.DescriptionModel);
    }

    [TestMethod]
    public async Task AnEmptyAnswer_SavesNothing()
    {
        Open();
        var photoId = await AddResourceAsync(await AddNoteAsync("A note", ""), "photo.png");

        var res = await AiPhotoDescriber.DescribeAsync(service, new FakeModel("  "), Ollama, photoId, true);

        Assert.IsFalse(res.IsValid);
        Assert.IsNull((await service.Notes.GetResourceAsync(photoId)).Entity.DescriptionModel);
    }

    #region Helpers

    // A model that answers a fixed text and keeps what it received; during, what happens while it writes.
    private sealed class FakeModel(string answer, Func<Task>? during = null) : IChatClient
    {
        public List<ChatMessage> Messages { get; private set; } = [];
        public ChatOptions? Options { get; private set; }
        public int Calls { get; private set; }

        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("The describer streams.");

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

    private async Task<Guid> AddNoteAsync(string topic, string description, Guid? noteTypeId = null)
    {
        var folder = await service.Repository.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "Photos folder" });
        Assert.IsTrue(folder.IsValid, folder.ErrorMessage);

        var noteId = Guid.NewGuid();
        var date = new DateTime(2026, 1, 2, 3, 4, 5);
        var note = await service.Repository.Notes.AddAsync(new NoteDto
        {
            NoteId = noteId,
            Topic = topic,
            Description = description,
            FolderId = folder.Entity.FolderId,
            NoteTypeId = noteTypeId,
            CreationDateTime = date,
            ModificationDateTime = date
        });
        Assert.IsTrue(note.IsValid, note.ErrorMessage);
        return noteId;
    }

    #endregion
}

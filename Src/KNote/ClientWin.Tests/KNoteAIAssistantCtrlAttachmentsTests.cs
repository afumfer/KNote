using System.Runtime.CompilerServices;
using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.ClientWin.Tests.Helpers;
using KNote.Model;
using KNote.Model.Dto;
using Microsoft.Extensions.AI;

namespace KNote.ClientWin.Tests;

/// <summary>
/// The files of a message in the desktop assistant (KNote Atlas, D12): kept until the message is answered, saved in the note of
/// the session before it is sent and, the images, sent to the model only in that turn.
/// </summary>
[TestClass]
public class KNoteAIAssistantCtrlAttachmentsTests
{
    private static readonly AiProviderRef OpenAi = new() { Alias = "OpenAI test", Provider = EnumAiProvider.OpenAI, Model = "gpt-test", ApiKey = "test" };
    private static readonly AiProviderRef Local = new() { Alias = "Local test", Provider = EnumAiProvider.Ollama, Host = "http://localhost:1", Model = "qwen-test" };

    // A 1x1 PNG.
    private static readonly byte[] Png = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==");

    private sealed class Setup
    {
        public KNoteAIAssistantCtrl Ctrl { get; init; } = null!;
        public FakeKntAiSessionService Sessions { get; init; } = null!;
        public FakeAIAssistantView View { get; init; } = null!;
        public List<AiChatAttachmentUploadDto> Uploads { get; } = new();
        public List<List<ChatMessage>> Calls { get; } = new();
        public Exception? ProviderFailure { get; set; }
    }

    private static async IAsyncEnumerable<ChatResponseUpdate> Answer([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        yield return new ChatResponseUpdate(ChatRole.Assistant, "An answer");
    }

    private static Setup CreateCtrl(AiProviderRef? provider = null)
    {
        var store = new Store(new TestFactoryViews());
        store.Settings.Ai.Providers.Add(provider ?? OpenAi);
        var view = new FakeAIAssistantView();
        store.FactoryViews.Registry.Register<KNoteAIAssistantCtrl, IViewBase>(_ => view);
        var service = new FakeKntService();
        store.AddServiceRef(TestServiceRefFactory.CreateWithFakeService(service));

        var ctrl = new KNoteAIAssistantCtrl(store) { PersistSession = true };
        var setup = new Setup { Ctrl = ctrl, Sessions = service.AiSessionsFake, View = view };

        var noteId = Guid.NewGuid();
        setup.Sessions.AddAttachmentAsyncImpl = upload =>
        {
            setup.Uploads.Add(upload);
            return Task.FromResult(new Result<AiChatAttachmentDto>
            {
                Entity = new AiChatAttachmentDto { ResourceId = Guid.NewGuid(), FileName = upload.FileName, ContentType = upload.ContentType, SessionNoteId = noteId }
            });
        };
        setup.Sessions.GetAttachmentAsyncImpl = (session, resource) => Task.FromResult(new Result<AiChatAttachmentContentDto>
        {
            Entity = new AiChatAttachmentContentDto { FileName = "a.png", ContentType = "image/png", ContentBase64 = Convert.ToBase64String(Png) }
        });
        setup.Sessions.SaveAsyncImpl = session => Task.FromResult(new Result<AiChatSessionDto> { Entity = new AiChatSessionDto { NoteId = noteId } });

        ctrl.SetChatClientForTesting(new FakeChatClient
        {
            GetStreamingResponseImpl = (messages, options, ct) =>
            {
                setup.Calls.Add(messages.ToList());
                return setup.ProviderFailure != null ? throw setup.ProviderFailure : Answer(ct);
            }
        }, provider ?? OpenAi);
        ctrl.RestartAIAssistant();
        return setup;
    }

    [TestMethod]
    public void AddAttachment_TakesItsTypeFromTheName_AndRefusesWhatCannotGo()
    {
        var ctrl = CreateCtrl().Ctrl;

        Assert.IsNull(ctrl.AddAttachment("a.png", Png));
        StringAssert.Contains(ctrl.AddAttachment("empty.png", Array.Empty<byte>()), "empty");
        StringAssert.Contains(ctrl.AddAttachment("big.png", new byte[KntConst.MaxLenResourceFile + 1]), "too big");

        Assert.AreEqual("image/png", ctrl.PendingAttachments.Single().ContentType);
    }

    [TestMethod]
    public void AddAttachment_StopsAtTheFilesOfAMessage()
    {
        var ctrl = CreateCtrl().Ctrl;
        for (var i = 0; i < AiAttachmentRules.MaxPerMessage; i++)
            Assert.IsNull(ctrl.AddAttachment($"{i}.png", Png));

        StringAssert.Contains(ctrl.AddAttachment("one more.png", Png), "at most");
        Assert.AreEqual(AiAttachmentRules.MaxPerMessage, ctrl.PendingAttachments.Count);
    }

    [TestMethod]
    public void ImagesSentTo_NamesAPaidProvider_OnlyWithImages()
    {
        var paid = CreateCtrl().Ctrl;
        paid.AddAttachment("manual.pdf", Png);
        Assert.IsNull(paid.ImagesSentTo);
        paid.AddAttachment("a.png", Png);
        Assert.AreEqual(EnumAiProvider.OpenAI, paid.ImagesSentTo);

        var local = CreateCtrl(Local).Ctrl;
        local.AddAttachment("a.png", Png);
        Assert.IsNull(local.ImagesSentTo);
    }

    [TestMethod]
    public void CanAttach_OnlyInAConversationThatIsSaved()
    {
        var ctrl = CreateCtrl().Ctrl;

        Assert.IsTrue(ctrl.CanAttach);
        ctrl.PersistSession = false;
        Assert.IsFalse(ctrl.CanAttach);
    }

    [TestMethod]
    public async Task AMessageWithAnImage_ReachesTheModel_AndTheNextOneDoesNot()
    {
        var setup = CreateCtrl();
        setup.Ctrl.AddAttachment("a.png", Png);
        setup.Ctrl.AddAttachment("manual.pdf", new byte[] { 1 });

        Assert.IsTrue(await setup.Ctrl.SaveAttachmentsAsync("What is it?"));
        await setup.Ctrl.StreamCompletionAsync("What is it?");
        await setup.Ctrl.StreamCompletionAsync("And now?");

        Assert.AreEqual(1, setup.Calls[0].SelectMany(m => m.Contents).OfType<DataContent>().Count());
        Assert.IsFalse(setup.Calls[1].SelectMany(m => m.Contents).OfType<DataContent>().Any());
        Assert.AreEqual(2, setup.Ctrl.ChatTurns[0].Attachments.Count);
        Assert.AreEqual("What is it?", setup.Ctrl.ChatTurns[0].Prompt);
        Assert.AreEqual(0, setup.Ctrl.ChatTurns[1].Attachments.Count);
        Assert.AreEqual(0, setup.Ctrl.PendingAttachments.Count);
    }

    [TestMethod]
    public async Task TheFirstFile_CreatesTheNote_AndTheSecondGoesInIt()
    {
        var setup = CreateCtrl();
        setup.Ctrl.AddAttachment("a.png", Png);
        setup.Ctrl.AddAttachment("b.png", Png);

        await setup.Ctrl.SaveAttachmentsAsync("Look");

        Assert.AreEqual(Guid.Empty, setup.Uploads[0].SessionNoteId);
        Assert.AreEqual(setup.Ctrl.SessionNoteId, setup.Uploads[1].SessionNoteId);
        Assert.AreNotEqual(Guid.Empty, setup.Ctrl.SessionNoteId);
        Assert.AreEqual("Look", setup.Uploads[0].Topic);
        Assert.AreEqual(OpenAi.Model, setup.Uploads[0].Model);
    }

    [TestMethod]
    public async Task AFileThatCannotBeSaved_StopsTheMessage_AndIsKept()
    {
        var setup = CreateCtrl();
        var failed = new Result<AiChatAttachmentDto>();
        failed.AddErrorMessage("No disk");
        setup.Sessions.AddAttachmentAsyncImpl = _ => Task.FromResult(failed);
        setup.Ctrl.AddAttachment("a.png", Png);

        var saved = await setup.Ctrl.SaveAttachmentsAsync("Look");

        Assert.IsFalse(saved);
        Assert.AreEqual(1, setup.Ctrl.PendingAttachments.Count);
        Assert.AreEqual(0, setup.Ctrl.TurnAttachments.Count);
        StringAssert.Contains(setup.View.LastShownInfo, "No disk");
    }

    [TestMethod]
    public async Task IfTheModelFails_TheFilesStay_AndAreNotSavedTwice()
    {
        var setup = CreateCtrl();
        setup.ProviderFailure = new InvalidOperationException("Provider down");
        setup.Ctrl.AddAttachment("a.png", Png);
        await setup.Ctrl.SaveAttachmentsAsync("Look");

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => setup.Ctrl.StreamCompletionAsync("Look"));
        Assert.AreEqual(1, setup.Ctrl.PendingAttachments.Count);

        setup.ProviderFailure = null;
        await setup.Ctrl.SaveAttachmentsAsync("Look");
        await setup.Ctrl.StreamCompletionAsync("Look");

        Assert.AreEqual(1, setup.Uploads.Count);
        Assert.AreEqual(1, setup.Ctrl.ChatTurns.Single().Attachments.Count);
        Assert.AreEqual(0, setup.Ctrl.PendingAttachments.Count);
    }

    [TestMethod]
    public async Task AnImage_HasItsPreview_AndAnotherFileDoesNot()
    {
        var setup = CreateCtrl();
        setup.Ctrl.AddAttachment("a.png", Png);
        setup.Ctrl.AddAttachment("manual.pdf", new byte[] { 1 });

        await setup.Ctrl.SaveAttachmentsAsync("Look");

        var image = setup.Ctrl.TurnAttachments.Single(a => a.IsImage);
        var pdf = setup.Ctrl.TurnAttachments.Single(a => !a.IsImage);
        StringAssert.StartsWith(setup.Ctrl.PreviewOf(image.ResourceId), "data:image/");
        Assert.IsNull(setup.Ctrl.PreviewOf(pdf.ResourceId));
    }

    [TestMethod]
    public void ANewConversation_ForgetsTheFilesOfTheOne_BeingWritten()
    {
        var setup = CreateCtrl();
        setup.Ctrl.AddAttachment("a.png", Png);

        setup.Ctrl.RestartAIAssistant();

        Assert.AreEqual(0, setup.Ctrl.PendingAttachments.Count);
    }
}

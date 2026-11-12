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
/// NoteEditorCtrl.TranscribePagesAsync (KNote Atlas, stage 4, 4.5b): «Transcribe scanned pages» of one document, page after page
/// with its progress (a page transcribed meanwhile is skipped without an error), and, at the end, the summary of a document that
/// waited for its pages (decided by the owner), updated in the editor without making it dirty; canceling stops and summarizes
/// nothing; a document with a description of its own is not summarized. The window (DocumentTextForm) is not covered here.
/// </summary>
[TestClass]
public class NoteEditorCtrlTranscriptionTests
{
    private static readonly AiProviderRef Ollama = new() { Alias = "Local", Provider = EnumAiProvider.Ollama, Model = "qwen3.5:9b" };

    private sealed class Calls
    {
        public List<int> Saved { get; } = [];
        public int Summaries { get; set; }
        public int ModelCalls { get; set; }
    }

    private static async Task<(NoteEditorCtrl Ctrl, Calls Calls)> CreateCtrlAsync(string? pendingError = null)
    {
        var factoryViews = new TestFactoryViews();
        factoryViews.Registry.Register<NoteEditorCtrl, IViewNoteEditorEmbeddable<NoteExtendedDto>>(c => new FakeNoteEditorView());
        var service = new FakeKntService { RepositoryRef = new RepositoryRef { Name = "Shared" } };
        var store = new Store(factoryViews);
        store.GrantAdmin(service);
        var calls = new Calls();

        var content = service.ResourceContentFake;
        content.GetPageContextAsyncImpl = (id, page) =>
        {
            var result = new Result<PageTranscriptionContextDto>();
            if (page == 3 && pendingError != null)
                result.AddErrorMessage(pendingError);
            else
                result.Entity = new PageTranscriptionContextDto
                {
                    ResourceId = id, Page = page, PagesCount = 3, FileName = "scan.pdf", Image = [1, 2, 3], ExpectedName = "x_scan.pdf"
                };
            return Task.FromResult(result);
        };
        content.SaveTranscriptionAsyncImpl = transcription =>
        {
            calls.Saved.Add(transcription.Page);
            return Task.FromResult(new Result<DocumentReadDto>(new DocumentReadDto { ResourceId = transcription.ResourceId, Pages = 3 }));
        };
        content.GetDocumentContextAsyncImpl = id => Task.FromResult(new Result<DocumentSummaryContextDto>(new DocumentSummaryContextDto
        {
            Resource = new ResourceInfoDto { ResourceId = id, Name = $"{Guid.NewGuid()}_scan.pdf", Description = "scan.pdf", FileType = "application/pdf" },
            Pages = 3, DocumentText = "[Page 2]\nThe transcribed text."
        }));
        content.SaveAiDescriptionAsyncImpl = description =>
        {
            calls.Summaries++;
            return Task.FromResult(new Result<ResourceInfoDto>(new ResourceInfoDto
            {
                ResourceId = description.ResourceId, Description = description.Description, DescriptionModel = description.Model
            }));
        };

        service.NotesFake.GetExtendedAsyncImpl = id => Task.FromResult(new Result<NoteExtendedDto>(new NoteExtendedDto
        {
            NoteId = id, Topic = "Manuals", FolderId = Guid.NewGuid(), Description = "", Tags = ""
        }));
        var ctrl = new NoteEditorCtrl(store) { ChatClientFactory = _ => new FakeChatClient { GetStreamingResponseImpl = (_, _, _) => Answer(calls) } };
        await ctrl.LoadModelById(service, Guid.NewGuid(), refreshView: false);
        return (ctrl, calls);
    }

    private static async IAsyncEnumerable<ChatResponseUpdate> Answer(Calls calls, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        calls.ModelCalls++;
        await Task.Yield();
        yield return new ChatResponseUpdate(ChatRole.Assistant, "A text.");
    }

    private static ResourceDto Document(string description = "scan.pdf")
    {
        var resource = new ResourceDto { ResourceId = Guid.NewGuid(), Name = $"{Guid.NewGuid()}_scan.pdf", FileType = "application/pdf", Description = description };
        resource.SetIsNew(false);
        resource.SetIsDirty(false);
        return resource;
    }

    [TestMethod]
    public async Task ThePages_AreTranscribed_AndADocumentThatWaited_IsSummarized()
    {
        var (ctrl, calls) = await CreateCtrlAsync(pendingError: AiTranscriptionRules.NotPendingMessage);
        var document = Document();
        var steps = new List<string>();

        var (transcribed, errors, summarized) = await ctrl.TranscribePagesAsync(Ollama, document, [2, 3],
            new SyncProgress<(int Done, string Text)>(step => steps.Add(step.Text)), CancellationToken.None);

        Assert.AreEqual(1, transcribed, "page 3 was transcribed meanwhile: skipped");
        Assert.AreEqual(0, errors.Count, string.Join(" ", errors));
        CollectionAssert.AreEqual(new[] { 2 }, calls.Saved);
        Assert.IsTrue(summarized);
        Assert.AreEqual((1, "A text.", "qwen3.5:9b"), (calls.Summaries, document.Description, document.DescriptionModel));
        Assert.IsFalse(document.IsDirty(), "updated without making the editor dirty");
        CollectionAssert.AreEqual(new[] { "Transcribing page 1 of 2…", "Transcribing page 2 of 2…", "Summarizing the document…" }, steps);
    }

    [TestMethod]
    public async Task ADocumentWithADescriptionOfItsOwn_IsNotSummarized()
    {
        var (ctrl, calls) = await CreateCtrlAsync();
        var document = Document("The manual of the QL, by Lucía.");

        var (transcribed, _, summarized) = await ctrl.TranscribePagesAsync(Ollama, document, [1, 2], null, CancellationToken.None);

        Assert.AreEqual(2, transcribed);
        Assert.IsFalse(summarized);
        Assert.AreEqual(0, calls.Summaries);
        Assert.AreEqual("The manual of the QL, by Lucía.", document.Description);
    }

    [TestMethod]
    public async Task Canceling_StopsAndSummarizesNothing()
    {
        var (ctrl, calls) = await CreateCtrlAsync();
        using var canceled = new CancellationTokenSource();
        canceled.Cancel();

        var (transcribed, errors, summarized) = await ctrl.TranscribePagesAsync(Ollama, Document(), [1, 2], null, canceled.Token);

        Assert.AreEqual((0, 0, false), (transcribed, errors.Count, summarized));
        Assert.AreEqual((0, 0), (calls.ModelCalls, calls.Summaries));
    }

    // Progress<T> posts to the context it was made in; this one reports at once.
    private sealed class SyncProgress<T>(Action<T> report) : IProgress<T>
    {
        public void Report(T value) => report(value);
    }
}

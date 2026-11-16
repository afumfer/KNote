using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Store.DocumentProcessing (KNote Atlas, stage 2, 2.1a and 2.1b): the summary or the translation of a long document in ClientWin, one at a time and never
/// with another run of KNote Atlas, with its progress published on Store.Events and in the status bar, and its end.
/// </summary>
[TestClass]
public class DocumentProcessingJobTests
{
    [TestMethod]
    public async Task ASummary_PublishesItsProgress_UntilItsNote()
    {
        var store = new Store(new TestFactoryViews());
        var service = new FakeKntService();
        var release = new TaskCompletionSource<Result<DocumentProcessProgressDto>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var reported = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var published = 0;
        store.Events.Subscribe<DocumentProcessingProgressChanged>(_ => Interlocked.Increment(ref published));
        store.DocumentProcessing.Runner = (_, _, request, progress, _) =>
        {
            progress.Report(new DocumentProcessProgressDto { ResourceId = request.ResourceId, FileName = "manual.pdf", Phase = EnumDocumentProcessPhase.Reading, CallsDone = 3, CallsTotal = 12 });
            reported.TrySetResult();
            return release.Task;
        };

        Assert.IsTrue(store.DocumentProcessing.Start(service, "Notes", new AiProviderRef { Model = "qwen3.5:9b" },
            new DocumentProcessRequestDto { ResourceId = Guid.NewGuid() }, hasSemanticIndex: false));
        await reported.Task.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.IsTrue(store.DocumentProcessing.IsRunning);
        Assert.IsTrue(store.AtlasJobRunning, "no other run of KNote Atlas meanwhile");
        Assert.AreEqual("Extended summary of manual.pdf: 3/12", store.DocumentProcessing.StatusBarText);

        release.SetResult(new Result<DocumentProcessProgressDto>(new DocumentProcessProgressDto { Phase = EnumDocumentProcessPhase.Completed, NoteNumber = 42 }));
        await store.DocumentProcessing.Completion;

        Assert.IsFalse(store.DocumentProcessing.IsRunning);
        Assert.IsNull(store.DocumentProcessing.StatusBarText);
        Assert.AreEqual(42, store.DocumentProcessing.Progress.NoteNumber);
        Assert.AreEqual("Notes", store.DocumentProcessing.Progress.RepositoryName);
        Assert.IsGreaterThanOrEqualTo(2, published);
    }

    // 2.1b: a translation, the same job; canceled, what was translated is in its note.
    [TestMethod]
    public async Task ATranslation_SaysItsPage_AndCanceledKeepsItsNote()
    {
        var store = new Store(new TestFactoryViews());
        var release = new TaskCompletionSource<Result<DocumentProcessProgressDto>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var reported = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        store.DocumentProcessing.Runner = (_, _, request, progress, _) =>
        {
            progress.Report(new DocumentProcessProgressDto
            {
                Kind = request.Kind, FileName = "manual.pdf", Phase = EnumDocumentProcessPhase.Translating, Page = 41, PagesCount = 138
            });
            reported.TrySetResult();
            return release.Task;
        };

        Assert.IsTrue(store.DocumentProcessing.Start(new FakeKntService(), "Notes", new AiProviderRef { Model = "qwen3.5:9b" },
            new DocumentProcessRequestDto { ResourceId = Guid.NewGuid(), Kind = EnumDocumentProcessKind.Translate, Language = "Spanish" }, hasSemanticIndex: false));
        await reported.Task.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.AreEqual("Translating manual.pdf: page 41/138", store.DocumentProcessing.StatusBarText);

        release.SetResult(new Result<DocumentProcessProgressDto>(new DocumentProcessProgressDto { Phase = EnumDocumentProcessPhase.Canceled, NoteNumber = 43 }));
        await store.DocumentProcessing.Completion;

        Assert.AreEqual(EnumDocumentProcessKind.Translate, store.DocumentProcessing.Progress.Kind);
        Assert.AreEqual("Canceled: what was translated is in the note #43; «Continue» goes on with it.", SummaryTexts.Progress(store.DocumentProcessing.Progress));
    }

    [TestMethod]
    public async Task ASummary_DoesntStart_WhileAnotherRunOfKNoteAtlasGoesOn()
    {
        var store = new Store(new TestFactoryViews());
        var service = new FakeKntService();
        var held = new TaskCompletionSource<Result<ResourceProcessingProgressDto>>(TaskCreationOptions.RunContinuationsAsynchronously);
        store.ResourceProcessing.Runner = (_, _, _, _, _) => held.Task;
        Assert.IsTrue(store.ResourceProcessing.Start(service, "Notes", new AiProviderRef(), new ResourceProcessingOptionsDto(), false));

        Assert.IsFalse(store.DocumentProcessing.Start(service, "Notes", new AiProviderRef(), new DocumentProcessRequestDto(), false));

        held.SetResult(new Result<ResourceProcessingProgressDto>(new ResourceProcessingProgressDto { Phase = EnumResourceProcessingPhase.Completed }));
        await store.ResourceProcessing.Completion;
    }

    [TestMethod]
    public async Task AFailedSummary_SaysWhy()
    {
        var store = new Store(new TestFactoryViews());
        var failed = new Result<DocumentProcessProgressDto>();
        failed.AddErrorMessage("The document has no text to summarize.");
        store.DocumentProcessing.Runner = (_, _, _, _, _) => Task.FromResult(failed);

        Assert.IsTrue(store.DocumentProcessing.Start(new FakeKntService(), "Notes", new AiProviderRef(), new DocumentProcessRequestDto(), false));
        await store.DocumentProcessing.Completion;

        Assert.AreEqual(EnumDocumentProcessPhase.Failed, store.DocumentProcessing.Progress.Phase);
        Assert.AreEqual("The document has no text to summarize.", store.DocumentProcessing.Progress.Message);
    }
}

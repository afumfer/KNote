using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.ClientWin.Tests.Helpers;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// ClientWin's side of process_document (KNote Atlas 2.1b2b): the model is the one of «Extended summary» (the repository's
/// recommended LLM), and starting it is the run of Store.DocumentProcessing in the assistant's repository, refused while another
/// run of KNote Atlas goes on.
/// </summary>
[TestClass]
public class KNoteAiToolsHostDocumentTests
{
    private static readonly AiProviderRef Recommended = new() { Alias = "Good", Provider = EnumAiProvider.OpenAI, Model = "gpt-6-luna", Kind = EnumAiProviderKind.Chat };

    [TestMethod]
    public async Task ItStartsTheRunOfTheStore_WithTheRepositorysModel_AndNotWhileAnotherOneGoesOn()
    {
        var service = new FakeKntService();
        var store = TestStoreFactory.CreateEmpty().GrantAdmin(service);
        store.Settings.Ai.Providers.Add(new AiProviderRef { Alias = "Local", Provider = EnumAiProvider.Ollama, Model = "small", Kind = EnumAiProviderKind.Chat });
        store.Settings.Ai.Providers.Add(Recommended);
        service.SystemValuesFake.GetRepositoryAiModelsAsyncImpl = () => Task.FromResult(new Result<RepositoryAiModelsDto>(new RepositoryAiModelsDto { LlmModel = "gpt-6-luna" }));
        service.SemanticSearchFake.GetIndexStatusAsyncImpl = () => Task.FromResult(new Result<SemanticIndexStatusDto>(new SemanticIndexStatusDto { Status = EnumSemanticIndexStatus.None }));
        var host = new KNoteAiToolsHost(store, TestServiceRefFactory.CreateWithFakeService(service));
        var release = new TaskCompletionSource<Result<DocumentProcessProgressDto>>(TaskCreationOptions.RunContinuationsAsynchronously);
        DocumentProcessRequestDto? asked = null;
        store.DocumentProcessing.Runner = (_, _, request, _, _) =>
        {
            asked = request;
            return release.Task;
        };
        var request = new DocumentProcessRequestDto { ResourceId = Guid.NewGuid(), Kind = EnumDocumentProcessKind.Translate, Language = "Spanish" };

        Assert.IsTrue(host.CanProcessDocuments);
        var model = await host.GetDocumentModelAsync(service);
        var started = await host.StartDocumentProcessAsync(service, model, request);
        var again = await host.StartDocumentProcessAsync(service, model, request);

        Assert.AreEqual("Good", model.Alias, "the repository's recommended LLM");
        Assert.IsNull(started);
        Assert.IsTrue(store.DocumentProcessing.IsRunning);
        Assert.AreEqual(SummaryTexts.OtherRunning, again);

        release.SetResult(new Result<DocumentProcessProgressDto>(new DocumentProcessProgressDto { Kind = request.Kind, Phase = EnumDocumentProcessPhase.Completed }));
        await store.DocumentProcessing.Completion;
        Assert.AreEqual(request.ResourceId, asked!.ResourceId);
    }

    [TestMethod]
    public void WithoutARepository_ItIsNotOffered() =>
        Assert.IsFalse(new KNoteAiToolsHost(TestStoreFactory.CreateEmpty()).CanProcessDocuments);
}

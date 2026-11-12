using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.ClientWin.Tests.Helpers;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// RepositoryAiModelsCtrl (Â«Repository modelsÂ» of the KNote Atlas window): it reads and saves the repository's models, a
/// change of embedding model is confirmed first, the changes not saved are saved, discarded or kept when the editor
/// is accepted, and the view says whether this installation has each model and whether it is a paid one.
/// </summary>
[TestClass]
public class RepositoryAiModelsCtrlTests
{
    private static (RepositoryAiModelsCtrl ctrl, FakeEditorView<RepositoryAiModelsDto> view, FakeKntService service) CreateCtrl(
        RepositoryAiModelsDto? saved = null, FakeAiProvidersManageView? providersView = null)
    {
        var factoryViews = new TestFactoryViews();
        var view = new FakeEditorView<RepositoryAiModelsDto>();
        factoryViews.Registry.Register<RepositoryAiModelsCtrl, IViewEditorEmbeddable<RepositoryAiModelsDto>>(c => view);
        if (providersView != null)
            factoryViews.Registry.Register<AiProvidersManageCtrl, IViewManageList<AiProviderRef>>(c => providersView);

        var store = new Store(factoryViews);
        store.Settings.Ai.Providers.Add(new AiProviderRef { Alias = "Local embeddings", Provider = EnumAiProvider.Ollama,
            Model = "qwen3-embedding:0.6b", Host = "http://localhost:11434", Kind = EnumAiProviderKind.Embeddings });
        store.Settings.Ai.Providers.Add(new AiProviderRef { Alias = "Paid embeddings", Provider = EnumAiProvider.OpenAI,
            Model = "text-embedding-3-small", Kind = EnumAiProviderKind.Embeddings });
        store.Settings.Ai.Providers.Add(new AiProviderRef { Alias = "Chat", Provider = EnumAiProvider.Ollama,
            Model = "qwen3.5:9b", Host = "http://localhost:11434" });
        store.Settings.Ai.Providers.Add(new AiProviderRef { Alias = "Paid chat", Provider = EnumAiProvider.OpenAI, Model = "gpt-4o-mini" });

        var service = new FakeKntService { RepositoryRef = new RepositoryRef { Name = "Notes" } };
        var stored = saved ?? new RepositoryAiModelsDto();
        service.SystemValuesFake.GetRepositoryAiModelsAsyncImpl = () => Task.FromResult(new Result<RepositoryAiModelsDto>(Copy(stored)));
        service.SystemValuesFake.SaveRepositoryAiModelsAsyncImpl = m => Task.FromResult(new Result<RepositoryAiModelsDto>(Copy(m)));
        store.AddServiceRef(TestServiceRefFactory.CreateWithFakeService(service));
        store.GrantAdmin(service);

        return (new RepositoryAiModelsCtrl(store), view, service);
    }

    private static RepositoryAiModelsDto Copy(RepositoryAiModelsDto m) => new()
    {
        EmbeddingModel = m.EmbeddingModel, EmbeddingDimensions = m.EmbeddingDimensions, QueryInstruction = m.QueryInstruction, LlmModel = m.LlmModel
    };

    [TestMethod]
    public void TheInstallationsModels_AreOfferedByKind()
    {
        var (ctrl, _, _) = CreateCtrl();

        CollectionAssert.AreEqual(new[] { "qwen3-embedding:0.6b", "text-embedding-3-small" }, ctrl.EmbeddingModels);
        CollectionAssert.AreEqual(new[] { "qwen3.5:9b", "gpt-4o-mini" }, ctrl.ChatModels);
    }

    [TestMethod]
    public async Task NewModel_ReadsTheRepositorysModels_NotDirty_AndShowsThem()
    {
        var (ctrl, view, service) = CreateCtrl(new RepositoryAiModelsDto { EmbeddingModel = "qwen3-embedding:0.6b" });

        Assert.IsTrue(await ctrl.NewModel(service));

        Assert.AreEqual("qwen3-embedding:0.6b", ctrl.Model.EmbeddingModel);
        Assert.AreEqual("qwen3-embedding:0.6b", ctrl.Saved.EmbeddingModel);
        Assert.IsFalse(ctrl.Model.IsDirty());
        Assert.AreSame(service, ctrl.Service);
        Assert.AreEqual(1, view.RefreshViewCount);
    }

    [TestMethod]
    public async Task SaveModel_TheFirstEmbeddingModel_IsSavedWithoutAsking_AndTheTabStaysOpen()
    {
        var (ctrl, view, service) = CreateCtrl();
        await ctrl.NewModel(service);
        RepositoryAiModelsDto? sent = null;
        service.SystemValuesFake.SaveRepositoryAiModelsAsyncImpl = m => { sent = m; return Task.FromResult(new Result<RepositoryAiModelsDto>(Copy(m))); };
        view.RefreshModelImpl = () => ctrl.Model.EmbeddingModel = "qwen3-embedding:0.6b";

        Assert.IsTrue(await ctrl.SaveModel());

        Assert.AreEqual("qwen3-embedding:0.6b", sent?.EmbeddingModel);
        Assert.AreEqual("qwen3-embedding:0.6b", ctrl.Saved.EmbeddingModel);
        Assert.AreEqual(0, view.ShownInfo.Count);
        Assert.AreEqual(2, view.RefreshViewCount, "The saved models are shown again.");
        Assert.AreNotEqual(EControllerState.Finalized, ctrl.ControllerState);
    }

    [TestMethod]
    [DataRow(DialogResult.No, false)]
    [DataRow(DialogResult.Yes, true)]
    public async Task SaveModel_AnotherEmbeddingModel_IsConfirmedFirst(DialogResult answer, bool saved)
    {
        var (ctrl, view, service) = CreateCtrl(new RepositoryAiModelsDto { EmbeddingModel = "qwen3-embedding:0.6b" });
        await ctrl.NewModel(service);
        var calls = 0;
        service.SystemValuesFake.SaveRepositoryAiModelsAsyncImpl = m => { calls++; return Task.FromResult(new Result<RepositoryAiModelsDto>(Copy(m))); };
        view.ShowInfoImpl = (_, buttons) => buttons == MessageBoxButtons.YesNo ? answer : DialogResult.OK;
        view.RefreshModelImpl = () => ctrl.Model.EmbeddingModel = "qwen3-embedding:4b";

        Assert.AreEqual(saved, await ctrl.SaveModel());

        Assert.AreEqual(saved ? 1 : 0, calls);
        StringAssert.Contains(view.ShownInfo.Single(), "built again");
    }

    [TestMethod]
    public async Task SaveModel_OnlyTheLlm_IsNotAskedAbout()
    {
        var (ctrl, view, service) = CreateCtrl(new RepositoryAiModelsDto { EmbeddingModel = "qwen3-embedding:0.6b" });
        await ctrl.NewModel(service);
        view.RefreshModelImpl = () => ctrl.Model.LlmModel = "qwen3.5:9b";

        Assert.IsTrue(await ctrl.SaveModel());

        Assert.AreEqual(0, view.ShownInfo.Count);
    }

    [TestMethod]
    public async Task SaveModel_Invalid_IsNotSaved()
    {
        var (ctrl, view, service) = CreateCtrl();
        await ctrl.NewModel(service);
        var calls = 0;
        service.SystemValuesFake.SaveRepositoryAiModelsAsyncImpl = m => { calls++; return Task.FromResult(new Result<RepositoryAiModelsDto>(Copy(m))); };
        view.RefreshModelImpl = () => ctrl.Model.EmbeddingDimensions = 512;

        Assert.IsFalse(await ctrl.SaveModel());

        Assert.AreEqual(0, calls);
        Assert.AreEqual(1, view.ShownInfo.Count);
    }

    [TestMethod]
    public async Task HasPendingChanges_OnlyWithChangesNotSaved()
    {
        var (ctrl, view, service) = CreateCtrl();
        Assert.IsFalse(ctrl.HasPendingChanges(), "Nothing read yet.");

        await ctrl.NewModel(service);
        Assert.IsFalse(ctrl.HasPendingChanges());

        view.RefreshModelImpl = () => ctrl.Model.LlmModel = "qwen3.5:9b";
        Assert.IsTrue(ctrl.HasPendingChanges());
    }

    [TestMethod]
    public async Task ResolvePendingChanges_WithoutChanges_DoesNotAsk()
    {
        var (ctrl, view, service) = CreateCtrl();
        await ctrl.NewModel(service);

        Assert.IsTrue(await ctrl.ResolvePendingChangesAsync());

        Assert.AreEqual(0, view.ShownInfo.Count);
    }

    [TestMethod]
    [DataRow(DialogResult.Yes, true, 1, "qwen3.5:9b")]
    [DataRow(DialogResult.No, true, 0, null)]
    [DataRow(DialogResult.Cancel, false, 0, "qwen3.5:9b")]
    public async Task ResolvePendingChanges_SavesDiscardsOrKeepsThem(DialogResult answer, bool resolved, int saves, string? llmAfter)
    {
        var (ctrl, view, service) = CreateCtrl();
        await ctrl.NewModel(service);
        var calls = 0;
        service.SystemValuesFake.SaveRepositoryAiModelsAsyncImpl = m => { calls++; return Task.FromResult(new Result<RepositoryAiModelsDto>(Copy(m))); };
        view.RefreshModelImpl = () => ctrl.Model.LlmModel = "qwen3.5:9b";
        view.ShowInfoImpl = (_, buttons) => buttons == MessageBoxButtons.YesNoCancel ? answer : DialogResult.OK;

        var result = await ctrl.ResolvePendingChangesAsync();

        Assert.AreEqual(resolved, result);
        Assert.AreEqual(saves, calls);
        Assert.AreEqual(llmAfter, ctrl.Model.LlmModel);
        StringAssert.Contains(view.ShownInfo.First(), "not saved");
    }

    [TestMethod]
    public async Task ManageAiProviders_KeepsWhatIsTyped_AndShowsTheNewProviders()
    {
        var providersView = new FakeAiProvidersManageView();
        var (ctrl, view, service) = CreateCtrl(providersView: providersView);
        await ctrl.NewModel(service);
        view.RefreshModelImpl = () => ctrl.Model.EmbeddingModel = "qwen3-embedding:4b";
        StringAssert.Contains(ctrl.DescribeEmbeddingModel("qwen3-embedding:4b"), "no embeddings provider");
        providersView.ShowModalImpl = () => ctrl.Store.Settings.Ai.Providers.Add(new AiProviderRef { Alias = "Bigger embeddings",
            Provider = EnumAiProvider.Ollama, Model = "qwen3-embedding:4b", Host = "http://localhost:11434", Kind = EnumAiProviderKind.Embeddings });

        await ctrl.ManageAiProvidersAsync();

        Assert.AreEqual(1, providersView.ShowModalCount);
        Assert.AreEqual("qwen3-embedding:4b", ctrl.Model.EmbeddingModel, "What was typed is kept.");
        Assert.AreEqual(2, view.RefreshViewCount, "Shown again with the new provider.");
        CollectionAssert.Contains(ctrl.EmbeddingModels, "qwen3-embedding:4b");
        StringAssert.Contains(ctrl.DescribeEmbeddingModel("qwen3-embedding:4b"), "Available in this installation");
    }

    [TestMethod]
    public void DescribeEmbeddingModel_SaysWhetherItIsHereAndWhetherItIsPaid()
    {
        var (ctrl, _, _) = CreateCtrl();

        StringAssert.Contains(ctrl.DescribeEmbeddingModel(" QWEN3-embedding:0.6b "), "Available in this installation");
        StringAssert.Contains(ctrl.DescribeEmbeddingModel("text-embedding-3-small"), "paid");
        StringAssert.Contains(ctrl.DescribeEmbeddingModel("qwen3-embedding:4b"), "no embeddings provider");
        StringAssert.Contains(ctrl.DescribeEmbeddingModel("qwen3.5:9b"), "no embeddings provider", "A chat model is not an embedding model.");
        StringAssert.Contains(ctrl.DescribeEmbeddingModel(""), "No embedding model");
    }

    [TestMethod]
    public void DescribeLlmModel_SaysWhetherItIsHereAndWhetherItIsPaid()
    {
        var (ctrl, _, _) = CreateCtrl();

        StringAssert.Contains(ctrl.DescribeLlmModel(" QWEN3.5:9B "), "Available in this installation");
        StringAssert.Contains(ctrl.DescribeLlmModel("gpt-4o-mini"), "paid");
        StringAssert.Contains(ctrl.DescribeLlmModel("qwen3.6:35b-a3b"), "no chat provider");
        StringAssert.Contains(ctrl.DescribeLlmModel("qwen3-embedding:0.6b"), "no chat provider", "An embedding model is not an LLM.");
        StringAssert.Contains(ctrl.DescribeLlmModel(""), "No recommended LLM");
    }
}

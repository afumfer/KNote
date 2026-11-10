using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

/// <summary>
/// Section Â«Repository modelsÂ» of the KNote Atlas window (AtlasCtrl): the AI models a repository
/// declares (CU-02), its embedding model, which every installation working with it must use, and the recommended LLM.
/// They are SystemValues of the repository (Service.SystemValues.Get/SaveRepositoryAiModelsAsync, Admin); where each
/// model runs is this installation's AI providers (Store.Settings.Ai.Providers). Included in the KNote Atlas window
/// (no [KntAuthorize]: it decides), and saved by its own button, as the other tabs save at once; the editor asks about
/// the changes not saved when it is closed (ResolvePendingChangesAsync, HasPendingChanges).
/// </summary>
public class RepositoryAiModelsCtrl : CtrlEditorBase<IViewEditorEmbeddable<RepositoryAiModelsDto>, RepositoryAiModelsDto>
{
    #region Constructor

    public RepositoryAiModelsCtrl(Store store) : base(store)
    {
        ControllerName = "Repository AI models";
    }

    #endregion

    #region Properties

    // The models as they are saved in the repository (Model is the copy being edited).
    public RepositoryAiModelsDto Saved { get; private set; }

    // This installation's embedding and chat models (its AI providers), to choose from.
    public List<string> EmbeddingModels => ModelsOf(p => p.IsEmbeddings);

    public List<string> ChatModels => ModelsOf(p => p.IsChat);

    #endregion

    #region Controller editor implementation

    protected override IViewEditorEmbeddable<RepositoryAiModelsDto> CreateView()
    {
        return Store.FactoryViews.Registry.Resolve<RepositoryAiModelsCtrl, IViewEditorEmbeddable<RepositoryAiModelsDto>>(this);
    }

    // Not applicable: the models are one per repository. Use NewModel(service), which reads them.
    public override Task<bool> LoadModelById(IKntService service, Guid id, bool refreshView = true)
    {
        throw new NotSupportedException();
    }

    // Reads the repository's models.
    public override async Task<bool> NewModel(IKntService service)
    {
        try
        {
            Service = service;
            var result = await Service.SystemValues.GetRepositoryAiModelsAsync();
            if (!result.IsValid)
            {
                ShowResultError(result);
                return false;
            }

            Show(result.Entity);
            View.RefreshView();
            return true;
        }
        catch (Exception ex)
        {
            View.ShowInfo(ex.Message);
            return false;
        }
    }

    public override async Task<bool> SaveModel()
    {
        View.RefreshModel();

        if (!Model.IsDirty())
            return true;

        var msgVal = Model.GetErrorMessage();
        if (!string.IsNullOrEmpty(msgVal))
        {
            View.ShowInfo(msgVal);
            return false;
        }

        if (ChangesEmbeddingModel(Model))
        {
            var answer = View.ShowInfo(
                $"The repository uses the embedding model {Saved.EmbeddingModel}. With another model or other dimensions its semantic index has to be built again. Change it?",
                "Change the embedding model", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (answer != DialogResult.Yes)
                return false;
        }

        try
        {
            var response = await Service.SystemValues.SaveRepositoryAiModelsAsync(Model);
            if (response.IsValid)
            {
                Show(response.Entity);
                View.RefreshView();
                OnSavedEntity(Model);
                return true;
            }
            else
            {
                ShowResultError(response);
                return false;
            }
        }
        catch (Exception ex)
        {
            View.ShowInfo(ex.Message);
            return false;
        }
    }

    // Not applicable: an empty embedding model is how the models are removed.
    public override Task<bool> DeleteModel(IKntService service, Guid id)
    {
        throw new NotSupportedException();
    }

    public override Task<bool> DeleteModel()
    {
        throw new NotSupportedException();
    }

    #endregion

    #region Public methods

    // Tools > Manage AI providers without leaving the editor (as the AI assistant's "Manage models"): what is typed is
    // kept, and the lists and notices are shown again with the providers as they are left.
    public async Task ManageAiProvidersAsync()
    {
        View.RefreshModel();

        var manageCtrl = new AiProvidersManageCtrl(Store);
        await manageCtrl.LoadEntitiesAsync(null, false);
        manageCtrl.RunModal();

        View.RefreshView();
    }

    // Whether the view has changes not saved yet (nothing to save before the models are read).
    public bool HasPendingChanges()
    {
        if (Service == null)
            return false;

        View.RefreshModel();
        return Model.IsDirty();
    }

    // Before the KNote Atlas window changes the repository or closes: the changes not saved yet are saved, discarded or kept (false: the
    // editor stays open).
    public async Task<bool> ResolvePendingChangesAsync()
    {
        if (!HasPendingChanges())
            return true;

        var answer = View.ShowInfo("The repository's AI models (Repository models) have changes not saved. Save them?",
            "Repository AI models", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

        switch (answer)
        {
            case DialogResult.Yes:
                return await SaveModel();
            case DialogResult.No:
                Show(Saved);
                View.RefreshView();
                return true;
            default:
                return false;
        }
    }

    // Whether saving these models changes the embedding model the repository already had (its index must be rebuilt).
    public bool ChangesEmbeddingModel(RepositoryAiModelsDto models) =>
        !string.IsNullOrEmpty(Saved?.EmbeddingModel) && !models.SameEmbeddingModel(Saved);

    // What the view says about an embedding model: whether this installation has it and whether it is a paid one.
    public string DescribeEmbeddingModel(string model)
    {
        model = RepositoryAiModelsDto.Normalize(model);
        if (model == null)
            return "No embedding model: no search by meaning in this repository.";

        var provider = ProviderOf(p => p.IsEmbeddings, model);
        if (provider == null)
            return "This installation has no embeddings provider with this model: the search by meaning will not work here (Tools > Manage AI providers).";

        return provider.IsPaid
            ? $"A paid model ({provider.Provider}): building the index sends the content of every note to the provider."
            : $"Available in this installation ({provider.Provider}, {provider.Alias}).";
    }

    // The same for the recommended LLM, among the chat providers.
    public string DescribeLlmModel(string model)
    {
        model = RepositoryAiModelsDto.Normalize(model);
        if (model == null)
            return "No recommended LLM: each AI task uses the installation's default.";

        var provider = ProviderOf(p => p.IsChat, model);
        if (provider == null)
            return "This installation has no chat provider with this model: the AI tasks will use another one here (Tools > Manage AI providers).";

        return provider.IsPaid
            ? $"A paid model ({provider.Provider}): the AI tasks send the content of the notes to the provider."
            : $"Available in this installation ({provider.Provider}, {provider.Alias}).";
    }

    #endregion

    #region Private methods

    private void Show(RepositoryAiModelsDto entity)
    {
        Saved = entity;
        Model = new RepositoryAiModelsDto
        {
            EmbeddingModel = entity.EmbeddingModel,
            EmbeddingDimensions = entity.EmbeddingDimensions,
            QueryInstruction = entity.QueryInstruction,
            LlmModel = entity.LlmModel,
            EmbeddingModelAvailable = entity.EmbeddingModelAvailable
        };
        Model.SetIsDirty(false);
    }

    private List<string> ModelsOf(Func<AiProviderRef, bool> kind) =>
        Store.Settings.Ai.Providers
            .Where(p => kind(p) && p.IsValid())
            .Select(p => p.Model.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    private AiProviderRef ProviderOf(Func<AiProviderRef, bool> kind, string model) =>
        Store.Settings.Ai.Providers.FirstOrDefault(p =>
            kind(p) && p.IsValid() && string.Equals(p.Model?.Trim(), model, StringComparison.OrdinalIgnoreCase));

    #endregion
}

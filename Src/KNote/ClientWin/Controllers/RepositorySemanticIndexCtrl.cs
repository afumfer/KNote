using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

/// <summary>
/// The "Semantic index" tab of the repository editor (RepositoryEditorCtrl, Management mode, CU-03): the status of the
/// repository's semantic index and creating, updating or rebuilding it (Service.SemanticSearch), with its progress, its
/// cancellation and the indexes of other models kept in the repository. The run is the Store's (Store.SemanticIndex,
/// one at a time in the app): it goes on when the editor is closed, and opening it again shows its progress. Included
/// in the repository editor (no [KntAuthorize]: it decides); the commands check the role (Admin to run).
/// </summary>
public class RepositorySemanticIndexCtrl : CtrlViewEmbeddableBase<IViewSemanticIndex>
{
    #region Constructor

    public RepositorySemanticIndexCtrl(Store store) : base(store)
    {
        ControllerName = "Repository semantic index";
        Store.Events.Subscribe<SemanticIndexProgressChanged>(OnProgressChanged);
    }

    #endregion

    #region Properties

    public IKntService Service { get; private set; }

    public string RepositoryName { get; set; }

    public SemanticIndexStatusDto Status { get; private set; }

    // The run of this repository (running or the last one), or null.
    public SemanticIndexProgressDto Progress => Store.SemanticIndex.IsOf(Service) ? Store.SemanticIndex.Progress : null;

    public bool IsRunning => Store.SemanticIndex.IsRunning && Store.SemanticIndex.IsOf(Service);

    // Another repository's run: this one waits (one at a time in the app).
    public bool OtherRepositoryRunning => Store.SemanticIndex.IsRunning && !Store.SemanticIndex.IsOf(Service);

    public bool CanRun => Status?.IndexKey != null && Status.EmbeddingModelAvailable && !Store.SemanticIndex.IsRunning;

    public bool CanUpdate => CanRun && !(Status.Status == EnumSemanticIndexStatus.Ready && Status.PendingNotes == 0);

    public bool CanRebuild => CanRun && Status.Status != EnumSemanticIndexStatus.None;

    public string RunText => Status?.Status == EnumSemanticIndexStatus.None ? "&Create index" : "&Update now";

    public string StatusText =>
        Status == null ? "" : Status.IndexKey == null
            ? "The repository has no embedding model: choose one in the KNote Atlas tab to create its index."
            : $"{SemanticIndexTexts.Status(Status)} · model {Status.IndexKey}";

    public string CountsText => Status?.IndexKey == null ? "" : SemanticIndexTexts.Counts(Status);

    public string LastCreationText => Status?.IndexKey == null ? "" : SemanticIndexTexts.LastCreation(Status) ?? "";

    public string WarningText =>
        Status?.IndexKey != null && !Status.EmbeddingModelAvailable
            ? $"This installation has no embeddings provider with the model {Status.EmbeddingModel}: the index cannot be created or updated here (Tools > Manage AI providers)."
            : OtherRepositoryRunning
                ? $"The index of {Store.SemanticIndex.RepositoryName} is being created: one index at a time."
                : "";

    public string ProgressText => Progress is { } progress ? SemanticIndexTexts.Progress(progress) : "";

    public List<string> ErrorLines =>
        Progress is { ErrorsCount: > 0 } progress ? new[] { SemanticIndexTexts.Errors(progress) }.Concat(progress.Errors).ToList() : new();

    #endregion

    #region Controller implementation

    protected override IViewSemanticIndex CreateView()
    {
        return Store.FactoryViews.Registry.Resolve<RepositorySemanticIndexCtrl, IViewSemanticIndex>(this);
    }

    #endregion

    #region Public methods

    // Reads the status of the index of the repository.
    public async Task<bool> LoadAsync(IKntService service, string repositoryName)
    {
        Service = service;
        RepositoryName = repositoryName;
        return await RefreshStatusAsync();
    }

    public async Task<bool> RefreshStatusAsync()
    {
        if (Service == null)
            return false;

        try
        {
            var result = await Service.SemanticSearch.GetIndexStatusAsync();
            if (!result.IsValid)
            {
                ShowResultError(result);
                return false;
            }
            Status = result.Entity;
            View.RefreshView();
            return true;
        }
        catch (Exception ex)
        {
            View.ShowInfo(ex.Message);
            return false;
        }
    }

    // Creates or updates the index (its pending notes) or rebuilds it, after saying what it is going to do.
    public bool Run(bool rebuild)
    {
        if (!(rebuild ? CanRebuild : CanUpdate))
            return false;

        var answer = View.ShowInfo(SemanticIndexTexts.RunConfirmation(Status, rebuild, IsPaidModel()), SemanticIndexTexts.RunTitle(Status, rebuild),
            MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
        if (answer != DialogResult.OK)
            return false;

        if (!Store.SemanticIndex.Start(Service, RepositoryName, rebuild))
            return false;
        View.RefreshView();
        return true;
    }

    public void CancelRun()
    {
        if (IsRunning)
            Store.SemanticIndex.Cancel();
    }

    public async Task<bool> DeleteIndexAsync(string indexKey)
    {
        var index = Status?.OtherIndexes.FirstOrDefault(i => i.IndexKey == indexKey);
        if (index == null)
            return false;

        var answer = View.ShowInfo($"Remove the index of the model {index.IndexKey} ({index.Chunks} fragments)?", "Remove an index",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (answer != DialogResult.Yes)
            return false;

        try
        {
            var result = await Service.SemanticSearch.DeleteModelIndexAsync(indexKey);
            if (!result.IsValid)
            {
                ShowResultError(result);
                return false;
            }
            return await RefreshStatusAsync();
        }
        catch (Exception ex)
        {
            View.ShowInfo(ex.Message);
            return false;
        }
    }

    public override void Dispose()
    {
        Store.Events.Unsubscribe<SemanticIndexProgressChanged>(OnProgressChanged);
        base.Dispose();
    }

    #endregion

    #region Private methods

    // Whether this installation's provider of the embedding model is a paid one (its content is sent).
    private bool IsPaidModel() =>
        Store.Settings.Ai.Providers.Any(p => p.IsEmbeddings && p.IsValid() && p.IsPaid
            && string.Equals(p.Model?.Trim(), Status?.EmbeddingModel, StringComparison.OrdinalIgnoreCase));

    private async void OnProgressChanged(SemanticIndexProgressChanged e)
    {
        if (Service == null)
            return;

        if (!e.Job.IsOf(Service))
        {
            // Another repository's run started or ended: the buttons change.
            View.RefreshView();
            return;
        }

        View.RefreshProgress();
        if (!e.Job.IsRunning)
            await RefreshStatusAsync();
    }

    #endregion
}

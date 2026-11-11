using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

/// <summary>
/// Section Â«Process pending resourcesÂ» of the KNote Atlas window (AtlasCtrl; KNote Atlas, stage 4, D10): what
/// «Process pending resources» would process (Service.ResourceContent) and running it, with the choice for the descriptions
/// written by people, its progress and its cancellation. The run is the Store's (Store.ResourceProcessing, one run of KNote
/// Atlas at a time in the app): it goes on when the editor is closed, and opening it again shows its progress. Included in
/// the KNote Atlas window (no [KntAuthorize]: it decides); the commands check the role (Admin to run).
/// </summary>
public class RepositoryResourcesCtrl : CtrlViewEmbeddableBase<IViewResourceProcessing>
{
    #region Constructor

    public RepositoryResourcesCtrl(Store store) : base(store)
    {
        ControllerName = "Repository resources";
        Store.Events.Subscribe<ResourceProcessingProgressChanged>(OnProgressChanged);
        Store.Events.Subscribe<SemanticIndexProgressChanged>(OnIndexProgressChanged);
    }

    #endregion

    #region Properties

    public IKntService Service { get; private set; }

    public string RepositoryName { get; set; }

    public ResourceProcessingStatusDto Status { get; private set; }

    // The choice for the descriptions written by people (the view's radio buttons).
    public EnumPersonDescriptions PersonDescriptions { get; set; } = EnumPersonDescriptions.Skip;

    // What to process (the view's check boxes; 4.4c).
    public bool Photos { get; set; } = true;
    public bool Documents { get; set; } = true;

    public ResourceProcessingOptionsDto Options => new() { Photos = Photos, Documents = Documents, PersonDescriptions = PersonDescriptions };

    // The run of this repository (running or the last one), or null.
    public ResourceProcessingProgressDto Progress => Store.ResourceProcessing.IsOf(Service) ? Store.ResourceProcessing.Progress : null;

    public bool IsRunning => Store.ResourceProcessing.IsRunning && Store.ResourceProcessing.IsOf(Service);

    // Something to do: photos to describe, documents to read or to summarize or, with «Keep them and lock them», descriptions to lock.
    public bool CanRun => Status != null && !Store.AtlasJobRunning && Options.Any && ResourceProcessingTexts.HasWork(Status, Options);

    public bool HasPersonPhotos => PersonPhotosText.Length > 0;

    public string CountsText => Status == null ? "" : ResourceProcessingTexts.Counts(Status);

    public string LastRunText => Status == null ? "" : ResourceProcessingTexts.LastRun(Status) ?? "";

    // The resources with a description written by people, of the kinds chosen.
    public string PersonPhotosText => Status == null ? ""
        : ResourceProcessingTexts.PersonDescriptions(Photos ? Status.PersonPhotos : 0, Documents ? Status.PersonDocuments : 0) ?? "";

    public string WarningText =>
        Store.SemanticIndex.IsRunning
            ? $"The semantic index of {Store.SemanticIndex.RepositoryName} is being created: one run of KNote Atlas at a time."
            : Store.ResourceProcessing.IsRunning && !IsRunning
                ? $"The resources of {Store.ResourceProcessing.RepositoryName} are being processed: one run of KNote Atlas at a time."
                : "";

    public string ProgressText => Progress is { } progress ? ResourceProcessingTexts.Progress(progress) : "";

    public List<string> ErrorLines =>
        Progress is { ErrorsCount: > 0 } progress ? new[] { ResourceProcessingTexts.Errors(progress) }.Concat(progress.Errors).ToList() : new();

    #endregion

    #region Controller implementation

    protected override IViewResourceProcessing CreateView()
    {
        return Store.FactoryViews.Registry.Resolve<RepositoryResourcesCtrl, IViewResourceProcessing>(this);
    }

    #endregion

    #region Public methods

    // Reads what the run would process in the repository.
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
            var result = await Service.ResourceContent.GetProcessingStatusAsync();
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

    // Runs «Process pending resources» after saying what it is going to do (with which model, how long, if it is paid).
    public async Task<bool> RunAsync()
    {
        if (!CanRun)
            return false;

        // The photos need a model that sees images; the documents, any model.
        var options = Options;
        var (provider, describer) = await Store.GetPhotoDescriberAsync(Service);
        if (options.Photos ? !describer.Available : !describer.CanSummarize)
        {
            View.ShowInfo(describer.Message, ResourceProcessingTexts.Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        var answer = View.ShowInfo(ResourceProcessingTexts.RunConfirmation(Status, options, describer), ResourceProcessingTexts.Title,
            MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
        if (answer != DialogResult.OK)
            return false;

        var index = await Service.SemanticSearch.GetIndexStatusAsync();
        var hasIndex = index.IsValid && index.Entity?.Status != EnumSemanticIndexStatus.None && index.Entity?.EmbeddingModelAvailable == true;
        if (!Store.ResourceProcessing.Start(Service, RepositoryName, provider, options, hasIndex))
            return false;
        View.RefreshView();
        return true;
    }

    public void CancelRun()
    {
        if (IsRunning)
            Store.ResourceProcessing.Cancel();
    }

    public override void Dispose()
    {
        Store.Events.Unsubscribe<ResourceProcessingProgressChanged>(OnProgressChanged);
        Store.Events.Unsubscribe<SemanticIndexProgressChanged>(OnIndexProgressChanged);
        base.Dispose();
    }

    #endregion

    #region Private methods

    private async void OnProgressChanged(ResourceProcessingProgressChanged e)
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

    // The index's runs also hold this one back (one Ollama).
    private void OnIndexProgressChanged(SemanticIndexProgressChanged e)
    {
        if (Service != null)
            View.RefreshView();
    }

    #endregion
}

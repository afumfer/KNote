using KNote.Ai;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

/// <summary>
/// «Summarize document» (KNote Atlas, stage 2, CU-43, 2.1a2): the summary of a long document of a note in a new note, with the
/// language, the length and the model; «Estimate» before (pages, calls, time and the scanned pages without text) and the run in the
/// Store (Store.DocumentProcessing), which goes on if the window is closed and is shown again when it opens. At the end, the new
/// note opens in the note editor. The AI and where the summary is kept are KNote.Ai and the Service (AiDocumentProcessor).
/// </summary>
[KntAuthorize(EnumRoles.Staff)]
public class LongDocumentSummaryCtrl : CtrlViewBase<IViewLongDocumentSummary>
{
    public LongDocumentSummaryCtrl(Store store) : base(store)
    {
        ControllerName = SummaryTexts.Title;
        Store.Events.Subscribe<DocumentProcessingProgressChanged>(OnProgressChanged);
        SaveSettings = () => Store.SaveConfig();
    }

    public ServiceRef ServiceRef { get; private set; }

    public IKntService Service => ServiceRef?.Service;

    protected override IKntService AuthorizationResource => Service;

    // The document (a resource saved, of a note of the repository).
    public ResourceDto Document { get; private set; }

    public string FileName => Document?.NameOut;

    // The chat models of this installation, and the one chosen.
    public List<AiProviderRef> Providers { get; private set; } = new();

    public AiProviderRef Provider { get; set; }

    public string Language { get; set; } = "";

    public EnumSummaryLength Length { get; set; } = EnumSummaryLength.Medium;

    // The last estimate, or why it couldn't be done.
    public DocumentProcessEstimateDto Estimate { get; private set; }

    public string EstimateError { get; private set; }

    // The run of the Store, if it is the summary of this document (running or the last one).
    public DocumentProcessProgressDto Progress =>
        Store.DocumentProcessing.IsOf(Service) && Store.DocumentProcessing.Progress?.ResourceId == Document?.ResourceId
            ? Store.DocumentProcessing.Progress : null;

    public bool IsRunning => Store.DocumentProcessing.IsRunning && Progress != null;

    // Another run of KNote Atlas (the index, the resources, or the summary of another document): this one waits.
    public bool OtherRunning => Store.AtlasJobRunning && !IsRunning;

    // «Transcribe them first» (the window that has the transcription of the document opens it).
    public event EventHandler TranscribeRequested;

    protected override IViewLongDocumentSummary CreateView() =>
        Store.FactoryViews.Registry.Resolve<LongDocumentSummaryCtrl, IViewLongDocumentSummary>(this);

    /// <summary>The document to summarize, with the model, the language and the length proposed.</summary>
    public async Task LoadAsync(ServiceRef serviceRef, ResourceDto document)
    {
        ServiceRef = serviceRef;
        Document = document;
        Providers = Store.Settings.Ai.Providers.Where(p => p.IsChat).ToList();

        var (provider, _) = await Store.GetPhotoDescriberAsync(Service);
        Provider = provider ?? Providers.FirstOrDefault();
        Language = Store.State.Session.SummaryLanguages.FirstOrDefault(l => l.RepositoryId == ServiceRef?.RepositoryId)?.Language ?? "";
    }

    public DocumentProcessRequestDto Request() => new()
    {
        ResourceId = Document.ResourceId,
        ProviderAlias = Provider?.Alias,
        Language = Language?.Trim(),
        Length = Length
    };

    /// <summary>What the summary would take; the view shows it.</summary>
    public async Task EstimateAsync(CancellationToken cancellationToken = default)
    {
        Estimate = null;
        EstimateError = null;
        if (Provider == null)
        {
            EstimateError = "There is no AI provider of chat configured (Tools › Manage AI providers).";
            View.RefreshView();
            return;
        }

        try
        {
            var estimate = await AiDocumentProcessor.EstimateAsync(Service, Provider, Request(), Store.Logger, cancellationToken);
            if (estimate.NotAuthorized)
            {
                ShowResultError(estimate);
                return;
            }
            if (estimate.IsValid)
                Estimate = estimate.Entity;
            else
                EstimateError = estimate.ErrorMessage;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            EstimateError = ex.Message;
        }
        View.RefreshView();
    }

    /// <summary>Starts the summary in the Store: false (and why, in the view) if another run of KNote Atlas is going on.</summary>
    public async Task<bool> GenerateAsync()
    {
        if (Provider == null)
            return false;

        var index = await Service.SemanticSearch.GetIndexStatusAsync();
        var hasIndex = index.IsValid && index.Entity?.Status != EnumSemanticIndexStatus.None;
        if (!Store.DocumentProcessing.Start(Service, ServiceRef.Name, Provider, Request(), hasIndex))
        {
            View.ShowInfo(SummaryTexts.OtherRunning);
            return false;
        }

        RememberLanguage();
        View.RefreshView();
        return true;
    }

    public void Cancel()
    {
        if (IsRunning)
            Store.DocumentProcessing.Cancel();
    }

    public void RequestTranscription() => TranscribeRequested?.Invoke(this, EventArgs.Empty);

    /// <summary>The note with the summary, in the note editor.</summary>
    public async Task OpenNoteAsync()
    {
        if (Progress?.NoteId is not Guid noteId)
            return;
        var editor = new NoteEditorCtrl(Store);
        if (await editor.LoadModelById(Service, noteId))
            editor.Run();
        else
            editor.Dispose();
    }

    // The language of this repository, proposed next time (D14). Store.SaveConfig is the app's: the tests replace it.
    internal Action SaveSettings { get; set; }

    private void RememberLanguage()
    {
        var languages = Store.State.Session.SummaryLanguages;
        var repositoryId = ServiceRef?.RepositoryId ?? Guid.Empty;
        languages.RemoveAll(l => l.RepositoryId == repositoryId);
        languages.Add(new RepositoryLanguageState { RepositoryId = repositoryId, Language = Language?.Trim() ?? "" });
        SaveSettings?.Invoke();
    }

    private void OnProgressChanged(DocumentProcessingProgressChanged change)
    {
        if (PreconditionsMet && ControllerState != EControllerState.Finalized && Document != null)
            View.RefreshProgress();
    }

    // What LoadAsync prepared, in the view before it is shown (it didn't exist yet).
    protected override Result<EControllerResult> OnInitialized()
    {
        var result = base.OnInitialized();
        View.RefreshView();
        return result;
    }

    protected override Result<EControllerResult> OnFinalized()
    {
        Store.Events.Unsubscribe<DocumentProcessingProgressChanged>(OnProgressChanged);
        return base.OnFinalized();
    }
}

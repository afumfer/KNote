using KNote.Ai;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

/// <summary>
/// «Summarize document» (KNote Atlas, stage 2, CU-43, 2.1a2) and «Translate document» (2.1b2): the summary or the translation of a long
/// document of a note in a new note, with the language, the length (of a summary) and the model; «Estimate» before (pages, calls, time
/// and the scanned pages without text) and the run in the Store (Store.DocumentProcessing), which goes on if the window is closed and is
/// shown again when it opens. A translation not finished of the document can go on («Continue»). At the end, the new note opens in the
/// note editor. The AI and where the result is kept are KNote.Ai and the Service (AiDocumentProcessor).
/// </summary>
[KntAuthorize(EnumRoles.Staff)]
public class LongDocumentSummaryCtrl : CtrlViewBase<IViewLongDocumentSummary>
{
    public LongDocumentSummaryCtrl(Store store) : base(store)
    {
        ControllerName = SummaryTexts.Title;
        Store.Events.Subscribe<DocumentProcessingProgressChanged>(OnProgressChanged);
        Store.Events.Subscribe<SemanticIndexProgressChanged>(OnOtherJobChanged);
        Store.Events.Subscribe<ResourceProcessingProgressChanged>(OnOtherJobChanged);
        SaveSettings = () => Store.SaveConfig();
    }

    public ServiceRef ServiceRef { get; private set; }

    public IKntService Service => ServiceRef?.Service;

    protected override IKntService AuthorizationResource => Service;

    // Summarize or translate.
    public EnumDocumentProcessKind Kind { get; private set; } = EnumDocumentProcessKind.Summarize;

    public bool IsTranslation => Kind == EnumDocumentProcessKind.Translate;

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

    // The translations of the document (a translation), the last one first, and the last one not finished, if any.
    public List<DocumentTranslationDto> Translations { get; private set; } = new();

    public DocumentTranslationDto Unfinished => IsTranslation ? Translations.FirstOrDefault(t => !t.Finished) : null;

    // The run of the Store, if it is the summary or the translation (as this window) of this document (running or the last one).
    public DocumentProcessProgressDto Progress =>
        Store.DocumentProcessing.IsOf(Service) && Store.DocumentProcessing.Progress is { } progress
            && progress.ResourceId == Document?.ResourceId && progress.Kind == Kind ? progress : null;

    public bool IsRunning => Store.DocumentProcessing.IsRunning && Progress != null;

    // Another run of KNote Atlas (the index, the resources, or the summary of another document): this one waits.
    public bool OtherRunning => Store.AtlasJobRunning && !IsRunning;

    // The index is being updated (also with the note of the summary or the translation just done).
    public bool IndexRunning => Store.SemanticIndex.IsRunning;

    // «Transcribe them first» (the window that has the transcription of the document opens it).
    public event EventHandler TranscribeRequested;

    protected override IViewLongDocumentSummary CreateView() =>
        Store.FactoryViews.Registry.Resolve<LongDocumentSummaryCtrl, IViewLongDocumentSummary>(this);

    /// <summary>The document to summarize or translate, with the model, the language and the length proposed.</summary>
    public async Task LoadAsync(ServiceRef serviceRef, ResourceDto document, EnumDocumentProcessKind kind = EnumDocumentProcessKind.Summarize)
    {
        ServiceRef = serviceRef;
        Document = document;
        Kind = kind;
        ControllerName = SummaryTexts.TitleOf(kind);
        Providers = Store.Settings.Ai.Providers.Where(p => p.IsChat).ToList();

        var (provider, _) = await Store.GetPhotoDescriberAsync(Service);
        Provider = provider ?? Providers.FirstOrDefault();
        Language = Languages.FirstOrDefault(l => l.RepositoryId == ServiceRef?.RepositoryId)?.Language ?? "";
        await LoadTranslationsAsync();
    }

    public DocumentProcessRequestDto Request(Guid? continueNoteId = null) => new()
    {
        ResourceId = Document.ResourceId,
        Kind = Kind,
        ProviderAlias = Provider?.Alias,
        Language = Language?.Trim(),
        Length = Length,
        ContinueNoteId = continueNoteId
    };

    /// <summary>What the summary or the translation would take; the view shows it.</summary>
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

    /// <summary>
    /// Starts the summary or the translation in the Store: false (and why, in the view) if another run of KNote Atlas is going on. A
    /// translation into the language of one not finished asks first whether to continue that one.
    /// </summary>
    public async Task<bool> GenerateAsync()
    {
        if (Provider == null)
            return false;
        if (IsTranslation && string.IsNullOrWhiteSpace(Language))
        {
            View.ShowInfo(KNote.Service.ServicesCommands.KntSummaryTexts.LanguageRequired);
            return false;
        }

        Guid? continueNoteId = null;
        if (Unfinished is { } unfinished && string.Equals(unfinished.Language?.Trim(), Language?.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            var answer = View.ShowInfo(SummaryTexts.ContinueInstead(unfinished), SummaryTexts.TranslateTitle, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (answer == DialogResult.Cancel)
                return false;
            if (answer == DialogResult.Yes)
                continueNoteId = unfinished.NoteId;
        }
        return await StartAsync(Request(continueNoteId));
    }

    /// <summary>Goes on with the translation not finished of the document, in its language.</summary>
    public async Task<bool> ContinueAsync()
    {
        if (Provider == null || Unfinished is not { } unfinished)
            return false;
        Language = unfinished.Language;
        return await StartAsync(Request(unfinished.NoteId));
    }

    private async Task<bool> StartAsync(DocumentProcessRequestDto request)
    {
        var index = await Service.SemanticSearch.GetIndexStatusAsync();
        var hasIndex = index.IsValid && index.Entity?.Status != EnumSemanticIndexStatus.None;
        if (!Store.DocumentProcessing.Start(Service, ServiceRef.Name, Provider, request, hasIndex))
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

    /// <summary>The note with the summary or the translation, in the note editor.</summary>
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

    // The language of this repository, proposed next time (D14), one for the summaries and one for the translations. Store.SaveConfig
    // is the app's: the tests replace it.
    internal Action SaveSettings { get; set; }

    private List<RepositoryLanguageState> Languages =>
        IsTranslation ? Store.State.Session.TranslationLanguages : Store.State.Session.SummaryLanguages;

    private void RememberLanguage()
    {
        var languages = Languages;
        var repositoryId = ServiceRef?.RepositoryId ?? Guid.Empty;
        languages.RemoveAll(l => l.RepositoryId == repositoryId);
        languages.Add(new RepositoryLanguageState { RepositoryId = repositoryId, Language = Language?.Trim() ?? "" });
        SaveSettings?.Invoke();
    }

    // The translations of the document: the one not finished can go on.
    private async Task LoadTranslationsAsync()
    {
        if (!IsTranslation || Service == null)
            return;
        var translations = await Service.Summaries.GetDocumentTranslationsAsync(Document.ResourceId);
        Translations = translations.IsValid ? translations.Entity ?? new() : new();
    }

    private async void OnProgressChanged(DocumentProcessingProgressChanged change)
    {
        if (!PreconditionsMet || ControllerState == EControllerState.Finalized || Document == null)
            return;
        View.RefreshProgress();

        // A translation that stopped (or ended): how far it went, for «Continue».
        if (IsTranslation && Progress?.IsFinished == true && !IsRunning)
        {
            await LoadTranslationsAsync();
            if (ControllerState != EControllerState.Finalized)
                View.RefreshView();
        }
    }

    // Another run of KNote Atlas that starts or ends: «Generate» and «Continue» wait for it. After a summary or a translation, the job
    // updates the index with the new note: without this, the window kept them disabled until another control refreshed it.
    private void OnOtherJobChanged<T>(T change)
    {
        if (!PreconditionsMet || ControllerState == EControllerState.Finalized || Document == null)
            return;
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
        Store.Events.Unsubscribe<SemanticIndexProgressChanged>(OnOtherJobChanged);
        Store.Events.Unsubscribe<ResourceProcessingProgressChanged>(OnOtherJobChanged);
        return base.OnFinalized();
    }
}

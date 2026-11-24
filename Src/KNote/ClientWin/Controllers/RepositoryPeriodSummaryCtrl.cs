using KNote.Ai;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;
using KNote.Service.Core;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace KNote.ClientWin.Controllers;

/// <summary>
/// Section «Period summary» of the KNote Atlas window (AtlasCtrl; KNote Atlas, stage 2, 2.3b, CU-15, D19): for now only the fields
/// of the selection of a summary (the period, with «From the beginning»; the folder and its subfolders; the keywords; «Talks about…»,
/// only if the repository can search by meaning here), its counts (Service.Summaries.PrepareSelectionAsync, prepared again at each
/// change: the last one wins) and «Review selection» (ReviewSelectionCtrl, modal; the selection is prepared again with what it
/// kept). The doubtful notes are asked to the AI on demand (2.4, C1-C6: ClassifyAsync, a batch at a time with AiNoteClassifier and the
/// repository's recommended model, cancelled between batches; what was asked stays). Below (2.5b), the summary: subject, words, language
/// (the last one of the repository, AppUserState.Session.PeriodSummaryLanguages), model and the attributes of «By concept» (the last ones
/// of the repository, AppUserState.Session.PeriodSummaryConceptAttributes); «Estimate» (AiDocumentProcessor), «Generate»
/// (the run of the Store, Store.DocumentProcessing, as the extended summary of a document: never with another run of KNote Atlas, and it
/// goes on if the window is closed), its progress, «Cancel» and «Open the note». Included in the KNote Atlas window (no
/// [KntAuthorize]: it decides, by the role of the command, Staff).
/// </summary>
public class RepositoryPeriodSummaryCtrl : CtrlViewEmbeddableBase<IViewPeriodSummary>
{
    #region Private fields

    private int _version;
    private CancellationTokenSource _classifyCancellation;

    #endregion

    #region Constructor

    public RepositoryPeriodSummaryCtrl(Store store) : base(store)
    {
        ControllerName = "Period summary";
        SaveSettings = () => Store.SaveConfig();
        Store.Events.Subscribe<DocumentProcessingProgressChanged>(OnProgressChanged);
        Store.Events.Subscribe<SemanticIndexProgressChanged>(OnOtherJobChanged);
        Store.Events.Subscribe<ResourceProcessingProgressChanged>(OnOtherJobChanged);
    }

    #endregion

    #region Properties

    public IKntService Service { get; private set; }

    public DateTime From { get; private set; } = DateTime.Today.AddDays(-6);

    public DateTime To { get; private set; } = DateTime.Today;

    public bool FromTheBeginning { get; private set; }

    public FolderInfoDto Folder { get; private set; }

    public string FolderText { get; private set; } = AllFolders;

    public string Keywords { get; private set; } = "";

    public string MeaningText { get; private set; } = "";

    // «Talks about…»: only if the repository has an index usable here.
    public bool MeaningAvailable { get; private set; }

    // The selection of the fields as they are; null while it is prepared or if it failed (LoadFailed).
    public NoteSelectionResultDto Selection { get; private set; }

    public bool LoadFailed { get; private set; }

    public string CountText => Selection != null ? NoteRelevanceRules.CountText(Selection)
        : LoadFailed ? "The selection could not be prepared." : "Preparing the selection…";

    public bool CanReview => Selection?.Notes.Count > 0 && !IsClassifying;

    // «Note relevance», where the corrections and the answers of the AI are kept: the link of the section (empty until it exists).
    public string RelevanceNoteText => Selection?.RelevanceNote is { } note ? $"Note relevance (#{note.NoteNumber})" : "";

    // 2.4: the doubtful notes that can be asked to the AI, and the run that asks them.
    public bool CanClassify => !IsClassifying && NoteClassificationRules.PendingCount(Selection) > 0;

    public bool IsClassifying { get; private set; }

    public string PendingText => IsClassifying ? ClassifyProgressText : NoteClassificationRules.PendingText(Selection);

    public string ClassifyProgressText { get; private set; } = "";

    public int ClassifyDone { get; private set; }

    public int ClassifyTotal { get; private set; }

    // The model of the doubtful notes (the repository's recommended LLM) and its client (replaced by the tests).
    internal Func<IKntService, Task<(AiProviderRef Provider, ResourceDescriberDto Info)>> ChooseModel { get; set; }

    internal Func<AiProviderRef, IChatClient> CreateChatClient { get; set; }

    public NoteSelectionRequestDto Request => new()
    {
        From = FromTheBeginning ? NoteSelectionRequestDto.Beginning : From,
        To = To,
        FolderId = Folder?.FolderId,
        Keywords = string.IsNullOrWhiteSpace(Keywords) ? null : Keywords.Trim(),
        MeaningText = MeaningAvailable && !string.IsNullOrWhiteSpace(MeaningText) ? MeaningText.Trim() : null
    };

    // Chooses a folder of the repository (the folder selector; replaced by the tests).
    internal Func<FolderInfoDto> ChooseFolder { get; set; }

    // Runs «Review selection» and says whether it kept something (replaced by the tests).
    internal Func<ReviewSelectionCtrl, bool> RunReview { get; set; }

    private const string AllFolders = "All folders";

    // 2.5b: the summary.
    public List<AiProviderRef> Providers { get; private set; } = new();

    // The fields of the summary, kept for the user and the repository as they change (Remember).
    private AiProviderRef _provider;
    private string _subject = "";
    private int _words = PeriodSummaryRules.DefaultWords;
    private string _language = "";
    private List<Guid> _conceptAttributeIds = new();

    // The model chosen; null: the repository's recommended one (resolved when it is used, as in the web).
    public AiProviderRef Provider { get => _provider; set { _provider = value; Remember(); } }

    public const string RecommendedModel = "The repository's recommended model";

    public string Subject { get => _subject; set { _subject = value ?? ""; Remember(); } }

    public int Words { get => _words; set { _words = value; Remember(); } }

    public string Language { get => _language; set { _language = value ?? ""; Remember(); } }

    // The attributes of the index «By concept» that can be chosen (PeriodConceptRules.Choices) and the ones chosen.
    public List<PeriodConceptRules.Choice> ConceptChoices { get; private set; } = new();

    public List<Guid> ConceptAttributeIds { get => _conceptAttributeIds; set { _conceptAttributeIds = value ?? new(); Remember(); } }

    public string DefaultSubject => PeriodSummaryRules.DefaultSubject(Request);

    public DocumentProcessEstimateDto Estimate { get; private set; }

    public string EstimateError { get; private set; }

    public string EstimateText => EstimateError ?? (Estimate == null ? "" : string.Join("\r\n", SummaryTexts.EstimateLines(Estimate, _usedProvider?.Provider)));

    // The run of the Store, if it is a period summary of this repository (running or the last one).
    public DocumentProcessProgressDto Progress =>
        Service != null && Store.DocumentProcessing.IsOf(Service) && Store.DocumentProcessing.Progress is { Kind: EnumDocumentProcessKind.PeriodSummary } progress ? progress : null;

    public bool IsRunning => Store.DocumentProcessing.IsRunning && Progress != null;

    // Another run of KNote Atlas (the index, the resources, or a summary of a document or of another repository): this one waits.
    public bool OtherRunning => Store.AtlasJobRunning && !IsRunning;

    public string ProgressText => Progress == null ? "" : SummaryTexts.Progress(Progress);

    public bool CanGenerate => !IsRunning && !OtherRunning && !IsClassifying && Providers.Count > 0 && Selection?.Notes.Any(n => n.IsIncluded) == true;

    public bool CanOpenNote => !IsRunning && SummaryTexts.HasNote(Progress);

    // Writes the fields kept (Store.State) to disk: at «Estimate», «Generate» and when the section is closed (the app also writes them
    // when it is closed). Store.SaveConfig is the app's: the tests replace it.
    internal Action SaveSettings { get; set; }

    #endregion

    #region Controller implementation

    protected override IViewPeriodSummary CreateView()
    {
        return Store.FactoryViews.Registry.Resolve<RepositoryPeriodSummaryCtrl, IViewPeriodSummary>(this);
    }

    #endregion

    #region Public methods

    // The repository of the window, with the fields as the user left them in it.
    public async Task LoadAsync(IKntService service)
    {
        var changed = service != Service;
        if (changed && Service != null)
            SaveSettings?.Invoke();
        Service = service;
        if (changed)
        {
            Providers = Store.Settings.Ai.Providers.Where(p => p.IsChat).ToList();
            Estimate = null;
            EstimateError = null;
            ConceptChoices = await ConceptChoicesAsync(service);
            await ApplyParametersAsync(service);
        }
        MeaningAvailable = await Store.CanSearchByMeaningAsync(service);
        await PrepareAsync();
    }

    // The model of the summary: the one chosen or the repository's recommended one (else the first chat provider).
    private async Task<AiProviderRef> ProviderToUseAsync()
    {
        if (Provider != null)
            return Provider;
        var (provider, _) = await (ChooseModel ?? Store.GetPhotoDescriberAsync)(Service);
        return provider ?? Providers.FirstOrDefault();
    }

    private AiProviderRef _usedProvider;

    public DocumentProcessRequestDto SummaryRequest() => new()
    {
        Kind = EnumDocumentProcessKind.PeriodSummary,
        ProviderAlias = Provider?.Alias,
        Language = Language?.Trim(),
        Period = new PeriodSummaryRequestDto
        {
            Selection = Request,
            Subject = string.IsNullOrWhiteSpace(Subject) ? null : Subject.Trim(),
            Words = PeriodSummaryRules.ClampWords(Words),
            ConceptAttributeIds = PeriodConceptRules.Chosen(ConceptAttributeIds, ConceptChoices)
        }
    };

    /// <summary>What the summary would take; the view shows it.</summary>
    public async Task EstimateAsync()
    {
        Estimate = null;
        EstimateError = null;
        SaveSettings?.Invoke();
        _usedProvider = Providers.Count == 0 ? null : await ProviderToUseAsync();
        if (_usedProvider == null)
            EstimateError = "There is no AI provider of chat configured (Tools › Manage AI providers).";
        else
        {
            try
            {
                var estimate = await AiDocumentProcessor.EstimateAsync(Service, _usedProvider, SummaryRequest(), Store.Logger);
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
            catch (Exception ex)
            {
                EstimateError = ex.Message;
            }
        }
        View.RefreshView();
    }

    /// <summary>Starts the summary in the Store: false (and why) if another run of KNote Atlas is going on.</summary>
    public async Task<bool> GenerateAsync()
    {
        if (!CanGenerate)
            return false;
        var provider = await ProviderToUseAsync();
        if (provider == null)
            return false;
        var index = await Service.SemanticSearch.GetIndexStatusAsync();
        var hasIndex = index.IsValid && index.Entity?.Status != EnumSemanticIndexStatus.None;
        var repositoryName = Store.GetServiceRef(Service.IdServiceRef)?.Name;
        if (!Store.DocumentProcessing.Start(Service, repositoryName, provider, SummaryRequest(), hasIndex))
        {
            View.ShowInfo(SummaryTexts.OtherRunning);
            return false;
        }
        SaveSettings?.Invoke();
        View.RefreshView();
        return true;
    }

    public void CancelSummary()
    {
        if (IsRunning)
            Store.DocumentProcessing.Cancel();
    }

    /// <summary>The note with the summary, in the note editor.</summary>
    public async Task OpenSummaryNoteAsync()
    {
        if (Progress?.NoteId is not Guid noteId)
            return;
        // Without refreshing the view on load, as the rest of the app: Run() already does it, and two
        // refreshes at once showed every relation of the note twice.
        var editor = new NoteEditorCtrl(Store);
        if (await editor.LoadModelById(Service, noteId, false))
            editor.Run();
        else
            editor.Dispose();
    }

    public override void Dispose()
    {
        Store.Events.Unsubscribe<DocumentProcessingProgressChanged>(OnProgressChanged);
        Store.Events.Unsubscribe<SemanticIndexProgressChanged>(OnOtherJobChanged);
        Store.Events.Unsubscribe<ResourceProcessingProgressChanged>(OnOtherJobChanged);
        if (Service != null)
            SaveSettings?.Invoke();
        base.Dispose();
    }

    public async Task SetPeriodAsync(DateTime from, DateTime to, bool fromTheBeginning)
    {
        if (From == from.Date && To == to.Date && FromTheBeginning == fromTheBeginning)
            return;
        From = from.Date;
        To = to.Date;
        FromTheBeginning = fromTheBeginning;
        Remember();
        await PrepareAsync();
    }

    public async Task SetKeywordsAsync(string keywords)
    {
        if ((keywords ?? "") == Keywords)
            return;
        Keywords = keywords ?? "";
        Remember();
        await PrepareAsync();
    }

    public async Task SetMeaningTextAsync(string meaningText)
    {
        if ((meaningText ?? "") == MeaningText)
            return;
        MeaningText = meaningText ?? "";
        Remember();
        await PrepareAsync();
    }

    public async Task<bool> ChooseFolderAsync()
    {
        if (Service == null)
            return false;
        var folder = (ChooseFolder ?? ChooseWithSelector)();
        if (folder == null)
            return false;
        Folder = folder;
        var path = await Service.Folders.GetPathAsync(folder.FolderId);
        FolderText = path.IsValid && !string.IsNullOrEmpty(path.Entity) ? path.Entity : folder.Name;
        Remember();
        await PrepareAsync();
        return true;
    }

    public async Task ClearFolderAsync()
    {
        if (Folder == null)
            return;
        Folder = null;
        FolderText = AllFolders;
        Remember();
        await PrepareAsync();
    }

    // «Review selection»: what it keeps prepares the selection again.
    public async Task<bool> ReviewAsync()
    {
        if (!CanReview)
            return false;
        var review = new ReviewSelectionCtrl(Store, Service, Selection);
        var saved = (RunReview ?? RunModal)(review);
        if (saved)
            await PrepareAsync();
        return saved;
    }

    /// <summary>
    /// «Classify doubtful notes with AI…»: after saying what it does (how many notes and calls, the model and, if it is paid, that
    /// the beginning of the notes is sent to its provider), asks the batches one by one; the selection is prepared again after.
    /// </summary>
    public async Task<bool> ClassifyAsync()
    {
        var toClassify = NoteClassificationRules.ToClassify(Selection);
        if (IsClassifying || toClassify.Count == 0 || Service == null)
            return false;

        var service = Service;
        var (provider, info) = await (ChooseModel ?? Store.GetPhotoDescriberAsync)(service);
        if (provider == null || !info.CanSummarize)
        {
            View.ShowInfo(info?.Message ?? "There is no AI model to classify the notes.", ControllerName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }
        if (View.ShowInfo(NoteClassificationRules.Confirmation(toClassify.Count, info.Model, info.IsPaid), "Classify doubtful notes",
            MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            return false;

        var batches = NoteClassificationRules.Batches(toClassify.Select(n => n.NoteId));
        var chatClient = (CreateChatClient ?? (p => AiChatClientFactory.Create(p)))(provider);
        var answered = 0;
        var failed = 0;
        string firstError = null;
        IsClassifying = true;
        ClassifyTotal = batches.Count;
        ClassifyDone = 0;
        _classifyCancellation = new CancellationTokenSource();
        try
        {
            foreach (var batch in batches)
            {
                if (_classifyCancellation.IsCancellationRequested)
                    break;
                ClassifyProgressText = NoteClassificationRules.Progress(ClassifyDone + 1, ClassifyTotal);
                View.RefreshView();
                try
                {
                    var result = await AiNoteClassifier.ClassifyAsync(service, chatClient, provider, batch, Store.Logger, _classifyCancellation.Token);
                    if (result.IsValid)
                        answered += result.Entity.Count;
                    else
                    {
                        failed++;
                        firstError ??= result.ErrorMessage;
                        if (result.NotAuthorized)
                            break;
                    }
                }
                catch (OperationCanceledException) when (_classifyCancellation.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    failed++;
                    firstError ??= ex.Message;
                }
                ClassifyDone++;
            }
        }
        finally
        {
            IsClassifying = false;
            _classifyCancellation.Dispose();
            _classifyCancellation = null;
        }

        View.ShowInfo(NoteClassificationRules.Done(answered, toClassify.Count, failed) + (firstError == null ? "" : $"\n\n{firstError}"),
            ControllerName, MessageBoxButtons.OK, failed == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        await PrepareAsync();
        return failed == 0;
    }

    public void CancelClassification() => _classifyCancellation?.Cancel();

    // The note «Note relevance» in the note editor of always (a Staff sees it; saving it, the Service refuses).
    public async Task<bool> OpenRelevanceNoteAsync()
    {
        if (Selection?.RelevanceNote is not { } relevance || Service == null)
            return false;
        var noteEditorCtrl = new NoteEditorCtrl(Store);
        if (!await noteEditorCtrl.LoadModelById(Service, relevance.NoteId, false))
        {
            noteEditorCtrl.Dispose();
            return false;
        }
        noteEditorCtrl.Run();
        return true;
    }

    // The selection of the fields as they are now; one left behind by a newer one is not shown.
    public async Task PrepareAsync()
    {
        if (Service == null)
            return;

        var version = ++_version;
        var service = Service;
        Selection = null;
        LoadFailed = false;
        View.RefreshView();

        try
        {
            var result = await service.Summaries.PrepareSelectionAsync(Request);
            if (version != _version || service != Service)
                return;
            if (result.IsValid)
                Selection = result.Entity;
            else
            {
                LoadFailed = true;
                ShowResultError(result);
            }
        }
        catch (Exception ex)
        {
            if (version != _version)
                return;
            LoadFailed = true;
            View.ShowInfo(ex.Message);
        }
        View.RefreshView();
    }

    #endregion

    #region Private methods

    private Guid RepositoryId => Service?.RepositoryRef?.RepositoryId ?? Guid.Empty;

    // The attributes that can group the notes; if they cannot be read, none (the summary goes on without «By concept»).
    private async Task<List<PeriodConceptRules.Choice>> ConceptChoicesAsync(IKntService service)
    {
        try
        {
            var attributes = await service.KAttributes.GetAllAsync();
            return attributes.IsValid ? PeriodConceptRules.Choices(attributes.Entity) : new();
        }
        catch (Exception ex)
        {
            Store.Logger?.LogWarning(ex, "KNote Atlas: the attributes of the repository could not be read.");
            return new();
        }
    }

    // While the fields of a repository are being applied, nothing is kept (they would be kept half applied).
    private bool _applying;

    private RepositoryPeriodSummaryState SavedState() => Store.State.Session.PeriodSummaries
        .FirstOrDefault(s => s.RepositoryId == RepositoryId && string.Equals(s.UserName ?? "", Store.AppUserName ?? "", StringComparison.OrdinalIgnoreCase));

    // The fields as they are now, kept in Store.State for the user and the repository (written to disk by SaveSettings).
    private void Remember()
    {
        if (_applying || Service == null)
            return;
        var state = SavedState();
        if (state == null)
            Store.State.Session.PeriodSummaries.Add(state = new RepositoryPeriodSummaryState { RepositoryId = RepositoryId, UserName = Store.AppUserName });
        state.Parameters = new PeriodSummaryParametersDto
        {
            From = From, To = To, FromTheBeginning = FromTheBeginning, FolderId = Folder?.FolderId, Keywords = Keywords, MeaningText = MeaningText,
            Subject = Subject, Words = Words, Language = Language, ProviderAlias = Provider?.Alias,
            ConceptAttributeIds = PeriodConceptRules.Chosen(ConceptAttributeIds, ConceptChoices)
        };
    }

    // The fields as the user left them in this repository (the last 7 days and the rest empty the first time; the language of the
    // version before, if it was kept). A folder or a model that no longer exists is left out.
    private async Task ApplyParametersAsync(IKntService service)
    {
        var parameters = SavedState()?.Parameters;
        _applying = true;
        try
        {
            From = (parameters?.From ?? DateTime.Today.AddDays(-6)).Date;
            To = (parameters?.To ?? DateTime.Today).Date;
            FromTheBeginning = parameters?.FromTheBeginning ?? false;
            Keywords = parameters?.Keywords ?? "";
            MeaningText = parameters?.MeaningText ?? "";
            Subject = parameters?.Subject ?? "";
            Words = PeriodSummaryRules.ClampWords(parameters?.Words ?? PeriodSummaryRules.DefaultWords);
            Language = parameters?.Language
                ?? Store.State.Session.PeriodSummaryLanguages.FirstOrDefault(l => l.RepositoryId == RepositoryId)?.Language ?? "";
            Provider = string.IsNullOrEmpty(parameters?.ProviderAlias) ? null : Providers.FirstOrDefault(p => p.Alias == parameters.ProviderAlias);
            ConceptAttributeIds = PeriodConceptRules.Chosen(parameters?.ConceptAttributeIds, ConceptChoices);
            Folder = null;
            FolderText = AllFolders;
            if (parameters?.FolderId is Guid folderId)
            {
                try
                {
                    var folder = await service.Folders.GetAsync(folderId);
                    if (folder.IsValid && folder.Entity != null)
                    {
                        Folder = folder.Entity;
                        var path = await service.Folders.GetPathAsync(folderId);
                        FolderText = path.IsValid && !string.IsNullOrEmpty(path.Entity) ? path.Entity : folder.Entity.Name;
                    }
                }
                catch (Exception ex)
                {
                    Store.Logger?.LogWarning(ex, "KNote Atlas: the folder of the period summary could not be read.");
                }
            }
        }
        finally
        {
            _applying = false;
        }
    }

    // The progress of the summary; when it ends, the selection again (the answers of the AI about the doubtful notes).
    private async void OnProgressChanged(DocumentProcessingProgressChanged change)
    {
        if (Service == null || ControllerState == EControllerState.Finalized)
            return;
        View.RefreshView();
        if (Progress is { IsFinished: true } && !IsRunning)
            await PrepareAsync();
    }

    // Another run of KNote Atlas that starts or ends: «Generate» waits for it.
    private void OnOtherJobChanged<T>(T change)
    {
        if (Service != null && ControllerState != EControllerState.Finalized)
            View.RefreshView();
    }

    private FolderInfoDto ChooseWithSelector()
    {
        var folderSelector = new FoldersSelectorCtrl(Store);
        folderSelector.ServicesRef = new List<ServiceRef> { Store.GetServiceRef(Service.IdServiceRef) };
        folderSelector.SelectedFolderId = Folder?.FolderId;
        var res = folderSelector.RunModal();
        return res.Entity == EControllerResult.Executed ? folderSelector.SelectedEntity?.FolderInfo : null;
    }

    private static bool RunModal(ReviewSelectionCtrl review)
    {
        review.RunModal();
        return review.Saved;
    }

    #endregion
}

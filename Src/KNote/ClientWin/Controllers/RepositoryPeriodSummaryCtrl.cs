using KNote.Ai;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;
using KNote.Service.Core;
using Microsoft.Extensions.AI;

namespace KNote.ClientWin.Controllers;

/// <summary>
/// Section «Period summary» of the KNote Atlas window (AtlasCtrl; KNote Atlas, stage 2, 2.3b, CU-15, D19): for now only the fields
/// of the selection of a summary (the period, with «From the beginning»; the folder and its subfolders; the keywords; «Talks about…»,
/// only if the repository can search by meaning here), its counts (Service.Summaries.PrepareSelectionAsync, prepared again at each
/// change: the last one wins) and «Review selection» (ReviewSelectionCtrl, modal; the selection is prepared again with what it
/// kept). The doubtful notes are asked to the AI on demand (2.4, C1-C6: ClassifyAsync, a batch at a time with AiNoteClassifier and the
/// repository's recommended model, cancelled between batches; what was asked stays). 2.5 adds the subject, the words, the model, the language, «Estimate» and «Generate». Included in the KNote Atlas window (no
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

    #endregion

    #region Controller implementation

    protected override IViewPeriodSummary CreateView()
    {
        return Store.FactoryViews.Registry.Resolve<RepositoryPeriodSummaryCtrl, IViewPeriodSummary>(this);
    }

    #endregion

    #region Public methods

    // The repository of the window: the folder chosen in another one does not go on.
    public async Task LoadAsync(IKntService service)
    {
        if (service != Service)
        {
            Folder = null;
            FolderText = AllFolders;
        }
        Service = service;
        MeaningAvailable = await Store.CanSearchByMeaningAsync(service);
        await PrepareAsync();
    }

    public async Task SetPeriodAsync(DateTime from, DateTime to, bool fromTheBeginning)
    {
        if (From == from.Date && To == to.Date && FromTheBeginning == fromTheBeginning)
            return;
        From = from.Date;
        To = to.Date;
        FromTheBeginning = fromTheBeginning;
        await PrepareAsync();
    }

    public async Task SetKeywordsAsync(string keywords)
    {
        if ((keywords ?? "") == Keywords)
            return;
        Keywords = keywords ?? "";
        await PrepareAsync();
    }

    public async Task SetMeaningTextAsync(string meaningText)
    {
        if ((meaningText ?? "") == MeaningText)
            return;
        MeaningText = meaningText ?? "";
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
        await PrepareAsync();
        return true;
    }

    public async Task ClearFolderAsync()
    {
        if (Folder == null)
            return;
        Folder = null;
        FolderText = AllFolders;
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

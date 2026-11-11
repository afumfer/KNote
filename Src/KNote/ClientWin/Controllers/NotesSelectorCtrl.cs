using KNote.Ai;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Reports;
using KNote.Service.Core;
using KNote.Service.ServicesCommands;

namespace KNote.ClientWin.Controllers;

public class NotesSelectorCtrl : CtrlSyncableSelectorBase<IViewNotesSelector, NoteMinimalDto>
{
    #region Properties

    public FolderInfoDto Folder
    {
        get;
        private set;
    }

    public NotesFilterDto NotesFilter
    {
        get;
        private set;
    }

    public NotesSearchDto NotesSearch
    {
        get;
        private set;
    }

    public string HiddenColumns
    {
        get; set;
    }

    // KNote Atlas: the list was found by meaning (its notes are NoteSearchHitDto, with Similarity and Fragment), and
    // the notes of the index not up to date after that search (D8), for the notice with «Update index».
    public bool ByMeaning
    {
        get; private set;
    }

    public int PendingNotes
    {
        get; private set;
    }

    // KNote Atlas, stage 4 (D14): by meaning, the photos or the documents found (All: the notes). The list has a row for each
    // one (ResourceSearchHitDto, a note found with its resource).
    public EnumResourceKind ResourceKind
    {
        get; private set;
    }

    public bool ByResources => ByMeaning && ResourceKind != EnumResourceKind.All;

    // KNote Atlas 4.6b: of the PendingNotes, the ones that wait for a full indexing and the ones whose documents have no file here.
    public int NotesForFullRun { get; private set; }

    public int NotesWithoutFiles { get; private set; }

    // «Update index»: the full indexing of the repository (as «Update now» of KNote Atlas, Admin), in the background; when it ends,
    // the same search again, if the list is still the one it was started from.
    public bool CanRunIndex => Service != null && Store.Security.IsAuthorized(typeof(KntSemanticSearchRunIndexAsyncCommand), Service);

    public bool IndexRunning => Store.SemanticIndex.IsRunning && Store.SemanticIndex.IsOf(Service);

    // Another run of KNote Atlas (an index of another repository, the resources): one at a time.
    public bool OtherAtlasJobRunning => Store.AtlasJobRunning && !IndexRunning;

    private Func<CancellationToken, Task<bool>> _searchAfterIndex;

    public IEnumerable<ResourceSearchHitDto> ResourceHits => (ListEntities ?? new()).OfType<ResourceSearchHitDto>();

    // The thumbnails already made, by resource and size.
    private readonly Dictionary<(Guid Resource, int Side), byte[]> _thumbnails = new();

    // The search by meaning of the list, to do it again («Update index»).
    private Func<CancellationToken, Task<bool>> _searchByMeaningAgain;

    public string ViewTitle
    {
        get; set;
    }

    // Opt-in: shows a text box above the list that filters the already-loaded ListEntities in
    // memory (by #number, Topic or Tags), plus an "Undo" button that clears it back to the
    // original collection set by Load*Entities. Off by default so existing callers (the shared
    // Store.NotesSelector used by "Insert template text"/"Insert code snippet"/prompt/assistant
    // catalogs, and KNoteManagementCtrl's embedded main panel) are unaffected.
    public bool EnableTextFilter
    {
        get; set;
    }

    #endregion

    #region Constructor

    public NotesSelectorCtrl(Store store) : base(store)
    {
        ControllerName = "Notes selector";
        HiddenColumns = "";
        Store.Events.Subscribe<SemanticIndexProgressChanged>(OnIndexProgressChanged);
    }

    public override void Dispose()
    {
        Store.Events.Unsubscribe<SemanticIndexProgressChanged>(OnIndexProgressChanged);
        base.Dispose();
    }

    #endregion

    #region ISelectorView implementation

    protected override IViewNotesSelector CreateView()
    {
        return Store.FactoryViews.Registry.Resolve<NotesSelectorCtrl, IViewNotesSelector>(this);
    }

    #endregion 

    #region Controller virtual / abstract public members

    public override async Task<bool> LoadEntities(IKntService service, bool refreshView = true)
    {
        return await LoadEntities(service, null, refreshView);
    }

    public async Task<bool> LoadEntities(IKntService service, FolderInfoDto folder, bool refreshView = true)
    {
        _searchAfterIndex = null;
        try
        {
            Service = service;
            Folder = folder;
            NotesFilter = null;

            Guid f;
            if (Folder == null)
                f = Guid.Empty;                     
            else 
                f = Folder.FolderId;

            Result<List<NoteMinimalDto>> response;
            if(folder == null)
                response = await Service.Notes.GetAllMinimalAsync();
            else 
                response = await Service.Notes.GetByFolderMinimalAsync(f);

            if (response.IsValid)
            {
                ListEntities = response.Entity;
                SetByMeaning(null);

                if(refreshView)
                    View.RefreshView();

                SelectDefaultEntity(refreshView);
            }
            else
            {
                View.ShowInfo(response.ErrorMessage);
                return false;
            }
        }
        catch (Exception ex)
        {
            View.ShowInfo(ex.Message);
            return false;
        }

        return true;
    }

    // KNote Atlas 4.6b: a search or a filter can take long and be canceled (the caller shows its progress, WorkProgressForm): canceled,
    // the list stays as it was (LastLoadCanceled). What the repository is doing is not stopped: its result is not used.
    public bool LastLoadCanceled { get; private set; }

    public async Task<bool> LoadSearchEntities(IKntService service, NotesSearchDto notesSearch, bool refreshView = true,
        CancellationToken cancellationToken = default)
    {
        bool resLoad = false;
        LastLoadCanceled = false;
        // Another list (also the same search done again): the one waiting for the index is not repeated.
        _searchAfterIndex = null;
        try
        {
            Result<List<NoteMinimalDto>> response;
            SemanticSearchResultDto found = null;
            if (string.IsNullOrEmpty(notesSearch?.TextSearch?.Trim()) || service == null || notesSearch == null)
            {
                response = new Result<List<NoteMinimalDto>>();
                response.Entity = new List<NoteMinimalDto>();
            }
            else if (notesSearch.Mode == EnumNotesSearchMode.Meaning)
            {
                (response, found) = await SearchByMeaningAsync(service,
                    new SemanticSearchRequestDto { Text = notesSearch.TextSearch, ResourceKind = notesSearch.ResourceKind }, cancellationToken);
            }
            else
            {
                response = await service.Notes.GetSearchMinimalAsync(notesSearch).WaitAsync(cancellationToken);
            }

            Service = service;
            Folder = null;

            if (response.IsValid)
            {
                ListEntities = response.Entity;
                NotesSearch = notesSearch;
                SetByMeaning(found, ct => LoadSearchEntities(service, notesSearch, refreshView, ct),
                    found != null ? notesSearch.ResourceKind : EnumResourceKind.All);

                if (refreshView)
                    View.RefreshView();

                SelectDefaultEntity(refreshView);
            }
            else
            {
                View.ShowInfo(response.ErrorMessage);
                resLoad = false;
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            LastLoadCanceled = true;
            resLoad = false;
        }
        catch (Exception ex)
        {
            View.ShowInfo(ex.Message);
            resLoad = false;
        }

        return resLoad;
    }

    public async Task<bool> LoadFilteredEntities(IKntService service, NotesFilterDto notesFilter, bool refreshView = true,
        CancellationToken cancellationToken = default)
    {
        bool resLoad = false;
        LastLoadCanceled = false;
        _searchAfterIndex = null;
        try
        {
            Result<List<NoteMinimalDto>> response;
            SemanticSearchResultDto found = null;
            if (service == null || notesFilter == null)
            {
                response = new Result<List<NoteMinimalDto>>();
                response.Entity = new List<NoteMinimalDto>();
            }
            else if (notesFilter.HasMeaningText)
            {
                (response, found) = await SearchByMeaningAsync(service, SemanticSearchRequestDto.ForFilter(notesFilter), cancellationToken);
            }
            else
            {
                response = await service.Notes.GetFilterMinimalAsync(notesFilter).WaitAsync(cancellationToken);
            }

            Service = service;
            Folder = null;

            if (response.IsValid)
            {
                ListEntities = response.Entity;
                NotesFilter = notesFilter;
                SetByMeaning(found, ct => LoadFilteredEntities(service, notesFilter, refreshView, ct));

                if (refreshView)
                    View.RefreshView();

                SelectDefaultEntity(refreshView);
            }
            else
            {
                View.ShowInfo(response.ErrorMessage);
                resLoad = false;
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            LastLoadCanceled = true;
            resLoad = false;
        }
        catch (Exception ex)
        {
            View.ShowInfo(ex.Message);
            resLoad = false;
        }

        return resLoad;
    }

    /// <summary>
    /// «Update index»: the same search by meaning again (before searching, it brings the index up to date: some pending
    /// notes).
    /// </summary>
    public Task<bool> UpdateIndexAsync(CancellationToken cancellationToken = default) =>
        ByMeaning && _searchByMeaningAgain != null ? _searchByMeaningAgain(cancellationToken) : Task.FromResult(false);

    /// <summary>
    /// «Update index» of the notice (4.6b): asks, and starts the full indexing of the repository of the list (Store.SemanticIndex, as
    /// «Update now» of KNote Atlas); the search is repeated when it ends if the list is still this one. False: not started.
    /// </summary>
    public bool RunFullIndex()
    {
        if (!ByMeaning || !CanRunIndex || Store.AtlasJobRunning || _searchByMeaningAgain == null)
            return false;
        if (View.ShowInfo(SemanticIndexTexts.UpdateIndexQuestion, SemanticIndexTexts.UpdateIndex, MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question) != DialogResult.OK)
            return false;

        var repositoryName = Store.GetServiceRef(Service.IdServiceRef)?.Name ?? Service.RepositoryRef?.Name;
        if (!Store.SemanticIndex.Start(Service, repositoryName, rebuild: false))
            return false;
        _searchAfterIndex = _searchByMeaningAgain;
        return true;
    }

    // The full indexing ended: the search again, if it is still the list shown.
    private async void OnIndexProgressChanged(SemanticIndexProgressChanged message)
    {
        if (_searchAfterIndex == null || message.Job.IsRunning || !message.Job.IsOf(Service))
            return;
        var searchAgain = _searchAfterIndex;
        _searchAfterIndex = null;
        await searchAgain(CancellationToken.None);
    }

    // The notes most similar to a text (the search, or the filter with «Talks about…»), the most similar first.
    private static async Task<(Result<List<NoteMinimalDto>> Response, SemanticSearchResultDto Found)> SearchByMeaningAsync(IKntService service,
        SemanticSearchRequestDto request, CancellationToken cancellationToken)
    {
        var response = new Result<List<NoteMinimalDto>>();
        var found = await service.SemanticSearch.SearchAsync(request, cancellationToken).WaitAsync(cancellationToken);
        if (!found.IsValid)
        {
            response.AddListErrorMessage(found.ListErrorMessage);
            return (response, null);
        }
        response.Entity = request.ResourceKind == EnumResourceKind.All
            ? found.Entity.Notes.Cast<NoteMinimalDto>().ToList()
            : found.Entity.Resources.Cast<NoteMinimalDto>().ToList();
        return (response, found.Entity);
    }

    private void SetByMeaning(SemanticSearchResultDto found, Func<CancellationToken, Task<bool>> searchAgain = null, EnumResourceKind kind = EnumResourceKind.All)
    {
        ByMeaning = found != null;
        PendingNotes = found?.PendingNotes ?? 0;
        NotesForFullRun = found?.NotesForFullRun ?? 0;
        NotesWithoutFiles = found?.NotesWithoutFiles ?? 0;
        ResourceKind = found != null ? kind : EnumResourceKind.All;
        _searchByMeaningAgain = found != null ? searchAgain : null;
    }

    /// <summary>
    /// The thumbnail of a photo found (a JPEG with its longest side of <paramref name="side"/> pixels), or null if it can't be read
    /// (its file is not here). Made once for each size.
    /// </summary>
    public async Task<byte[]> GetThumbnailAsync(ResourceSearchHitDto hit, int side)
    {
        var key = (hit.Resource.ResourceId, side);
        if (_thumbnails.TryGetValue(key, out var thumbnail))
            return thumbnail;

        var photo = await Service.ResourceContent.GetPhotoFileAsync(hit.Resource.ResourceId);
        if (photo.IsValid)
            thumbnail = await Task.Run(() => AiImageReducer.Reduce(photo.Entity.ContentArrayBytes,
                AiPhotoRules.ContentType(photo.Entity.FileType, photo.Entity.Name), side)?.Content);
        return _thumbnails[key] = thumbnail;
    }

    /// <summary>
    /// How a resource found is opened (D14): a photo or a PDF in a window of the app (a PDF at the page of its fragment), the
    /// other documents with their program. Its file here or, without it, its URL; null if it has neither.
    /// </summary>
    public (string Url, bool InApp) ResourceToOpen(ResourceSearchHitDto hit)
    {
        var resource = new ResourceDto();
        resource.SetSimpleDto(hit.Resource);
        var path = Service.Notes.UtilGetResourceFilePath(resource);
        var local = !string.IsNullOrEmpty(path) && File.Exists(path);
        var url = local ? new Uri(path).AbsoluteUri : hit.Resource.FullUrl;
        if (string.IsNullOrEmpty(url))
            return (null, false);

        var isPdf = DocumentTextRules.FormatOf(hit.Resource.FileType, hit.Resource.Name) == EnumDocumentFormat.Pdf;
        if (AiPhotoRules.IsPhoto(hit.Resource) || isPdf)
            return (isPdf && hit.FragmentPage is int page ? $"{url}#page={page}" : url, true);
        return (local ? path : url, false);
    }

    public override void SelectItem(NoteMinimalDto item)
    {
        throw new NotImplementedException();
    }

    public override void RefreshItem(NoteMinimalDto note)
    {
        var updateNote = ListEntities?.FirstOrDefault(_ => _.NoteId == note.NoteId);

        if (Folder == null)
        {                
            if (updateNote != null)
            {
                updateNote.SetSimpleDto(note);
                View.RefreshItem(updateNote);
            }
        }
        else
        {
            if (Folder.FolderId == note.FolderId)
            {
                if (updateNote != null)
                {
                    updateNote.SetSimpleDto(note);
                    View.RefreshItem(updateNote);
                }
                else
                {
                    ListEntities.Add(note);
                    View.AddItem(note);
                }
            }
            else
            {
                if(updateNote != null)
                {
                    ListEntities.RemoveAll(_ => _.NoteId == note.NoteId);
                    View.DeleteItem(note);
                }
            }
        }
    }

    public override void AddItem(NoteMinimalDto note)
    {
        RefreshItem(note);
    }

    public override void DeleteItem(NoteMinimalDto note)
    {
        var entiesFoud = ListEntities?.Where(_ => _.NoteId == note.NoteId).Select(_ => _.NoteId).ToList();
        if (entiesFoud?.Count > 0)
        {
            ListEntities.RemoveAll(_ => _.NoteId == note.NoteId);
            View.DeleteItem(note);
        }
    }

    #endregion

    #region Extra methods

    // Fire-and-forget from the grid's header-click handler: a failure to persist the folder's
    // remembered sort order must never disrupt browsing, so exceptions are swallowed here.
    public async Task PersistFolderOrderNotesAsync(string orderNotes)
    {
        if (Folder == null || Service == null)
            return;

        try
        {
            var res = await Service.Folders.UpdateOrderNotesAsync(Folder.FolderId, orderNotes);
            if (res.IsValid)
                Folder.OrderNotes = orderNotes;
        }
        catch (Exception)
        {
        }
    }

    // A refreshed view has already selected its row (the previously selected note, if it is still in the
    // list, see NotesSelectorForm.RefreshView): keep it. Otherwise, the first note.
    private void SelectDefaultEntity(bool refreshView)
    {
        if (ListEntities == null || ListEntities.Count == 0)
            SelectedEntity = null;
        else if (!refreshView || SelectedEntity == null || !ListEntities.Any(n => n.NoteId == SelectedEntity.NoteId))
            SelectedEntity = ListEntities[0];
    }

    public List<NoteMinimalDto> GetSelectedListNotesMinimal()
    {
        return View.GetSelectedListItem();
    }

    public NotesListSnapshot GetDisplayedNotes()
    {
        return View.GetDisplayedNotes();
    }

    public void CleanView()
    {
        ListEntities = new List<NoteMinimalDto>();            
        View.RefreshView();
    }

    #endregion 
}

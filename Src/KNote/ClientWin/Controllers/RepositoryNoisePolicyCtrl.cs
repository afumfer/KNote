using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Service.ServicesCommands;

namespace KNote.ClientWin.Controllers;

/// <summary>
/// Section «Noise policy» of the KNote Atlas window (AtlasCtrl; KNote Atlas, stage 2, 2.2, CU-13, D3): the note types and the
/// folders of the repository whose notes never go into a summary (Service.Summaries). Everyone sees it; a ProjectManager or an
/// Admin changes it (CanSave, the role of its command; the Service checks it too). Each folder is left out on its own, not with its
/// subfolders. How many notes it leaves out, and why, is counted again at each change (the policy as it is edited, not saved yet)
/// and on demand («Count again»: the notes change without the policy).
/// KNote's own types and folders are always out: they are not offered. The changes not saved are asked about before the window
/// changes the repository or closes. Included in the KNote Atlas window (no [KntAuthorize]: it decides).
/// </summary>
public class RepositoryNoisePolicyCtrl : CtrlViewEmbeddableBase<IViewNoisePolicy>
{
    #region Private fields

    private readonly Dictionary<Guid, string> _folderPaths = new();
    private int _countVersion;

    #endregion

    #region Constructor

    public RepositoryNoisePolicyCtrl(Store store) : base(store)
    {
        ControllerName = "Noise policy";
    }

    #endregion

    #region Properties

    public IKntService Service { get; private set; }

    // The policy as it is saved in the repository (Policy is the copy being edited).
    public NoisePolicyDto Saved { get; private set; }

    public NoisePolicyDto Policy { get; private set; }

    // The recount of Policy; null while it is counted.
    public NoiseCountDto Count { get; private set; }

    // The note types that can be left out (not KNote's own), by name.
    public List<NoteTypeDto> NoteTypes { get; private set; } = new();

    // The folders left out, with their paths.
    public List<(Guid FolderId, string Path)> Folders =>
        Policy == null ? new() : Policy.ExcludedFolderIds.Select(id => (id, _folderPaths.TryGetValue(id, out var path) ? path : id.ToString())).ToList();

    public bool CanSave => Service != null && Store.Security.IsAuthorized(typeof(KntSummarySaveNoisePolicyAsyncCommand), Service);

    public bool IsChanged => Policy != null && Saved != null
        && (!Policy.ExcludedNoteTypeIds.ToHashSet().SetEquals(Saved.ExcludedNoteTypeIds) || !Policy.ExcludedFolderIds.ToHashSet().SetEquals(Saved.ExcludedFolderIds));

    public string CountText => Policy == null ? "" : Count == null ? "Counting…" : string.Join("\r\n", NoiseRules.CountLines(Count));

    public string ReadOnlyText => CanSave ? "" : "Only a ProjectManager or an Admin changes the noise policy.";

    // Chooses a folder of the repository (the folder selector; replaced by the tests).
    internal Func<FolderInfoDto> ChooseFolder { get; set; }

    #endregion

    #region Controller implementation

    protected override IViewNoisePolicy CreateView()
    {
        return Store.FactoryViews.Registry.Resolve<RepositoryNoisePolicyCtrl, IViewNoisePolicy>(this);
    }

    #endregion

    #region Public methods

    // Reads the policy of the repository and its note types.
    public async Task<bool> LoadAsync(IKntService service)
    {
        Service = service;
        Saved = Policy = null;
        Count = null;
        _folderPaths.Clear();

        try
        {
            var policy = await service.Summaries.GetNoisePolicyAsync();
            var noteTypes = await service.NoteTypes.GetAllAsync();
            // Another repository was chosen meanwhile: its own read shows it.
            if (service != Service)
                return false;
            if (!policy.IsValid || !noteTypes.IsValid)
            {
                ShowResultError(!policy.IsValid ? policy : noteTypes);
                View.RefreshView();
                return false;
            }

            NoteTypes = (noteTypes.Entity ?? new()).Where(t => !NoiseRules.KNoteNoteTypeNames.Contains(t.Name)).OrderBy(t => t.Name).ToList();
            await ShowAsync(policy.Entity);
            return true;
        }
        catch (Exception ex)
        {
            View.ShowInfo(ex.Message);
            return false;
        }
    }

    public bool IsExcluded(Guid noteTypeId) => Policy?.ExcludedNoteTypeIds.Contains(noteTypeId) == true;

    public async Task SetNoteTypeExcludedAsync(Guid noteTypeId, bool excluded)
    {
        if (Policy == null || IsExcluded(noteTypeId) == excluded)
            return;
        if (excluded)
            Policy.ExcludedNoteTypeIds.Add(noteTypeId);
        else
            Policy.ExcludedNoteTypeIds.Remove(noteTypeId);
        await RecountAsync();
    }

    // «Add…»: a folder chosen with the folder selector.
    public async Task<bool> ChooseAndAddFolderAsync()
    {
        if (Policy == null)
            return false;
        var folder = (ChooseFolder ?? ChooseWithSelector)();
        return folder != null && await AddFolderAsync(folder);
    }

    public async Task<bool> AddFolderAsync(FolderInfoDto folder)
    {
        if (Policy == null || folder == null)
            return false;
        if (KntConst.SystemFolderNames.Contains(folder.Name))
        {
            View.ShowInfo("KNote's own folders are always left out.", ControllerName);
            return false;
        }
        if (Policy.ExcludedFolderIds.Contains(folder.FolderId))
            return false;

        Policy.ExcludedFolderIds.Add(folder.FolderId);
        await LoadPathAsync(folder.FolderId);
        await RecountAsync();
        return true;
    }

    public async Task RemoveFolderAsync(Guid folderId)
    {
        if (Policy == null || !Policy.ExcludedFolderIds.Remove(folderId))
            return;
        await RecountAsync();
    }

    // «Count again»: the notes change without the policy (a tag #NoKnowledge written meanwhile).
    public bool CanCountAgain => Policy != null && Count != null;

    public Task CountAgainAsync() => RecountAsync();

    public async Task DiscardAsync()
    {
        if (Saved != null)
            await ShowAsync(Saved);
    }

    public async Task<bool> SaveAsync()
    {
        if (Policy == null || !CanSave)
            return false;

        try
        {
            var result = await Service.Summaries.SaveNoisePolicyAsync(Copy(Policy));
            if (!result.IsValid)
            {
                ShowResultError(result);
                return false;
            }
            await ShowAsync(result.Entity);
            return true;
        }
        catch (Exception ex)
        {
            View.ShowInfo(ex.Message);
            return false;
        }
    }

    public bool HasPendingChanges() => IsChanged;

    // Before the KNote Atlas window changes the repository or closes: the changes not saved yet are saved, discarded or kept (false:
    // the window stays).
    public async Task<bool> ResolvePendingChangesAsync()
    {
        if (!HasPendingChanges())
            return true;

        var answer = View.ShowInfo("The noise policy has changes not saved. Save them?", ControllerName,
            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

        switch (answer)
        {
            case DialogResult.Yes:
                return await SaveAsync();
            case DialogResult.No:
                await DiscardAsync();
                return true;
            default:
                return false;
        }
    }

    #endregion

    #region Private methods

    private async Task ShowAsync(NoisePolicyDto policy)
    {
        Saved = Copy(policy ?? new NoisePolicyDto());
        Policy = Copy(Saved);
        foreach (var folderId in Policy.ExcludedFolderIds)
            await LoadPathAsync(folderId);
        View.RefreshView();
        await RecountAsync();
    }

    private async Task LoadPathAsync(Guid folderId)
    {
        if (_folderPaths.ContainsKey(folderId))
            return;
        var path = await Service.Folders.GetPathAsync(folderId);
        if (path.IsValid && !string.IsNullOrEmpty(path.Entity))
            _folderPaths[folderId] = path.Entity;
    }

    // The policy as it is now, saved or not; a recount that a newer one left behind is not shown.
    private async Task RecountAsync()
    {
        if (Policy == null)
            return;

        var version = ++_countVersion;
        var service = Service;
        Count = null;
        View.RefreshView();

        var result = await service.Summaries.CountNoiseAsync(Copy(Policy));
        if (version != _countVersion || service != Service)
            return;
        Count = result.IsValid ? result.Entity : new NoiseCountDto();
        View.RefreshCount();
    }

    private FolderInfoDto ChooseWithSelector()
    {
        var folderSelector = new FoldersSelectorCtrl(Store);
        folderSelector.ServicesRef = new List<ServiceRef> { Store.GetServiceRef(Service.IdServiceRef) };
        var res = folderSelector.RunModal();
        return res.Entity == EControllerResult.Executed ? folderSelector.SelectedEntity?.FolderInfo : null;
    }

    private static NoisePolicyDto Copy(NoisePolicyDto policy) => new()
    {
        ExcludedNoteTypeIds = policy.ExcludedNoteTypeIds.ToList(),
        ExcludedFolderIds = policy.ExcludedFolderIds.ToList()
    };

    #endregion
}

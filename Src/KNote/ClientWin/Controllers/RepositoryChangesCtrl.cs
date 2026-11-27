using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

/// <summary>
/// «Repository changes» (the history, H28), the Edit menu of the main window: the changes of a repository in a period, the newest
/// first, 100 at a time, with its filters (the period: the last 7 days at first). The structure only for ProjectManager and up (H29:
/// the Service leaves it out and its kinds are not offered). «Open note» opens a note that exists; «See this version», the note as
/// it was after any change of it; a deleted note is recovered in «Recover deleted notes ...», which opens with the note of the row
/// chosen already selected (if it is still there). Each row has its state (HistoryRowStates: in the
/// deleted notes until a day, can't be recovered, only noted); «Recover deleted notes ...» only for Staff and up (CanRecover). Guest,
/// as reading the notes.
/// </summary>
[KntAuthorize(EnumRoles.Guest)]
public class RepositoryChangesCtrl : CtrlViewBase<IViewRepositoryChanges>
{
    #region Constructor

    public RepositoryChangesCtrl(Store store, IKntService service) : base(store)
    {
        ControllerName = "Repository changes";
        Service = service;
    }

    #endregion

    #region Properties

    public IKntService Service { get; }

    protected override IKntService AuthorizationResource => Service;

    /// <summary>The filters, as the view sets them.</summary>
    public RepositoryChangesQueryDto Query { get; } = new();

    public List<KLogDto> Rows { get; } = new();
    public int Total { get; private set; }
    public bool HasMore => Rows.Count < Total;
    public bool StructureIncluded { get; private set; }
    public List<string> Users { get; private set; } = new();

    /// <summary>The user can recover deleted notes (Staff and up): if not, no «Recover deleted notes ...», and the rows say who can.</summary>
    public bool CanRecover { get; private set; } = true;

    /// <summary>Some note was recovered while the window was open: the main window reads its list again.</summary>
    public bool AnyRecovered { get; private set; }

    public string Error { get; private set; }

    /// <summary>The day the history started (HISTORY_START): a period before it shows nothing more.</summary>
    public DateTime? Since { get; private set; }

    /// <summary>How many changes, in which period and, if it starts before the history, since when there is history.</summary>
    public string Summary => HistoryRules.ChangesSummary(Total, Query.From, Query.To, Since);

    public string Title => $"Repository changes [{Service?.RepositoryRef?.Name}]";

    private bool _sinceRead;

    private readonly HashSet<Guid> _existingNotes = new();

    // The state of each row read that has one (HistoryRowStates.ForChange, given by the Service).
    private readonly Dictionary<Guid, HistoryRowStateDto> _states = new();

    #endregion

    #region Controller implementation

    protected override IViewRepositoryChanges CreateView() =>
        Store.FactoryViews.Registry.Resolve<RepositoryChangesCtrl, IViewRepositoryChanges>(this);

    #endregion

    #region Public methods

    /// <summary>The first page with the filters of Query, and the users of the period; false (with its error) if it can't be read.</summary>
    public async Task<bool> LoadAsync()
    {
        Error = null;
        if (!_sinceRead)
        {
            var retention = await Service.History.GetRetentionAsync();
            Since = retention.IsValid ? retention.Entity?.Since : null;
            _sinceRead = true;
        }
        Query.Skip = 0;
        var result = await Service.History.GetRepositoryChangesAsync(Query);
        if (!result.IsValid)
        {
            Error = result.ErrorMessage;
            return false;
        }
        Rows.Clear();
        _existingNotes.Clear();
        _states.Clear();
        Add(result.Entity);
        StructureIncluded = result.Entity.StructureIncluded;

        var users = await Service.History.GetRepositoryChangesUsersAsync(Query);
        Users = users.IsValid ? users.Entity ?? new() : new();
        if (!string.IsNullOrEmpty(Query.UserName) && !Users.Contains(Query.UserName))
            Users.Add(Query.UserName);
        return true;
    }

    /// <summary>The next page («Show more»).</summary>
    public async Task<bool> LoadMoreAsync()
    {
        Error = null;
        Query.Skip = Rows.Count;
        var result = await Service.History.GetRepositoryChangesAsync(Query);
        Query.Skip = 0;
        if (!result.IsValid)
        {
            Error = result.ErrorMessage;
            return false;
        }
        Add(result.Entity);
        return true;
    }

    /// <summary>What can be brought back of a row (its column «State», the tone of its row and the legend).</summary>
    public EnumHistoryRowState StateOf(KLogDto row) =>
        row != null && _states.TryGetValue(row.KLogId, out var state) ? state.State : EnumHistoryRowState.None;

    /// <summary>The column «State» of a row: «Recoverable until 4/12/2026», «Can't be recovered»...</summary>
    public string StateText(KLogDto row) =>
        row != null && _states.TryGetValue(row.KLogId, out var state) ? HistoryRowStates.Text(state.State, state.Until) : "";

    /// <summary>The states of the rows read, for the legend.</summary>
    public IEnumerable<EnumHistoryRowState> States => _states.Values.Select(s => s.State);

    public bool CanOpen(KLogDto row) => row?.EntityName == HistoryEntities.Note && _existingNotes.Contains(row.EntityId);

    /// <summary>The deleted note of a row, to select it in «Recover deleted notes» (null if the row isn't a deletion of a note).</summary>
    public static Guid? DeletedNoteOf(KLogDto row) =>
        row?.EntityName == HistoryEntities.Note && row.ActionType == nameof(EnumHistoryAction.Deleted) ? row.EntityId : null;

    /// <summary>
    /// Any change of a note: the note as it was after it (H18), also of a note deleted or recovered since; not a deletion whose copy is
    /// gone when the note doesn't exist (the trash emptied it, or «Remove permanently»), nor that removal (HistoryRules.CanSeeVersion).
    /// </summary>
    public bool CanSeeVersion(KLogDto row) => row != null && HistoryRules.CanSeeVersion(row, CanOpen(row));

    /// <summary>«Open note»: in its editor.</summary>
    public async Task OpenNoteAsync(KLogDto row)
    {
        if (!CanOpen(row))
            return;
        var editor = new NoteEditorCtrl(Store);
        if (await editor.LoadModelById(Service, row.EntityId, false))
            editor.Run();
        else
            editor.Dispose();
    }

    /// <summary>«See this version»: the note as it was after that change (a deleted one, as it was deleted).</summary>
    public async Task ShowVersionAsync(KLogDto row)
    {
        if (!CanSeeVersion(row))
            return;
        var versionCtrl = new NoteVersionCtrl(Store, Service, row.EntityId, row, row.EntityTitle);
        if (await versionCtrl.LoadAsync())
            versionCtrl.Run();
        else
        {
            View.ShowInfo(versionCtrl.Error);
            versionCtrl.Finalize();
        }
    }

    /// <summary>
    /// «Recover deleted notes ...» (H24), over this window, with the note of the row chosen already selected if it is the deletion of
    /// a note that can still be recovered; if it recovered some, the list again.
    /// </summary>
    public async Task OpenDeletedNotesAsync(KLogDto row = null)
    {
        var deletedNotesCtrl = new DeletedNotesCtrl(Store, Service) { SelectedNoteId = DeletedNoteOf(row) };
        if (!deletedNotesCtrl.CheckAccess())
            return;
        if (!await deletedNotesCtrl.LoadAsync())
        {
            View.ShowInfo(deletedNotesCtrl.Error);
            deletedNotesCtrl.Finalize();
            return;
        }
        deletedNotesCtrl.RunModal();
        if (deletedNotesCtrl.AnyRecovered)
        {
            AnyRecovered = true;
            await LoadAsync();
            View.RefreshView();
        }
    }

    #endregion

    private void Add(RepositoryChangesPageDto page)
    {
        Rows.AddRange(page.Rows.Where(r => Rows.All(k => k.KLogId != r.KLogId)));
        Total = page.Total;
        _existingNotes.UnionWith(page.ExistingNotes ?? new());
        foreach (var state in page.States ?? new())
            _states[state.KLogId] = state;
        CanRecover = page.CanRecover;
    }
}

using KNote.Model;

namespace KNote.ClientWin.Core;

//TODO: refactor view hierarchy

#region  Base views

public interface IViewBase
{        
    void ShowView();
    Result<EControllerResult> ShowModalView();
    void RefreshView();
    void OnClosingView();        
    DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information);
}

public interface IViewEmbeddable : IViewBase
{
    Control PanelView();
    void ConfigureEmbededMode();
    void ConfigureWindowMode();   
}

#endregion

#region Generals views

public interface IViewEditor<T> : IViewBase
{
    void RefreshModel();
}

public interface IViewEditorEmbeddable<T> : IViewEmbeddable
{
    void CleanView();
    void RefreshModel();
    void RefreshViewOnlyRequiredCtrl();
}

// Small, standalone capability - not every editor shows a folder/repository (e.g.
// PostItPropertiesForm edits window styling, not note content), so this stays out of the generic
// IViewEditorEmbeddable<T>/IViewPostIt<T> contracts and is combined into the narrower view
// interfaces below, only for the Ctrls that actually need it.
public interface IFolderAndRepositoryDisplay
{
    // Re-renders only the folder path / repository alias display, without touching the rest of the
    // view (a full RefreshView() would reset scroll position, selected list item, active tab, etc.)
    // - used when a folder or repository gets renamed elsewhere while this note/post-it is open, see
    // NoteEditorCtrl/PostItEditorCtrl's Store.Events subscriptions.
    Task RefreshFolderAndRepositoryDisplayAsync();
}

// NoteEditorCtrl's own view contract: IViewEditorEmbeddable<T> plus the folder/repository display
// capability. Kept as its own interface (rather than widening IViewEditorEmbeddable<T> itself) so
// other, unrelated IViewEditorEmbeddable<T> implementers aren't forced to implement a member they
// have no use for.
public interface IViewNoteEditorEmbeddable<T> : IViewEditorEmbeddable<T>, IFolderAndRepositoryDisplay
{
}

/// <summary>
/// View for a selector whose list is kept in sync in place (CtrlSyncableSelectorBase) - AddItem/
/// DeleteItem/RefreshItem/SelectItem exist for a list that keeps updating while the selector stays
/// open (e.g. FoldersSelectorForm/NotesSelectorForm). A "pick one from a fixed, load-once list"
/// selector (e.g. NoteTypesSelectorForm/NoteTypesSelectorCtrl : CtrlSelectorBase) has no use for any
/// of that and implements plain IViewEmbeddable instead, not this interface.
/// </summary>
public interface IViewSelector<TItem> : IViewEmbeddable
{
    void RefreshItem(TItem item);
    void DeleteItem(TItem item);
    void AddItem(TItem item);
    object SelectItem(TItem item);
    List<TItem> GetSelectedListItem();
}

/// <summary>
/// View for the "manage list" family (CtrlManageListBase): an embeddable list of entities with
/// add/edit/delete actions that persist immediately, as opposed to IViewSelector (pick-and-return an
/// entity) or IViewEditor (edit a single entity already loaded elsewhere).
/// </summary>
public interface IViewManageList<TEntity> : IViewEmbeddable
{
    void AddItem(TEntity item);
    void UpdateItem(TEntity item);
    void RemoveItem(TEntity item);
}

#endregion

#region Specific views

public interface IViewKNoteManagement : IViewBase
{
    void HideView();
    void ActivateView();
    void ActivateWaitState();
    void DeactivateWaitState();
    void ReportProgressKNoteManagement(int porcentaje);
    void SetVisibleProgressBar(bool visible);

    // Single-value input prompt (backed by ReadVarForm), used by KNoteManagementCtrl.ChangeTags so the
    // controller doesn't have to reference a concrete WinForms Form to ask the user for the tag text.
    // Returns the value the user typed, or null if the dialog was canceled.
    string PromptForValue(string label, string caption);

    // Raised once this view is actually visible under a running message loop. The app bootstrap
    // (Program.cs) needs this to defer KNoteManagementCtrl.Run() until Application.Run's loop is
    // truly pumping (some notes need it, e.g. WebView2 content), without knowing this view is a
    // WinForms Form or anything about its native Shown event.
    event EventHandler ViewShown;
}

public interface IViewPostIt<T> : IViewBase
{
    void RefreshModel();
    void HideView();
    void ActivateView();
}

// PostItEditorCtrl's own view contract: IViewPostIt<T> plus the folder/repository display
// capability (see IFolderAndRepositoryDisplay). Kept separate so other IViewPostIt<T> implementers
// with nothing to do with folders/repositories (e.g. PostItPropertiesForm, which edits window
// styling) aren't forced to implement a member they have no use for.
public interface IViewPostItEditor<T> : IViewPostIt<T>, IFolderAndRepositoryDisplay
{
}

public interface IViewChat : IViewBase
{
    void VisibleView(bool visible);
}

public interface IViewServerCOM : IViewChat
{
    void RefreshStatus();
}

// AppInfoAlarmsCtrl's view: a single persistent window that accumulates rows over time (added from
// the alarms timer, see MessagesManagementCtrl.AppAlarm) rather than being loaded/replaced as a
// whole, so it gets its own AddOrUpdateRow instead of the pull-based IViewSelector<T>/
// IViewManageList<T> shapes.
public interface IViewAppInfoAlarms : IViewEmbeddable
{
    void AddOrUpdateRow(AppInfoAlarmRow row);
    void RemoveRow(Guid kMessageId);
    void HideView();
    void ActivateView();
}

public interface IViewHeavyProcess : IViewBase
{
    CancellationTokenSource CancellationToken { get; set; }
    public IProgress<KeyValuePair<int, string>> ReportProgress { get; set; }
    void UpdateProgress(int progress);
    void UpdateProcessName(string process);
    void UpdateProcessInfo(string info);
    void HideView();    
}


#endregion 


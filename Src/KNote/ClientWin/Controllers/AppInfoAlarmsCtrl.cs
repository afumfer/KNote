using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using Microsoft.Extensions.Logging;

namespace KNote.ClientWin.Controllers;

// Owns the single, persistent "Application info" alarms panel: an alternative to PostIt notes for
// users who prefer a list of pending reminders over PostIts scattered on screen. Unlike the
// selector/editor Ctrl families, it isn't loaded once and torn down - KNoteManagementCtrl keeps one
// instance alive for the whole session and keeps feeding it rows as AppInfo alarms fire.
//
// Deliberately NOT a CtrlSelectorBase/CtrlSyncableSelectorBase: those families model "pick one entity
// and hand it back to the caller" (Accept/Cancel, a single IKntService), which isn't what happens
// here - rows arrive pushed from the alarms timer (not loaded from one repository on demand), the
// list aggregates across every configured repository at once (each row carries its own
// RepositoryAlias), and double-click/"Remove from list" act immediately instead of closing the
// controller with a selection. See CtrlViewEmbeddableBase's own doc comment for this same call on
// NotesSearchParamCtrl/NotesFilterParamCtrl.
public class AppInfoAlarmsCtrl : CtrlViewEmbeddableBase<IViewAppInfoAlarms>
{
    #region Constructor

    public AppInfoAlarmsCtrl(Store store) : base(store)
    {
        ControllerName = "Application info alarms";

        // A row must disappear if its note is deleted from anywhere else (full editor or PostIt),
        // same as PostIt windows already do for themselves (see PostItEditorCtrl.OnNoteDeletedElsewhere).
        Store.Events.Subscribe<EntityDeleted<NoteDto>>(OnNoteDeletedElsewhere);
        Store.Events.Subscribe<EntityDeleted<NoteExtendedDto>>(OnNoteDeletedElsewhereExtended);

        // ...and its displayed data must follow note/alarm edits made in the full editor (the PostIt
        // can't change the topic or the alarms shown here, so its EntitySaved<NoteDto> is not needed).
        Store.Events.Subscribe<EntitySaved<NoteExtendedDto>>(OnNoteSavedElsewhereExtended);
    }

    public override void Dispose()
    {
        Store.Events.Unsubscribe<EntityDeleted<NoteDto>>(OnNoteDeletedElsewhere);
        Store.Events.Unsubscribe<EntityDeleted<NoteExtendedDto>>(OnNoteDeletedElsewhereExtended);
        Store.Events.Unsubscribe<EntitySaved<NoteExtendedDto>>(OnNoteSavedElsewhereExtended);
        base.Dispose();
    }

    #endregion

    #region Controller override methods

    protected override IViewAppInfoAlarms CreateView()
    {
        return Store.FactoryViews.Registry.Resolve<AppInfoAlarmsCtrl, IViewAppInfoAlarms>(this);
    }

    protected override Result<EControllerResult> OnInitialized()
    {
        var result = base.OnInitialized();
        LoadPersistedRows();
        return result;
    }

    #endregion

    #region Controller methods

    // Only KMessageId/RepositoryAlias/NotifiedAt actually come back from the state file
    // (see AppInfoAlarmRowConfig) - the rest is looked up fresh from the owning repository so the
    // list never shows a stale note title/comment/user. A message that no longer exists (note or
    // message deleted while the app was closed) quietly drops its row instead of showing a blank one.
    // Fire-and-forget from the synchronous OnInitialized(), same pattern as
    // MessagesManagementCtrl.OnInitialized() kicking off VisibleWindows().
    private async void LoadPersistedRows()
    {
        foreach (var saved in Store.State.AppInfoAlarmsWindow.Rows.ToList())
        {
            var serviceRef = Store.GetServiceRef(saved.RepositoryAlias);
            if (serviceRef == null)
                continue;

            if (!await TryHydrateRowAsync(serviceRef.Service, saved))
            {
                Store.State.AppInfoAlarmsWindow.Rows.Remove(saved);
                continue;
            }

            View.AddOrUpdateRow(saved);
        }
        Store.SaveConfig();
    }

    private static async Task<bool> TryHydrateRowAsync(IKntService service, AppInfoAlarmRowConfig row)
    {
        var message = await service.Notes.GetMessageAsync(row.KMessageId);
        if (!message.IsValid || message.Entity.NoteId == null)
            return false;

        var note = await service.Notes.GetAsync(message.Entity.NoteId.Value);
        if (!note.IsValid)
            return false;

        var user = await service.Users.GetAsync(message.Entity.UserId ?? Guid.Empty);

        row.NoteId = message.Entity.NoteId.Value;
        row.NoteTopic = note.Entity.Topic;
        row.Comment = message.Entity.Comment;
        row.UserFullName = user.Entity?.FullName;
        return true;
    }

    public void AddOrUpdateRow(AppInfoAlarmRowConfig row)
    {
        Store.State.AppInfoAlarmsWindow.Rows.RemoveAll(r => r.KMessageId == row.KMessageId);
        Store.State.AppInfoAlarmsWindow.Rows.Add(row);
        Store.SaveConfig();

        View.AddOrUpdateRow(row);
    }

    // Called from the panel's "Remove from list" context menu action, and from OnNoteDeletedElsewhere
    // - the only two ways a row is meant to leave the list.
    public void RemoveRow(Guid kMessageId)
    {
        Store.State.AppInfoAlarmsWindow.Rows.RemoveAll(r => r.KMessageId == kMessageId);
        Store.SaveConfig();

        View.RemoveRow(kMessageId);
    }

    public void Activate()
    {
        View.ActivateView();
    }

    // The row only keeps its RepositoryAlias (it must stay a plain, serializable Model type to live
    // in the state file) - the live IKntService needed to actually reopen the note is resolved here, on
    // demand, instead of being cached on the row for the row's whole (persisted, cross-session)
    // lifetime, where it could go stale if the repository were removed/reconnected meanwhile.
    public void RequestOpenNote(AppInfoAlarmRowConfig row)
    {
        var serviceRef = Store.GetServiceRef(row.RepositoryAlias);
        if (serviceRef == null)
        {
            View.ShowInfo($"The repository \"{row.RepositoryAlias}\" is no longer configured, so this note cannot be opened from here.");
            return;
        }

        OpenNoteRequested?.Invoke(this, new ControllerEventArgs<ServiceWithNoteId>(
            new ServiceWithNoteId { Service = serviceRef.Service, NoteId = row.NoteId }));
    }

    #endregion

    #region Store events

    private void OnNoteDeletedElsewhere(EntityDeleted<NoteDto> e) => RemoveRowsForNote(e.Entity.NoteId);

    private void OnNoteDeletedElsewhereExtended(EntityDeleted<NoteExtendedDto> e) => RemoveRowsForNote(e.Entity.NoteId);

    // Async void: Store.Events handlers are synchronous Action<T>. Everything before the first await
    // (and after it, since the event is published from the UI thread) runs on the UI thread, like the
    // other Store.Events subscribers.
    private async void OnNoteSavedElsewhereExtended(EntitySaved<NoteExtendedDto> e)
    {
        try
        {
            var rows = Store.State.AppInfoAlarmsWindow.Rows;
            if (!rows.Any(r => r.NoteId == e.Entity.NoteId))
                return;

            // Which user is the "active" one is per repository; resolve it once per repository involved.
            var activeUsers = new Dictionary<string, Guid?>();
            foreach (var alias in rows.Where(r => r.NoteId == e.Entity.NoteId).Select(r => r.RepositoryAlias).Distinct())
            {
                var serviceRef = Store.GetServiceRef(alias);
                activeUsers[alias] = serviceRef == null ? null : await Store.GetUserId(serviceRef.Service);
            }

            ApplyChanges(AppInfoRowRefresh.ApplyNoteSaved(rows, e.Entity, alias => activeUsers.GetValueOrDefault(alias)));
        }
        catch (Exception ex)
        {
            Store.Logger?.LogError(ex, "OnNoteSavedElsewhereExtended: {message}", ex.Message);
        }
    }

    // Updated rows only change [XmlIgnore] display fields, so there is nothing to persist; a removed
    // row goes through RemoveRow, which also updates the state file.
    private void ApplyChanges(IEnumerable<AppInfoRowRefresh.RowChange> changes)
    {
        foreach (var change in changes.ToList())
        {
            if (change.Kind == AppInfoRowRefresh.ChangeKind.Removed)
                RemoveRow(change.Row.KMessageId);
            else
                View.AddOrUpdateRow(change.Row);
        }
    }

    private void RemoveRowsForNote(Guid noteId)
    {
        var affected = Store.State.AppInfoAlarmsWindow.Rows.Where(r => r.NoteId == noteId).Select(r => r.KMessageId).ToList();
        foreach (var kMessageId in affected)
            RemoveRow(kMessageId);
    }

    #endregion

    #region Controller events

    // Raised by RequestOpenNote; KNoteManagementCtrl (which owns this Ctrl) subscribes to actually
    // open the note, since that's its responsibility, not this Ctrl's.
    public event EventHandler<ControllerEventArgs<ServiceWithNoteId>> OpenNoteRequested;

    #endregion
}

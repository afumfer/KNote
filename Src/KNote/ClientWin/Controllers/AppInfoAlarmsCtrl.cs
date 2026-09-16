using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

// Owns the single, persistent "Application info" alarms panel: an alternative to PostIt notes for
// users who prefer a list of pending reminders over PostIts scattered on screen. Unlike the
// selector/editor Ctrl families, it isn't loaded once and torn down - KNoteManagmentCtrl keeps one
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
    }

    public override void Dispose()
    {
        Store.Events.Unsubscribe<EntityDeleted<NoteDto>>(OnNoteDeletedElsewhere);
        Store.Events.Unsubscribe<EntityDeleted<NoteExtendedDto>>(OnNoteDeletedElsewhereExtended);
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

    // Only KMessageId/RepositoryAlias/NotifiedAt actually come back from AppConfig deserialization
    // (see AppInfoAlarmRowConfig) - the rest is looked up fresh from the owning repository so the
    // list never shows a stale note title/comment/user. A message that no longer exists (note or
    // message deleted while the app was closed) quietly drops its row instead of showing a blank one.
    // Fire-and-forget from the synchronous OnInitialized(), same pattern as
    // MessagesManagmentCtrl.OnInitialized() kicking off VisibleWindows().
    private async void LoadPersistedRows()
    {
        foreach (var saved in Store.AppConfig.AppInfoAlarmsRows.ToList())
        {
            var serviceRef = Store.GetServiceRef(saved.RepositoryAlias);
            if (serviceRef == null)
                continue;

            if (!await TryHydrateRowAsync(serviceRef.Service, saved))
            {
                Store.AppConfig.AppInfoAlarmsRows.Remove(saved);
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
        Store.AppConfig.AppInfoAlarmsRows.RemoveAll(r => r.KMessageId == row.KMessageId);
        Store.AppConfig.AppInfoAlarmsRows.Add(row);
        Store.SaveConfig();

        View.AddOrUpdateRow(row);
    }

    // Called from the panel's "Remove from list" context menu action, and from OnNoteDeletedElsewhere
    // - the only two ways a row is meant to leave the list.
    public void RemoveRow(Guid kMessageId)
    {
        Store.AppConfig.AppInfoAlarmsRows.RemoveAll(r => r.KMessageId == kMessageId);
        Store.SaveConfig();

        View.RemoveRow(kMessageId);
    }

    public void Activate()
    {
        View.ActivateView();
    }

    // The row only keeps its RepositoryAlias (it must stay a plain, serializable Model type to live
    // in AppConfig) - the live IKntService needed to actually reopen the note is resolved here, on
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

    private void RemoveRowsForNote(Guid noteId)
    {
        var affected = Store.AppConfig.AppInfoAlarmsRows.Where(r => r.NoteId == noteId).Select(r => r.KMessageId).ToList();
        foreach (var kMessageId in affected)
            RemoveRow(kMessageId);
    }

    #endregion

    #region Controller events

    // Raised by RequestOpenNote; KNoteManagmentCtrl (which owns this Ctrl) subscribes to actually
    // open the note, since that's its responsibility, not this Ctrl's.
    public event EventHandler<ControllerEventArgs<ServiceWithNoteId>> OpenNoteRequested;

    #endregion
}

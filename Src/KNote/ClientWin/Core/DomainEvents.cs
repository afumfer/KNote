using KNote.Service.Core;

namespace KNote.ClientWin.Core;

/// <summary>
/// Messages published on Store.Events by CtrlEditorBase (Fase 3 of the ClientWin architecture
/// refactor, see ClientWin/CLAUDE.md). Generic over the entity type so they apply to every editor
/// controller (NoteEditorCtrl, PostItEditorCtrl, TaskEditorCtrl, ResourceEditorCtrl, ...), not just
/// the two special-cased today in Store.AddController/RemoveController.
/// </summary>
public record EntitySaved<TEntity>(TEntity Entity);

public record EntityAdded<TEntity>(TEntity Entity);

public record EntityDeleted<TEntity>(TEntity Entity);

/// <summary>
/// Published by NoteEditorCtrl.OnPostItEdit (Fase 3b): the user wants to keep editing this note
/// as a post-it. Previously only reachable via Store's NoteEditorCtrl special-casing.
/// </summary>
public record PostItEditRequested(ServiceWithNoteId Target);

/// <summary>
/// Published by PostItEditorCtrl.OnExtendedEdit (Fase 3b): the user wants to switch this post-it
/// to the full note editor. Previously only reachable via Store's PostItEditorCtrl special-casing.
/// </summary>
public record ExtendedEditRequested(ServiceWithNoteId Target);

/// <summary>
/// Republished on Store.Events by Store.AddServiceRef/RemoveServiceRef's subscription to every
/// IKntService.CommandExecuting/CommandExecuted it manages - lets any controller observe every
/// service command run against any repository the app has open, without knowing about ServiceRef
/// internals. Args.Service.IdServiceRef + Store.GetServiceRef(id) resolves the owning ServiceRef
/// (alias, RepositoryRef) for a subscriber that needs it. See MonitorCtrl for a reference subscriber.
/// </summary>
public record ServiceCommandExecuting(CommandExecutingEventArgs Args);

public record ServiceCommandExecuted(CommandExecutedEventArgs Args);

/// <summary>
/// Republished on Store.Events by Store.AddController/RemoveController/AddServiceRef/RemoveServiceRef/
/// OnControllerNotification - migrated from Store's own dedicated EventHandler&lt;T&gt; fields
/// (AddedController/RemovedController/ControllerStateChanged/AddedServiceRef/RemovedServiceRef/
/// ControllerNotification) to the same Store.Events mechanism used for domain and command events, for
/// consistency. See MonitorCtrl for a subscriber.
/// </summary>
public record ControllerAdded(CtrlBase Controller);

public record ControllerRemoved(CtrlBase Controller);

public record ControllerStateChanged(CtrlBase Controller, EControllerState State);

public record ControllerNotification(CtrlBase Controller, string Message);

public record ServiceRefAdded(ServiceRef ServiceRef);

public record ServiceRefRemoved(ServiceRef ServiceRef);

/// <summary>
/// Published by OptionsEditorCtrl.SaveModel when an option that changes how the notes list is laid out
/// (currently "Compact view in notes list") has been changed and saved: NotesSelectorForm listens to it
/// to apply the change to the already-open list instead of waiting for a restart.
/// </summary>
public record NotesListViewOptionsChanged;

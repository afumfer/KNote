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

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

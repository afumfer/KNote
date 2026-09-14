using KNote.ClientWin.Controllers;

namespace KNote.ClientWin.Core;

/// <summary>
/// Storage for the CtrlBase instances currently alive in the app (Fase 2 of the ClientWin
/// architecture refactor, see ClientWin/CLAUDE.md). Event wiring (including the NoteEditorCtrl/
/// PostItEditorCtrl special-casing) and logging stay in Store; this class only owns the collection.
/// </summary>
public class ControllerRegistry
{
    // Add/Remove normally run on the UI thread (every CtrlBase registers/unregisters itself in its
    // constructor/Finalize), but KNoteScriptLibrary - callable from a running KntScript, which
    // Store.RunKntSCodeInNewThread executes on its own background Thread - constructs controllers
    // directly (e.g. new MonitorCtrl(_store), new NoteEditorCtrl(_store)), so Add can happen from a
    // non-UI thread too. Meanwhile Store.SaveActiveNotes() (the autosave timer, UI thread) iterates
    // All. Same shape of bug as DomainEventBus: a lock, and All returning a snapshot instead of the
    // live list, so a concurrent Add/Remove can't corrupt an in-progress enumeration.
    private readonly object _lock = new();
    private readonly List<CtrlBase> _controllers = new();

    public void Add(CtrlBase controller)
    {
        lock (_lock)
            _controllers.Add(controller);
    }

    public void Remove(CtrlBase controller)
    {
        lock (_lock)
            _controllers.Remove(controller);
    }

    public IReadOnlyList<CtrlBase> All
    {
        get
        {
            lock (_lock)
                return _controllers.ToList();
        }
    }
}

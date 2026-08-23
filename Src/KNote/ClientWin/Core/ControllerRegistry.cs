using KNote.ClientWin.Controllers;

namespace KNote.ClientWin.Core;

/// <summary>
/// Storage for the CtrlBase instances currently alive in the app (Fase 2 of the ClientWin
/// architecture refactor, see ClientWin/CLAUDE.md). Event wiring (including the NoteEditorCtrl/
/// PostItEditorCtrl special-casing) and logging stay in Store; this class only owns the collection.
/// </summary>
public class ControllerRegistry
{
    private readonly List<CtrlBase> _controllers = new();

    public void Add(CtrlBase controller) => _controllers.Add(controller);

    public void Remove(CtrlBase controller) => _controllers.Remove(controller);

    public IReadOnlyList<CtrlBase> All => _controllers;
}

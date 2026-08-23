namespace KNote.ClientWin.Core;

public interface IFactoryViews
{
    /// <summary>
    /// Registry every Ctrl resolves its view against (Fase 4/4b of the ClientWin architecture
    /// refactor, see ClientWin/CLAUDE.md). Register a factory here (typically in
    /// FactoryViewsWinForms's constructor) and resolve it from CreateView() - no interface
    /// overload needed.
    /// </summary>
    ViewFactoryRegistry Registry { get; }
}

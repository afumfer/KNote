namespace KNote.ClientWin.Core;

/// <summary>
/// Generic Ctrl -> View factory registry (Fase 4 of the ClientWin architecture refactor, see
/// ClientWin/CLAUDE.md). Lets a Ctrl/View pair be wired up by registering a factory function
/// instead of adding a new overload to IFactoryViews - the interface every concrete Ctrl type had
/// to be added to before this. FactoryViewsWinForms registers its 25 existing mappings here in its
/// constructor and its IFactoryViews methods now just resolve through it; IFactoryViews itself is
/// left in place unchanged (single implementation, nothing else depends on adding to it).
///
/// The optional "key" disambiguates controller types that need more than one view (e.g.
/// KNoteManagmentCtrl has a main view, a notify view and an about view).
/// </summary>
public class ViewFactoryRegistry
{
    private readonly Dictionary<(Type ControllerType, string Key), Delegate> _factories = new();

    public void Register<TCtrl, TView>(Func<TCtrl, TView> factory, string key = "") where TCtrl : CtrlBase
    {
        _factories[(typeof(TCtrl), key)] = factory;
    }

    public TView Resolve<TCtrl, TView>(TCtrl controller, string key = "") where TCtrl : CtrlBase
    {
        if (!_factories.TryGetValue((typeof(TCtrl), key), out var factory))
        {
            var keySuffix = string.IsNullOrEmpty(key) ? "" : $" with key '{key}'";
            throw new InvalidOperationException($"No view factory registered for controller type '{typeof(TCtrl).Name}'{keySuffix}.");
        }

        return ((Func<TCtrl, TView>)factory).Invoke(controller);
    }
}

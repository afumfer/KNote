using KNote.ClientWin.Core;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

public class NotesSearchParamCtrl : CtrlViewEmbeddableBase<IViewEmbeddable>
{
    #region Properties

    private List<ServiceRef> _servicesRef;
    public List<ServiceRef> ServicesRef
    {
        get
        {
            if (_servicesRef == null)
                _servicesRef = Store.GetAllServiceRef();
            return _servicesRef;
        }
        set
        {
            _servicesRef = value;
        }
    }

    #endregion

    #region Constructor

    public NotesSearchParamCtrl(Store store) : base(store)
    {
        ControllerName = "Notes search param";
    }

    #endregion

    #region ControllerEditorBase implementation

    protected override IViewEmbeddable CreateView()
    {
        return Store.FactoryViews.Registry.Resolve<NotesSearchParamCtrl, IViewEmbeddable>(this);
    }

    #endregion

    #region Controller events

    public event EventHandler<ControllerEventArgs<SelectedNotesInServiceRef>> SearchApplied;

    protected virtual void OnSearchApplied(SelectedNotesInServiceRef search)
    {
        SearchApplied?.Invoke(this, new ControllerEventArgs<SelectedNotesInServiceRef>(search));
    }

    public void NotifySearchApplied(SelectedNotesInServiceRef search)
    {
        OnSearchApplied(search);
    }

    #endregion
}

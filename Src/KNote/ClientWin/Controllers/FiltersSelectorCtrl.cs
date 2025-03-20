using KNote.ClientWin.Core;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

public class FiltersSelectorCtrl : CtrlSelectorBase<IViewSelector<SelectedNotesInServiceRef>, SelectedNotesInServiceRef>
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

    public FiltersSelectorCtrl(Store store): base (store)
    {
        ControllerName = "Filter param";
        
        // ListEntities == TODO: ... load all stores filters 
    }

    #endregion

    #region ComponentEditorBase implementation 

    protected override IViewSelector<SelectedNotesInServiceRef> CreateView()
    {
        return Store.FactoryViews.View(this);
    }

    public override Task<bool> LoadEntities(IKntService service, bool refreshView = true)
    {
        throw new NotImplementedException();
    }

    public override void SelectItem(SelectedNotesInServiceRef item)
    {
        throw new NotImplementedException();
    }

    public override void RefreshItem(SelectedNotesInServiceRef item)
    {
        throw new NotImplementedException();
    }

    public override void AddItem(SelectedNotesInServiceRef item)
    {
        throw new NotImplementedException();
    }

    public override void DeleteItem(SelectedNotesInServiceRef item)
    {
        throw new NotImplementedException();
    }

    #endregion
}

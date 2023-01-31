using KNote.ClientWin.Core;
using KNote.Service.Core;

namespace KNote.ClientWin.Components;

public class FiltersSelectorComponent : ComponentSelectorBase<ISelectorView<NotesFilterWithServiceRef>, NotesFilterWithServiceRef>
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

    public FiltersSelectorComponent(Store store): base (store)
    {
        ComponentName = "Filter param";
        
        // ListEntities == TODO: ... load all stores filters 
    }

    #endregion

    #region ComponentEditorBase implementation 

    protected override ISelectorView<NotesFilterWithServiceRef> CreateView()
    {
        return Store.FactoryViews.View(this);
    }

    public override Task<bool> LoadEntities(IKntService service, bool refreshView = true)
    {
        throw new NotImplementedException();
    }

    public override void SelectItem(NotesFilterWithServiceRef item)
    {
        throw new NotImplementedException();
    }

    public override void RefreshItem(NotesFilterWithServiceRef item)
    {
        throw new NotImplementedException();
    }

    public override void AddItem(NotesFilterWithServiceRef item)
    {
        throw new NotImplementedException();
    }

    public override void DeleteItem(NotesFilterWithServiceRef item)
    {
        throw new NotImplementedException();
    }

    #endregion
}

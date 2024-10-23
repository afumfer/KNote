using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

public class FoldersSelectorCtrl : CtrlSelectorBase<IViewSelector<FolderWithServiceRef>, FolderWithServiceRef>
{
    #region Properties

    private List<ServiceRef> _servicesRef;
    public List<ServiceRef> ServicesRef
    {
        get
        {
            if(_servicesRef == null)
                _servicesRef = Store.GetAllServiceRef();
            return _servicesRef;
        }
        set 
        {
            _servicesRef = value;
        }
    }

    public string Path { get; set; }

    public Guid? OldParent { get; set; }

    #endregion 

    #region Constructor

    public FoldersSelectorCtrl(Store store) : base(store)
    {
        ComponentName = "Folders selector";
        ListEntities = new List<FolderWithServiceRef>();
    }

    #endregion 

    #region Component override methods 

    protected override IViewSelector<FolderWithServiceRef> CreateView()
    {
        return Store.FactoryViews.View(this);
    }

    #endregion 

    #region Component methods

    public override Task<bool> LoadEntities(IKntService service, bool refreshView = true)
    {
        throw new NotImplementedException();
    }

    public async Task<List<FolderDto>> LoadEntities(ServiceRef serviceRef)
    {
        try
        {
            return (await serviceRef.Service.Folders.GetTreeAsync()).Entity;
        }
        catch (Exception ex)
        {
            View.ShowInfo(ex.Message);
            throw;
        }
    }

    public override void SelectItem(FolderWithServiceRef folder)
    {
        View.SelectItem(folder);
    }

    public override void RefreshItem(FolderWithServiceRef item)
    {
        View.RefreshItem(item);
        if (SelectedEntity.FolderInfo.FolderId == item.FolderInfo.FolderId)            
            SelectedEntity.FolderInfo.SetSimpleDto(item.FolderInfo);                        
    }

    public override void AddItem(FolderWithServiceRef item)
    {
        ListEntities.Add(item);
        View.AddItem(item);
    }

    public override void DeleteItem(FolderWithServiceRef item)
    {
        ListEntities.Remove(item);
        View.DeleteItem(item);
    }

    #endregion

    #region Component extra methods

    public void Refresh()
    {
        View.RefreshView();
    }

    #endregion
}

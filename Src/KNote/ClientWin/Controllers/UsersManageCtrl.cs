using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

/// <summary>
/// Users tab of the repository administration screen (RepositoryEditorCtrl): lists the users of the
/// repository being managed and delegates add/edit to the UserEditorCtrl popup. No "can't delete the
/// last Admin" guard - matches the Blazor admin panel today (UsersIndex.razor has none either).
/// </summary>
public class UsersManageCtrl : CtrlManageListBase<IViewManageList<UserDto>, UserDto>
{
    #region Constructor

    public UsersManageCtrl(Store store) : base(store)
    {
        ControllerName = "Users management";
    }

    #endregion

    #region CtrlManageListBase implementation

    protected override IViewManageList<UserDto> CreateView()
    {
        return Store.FactoryViews.Registry.Resolve<UsersManageCtrl, IViewManageList<UserDto>>(this);
    }

    public override async Task<bool> LoadEntitiesAsync(IKntService service, bool refreshView = true)
    {
        try
        {
            Service = service;

            var response = await Service.Users.GetAllAsync();

            if (response.IsValid)
            {
                ListEntities = response.Entity;

                if (refreshView)
                    View.RefreshView();

                return true;
            }
            else
            {
                View.ShowInfo(response.ErrorMessage);
                return false;
            }
        }
        catch (Exception ex)
        {
            View.ShowInfo(ex.Message);
            return false;
        }
    }

    public override async Task<bool> AddItemAsync()
    {
        var editorCtrl = new UserEditorCtrl(Store);
        await editorCtrl.NewModel(Service);

        var res = editorCtrl.RunModal();
        if (res.Entity == EControllerResult.Executed)
        {
            ListEntities.Add(editorCtrl.Model);
            View.AddItem(editorCtrl.Model);
            return true;
        }
        return false;
    }

    public override async Task<bool> EditItemAsync(UserDto item)
    {
        var editorCtrl = new UserEditorCtrl(Store);
        // refreshView must stay true here: unlike embedded-only forms, this popup is shown via
        // RunModal() right after, and there is no Form_Load wiring to populate it later - without
        // this, the dialog opens with the fields blank even though Model has the loaded entity.
        await editorCtrl.LoadModelById(Service, item.UserId, true);

        var res = editorCtrl.RunModal();
        if (res.Entity == EControllerResult.Executed)
        {
            var index = ListEntities.FindIndex(_ => _.UserId == item.UserId);
            if (index >= 0)
                ListEntities[index] = editorCtrl.Model;

            View.UpdateItem(editorCtrl.Model);
            return true;
        }
        return false;
    }

    public override async Task<bool> DeleteItemAsync(UserDto item)
    {
        var editorCtrl = new UserEditorCtrl(Store);
        var deleted = await editorCtrl.DeleteModel(Service, item.UserId);

        if (deleted)
        {
            ListEntities.RemoveAll(_ => _.UserId == item.UserId);
            View.RemoveItem(item);
        }
        return deleted;
    }

    #endregion
}

using KNote.ClientWin.Core;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

public class UsersSelectorCtrl : CtrlSelectorBase<IViewEmbeddable, UserDto>
{
    #region Properties

    // User to preselect in the list once loaded, e.g. the user currently assigned to the message/alarm.
    public Guid? SelectedUserId { get; set; }

    #endregion

    #region Constructor

    public UsersSelectorCtrl(Store store): base(store)
    {
        ControllerName = "User selector";
    }

    #endregion

    #region ISelectorView

    protected override IViewEmbeddable CreateView()
    {
        return Store.FactoryViews.Registry.Resolve<UsersSelectorCtrl, IViewEmbeddable>(this);
    }

    #endregion

    #region Controller override

    public async override Task<bool> LoadEntities(IKntService service, bool refreshView = true)
    {
        try
        {
            Service = service;

            var response = await Service.Users.GetAllAsync();

            if (response.IsValid)
            {
                ListEntities = response.Entity.Where(u => !u.Disabled).OrderBy(u => u.FullName).ToList();

                if (refreshView)
                    View.RefreshView();

                SelectedEntity = ListEntities?.FirstOrDefault(u => u.UserId == SelectedUserId) ?? ListEntities?.FirstOrDefault();

                NotifySelectedEntity();
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

        return true;
    }

    #endregion
}

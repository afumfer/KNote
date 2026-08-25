using KNote.ClientWin.Core;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

public class UserRegisterCtrl : CtrlEditorBase<IViewEditor<UserRegisterDto>, UserRegisterDto>
{
    #region Constructor

    public UserRegisterCtrl(Store store) : base(store)
    {
        ControllerName = "User register";
    }

    #endregion

    #region Controller editor implementation

    protected override IViewEditor<UserRegisterDto> CreateView()
    {
        return Store.FactoryViews.Registry.Resolve<UserRegisterCtrl, IViewEditor<UserRegisterDto>>(this);
    }

    public override Task<bool> LoadModelById(IKntService service, Guid id, bool refreshView = true)
    {
        throw new NotImplementedException();
    }

    public override Task<bool> NewModel(IKntService service)
    {
        Service = service;

        Model = new UserRegisterDto
        {
            UserName = Store.AppUserName,
            RoleDefinition = "Public"
        };

        return Task.FromResult(true);
    }

    public async override Task<bool> SaveModel()
    {
        View.RefreshModel();

        // UserDto.Validate (called by GetErrorMessage) only covers UserName/EMail/FullName: Password is
        // declared on UserRegisterDto and has no Validate override of its own, so it's checked here.
        var msgVal = Model.GetErrorMessage();
        if (string.IsNullOrWhiteSpace(Model.Password))
            msgVal += "Password is required.\n";
        if (!string.IsNullOrEmpty(msgVal))
        {
            View.ShowInfo(msgVal);
            return false;
        }

        try
        {
            var result = await Service.Users.CreateAsync(Model);
            if (result.IsValid)
            {
                Model.SetIsDirty(false);
                OnAddedEntity(Model);
                Finalize();
                return true;
            }
            else
            {
                View.ShowInfo(result.ErrorMessage);
                return false;
            }
        }
        catch (Exception ex)
        {
            View.ShowInfo(RootExceptionMessage(ex));
            return false;
        }
    }

    public override Task<bool> DeleteModel(IKntService service, Guid id)
    {
        throw new NotImplementedException();
    }

    public override Task<bool> DeleteModel()
    {
        throw new NotImplementedException();
    }

    #endregion
}

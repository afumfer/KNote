using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

/// <summary>
/// Signs the user in at startup when the application is set to identify its user with a KNote user name and
/// password (SecurityConfig.AuthenticationMode = Credentials) instead of the Windows account. It only
/// starts the session (Store.AppUserName, Store.Security): the credentials are checked against each
/// repository afterwards, as each one is linked (Store.AuthenticateRepositoryAsync), since every repository
/// has its own Users table. The user can also go back to the Windows account from here.
/// </summary>
public class LoginCtrl : CtrlEditorBase<IViewEditor<LoginModel>, LoginModel>
{
    #region Constructor

    public LoginCtrl(Store store) : base(store)
    {
        ControllerName = "Sign in";
    }

    #endregion

    #region Properties

    // The Windows account running the application, offered as the alternative to a KNote user.
    public string WindowsUserName { get; set; } = SystemInformation.UserName;

    #endregion

    #region Controller editor implementation

    protected override IViewEditor<LoginModel> CreateView()
    {
        return Store.FactoryViews.Registry.Resolve<LoginCtrl, IViewEditor<LoginModel>>(this);
    }

    public override Task<bool> LoadModelById(IKntService service, Guid id, bool refreshView = true)
    {
        throw new NotImplementedException();
    }

    public override Task<bool> NewModel(IKntService service = null)
    {
        Model = new LoginModel { UserName = Store.State.Session.LastUserName };
        Model.SetIsDirty(false);
        return Task.FromResult(true);
    }

    public override Task<bool> SaveModel()
    {
        View.RefreshModel();

        var msgVal = Model.GetErrorMessage(false);
        if (!string.IsNullOrEmpty(msgVal))
        {
            View.ShowInfo(msgVal);
            return Task.FromResult(false);
        }

        var userName = Model.UserName.Trim();
        Store.AppUserName = userName;
        Store.Security.StartSession(AppAuthenticationMode.Credentials, Model.Password);
        Store.State.Session.LastUserName = userName;

        return Task.FromResult(true);
    }

    /// <summary>
    /// Signs in with the Windows account instead, now and from now on: also switches the setting back, so
    /// the sign-in dialog isn't shown again on the next start.
    /// </summary>
    public void UseWindowsAccount()
    {
        Store.Settings.Security.AuthenticationMode = AppAuthenticationMode.Windows;
        Store.AppUserName = WindowsUserName;
        Store.Security.StartSession(AppAuthenticationMode.Windows);
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

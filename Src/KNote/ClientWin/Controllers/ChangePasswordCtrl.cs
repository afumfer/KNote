using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

/// <summary>
/// The user changes their own password (Users.ChangePasswordAsync), in every linked repository: the user exists once
/// in each one, and signed in with a KNote user the same password must open all of them. A repository where it can't
/// be changed (another current password there, a disabled user) doesn't stop the others, and the user is told which.
/// </summary>
[KntAuthorize(EnumRoles.Guest, AuthorizationScope.Application)]
public class ChangePasswordCtrl : CtrlEditorBase<IViewEditor<ChangePasswordModel>, ChangePasswordModel>
{
    #region Constructor

    public ChangePasswordCtrl(Store store) : base(store)
    {
        ControllerName = "Change password";
    }

    #endregion

    #region Properties

    // Where the password changes, for the view.
    public string RepositoriesText => string.Join(", ", Store.GetAllServiceRef().Select(s => s.Name));

    public string PasswordPolicyText => $"At least {KntPasswordPolicy.MinLength} characters, not your user name.";

    #endregion

    #region Controller editor implementation

    protected override IViewEditor<ChangePasswordModel> CreateView()
    {
        return Store.FactoryViews.Registry.Resolve<ChangePasswordCtrl, IViewEditor<ChangePasswordModel>>(this);
    }

    public override Task<bool> LoadModelById(IKntService service, Guid id, bool refreshView = true)
    {
        throw new NotImplementedException();
    }

    public override Task<bool> NewModel(IKntService service = null)
    {
        Service = service;
        Model = new ChangePasswordModel();
        Model.SetIsDirty(false);
        return Task.FromResult(true);
    }

    public override async Task<bool> SaveModel()
    {
        View.RefreshModel();

        var msgVal = Model.GetErrorMessage(false);
        if (string.IsNullOrEmpty(msgVal))
            msgVal = KntPasswordPolicy.Validate(Model.NewPassword, Store.AppUserName);
        if (!string.IsNullOrEmpty(msgVal))
        {
            View.ShowInfo(msgVal);
            return false;
        }

        var services = Store.GetAllServiceRef().Select(s => s.Service).ToList();
        if (services.Count == 0)
        {
            View.ShowInfo("There are no linked repositories: your user, and its password, belong to each repository.");
            return false;
        }

        var passwords = new UserChangePasswordDto { CurrentPassword = Model.CurrentPassword, NewPassword = Model.NewPassword };
        var operation = await RepositoriesOperation<Result<UserDto>>.RunAsync(services, s => s.Users.ChangePasswordAsync(passwords));

        if (operation.Done.Count == 0)
        {
            View.ShowInfo($"The password was not changed:{Environment.NewLine}{operation.FailedText()}");
            return false;
        }

        Store.Security.PasswordChanged(Model.NewPassword);
        View.ShowInfo(SavedInfo(operation));

        Model.SetIsDirty(false);
        OnSavedEntity(Model);
        Finalize();
        return true;
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

    #region Private methods

    private string SavedInfo(RepositoriesOperation<Result<UserDto>> operation)
    {
        var lines = new List<string> { $"The password was changed in: {operation.DoneText()}." };

        if (operation.Failed.Count > 0)
        {
            lines.Add($"It was not changed in:{Environment.NewLine}{operation.FailedText()}");
            lines.Add(Store.Security.AuthenticationMode == AppAuthenticationMode.Credentials
                ? "Those repositories keep the old password, so they won't open in your next session until it is the same " +
                  "(an administrator of each one can set it in its users)."
                : "Those repositories keep the old password.");
        }

        // The chat room's server may use one of these databases: its token, issued with the old password, is refused now.
        if (!string.IsNullOrEmpty(Store.Settings.Connectivity.ChatHub.Token))
            lines.Add("If the chat room's server uses one of these repositories, sign in to it again in Options > Chat room.");

        return string.Join(Environment.NewLine + Environment.NewLine, lines);
    }

    #endregion
}

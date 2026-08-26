using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

/// <summary>
/// Single user add/edit popup, used by UsersManageCtrl (repository administration - Users tab).
/// Like NoteTypeEditorCtrl/AttributeEditorCtrl, there is no parent "Save" to stage into: everything
/// persists immediately (AutoDBSave stays true, the CtrlEditorBase default).
///
/// Model stays a plain UserDto throughout (matching Service.Users.GetAsync/SaveAsync/DeleteAsync
/// exactly) even though creating a user needs a password: NewUserPassword carries that separately,
/// and SaveModel builds a one-off UserRegisterDto from Model + NewUserPassword only for the
/// Service.Users.CreateAsync call, reusing the same command UserRegisterCtrl already uses for the
/// Windows-identity self-registration flow (fixes a real Blazor gap: users created there via
/// SaveAsync's plain UserDto never got a password and couldn't log in).
/// </summary>
public class UserEditorCtrl : CtrlEditorBase<IViewEditor<UserDto>, UserDto>
{
    #region Properties

    /// <summary>Only read when Model.UserId == Guid.Empty (new user); ignored otherwise.</summary>
    public string NewUserPassword { get; set; }

    #endregion

    #region Constructor

    public UserEditorCtrl(Store store) : base(store)
    {
        ControllerName = "User editor";
    }

    #endregion

    #region Controller editor implementation

    protected override IViewEditor<UserDto> CreateView()
    {
        return Store.FactoryViews.Registry.Resolve<UserEditorCtrl, IViewEditor<UserDto>>(this);
    }

    public override async Task<bool> LoadModelById(IKntService service, Guid id, bool refreshView = true)
    {
        try
        {
            Service = service;

            Model = (await Service.Users.GetAsync(id)).Entity;
            Model.SetIsDirty(false);
            NewUserPassword = null;

            if (refreshView)
                View.RefreshView();
            return true;
        }
        catch (Exception ex)
        {
            View.ShowInfo(ex.Message);
            return false;
        }
    }

    public override Task<bool> NewModel(IKntService service)
    {
        Service = service;

        Model = new UserDto { RoleDefinition = nameof(EnumRoles.Public) };
        NewUserPassword = null;

        return Task.FromResult(true);
    }

    public async override Task<bool> SaveModel()
    {
        View.RefreshModel();

        var isNew = Model.UserId == Guid.Empty;

        if (!isNew && !Model.IsDirty())
            return true;

        var msgVal = Model.GetErrorMessage();
        if (isNew && string.IsNullOrWhiteSpace(NewUserPassword))
            msgVal += "Password is required.\n";
        if (!string.IsNullOrEmpty(msgVal))
        {
            View.ShowInfo(msgVal);
            return false;
        }

        try
        {
            if (isNew)
            {
                var registerDto = new UserRegisterDto
                {
                    UserName = Model.UserName,
                    EMail = Model.EMail,
                    FullName = Model.FullName,
                    RoleDefinition = Model.RoleDefinition,
                    Disabled = Model.Disabled,
                    Password = NewUserPassword
                };

                var response = await Service.Users.CreateAsync(registerDto);
                if (response.IsValid)
                {
                    Model = response.Entity;
                    Model.SetIsDirty(false);
                    OnAddedEntity(Model);
                    Finalize();
                    return true;
                }
                else
                {
                    View.ShowInfo(response.ErrorMessage);
                    return false;
                }
            }
            else
            {
                var response = await Service.Users.SaveAsync(Model);
                if (response.IsValid)
                {
                    Model = response.Entity;
                    Model.SetIsDirty(false);
                    OnSavedEntity(Model);
                    Finalize();
                    return true;
                }
                else
                {
                    View.ShowInfo(response.ErrorMessage);
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            // Duplicate UserName/EMail (KntUsersCreateAsyncCommand throws rather than returning an
            // invalid Result for those) fails this way - same unwrap as UserRegisterCtrl.SaveModel.
            View.ShowInfo(RootExceptionMessage(ex));
            return false;
        }
    }

    public async override Task<bool> DeleteModel(IKntService service, Guid id)
    {
        var result = View.ShowInfo("Are you sure you want to delete this user?", "Delete user", MessageBoxButtons.YesNo);
        if (result == DialogResult.Yes)
        {
            try
            {
                var response = await service.Users.DeleteAsync(id);

                if (response.IsValid)
                {
                    OnDeletedEntity(response.Entity);
                    return true;
                }
                else
                {
                    View.ShowInfo(response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                View.ShowInfo(RootExceptionMessage(ex));
            }
        }
        return false;
    }

    public async override Task<bool> DeleteModel()
    {
        return await DeleteModel(Service, Model.UserId);
    }

    #endregion

    #region Reset password (separate from Save - only meaningful for an existing user)

    public async Task<bool> ResetPassword(string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword))
        {
            View.ShowInfo("Password is required.");
            return false;
        }

        try
        {
            var response = await Service.Users.SetPasswordAsync(Model.UserId, newPassword);
            if (response.IsValid)
            {
                View.ShowInfo("Password updated.");
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
            View.ShowInfo(RootExceptionMessage(ex));
            return false;
        }
    }

    #endregion
}

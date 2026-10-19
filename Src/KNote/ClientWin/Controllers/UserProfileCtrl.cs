using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

/// <summary>
/// "My profile": the full name and the email of the user (Users.GetProfileAsync/SaveProfileAsync, always the current
/// user, Store.AppUserName). The user exists once in each linked repository: the profile is shown as it is in one of
/// them and saved, by default, in all of them. The user name is only shown; a new email asks for the current password.
/// </summary>
[KntAuthorize(EnumRoles.Guest, AuthorizationScope.Application)]
public class UserProfileCtrl : CtrlEditorBase<IViewEditor<UserProfileDto>, UserProfileDto>
{
    #region Constructor

    public UserProfileCtrl(Store store) : base(store)
    {
        ControllerName = "My profile";
    }

    #endregion

    #region Properties

    // Saved in every linked repository (the default) or only in the one shown.
    public bool ApplyToAllRepositories { get; set; } = true;

    public string RepositoryName => Service?.RepositoryRef?.Name;

    public int RepositoriesCount => Store.GetAllServiceRef().Count;

    // The email as it was loaded: changing it asks for the current password.
    public string LoadedEMail { get; private set; }

    public bool IsEMailChanged(string eMail)
        => !string.Equals(eMail?.Trim(), LoadedEMail, StringComparison.OrdinalIgnoreCase);

    #endregion

    #region Controller editor implementation

    protected override IViewEditor<UserProfileDto> CreateView()
    {
        return Store.FactoryViews.Registry.Resolve<UserProfileCtrl, IViewEditor<UserProfileDto>>(this);
    }

    /// <summary>
    /// The profile as it is in the repository of service. Not valid when the user isn't there (or is disabled):
    /// the caller tells the user, as this use case hasn't started yet.
    /// </summary>
    public async Task<Result> LoadModelAsync(IKntService service)
    {
        Service = service;
        var result = new Result();

        try
        {
            var resProfile = await service.Users.GetProfileAsync();
            if (!resProfile.IsValid)
            {
                result.AddListErrorMessage(resProfile.ListErrorMessage);
                return result;
            }

            Model = resProfile.Entity;
            Model.SetIsDirty(false);
            LoadedEMail = Model.EMail;
        }
        catch (Exception ex)
        {
            result.AddErrorMessage(RootExceptionMessage(ex));
        }

        return result;
    }

    public override Task<bool> LoadModelById(IKntService service, Guid id, bool refreshView = true)
    {
        throw new NotImplementedException();
    }

    public override Task<bool> NewModel(IKntService service = null)
    {
        throw new NotImplementedException();
    }

    public override async Task<bool> SaveModel()
    {
        View.RefreshModel();

        var msgVal = Model.GetErrorMessage();
        if (string.IsNullOrEmpty(msgVal) && IsEMailChanged(Model.EMail) && string.IsNullOrEmpty(Model.CurrentPassword))
            msgVal = "Enter your current password to change the email.";
        if (!string.IsNullOrEmpty(msgVal))
        {
            View.ShowInfo(msgVal);
            return false;
        }

        if (!Model.IsDirty())
        {
            Finalize();
            return true;
        }

        var services = ApplyToAllRepositories
            ? Store.GetAllServiceRef().Select(s => s.Service).ToList()
            : new List<IKntService> { Service };
        var operation = await RepositoriesOperation<Result<UserProfileDto>>.RunAsync(services, s => s.Users.SaveProfileAsync(Model));

        if (operation.Done.Count == 0)
        {
            View.ShowInfo($"The profile was not saved:{Environment.NewLine}{operation.FailedText()}");
            return false;
        }

        if (operation.Failed.Count > 0)
            View.ShowInfo($"The profile was saved in: {operation.DoneText()}.{Environment.NewLine}{Environment.NewLine}" +
                $"It was not saved in:{Environment.NewLine}{operation.FailedText()}");

        Model = (operation.ResultOf(Service) ?? operation.Done[0].Result).Entity;
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
}

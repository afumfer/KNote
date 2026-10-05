using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Tests for Store.AuthenticateRepositoryAsync: whether the session's user may use a repository before it
/// is linked, and its role there (Store.Security). Signed in with the Windows account, being registered is
/// enough; signed in with a KNote user name, the session's password must also match. An unregistered user
/// is offered the registration dialog (UserRegisterCtrl).
/// </summary>
[TestClass]
public class StoreAuthenticationTests
{
    private static (Store store, FakeUserRegisterView registerView, FakeKntService service) CreateStore(
        AppAuthenticationMode mode = AppAuthenticationMode.Windows, string password = null)
    {
        var factoryViews = new TestFactoryViews();
        var registerView = new FakeUserRegisterView();
        factoryViews.Registry.Register<UserRegisterCtrl, IViewEditor<UserRegisterDto>>(c => registerView);

        var store = new Store(factoryViews) { AppUserName = "jdoe" };
        store.Security.StartSession(mode, password);
        var service = new FakeKntService { CurrentUser = null };

        return (store, registerView, service);
    }

    private static UserDto User(string roles, bool disabled = false)
        => new() { UserId = Guid.NewGuid(), UserName = "jdoe", RoleDefinition = roles, Disabled = disabled };

    private static void RegistrationNotExpected(FakeUserRegisterView view)
        => view.ShowModalViewImpl = () => throw new InvalidOperationException("The registration dialog should not be shown.");

    [TestMethod]
    public async Task Windows_RegisteredUser_IsAuthenticatedWithItsRole()
    {
        var (store, registerView, service) = CreateStore();
        service.CurrentUser = User("Staff");
        RegistrationNotExpected(registerView);

        var result = await store.AuthenticateRepositoryAsync(service);

        Assert.IsTrue(result.IsValid, result.ErrorMessage);
        Assert.AreEqual(EnumRoles.Staff, store.Security.GetRepositoryRole(service));
    }

    [TestMethod]
    public async Task Windows_UnregisteredUser_WhoRegisters_IsAuthenticated()
    {
        var (store, registerView, service) = CreateStore();
        registerView.ShowModalViewImpl = () =>
        {
            service.CurrentUser = User("Admin"); // what RegisterAsync would have stored
            return new Result<EControllerResult>(EControllerResult.Executed);
        };

        var result = await store.AuthenticateRepositoryAsync(service);

        Assert.IsTrue(result.IsValid, result.ErrorMessage);
        Assert.AreEqual(EnumRoles.Admin, store.Security.GetRepositoryRole(service));
    }

    [TestMethod]
    public async Task UnregisteredUser_WhoCancelsTheRegistration_IsRefused()
    {
        var (store, registerView, service) = CreateStore();
        registerView.ShowModalViewImpl = () => new Result<EControllerResult>(EControllerResult.Canceled);

        var result = await store.AuthenticateRepositoryAsync(service);

        Assert.IsFalse(result.IsValid);
        StringAssert.Contains(result.ErrorMessage, "not registered");
        Assert.IsNull(store.Security.GetRepositoryRole(service));
    }

    [TestMethod]
    public async Task DisabledUser_IsRefused()
    {
        var (store, registerView, service) = CreateStore();
        service.CurrentUser = User("Admin", disabled: true);
        RegistrationNotExpected(registerView);

        var result = await store.AuthenticateRepositoryAsync(service);

        Assert.IsFalse(result.IsValid);
        StringAssert.Contains(result.ErrorMessage, "disabled");
    }

    [TestMethod]
    public async Task UserWithoutAnyKnownRole_IsRefused()
    {
        var (store, _, service) = CreateStore();
        service.CurrentUser = User("Public"); // an old role name that no longer means anything

        var result = await store.AuthenticateRepositoryAsync(service);

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public async Task Credentials_RightPassword_IsAuthenticated()
    {
        var (store, registerView, service) = CreateStore(AppAuthenticationMode.Credentials, "secret");
        service.CurrentUser = User("Guest");
        UserCredentialsDto sent = null;
        service.UsersFake.AuthenticateAsyncImpl = c => { sent = c; return Task.FromResult(new Result<UserDto>(service.CurrentUser)); };
        RegistrationNotExpected(registerView);

        var result = await store.AuthenticateRepositoryAsync(service);

        Assert.IsTrue(result.IsValid, result.ErrorMessage);
        Assert.AreEqual("jdoe", sent.UserName);
        Assert.AreEqual("secret", sent.Password);
        Assert.AreEqual(EnumRoles.Guest, store.Security.GetRepositoryRole(service));
    }

    [TestMethod]
    public async Task Credentials_WrongPassword_IsRefusedWithoutOfferingTheRegistration()
    {
        var (store, registerView, service) = CreateStore(AppAuthenticationMode.Credentials, "wrong");
        service.CurrentUser = User("Admin");
        service.UsersFake.AuthenticateAsyncImpl = _ =>
        {
            var res = new Result<UserDto>();
            res.AddErrorMessage("User not authenticated");
            return Task.FromResult(res);
        };
        RegistrationNotExpected(registerView);

        var result = await store.AuthenticateRepositoryAsync(service);

        Assert.IsFalse(result.IsValid);
        StringAssert.Contains(result.ErrorMessage, "password");
        Assert.IsNull(store.Security.GetRepositoryRole(service));
    }

    [TestMethod]
    public async Task Credentials_UserWithoutPassword_IsRefused()
    {
        // A user created from the users management screen may have no password yet: checking it throws.
        var (store, _, service) = CreateStore(AppAuthenticationMode.Credentials, "secret");
        service.CurrentUser = User("Admin");
        service.UsersFake.AuthenticateAsyncImpl = _ => throw new ArgumentException("Invalid length of password hash (64 bytes expected).");

        var result = await store.AuthenticateRepositoryAsync(service);

        Assert.IsFalse(result.IsValid);
        StringAssert.Contains(result.ErrorMessage, "password");
    }

    [TestMethod]
    public async Task Windows_DoesNotCheckAnyPassword()
    {
        var (store, _, service) = CreateStore(AppAuthenticationMode.Windows);
        service.CurrentUser = User("Admin");
        service.UsersFake.AuthenticateAsyncImpl = _ => throw new InvalidOperationException("No password to check with the Windows account.");

        var result = await store.AuthenticateRepositoryAsync(service);

        Assert.IsTrue(result.IsValid, result.ErrorMessage);
    }

    [TestMethod]
    public async Task RefreshRepositoryRole_FollowsTheUsersCurrentRole()
    {
        var (store, _, service) = CreateStore();
        service.CurrentUser = User("Admin");
        await store.AuthenticateRepositoryAsync(service);

        service.CurrentUser = User("Staff");
        await store.RefreshRepositoryRoleAsync(service);
        Assert.AreEqual(EnumRoles.Staff, store.Security.GetRepositoryRole(service));

        service.CurrentUser = User("Staff", disabled: true);
        await store.RefreshRepositoryRoleAsync(service);
        Assert.IsNull(store.Security.GetRepositoryRole(service));
    }
}

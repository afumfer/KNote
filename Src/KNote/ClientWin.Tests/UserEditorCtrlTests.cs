using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Tests for UserEditorCtrl: the single user add/edit popup used by UsersManageCtrl (repository
/// administration - Users tab). Model stays a plain UserDto throughout; NewUserPassword carries the
/// password separately and is only used to build a one-off UserRegisterDto for Service.Users.CreateAsync
/// when adding a new user - fixing the real Blazor gap where an Admin-created user never got a password.
/// </summary>
[TestClass]
public class UserEditorCtrlTests
{
    private static (UserEditorCtrl ctrl, FakeUserEditorView view, FakeKntService service) CreateCtrl()
    {
        var factoryViews = new TestFactoryViews();
        var view = new FakeUserEditorView();
        factoryViews.Registry.Register<UserEditorCtrl, IViewEditor<UserDto>>(c => view);

        var store = new Store(factoryViews) { AppUserName = "jdoe" };
        var ctrl = new UserEditorCtrl(store);
        var service = new FakeKntService();

        return (ctrl, view, service);
    }

    [TestMethod]
    public async Task NewModel_CreatesEmptyModel_WithPublicRole()
    {
        var (ctrl, _, service) = CreateCtrl();

        await ctrl.NewModel(service);

        Assert.AreEqual(Guid.Empty, ctrl.Model.UserId);
        Assert.AreEqual("Public", ctrl.Model.RoleDefinition);
    }

    [TestMethod]
    public async Task SaveModel_NewUser_MissingPassword_ReturnsFalseWithoutCallingService()
    {
        var (ctrl, view, service) = CreateCtrl();
        await ctrl.NewModel(service);
        ctrl.Model.UserName = "jdoe";
        ctrl.Model.EMail = "jdoe@example.com";
        ctrl.Model.FullName = "John Doe";
        // NewUserPassword intentionally left empty.

        var createCalled = false;
        service.UsersFake.CreateAsyncImpl = u => { createCalled = true; return Task.FromResult(new Result<UserDto>(u)); };

        var saved = await ctrl.SaveModel();

        Assert.IsFalse(saved);
        Assert.IsFalse(createCalled);
        Assert.IsFalse(string.IsNullOrEmpty(view.LastShownInfo));
    }

    [TestMethod]
    public async Task SaveModel_NewUser_CallsCreateAsync_ReturnsTrue_FiresAddedEntity()
    {
        var (ctrl, _, service) = CreateCtrl();
        await ctrl.NewModel(service);
        ctrl.Model.UserName = "jdoe";
        ctrl.Model.EMail = "jdoe@example.com";
        ctrl.Model.FullName = "John Doe";
        ctrl.NewUserPassword = "secret";

        UserRegisterDto? sentRegisterDto = null;
        service.UsersFake.CreateAsyncImpl = u =>
        {
            sentRegisterDto = u;
            var created = new UserDto { UserId = Guid.NewGuid(), UserName = u.UserName, EMail = u.EMail, FullName = u.FullName, RoleDefinition = u.RoleDefinition };
            return Task.FromResult(new Result<UserDto>(created));
        };
        UserDto? addedEntity = null;
        ctrl.AddedEntity += (s, e) => addedEntity = e.Entity;

        var saved = await ctrl.SaveModel();

        Assert.IsTrue(saved);
        Assert.IsNotNull(addedEntity);
        Assert.AreNotEqual(Guid.Empty, addedEntity!.UserId);
        Assert.IsNotNull(sentRegisterDto);
        Assert.AreEqual("secret", sentRegisterDto!.Password);
    }

    [TestMethod]
    public async Task SaveModel_NewUser_ServiceThrowsWrappedException_ShowsRootExceptionMessage()
    {
        var (ctrl, view, service) = CreateCtrl();
        await ctrl.NewModel(service);
        ctrl.Model.UserName = "jdoe";
        ctrl.Model.EMail = "jdoe@example.com";
        ctrl.Model.FullName = "John Doe";
        ctrl.NewUserPassword = "secret";

        // KntUsersCreateAsyncCommand throws a plain Exception (not an invalid Result) for a
        // duplicate username/email.
        service.UsersFake.CreateAsyncImpl = _ => throw new Exception(
            "KNote service error. (KntUsersCreateAsyncCommand). ",
            new Exception("Username \"jdoe\" is already taken"));

        var saved = await ctrl.SaveModel();

        Assert.IsFalse(saved);
        Assert.AreEqual("Username \"jdoe\" is already taken", view.LastShownInfo);
    }

    [TestMethod]
    public async Task SaveModel_ExistingUser_NotDirty_ReturnsTrueWithoutCallingService()
    {
        var (ctrl, _, service) = CreateCtrl();
        var existingId = Guid.NewGuid();
        service.UsersFake.GetAsyncImpl = _ => Task.FromResult(new Result<UserDto>(new UserDto { UserId = existingId, UserName = "jdoe" }));
        await ctrl.LoadModelById(service, existingId, false);

        var saveCalled = false;
        service.UsersFake.SaveAsyncImpl = u => { saveCalled = true; return Task.FromResult(new Result<UserDto>(u)); };

        var saved = await ctrl.SaveModel();

        Assert.IsTrue(saved);
        Assert.IsFalse(saveCalled);
    }

    [TestMethod]
    public async Task SaveModel_ExistingUser_CallsSaveAsync_ReturnsTrue_FiresSavedEntity()
    {
        var (ctrl, _, service) = CreateCtrl();
        var existingId = Guid.NewGuid();
        service.UsersFake.GetAsyncImpl = _ => Task.FromResult(new Result<UserDto>(new UserDto { UserId = existingId, UserName = "jdoe", EMail = "jdoe@example.com", FullName = "John Doe" }));
        await ctrl.LoadModelById(service, existingId, false);
        ctrl.Model.FullName = "John Doe Jr.";

        service.UsersFake.SaveAsyncImpl = u => Task.FromResult(new Result<UserDto>(u));
        UserDto? savedEntity = null;
        UserDto? addedEntity = null;
        ctrl.SavedEntity += (s, e) => savedEntity = e.Entity;
        ctrl.AddedEntity += (s, e) => addedEntity = e.Entity;

        var saved = await ctrl.SaveModel();

        Assert.IsTrue(saved);
        Assert.IsNotNull(savedEntity);
        Assert.IsNull(addedEntity);
    }

    [TestMethod]
    public async Task SaveModel_ExistingUser_ServiceReturnsInvalidResult_ReturnsFalseAndShowsError()
    {
        var (ctrl, view, service) = CreateCtrl();
        var existingId = Guid.NewGuid();
        service.UsersFake.GetAsyncImpl = _ => Task.FromResult(new Result<UserDto>(new UserDto { UserId = existingId, UserName = "jdoe", EMail = "jdoe@example.com", FullName = "John Doe" }));
        await ctrl.LoadModelById(service, existingId, false);
        ctrl.Model.FullName = "John Doe Jr.";

        var invalidResult = new Result<UserDto>();
        invalidResult.AddErrorMessage("Email already in use");
        service.UsersFake.SaveAsyncImpl = _ => Task.FromResult(invalidResult);

        var saved = await ctrl.SaveModel();

        Assert.IsFalse(saved);
        Assert.AreEqual("Email already in use", view.LastShownInfo);
    }

    [TestMethod]
    public async Task DeleteModel_Confirmed_CallsDeleteAsync_ReturnsTrue_FiresDeletedEntity()
    {
        var (ctrl, view, service) = CreateCtrl();
        view.NextShowInfoResult = DialogResult.Yes;
        var id = Guid.NewGuid();
        service.UsersFake.DeleteAsyncImpl = _ => Task.FromResult(new Result<UserDto>(new UserDto { UserId = id }));
        UserDto? deletedEntity = null;
        ctrl.DeletedEntity += (s, e) => deletedEntity = e.Entity;

        var deleted = await ctrl.DeleteModel(service, id);

        Assert.IsTrue(deleted);
        Assert.IsNotNull(deletedEntity);
    }

    [TestMethod]
    public async Task DeleteModel_NotConfirmed_DoesNotCallDeleteAsync_ReturnsFalse()
    {
        var (ctrl, view, service) = CreateCtrl();
        view.NextShowInfoResult = DialogResult.No;
        var deleteCalled = false;
        service.UsersFake.DeleteAsyncImpl = id => { deleteCalled = true; return Task.FromResult(new Result<UserDto>(new UserDto { UserId = id })); };

        var deleted = await ctrl.DeleteModel(service, Guid.NewGuid());

        Assert.IsFalse(deleted);
        Assert.IsFalse(deleteCalled);
    }

    [TestMethod]
    public async Task ResetPassword_EmptyPassword_ReturnsFalseWithoutCallingService()
    {
        var (ctrl, view, service) = CreateCtrl();
        await ctrl.NewModel(service);

        var setPasswordCalled = false;
        service.UsersFake.SetPasswordAsyncImpl = (id, pwd) => { setPasswordCalled = true; return Task.FromResult(new Result<UserDto>(new UserDto { UserId = id })); };

        var result = await ctrl.ResetPassword("   ");

        Assert.IsFalse(result);
        Assert.IsFalse(setPasswordCalled);
        Assert.IsFalse(string.IsNullOrEmpty(view.LastShownInfo));
    }

    [TestMethod]
    public async Task ResetPassword_Success_CallsSetPasswordAsync_ReturnsTrue()
    {
        var (ctrl, _, service) = CreateCtrl();
        var existingId = Guid.NewGuid();
        service.UsersFake.GetAsyncImpl = _ => Task.FromResult(new Result<UserDto>(new UserDto { UserId = existingId, UserName = "jdoe" }));
        await ctrl.LoadModelById(service, existingId, false);

        Guid? passedId = null;
        string? passedPassword = null;
        service.UsersFake.SetPasswordAsyncImpl = (id, pwd) =>
        {
            passedId = id;
            passedPassword = pwd;
            return Task.FromResult(new Result<UserDto>(new UserDto { UserId = id }));
        };

        var result = await ctrl.ResetPassword("newSecret");

        Assert.IsTrue(result);
        Assert.AreEqual(existingId, passedId);
        Assert.AreEqual("newSecret", passedPassword);
    }

    [TestMethod]
    public async Task ResetPassword_ServiceReturnsInvalidResult_ReturnsFalseAndShowsError()
    {
        var (ctrl, view, service) = CreateCtrl();
        var existingId = Guid.NewGuid();
        service.UsersFake.GetAsyncImpl = _ => Task.FromResult(new Result<UserDto>(new UserDto { UserId = existingId, UserName = "jdoe" }));
        await ctrl.LoadModelById(service, existingId, false);

        var invalidResult = new Result<UserDto>();
        invalidResult.AddErrorMessage("Something went wrong");
        service.UsersFake.SetPasswordAsyncImpl = (_, _) => Task.FromResult(invalidResult);

        var result = await ctrl.ResetPassword("newSecret");

        Assert.IsFalse(result);
        Assert.AreEqual("Something went wrong", view.LastShownInfo);
    }
}

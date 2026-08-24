using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Tests for UserRegisterCtrl: the use case that registers the current Windows user in a KNote
/// repository's Users table when it's missing there (see Store.EnsureCurrentUserRegistered).
/// </summary>
[TestClass]
public class UserRegisterCtrlTests
{
    private static (UserRegisterCtrl ctrl, FakeUserRegisterView view, FakeKntService service, Store store) CreateCtrl()
    {
        var factoryViews = new TestFactoryViews();
        var view = new FakeUserRegisterView();
        factoryViews.Registry.Register<UserRegisterCtrl, IViewEditor<UserRegisterDto>>(c => view);

        var store = new Store(factoryViews) { AppUserName = "jdoe" };
        var ctrl = new UserRegisterCtrl(store);
        var service = new FakeKntService();

        return (ctrl, view, service, store);
    }

    [TestMethod]
    public async Task NewModel_PrefillsUserNameFromStoreAppUserName_AndSetsPublicRole()
    {
        var (ctrl, _, service, store) = CreateCtrl();

        await ctrl.NewModel(service);

        Assert.AreEqual(store.AppUserName, ctrl.Model.UserName);
        Assert.AreEqual("Public", ctrl.Model.RoleDefinition);
    }

    [TestMethod]
    public async Task SaveModel_ValidData_CallsCreateAsync_ReturnsTrue_FiresAddedEntity()
    {
        var (ctrl, view, service, store) = CreateCtrl();
        await ctrl.NewModel(service);
        ctrl.Model.FullName = "John Doe";
        ctrl.Model.EMail = "john@doe.com";
        ctrl.Model.Password = "secret";

        service.UsersFake.CreateAsyncImpl = u => Task.FromResult(new Result<UserDto>(u));
        UserRegisterDto addedEntity = null;
        ctrl.AddedEntity += (s, e) => addedEntity = e.Entity;

        var saved = await ctrl.SaveModel();

        Assert.IsTrue(saved);
        Assert.IsNotNull(addedEntity);
        Assert.AreEqual("John Doe", addedEntity.FullName);
    }

    [TestMethod]
    public async Task SaveModel_MissingFullName_ReturnsFalseWithoutCallingService()
    {
        var (ctrl, view, service, store) = CreateCtrl();
        await ctrl.NewModel(service);
        ctrl.Model.EMail = "john@doe.com";
        ctrl.Model.Password = "secret";
        // FullName intentionally left empty.

        var createCalled = false;
        service.UsersFake.CreateAsyncImpl = u => { createCalled = true; return Task.FromResult(new Result<UserDto>(u)); };

        var saved = await ctrl.SaveModel();

        Assert.IsFalse(saved);
        Assert.IsFalse(createCalled);
        Assert.IsFalse(string.IsNullOrEmpty(view.LastShownInfo));
    }

    [TestMethod]
    public async Task SaveModel_ServiceReturnsInvalidResult_ReturnsFalseAndShowsError()
    {
        var (ctrl, view, service, store) = CreateCtrl();
        await ctrl.NewModel(service);
        ctrl.Model.FullName = "John Doe";
        ctrl.Model.EMail = "john@doe.com";
        ctrl.Model.Password = "secret";

        var invalidResult = new Result<UserDto>();
        invalidResult.AddErrorMessage("Username \"jdoe\" is already taken");
        service.UsersFake.CreateAsyncImpl = _ => Task.FromResult(invalidResult);

        var saved = await ctrl.SaveModel();

        Assert.IsFalse(saved);
        Assert.AreEqual("Username \"jdoe\" is already taken", view.LastShownInfo);
    }

    [TestMethod]
    public async Task SaveModel_MissingPassword_ReturnsFalseWithoutCallingService()
    {
        var (ctrl, view, service, store) = CreateCtrl();
        await ctrl.NewModel(service);
        ctrl.Model.FullName = "John Doe";
        ctrl.Model.EMail = "john@doe.com";
        // Password intentionally left empty. UserDto.Validate doesn't cover it (Password is declared
        // on UserRegisterDto), so SaveModel checks it explicitly before calling the service.

        var createCalled = false;
        service.UsersFake.CreateAsyncImpl = u => { createCalled = true; return Task.FromResult(new Result<UserDto>(u)); };

        var saved = await ctrl.SaveModel();

        Assert.IsFalse(saved);
        Assert.IsFalse(createCalled);
        Assert.IsFalse(string.IsNullOrEmpty(view.LastShownInfo));
    }

    [TestMethod]
    public async Task SaveModel_ServiceThrowsSingleWrappedException_ShowsInnerExceptionMessage()
    {
        var (ctrl, view, service, store) = CreateCtrl();
        await ctrl.NewModel(service);
        ctrl.Model.FullName = "John Doe";
        ctrl.Model.EMail = "john@doe.com";
        ctrl.Model.Password = "secret";

        // KntServiceBase.ExecuteCommand wraps every exception thrown by a command (e.g. duplicate
        // username) into a generic KntServiceException with the real cause as InnerException.
        service.UsersFake.CreateAsyncImpl = _ => throw new Exception(
            "KNote service error. (KntUsersCreateAsyncCommand). ",
            new Exception("Username \"jdoe\" is already taken"));

        var saved = await ctrl.SaveModel();

        Assert.IsFalse(saved);
        Assert.AreEqual("Username \"jdoe\" is already taken", view.LastShownInfo);
    }

    [TestMethod]
    public async Task SaveModel_ServiceThrowsDoublyWrappedException_ShowsRootExceptionMessage()
    {
        var (ctrl, view, service, store) = CreateCtrl();
        await ctrl.NewModel(service);
        ctrl.Model.FullName = "John Doe";
        ctrl.Model.EMail = "john@doe.com";
        ctrl.Model.Password = "secret";

        // A DB error is first wrapped by KntUserRepository.AddInternalAsync into a generic
        // KntRepositoryException ("KNote repository error. (...)"), and that is wrapped again by
        // KntServiceBase.ExecuteCommand into a KntServiceException ("KNote service error. (...)").
        // SaveModel must unwrap both layers to reach the real cause, not stop at the first one.
        service.UsersFake.CreateAsyncImpl = _ => throw new Exception(
            "KNote service error. (KntUsersCreateAsyncCommand). ",
            new Exception(
                "KNote repository error. (KNote.Repository.Dapper.KntUserRepository)",
                new Exception("UNIQUE constraint failed: Users.UserName")));

        var saved = await ctrl.SaveModel();

        Assert.IsFalse(saved);
        Assert.AreEqual("UNIQUE constraint failed: Users.UserName", view.LastShownInfo);
    }
}

using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Tests for Store.EnsureCurrentUserRegistered: the orchestration that checks whether the current
/// Windows user is registered in a repository's Users table and, if not, shows the UserRegisterCtrl
/// registration dialog. Cancelling that dialog is not blocking (see UserRegisterCtrl.SaveModel).
/// </summary>
[TestClass]
public class StoreEnsureCurrentUserRegisteredTests
{
    private static (Store store, FakeUserRegisterView view, FakeKntService service) CreateStore()
    {
        var factoryViews = new TestFactoryViews();
        var view = new FakeUserRegisterView();
        factoryViews.Registry.Register<UserRegisterCtrl, IViewEditor<UserRegisterDto>>(c => view);

        var store = new Store(factoryViews) { AppUserName = "jdoe" };
        var service = new FakeKntService();

        return (store, view, service);
    }

    [TestMethod]
    public async Task UserAlreadyExists_ReturnsTrueWithoutShowingDialog()
    {
        var (store, view, service) = CreateStore();
        service.UsersFake.GetByUserNameAsyncImpl = name =>
            Task.FromResult(new Result<UserDto>(new UserDto { UserId = Guid.NewGuid(), UserName = name }));
        view.ShowModalViewImpl = () => throw new InvalidOperationException("Dialog should not be shown when the user already exists.");

        var result = await store.EnsureCurrentUserRegistered(service);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task UserMissing_DialogAccepted_ReturnsTrue()
    {
        var (store, view, service) = CreateStore();
        service.UsersFake.GetByUserNameAsyncImpl = _ => Task.FromResult(new Result<UserDto>());
        view.ShowModalViewImpl = () => new Result<EControllerResult>(EControllerResult.Executed);

        var result = await store.EnsureCurrentUserRegistered(service);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task UserMissing_DialogCanceled_ReturnsFalse()
    {
        var (store, view, service) = CreateStore();
        service.UsersFake.GetByUserNameAsyncImpl = _ => Task.FromResult(new Result<UserDto>());
        view.ShowModalViewImpl = () => new Result<EControllerResult>(EControllerResult.Canceled);

        var result = await store.EnsureCurrentUserRegistered(service);

        Assert.IsFalse(result);
    }
}

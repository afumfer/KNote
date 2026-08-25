using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Tests for Store.IsCurrentUserAdminAsync: the check that gates the repository administration tabs
/// (Users, Note types, Attributes) in RepositoryEditorCtrl/RepositoryEditorForm.
/// </summary>
[TestClass]
public class StoreIsCurrentUserAdminAsyncTests
{
    private static (Store store, FakeKntService service) CreateStore()
    {
        var store = new Store(new TestFactoryViews()) { AppUserName = "jdoe" };
        var service = new FakeKntService();

        return (store, service);
    }

    [TestMethod]
    public async Task UserHasAdminRoleOnly_ReturnsTrue()
    {
        var (store, service) = CreateStore();
        service.UsersFake.GetByUserNameAsyncImpl = _ =>
            Task.FromResult(new Result<UserDto>(new UserDto { RoleDefinition = "Admin" }));

        Assert.IsTrue(await store.IsCurrentUserAdminAsync(service));
    }

    [TestMethod]
    public async Task UserHasAdminAmongSeveralRoles_ReturnsTrue()
    {
        var (store, service) = CreateStore();
        service.UsersFake.GetByUserNameAsyncImpl = _ =>
            Task.FromResult(new Result<UserDto>(new UserDto { RoleDefinition = "Staff, Admin" }));

        Assert.IsTrue(await store.IsCurrentUserAdminAsync(service));
    }

    [TestMethod]
    public async Task UserHasOnlyNonAdminRoles_ReturnsFalse()
    {
        var (store, service) = CreateStore();
        service.UsersFake.GetByUserNameAsyncImpl = _ =>
            Task.FromResult(new Result<UserDto>(new UserDto { RoleDefinition = "Public" }));

        Assert.IsFalse(await store.IsCurrentUserAdminAsync(service));
    }

    [TestMethod]
    public async Task UserNotRegisteredInRepository_ReturnsFalse()
    {
        var (store, service) = CreateStore();
        service.UsersFake.GetByUserNameAsyncImpl = _ => Task.FromResult(new Result<UserDto>());

        Assert.IsFalse(await store.IsCurrentUserAdminAsync(service));
    }

    [TestMethod]
    public async Task UserHasNullRoleDefinition_ReturnsFalse()
    {
        var (store, service) = CreateStore();
        service.UsersFake.GetByUserNameAsyncImpl = _ =>
            Task.FromResult(new Result<UserDto>(new UserDto { RoleDefinition = null }));

        Assert.IsFalse(await store.IsCurrentUserAdminAsync(service));
    }
}

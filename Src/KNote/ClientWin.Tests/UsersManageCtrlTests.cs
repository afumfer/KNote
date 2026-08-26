using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Tests for UsersManageCtrl: the Users tab of the repository administration screen
/// (RepositoryEditorCtrl). Covers the list load and the orchestration of the UserEditorCtrl popup
/// for add/edit/delete - UserEditorCtrl's own save/delete/reset-password logic is covered separately
/// in UserEditorCtrlTests.
/// </summary>
[TestClass]
public class UsersManageCtrlTests
{
    private static (UsersManageCtrl ctrl, FakeUsersManageView view, FakeKntService service) CreateCtrl(
        Func<UserEditorCtrl, Result<EControllerResult>>? editorShowModalViewImpl = null)
    {
        var factoryViews = new TestFactoryViews();
        var manageView = new FakeUsersManageView();
        factoryViews.Registry.Register<UsersManageCtrl, IViewManageList<UserDto>>(c => manageView);
        factoryViews.Registry.Register<UserEditorCtrl, IViewEditor<UserDto>>(c => new FakeUserEditorView
        {
            ShowModalViewImpl = editorShowModalViewImpl != null ? () => editorShowModalViewImpl(c) : null
        });

        var store = new Store(factoryViews) { AppUserName = "jdoe" };
        var ctrl = new UsersManageCtrl(store);
        var service = new FakeKntService();

        return (ctrl, manageView, service);
    }

    [TestMethod]
    public async Task LoadEntitiesAsync_Success_PopulatesListEntities_CallsRefreshView()
    {
        var (ctrl, view, service) = CreateCtrl();
        var users = new List<UserDto> { new() { UserName = "jdoe" }, new() { UserName = "asmith" } };
        service.UsersFake.GetAllAsyncImpl = _ => Task.FromResult(new Result<List<UserDto>>(users));

        var loaded = await ctrl.LoadEntitiesAsync(service);

        Assert.IsTrue(loaded);
        Assert.AreEqual(2, ctrl.ListEntities.Count);
        Assert.IsTrue(view.RefreshViewCalled);
    }

    [TestMethod]
    public async Task LoadEntitiesAsync_InvalidResult_ReturnsFalseAndShowsError()
    {
        var (ctrl, view, service) = CreateCtrl();
        var invalidResult = new Result<List<UserDto>>();
        invalidResult.AddErrorMessage("DB unavailable");
        service.UsersFake.GetAllAsyncImpl = _ => Task.FromResult(invalidResult);

        var loaded = await ctrl.LoadEntitiesAsync(service);

        Assert.IsFalse(loaded);
        Assert.AreEqual("DB unavailable", view.LastShownInfo);
    }

    [TestMethod]
    public async Task AddItemAsync_EditorExecuted_AddsToListEntitiesAndView()
    {
        var (ctrl, view, service) = CreateCtrl(editorCtrl =>
        {
            editorCtrl.Model.UserName = "jdoe";
            return new Result<EControllerResult>(EControllerResult.Executed);
        });
        service.UsersFake.GetAllAsyncImpl = _ => Task.FromResult(new Result<List<UserDto>>(new List<UserDto>()));
        await ctrl.LoadEntitiesAsync(service);

        var added = await ctrl.AddItemAsync();

        Assert.IsTrue(added);
        Assert.AreEqual(1, ctrl.ListEntities.Count);
        Assert.AreEqual("jdoe", ctrl.ListEntities[0].UserName);
        Assert.AreEqual(1, view.AddedItems.Count);
    }

    [TestMethod]
    public async Task AddItemAsync_EditorCanceled_DoesNotAddToListOrView()
    {
        var (ctrl, view, service) = CreateCtrl(_ => new Result<EControllerResult>(EControllerResult.Canceled));
        service.UsersFake.GetAllAsyncImpl = _ => Task.FromResult(new Result<List<UserDto>>(new List<UserDto>()));
        await ctrl.LoadEntitiesAsync(service);

        var added = await ctrl.AddItemAsync();

        Assert.IsFalse(added);
        Assert.AreEqual(0, ctrl.ListEntities.Count);
        Assert.AreEqual(0, view.AddedItems.Count);
    }

    [TestMethod]
    public async Task EditItemAsync_EditorExecuted_ReplacesInListEntitiesAndCallsViewUpdateItem()
    {
        var existingId = Guid.NewGuid();
        var existing = new UserDto { UserId = existingId, UserName = "jdoe" };

        var (ctrl, view, service) = CreateCtrl(editorCtrl =>
        {
            editorCtrl.Model.FullName = "John Doe";
            return new Result<EControllerResult>(EControllerResult.Executed);
        });
        service.UsersFake.GetAllAsyncImpl = _ => Task.FromResult(new Result<List<UserDto>>(new List<UserDto> { existing }));
        service.UsersFake.GetAsyncImpl = id => Task.FromResult(new Result<UserDto>(new UserDto { UserId = id, UserName = "jdoe" }));
        await ctrl.LoadEntitiesAsync(service);

        var edited = await ctrl.EditItemAsync(existing);

        Assert.IsTrue(edited);
        Assert.AreEqual(1, ctrl.ListEntities.Count);
        Assert.AreEqual("John Doe", ctrl.ListEntities[0].FullName);
        Assert.AreEqual(1, view.UpdatedItems.Count);
    }

    [TestMethod]
    public async Task DeleteItemAsync_Confirmed_RemovesFromListEntitiesAndCallsViewRemoveItem()
    {
        var existingId = Guid.NewGuid();
        var existing = new UserDto { UserId = existingId, UserName = "jdoe" };

        var (ctrl, view, service) = CreateCtrl();
        service.UsersFake.GetAllAsyncImpl = _ => Task.FromResult(new Result<List<UserDto>>(new List<UserDto> { existing }));
        service.UsersFake.DeleteAsyncImpl = id => Task.FromResult(new Result<UserDto>(new UserDto { UserId = id }));
        await ctrl.LoadEntitiesAsync(service);

        var deleted = await ctrl.DeleteItemAsync(existing);

        Assert.IsTrue(deleted);
        Assert.AreEqual(0, ctrl.ListEntities.Count);
        Assert.AreEqual(1, view.RemovedItems.Count);
    }
}

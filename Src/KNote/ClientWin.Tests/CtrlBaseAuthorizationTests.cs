using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.Model;
using KNote.Service.Core;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Tests for the controller-level authorization of CtrlBase: the [KntAuthorize] requirement of a controller
/// class is checked once, by CheckPreconditions (Run/RunModal) or CheckAccess, against the user's role
/// (Store.Security) - in the controller's repository or application-wide. A refused controller tells the
/// user (Store.AccessDeniedNotifier), is finalized, and never shows (or even creates) its view.
/// </summary>
[TestClass]
public class CtrlBaseAuthorizationTests
{
    private class OpenCtrl(Store store) : CtrlBase(store);

    [KntAuthorize(EnumRoles.Admin, AuthorizationScope.Application)]
    private class AdminOnlyCtrl : CtrlBase
    {
        public AdminOnlyCtrl(Store store) : base(store) { ControllerName = "Admin only"; }
    }

    [KntAuthorize(EnumRoles.Staff)]
    private class StaffInRepositoryCtrl : CtrlBase
    {
        public StaffInRepositoryCtrl(Store store) : base(store) { ControllerName = "Staff in repository"; }

        public IKntService Repository { get; set; }

        protected override IKntService AuthorizationResource => Repository;
    }

    [KntAuthorize(EnumRoles.Staff, AuthorizationScope.Application)]
    private class StaffViewCtrl : CtrlViewBase<IViewBase>
    {
        public int ViewsCreated { get; private set; }
        public FakeBaseView FakeView { get; } = new();

        public StaffViewCtrl(Store store) : base(store) { }

        protected override IViewBase CreateView()
        {
            ViewsCreated++;
            return FakeView;
        }
    }

    private static (Store store, List<string> notified) CreateStore()
    {
        var store = new Store(new TestFactoryViews());
        var notified = new List<string>();
        store.AccessDeniedNotifier = notified.Add;
        return (store, notified);
    }

    private static FakeKntService Repository(string alias) => new() { RepositoryRef = new RepositoryRef { Alias = alias } };

    [TestMethod]
    public void ControllerWithoutRequirement_Runs()
    {
        var (store, notified) = CreateStore();
        var ctrl = new OpenCtrl(store);

        var result = ctrl.Run();

        Assert.IsTrue(result.IsValid);
        Assert.IsTrue(ctrl.PreconditionsMet);
        Assert.AreEqual(EControllerState.Started, ctrl.ControllerState);
        Assert.AreEqual(0, notified.Count);
    }

    [TestMethod]
    public void ApplicationRequirement_NotMet_IsRefusedToldAndFinalized()
    {
        var (store, notified) = CreateStore();
        store.Security.SetRepositoryRole(Repository("Personal"), EnumRoles.ProjectManager);
        var ctrl = new AdminOnlyCtrl(store);

        var result = ctrl.Run();

        Assert.IsFalse(result.IsValid);
        Assert.IsFalse(ctrl.PreconditionsMet);
        Assert.AreEqual(EControllerState.Finalized, ctrl.ControllerState);
        Assert.AreEqual(1, notified.Count);
        StringAssert.Contains(notified[0], "'Admin only' requires the role 'Admin'");
    }

    [TestMethod]
    public void ApplicationRequirement_IsMetWithTheHighestRoleOfAnyRepository()
    {
        var (store, notified) = CreateStore();
        store.Security.SetRepositoryRole(Repository("Shared"), EnumRoles.Guest);
        store.Security.SetRepositoryRole(Repository("Personal"), EnumRoles.Admin);

        var result = new AdminOnlyCtrl(store).Run();

        Assert.IsTrue(result.IsValid);
        Assert.AreEqual(0, notified.Count);
    }

    [TestMethod]
    public void RepositoryRequirement_IsCheckedAgainstTheControllersRepository()
    {
        var (store, notified) = CreateStore();
        var personal = Repository("Personal");
        var shared = Repository("Shared");
        store.Security.SetRepositoryRole(personal, EnumRoles.Admin);
        store.Security.SetRepositoryRole(shared, EnumRoles.Guest);

        Assert.IsTrue(new StaffInRepositoryCtrl(store) { Repository = personal }.Run().IsValid);

        var refused = new StaffInRepositoryCtrl(store) { Repository = shared }.Run();
        Assert.IsFalse(refused.IsValid, "Being Admin in another repository doesn't count.");
        StringAssert.Contains(notified.Single(), "in the repository 'Shared'");
    }

    [TestMethod]
    public void RepositoryRequirement_WithoutRepositoryYet_UsesTheApplicationRole()
    {
        var (store, _) = CreateStore();
        store.Security.SetRepositoryRole(Repository("Personal"), EnumRoles.Staff);

        Assert.IsTrue(new StaffInRepositoryCtrl(store) { Repository = null }.Run().IsValid);
    }

    [TestMethod]
    public void IsAuthorized_TellsTheUserNothing()
    {
        var (store, notified) = CreateStore();
        var ctrl = new AdminOnlyCtrl(store);

        Assert.IsFalse(ctrl.IsAuthorized());
        Assert.AreEqual(0, notified.Count);
        Assert.AreNotEqual(EControllerState.Finalized, ctrl.ControllerState);
    }

    [TestMethod]
    public void CheckAccess_WhenMet_LeavesTheControllerReadyToRun()
    {
        var (store, notified) = CreateStore();
        store.Security.SetRepositoryRole(Repository("Personal"), EnumRoles.Admin);
        var ctrl = new AdminOnlyCtrl(store);

        Assert.IsTrue(ctrl.CheckAccess());
        Assert.IsTrue(ctrl.Run().IsValid);
        Assert.AreEqual(0, notified.Count);
    }

    [TestMethod]
    public void RefusedViewController_NeverCreatesItsView()
    {
        var (store, notified) = CreateStore();
        var ctrl = new StaffViewCtrl(store);

        var run = ctrl.Run();
        var runModal = new StaffViewCtrl(store).RunModal();

        Assert.IsFalse(run.IsValid);
        Assert.AreNotEqual(EControllerResult.Executed, runModal.Entity);
        Assert.AreEqual(0, ctrl.ViewsCreated);
        Assert.AreEqual(2, notified.Count);
    }

    [TestMethod]
    public void AllowedViewController_ShowsItsView()
    {
        var (store, _) = CreateStore();
        store.Security.SetRepositoryRole(Repository("Personal"), EnumRoles.Staff);
        var ctrl = new StaffViewCtrl(store);

        ctrl.Run();
        var modal = new StaffViewCtrl(store);
        modal.RunModal();

        Assert.AreEqual(1, ctrl.FakeView.ShowViewCallCount);
        Assert.AreEqual(1, modal.FakeView.ShowModalViewCallCount);
    }
}

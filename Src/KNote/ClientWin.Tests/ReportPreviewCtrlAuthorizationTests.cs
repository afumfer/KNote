using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Core.Reports;
using KNote.ClientWin.Tests.Fakes;
using KNote.Model;

namespace KNote.ClientWin.Tests;

/// <summary>
/// ReportPreviewCtrl.ShowAsync, the entry point of every print use case: printing what a repository holds
/// requires the Staff role there, checked before building the report (which can take a while).
/// </summary>
[TestClass]
public class ReportPreviewCtrlAuthorizationTests
{
    private static (Store store, FakeBaseView view, FakeKntService service, List<string> notified) CreateStore(EnumRoles role)
    {
        var factoryViews = new TestFactoryViews();
        var view = new FakeBaseView();
        factoryViews.Registry.Register<ReportPreviewCtrl, IViewBase>(c => view);

        var store = new Store(factoryViews);
        var notified = new List<string>();
        store.AccessDeniedNotifier = notified.Add;
        var service = new FakeKntService { RepositoryRef = new RepositoryRef { Alias = "Shared" } };
        store.Security.SetRepositoryRole(service, role);

        return (store, view, service, notified);
    }

    [TestMethod]
    public async Task Guest_IsRefusedWithoutBuildingTheReport()
    {
        var (store, view, service, notified) = CreateStore(EnumRoles.Guest);
        var built = false;

        var result = await ReportPreviewCtrl.ShowAsync(store, service, () =>
        {
            built = true;
            return Task.FromResult(new ReportDocument { Title = "Notes list" });
        });

        Assert.IsFalse(built);
        Assert.AreEqual(EControllerResult.Canceled, result.Entity);
        Assert.IsTrue(result.IsValid, "Already told the user: no error left for the caller to report.");
        Assert.AreEqual(0, view.ShowViewCallCount);
        Assert.AreEqual(1, notified.Count);
    }

    [TestMethod]
    public async Task Staff_BuildsAndPreviewsTheReport()
    {
        var (store, view, service, notified) = CreateStore(EnumRoles.Staff);

        var result = await ReportPreviewCtrl.ShowAsync(store, service,
            () => Task.FromResult(new ReportDocument { Title = "Notes list" }));

        Assert.IsTrue(result.IsValid, result.ErrorMessage);
        Assert.AreEqual(1, view.ShowViewCallCount);
        Assert.AreEqual(0, notified.Count);
    }
}

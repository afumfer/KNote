using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.ClientWin.Tests.Helpers;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// UserProfileCtrl: the profile as it is in one repository, saved in every linked one by default (or only in that
/// one); a new email needs the current password; a repository that refuses it doesn't stop the others.
/// </summary>
[TestClass]
public class UserProfileCtrlTests
{
    private static (UserProfileCtrl ctrl, FakeEditorView<UserProfileDto> view, Store store, FakeKntService[] services) CreateCtrl()
    {
        var factoryViews = new TestFactoryViews();
        var view = new FakeEditorView<UserProfileDto>();
        factoryViews.Registry.Register<UserProfileCtrl, IViewEditor<UserProfileDto>>(c => view);

        var store = new Store(factoryViews) { AppUserName = "jdoe" };
        var services = new[] { "Notes", "Work" }
            .Select(name => new FakeKntService { RepositoryRef = new RepositoryRef { Name = name } })
            .ToArray();
        foreach (var service in services)
        {
            service.UsersFake.GetProfileAsyncImpl = () => Task.FromResult(new Result<UserProfileDto>(Profile()));
            service.UsersFake.SaveProfileAsyncImpl = p => Task.FromResult(new Result<UserProfileDto>(Copy(p)));
            store.AddServiceRef(TestServiceRefFactory.CreateWithFakeService(service));
        }
        store.GrantAdmin(services);

        return (new UserProfileCtrl(store), view, store, services);
    }

    private static UserProfileDto Profile() => new() { UserName = "jdoe", FullName = "John Doe", EMail = "john@doe.com" };

    private static UserProfileDto Copy(UserProfileDto p) => new() { UserName = "jdoe", FullName = p.FullName, EMail = p.EMail };

    [TestMethod]
    public async Task LoadModelAsync_IsTheProfileInThatRepository()
    {
        var (ctrl, _, _, services) = CreateCtrl();

        var res = await ctrl.LoadModelAsync(services[1]);

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        Assert.AreEqual("John Doe", ctrl.Model.FullName);
        Assert.AreEqual("Work", ctrl.RepositoryName);
        Assert.IsFalse(ctrl.Model.IsDirty());
    }

    [TestMethod]
    public async Task LoadModelAsync_NotRegistered_IsNotValid()
    {
        var (ctrl, _, _, services) = CreateCtrl();
        services[0].UsersFake.GetProfileAsyncImpl = () =>
        {
            var res = new Result<UserProfileDto>();
            res.AddErrorMessage("The user 'jdoe' is not registered.");
            return Task.FromResult(res);
        };

        var res = await ctrl.LoadModelAsync(services[0]);

        Assert.IsFalse(res.IsValid);
        StringAssert.Contains(res.ErrorMessage, "not registered");
    }

    [TestMethod]
    [DataRow(true, 2)]
    [DataRow(false, 1)]
    public async Task SaveModel_InEveryRepository_OrOnlyInTheOneShown(bool applyToAll, int expectedSaves)
    {
        var (ctrl, _, _, services) = CreateCtrl();
        var savedIn = new List<string>();
        foreach (var service in services)
            service.UsersFake.SaveProfileAsyncImpl = p => { savedIn.Add(service.RepositoryRef.Name); return Task.FromResult(new Result<UserProfileDto>(Copy(p))); };
        await ctrl.LoadModelAsync(services[0]);
        ctrl.ApplyToAllRepositories = applyToAll;
        ctrl.Model.FullName = "John R. Doe";

        var saved = await ctrl.SaveModel();

        Assert.IsTrue(saved);
        Assert.AreEqual(expectedSaves, savedIn.Count);
        Assert.AreEqual("Notes", savedIn[0]);
        Assert.AreEqual("John R. Doe", ctrl.Model.FullName);
    }

    [TestMethod]
    public async Task SaveModel_NewEmailWithoutCurrentPassword_DoesNotCallTheService()
    {
        var (ctrl, view, _, services) = CreateCtrl();
        var called = false;
        foreach (var service in services)
            service.UsersFake.SaveProfileAsyncImpl = p => { called = true; return Task.FromResult(new Result<UserProfileDto>(Copy(p))); };
        await ctrl.LoadModelAsync(services[0]);
        ctrl.Model.EMail = "johnny@doe.com";

        Assert.IsTrue(ctrl.IsEMailChanged(ctrl.Model.EMail));
        Assert.IsFalse(await ctrl.SaveModel());
        Assert.IsFalse(called);
        StringAssert.Contains(view.LastShownInfo, "current password");
    }

    [TestMethod]
    public async Task SaveModel_NewEmailWithCurrentPassword_SendsIt()
    {
        var (ctrl, _, _, services) = CreateCtrl();
        UserProfileDto? sent = null;
        services[0].UsersFake.SaveProfileAsyncImpl = p => { sent = p; return Task.FromResult(new Result<UserProfileDto>(Copy(p))); };
        await ctrl.LoadModelAsync(services[0]);
        ctrl.ApplyToAllRepositories = false;
        ctrl.Model.EMail = "johnny@doe.com";
        ctrl.Model.CurrentPassword = "Password-1!";

        Assert.IsTrue(await ctrl.SaveModel());
        Assert.AreEqual("Password-1!", sent!.CurrentPassword);
    }

    [TestMethod]
    public async Task SaveModel_RefusedInOneRepository_SavesTheOthers_AndSaysWhere()
    {
        var (ctrl, view, _, services) = CreateCtrl();
        services[1].UsersFake.SaveProfileAsyncImpl = p =>
        {
            var res = new Result<UserProfileDto>();
            res.AddErrorMessage("Email \"x\" is already in use.");
            return Task.FromResult(res);
        };
        await ctrl.LoadModelAsync(services[0]);
        ctrl.Model.FullName = "John R. Doe";

        Assert.IsTrue(await ctrl.SaveModel());
        StringAssert.Contains(view.LastShownInfo, "saved in: Notes.");
        StringAssert.Contains(view.LastShownInfo, "Work: Email \"x\" is already in use.");
    }

    [TestMethod]
    public async Task SaveModel_NothingChanged_DoesNotCallTheService()
    {
        var (ctrl, _, _, services) = CreateCtrl();
        var called = false;
        foreach (var service in services)
            service.UsersFake.SaveProfileAsyncImpl = p => { called = true; return Task.FromResult(new Result<UserProfileDto>(Copy(p))); };
        await ctrl.LoadModelAsync(services[0]);

        Assert.IsTrue(await ctrl.SaveModel());
        Assert.IsFalse(called);
    }
}

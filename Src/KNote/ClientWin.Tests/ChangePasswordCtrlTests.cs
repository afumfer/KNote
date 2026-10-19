using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.ClientWin.Tests.Helpers;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// ChangePasswordCtrl: the password changes in every linked repository (the user exists once in each one); one that
/// refuses it doesn't stop the others, and signed in with a KNote user the session's password becomes the new one.
/// </summary>
[TestClass]
public class ChangePasswordCtrlTests
{
    private const string NewPassword = "New-Password-1!";

    private static (ChangePasswordCtrl ctrl, FakeEditorView<ChangePasswordModel> view, Store store, FakeKntService[] services)
        CreateCtrl(int repositories = 2, AppAuthenticationMode mode = AppAuthenticationMode.Credentials)
    {
        var factoryViews = new TestFactoryViews();
        var view = new FakeEditorView<ChangePasswordModel>();
        factoryViews.Registry.Register<ChangePasswordCtrl, IViewEditor<ChangePasswordModel>>(c => view);

        var store = new Store(factoryViews) { AppUserName = "jdoe" };
        store.Security.StartSession(mode, "Old-Password-1!");

        var services = Enumerable.Range(1, repositories)
            .Select(i => new FakeKntService { RepositoryRef = new RepositoryRef { Name = $"Repository {i}" } })
            .ToArray();
        foreach (var service in services)
        {
            service.UsersFake.ChangePasswordAsyncImpl = p => Task.FromResult(new Result<UserDto>(new UserDto { UserName = "jdoe" }));
            store.AddServiceRef(TestServiceRefFactory.CreateWithFakeService(service));
        }
        store.GrantAdmin(services);

        var ctrl = new ChangePasswordCtrl(store);
        return (ctrl, view, store, services);
    }

    private static async Task Fill(ChangePasswordCtrl ctrl, string current, string newPassword, string confirm)
    {
        await ctrl.NewModel();
        ctrl.Model.CurrentPassword = current;
        ctrl.Model.NewPassword = newPassword;
        ctrl.Model.ConfirmPassword = confirm;
    }

    [TestMethod]
    public async Task SaveModel_ChangesItInEveryRepository_AndInTheSession()
    {
        var (ctrl, view, store, services) = CreateCtrl();
        var sent = new List<UserChangePasswordDto>();
        foreach (var service in services)
            service.UsersFake.ChangePasswordAsyncImpl = p => { sent.Add(p); return Task.FromResult(new Result<UserDto>(new UserDto())); };
        await Fill(ctrl, "Old-Password-1!", NewPassword, NewPassword);

        var saved = await ctrl.SaveModel();

        Assert.IsTrue(saved);
        Assert.AreEqual(2, sent.Count);
        Assert.IsTrue(sent.All(p => p.CurrentPassword == "Old-Password-1!" && p.NewPassword == NewPassword));
        Assert.AreEqual(NewPassword, store.Security.Password);
        StringAssert.Contains(view.LastShownInfo, "Repository 1, Repository 2");
    }

    [TestMethod]
    [DataRow(NewPassword, "Other-Password-1!", "not the same")]
    [DataRow("", "", "required")]
    [DataRow("short", "short", "at least")]
    [DataRow("jdoe", "jdoe", "at least")]
    public async Task SaveModel_InvalidNewPassword_DoesNotCallTheService(string newPassword, string confirm, string expected)
    {
        var (ctrl, view, store, services) = CreateCtrl();
        var called = false;
        foreach (var service in services)
            service.UsersFake.ChangePasswordAsyncImpl = p => { called = true; return Task.FromResult(new Result<UserDto>(new UserDto())); };
        await Fill(ctrl, "Old-Password-1!", newPassword, confirm);

        var saved = await ctrl.SaveModel();

        Assert.IsFalse(saved);
        Assert.IsFalse(called);
        StringAssert.Contains(view.LastShownInfo, expected);
        Assert.AreEqual("Old-Password-1!", store.Security.Password);
    }

    [TestMethod]
    public async Task SaveModel_TheUserNameAsPassword_IsRefused()
    {
        var (ctrl, view, _, _) = CreateCtrl();
        ctrl.Store.AppUserName = "jdoe-long-name";
        await Fill(ctrl, "Old-Password-1!", "JDOE-LONG-NAME", "JDOE-LONG-NAME");

        Assert.IsFalse(await ctrl.SaveModel());
        StringAssert.Contains(view.LastShownInfo, "user name");
    }

    [TestMethod]
    public async Task SaveModel_RefusedInOneRepository_ChangesTheOthers_AndSaysWhere()
    {
        var (ctrl, view, store, services) = CreateCtrl();
        services[1].UsersFake.ChangePasswordAsyncImpl = p =>
        {
            var res = new Result<UserDto>();
            res.AddErrorMessage("The current password is not correct.");
            return Task.FromResult(res);
        };
        await Fill(ctrl, "Old-Password-1!", NewPassword, NewPassword);

        var saved = await ctrl.SaveModel();

        Assert.IsTrue(saved);
        Assert.AreEqual(NewPassword, store.Security.Password);
        StringAssert.Contains(view.LastShownInfo, "changed in: Repository 1.");
        StringAssert.Contains(view.LastShownInfo, "Repository 2: The current password is not correct.");
        StringAssert.Contains(view.LastShownInfo, "next session");
    }

    [TestMethod]
    public async Task SaveModel_RefusedEverywhere_KeepsTheDialogAndTheSessionPassword()
    {
        var (ctrl, view, store, services) = CreateCtrl();
        foreach (var service in services)
            service.UsersFake.ChangePasswordAsyncImpl = p => throw new InvalidOperationException("wrapper", new Exception("Database is locked"));
        await Fill(ctrl, "Old-Password-1!", NewPassword, NewPassword);

        var saved = await ctrl.SaveModel();

        Assert.IsFalse(saved);
        Assert.AreEqual("Old-Password-1!", store.Security.Password);
        StringAssert.Contains(view.LastShownInfo, "not changed");
        StringAssert.Contains(view.LastShownInfo, "Database is locked");
    }

    [TestMethod]
    public async Task SaveModel_WithTheWindowsAccount_HasNoSessionPassword()
    {
        var (ctrl, _, store, _) = CreateCtrl(mode: AppAuthenticationMode.Windows);
        await Fill(ctrl, "", NewPassword, NewPassword);

        Assert.IsTrue(await ctrl.SaveModel());
        Assert.IsNull(store.Security.Password);
    }

    [TestMethod]
    public async Task SaveModel_WithoutRepositories_SaysSo()
    {
        var (ctrl, view, _, _) = CreateCtrl(repositories: 0);
        await Fill(ctrl, "Old-Password-1!", NewPassword, NewPassword);

        Assert.IsFalse(await ctrl.SaveModel());
        StringAssert.Contains(view.LastShownInfo, "no linked repositories");
    }
}

using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.Model;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Tests for LoginCtrl: the startup sign-in with a KNote user name and password. It only starts the
/// session (Store.AppUserName, Store.Security); each repository checks the credentials when it is linked.
/// </summary>
[TestClass]
public class LoginCtrlTests
{
    private static (LoginCtrl ctrl, FakeLoginView view, Store store) CreateCtrl()
    {
        var factoryViews = new TestFactoryViews();
        var view = new FakeLoginView();
        factoryViews.Registry.Register<LoginCtrl, IViewEditor<LoginModel>>(c => view);

        var store = new Store(factoryViews) { AppUserName = "WINUSER" };
        store.Settings.Security.AuthenticationMode = AppAuthenticationMode.Credentials;
        var ctrl = new LoginCtrl(store) { WindowsUserName = "WINUSER" };

        return (ctrl, view, store);
    }

    [TestMethod]
    public async Task NewModel_ProposesTheLastUserName()
    {
        var (ctrl, _, store) = CreateCtrl();
        store.State.Session.LastUserName = "jdoe";

        await ctrl.NewModel();

        Assert.AreEqual("jdoe", ctrl.Model.UserName);
        Assert.IsNull(ctrl.Model.Password);
    }

    [TestMethod]
    [DataRow("", "secret")]
    [DataRow("jdoe", "")]
    [DataRow("  ", "  ")]
    public async Task SaveModel_MissingUserNameOrPassword_IsRejected(string userName, string password)
    {
        var (ctrl, view, store) = CreateCtrl();
        await ctrl.NewModel();
        ctrl.Model.UserName = userName;
        ctrl.Model.Password = password;

        var saved = await ctrl.SaveModel();

        Assert.IsFalse(saved);
        Assert.IsFalse(string.IsNullOrEmpty(view.LastShownInfo));
        Assert.AreEqual("WINUSER", store.AppUserName, "The session must not start.");
    }

    [TestMethod]
    public async Task SaveModel_StartsACredentialsSession()
    {
        var (ctrl, _, store) = CreateCtrl();
        await ctrl.NewModel();
        ctrl.Model.UserName = " jdoe ";
        ctrl.Model.Password = "secret";

        var saved = await ctrl.SaveModel();

        Assert.IsTrue(saved);
        Assert.AreEqual("jdoe", store.AppUserName);
        Assert.AreEqual(AppAuthenticationMode.Credentials, store.Security.AuthenticationMode);
        Assert.AreEqual("secret", store.Security.Password);
        Assert.AreEqual("jdoe", store.State.Session.LastUserName);
    }

    [TestMethod]
    public async Task UseWindowsAccount_StartsAWindowsSessionAndSwitchesTheSettingBack()
    {
        var (ctrl, _, store) = CreateCtrl();
        await ctrl.NewModel();
        store.AppUserName = "someone-else";

        ctrl.UseWindowsAccount();

        Assert.AreEqual("WINUSER", store.AppUserName);
        Assert.AreEqual(AppAuthenticationMode.Windows, store.Security.AuthenticationMode);
        Assert.IsNull(store.Security.Password);
        Assert.AreEqual(AppAuthenticationMode.Windows, store.Settings.Security.AuthenticationMode);
    }
}

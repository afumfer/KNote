using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.ClientWin.Tests.Helpers;
using KNote.Model;
using KNote.Service.ServicesCommands;

namespace KNote.ClientWin.Tests;

/// <summary>
/// MessagesManagementCtrl runs on timers (reopening PostIts, processing alarms): it only processes the
/// repositories where the user's role allows it, and skips the rest without a word.
/// </summary>
[TestClass]
public class MessagesManagementAuthorizationTests
{
    [TestMethod]
    public void RepositoriesAuthorizedFor_KeepsOnlyTheRepositoriesWhereTheRoleAllowsIt()
    {
        var store = new Store(new TestFactoryViews());
        var staffService = new FakeKntService();
        var guestService = new FakeKntService();
        var staffRepository = TestServiceRefFactory.CreateWithFakeService(staffService);
        var guestRepository = TestServiceRefFactory.CreateWithFakeService(guestService);
        store.AddServiceRef(staffRepository);
        store.AddServiceRef(guestRepository);
        store.Security.SetRepositoryRole(staffService, EnumRoles.Staff);
        store.Security.SetRepositoryRole(guestService, EnumRoles.Guest);

        var ctrl = new MessagesManagementCtrl(store);

        CollectionAssert.AreEqual(new[] { staffRepository }, ctrl.RepositoriesAuthorizedFor(typeof(PostItEditorCtrl)));
        CollectionAssert.AreEqual(new[] { staffRepository }, ctrl.RepositoriesAuthorizedFor(typeof(KntNotesGetAlarmNotesIdAsyncCommand)));
        Assert.AreEqual(2, ctrl.RepositoriesAuthorizedFor(typeof(NoteEditorCtrl)).Count, "Reading notes is open to a Guest.");
    }
}

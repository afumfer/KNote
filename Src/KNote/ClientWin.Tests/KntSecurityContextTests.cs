using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.Model;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Tests for KntSecurityContext (Store.Security): how the session's user signed in and its role in each
/// linked repository, plus the application role used by the use cases that don't belong to a repository.
/// </summary>
[TestClass]
public class KntSecurityContextTests
{
    [TestMethod]
    public void NewContext_IsAWindowsSessionWithoutPassword()
    {
        var security = new KntSecurityContext();

        Assert.AreEqual(AppAuthenticationMode.Windows, security.AuthenticationMode);
        Assert.IsNull(security.Password);
    }

    [TestMethod]
    public void StartSession_KeepsThePasswordOnlyForCredentials()
    {
        var security = new KntSecurityContext();

        security.StartSession(AppAuthenticationMode.Credentials, "secret");
        Assert.AreEqual("secret", security.Password);

        security.StartSession(AppAuthenticationMode.Windows, "ignored");
        Assert.AreEqual(AppAuthenticationMode.Windows, security.AuthenticationMode);
        Assert.IsNull(security.Password);
    }

    [TestMethod]
    public void ApplicationRole_WithoutRepositories_IsGuest()
    {
        Assert.AreEqual(EnumRoles.Guest, new KntSecurityContext().ApplicationRole);
    }

    [TestMethod]
    public void ApplicationRole_IsTheHighestRoleAcrossRepositories()
    {
        var security = new KntSecurityContext();
        var personal = new FakeKntService();
        var shared = new FakeKntService();

        security.SetRepositoryRole(personal, EnumRoles.Admin);
        security.SetRepositoryRole(shared, EnumRoles.Guest);

        Assert.AreEqual(EnumRoles.Admin, security.ApplicationRole);
        Assert.AreEqual(EnumRoles.Admin, security.GetRepositoryRole(personal));
        Assert.AreEqual(EnumRoles.Guest, security.GetRepositoryRole(shared));
    }

    [TestMethod]
    public void GetRepositoryRoleName_ShowsTheRoleAsTheUserReadsIt()
    {
        var security = new KntSecurityContext();
        var service = new FakeKntService();

        Assert.AreEqual("no role", security.GetRepositoryRoleName(service));

        security.SetRepositoryRole(service, EnumRoles.ProjectManager);
        Assert.AreEqual("Project manager", security.GetRepositoryRoleName(service));
    }

    [TestMethod]
    public void IsAuthorized_WithoutRequirement_IsAlwaysTrue()
    {
        Assert.IsTrue(new KntSecurityContext().IsAuthorized((KntAuthorizeAttribute)null));
    }

    [TestMethod]
    public void IsAuthorized_RepositoryRequirement_UsesTheRoleInThatRepository()
    {
        var security = new KntSecurityContext();
        var personal = new FakeKntService();
        var shared = new FakeKntService();
        security.SetRepositoryRole(personal, EnumRoles.Admin);
        security.SetRepositoryRole(shared, EnumRoles.Guest);
        var staff = new KntAuthorizeAttribute(EnumRoles.Staff);

        Assert.IsTrue(security.IsAuthorized(staff, personal));
        Assert.IsFalse(security.IsAuthorized(staff, shared));
        Assert.IsFalse(security.IsAuthorized(staff, new FakeKntService()), "No role in an unknown repository.");
        Assert.IsTrue(security.IsAuthorized(staff), "No repository: the application role.");
    }

    [TestMethod]
    public void IsAuthorized_ApplicationRequirement_UsesTheApplicationRole()
    {
        var security = new KntSecurityContext();
        var shared = new FakeKntService();
        security.SetRepositoryRole(shared, EnumRoles.Guest);
        security.SetRepositoryRole(new FakeKntService(), EnumRoles.ProjectManager);

        Assert.IsTrue(security.IsAuthorized(new KntAuthorizeAttribute(EnumRoles.ProjectManager, AuthorizationScope.Application), shared));
        Assert.IsFalse(security.IsAuthorized(new KntAuthorizeAttribute(EnumRoles.Admin, AuthorizationScope.Application)));
    }

    [TestMethod]
    public void IsAuthorized_ByType_ReadsTheTypesAttribute()
    {
        var security = new KntSecurityContext();
        var service = new FakeKntService();
        security.SetRepositoryRole(service, EnumRoles.Guest);

        Assert.IsTrue(security.IsAuthorized(typeof(KNote.Service.ServicesCommands.KntNotesGetExtendedAsyncCommand), service));
        Assert.IsFalse(security.IsAuthorized(typeof(KNote.Service.ServicesCommands.KntNotesSaveExtendedAsyncCommand), service));
        Assert.IsTrue(security.IsAuthorized(typeof(string)), "A type without [KntAuthorize] requires nothing.");
    }

    [TestMethod]
    public void RemoveRepository_ForgetsItsRole()
    {
        var security = new KntSecurityContext();
        var personal = new FakeKntService();
        var shared = new FakeKntService();
        security.SetRepositoryRole(personal, EnumRoles.Admin);
        security.SetRepositoryRole(shared, EnumRoles.Staff);

        security.RemoveRepository(personal);

        Assert.IsNull(security.GetRepositoryRole(personal));
        Assert.AreEqual(EnumRoles.Staff, security.ApplicationRole);
    }

    [TestMethod]
    public void StartSession_ForgetsThePreviousSessionsRoles()
    {
        var security = new KntSecurityContext();
        var service = new FakeKntService();
        security.SetRepositoryRole(service, EnumRoles.Admin);

        security.StartSession(AppAuthenticationMode.Credentials, "secret");

        Assert.IsNull(security.GetRepositoryRole(service));
        Assert.AreEqual(EnumRoles.Guest, security.ApplicationRole);
    }
}

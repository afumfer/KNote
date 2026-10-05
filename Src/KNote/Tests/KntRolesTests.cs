using KNote.Model;

namespace KNote.Tests;

/// <summary>
/// Tests for KntRoles: reading User.RoleDefinition (comma-separated EnumRoles names) and the role
/// hierarchy Guest &lt; Staff &lt; ProjectManager &lt; Admin.
/// </summary>
[TestClass]
public class KntRolesTests
{
    [TestMethod]
    [DataRow("Guest", EnumRoles.Guest)]
    [DataRow("Staff", EnumRoles.Staff)]
    [DataRow("ProjectManager", EnumRoles.ProjectManager)]
    [DataRow("Admin", EnumRoles.Admin)]
    [DataRow("Staff, Admin", EnumRoles.Admin)]
    [DataRow("Admin,Guest", EnumRoles.Admin)]
    [DataRow(" Guest ,  ProjectManager ", EnumRoles.ProjectManager)]
    [DataRow("Guest, Unknown", EnumRoles.Guest)]
    public void Highest_ReturnsTheHighestKnownRole(string roleDefinition, EnumRoles expected)
    {
        Assert.AreEqual(expected, KntRoles.Highest(roleDefinition));
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("  ")]
    [DataRow("Unknown")]
    [DataRow("Public")]          // old name, renamed to Guest by KntSchemaUpdater revision 4
    [DataRow("ProjecManager")]   // old typo, renamed to ProjectManager by KntSchemaUpdater revision 4
    [DataRow("admin")]           // names are persisted with their exact case
    [DataRow("4")]               // a numeric value must not be read as EnumRoles.Admin
    public void Highest_WithoutKnownRoles_ReturnsNull(string? roleDefinition)
    {
        Assert.IsNull(KntRoles.Highest(roleDefinition));
    }

    [TestMethod]
    public void Parse_IgnoresDuplicatesAndUnknownNames()
    {
        var roles = KntRoles.Parse("Staff, Staff, Unknown, Guest");

        CollectionAssert.AreEquivalent(new[] { EnumRoles.Staff, EnumRoles.Guest }, roles);
    }

    [TestMethod]
    [DataRow(EnumRoles.Guest, EnumRoles.Guest, true)]
    [DataRow(EnumRoles.Guest, EnumRoles.Staff, false)]
    [DataRow(EnumRoles.Guest, EnumRoles.ProjectManager, false)]
    [DataRow(EnumRoles.Guest, EnumRoles.Admin, false)]
    [DataRow(EnumRoles.Staff, EnumRoles.Guest, true)]
    [DataRow(EnumRoles.Staff, EnumRoles.Staff, true)]
    [DataRow(EnumRoles.Staff, EnumRoles.ProjectManager, false)]
    [DataRow(EnumRoles.Staff, EnumRoles.Admin, false)]
    [DataRow(EnumRoles.ProjectManager, EnumRoles.Guest, true)]
    [DataRow(EnumRoles.ProjectManager, EnumRoles.Staff, true)]
    [DataRow(EnumRoles.ProjectManager, EnumRoles.ProjectManager, true)]
    [DataRow(EnumRoles.ProjectManager, EnumRoles.Admin, false)]
    [DataRow(EnumRoles.Admin, EnumRoles.Guest, true)]
    [DataRow(EnumRoles.Admin, EnumRoles.Staff, true)]
    [DataRow(EnumRoles.Admin, EnumRoles.ProjectManager, true)]
    [DataRow(EnumRoles.Admin, EnumRoles.Admin, true)]
    public void IsInRole_FollowsTheRoleHierarchy(EnumRoles userRole, EnumRoles minimumRole, bool expected)
    {
        Assert.AreEqual(expected, KntRoles.IsInRole(userRole.ToString(), minimumRole));
    }

    [TestMethod]
    public void IsInRole_WithoutKnownRoles_IsAlwaysFalse()
    {
        Assert.IsFalse(KntRoles.IsInRole(null, EnumRoles.Guest));
        Assert.IsFalse(KntRoles.IsInRole("Public", EnumRoles.Guest));
    }

    [TestMethod]
    public void RolesCatalog_ListsEveryRoleInHierarchyOrder()
    {
        CollectionAssert.AreEqual(
            new[] { EnumRoles.Guest, EnumRoles.Staff, EnumRoles.ProjectManager, EnumRoles.Admin },
            KntConst.Roles.Keys.ToArray());
    }
}

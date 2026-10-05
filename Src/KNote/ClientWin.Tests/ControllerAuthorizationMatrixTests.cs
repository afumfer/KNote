using System.Reflection;
using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.Model;

namespace KNote.ClientWin.Tests;

/// <summary>
/// The role every ClientWin controller (use case) requires, as declared with [KntAuthorize] on its class,
/// compared both ways against the agreed authorization matrix below. "Included" controllers declare nothing:
/// they run inside another use case, which decides (a selector, a tab of the repository editor...). A new
/// controller makes this fail until it is placed in the matrix - a deliberate decision, not a default. The
/// finer-grained operations (saving, deleting...) are authorized by the Service layer, see
/// Tests/ServiceTests/CommandAuthorizationMatrixTests.
/// </summary>
[TestClass]
public class ControllerAuthorizationMatrixTests
{
    private const string Included = "Included";

    private static string Requirement(EnumRoles role, AuthorizationScope scope) => $"{role} ({scope})";

    private static readonly Dictionary<string, string> ExpectedMatrix = new()
    {
        // Application-wide use cases.
        ["OptionsEditorCtrl"] = Requirement(EnumRoles.Staff, AuthorizationScope.Application),
        ["AiProvidersManageCtrl"] = Requirement(EnumRoles.Staff, AuthorizationScope.Application),
        ["KntChatCtrl"] = Requirement(EnumRoles.Staff, AuthorizationScope.Application),
        ["KNoteAIAssistantCtrl"] = Requirement(EnumRoles.Staff, AuthorizationScope.Application),
        ["AppInfoAlarmsCtrl"] = Requirement(EnumRoles.Staff, AuthorizationScope.Application),
        ["KntScriptConsoleCtrl"] = Requirement(EnumRoles.ProjectManager, AuthorizationScope.Application),
        ["KntServerCOMCtrl"] = Requirement(EnumRoles.Admin, AuthorizationScope.Application),
        ["KntLabCtrl"] = Requirement(EnumRoles.Admin, AuthorizationScope.Application),
        ["MonitorCtrl"] = Requirement(EnumRoles.Admin, AuthorizationScope.Application),
        ["KntHttpClientCtrl"] = Requirement(EnumRoles.Admin, AuthorizationScope.Application),

        // Use cases on one repository.
        ["NoteEditorCtrl"] = Requirement(EnumRoles.Guest, AuthorizationScope.Repository),
        ["PostItEditorCtrl"] = Requirement(EnumRoles.Staff, AuthorizationScope.Repository),
        ["ReportPreviewCtrl"] = Requirement(EnumRoles.Staff, AuthorizationScope.Repository),
        ["NotesListCsvExportCtrl"] = Requirement(EnumRoles.Staff, AuthorizationScope.Repository),
        ["FolderEditorCtrl"] = Requirement(EnumRoles.ProjectManager, AuthorizationScope.Repository),

        // Depends on its EditorMode (see RepositoryEditorModes_RequireTheirOwnRole).
        ["RepositoryEditorCtrl"] = Included,

        // The hub: every use case it launches decides for itself (Guest at least for anybody signed in).
        ["KNoteManagementCtrl"] = Included,

        // Included in another use case, or part of signing in.
        ["AiProviderEditorCtrl"] = Included,
        ["AttributeEditorCtrl"] = Included,
        ["FoldersSelectorCtrl"] = Included,
        ["HeavyProcessCtrl"] = Included,
        ["KAttributeTabulatedValueEditorCtrl"] = Included,
        ["KAttributesManageCtrl"] = Included,
        ["LoginCtrl"] = Included,
        ["MessageEditorCtrl"] = Included,
        ["MessagesManagementCtrl"] = Included,
        ["NoteAttributeEditorCtrl"] = Included,
        ["NoteTypeEditorCtrl"] = Included,
        ["NoteTypesManageCtrl"] = Included,
        ["NoteTypesSelectorCtrl"] = Included,
        ["NotesFilterParamCtrl"] = Included,
        ["NotesSearchParamCtrl"] = Included,
        ["NotesSelectorCtrl"] = Included,
        ["PostItPropertiesCtrl"] = Included,
        ["ResourceEditorCtrl"] = Included,
        ["TaskEditorCtrl"] = Included,
        ["TraceNoteEditorCtrl"] = Included,
        ["TraceNoteTypeEditorCtrl"] = Included,
        ["TraceNoteTypesManageCtrl"] = Included,
        ["UserEditorCtrl"] = Included,
        ["UserRegisterCtrl"] = Included,
        ["UsersManageCtrl"] = Included,
        ["UsersSelectorCtrl"] = Included,
    };

    private static string DeclaredRequirement(Type controllerType)
    {
        var attribute = controllerType.GetCustomAttribute<KntAuthorizeAttribute>(inherit: false);
        return attribute == null ? Included : Requirement(attribute.MinimumRole, attribute.Scope);
    }

    [TestMethod]
    public void DeclaredRequirements_MatchTheAuthorizationMatrix()
    {
        var declared = typeof(CtrlBase).Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(CtrlBase).IsAssignableFrom(t))
            .ToDictionary(t => t.Name, DeclaredRequirement);

        var differences = new List<string>();
        foreach (var (controller, requirement) in declared)
        {
            if (!ExpectedMatrix.TryGetValue(controller, out var expected))
                differences.Add($"{controller}: requires {requirement}, missing from the matrix");
            else if (expected != requirement)
                differences.Add($"{controller}: requires {requirement}, the matrix says {expected}");
        }
        foreach (var controller in ExpectedMatrix.Keys.Except(declared.Keys))
            differences.Add($"{controller}: in the matrix, but no such controller in ClientWin");

        Assert.AreEqual(0, differences.Count, string.Join(Environment.NewLine, differences));
    }

    private class RepositoryEditorRequirement(Store store) : RepositoryEditorCtrl(store)
    {
        public KntAuthorizeAttribute Requirement => RequiredAuthorization;
    }

    [TestMethod]
    public void RepositoryEditorModes_RequireTheirOwnRole()
    {
        var ctrl = new RepositoryEditorRequirement(new Store(new TestFactoryViews()));

        ctrl.EditorMode = EnumRepositoryEditorMode.AddLink;
        Assert.IsNull(ctrl.Requirement, "Linking a repository is open to anybody signed in.");

        ctrl.EditorMode = EnumRepositoryEditorMode.Create;
        Assert.AreEqual(Requirement(EnumRoles.Admin, AuthorizationScope.Application),
            Requirement(ctrl.Requirement.MinimumRole, ctrl.Requirement.Scope));

        ctrl.EditorMode = EnumRepositoryEditorMode.Management;
        Assert.AreEqual(Requirement(EnumRoles.Admin, AuthorizationScope.Repository),
            Requirement(ctrl.Requirement.MinimumRole, ctrl.Requirement.Scope));
        Assert.IsTrue(ctrl.AdministrationAvailable);
    }
}

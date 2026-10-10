using System.Reflection;
using KNote.Model;
using KNote.Service.Core;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// The role every Service command requires, as declared with [KntAuthorize]/[KntAllowAnonymous] on its
/// class, compared both ways against the agreed authorization matrix below. A new command without an
/// attribute, a command whose role changes, or a command removed from the code makes this fail - change
/// the matrix only together with a deliberate change of the security design.
/// </summary>
[TestClass]
public class CommandAuthorizationMatrixTests
{
    private const string Anonymous = "Anonymous";

    private static readonly Dictionary<string, string> ExpectedMatrix = BuildExpectedMatrix();

    private static Dictionary<string, string> BuildExpectedMatrix()
    {
        var matrix = new Dictionary<string, string>();

        void Add(string role, params string[] commands)
        {
            foreach (var command in commands)
                matrix.Add(command, role);
        }

        Add(Anonymous,
            "KntUsersAuthenticateAsyncCommand", "KntUsersRegisterAsyncCommand");

        // Reading anything in the repository.
        Add(nameof(EnumRoles.Guest),
            "KntFoldersGetAllAsyncCommand", "KntFoldersGetAsyncCommand", "KntFoldersGetByNumAsyncCommand",
            "KntFoldersGetTreeAsyncCommand", "KntFoldersGetHomeAsyncCommand",
            "KntKAttributesGetAllAsyncCommand", "KntKAttributesGetAllByTypeAsyncCommand", "KntKAttributesGetAsyncCommand",
            "KntKAttributesTabulatedValuesAsyncCommand",
            "KntNotesGetAllAsyncCommand", "KntNotesGetMinimalAllAsyncCommand", "KntNotesHomeAllAsyncCommand",
            "KntNotesGetAsyncCommand", "KntNotesGetByNumberAsyncCommand", "KntNotesGetExtendedAsyncCommand",
            "KntNotesGetByFolderAsyncCommand", "KntNotesGetMinimalByFolderAsyncCommand", "KntNotesGetFilterAsyncCommand",
            "KntNotesGetMinimalFilterAsyncCommand", "KntNotesGetSearchAsyncCommand", "KntNotesGetMinimalSearchAsyncCommand",
            "KntNotesGetResourcesAsyncCommand", "KntNotesGetResourcesInfoAsyncCommand", "KntNotesGetResourceAsyncCommand",
            "KntNotesGetNoteTasksAsyncCommand", "KntNotesGetStartedTasksByDateTimeRageAsyncCommand",
            "KntNotesGetEstimatedTasksByDateTimeRageAsyncCommand", "KntNotesGetNoteTaskAsyncCommand",
            "KntNotesGetMessagesAsyncCommand", "KntNotesGetMessageAsyncCommand",
            "KntNotesGetTraceNotesFromAsyncCommand", "KntNotesGetTraceNotesToAsyncCommand",
            "KntNotesGetWindowAsyncCommand", "KntNotesGetVisibleNotesIdAsyncCommand",
            "KntNoteTypeGetAllAsyncCommand", "KntNoteTypeGetAsyncCommand",
            "KntSystemValueGetAllAsyncCommand", "KntSystemValueGetByScopeKeyAsyncCommand", "KntSystemValueGetAsyncCommand",
            "KntTraceNoteTypeGetAllAsyncCommand", "KntTraceNoteTypeGetAsyncCommand",
            "KntUsersGetAllAsyncCommand", "KntUsersGetAsyncCommand", "KntUsersGetByUserNameAsyncCommand");

        // Creating, editing and deleting notes and everything inside them. KntNotesGetAlarmNotesIdAsyncCommand
        // is here too: besides reading the due alarms it updates them (alarm processing).
        Add(nameof(EnumRoles.Staff),
            "KntFoldersUpdateOrderNotesAsyncCommand",
            "KntNotesNewAsyncCommand", "KntNotesNewExtendedAsyncCommand", "KntNotesSaveAsyncCommand",
            "KntNotesSaveExtendedAsyncCommand", "KntNotesDeleteAsyncCommand", "KntNotesDeleteExtendedAsyncCommand",
            "KntNotesSaveResourceAsyncCommand", "KntNotesSaveResourceInfoAsyncCommand", "KntNotesDeleteResourceAsyncCommand",
            "KntNotesDeleteResourceInfoAsyncCommand", "KntNotesSaveNoteTaskAsyncCommand", "KntNotesDeleteNoteTaskAsyncCommand",
            "KntNotesSaveMessageAsyncCommand", "KntNotesDeleteMessageAsyncCommand", "KntNotesSaveTraceNoteAsyncCommand",
            "KntNotesDeleteTraceNoteAsyncCommand", "KntNotesSaveWindowAsyncCommand", "KntNotesGetAlarmNotesIdAsyncCommand",
            "KntNotesPatchFolderAsyncCommand", "KntNotesPatchChangeTagsAsyncCommand");

        // The AI assistant's sessions: notes and tasks of the user's own (Staff, like the assistant), also to
        // read them, since the assistant itself requires Staff.
        Add(nameof(EnumRoles.Staff),
            "KntAiSessionsGetUserSessionsAsyncCommand", "KntAiSessionsGetAsyncCommand", "KntAiSessionsSaveAsyncCommand");

        Add(nameof(EnumRoles.ProjectManager),
            "KntFoldersSaveAsyncCommand", "KntFoldersDeleteAsyncCommand");

        // Repository administration.
        Add(nameof(EnumRoles.Admin),
            "KntKAttributesSaveAsyncCommand", "KntKAttributesDeleteAsyncCommand",
            "KntNoteTypeSaveAsyncCommand", "KntNoteTypeDeleteAsyncCommand",
            "KntSystemValueSaveAsyncCommand", "KntSystemValueDeleteAsyncCommand",
            "KntTraceNoteTypeSaveAsyncCommand", "KntTraceNoteTypeDeleteAsyncCommand",
            "KntUsersSaveAsyncCommand", "KntUsersDeleteAsyncCommand", "KntUsersCreateAsyncCommand",
            "KntUsersSetPasswordAsyncCommand");

        return matrix;
    }

    private static List<Type> CommandTypes() =>
        typeof(KntService).Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && IsCommand(t))
            .ToList();

    private static bool IsCommand(Type type)
    {
        for (var t = type.BaseType; t != null; t = t.BaseType)
        {
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(KntCommandServiceBase<>))
                return true;
        }
        return false;
    }

    private static string? DeclaredRole(Type commandType)
    {
        var anonymous = commandType.GetCustomAttribute<KntAllowAnonymousAttribute>(inherit: false);
        var authorize = commandType.GetCustomAttribute<KntAuthorizeAttribute>(inherit: false);

        if (anonymous != null && authorize != null)
            return "both [KntAllowAnonymous] and [KntAuthorize]";
        if (anonymous != null)
            return Anonymous;
        return authorize?.MinimumRole.ToString();
    }

    [TestMethod]
    public void EveryCommand_DeclaresExactlyOneAuthorizationAttribute()
    {
        var undeclared = CommandTypes()
            .Where(t => DeclaredRole(t) is null or "both [KntAllowAnonymous] and [KntAuthorize]")
            .Select(t => t.Name)
            .ToList();

        Assert.AreEqual(0, undeclared.Count, "Commands without a single authorization attribute: " + string.Join(", ", undeclared));
    }

    [TestMethod]
    public void DeclaredRoles_MatchTheAuthorizationMatrix()
    {
        var declared = CommandTypes().ToDictionary(t => t.Name, DeclaredRole);

        var differences = new List<string>();
        foreach (var (command, role) in declared)
        {
            if (!ExpectedMatrix.TryGetValue(command, out var expected))
                differences.Add($"{command}: declares {role}, missing from the matrix");
            else if (expected != role)
                differences.Add($"{command}: declares {role}, the matrix says {expected}");
        }
        foreach (var command in ExpectedMatrix.Keys.Except(declared.Keys))
            differences.Add($"{command}: in the matrix, but no such command in KNote.Service");

        Assert.AreEqual(0, differences.Count, string.Join(Environment.NewLine, differences));
    }

    [TestMethod]
    public void Commands_UseTheRepositoryScope()
    {
        var applicationScoped = CommandTypes()
            .Where(t => t.GetCustomAttribute<KntAuthorizeAttribute>(inherit: false)?.Scope == AuthorizationScope.Application)
            .Select(t => t.Name)
            .ToList();

        Assert.AreEqual(0, applicationScoped.Count,
            "A command always acts on one repository: " + string.Join(", ", applicationScoped));
    }
}

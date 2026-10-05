using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Running code needs the ProjectManager role application-wide (Store.RunScriptsRequirement): every way of
/// running code (KntScript, C#/Python/JavaScript, a natural language request, a shell command) goes through
/// Store, which checks it first and tells the user when refused.
/// </summary>
[TestClass]
public class StoreScriptAuthorizationTests
{
    private static (Store store, List<string> notified) CreateStore(EnumRoles? role)
    {
        var store = new Store(new TestFactoryViews());
        if (role != null)
            store.Security.SetRepositoryRole(new FakeKntService(), role.Value);
        var notified = new List<string>();
        store.AccessDeniedNotifier = notified.Add;
        return (store, notified);
    }

    [TestMethod]
    [DataRow(null, false)]
    [DataRow(EnumRoles.Guest, false)]
    [DataRow(EnumRoles.Staff, false)]
    [DataRow(EnumRoles.ProjectManager, true)]
    [DataRow(EnumRoles.Admin, true)]
    public void CanRunScripts_TakesTheProjectManagerRole(EnumRoles? role, bool expected)
    {
        var (store, _) = CreateStore(role);

        Assert.AreEqual(expected, store.CanRunScripts);
    }

    [TestMethod]
    public void ExecuteCommand_WithoutTheRole_IsRefusedWithoutRunningAnything()
    {
        var (store, notified) = CreateStore(EnumRoles.Staff);

        var (output, error) = store.ExecuteCommand("cmd /c echo should-not-run", Path.GetTempPath());

        Assert.AreEqual("", output);
        StringAssert.Contains(error, "Not authorized");
        StringAssert.Contains(notified.Single(), "Project manager");
    }

    [TestMethod]
    public void RunPyCode_WithoutTheRole_IsRefused()
    {
        var (store, notified) = CreateStore(EnumRoles.Guest);

        var (output, _) = store.RunPyCode("print('should-not-run')", true);

        Assert.AreEqual("", output);
        Assert.AreEqual(1, notified.Count);
    }

    [TestMethod]
    public async Task RunCode_WithoutTheRole_IsRefusedBeforeLookingAtTheNote()
    {
        var (store, notified) = CreateStore(EnumRoles.Staff);
        var note = new NoteDto { Script = "printline \"should not run\";", ContentType = "markdown" };

        await store.RunCode(note);

        Assert.AreEqual(1, notified.Count);
    }

    [TestMethod]
    public async Task KNoteAssistantFromTheAIAssistant_WithoutTheRole_IsRefusedBeforeChoosingIt()
    {
        // A KNote assistant of the catalog is a KntScript script; refused before the catalog selector opens
        // (which would need a repository: none is set up here).
        var (store, notified) = CreateStore(EnumRoles.Staff);
        var ctrl = new KNote.ClientWin.Controllers.KNoteAIAssistantCtrl(store);

        await ctrl.ExecChatAssistant();

        StringAssert.Contains(notified.Single(), "Project manager");
    }

    [TestMethod]
    public async Task KNoteAssistantFromTheNoteEditor_WithoutTheRole_IsRefusedBeforeChoosingIt()
    {
        var (store, notified) = CreateStore(EnumRoles.Staff);
        var ctrl = new KNote.ClientWin.Controllers.NoteEditorCtrl(store);

        await ctrl.ExecKNoteAssistant();

        StringAssert.Contains(notified.Single(), "Project manager");
    }

    [TestMethod]
    public async Task RunNaturalLanguageCode_WithoutTheRole_IsRefused()
    {
        var (store, notified) = CreateStore(EnumRoles.Guest);

        await store.RunNaturalLanguageCode("Summarize this note.");

        Assert.AreEqual(1, notified.Count);
    }
}

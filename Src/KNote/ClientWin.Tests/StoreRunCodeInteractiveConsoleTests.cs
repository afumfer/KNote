using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.ClientWin.Tests.Helpers;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Store.RunCode's cs/py/js dispatch (Phase 3 of the interactive-console work: scripts triggered
/// from a note/alarm/KNoteManagment, with runInNewTask=false - F5 in NoteEditor/KNoteManagment -
/// now open the same interactive KntScriptConsole already used by its own "Run" menu, instead of
/// shelling out to a bare, non-capturing process). This only tests that RunCode opens a
/// KntScriptConsoleCtrl correctly configured via ConfigureAutoRun - the console itself
/// (InteractiveScriptSession, the input box, the readline stdin-release shim) is already covered by
/// InteractiveScriptSessionTests and verified live against the running app; re-running a real
/// interpreter from here would just duplicate that coverage.
///
/// runInNewTask=true (Ctrl+F5) - the standalone OS console - is NOT covered here: it fires a real
/// process via an unobserved Task.Run(() => RunCSCode(...)) with no seam to intercept, so a test
/// would either need real process-launcher injection (real scope creep for what's a one-line
/// if/else) or actually spawn dotnet/python/node from a unit test. RunCSCode/RunPyCode/RunJsCode
/// themselves already have coverage (StoreRunPyCodeTests/StoreRunJsCodeTests); only the dispatch
/// branch that picks between them and the console is what's new here and worth a test.
/// </summary>
[TestClass]
public class StoreRunCodeInteractiveConsoleTests
{
    // GetIncludeGlobalCode (the pre-existing "experimental hack" every engine runs through) needs
    // a working assistant ServiceRef - a fake with an empty result is enough to reach the dispatch
    // under test without touching a real database.
    private static Store CreateStoreWithEmptyGlobalIncludes()
    {
        var store = TestStoreFactory.CreateEmpty();
        var fakeService = new FakeKntService();
        fakeService.NotesFake.GetFilterAsyncImpl = _ => Task.FromResult(new Result<List<NoteInfoDto>>(new List<NoteInfoDto>()));
        store.SetAssistantServiceRef(TestServiceRefFactory.CreateWithFakeService(fakeService));
        return store;
    }

    private static NoteDto CreateNote(string script, string forScript)
    {
        var note = new NoteDto { Script = script };
        note.SetContentTypeExt(new ContentTypeExt { ForDescription = "markdown", ForScript = forScript });
        return note;
    }

    [DataTestMethod]
    [DataRow("py")]
    [DataRow("js")]
    [DataRow("cs")]
    public async Task RunCode_ScriptEngine_OpensConsoleConfiguredForAutoRun(string forScript)
    {
        var store = CreateStoreWithEmptyGlobalIncludes();
        KntScriptConsoleCtrl capturedCtrl = null;
        var fakeView = new FakeKntScriptConsoleView();
        store.FactoryViews.Registry.Register<KntScriptConsoleCtrl, IViewBase>(c => { capturedCtrl = c; return fakeView; });

        var note = CreateNote("print('hi')", forScript);

        // F5 in NoteEditor/KNoteManagment (buttonExecuteKntScript/menuExecuteCode) -> the embedded
        // console. Ctrl+F5 (.../InNewTask) -> the standalone OS console instead, see class summary.
        await store.RunCode(note, runInNewTask: false);

        Assert.IsNotNull(capturedCtrl, "Store.RunCode did not open a KntScriptConsoleCtrl.");
        Assert.IsTrue(capturedCtrl.AutoRunMode);
        Assert.AreEqual(forScript, capturedCtrl.AutoRunForScript);
        Assert.AreEqual("print('hi')", capturedCtrl.AutoRunCode);
        Assert.AreEqual(1, fakeView.ShowViewCallCount);
    }

    // Deliberately no "knt doesn't open the interactive console" test here: exercising Store.RunCode
    // for "knt" runs the real KntSEngine, which shows a real InOutDeviceForm window
    // (KntSEngine.Run() -> IInOutDevice.Show()) - not something to trigger from an automated test.
    // "knt" is untouched by this Phase 3 change anyway (still goes through the default branch,
    // unrelated to ShowInteractiveScriptConsole).

    // RunCodeInStdOutConsole's "unsupported engine" guard (SupportsStdOutConsole) short-circuits
    // before touching any process or view, so it's safe to test directly - unlike the "supported"
    // path (cs/py/js), which would launch a real dotnet/python/node process via an unobserved
    // Task.Run, same reason that path isn't covered above either.
    [DataTestMethod]
    [DataRow("knt")]
    [DataRow("ln")]
    public async Task RunCodeInStdOutConsole_UnsupportedEngine_ReturnsFalseWithoutRunningAnything(string forScript)
    {
        var store = CreateStoreWithEmptyGlobalIncludes();
        var note = CreateNote("irrelevant", forScript);

        var executed = await store.RunCodeInStdOutConsole(note);

        Assert.IsFalse(executed);
    }
}

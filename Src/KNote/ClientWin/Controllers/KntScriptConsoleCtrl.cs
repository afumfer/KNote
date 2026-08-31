using KNote.ClientWin.Core;
using KNote.ClientWin.Views;
using KNote.Model;
using KntScript;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace KNote.ClientWin.Controllers;

public class KntScriptConsoleCtrl : CtrlViewBase<IViewBase>
{
    #region Private fields

    private KntSEngine _kntSEngine;
    private InteractiveScriptSession _activeSession;

    #endregion

    #region Properties

    public string CodeFile { get; set; }

    public IInOutDevice KntScriptInOutDevice
    {
        get { return _kntSEngine.InOutDevice; }
    }

    public bool IsScriptRunning => _activeSession != null && _activeSession.IsRunning;

    // Set via ConfigureAutoRun before Run() - lets the view know it was opened programmatically
    // (Store.RunCode, for a script triggered from a note/alarm/KNoteManagment) rather than by the
    // user picking "KntScript console" from the Tools menu, so it can hide the editor/toolbar
    // (nothing to edit or save - the code came from the note, not from the user typing it here)
    // and start the run immediately instead of waiting for a manual "Run" click.
    public bool AutoRunMode { get; private set; }
    public string AutoRunCode { get; private set; }
    public string AutoRunForScript { get; private set; }

    #endregion

    #region Events

    // Relays InteractiveScriptSession.Exited (fires off the UI thread) so the view can re-enable
    // its toolbar/input box once a cs/py/js run actually finishes.
    public event EventHandler<int> ScriptExited;

    #endregion

    #region Constructor

    public KntScriptConsoleCtrl(Store store): base(store)
    {
        ControllerName = "KeyNote script console";
        _kntSEngine = new KntSEngine(new InOutDeviceForm(), new KNoteScriptLibrary(Store));
    }

    #endregion

    #region Controller overrid methods

    protected override IViewBase CreateView()
    {
        return Store.FactoryViews.Registry.Resolve<KntScriptConsoleCtrl, IViewBase>(this);
    }

    protected override Result<EControllerResult> OnFinalized()
    {
        // Safety net: if the view closes while a script is still running (waiting on input that
        // will never come), don't leave the process orphaned.
        _activeSession?.Dispose();
        _activeSession = null;
        return base.OnFinalized();
    }

    #endregion

    #region Public methods

    public void ConfigureAutoRun(string code, string forScript)
    {
        AutoRunMode = true;
        AutoRunCode = code;
        AutoRunForScript = forScript;
    }

    public void RunKntSCode(string code)
    {
        _kntSEngine.InOutDevice.Clear();
        _kntSEngine.ClearAllVars();
        _kntSEngine.Run(code);        
    }

    // Interactive: streams output live and accepts input (SendInput) while the process runs,
    // instead of blocking until it exits. See InteractiveScriptSession for the mechanics.
    public void RunCSCode(string code) => RunScriptInteractive(code, "cs", "dotnet run {0}");

    public void RunCSCodeStdOut(string code)
    {
        KntScriptInOutDevice.Clear();
        Store.RunCSCode(code, false);
        KntScriptInOutDevice.Print("\r\n\r\n----\r\nCS code executed.");
    }

    public void RunPyCode(string code) => RunScriptInteractive(code, "py", "python {0}");

    public void RunPyCodeStdOut(string code)
    {
        KntScriptInOutDevice.Clear();
        Store.RunPyCode(code, false);
        KntScriptInOutDevice.Print("\r\n\r\n----\r\nPython code executed.");
    }

    public void RunJsCode(string code) => RunScriptInteractive(code, "js", "node {0}");

    public void RunJsCodeStdOut(string code)
    {
        KntScriptInOutDevice.Clear();
        Store.RunJsCode(code, false);
        KntScriptInOutDevice.Print("\r\n\r\n----\r\nJavaScript code executed.");
    }

    public async Task RunNaturalLanguageCode(string prompt)
    {
        KntScriptInOutDevice.Clear();
        await Store.RunNaturalLanguageCode(prompt);
        KntScriptInOutDevice.Print("Natural language prompt sent to the KNote AI Assistant - see its window for the response.");
    }

    // Forwarded to the running process's stdin - a no-op if none is active. The view is
    // responsible for only allowing this while IsScriptRunning is true.
    public void SendInput(string text)
    {
        _activeSession?.SendInput(text);
    }

    // Signals end-of-input (Ctrl+Z equivalent) to the running process - needed for runtimes like
    // Node, whose event loop stays alive on an open stdin pipe even after the script's own logic
    // finished (see InteractiveScriptSession.CloseInput).
    public void CloseInput()
    {
        _activeSession?.CloseInput();
    }

    #endregion

    #region Private methods

    private void RunScriptInteractive(string code, string fileExtension, string runCommandTemplate)
    {
        KntScriptInOutDevice.Clear();

        // OutputReceived/ErrorReceived now deliver raw chunks, not whole lines (see
        // InteractiveScriptSession) - newLine: false, since any line breaks the process actually
        // wrote are already inside the chunk text itself; appending one here on every chunk would
        // inject a spurious blank line into the middle of the process's own output.
        var session = InteractiveScriptSession.Create(code, fileExtension, runCommandTemplate, Path.GetTempPath());
        session.OutputReceived += (s, chunk) => KntScriptInOutDevice.Print(chunk, newLine: false);
        session.ErrorReceived += (s, chunk) => KntScriptInOutDevice.Print(chunk, newLine: false);
        session.Exited += Session_Exited;

        _activeSession = session;
        session.Start();
    }

    private void Session_Exited(object sender, int exitCode)
    {
        KntScriptInOutDevice.Print($"\r\n----\r\nProcess exited (code {exitCode}).", newLine: true);
        ScriptExited?.Invoke(this, exitCode);
    }

    #endregion
}

using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KntScript;
using System.Runtime.InteropServices;
using KNote.ClientWin.Utils;
using KntWebView;
using KntIcons;

namespace KNote.ClientWin.Views;

internal partial class KntScriptConsoleForm : KntForm, IViewBase
{
    #region Private fields

    private readonly KntScriptConsoleCtrl _ctrl;

    private string _sourceCodeDirWork;

    // The code was edited since it was last loaded, saved or cleared (New).
    private bool _isDirty;

    // Language (Store.ScriptTypes code) -> source file extension, for Open/Save.
    private static readonly (string Code, string Extension, string Description)[] SourceFileTypes =
    {
        ("knt", ".knts", "KntScript file"),
        ("cs", ".cs", "C# file"),
        ("py", ".py", "Python file"),
        ("js", ".js", "JavaScript file"),
        ("ln", ".txt", "Natural language prompt"),
    };

    private const int EM_SETTABSTOPS = 0x00CB;
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr h, int msg, int wParam, int[] lParam);

    #endregion

    #region Constructor

    public KntScriptConsoleForm(KntScriptConsoleCtrl ctrl)
    {
        InitializeComponent();
        PersonalizeTabStop();
        KntEditView.UseMonospaceFont(textSourceCode);

        buttonNew.SetKntIcon(KntIcon.NewDocument);
        buttonOpen.SetKntIcon(KntIcon.FolderOpen);
        buttonSave.SetKntIcon(KntIcon.Save);
        buttonInsertCode.SetKntIcon(KntIcon.Script);
        buttonRun.SetKntIcon(KntIcon.Run);

        foreach (var scriptType in Store.ScriptTypes)
            comboLanguage.Items.Add(scriptType.Text);
        comboLanguage.SelectedIndex = 0;

        _ctrl = ctrl;
        _ctrl.ScriptExited += Ctrl_ScriptExited;
    }

    #endregion

    #region Form events controllers

    private void KntScriptForm_Load(object sender, EventArgs e)
    {
        _ctrl.KntScriptInOutDevice.SetEmbeddedMode();
        var outputView = (Control)_ctrl.KntScriptInOutDevice;
        splitContainer1.Panel2.Controls.Add(outputView);
        // Docked controls are laid out from the back of the z-order: brought to the front, the output (Fill)
        // gets what panelInput (Bottom) leaves, instead of running under it and losing its bottom border.
        outputView.BringToFront();

        if (_ctrl.AutoRunMode)
            ApplyAutoRunMode();
        else
            LoadFile(_ctrl.CodeFile);

        _ctrl.KntScriptInOutDevice.Show();
    }

    // Auto-run mode has no run actions (toolStripConsole is hidden - textSourceCode holds the note's
    // cs/py/js AutoRunCode, already running). Without this guard, an F5 KeyUp can still land on this
    // form right after it opens (it grabs focus mid-keystroke from the very F5 press in
    // NoteEditor/KNoteManagement that triggered the auto-run), starting a second run of that source
    // by mistake - with the KntScript engine, KntSEngine.Run swallowed the resulting parser/scanner
    // exception and printed its message straight into the shared KntScriptInOutDevice with no
    // trailing newline, concatenated with whatever the real interactive session printed next
    // (e.g. "expected EOFEnter your name: ").
    private async void KntScriptForm_KeyUp(object sender, KeyEventArgs e)
    {
        if (_ctrl.AutoRunMode || !toolStripConsole.Enabled)
            return;

        if (e.KeyData == Keys.F5)
            await RunInIntegratedConsole();
        else if (e.KeyData == (Keys.Shift | Keys.F5) && buttonRunStdOut.Enabled)
            await RunInStdOut();
    }

    private void comboLanguage_SelectedIndexChanged(object sender, EventArgs e)
    {
        // "...in standard output" only makes sense for engines that shell out to a real OS process
        // (cs/py/js) - disabled for knt/ln, which have no OS console to speak of.
        buttonRunStdOut.Enabled = Store.SupportsStdOutConsole(SelectedLanguage);
    }

    private async void buttonRunIntegrated_Click(object sender, EventArgs e)
    {
        await RunInIntegratedConsole();
    }

    private async void buttonRunStdOut_Click(object sender, EventArgs e)
    {
        await RunInStdOut();
    }

    private async void buttonInsertCode_Click(object sender, EventArgs e)
    {
        var codeSnippet = await _ctrl.GetCatalogCode();
        if (codeSnippet == null || string.IsNullOrEmpty(codeSnippet.Script))
            return;

        SetLanguage(codeSnippet.GetContentTypeExt()?.ForScript);

        // SelectedText replaces the selection (if any) at the caret, scrolls to it and keeps Undo.
        textSourceCode.SelectedText = codeSnippet.Script.ReplaceLineEndings("\r\n");
        textSourceCode.Focus();
    }

    private void textSourceCode_TextChanged(object sender, EventArgs e)
    {
        _isDirty = true;
    }

    private void textInput_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
            return;

        e.SuppressKeyPress = true;

        // No IsNullOrEmpty guard on the text itself: pressing Enter with nothing typed is a
        // legitimate action on a real console - it sends an empty line, which is exactly what
        // input() needs to unblock and return "". Skipping the send here (as this used to do) is
        // why the embedded console needed an extra character before Enter would do anything, unlike
        // the stdout console.
        if (!_ctrl.IsScriptRunning)
            return;

        _ctrl.KntScriptInOutDevice.Print($"> {textInput.Text}", newLine: true);
        _ctrl.SendInput(textInput.Text);
        textInput.Clear();
    }

    private void buttonCloseInput_Click(object sender, EventArgs e)
    {
        _ctrl.CloseInput();
        SetInteractiveInputEnabled(false);
    }

    // InteractiveScriptSession.Exited (relayed via Ctrl.ScriptExited) fires on the ThreadPool
    // "wait thread" .NET uses to raise Process.Exited - marshal before touching any control, and
    // do it with BeginInvoke (post and return immediately), never Invoke (block until done): in
    // auto-run mode this handler ends up disposing the very same Process object whose Exited event
    // is what's running right now (Close() -> FormClosing -> Ctrl.Finalize() -> Dispose() of the
    // InteractiveScriptSession/Process that just exited). Invoke would block that same wait thread
    // until Close() finished, and Close() can't finish until that wait thread's own Process-internal
    // bookkeeping is done - a real deadlock, confirmed live (Close() calling Invoke'd but never
    // returning) before switching to BeginInvoke fixed it.
    private void Ctrl_ScriptExited(object sender, int exitCode)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => Ctrl_ScriptExited(sender, exitCode)));
            return;
        }

        // Auto-run mode (opened programmatically for a single note's script, editor/toolbar
        // already hidden) closes itself when the script finishes, same as the standalone OS
        // console window does. The manually-opened console (Tools > KntScript console) keeps
        // itself open instead - closing it would also throw away whatever the user is editing.
        if (_ctrl.AutoRunMode)
        {
            Close();
            return;
        }

        RefreshStatusAction(false);
        SetInteractiveInputEnabled(false);
    }

    private void buttonNew_Click(object sender, EventArgs e)
    {
        if (!ConfirmDiscardChanges())
            return;

        _ctrl.CodeFile = "";
        SetSourceCode("");
        statusFileName.Text = "";
    }

    private void buttonOpen_Click(object sender, EventArgs e)
    {
        if (!ConfirmDiscardChanges())
            return;

        if (string.IsNullOrEmpty(_sourceCodeDirWork))
            _sourceCodeDirWork = Application.StartupPath;
        openFileDialogScript.Title = "Open script file";
        openFileDialogScript.InitialDirectory = _sourceCodeDirWork;
        openFileDialogScript.Filter =
            $"Script files|{string.Join(";", SourceFileTypes.Select(t => "*" + t.Extension))}|{SourceFileFilter()}";
        openFileDialogScript.FileName = "";
        openFileDialogScript.CheckFileExists = true;

        if (openFileDialogScript.ShowDialog() == DialogResult.OK)
            LoadFile(openFileDialogScript.FileName);
    }

    private void buttonSave_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_ctrl.CodeFile))
        {
            var fileTypeIndex = Math.Max(0, Array.FindIndex(SourceFileTypes, t => t.Code == SelectedLanguage));

            saveFileDialogScript.Title = "Save script file";
            saveFileDialogScript.InitialDirectory = _sourceCodeDirWork;
            saveFileDialogScript.Filter = SourceFileFilter();
            saveFileDialogScript.FilterIndex = fileTypeIndex + 1;
            saveFileDialogScript.FileName = "";

            if (saveFileDialogScript.ShowDialog() == DialogResult.OK)
            {
                if (Path.GetExtension(saveFileDialogScript.FileName) == "")
                    saveFileDialogScript.FileName += SourceFileTypes[fileTypeIndex].Extension;
                _ctrl.CodeFile = saveFileDialogScript.FileName;
                _sourceCodeDirWork = Path.GetDirectoryName(_ctrl.CodeFile);
                SaveFile(_ctrl.CodeFile);
                statusFileName.Text = _ctrl.CodeFile;
            }
        }
        else
            SaveFile(_ctrl.CodeFile);
    }

    // Only when the user closes the window: a close requested by the application (OnClosingView)
    // doesn't get here, and auto-run mode has no code of the user's to lose.
    protected override void OnUserClosing(FormClosingEventArgs e)
    {
        if (!_ctrl.AutoRunMode && e.CloseReason == CloseReason.UserClosing && !ConfirmDiscardChanges())
        {
            e.Cancel = true;
            return;
        }

        _ctrl.Finalize();
    }

    #endregion

    #region Private methods

    private string SelectedLanguage => Store.ScriptTypes[comboLanguage.SelectedIndex].Code;

    // Unknown or empty codes (e.g. a snippet with no script type) leave the current language as is.
    private void SetLanguage(string forScript)
    {
        var index = Array.FindIndex(Store.ScriptTypes, t => t.Code == forScript);
        if (index >= 0)
            comboLanguage.SelectedIndex = index;
    }

    private static string SourceFileFilter() =>
        string.Join("|", SourceFileTypes.Select(t => $"{t.Description} (*{t.Extension})|*{t.Extension}"));

    // knt and ln (the AI assistant answers in its own window) wait for the run to finish; cs/py/js are
    // interactive: they return as soon as the process is launched, and the toolbar/input box are
    // re-enabled later from Ctrl_ScriptExited.
    private async Task RunInIntegratedConsole()
    {
        switch (SelectedLanguage)
        {
            case "cs": RunInteractive(_ctrl.RunCSCode); break;
            case "py": RunInteractive(_ctrl.RunPyCode); break;
            case "js": RunInteractive(_ctrl.RunJsCode); break;
            case "ln": await RunAndWait(_ctrl.RunNaturalLanguageCode); break;
            default: await RunAndWait(code => { _ctrl.RunKntSCode(code); return Task.CompletedTask; }); break;
        }
    }

    private async Task RunInStdOut()
    {
        switch (SelectedLanguage)
        {
            case "cs": await RunAndWait(code => { _ctrl.RunCSCodeStdOut(code); return Task.CompletedTask; }); break;
            case "py": await RunAndWait(code => { _ctrl.RunPyCodeStdOut(code); return Task.CompletedTask; }); break;
            case "js": await RunAndWait(code => { _ctrl.RunJsCodeStdOut(code); return Task.CompletedTask; }); break;
        }
    }

    private async Task RunAndWait(Func<string, Task> run)
    {
        if (!HasCodeToRun())
            return;

        try
        {
            RefreshStatusAction(true);
            await run(textSourceCode.Text);
        }
        catch (Exception err)
        {
            KntMessageBox.Show(err.Message);
        }
        finally
        {
            RefreshStatusAction(false);
        }
    }

    private void RunInteractive(Action<string> run)
    {
        if (!TryStartInteractiveRun())
            return;

        run(textSourceCode.Text);
    }

    private bool HasCodeToRun()
    {
        if (!string.IsNullOrEmpty(textSourceCode.Text.Trim()))
            return true;

        KntMessageBox.Show("No code found to run", "KntScript");
        return false;
    }

    private bool ConfirmDiscardChanges()
    {
        return !_isDirty
            || ShowInfo("The code has unsaved changes. Do you want to discard them?", "KntScript",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    }

    // Replaces the whole code without counting as an edit of the user's.
    private void SetSourceCode(string code)
    {
        textSourceCode.Text = code;
        _isDirty = false;
    }

    private void PersonalizeTabStop()
    {
        // define value of the Tab indent and change the indent
        int[] stops = { 12 };
        SendMessage(this.textSourceCode.Handle, EM_SETTABSTOPS, 1, stops);
    }

    private void LoadFile(string sourceCodeFile)
    {
        if (string.IsNullOrEmpty(sourceCodeFile))
            return;

        if (File.Exists(sourceCodeFile))
        {
            SetSourceCode(File.ReadAllText(sourceCodeFile));
            _ctrl.CodeFile = sourceCodeFile;
            _sourceCodeDirWork = Path.GetDirectoryName(sourceCodeFile);
            textSourceCode.Select(0, 0);
            statusFileName.Text = _ctrl.CodeFile;

            var extension = Path.GetExtension(sourceCodeFile);
            SetLanguage(SourceFileTypes.FirstOrDefault(t => string.Equals(t.Extension, extension, StringComparison.OrdinalIgnoreCase)).Code);
        }
        else
        {
            ShowInfo("Source code file no exist.");
        }
    }

    private void SaveFile(string sourceCodeFile)
    {
        try
        {
            File.WriteAllLines(sourceCodeFile, textSourceCode.Lines);
            _isDirty = false;
        }
        catch (Exception err)
        {
            KntMessageBox.Show(err.Message);
        }
    }

    private void RefreshStatusAction(bool running)
    {
        if (running)
        {
            statusAction.Text = "Running code ... ";
            statusStripKntConsole.Refresh();
            toolStripConsole.Enabled = false;
        }
        else
        {
            statusAction.Text = "";
            statusStripKntConsole.Refresh();
            toolStripConsole.Enabled = true;
        }
    }

    // Shared precondition + status setup for the interactive cs/py/js runs (RunInteractive and
    // StartAutoRun): validates there's code to run and that no interactive session is already
    // active, then flips the UI into "running" state. The actual RunXxCode(...) call is left to the
    // caller since it differs per engine.
    private bool TryStartInteractiveRun()
    {
        if (!HasCodeToRun())
            return false;

        if (_ctrl.IsScriptRunning)
        {
            KntMessageBox.Show("A script is already running - use \"Close stdin\" or wait for it to finish before starting another one.", "KntScript");
            return false;
        }

        RefreshStatusAction(true);
        SetInteractiveInputEnabled(true);
        return true;
    }

    private void SetInteractiveInputEnabled(bool enabled)
    {
        textInput.Enabled = enabled;
        buttonCloseInput.Enabled = enabled;
        if (enabled)
            textInput.Focus();
        else
            textInput.Clear();
    }

    // Opened programmatically (Store.RunCode, for a note/alarm/KNoteManagement-triggered cs/py/js
    // script) instead of by the user picking "KntScript console" from the Tools menu: nothing here
    // came from typing into this window, and there's nothing to New/Open/Save, so that whole
    // toolbar - and the source editor next to it, which would otherwise sit there empty/unused -
    // are hidden, leaving only the output+input panel this mode actually needs.
    private void ApplyAutoRunMode()
    {
        toolStripConsole.Visible = false;
        splitContainer1.Panel1Collapsed = true;
        Text = $"KntScript - Console ({_ctrl.AutoRunForScript})";

        SetSourceCode(_ctrl.AutoRunCode);
        statusFileName.Text = "";

        StartAutoRun();
    }

    private void StartAutoRun()
    {
        if (!TryStartInteractiveRun())
            return;

        switch (_ctrl.AutoRunForScript)
        {
            case "cs": _ctrl.RunCSCode(_ctrl.AutoRunCode); break;
            case "py": _ctrl.RunPyCode(_ctrl.AutoRunCode); break;
            case "js": _ctrl.RunJsCode(_ctrl.AutoRunCode); break;
        }
    }

    #endregion

    #region IView interface

    public override void RefreshView()
    {
        throw new NotImplementedException();
    }

    #endregion

}

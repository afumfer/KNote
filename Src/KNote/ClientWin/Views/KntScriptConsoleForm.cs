using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KntScript;
using System.Runtime.InteropServices;

namespace KNote.ClientWin.Views;

internal partial class KntScriptConsoleForm : Form, IViewBase
{
    #region Private fields

    private readonly KntScriptConsoleCtrl _ctrl;
    private bool _viewFinalized = false;

    private string _sourceCodeDirWork;

    private const int EM_SETTABSTOPS = 0x00CB;
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr h, int msg, int wParam, int[] lParam);

    #endregion

    #region Constructor

    public KntScriptConsoleForm(KntScriptConsoleCtrl ctrl)
    {
        InitializeComponent();
        PersonalizeTabStop();

        _ctrl = ctrl;
        _ctrl.ScriptExited += Ctrl_ScriptExited;
    }

    #endregion

    #region Form events controllers

    private void KntScriptForm_Load(object sender, EventArgs e)
    {
        _ctrl.KntScriptInOutDevice.SetEmbeddedMode();
        splitContainer1.Panel2.Controls.Add((Control)_ctrl.KntScriptInOutDevice);

        if (_ctrl.AutoRunMode)
            ApplyAutoRunMode();
        else
            LoadFile(_ctrl.CodeFile);

        _ctrl.KntScriptInOutDevice.Show();
    }

    // Auto-run mode has no "Run KntScript" action (toolStripConsole is hidden - there's no KntScript
    // source here, textSourceCode holds the note's cs/py/js AutoRunCode instead). Without this guard,
    // an F5 KeyUp can still land on this form right after it opens (it grabs focus mid-keystroke from
    // the very F5 press in NoteEditor/KNoteManagment that triggered the auto-run), firing
    // buttonRunKntSCode_Click and running the cs/py/js source through the KntScript engine by mistake.
    // KntSEngine.Run swallows the resulting parser/scanner exception and prints its message straight
    // into the shared KntScriptInOutDevice with no trailing newline, so it lands concatenated with
    // whatever the real interactive session prints next (e.g. "expected EOFEnter your name: ").
    private void KntScriptForm_KeyUp(object sender, KeyEventArgs e)
    {
        if (_ctrl.AutoRunMode)
            return;

        if (e.KeyData == Keys.F5)
            buttonRunKntSCode_Click(this, new EventArgs());
    }

    private void buttonRunKntSCode_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(textSourceCode.Text.Trim()))
        {
            MessageBox.Show("No code found to run", "KntScript");
            return;
        }

        try
        {            
            RefreshStatusAction(true);

            _ctrl.RunKntSCode(textSourceCode.Text);
        }
        catch (Exception err)
        {
            MessageBox.Show(err.Message);
        }
        finally
        {
            RefreshStatusAction(false);
        }
    }

    // cs/py/js (non-StdOut) are interactive: RunXxCode returns as soon as the process is
    // launched, not when it finishes, so the toolbar/input box are re-enabled later from
    // Ctrl_ScriptExited - not here in a finally, which would fire immediately after start.
    private void buttonRunCSCode_Click(object sender, EventArgs e)
    {
        if (!TryStartInteractiveRun())
            return;

        _ctrl.RunCSCode(textSourceCode.Text);
    }

    private void buttonRunCSCodeStdOut_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(textSourceCode.Text.Trim()))
        {
            MessageBox.Show("No code found to run", "KntScript");
            return;
        }

        try
        {
            RefreshStatusAction(true);
            _ctrl.RunCSCodeStdOut(textSourceCode.Text);
        }
        catch (Exception err)
        {
            MessageBox.Show(err.Message);
        }
        finally
        {
            RefreshStatusAction(false);
        }
    }

    private void buttonRunPyCode_Click(object sender, EventArgs e)
    {
        if (!TryStartInteractiveRun())
            return;

        _ctrl.RunPyCode(textSourceCode.Text);
    }

    private void buttonRunPyCodeStdOut_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(textSourceCode.Text.Trim()))
        {
            MessageBox.Show("No code found to run", "KntScript");
            return;
        }

        try
        {
            RefreshStatusAction(true);
            _ctrl.RunPyCodeStdOut(textSourceCode.Text);
        }
        catch (Exception err)
        {
            MessageBox.Show(err.Message);
        }
        finally
        {
            RefreshStatusAction(false);
        }
    }

    private void buttonRunJsCode_Click(object sender, EventArgs e)
    {
        if (!TryStartInteractiveRun())
            return;

        _ctrl.RunJsCode(textSourceCode.Text);
    }

    private void buttonRunJsCodeStdOut_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(textSourceCode.Text.Trim()))
        {
            MessageBox.Show("No code found to run", "KntScript");
            return;
        }

        try
        {
            RefreshStatusAction(true);
            _ctrl.RunJsCodeStdOut(textSourceCode.Text);
        }
        catch (Exception err)
        {
            MessageBox.Show(err.Message);
        }
        finally
        {
            RefreshStatusAction(false);
        }
    }

    private async void buttonRunNaturalLanguage_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(textSourceCode.Text.Trim()))
        {
            MessageBox.Show("No code found to run", "KntScript");
            return;
        }

        try
        {
            RefreshStatusAction(true);
            await _ctrl.RunNaturalLanguageCode(textSourceCode.Text);
        }
        catch (Exception err)
        {
            MessageBox.Show(err.Message);
        }
        finally
        {
            RefreshStatusAction(false);
        }
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
        _ctrl.CodeFile = "";
        textSourceCode.Text = "";
        statusFileName.Text = "";
    }

    private void buttonOpen_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_sourceCodeDirWork))
            _sourceCodeDirWork = Application.StartupPath;
        openFileDialogScript.Title = "Open KntScript file";
        openFileDialogScript.InitialDirectory = _sourceCodeDirWork;
        openFileDialogScript.Filter = "KntScript file (*.knts)|*.knts|CSharp file (*.cs)|*.cs";
        openFileDialogScript.FileName = "";
        openFileDialogScript.CheckFileExists = true;

        if (openFileDialogScript.ShowDialog() == DialogResult.OK)
            LoadFile(openFileDialogScript.FileName);
    }

    private void buttonSave_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_ctrl.CodeFile))
        {
            saveFileDialogScript.Title = "Save KntScript file";
            saveFileDialogScript.InitialDirectory = _sourceCodeDirWork;
            saveFileDialogScript.Filter = "KntScript file (*.ants)|*.ants|CSharp file (*.cs)|*.cs";
            saveFileDialogScript.FileName = "";

            if (saveFileDialogScript.ShowDialog() == DialogResult.OK)
            {
                if (Path.GetExtension(saveFileDialogScript.FileName) == "")
                    saveFileDialogScript.FileName += @".ants";
                _ctrl.CodeFile = saveFileDialogScript.FileName;
                _sourceCodeDirWork = Path.GetDirectoryName(_ctrl.CodeFile);
                SaveFile(_ctrl.CodeFile);
                statusFileName.Text = _ctrl.CodeFile;
            }
        }
        else
            SaveFile(_ctrl.CodeFile);
    }

    private void KntScriptConsoleForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!_viewFinalized)
            _ctrl.Finalize();
    }

    #endregion

    #region Private methods

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
            using (TextReader input = File.OpenText(sourceCodeFile))
            {
                textSourceCode.Text = input.ReadToEnd().ToString();
                _ctrl.CodeFile = sourceCodeFile;
                _sourceCodeDirWork = Path.GetDirectoryName(sourceCodeFile);
                textSourceCode.Select(0, 0);
                textSourceCode.Select(0, 0);
                statusFileName.Text = _ctrl.CodeFile;                
            }
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
        }
        catch (Exception err)
        {
            MessageBox.Show(err.Message);
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

    // Shared precondition + status setup for the interactive cs/py/js runs (buttonRunCSCode_Click/
    // buttonRunPyCode_Click/buttonRunJsCode_Click): validates there's code to run and that no
    // interactive session is already active, then flips the UI into "running" state. The actual
    // RunXxCode(...) call is left to the caller since it differs per engine.
    private bool TryStartInteractiveRun()
    {
        if (string.IsNullOrEmpty(textSourceCode.Text.Trim()))
        {
            MessageBox.Show("No code found to run", "KntScript");
            return false;
        }

        if (_ctrl.IsScriptRunning)
        {
            MessageBox.Show("A script is already running - use \"Close stdin\" or wait for it to finish before starting another one.", "KntScript");
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

    // Opened programmatically (Store.RunCode, for a note/alarm/KNoteManagment-triggered cs/py/js
    // script) instead of by the user picking "KntScript console" from the Tools menu: nothing here
    // came from typing into this window, and there's nothing to New/Open/Save, so that whole
    // toolbar - and the source editor next to it, which would otherwise sit there empty/unused -
    // are hidden, leaving only the output+input panel this mode actually needs.
    private void ApplyAutoRunMode()
    {
        toolStripConsole.Visible = false;
        splitContainer1.Panel1Collapsed = true;
        Text = $"KntScript - Console ({_ctrl.AutoRunForScript})";

        textSourceCode.Text = _ctrl.AutoRunCode;
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

    public void ShowView()
    {
        this.Show();
    }

    public Result<EControllerResult> ShowModalView()
    {
        return _ctrl.DialogResultToControllerResult(this.ShowDialog());
    }

    public void OnClosingView()
    {
        _viewFinalized = true;
        this.Close();
    }

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        return MessageBox.Show(info, caption, buttons, icon);
    }

    public void RefreshView()
    {
        throw new NotImplementedException();
    }

    #endregion

}
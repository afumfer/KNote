using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KntScript;
using ReverseMarkdown.Converters;
using System.Runtime.InteropServices;

namespace KNote.ClientWin.Views;

internal partial class KntScriptConsoleForm : Form, IViewBase
{
    #region Private fields

    private readonly KntScriptConsoleCtrl _ctrl;
    private bool _viewFinalized = false;

    private string _sourceCodeDirWork;
    private string _sourceCodeFile;
    private KntSEngine _kntScriptEngine;

    private const int EM_SETTABSTOPS = 0x00CB;
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr h, int msg, int wParam, int[] lParam);

    #endregion

    #region Constructor

    public KntScriptConsoleForm(KntScriptConsoleCtrl ctrl)
    {
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();
        PersonalizeTabStop();

        _ctrl = ctrl;
        _kntScriptEngine = _ctrl.KntSEngine;
        _sourceCodeFile = _ctrl.CodeFile;
    }

    #endregion

    #region Form events controllers

    private void KntScriptForm_Load(object sender, EventArgs e)
    {
        _kntScriptEngine.InOutDevice.SetEmbeddedMode();
        splitContainer1.Panel2.Controls.Add((Control)_kntScriptEngine.InOutDevice);

        LoadFile(_sourceCodeFile);

        _kntScriptEngine.InOutDevice.Show();        
    }

    private void KntScriptForm_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyData == Keys.F5)
            buttonRun_Click(this, new EventArgs());
    }

    private void buttonRun_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(textSourceCode.Text.Trim()))
        {
            MessageBox.Show("No code found to run", "KntScript");
            return;
        }

        try
        {            
            RefreshStatusAction(true);

            _kntScriptEngine.InOutDevice.Clear();
            _kntScriptEngine.ClearAllVars();
            _kntScriptEngine.Run(textSourceCode.Text);
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

    private void buttonRunCSCode_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(textSourceCode.Text.Trim()))
        {
            MessageBox.Show("No code found to run", "KntScript");
            return;
        }

        try
        {            
            RefreshStatusAction(true);
            _kntScriptEngine.InOutDevice.Clear();
            (var result, var error) = RunCSCode(textSourceCode.Text, true);
            _kntScriptEngine.InOutDevice.Print($"{result}\n\n{"----"}\n{error}");
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
            RunCSCode(textSourceCode.Text, false);
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

    private void buttonNew_Click(object sender, EventArgs e)
    {
        _sourceCodeFile = "";
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
        if (string.IsNullOrEmpty(_sourceCodeFile))
        {
            saveFileDialogScript.Title = "Save KntScript file";
            saveFileDialogScript.InitialDirectory = _sourceCodeDirWork;
            saveFileDialogScript.Filter = "KntScript file (*.ants)|*.ants|CSharp file (*.cs)|*.cs";
            saveFileDialogScript.FileName = "";

            if (saveFileDialogScript.ShowDialog() == DialogResult.OK)
            {
                if (Path.GetExtension(saveFileDialogScript.FileName) == "")
                    saveFileDialogScript.FileName += @".ants";
                _sourceCodeFile = saveFileDialogScript.FileName;
                _sourceCodeDirWork = Path.GetDirectoryName(_sourceCodeFile);
                SaveFile(_sourceCodeFile);
                statusFileName.Text = _sourceCodeFile;
            }
        }
        else
            SaveFile(_sourceCodeFile);
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
                _sourceCodeFile = sourceCodeFile;
                _sourceCodeDirWork = Path.GetDirectoryName(sourceCodeFile);
                textSourceCode.Select(0, 0);
                textSourceCode.Select(0, 0);
                statusFileName.Text = _sourceCodeFile;                
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

    private (string, string) RunCSCode(string code, bool redirectStandardOut)
    {
        string tempDir = Path.GetTempPath();
        //string nameFile = Guid.NewGuid().ToString() + ".cs";
        string nameFile = "kntTmpCodeFile.cs";
        string tempFullFileName = Path.Combine(tempDir, nameFile);
        File.WriteAllText(tempFullFileName, code);

        (var result, var error) = _ctrl.Store.ExecuteCommand($"dotnet run {nameFile}", tempDir, redirectStandardOut);

        File.Delete(tempFullFileName);

        return (result, error);
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
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
    }

    #endregion

    #region Form events controllers

    private void KntScriptForm_Load(object sender, EventArgs e)
    {
        _ctrl.KntScriptInOutDevice.SetEmbeddedMode();
        splitContainer1.Panel2.Controls.Add((Control)_ctrl.KntScriptInOutDevice);

        LoadFile(_ctrl.CodeFile);

        _ctrl.KntScriptInOutDevice.Show();        
    }

    private void KntScriptForm_KeyUp(object sender, KeyEventArgs e)
    {
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
            _ctrl.RunCSCode(textSourceCode.Text);
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
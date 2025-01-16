using System.Runtime.InteropServices;

using KNote.ClientWin.Core;
using KNote.Model;
using KNote.ClientWin.Controllers;
using KntScript;

namespace KNote.ClientWin.Views;

internal partial class KntScriptConsoleForm : Form, IViewBase
{
    #region Private fields

    private readonly KntScriptConsoleCtrl _ctrl;
    private bool _viewFinalized = false;

    private string _sourceCodeDirWork;
    private string _sourceCodeFile;
    private KntSEngine _engine;        

    private const int EM_SETTABSTOPS = 0x00CB; 
    [DllImport("User32.dll", CharSet = CharSet.Auto)] 
    private static extern IntPtr SendMessage(IntPtr h, int msg, int wParam, int [] lParam);

    #endregion

    #region Constructor

    public KntScriptConsoleForm(KntScriptConsoleCtrl ctrl)
    {
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();
        PersonalizeTabStop();

        _ctrl = ctrl;
        _engine = _ctrl.KntSEngine;
        _sourceCodeFile = _ctrl.CodeFile;            
    }

    #endregion

    #region Form events controllers

    private void KntScriptForm_Load(object sender, EventArgs e)
    {
        _engine.InOutDevice.SetEmbeddedMode();
        splitContainer1.Panel2.Controls.Add((Control)_engine.InOutDevice);

        LoadFile(_sourceCodeFile);

        _engine.InOutDevice.Show();            
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
            toolStripConsole.Enabled = false;

            _engine.InOutDevice.Clear();                
            _engine.ClearAllVars();
            _engine.Run(textSourceCode.Text);
        }
        catch (Exception err)
        {
            MessageBox.Show(err.Message);
        }
        finally
        {
            toolStripConsole.Enabled = true;
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
        openFileDialogScript.Filter = "KntScript file (*.knts)|*.knts";
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
            saveFileDialogScript.Filter = "KntScript file (*.ants)|*.ants";
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

        if (!string.IsNullOrEmpty(_sourceCodeFile))
        {
            if (File.Exists(_sourceCodeFile))
            {
                using (TextReader input = File.OpenText(_sourceCodeFile))
                {
                    textSourceCode.Text = input.ReadToEnd().ToString();
                    textSourceCode.Select(0, 0);
                }
            }
            else
            {
                throw new Exception("Source code file no exist.");
            }

        }
        
        _sourceCodeFile = sourceCodeFile;            
        statusFileName.Text = sourceCodeFile;

        textSourceCode.Select(0, 0);
        _sourceCodeDirWork = Path.GetDirectoryName(sourceCodeFile);
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

    #endregion

    #region IView interface 

    public void ShowView()
    {
        this.Show();
    }

    public Result<EComponentResult> ShowModalView()
    {
        return _ctrl.DialogResultToComponentResult(this.ShowDialog());
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
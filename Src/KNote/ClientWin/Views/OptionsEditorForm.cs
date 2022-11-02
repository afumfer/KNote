using KNote.ClientWin.Components;
using KNote.ClientWin.Core;
using KNote.Model;
using SQLitePCL;
using System.Windows.Forms;

namespace KNote.ClientWin.Views;

public partial class OptionsEditorForm : Form, IEditorView<AppConfig>
{
    #region Fields

    private readonly OptionsEditorComponent _com;
    private bool _viewFinalized = false;
    private bool _formIsDisty = false;

    #endregion

    #region Constructor 

    public OptionsEditorForm(OptionsEditorComponent com)
    {
        InitializeComponent();
        _com = com;
    }

    #endregion 

    #region IEditorView implementation 

    public Control PanelView()
    {
        return panelForm;
    }

    public void ShowView()
    {
        this.Show();
    }

    public Result<EComponentResult> ShowModalView()
    {
        return _com.DialogResultToComponentResult(this.ShowDialog());
    }

    public void RefreshView()
    {
        ModelToControls();
    }

    public void RefreshModel()
    {
        ControlsToModel();
    }

    public void CleanView()
    {
        // clear controls 
    }

    public DialogResult ShowInfo(string info, string caption = "KaNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Asterisk)
    {
        return MessageBox.Show(info, caption, buttons, icon);
    }

    public void OnClosingView()
    {
        _viewFinalized = true;
        this.Close();
    }

    public void ConfigureEmbededMode()
    {
            
    }

    public void ConfigureWindowMode()
    {
            
    }

    #endregion

    #region Form events handler

    private async void buttonAccept_Click(object sender, EventArgs e)
    {
        var res = await _com.SaveModel();
        if (res)
        {
            _formIsDisty = false;
            this.DialogResult = DialogResult.OK;
        }
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
        OnCancelEdition();
    }

    private void OptionsEditorForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!_viewFinalized)
        {
            var confirmExit = OnCancelEdition();
            if (!confirmExit)
                e.Cancel = true;
        }
    }

    private void OptionsEditorForm_KeyPress(object sender, KeyPressEventArgs e)
    {
        _formIsDisty = true;
    }

    private void OptionsEditorForm_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            _formIsDisty = true;
    }

    #endregion 

    #region Private methods

    private bool OnCancelEdition()
    {
        if (_formIsDisty)
        {
            if (MessageBox.Show("You have modified this entity, are you sure you want to exit without recording?", "KaNote", MessageBoxButtons.YesNo) == DialogResult.No)
                return false;
        }

        this.DialogResult = DialogResult.Cancel;
        _com.CancelEdition();
        return true;
    }

    private void ModelToControls() 
    {
        checkAlarmActivated.Checked = _com.Model.AlarmActivated;
        textAlarmSeconds.Text = _com.Model.AlarmSeconds.ToString();
        checkAutoSaveActivated.Checked = _com.Model.AutoSaveActivated;
        textAutosaveSeconds.Text = _com.Model.AutoSaveSeconds.ToString();
        checkCompactViewNotesList.Checked = _com.Model.CompactViewNoteslist;
        textChatHubUrl.Text = _com.Model.ChatHubUrl;

        //var x5 = _com.Model.LogActivated;
        //var x6 = _com.Model.LogFile;
    }

    private void ControlsToModel() 
    {
        _com.Model.AlarmActivated = checkAlarmActivated.Checked;
        _com.Model.AlarmSeconds = int.Parse(textAlarmSeconds.Text);
        _com.Model.AutoSaveActivated = checkAutoSaveActivated.Checked;
        _com.Model.AutoSaveSeconds = int.Parse(textAutosaveSeconds.Text);
        _com.Model.CompactViewNoteslist = checkCompactViewNotesList.Checked;
        _com.Model.ChatHubUrl = textChatHubUrl.Text;
    }

    #endregion
}


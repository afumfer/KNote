using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Views;

public partial class OptionsEditorForm : Form, IViewEditor<AppConfig>
{
    #region Privage Fields

    private readonly OptionsEditorCtrl _ctrl;
    private bool _viewFinalized = false;
    private bool _formIsDisty = false;

    #endregion

    #region Constructor 

    public OptionsEditorForm(OptionsEditorCtrl ctrl)
    {
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();
        this.Text = $"{KntConst.AppName} options";

        _ctrl = ctrl;
    }

    #endregion 

    #region IEditorView implementation 

    public void ShowView()
    {
        this.Show();
    }

    public Result<EControllerResult> ShowModalView()
    {
        return _ctrl.DialogResultToControllerResult(this.ShowDialog());
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

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Asterisk)
    {
        return MessageBox.Show(info, caption, buttons, icon);
    }

    public void OnClosingView()
    {
        _viewFinalized = true;
        this.Close();
    }

    #endregion

    #region Form events handler

    private async void buttonAccept_Click(object sender, EventArgs e)
    {
        var res = await _ctrl.SaveModel();
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
            if (MessageBox.Show("You have modified this entity, are you sure you want to exit without recording?", KntConst.AppName, MessageBoxButtons.YesNo) == DialogResult.No)
                return false;
        }

        this.DialogResult = DialogResult.Cancel;
        _ctrl.CancelEdition();
        return true;
    }

    private void ModelToControls() 
    {
        checkAlarmActivated.Checked = _ctrl.Model.AlarmActivated;
        textAlarmSeconds.Text = _ctrl.Model.AlarmSeconds.ToString();
        checkAutoSaveActivated.Checked = _ctrl.Model.AutoSaveActivated;
        textAutosaveSeconds.Text = _ctrl.Model.AutoSaveSeconds.ToString();
        checkCompactViewNotesList.Checked = _ctrl.Model.CompactViewNoteslist;
        textChatHubUrl.Text = _ctrl.Model.ChatHubUrl;

        //var x5 = _ctrl.Model.LogActivated;
        //var x6 = _ctrl.Model.LogFile;
    }

    private void ControlsToModel() 
    {
        _ctrl.Model.AlarmActivated = checkAlarmActivated.Checked;
        _ctrl.Model.AlarmSeconds = int.Parse(textAlarmSeconds.Text);
        _ctrl.Model.AutoSaveActivated = checkAutoSaveActivated.Checked;
        _ctrl.Model.AutoSaveSeconds = int.Parse(textAutosaveSeconds.Text);
        _ctrl.Model.CompactViewNoteslist = checkCompactViewNotesList.Checked;
        _ctrl.Model.ChatHubUrl = textChatHubUrl.Text;
    }

    #endregion
}


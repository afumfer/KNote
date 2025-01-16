using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

public partial class MessageEditorForm : Form, IViewEditor<KMessageDto>
{
    #region Private fields

    private readonly MessageEditorCtrl _ctrl;
    private bool _viewFinalized = false;
    private bool _formIsDisty = false;

    #endregion

    #region Constructor 

    public MessageEditorForm(MessageEditorCtrl ctrl)
    {
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();
        PersonalizeControls();

        _ctrl = ctrl;
    }

    #endregion

    #region IEditorView implementation 

    public void ShowView()
    {
        this.Show();
    }

    public Result<EComponentResult> ShowModalView()
    {
        var res = _ctrl.DialogResultToComponentResult(this.ShowDialog());
        return res;
    }

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        return MessageBox.Show(info, caption, buttons, icon);
    }

    public void RefreshView()
    {
        ModelToControls();
    }

    public void CleanView()
    {
        textUserFullName.Text = "";
        textAlarmDateTime.Text = "";
        comboAlarmPeriodicity.SelectedIndex = 0;
        comboNotificationType.SelectedIndex = 0;
        textContent.Text = "";
        checkAlarmActivated.Checked = true;
    }

    public void RefreshModel()
    {
        ControlsToModel();
    }

    public void OnClosingView()
    {
        _viewFinalized = true;
        this.Close();
    }

    #endregion

    #region Form event handlers

    private void MessageEditorForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!_viewFinalized)
        {
            var confirmExit = OnCandelEdition();
            if (!confirmExit)
                e.Cancel = true;            
        }
    }

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
        OnCandelEdition();
    }

    private void MessageEditorForm_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            _formIsDisty = true;
    }

    private void MessageEditorForm_KeyPress(object sender, KeyPressEventArgs e)
    {
        _formIsDisty = true;
    }

    private void MessageEditorForm_Load(object sender, EventArgs e)
    {

    }

    private void buttonSelectDate_Click(object sender, EventArgs e)
    {
        DateTime selDate;
        if (!DateTime.TryParse(textAlarmDateTime.Text, out selDate))
            selDate = DateTime.Now;

        DateSelectorForm dateSelector = new DateSelectorForm();
        dateSelector.Date = selDate;

        if (dateSelector.ShowDialog() == DialogResult.OK)
            textAlarmDateTime.Text = dateSelector.Date.ToString("dd/MM/yyyy HH:mm");
    }

    #endregion

    #region Private methods

    private bool OnCandelEdition()
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

    private void PersonalizeControls()
    {
        foreach (var alarmType in KntConst.AlarmType)
            comboAlarmPeriodicity.Items.Add(alarmType);
        comboAlarmPeriodicity.ValueMember = "Key";
        comboAlarmPeriodicity.DisplayMember = "Value";
        comboAlarmPeriodicity.SelectedIndex = 0;

        foreach (var notType in KntConst.NotificationType)
            comboNotificationType.Items.Add(notType);
        comboNotificationType.ValueMember = "Key";
        comboNotificationType.DisplayMember = "Value";
        comboNotificationType.SelectedIndex = 0;
    }

    private void ModelToControls()
    {
        textUserFullName.Text = _ctrl.Model.UserFullName?.ToString();
        textAlarmDateTime.Text = _ctrl.Model.AlarmDateTime.ToString();
        comboAlarmPeriodicity.SelectedIndex = (int)_ctrl.Model.AlarmType;
        comboNotificationType.SelectedIndex = (int)_ctrl.Model.NotificationType;
        textContent.Text = _ctrl.Model.Comment.ToString();
        checkAlarmActivated.Checked = _ctrl.Model.AlarmActivated ?? false;
        textMinutes.Text = _ctrl.Model.AlarmMinutes?.ToString();            
    }

    private void ControlsToModel()
    {
        _ctrl.Model.AlarmDateTime = _ctrl.Store.TextToDateTime(textAlarmDateTime.Text);
        _ctrl.Model.AlarmType = (EnumAlarmType)comboAlarmPeriodicity.SelectedIndex;
        _ctrl.Model.NotificationType = (EnumNotificationType)comboNotificationType.SelectedIndex;
        _ctrl.Model.Comment = textContent.Text;
        _ctrl.Model.AlarmActivated = checkAlarmActivated.Checked;
        _ctrl.Model.AlarmMinutes = _ctrl.Store.TextToInt(textMinutes.Text);
    }
    private void comboAlarmPeriodicity_SelectedIndexChanged(object sender, EventArgs e)
    {
        var selected = (KeyValuePair<EnumAlarmType,string>)comboAlarmPeriodicity.SelectedItem;
        
        if (selected.Key == EnumAlarmType.InMinutes)
        {
            textMinutes.Visible = true;
            labelMinutes.Visible = true;
        }
        else
        {
            textMinutes.Visible = false;
            labelMinutes.Visible = false;
        }
    }

    #endregion
}

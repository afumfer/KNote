using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

public partial class TaskEditorForm : Form, IViewEditor<NoteTaskDto>
{
    #region Private fields

    private readonly TaskEditorCtrl _ctrl;
    private bool _viewFinalized = false;
    private bool _formIsDisty = false;

    #endregion

    #region Constructor

    public TaskEditorForm(TaskEditorCtrl com)
    {
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();

        _ctrl = com;
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

    public void OnClosingView()
    {
        _viewFinalized = true;
        this.Close();
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
        textUser.Text = "";
        textTags.Text = "";
        textPriority.Text = "";
        textEstimatedTime.Text = "";
        textSpendTime.Text = "";
        textExStartDate.Text = "";
        textExEndDate.Text = "";
        textDificultyLevel.Text = "";
        textStartDate.Text = "";
        textEndDate.Text = "";
        checkResolved.Checked = false;
        textDescription.Text = "";
    }

    #endregion

    #region Form event handelrs

    private void TaskEditorForm_FormClosing(object sender, FormClosingEventArgs e)
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

    private void TaskEditorForm_KeyPress(object sender, KeyPressEventArgs e)
    {
        _formIsDisty = true;
    }

    private void TaskEditorForm_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            _formIsDisty = true;
    }

    private void buttonSelDate_Click(object sender, EventArgs e)
    {
        Button buttonSel;
        buttonSel = (Button)sender;

        if (buttonSel == buttonSelDateExS)
        {
            textExStartDate.Text = SelDate(textExStartDate.Text);
        }
        else if (buttonSel == buttonSelDateExE)
        {
            textExEndDate.Text = SelDate(textExEndDate.Text);
        }
        else if (buttonSel == buttonSelDateStart)
        {
            textStartDate.Text = SelDate(textStartDate.Text);
        }
        else if (buttonSel == buttonSelDateEnd)
        {
            textEndDate.Text = SelDate(textEndDate.Text);
        }
    }

    private void checkResolved_Click(object sender, EventArgs e)
    {
        if (checkResolved.Checked == true)
            if (string.IsNullOrEmpty(textEndDate.Text))
                textEndDate.Text = DateTime.Now.ToString();
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

    private void ModelToControls()
    {
        textUser.Text = _ctrl.Model.UserFullName;
        textTags.Text = _ctrl.Model.Tags;
        textPriority.Text = _ctrl.Model.Priority.ToString();
        textEstimatedTime.Text = _ctrl.Model.EstimatedTime?.ToString();
        textSpendTime.Text = _ctrl.Model.SpentTime?.ToString();
        textExStartDate.Text = _ctrl.Model.ExpectedStartDate?.ToString();
        textExEndDate.Text = _ctrl.Model.ExpectedEndDate?.ToString();
        textDificultyLevel.Text = _ctrl.Model.DifficultyLevel?.ToString();
        textStartDate.Text = _ctrl.Model.StartDate?.ToString();
        textEndDate.Text = _ctrl.Model.EndDate?.ToString();
        checkResolved.Checked = _ctrl.Model.Resolved;
        textDescription.Text = _ctrl.Model.Description;
    }

    private void ControlsToModel()
    {
        _ctrl.Model.UserFullName = textUser.Text;
        _ctrl.Model.Tags = textTags.Text;
        _ctrl.Model.Priority = _ctrl.Store.TextToInt(textPriority.Text);

        _ctrl.Model.EstimatedTime = _ctrl.Store.TextToDouble(textEstimatedTime.Text);
        _ctrl.Model.SpentTime = _ctrl.Store.TextToDouble(textSpendTime.Text);
        _ctrl.Model.DifficultyLevel = _ctrl.Store.TextToDouble(textDificultyLevel.Text);

        _ctrl.Model.ExpectedStartDate = _ctrl.Store.TextToDateTime(textExStartDate.Text);
        _ctrl.Model.ExpectedEndDate = _ctrl.Store.TextToDateTime(textExEndDate.Text);
        _ctrl.Model.StartDate = _ctrl.Store.TextToDateTime(textStartDate.Text);
        _ctrl.Model.EndDate = _ctrl.Store.TextToDateTime(textEndDate.Text);

        _ctrl.Model.Resolved = checkResolved.Checked;
        _ctrl.Model.Description = textDescription.Text;
    }

    private string SelDate(string date)
    {
        DateTime selDateIn;
        string selDate = "";

        if (!DateTime.TryParse(date, out selDateIn))
            selDateIn = DateTime.Now;

        DateSelectorForm dateSelector = new DateSelectorForm();
        dateSelector.Date = selDateIn;

        if (dateSelector.ShowDialog() == DialogResult.OK)
            selDate = dateSelector.Date.ToString("dd/MM/yyyy HH:mm");

        return selDate;
    }

    #endregion
}

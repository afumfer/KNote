using KNote.ClientWin.Components;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

public partial class TaskEditorForm : Form, IViewEditor<NoteTaskDto>
{
    #region Private fields

    private readonly TaskEditorComponent _com;
    private bool _viewFinalized = false;
    private bool _formIsDisty = false;

    #endregion

    #region Constructor

    public TaskEditorForm(TaskEditorComponent com)
    {
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();

        _com = com;
    }

    #endregion

    #region IEditorView implementation 

    public void ShowView()
    {
        this.Show();
    }

    public Result<EComponentResult> ShowModalView()
    {
        var res = _com.DialogResultToComponentResult(this.ShowDialog());
        return res;
    }

    public DialogResult ShowInfo(string info, string caption = "KaNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
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
        var res = await _com.SaveModel();
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
            if (MessageBox.Show("You have modified this entity, are you sure you want to exit without recording?", "KaNote", MessageBoxButtons.YesNo) == DialogResult.No)
                return false;
        }

        this.DialogResult = DialogResult.Cancel;
        _com.CancelEdition();
        return true;
    }

    private void ModelToControls()
    {
        textUser.Text = _com.Model.UserFullName;
        textTags.Text = _com.Model.Tags;
        textPriority.Text = _com.Model.Priority.ToString();
        textEstimatedTime.Text = _com.Model.EstimatedTime?.ToString();
        textSpendTime.Text = _com.Model.SpentTime?.ToString();
        textExStartDate.Text = _com.Model.ExpectedStartDate?.ToString();
        textExEndDate.Text = _com.Model.ExpectedEndDate?.ToString();
        textDificultyLevel.Text = _com.Model.DifficultyLevel?.ToString();
        textStartDate.Text = _com.Model.StartDate?.ToString();
        textEndDate.Text = _com.Model.EndDate?.ToString();
        checkResolved.Checked = _com.Model.Resolved;
        textDescription.Text = _com.Model.Description;
    }

    private void ControlsToModel()
    {
        _com.Model.UserFullName = textUser.Text;
        _com.Model.Tags = textTags.Text;
        _com.Model.Priority = _com.TextToInt(textPriority.Text);

        _com.Model.EstimatedTime = _com.TextToDouble(textEstimatedTime.Text);
        _com.Model.SpentTime = _com.TextToDouble(textSpendTime.Text);
        _com.Model.DifficultyLevel = _com.TextToDouble(textDificultyLevel.Text);

        _com.Model.ExpectedStartDate = _com.TextToDateTime(textExStartDate.Text);
        _com.Model.ExpectedEndDate = _com.TextToDateTime(textExEndDate.Text);
        _com.Model.StartDate = _com.TextToDateTime(textStartDate.Text);
        _com.Model.EndDate = _com.TextToDateTime(textEndDate.Text);

        _com.Model.Resolved = checkResolved.Checked;
        _com.Model.Description = textDescription.Text;
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

using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.ClientWin.Utils;

namespace KNote.ClientWin.Views;

public partial class TaskEditorForm : KntEditorForm, IViewEditor<NoteTaskDto>
{
    #region Private fields

    private readonly TaskEditorCtrl _ctrl;

    #endregion

    #region Constructor

    public TaskEditorForm(TaskEditorCtrl ctrl)
    {
        InitializeComponent();

        _ctrl = ctrl;
    }

    #endregion

    #region Form event handelrs

    private async void buttonAccept_Click(object sender, EventArgs e)
    {
        await AcceptEditionAsync();
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
        TryCancelEdition();
    }

    protected override Task<bool> SaveModelAsync()
        => _ctrl.SaveModel();

    protected override void CancelEdition()
        => _ctrl.CancelEdition();

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

    private void TaskEditorForm_Shown(object sender, EventArgs e)
    {
        textTags.Focus();
        //textDescription.Focus();
        //textDescription.SelectionStart = textDescription.Text.Length;
    }
    #endregion

    #region Private methods

    protected override void ModelToControls()
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

    protected override void ControlsToModel()
    {
        _ctrl.Model.UserFullName = textUser.Text;
        _ctrl.Model.Tags = textTags.Text;
        _ctrl.Model.Priority = _ctrl.Store.KntTextUtils.TextToInt(textPriority.Text);

        _ctrl.Model.EstimatedTime = _ctrl.Store.KntTextUtils.TextToDouble(textEstimatedTime.Text);
        _ctrl.Model.SpentTime = _ctrl.Store.KntTextUtils.TextToDouble(textSpendTime.Text);
        _ctrl.Model.DifficultyLevel = _ctrl.Store.KntTextUtils.TextToDouble(textDificultyLevel.Text);

        _ctrl.Model.ExpectedStartDate = _ctrl.Store.KntTextUtils.TextToDateTime(textExStartDate.Text);
        _ctrl.Model.ExpectedEndDate = _ctrl.Store.KntTextUtils.TextToDateTime(textExEndDate.Text);
        _ctrl.Model.StartDate = _ctrl.Store.KntTextUtils.TextToDateTime(textStartDate.Text);
        _ctrl.Model.EndDate = _ctrl.Store.KntTextUtils.TextToDateTime(textEndDate.Text);

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

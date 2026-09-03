using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

public partial class TraceNoteEditorForm : Form, IViewEditor<TraceNoteDto>
{
    #region Private fields

    // Sentinel item for "no relation type" (TraceNoteDto.TraceNoteTypeId is nullable): a real
    // TraceNoteTypeDto so comboTraceNoteType.DisplayMember="Name" works uniformly for every item,
    // same idiom as NotesFilterParamForm's NoNoteTypeItem.
    private static readonly TraceNoteTypeDto NoTraceNoteTypeItem = new() { TraceNoteTypeId = Guid.Empty, Name = "(none)" };

    private readonly TraceNoteEditorCtrl _ctrl;
    private bool _viewFinalized = false;
    private bool _formIsDisty = false;

    #endregion

    #region Constructor

    public TraceNoteEditorForm(TraceNoteEditorCtrl ctrl)
    {
        InitializeComponent();

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
        textRelatedNote.Text = "";
        comboTraceNoteType.Items.Clear();
        textOrder.Text = "";
        textWeight.Text = "";
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

    #endregion

    #region Form event handlers

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

    private async void buttonSelectRelatedNote_Click(object sender, EventArgs e)
    {
        var notesSelector = new NotesSelectorCtrl(_ctrl.Store)
        {
            EmbededMode = false,
            EnableTextFilter = true
        };
        await notesSelector.LoadEntities(_ctrl.Service);

        var res = notesSelector.RunModal();
        if (res.Entity == EControllerResult.Executed && notesSelector.SelectedEntity != null)
        {
            await _ctrl.SetRelatedNoteAsync(notesSelector.SelectedEntity.NoteId);
            textRelatedNote.Text = _ctrl.RelatedNoteDisplay;
            _formIsDisty = true;
        }
    }

    private void TraceNoteEditorForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!_viewFinalized)
        {
            var confirmExit = OnCancelEdition();
            if (!confirmExit)
                e.Cancel = true;
        }
    }

    private void TraceNoteEditorForm_KeyPress(object sender, KeyPressEventArgs e)
    {
        _formIsDisty = true;
    }

    private void TraceNoteEditorForm_KeyUp(object sender, KeyEventArgs e)
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
        textRelatedNote.Text = _ctrl.RelatedNoteDisplay;

        comboTraceNoteType.DisplayMember = "Name";
        comboTraceNoteType.Items.Clear();
        comboTraceNoteType.Items.Add(NoTraceNoteTypeItem);
        foreach (var traceNoteType in _ctrl.TraceNoteTypeOptions)
            comboTraceNoteType.Items.Add(traceNoteType);

        var selectedType = _ctrl.Model.TraceNoteTypeId.HasValue
            ? _ctrl.TraceNoteTypeOptions.FirstOrDefault(t => t.TraceNoteTypeId == _ctrl.Model.TraceNoteTypeId.Value)
            : null;
        comboTraceNoteType.SelectedItem = selectedType ?? NoTraceNoteTypeItem;

        textOrder.Text = _ctrl.Model.Order.ToString();
        textWeight.Text = _ctrl.Model.Weight.ToString();
    }

    private void ControlsToModel()
    {
        var selectedType = comboTraceNoteType.SelectedItem as TraceNoteTypeDto;
        _ctrl.Model.TraceNoteTypeId = (selectedType == null || selectedType.TraceNoteTypeId == Guid.Empty) ? null : selectedType.TraceNoteTypeId;

        _ctrl.Model.Order = _ctrl.Store.KntTextUtils.TextToInt(textOrder.Text);
        _ctrl.Model.Weight = _ctrl.Store.KntTextUtils.TextToDouble(textWeight.Text) ?? 0;
    }

    #endregion
}

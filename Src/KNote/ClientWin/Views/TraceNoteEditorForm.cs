using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.ClientWin.Utils;

namespace KNote.ClientWin.Views;

public partial class TraceNoteEditorForm : KntEditorForm, IViewEditor<TraceNoteDto>
{
    #region Private fields

    // Sentinel item for "no relation type" (TraceNoteDto.TraceNoteTypeId is nullable): a real
    // TraceNoteTypeDto so comboTraceNoteType.DisplayMember="Name" works uniformly for every item,
    // same idiom as NotesFilterParamForm's NoNoteTypeItem.
    private static readonly TraceNoteTypeDto NoTraceNoteTypeItem = new() { TraceNoteTypeId = Guid.Empty, Name = "(none)" };

    private readonly TraceNoteEditorCtrl _ctrl;

    #endregion

    #region Constructor

    public TraceNoteEditorForm(TraceNoteEditorCtrl ctrl)
    {
        InitializeComponent();

        _ctrl = ctrl;
    }

    #endregion

    #region Form event handlers

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
            FormIsDirty = true;
        }
    }

    #endregion

    #region Private methods

    protected override void ModelToControls()
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

    protected override void ControlsToModel()
    {
        var selectedType = comboTraceNoteType.SelectedItem as TraceNoteTypeDto;
        _ctrl.Model.TraceNoteTypeId = (selectedType == null || selectedType.TraceNoteTypeId == Guid.Empty) ? null : selectedType.TraceNoteTypeId;

        _ctrl.Model.Order = _ctrl.Store.KntTextUtils.TextToInt(textOrder.Text);
        _ctrl.Model.Weight = _ctrl.Store.KntTextUtils.TextToDouble(textWeight.Text) ?? 0;
    }

    #endregion
}

using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

public partial class TraceNoteTypeEditorForm : KntEditorForm, IViewEditor<TraceNoteTypeDto>
{
    #region Private fields

    private readonly TraceNoteTypeEditorCtrl _ctrl;

    #endregion

    #region Constructor

    public TraceNoteTypeEditorForm(TraceNoteTypeEditorCtrl ctrl)
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

    #endregion

    #region Private methods

    protected override void ModelToControls()
    {
        textName.Text = _ctrl.Model.Name;
        textDescription.Text = _ctrl.Model.Description;
    }

    protected override void ControlsToModel()
    {
        _ctrl.Model.Name = textName.Text;
        _ctrl.Model.Description = textDescription.Text;
    }

    #endregion
}

using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

public partial class NoteTypeEditorForm : KntEditorForm, IViewEditor<NoteTypeDto>
{
    #region Private fields

    private readonly NoteTypeEditorCtrl _ctrl;

    #endregion

    #region Constructor

    public NoteTypeEditorForm(NoteTypeEditorCtrl ctrl)
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

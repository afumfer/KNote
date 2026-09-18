using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.ClientWin.Utils;

namespace KNote.ClientWin.Views;

public partial class KAttributeTabulatedValueEditorForm : KntEditorForm, IViewEditor<KAttributeTabulatedValueDto>
{
    #region Private fields

    private readonly KAttributeTabulatedValueEditorCtrl _ctrl;

    #endregion

    #region Constructor

    public KAttributeTabulatedValueEditorForm(KAttributeTabulatedValueEditorCtrl ctrl)
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
        textValue.Text = _ctrl.Model.Value;
        textDescription.Text = _ctrl.Model.Description;
        numericOrder.Value = _ctrl.Model.Order;
    }

    protected override void ControlsToModel()
    {
        _ctrl.Model.Value = textValue.Text;
        _ctrl.Model.Description = textDescription.Text;
        _ctrl.Model.Order = (int)numericOrder.Value;
    }

    #endregion
}

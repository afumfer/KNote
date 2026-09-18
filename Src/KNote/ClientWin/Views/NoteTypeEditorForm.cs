using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.ClientWin.Utils;

namespace KNote.ClientWin.Views;

public partial class NoteTypeEditorForm : KntForm, IViewEditor<NoteTypeDto>
{
    #region Private fields

    private readonly NoteTypeEditorCtrl _ctrl;
    private bool _formIsDisty = false;

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

    private void NoteTypeEditorForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!ViewFinalized)
        {
            var confirmExit = OnCancelEdition();
            if (!confirmExit)
                e.Cancel = true;
        }
    }

    private void NoteTypeEditorForm_KeyPress(object sender, KeyPressEventArgs e)
    {
        _formIsDisty = true;
    }

    private void NoteTypeEditorForm_KeyUp(object sender, KeyEventArgs e)
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
            if (KntMessageBox.Show("You have modified this entity, are you sure you want to exit without recording?", KntConst.AppName, MessageBoxButtons.YesNo) == DialogResult.No)
                return false;
        }

        this.DialogResult = DialogResult.Cancel;
        _ctrl.CancelEdition();
        return true;
    }

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

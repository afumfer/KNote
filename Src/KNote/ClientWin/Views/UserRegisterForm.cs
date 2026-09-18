using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

public partial class UserRegisterForm : KntForm, IViewEditor<UserRegisterDto>
{
    #region Fields

    private readonly UserRegisterCtrl _ctrl;

    #endregion

    #region Constructor

    public UserRegisterForm(UserRegisterCtrl ctrl)
    {
        InitializeComponent();

        _ctrl = ctrl;
    }

    #endregion

    #region IEditorView implementation

    public void RefreshView()
    {
        ModelToControls();
    }

    public void RefreshModel()
    {
        ControlsToModel();
    }

    #endregion

    #region Form events handler

    private async void buttonAccept_Click(object sender, EventArgs e)
    {
        var res = await _ctrl.SaveModel();
        if (res)
        {
            this.DialogResult = DialogResult.OK;
        }
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
        this.DialogResult = DialogResult.Cancel;
    }

    private void UserRegisterForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!ViewFinalized)
            _ctrl.CancelEdition();
    }

    #endregion

    #region Private methods

    private void ModelToControls()
    {
        var alias = _ctrl.ServiceRef?.RepositoryRef?.Alias ?? "this repository";
        labelInfo.Text = $"The Windows user '{_ctrl.Model.UserName}' is not registered in '{alias}'. " +
            "Please provide the following details to register it.";

        textUserName.Text = _ctrl.Model.UserName;
        textFullName.Text = _ctrl.Model.FullName;
        textEMail.Text = _ctrl.Model.EMail;
        textPassword.Text = _ctrl.Model.Password;
    }

    private void ControlsToModel()
    {
        _ctrl.Model.FullName = textFullName.Text;
        _ctrl.Model.EMail = textEMail.Text;
        _ctrl.Model.Password = textPassword.Text;
    }

    #endregion
}

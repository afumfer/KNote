using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;

namespace KNote.ClientWin.Views;

public partial class ChangePasswordForm : KntForm, IViewEditor<ChangePasswordModel>
{
    #region Fields

    private readonly ChangePasswordCtrl _ctrl;

    #endregion

    #region Constructor

    public ChangePasswordForm(ChangePasswordCtrl ctrl)
    {
        InitializeComponent();

        _ctrl = ctrl;
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

    protected override void OnUserClosing(FormClosingEventArgs e)
        => _ctrl.CancelEdition();

    #endregion

    #region Private methods

    protected override void ModelToControls()
    {
        labelInfo.Text = $"Your password changes in every linked repository: {_ctrl.RepositoriesText}.";
        labelPolicy.Text = _ctrl.PasswordPolicyText;

        textCurrentPassword.Text = _ctrl.Model.CurrentPassword;
        textNewPassword.Text = _ctrl.Model.NewPassword;
        textConfirmPassword.Text = _ctrl.Model.ConfirmPassword;
    }

    protected override void ControlsToModel()
    {
        _ctrl.Model.CurrentPassword = textCurrentPassword.Text;
        _ctrl.Model.NewPassword = textNewPassword.Text;
        _ctrl.Model.ConfirmPassword = textConfirmPassword.Text;
    }

    #endregion
}

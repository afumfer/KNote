using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

public partial class UserProfileForm : KntForm, IViewEditor<UserProfileDto>
{
    #region Fields

    private readonly UserProfileCtrl _ctrl;

    #endregion

    #region Constructor

    public UserProfileForm(UserProfileCtrl ctrl)
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

    // The current password is only asked for a new email.
    private void textEMail_TextChanged(object sender, EventArgs e)
    {
        textCurrentPassword.Enabled = _ctrl.IsEMailChanged(textEMail.Text);
        if (!textCurrentPassword.Enabled)
            textCurrentPassword.Text = "";
    }

    protected override void OnUserClosing(FormClosingEventArgs e)
        => _ctrl.CancelEdition();

    #endregion

    #region Private methods

    protected override void ModelToControls()
    {
        labelInfo.Text = $"Your account as it is in the repository '{_ctrl.RepositoryName}'. " +
            "Only an administrator can change your user name.";

        textUserName.Text = _ctrl.Model.UserName;
        textFullName.Text = _ctrl.Model.FullName;
        textEMail.Text = _ctrl.Model.EMail;
        textCurrentPassword.Text = "";
        textCurrentPassword.Enabled = false;

        // With a single repository there is nothing to choose.
        checkApplyToAll.Text = $"Save it in every linked repository ({_ctrl.RepositoriesCount})";
        checkApplyToAll.Checked = _ctrl.ApplyToAllRepositories;
        checkApplyToAll.Visible = _ctrl.RepositoriesCount > 1;
    }

    protected override void ControlsToModel()
    {
        _ctrl.Model.FullName = textFullName.Text;
        _ctrl.Model.EMail = textEMail.Text;
        _ctrl.Model.CurrentPassword = textCurrentPassword.Text;
        _ctrl.ApplyToAllRepositories = checkApplyToAll.Checked;
    }

    #endregion
}

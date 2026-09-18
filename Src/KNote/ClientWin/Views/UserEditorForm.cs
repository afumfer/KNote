using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.ClientWin.Utils;

namespace KNote.ClientWin.Views;

public partial class UserEditorForm : KntEditorForm, IViewEditor<UserDto>
{
    #region Private fields

    private readonly UserEditorCtrl _ctrl;

    #endregion

    #region Constructor

    public UserEditorForm(UserEditorCtrl ctrl)
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

    private async void buttonResetPassword_Click(object sender, EventArgs e)
    {
        // Deliberately separate from Accept/SaveModel: takes effect immediately, doesn't close the
        // dialog, and doesn't require the rest of the form to be dirty/valid.
        if (await _ctrl.ResetPassword(textPassword.Text))
            textPassword.Text = "";
    }

    #endregion

    #region Private methods

    protected override void ModelToControls()
    {
        var isNew = _ctrl.Model.UserId == Guid.Empty;

        textUserName.Text = _ctrl.Model.UserName;
        textEMail.Text = _ctrl.Model.EMail;
        textFullName.Text = _ctrl.Model.FullName;

        var roles = (_ctrl.Model.RoleDefinition ?? "").Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        checkPublic.Checked = roles.Contains(nameof(EnumRoles.Public));
        checkStaff.Checked = roles.Contains(nameof(EnumRoles.Staff));
        checkProjectManager.Checked = roles.Contains(nameof(EnumRoles.ProjecManager));
        checkAdmin.Checked = roles.Contains(nameof(EnumRoles.Admin));

        textPassword.Text = "";
        // A new user's password is required and entered through the same Accept flow as the rest of
        // the form; an existing user's password is only ever changed through the separate, immediate
        // "Reset password" action below - Save never touches it.
        labelPassword.Text = isNew ? "Password:" : "New password:";
        buttonResetPassword.Visible = !isNew;
    }

    protected override void ControlsToModel()
    {
        _ctrl.Model.UserName = textUserName.Text;
        _ctrl.Model.EMail = textEMail.Text;
        _ctrl.Model.FullName = textFullName.Text;

        var roles = new List<string>();
        if (checkPublic.Checked) roles.Add(nameof(EnumRoles.Public));
        if (checkStaff.Checked) roles.Add(nameof(EnumRoles.Staff));
        if (checkProjectManager.Checked) roles.Add(nameof(EnumRoles.ProjecManager));
        if (checkAdmin.Checked) roles.Add(nameof(EnumRoles.Admin));
        _ctrl.Model.RoleDefinition = string.Join(",", roles);

        _ctrl.NewUserPassword = textPassword.Text;
    }

    #endregion
}

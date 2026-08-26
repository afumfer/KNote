using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

public partial class UserEditorForm : Form, IViewEditor<UserDto>
{
    #region Private fields

    private readonly UserEditorCtrl _ctrl;
    private bool _viewFinalized = false;
    private bool _formIsDisty = false;

    #endregion

    #region Constructor

    public UserEditorForm(UserEditorCtrl ctrl)
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
        textUserName.Text = "";
        textEMail.Text = "";
        textFullName.Text = "";
        textPassword.Text = "";
        checkPublic.Checked = false;
        checkStaff.Checked = false;
        checkProjectManager.Checked = false;
        checkAdmin.Checked = false;
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

    private async void buttonResetPassword_Click(object sender, EventArgs e)
    {
        // Deliberately separate from Accept/SaveModel: takes effect immediately, doesn't close the
        // dialog, and doesn't require the rest of the form to be dirty/valid.
        if (await _ctrl.ResetPassword(textPassword.Text))
            textPassword.Text = "";
    }

    private void UserEditorForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!_viewFinalized)
        {
            var confirmExit = OnCancelEdition();
            if (!confirmExit)
                e.Cancel = true;
        }
    }

    private void UserEditorForm_KeyPress(object sender, KeyPressEventArgs e)
    {
        _formIsDisty = true;
    }

    private void UserEditorForm_KeyUp(object sender, KeyEventArgs e)
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

    private void ControlsToModel()
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

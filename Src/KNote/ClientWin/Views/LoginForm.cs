using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KntIcons;

namespace KNote.ClientWin.Views;

public partial class LoginForm : KntForm, IViewEditor<LoginModel>
{
    #region Fields

    private readonly LoginCtrl _ctrl;

    #endregion

    #region Constructor

    public LoginForm(LoginCtrl ctrl)
    {
        InitializeComponent();
        this.Text = $"{KntConst.AppName} - Sign in";
        // A dark band like the management window's header, with a light icon over it.
        pictureBoxSignIn.SetKntIcon(KntIcon.SignIn, 56, Color.WhiteSmoke);

        _ctrl = ctrl;
    }

    #endregion

    #region Form events handler

    private async void buttonAccept_Click(object sender, EventArgs e)
    {
        var res = await _ctrl.SaveModel();
        if (res)
            this.DialogResult = DialogResult.OK;
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
        this.DialogResult = DialogResult.Cancel;
    }

    private void linkWindowsAccount_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        _ctrl.UseWindowsAccount();
        this.DialogResult = DialogResult.OK;
    }

    protected override void OnUserClosing(FormClosingEventArgs e)
        => _ctrl.CancelEdition();

    #endregion

    #region Private methods

    protected override void ModelToControls()
    {
        textUserName.Text = _ctrl.Model.UserName;
        textPassword.Text = _ctrl.Model.Password;
        linkWindowsAccount.Text = $"Use my Windows account ({_ctrl.WindowsUserName}) instead";

        // The user name is usually remembered from the last time: start on the password then.
        ActiveControl = string.IsNullOrEmpty(textUserName.Text) ? textUserName : textPassword;
    }

    protected override void ControlsToModel()
    {
        _ctrl.Model.UserName = textUserName.Text;
        _ctrl.Model.Password = textPassword.Text;
    }

    #endregion
}

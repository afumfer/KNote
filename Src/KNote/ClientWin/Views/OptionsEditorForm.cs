using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Service.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

public partial class OptionsEditorForm : KntEditorForm, IViewEditor<OptionsModel>
{
    #region Privage Fields

    private readonly OptionsEditorCtrl _ctrl;

    // The chat hub url the tokens in the model were given for: they belong to that Server, so changing the url
    // forgets them (see ControlsToModel).
    private string _chatSignedInUrl;

    // Items of comboColorMode, in the same order.
    private static readonly (AppColorMode Mode, string Text)[] ColorModes =
    {
        (AppColorMode.Light, "Light"),
        (AppColorMode.Dark, "Dark"),
        (AppColorMode.System, "Use the Windows setting")
    };

    // Items of comboAuthenticationMode, in the same order.
    private static readonly (AppAuthenticationMode Mode, string Text)[] AuthenticationModes =
    {
        (AppAuthenticationMode.Windows, "With my Windows account"),
        (AppAuthenticationMode.Credentials, "With a KNote user name and password")
    };

    #endregion

    #region Constructor 

    public OptionsEditorForm(OptionsEditorCtrl ctrl)
    {
        InitializeComponent();
        this.Text = $"{KntConst.AppName} options";
        comboColorMode.Items.AddRange(ColorModes.Select(c => (object)c.Text).ToArray());
        comboAuthenticationMode.Items.AddRange(AuthenticationModes.Select(a => (object)a.Text).ToArray());

        _ctrl = ctrl;
    }

    #endregion 

    #region Form events handler

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

    // Signs in to the chat room's Server (its api/users/login, next to the hub) in every repository where the user is
    // (each one has its chat room, chosen in the chat room's window) and checks the hub takes the token. Only the user
    // name and the repositories with their tokens are kept (encrypted on disk, see ChatHubConfig): the password is
    // cleared. Kept when the dialog is accepted, as every other option.
    private async void buttonChatSignIn_Click(object sender, EventArgs e)
    {
        var url = textChatHubUrl.Text?.Trim();
        var userName = textChatUserName.Text?.Trim();

        buttonChatSignIn.Enabled = false;
        this.Cursor = Cursors.WaitCursor;
        try
        {
            var signIn = await ChatRoomServer.SignInAsync(url, userName, textChatPassword.Text);
            if (!signIn.IsValid)
            {
                ShowInfo($"Could not sign in to the chat room server. {signIn.ErrorMessage}");
                return;
            }

            var test = await ChatRoomServer.TestConnectionAsync(url, signIn.Entity[0].Token);
            if (!test.IsValid)
            {
                ShowInfo($"Signed in, but the chat room could not be reached. {test.ErrorMessage}");
                return;
            }

            textChatHubUrl.Text = url;
            _ctrl.Model.ChatHubUserName = userName;
            _ctrl.Model.ChatHubRepositories = signIn.Entity;
            _ctrl.Model.ChatHubAutoConnectDisabled = false;
            _chatSignedInUrl = url;
            textChatPassword.Text = "";
            RefreshChatStatus();
            ShowInfo($"Signed in to the chat room as {userName}{InRepositories()}. Accept the options to keep it.");
        }
        finally
        {
            this.Cursor = Cursors.Default;
            buttonChatSignIn.Enabled = true;
        }
    }

    private void buttonChatSignOut_Click(object sender, EventArgs e)
    {
        _ctrl.Model.ChatHubRepositories = new();
        _chatSignedInUrl = null;
        RefreshChatStatus();
    }

    private void RefreshChatStatus()
    {
        var signedIn = _ctrl.Model.ChatHubSignedIn;
        labelChatStatus.Text = signedIn
            ? $"Signed in as {_ctrl.Model.ChatHubUserName}{InRepositories()}" +
                (_ctrl.Model.ChatHubRepositories.Count > 1 ? " (choose the chat room in its window)" : "") +
                ". Only the sign-in tokens are kept (encrypted), not the password. Sign in again if the server refuses them (they expire after a year)."
            : "Not signed in. Enter the user name and the password of your user in the KNote Server (Staff or higher) and sign in. The password is not kept.";
        buttonChatSignOut.Enabled = signedIn;
    }

    // " in the repository A" or " in the repositories A, B" (nothing for a Server that doesn't name them).
    private string InRepositories()
    {
        var names = _ctrl.Model.ChatHubRepositories.Select(r => r.Name).Where(n => !string.IsNullOrEmpty(n)).ToList();
        return names.Count switch
        {
            0 => "",
            1 => $" in the repository {names[0]}",
            _ => $" in the repositories {string.Join(", ", names)}"
        };
    }

    private async void buttonTestSmtp_Click(object sender, EventArgs e)
    {
        var to = textTestEmailTo.Text?.Trim();
        if (string.IsNullOrEmpty(to))
        {
            ShowInfo("Enter a recipient address for the test email first.");
            return;
        }

        if (string.IsNullOrEmpty(textSmtpHost.Text) || string.IsNullOrEmpty(textSmtpFromAddress.Text))
        {
            ShowInfo("Enter at least the SMTP host and the from address first.");
            return;
        }

        var settings = new SmtpSettings
        {
            Host = textSmtpHost.Text,
            Port = string.IsNullOrWhiteSpace(textSmtpPort.Text) ? 587 : _ctrl.Store.KntTextUtils.TextToInt(textSmtpPort.Text),
            EnableSsl = checkSmtpEnableSsl.Checked,
            FromAddress = textSmtpFromAddress.Text,
            FromDisplayName = textSmtpFromDisplayName.Text,
            Username = string.IsNullOrEmpty(textSmtpUsername.Text) ? textSmtpFromAddress.Text : textSmtpUsername.Text,
            Password = textSmtpPassword.Text
        };

        buttonTestSmtp.Enabled = false;
        this.Cursor = Cursors.WaitCursor;
        try
        {
            await Task.Run(() => new SmtpEmailSender().Send(settings, to, $"{KntConst.AppName} test email",
                "This is a test email sent from the KNote options dialog."));
            ShowInfo("Test email sent successfully.");
        }
        catch (Exception ex)
        {
            ShowInfo($"The test email could not be sent. Error: {ex.Message}");
        }
        finally
        {
            this.Cursor = Cursors.Default;
            buttonTestSmtp.Enabled = true;
        }
    }

    #endregion

    #region Private methods

    protected override void ModelToControls() 
    {
        checkAlarmActivated.Checked = _ctrl.Model.AlarmActivated;
        textAlarmSeconds.Text = _ctrl.Model.AlarmSeconds.ToString();
        checkAutoSaveActivated.Checked = _ctrl.Model.AutoSaveActivated;
        textAutosaveSeconds.Text = _ctrl.Model.AutoSaveSeconds.ToString();
        textChatHubUrl.Text = _ctrl.Model.ChatHubUrl;
        textChatUserName.Text = _ctrl.Model.ChatHubUserName;
        textChatPassword.Text = "";
        _chatSignedInUrl = _ctrl.Model.ChatHubSignedIn ? _ctrl.Model.ChatHubUrl : null;
        RefreshChatStatus();
        comboColorMode.SelectedIndex = Math.Max(0, Array.FindIndex(ColorModes, c => c.Mode == _ctrl.Model.ColorMode));
        comboAuthenticationMode.SelectedIndex = Math.Max(0, Array.FindIndex(AuthenticationModes, a => a.Mode == _ctrl.Model.AuthenticationMode));
        textSmtpHost.Text = _ctrl.Model.SmtpHost;
        textSmtpPort.Text = _ctrl.Model.SmtpPort.ToString();
        checkSmtpEnableSsl.Checked = _ctrl.Model.SmtpEnableSsl;
        textSmtpFromAddress.Text = _ctrl.Model.SmtpFromAddress;
        textSmtpFromDisplayName.Text = _ctrl.Model.SmtpFromDisplayName;
        textSmtpUsername.Text = _ctrl.Model.SmtpUsername;
        textSmtpPassword.Text = _ctrl.Model.SmtpPassword;
        checkProcessEmailAlarms.Checked = _ctrl.Model.ProcessEmailAlarms;

        //var x5 = _ctrl.Model.LogActivated;
        //var x6 = _ctrl.Model.LogFile;
    }

    protected override void ControlsToModel()
    {
        _ctrl.Model.AlarmActivated = checkAlarmActivated.Checked;
        _ctrl.Model.AlarmSeconds = int.Parse(textAlarmSeconds.Text);
        _ctrl.Model.AutoSaveActivated = checkAutoSaveActivated.Checked;
        _ctrl.Model.AutoSaveSeconds = int.Parse(textAutosaveSeconds.Text);
        _ctrl.Model.ChatHubUrl = textChatHubUrl.Text?.Trim();
        // The tokens belong to the Server they were given by: another url needs signing in again.
        if (_ctrl.Model.ChatHubUrl != _chatSignedInUrl)
            _ctrl.Model.ChatHubRepositories = new();
        _ctrl.Model.ColorMode = ColorModes[comboColorMode.SelectedIndex].Mode;
        _ctrl.Model.AuthenticationMode = AuthenticationModes[comboAuthenticationMode.SelectedIndex].Mode;
        _ctrl.Model.SmtpHost = textSmtpHost.Text;
        _ctrl.Model.SmtpPort = _ctrl.Store.KntTextUtils.TextToInt(textSmtpPort.Text);
        _ctrl.Model.SmtpEnableSsl = checkSmtpEnableSsl.Checked;
        _ctrl.Model.SmtpFromAddress = textSmtpFromAddress.Text;
        _ctrl.Model.SmtpFromDisplayName = textSmtpFromDisplayName.Text;
        _ctrl.Model.SmtpUsername = textSmtpUsername.Text;
        _ctrl.Model.SmtpPassword = textSmtpPassword.Text;
        _ctrl.Model.ProcessEmailAlarms = checkProcessEmailAlarms.Checked;
    }

    #endregion
}


using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Views;

public partial class OptionsEditorForm : KntEditorForm, IViewEditor<OptionsModel>
{
    #region Privage Fields

    private readonly OptionsEditorCtrl _ctrl;

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

    private async void buttonTestChatHubUrl_Click(object sender, EventArgs e)
    {
        var url = textChatHubUrl.Text?.Trim();
        if (string.IsNullOrEmpty(url))
        {
            ShowInfo("Enter a chat hub url first.");
            return;
        }

        buttonTestChatHubUrl.Enabled = false;
        this.Cursor = Cursors.WaitCursor;
        try
        {
            var testResult = await KntChatCtrl.TestConnectionAsync(url);
            if (testResult.IsValid)
            {
                _ctrl.Model.ChatHubAutoConnectDisabled = false;
                ShowInfo("Connection successful.");
            }
            else
            {
                ShowInfo($"The connection could not be established. Error: {testResult.ErrorMessage}");
            }
        }
        finally
        {
            this.Cursor = Cursors.Default;
            buttonTestChatHubUrl.Enabled = true;
        }
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
        comboColorMode.SelectedIndex = Math.Max(0, Array.FindIndex(ColorModes, c => c.Mode == _ctrl.Model.ColorMode));
        comboAuthenticationMode.SelectedIndex = Math.Max(0, Array.FindIndex(AuthenticationModes, a => a.Mode == _ctrl.Model.AuthenticationMode));
        textSmtpHost.Text = _ctrl.Model.SmtpHost;
        textSmtpPort.Text = _ctrl.Model.SmtpPort.ToString();
        checkSmtpEnableSsl.Checked = _ctrl.Model.SmtpEnableSsl;
        textSmtpFromAddress.Text = _ctrl.Model.SmtpFromAddress;
        textSmtpFromDisplayName.Text = _ctrl.Model.SmtpFromDisplayName;
        textSmtpUsername.Text = _ctrl.Model.SmtpUsername;
        textSmtpPassword.Text = _ctrl.Model.SmtpPassword;

        //var x5 = _ctrl.Model.LogActivated;
        //var x6 = _ctrl.Model.LogFile;
    }

    protected override void ControlsToModel()
    {
        _ctrl.Model.AlarmActivated = checkAlarmActivated.Checked;
        _ctrl.Model.AlarmSeconds = int.Parse(textAlarmSeconds.Text);
        _ctrl.Model.AutoSaveActivated = checkAutoSaveActivated.Checked;
        _ctrl.Model.AutoSaveSeconds = int.Parse(textAutosaveSeconds.Text);
        _ctrl.Model.ChatHubUrl = textChatHubUrl.Text;
        _ctrl.Model.ColorMode = ColorModes[comboColorMode.SelectedIndex].Mode;
        _ctrl.Model.AuthenticationMode = AuthenticationModes[comboAuthenticationMode.SelectedIndex].Mode;
        _ctrl.Model.SmtpHost = textSmtpHost.Text;
        _ctrl.Model.SmtpPort = _ctrl.Store.KntTextUtils.TextToInt(textSmtpPort.Text);
        _ctrl.Model.SmtpEnableSsl = checkSmtpEnableSsl.Checked;
        _ctrl.Model.SmtpFromAddress = textSmtpFromAddress.Text;
        _ctrl.Model.SmtpFromDisplayName = textSmtpFromDisplayName.Text;
        _ctrl.Model.SmtpUsername = textSmtpUsername.Text;
        _ctrl.Model.SmtpPassword = textSmtpPassword.Text;
    }

    #endregion
}


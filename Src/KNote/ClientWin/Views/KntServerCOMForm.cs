using KNote.ClientWin.Core;
using KNote.ClientWin.Controllers;
using KNote.ClientWin.Utils;
using KNote.Model;
using System.IO.Ports;

namespace KNote.ClientWin.Views;

public partial class KntServerCOMForm : KntForm, IViewServerCOM
{
    #region Private members

    private readonly KntServerCOMCtrl _ctrl;

    #endregion

    #region Constructor

    public KntServerCOMForm(KntServerCOMCtrl ctrl)
    {
        InitializeComponent();

        _ctrl = ctrl;

        InitializeSettingsControls();
        ShowSettings();
        PopulateAiProviders();
        SetButtonIcons();

        _ctrl.ReceiveMessage += _com_ReceiveMessage;
    }

    #endregion

    #region IViewChat implementation

    public override void OnClosingView()
    {
        _ctrl.ReceiveMessage -= _com_ReceiveMessage;
        base.OnClosingView();
    }

    public override void RefreshView()
    {
        Refresh();
        RefreshStatus();
    }

    public void VisibleView(bool visible)
    {
        if (visible)
            Show();
        else
            Hide();
    }

    public void RefreshStatus()
    {
        // Can be called from the service threads before the handle exists or after the form is gone.
        if (!IsHandleCreated || IsDisposed)
            return;

        statusInfo.Invoke((MethodInvoker)delegate
        {
            // Running on the UI thread
            statusLabelInfo.Text = $"Runing service: {_ctrl.RunningService} | Message sending: {_ctrl.MessageSending}";

            // Start is only meaningful while stopped and Stop while running; the settings are locked
            // while the port is open.
            buttonStart.Enabled = !_ctrl.RunningService;
            buttonStop.Enabled = _ctrl.RunningService;
            panelSettings.Enabled = !_ctrl.RunningService;
        });

    }

    #endregion

    #region Form and component event handler

    private void KntServerCOMForm_Load(object sender, EventArgs e)
    {
        RefreshStatus();
    }

    private void buttonStart_Click(object sender, EventArgs e)
    {
        // What is shown in the Settings tab is what runs, whether or not it has been saved.
        if (!ApplySettingsToCtrl())
            return;

        if (!_ctrl.StartService())
            KntMessageBox.Show(_ctrl.Error);
    }

    private void buttonStop_Click(object sender, EventArgs e)
    {
        _ctrl.StopService();
    }

    private void buttonSend_Click(object sender, EventArgs e)
    {
        if (!_ctrl.RunningService)
        {
            KntMessageBox.Show("The service is not running. Press Start button.");
            return;
        }

        if (_ctrl.MessageSending)
        {
            KntMessageBox.Show("Sending message now ... try later");
            return;
        }

        _ctrl.Send(textBoxSend.Text);
    }

    private void buttonRefreshPorts_Click(object sender, EventArgs e)
    {
        PopulatePortNames();
    }

    private void buttonRestoreDefaults_Click(object sender, EventArgs e)
    {
        _ctrl.ResetSerialDefaults();
        ShowFrameSettings();
    }

    private void buttonSaveSettings_Click(object sender, EventArgs e)
    {
        if (!ApplySettingsToCtrl())
            return;

        if (_ctrl.SaveSettings())
            labelSaveResult.Text = "Settings saved.";
        else
            KntMessageBox.Show(_ctrl.Error);
    }

    private void SettingsControl_Changed(object sender, EventArgs e)
    {
        labelSaveResult.Text = "";
    }

    private void comboAiProviders_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (comboAiProviders.SelectedItem is not AiProviderRef providerRef || providerRef == _ctrl.CurrentAiProviderRef)
            return;

        if (!_ctrl.SetAiProvider(providerRef))
        {
            KntMessageBox.Show(_ctrl.Error);
            SelectCurrentAiProvider();
        }
    }

    private async void buttonManageProviders_Click(object sender, EventArgs e)
    {
        var manageCtrl = new AiProvidersManageCtrl(_ctrl.Store);
        await manageCtrl.LoadEntitiesAsync(null, false);
        manageCtrl.RunModal();

        // Providers may have been added/edited/removed: refresh the picker in place.
        PopulateAiProviders();
    }

    protected override void OnUserClosing(FormClosingEventArgs e)
    {
        if (_ctrl.AutoCloseCtrlOnViewExit)
            _ctrl.Finalize();
        else
        {
            Hide();
            e.Cancel = true;
        }
    }

    private void _com_ReceiveMessage(object sender, ControllerEventArgs<string> e)
    {
        if (!IsHandleCreated || IsDisposed)
            return;

        listBoxEcho.Invoke((MethodInvoker)delegate
        {
            // Running on the UI thread                        
            listBoxEcho.Items.Add("Recived: " + e.Entity.ToString());
        });
    }

    #endregion

    #region AI provider selector

    private void PopulateAiProviders()
    {
        // Detach first: setting DataSource auto-selects an item and would otherwise fire
        // comboAiProviders_SelectedIndexChanged (which resets the conversation) during startup.
        comboAiProviders.SelectedIndexChanged -= comboAiProviders_SelectedIndexChanged;

        comboAiProviders.DataSource = null;
        comboAiProviders.DisplayMember = nameof(AiProviderRef.Alias);
        comboAiProviders.DataSource = _ctrl.AiProviderRefs;
        ShowCurrentAiProvider();

        comboAiProviders.SelectedIndexChanged += comboAiProviders_SelectedIndexChanged;
    }

    // Puts the combo back on the provider in use when a change was refused.
    private void SelectCurrentAiProvider()
    {
        comboAiProviders.SelectedIndexChanged -= comboAiProviders_SelectedIndexChanged;
        ShowCurrentAiProvider();
        comboAiProviders.SelectedIndexChanged += comboAiProviders_SelectedIndexChanged;
    }

    private void ShowCurrentAiProvider()
    {
        comboAiProviders.Enabled = _ctrl.AiProviderRefs.Count > 0;
        comboAiProviders.SelectedItem = _ctrl.CurrentAiProviderRef;
    }

    #endregion

    #region Settings tab

    private static readonly string[] BaudRates = { "1200", "2400", "4800", "9600", "19200", "38400", "57600", "115200", "230400" };

    // The item index of these combos is the int value stored in the settings (the System.IO.Ports enum
    // value), except StopBits, whose first item ("One") is 1.
    private static readonly string[] HandshakeNames = Enum.GetNames<Handshake>();
    private static readonly string[] ParityNames = Enum.GetNames<Parity>();
    private const int FirstStopBitsValue = (int)StopBits.One;

    private void InitializeSettingsControls()
    {
        comboBaudRate.Items.AddRange(BaudRates);
        comboHandshake.Items.AddRange(HandshakeNames);
        comboParity.Items.AddRange(ParityNames);
        comboStopBits.Items.AddRange(new object[] { nameof(StopBits.One), nameof(StopBits.Two), nameof(StopBits.OnePointFive) });

        comboPortName.TextChanged += SettingsControl_Changed;
        comboBaudRate.TextChanged += SettingsControl_Changed;
        comboHandshake.SelectedIndexChanged += SettingsControl_Changed;
        numericRetroDelay.ValueChanged += SettingsControl_Changed;
        comboParity.SelectedIndexChanged += SettingsControl_Changed;
        numericDataBits.ValueChanged += SettingsControl_Changed;
        comboStopBits.SelectedIndexChanged += SettingsControl_Changed;
    }

    // Shows the current settings of the controller (loaded from KNoteData.config, plus any override
    // applied to it before the view was created, e.g. from KntScript).
    private void ShowSettings()
    {
        PopulatePortNames();
        comboPortName.Text = _ctrl.PortName;
        comboBaudRate.Text = _ctrl.BaudRate.ToString();
        comboHandshake.SelectedIndex = Math.Clamp(_ctrl.HandShake, 0, comboHandshake.Items.Count - 1);
        numericRetroDelay.Value = Math.Clamp(_ctrl.RetroDelay, (int)numericRetroDelay.Minimum, (int)numericRetroDelay.Maximum);
        ShowFrameSettings();
        labelSaveResult.Text = "";
    }

    private void ShowFrameSettings()
    {
        comboParity.SelectedIndex = Math.Clamp(_ctrl.Parity, 0, comboParity.Items.Count - 1);
        numericDataBits.Value = Math.Clamp(_ctrl.DataBits, (int)numericDataBits.Minimum, (int)numericDataBits.Maximum);
        comboStopBits.SelectedIndex = Math.Clamp(_ctrl.StopBits - FirstStopBitsValue, 0, comboStopBits.Items.Count - 1);
    }

    private void PopulatePortNames()
    {
        var current = comboPortName.Text;

        var ports = SerialPort.GetPortNames().OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToArray();
        comboPortName.Items.Clear();
        comboPortName.Items.AddRange(ports);
        comboPortName.Text = current;

        labelSettingsInfo.Text = "The settings can only be changed while the service is stopped." +
            (ports.Length == 0 ? " No COM ports were detected on this computer." : "");
    }

    // Copies the values of the Settings tab to the controller. Returns false (after telling the user)
    // if the baud rate is not a number.
    private bool ApplySettingsToCtrl()
    {
        if (!int.TryParse(comboBaudRate.Text, out var baudRate))
        {
            KntMessageBox.Show("The baud rate must be a number.");
            tabControlMain.SelectedTab = tabPageSettings;
            comboBaudRate.Focus();
            return false;
        }

        _ctrl.PortName = comboPortName.Text.Trim();
        _ctrl.BaudRate = baudRate;
        _ctrl.HandShake = comboHandshake.SelectedIndex;
        _ctrl.RetroDelay = (int)numericRetroDelay.Value;
        _ctrl.Parity = comboParity.SelectedIndex;
        _ctrl.DataBits = (int)numericDataBits.Value;
        _ctrl.StopBits = comboStopBits.SelectedIndex + FirstStopBitsValue;
        return true;
    }

    #endregion

    #region Button icons

    // Same technique as AppInfoAlarmsForm/NotesSelectorForm: PNGs embedded as resources (see
    // KNote.ClientWin.csproj) instead of going through the Designer's .resx. A missing icon just
    // leaves the button text-only.
    private void SetButtonIcons()
    {
        SetButtonIcon(buttonStart, "play_16.png");
        SetButtonIcon(buttonStop, "stop_16.png");
    }

    private static void SetButtonIcon(Button button, string resourceName)
    {
        try
        {
            using var stream = System.Reflection.Assembly.GetExecutingAssembly()
                .GetManifestResourceStream($"KNote.ClientWin.Resources.Icons.{resourceName}");
            if (stream == null)
                return;

            // A Bitmap needs its stream for its whole life: copy it into a Bitmap that owns its data.
            using var loaded = new Bitmap(stream);
            button.Image = new Bitmap(loaded);
            button.ImageAlign = ContentAlignment.MiddleLeft;
            button.TextImageRelation = TextImageRelation.ImageBeforeText;
        }
        catch (Exception)
        {
            // Keep the text-only button.
        }
    }

    #endregion
}

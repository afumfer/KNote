using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Views;

public partial class AiProviderEditorForm : Form, IViewEditor<AiProviderRef>
{
    #region Private fields

    private readonly AiProviderEditorCtrl _ctrl;
    private bool _viewFinalized = false;
    private bool _formIsDisty = false;

    #endregion

    #region Constructor

    public AiProviderEditorForm(AiProviderEditorCtrl ctrl)
    {
        InitializeComponent();

        _ctrl = ctrl;

        // Assigning DataSource auto-selects the first item, which fires SelectedIndexChanged;
        // that must not count as a user edit (see comboProvider_SelectionChangeCommitted below).
        comboProvider.DataSource = EnumAiProvider.All;
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
        textAlias.Text = "";
        comboProvider.SelectedIndex = -1;
        textModelName.Text = "";
        textApiKey.Text = "";
        textHost.Text = "";
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

    private void AiProviderEditorForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!_viewFinalized)
        {
            var confirmExit = OnCancelEdition();
            if (!confirmExit)
                e.Cancel = true;
        }
    }

    private void AiProviderEditorForm_KeyPress(object sender, KeyPressEventArgs e)
    {
        _formIsDisty = true;
    }

    private void AiProviderEditorForm_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            _formIsDisty = true;
    }

    // Fires on both user selection and programmatic assignment (DataSource binding,
    // ModelToControls): only updates the Host field's enabled state, never the dirty flag.
    private void comboProvider_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateHostEnabled();
    }

    // Fires only on an actual user-driven selection (mouse/keyboard), unlike
    // SelectedIndexChanged - this is what should mark the form as modified.
    private void comboProvider_SelectionChangeCommitted(object sender, EventArgs e)
    {
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

    private void UpdateHostEnabled()
    {
        var isOllama = (string)comboProvider.SelectedItem == EnumAiProvider.Ollama;
        labelHost.Enabled = isOllama;
        textHost.Enabled = isOllama;
    }

    private void ModelToControls()
    {
        textAlias.Text = _ctrl.Model.Alias;
        comboProvider.SelectedItem = _ctrl.Model.Provider;
        textModelName.Text = _ctrl.Model.Model;
        textApiKey.Text = _ctrl.Model.ApiKey;
        textHost.Text = _ctrl.Model.Host;
        UpdateHostEnabled();
    }

    private void ControlsToModel()
    {
        _ctrl.Model.Alias = textAlias.Text;
        _ctrl.Model.Provider = comboProvider.SelectedItem as string;
        _ctrl.Model.Model = textModelName.Text;
        _ctrl.Model.ApiKey = textApiKey.Text;
        _ctrl.Model.Host = textHost.Text;
    }

    #endregion
}

using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.ClientWin.Utils;

namespace KNote.ClientWin.Views;

public partial class AiProviderEditorForm : KntEditorForm, IViewEditor<AiProviderRef>
{
    #region Private fields

    private readonly AiProviderEditorCtrl _ctrl;

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
        FormIsDirty = true;
    }

    #endregion

    #region Private methods

    private void UpdateHostEnabled()
    {
        var isOllama = (string)comboProvider.SelectedItem == EnumAiProvider.Ollama;
        labelHost.Enabled = isOllama;
        textHost.Enabled = isOllama;
    }

    protected override void ModelToControls()
    {
        textAlias.Text = _ctrl.Model.Alias;
        comboProvider.SelectedItem = _ctrl.Model.Provider;
        textModelName.Text = _ctrl.Model.Model;
        textApiKey.Text = _ctrl.Model.ApiKey;
        textHost.Text = _ctrl.Model.Host;
        UpdateHostEnabled();
    }

    protected override void ControlsToModel()
    {
        _ctrl.Model.Alias = textAlias.Text;
        _ctrl.Model.Provider = comboProvider.SelectedItem as string;
        _ctrl.Model.Model = textModelName.Text;
        _ctrl.Model.ApiKey = textApiKey.Text;
        _ctrl.Model.Host = textHost.Text;
    }

    #endregion
}

using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;

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
        comboKind.DataSource = EnumAiProviderKind.All;
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

    private void comboKind_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateHostEnabled();
    }

    // Anthropic has no embedding models: choosing Embeddings with it moves to Ollama.
    private void comboKind_SelectionChangeCommitted(object sender, EventArgs e)
    {
        FormIsDirty = true;
        if ((string)comboKind.SelectedItem == EnumAiProviderKind.Embeddings
            && Array.IndexOf(EnumAiProvider.WithEmbeddings, comboProvider.SelectedItem as string) < 0)
            comboProvider.SelectedItem = EnumAiProvider.Ollama;
    }

    // Tests the provider as it is in the form (saved or not): what the model answered, or why it failed.
    private async void buttonTest_Click(object sender, EventArgs e)
    {
        var candidate = new AiProviderRef();
        ControlsToProvider(candidate);

        buttonTest.Enabled = false;
        var cursor = Cursor;
        Cursor = Cursors.WaitCursor;
        try
        {
            var result = await _ctrl.TestAsync(candidate);
            ShowInfo(result.IsValid ? result.Entity : result.ErrorMessage, "Test AI provider",
                MessageBoxButtons.OK, result.IsValid ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }
        finally
        {
            Cursor = cursor;
            buttonTest.Enabled = true;
        }
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

        // Only an Ollama chat model has its own context size.
        var hasContextWindow = isOllama && (string)comboKind.SelectedItem != EnumAiProviderKind.Embeddings;
        labelContextWindow.Enabled = hasContextWindow;
        textContextWindow.Enabled = hasContextWindow;

        // The same for the key of ollama.com's web search: the paid providers search with their own key.
        labelWebSearchApiKey.Enabled = hasContextWindow;
        textWebSearchApiKey.Enabled = hasContextWindow;
    }

    protected override void ModelToControls()
    {
        textAlias.Text = _ctrl.Model.Alias;
        comboKind.SelectedItem = _ctrl.Model.Kind ?? EnumAiProviderKind.Chat;
        comboProvider.SelectedItem = _ctrl.Model.Provider;
        textModelName.Text = _ctrl.Model.Model;
        textApiKey.Text = _ctrl.Model.ApiKey;
        textHost.Text = _ctrl.Model.Host;
        textContextWindow.Text = _ctrl.Model.ContextWindow?.ToString();
        textWebSearchApiKey.Text = _ctrl.Model.WebSearchApiKey;
        UpdateHostEnabled();
    }

    protected override void ControlsToModel()
    {
        ControlsToProvider(_ctrl.Model);
    }

    private void ControlsToProvider(AiProviderRef provider)
    {
        provider.Alias = textAlias.Text;
        provider.Kind = comboKind.SelectedItem as string ?? EnumAiProviderKind.Chat;
        provider.Provider = comboProvider.SelectedItem as string;
        provider.Model = textModelName.Text;
        provider.ApiKey = textApiKey.Text;
        provider.Host = textHost.Text;
        // Only an Ollama chat model keeps it; a text that is not a number is left for the validation to refuse.
        provider.ContextWindow = !textContextWindow.Enabled || string.IsNullOrWhiteSpace(textContextWindow.Text) ? null
            : int.TryParse(textContextWindow.Text.Trim(), out var tokens) ? tokens : 0;
        provider.WebSearchApiKey = textWebSearchApiKey.Enabled && !string.IsNullOrWhiteSpace(textWebSearchApiKey.Text)
            ? textWebSearchApiKey.Text.Trim() : null;
    }

    #endregion
}

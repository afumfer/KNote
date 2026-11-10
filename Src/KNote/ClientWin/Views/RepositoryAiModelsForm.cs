using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

// Section Â«Repository modelsÂ» of the KNote Atlas window (RepositoryAiModelsCtrl): the AI models of the repository. Always used
// embedded (PanelView()) inside RepositoryEditorForm's TabPage, never shown as a window. The model names can be typed
// (a model declared for the repository may not be configured in this installation); the lists are this installation's
// models ("Manage models" opens this installation's AI providers). "Save models" saves them at once, as the other tabs
// of the editor do.
public partial class RepositoryAiModelsForm : KntForm, IViewEditorEmbeddable<RepositoryAiModelsDto>
{
    #region Private fields

    private readonly RepositoryAiModelsCtrl _ctrl;

    #endregion

    #region Constructor

    public RepositoryAiModelsForm(RepositoryAiModelsCtrl ctrl)
    {
        InitializeComponent();

        _ctrl = ctrl;
    }

    #endregion

    #region IViewEditorEmbeddable implementation

    public Control PanelView()
    {
        return panelForm;
    }

    public void CleanView()
    {
        comboEmbeddingModel.Text = "";
        textDimensions.Text = "";
        textQueryInstruction.Text = "";
        comboLlmModel.Text = "";
    }

    public void RefreshViewOnlyRequiredCtrl()
    {
        RefreshView();
    }

    #endregion

    #region Form event handlers

    private async void buttonSave_Click(object sender, EventArgs e)
    {
        buttonSave.Enabled = false;
        try
        {
            await _ctrl.SaveModel();
        }
        finally
        {
            buttonSave.Enabled = true;
        }
    }

    private async void buttonManageProviders_Click(object sender, EventArgs e)
    {
        await _ctrl.ManageAiProvidersAsync();
    }

    private void comboEmbeddingModel_TextChanged(object sender, EventArgs e)
    {
        labelEmbeddingModelInfo.Text = _ctrl.DescribeEmbeddingModel(comboEmbeddingModel.Text);
    }

    private void comboLlmModel_TextChanged(object sender, EventArgs e)
    {
        labelLlmModelInfo.Text = _ctrl.DescribeLlmModel(comboLlmModel.Text);
    }

    #endregion

    #region Private methods

    protected override void ModelToControls()
    {
        FillModels(comboEmbeddingModel, _ctrl.EmbeddingModels, _ctrl.Model.EmbeddingModel);
        textDimensions.Text = _ctrl.Model.EmbeddingDimensions?.ToString();
        textQueryInstruction.Text = _ctrl.Model.QueryInstruction;
        FillModels(comboLlmModel, _ctrl.ChatModels, _ctrl.Model.LlmModel);
        labelEmbeddingModelInfo.Text = _ctrl.DescribeEmbeddingModel(comboEmbeddingModel.Text);
        labelLlmModelInfo.Text = _ctrl.DescribeLlmModel(comboLlmModel.Text);
    }

    protected override void ControlsToModel()
    {
        _ctrl.Model.EmbeddingModel = RepositoryAiModelsDto.Normalize(comboEmbeddingModel.Text);
        // A text that is not a number is left for the validation to refuse.
        _ctrl.Model.EmbeddingDimensions = string.IsNullOrWhiteSpace(textDimensions.Text) ? null
            : int.TryParse(textDimensions.Text.Trim(), out var dimensions) ? dimensions : 0;
        _ctrl.Model.QueryInstruction = RepositoryAiModelsDto.Normalize(textQueryInstruction.Text);
        _ctrl.Model.LlmModel = RepositoryAiModelsDto.Normalize(comboLlmModel.Text);
    }

    private static void FillModels(ComboBox combo, List<string> models, string value)
    {
        combo.Items.Clear();
        foreach (var model in models)
            combo.Items.Add(model);
        combo.Text = value ?? "";
    }

    #endregion
}

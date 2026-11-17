using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Utils;
using KNote.Model.Core;

namespace KNote.ClientWin.Views;

// Section «Period summary» of the KNote Atlas window (RepositoryPeriodSummaryCtrl, 2.3b): the fields of the selection (the period
// with «From the beginning», the folder, the keywords and «Talks about…»), its counts and «Review selection...». A date or the folder
// prepares the selection again at once; a text, when it is left or with Enter. Below, the doubtful notes that can be asked to the AI
// (2.4) with «Classify doubtful notes with AI…», its progress and «Cancel». Always used embedded (PanelView()) inside AtlasForm.
public partial class RepositoryPeriodSummaryForm : KntForm, IViewPeriodSummary
{
    #region Private fields

    private readonly RepositoryPeriodSummaryCtrl _ctrl;
    private bool _refreshing;

    #endregion

    #region Constructor

    public RepositoryPeriodSummaryForm(RepositoryPeriodSummaryCtrl ctrl)
    {
        InitializeComponent();

        _ctrl = ctrl;
        labelDescription.Text = NoteRelevanceRules.SelectionHelp;
        buttonFolder.AlignToTextBox(textFolder);
        buttonClearFolder.AlignToTextBox(textFolder);
    }

    #endregion

    #region IViewPeriodSummary implementation

    public Control PanelView()
    {
        return panelForm;
    }

    public override void RefreshView()
    {
        _refreshing = true;
        try
        {
            dateFrom.Value = _ctrl.From;
            dateTo.Value = _ctrl.To;
            checkFromBeginning.Checked = _ctrl.FromTheBeginning;
            dateFrom.Enabled = !_ctrl.FromTheBeginning;
            textFolder.Text = _ctrl.FolderText;
            buttonClearFolder.Enabled = _ctrl.Folder != null;
            if (!textKeywords.Focused)
                textKeywords.Text = _ctrl.Keywords;
            labelMeaning.Visible = textMeaning.Visible = _ctrl.MeaningAvailable;
            if (!textMeaning.Focused)
                textMeaning.Text = _ctrl.MeaningText;
            labelCount.Text = _ctrl.CountText;
            linkRelevance.Text = _ctrl.RelevanceNoteText;
            linkRelevance.Visible = linkRelevance.Text.Length > 0;
            buttonReview.Enabled = _ctrl.CanReview;
            labelPending.Text = _ctrl.PendingText;
            buttonClassify.Visible = !_ctrl.IsClassifying && _ctrl.CanClassify;
            buttonCancelClassify.Visible = progressClassify.Visible = _ctrl.IsClassifying;
            progressClassify.Maximum = Math.Max(_ctrl.ClassifyTotal, 1);
            progressClassify.Value = Math.Min(_ctrl.ClassifyDone, progressClassify.Maximum);
        }
        finally
        {
            _refreshing = false;
        }
    }

    #endregion

    #region Form event handlers

    private async void period_Changed(object sender, EventArgs e)
    {
        if (!_refreshing)
            await _ctrl.SetPeriodAsync(dateFrom.Value, dateTo.Value, checkFromBeginning.Checked);
    }

    private async void textKeywords_Leave(object sender, EventArgs e) => await _ctrl.SetKeywordsAsync(textKeywords.Text);

    private async void textMeaning_Leave(object sender, EventArgs e) => await _ctrl.SetMeaningTextAsync(textMeaning.Text);

    private async void text_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
            return;
        e.SuppressKeyPress = true;
        if (sender == textKeywords)
            await _ctrl.SetKeywordsAsync(textKeywords.Text);
        else
            await _ctrl.SetMeaningTextAsync(textMeaning.Text);
    }

    private async void buttonFolder_Click(object sender, EventArgs e) => await _ctrl.ChooseFolderAsync();

    private async void buttonClearFolder_Click(object sender, EventArgs e) => await _ctrl.ClearFolderAsync();

    private async void buttonClassify_Click(object sender, EventArgs e) => await _ctrl.ClassifyAsync();

    private async void linkRelevance_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => await _ctrl.OpenRelevanceNoteAsync();

    private void buttonCancelClassify_Click(object sender, EventArgs e) => _ctrl.CancelClassification();

    private async void buttonReview_Click(object sender, EventArgs e)
    {
        // The texts being written count.
        await _ctrl.SetKeywordsAsync(textKeywords.Text);
        await _ctrl.SetMeaningTextAsync(textMeaning.Text);
        await _ctrl.ReviewAsync();
    }

    #endregion
}

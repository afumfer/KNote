using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Utils;
using KNote.Model;
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
        numericWords.Minimum = PeriodSummaryRules.MinWords;
        numericWords.Maximum = PeriodSummaryRules.MaxWords;
        numericWords.Value = PeriodSummaryRules.DefaultWords;
        comboLanguage.Items.AddRange(SummaryTexts.Languages.ToArray<object>());
        comboModel.Format += (_, e) => e.Value = e.ListItem is AiProviderRef p ? $"{p.Alias} ({p.Model})" : e.Value;
        // Each field of the summary goes to the ctrl as it changes: a refresh of the view shows what the user wrote.
        textSubject.TextChanged += (_, _) => { if (!_refreshing) _ctrl.Subject = textSubject.Text; };
        numericWords.ValueChanged += (_, _) => { if (!_refreshing) _ctrl.Words = (int)numericWords.Value; };
        comboLanguage.TextChanged += (_, _) => { if (!_refreshing) _ctrl.Language = comboLanguage.Text; };
        comboModel.SelectedIndexChanged += (_, _) => { if (!_refreshing) _ctrl.Provider = comboModel.SelectedItem as AiProviderRef; };
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
            RefreshSummary();
        }
        finally
        {
            _refreshing = false;
        }
    }

    // The summary (2.5b): what the ctrl has, without rewriting what is being written.
    private void RefreshSummary()
    {
        if (!textSubject.Focused)
            textSubject.Text = _ctrl.Subject;
        textSubject.PlaceholderText = _ctrl.DefaultSubject;
        numericWords.Value = Math.Clamp(_ctrl.Words, PeriodSummaryRules.MinWords, PeriodSummaryRules.MaxWords);
        if (!comboLanguage.Focused)
            comboLanguage.Text = _ctrl.Language;
        // The first item, the repository's recommended model (Provider null).
        if (!comboModel.Items.OfType<AiProviderRef>().SequenceEqual(_ctrl.Providers))
        {
            comboModel.Items.Clear();
            comboModel.Items.Add(RepositoryPeriodSummaryCtrl.RecommendedModel);
            comboModel.Items.AddRange(_ctrl.Providers.ToArray<object>());
        }
        comboModel.SelectedItem = (object)_ctrl.Provider ?? RepositoryPeriodSummaryCtrl.RecommendedModel;

        var running = _ctrl.IsRunning;
        textSubject.Enabled = numericWords.Enabled = comboLanguage.Enabled = comboModel.Enabled = !running;
        labelEstimate.Text = _ctrl.EstimateText;
        progressSummary.Visible = running;
        var progress = _ctrl.Progress;
        progressSummary.Maximum = Math.Max(progress?.CallsTotal ?? 1, 1);
        progressSummary.Value = Math.Min(progress?.CallsDone ?? 0, progressSummary.Maximum);
        progressSummary.Style = progress?.CallsTotal > 0 ? ProgressBarStyle.Continuous : ProgressBarStyle.Marquee;
        labelProgress.Text = running ? $"{_ctrl.ProgressText}\r\n{SummaryTexts.PeriodRunning}"
            : _ctrl.OtherRunning ? SummaryTexts.OtherRunning : _ctrl.ProgressText;
        buttonCancelSummary.Visible = running;
        linkOpenNote.Visible = _ctrl.CanOpenNote;
        buttonEstimate.Enabled = !running && _ctrl.Selection?.Notes.Any(n => n.IsIncluded) == true;
        buttonGenerate.Enabled = _ctrl.CanGenerate;
    }

    // The fields of the summary, into the ctrl before it is used.
    private void SummaryToCtrl()
    {
        _ctrl.Subject = textSubject.Text;
        _ctrl.Words = (int)numericWords.Value;
        _ctrl.Language = comboLanguage.Text;
        _ctrl.Provider = comboModel.SelectedItem as AiProviderRef;
    }

    #endregion

    #region Form event handlers

    private async void buttonEstimate_Click(object sender, EventArgs e)
    {
        SummaryToCtrl();
        Cursor = Cursors.WaitCursor;
        try
        {
            await _ctrl.EstimateAsync();
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private async void buttonGenerate_Click(object sender, EventArgs e)
    {
        SummaryToCtrl();
        await _ctrl.GenerateAsync();
    }

    private void buttonCancelSummary_Click(object sender, EventArgs e) => _ctrl.CancelSummary();

    private async void linkOpenNote_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => await _ctrl.OpenSummaryNoteAsync();

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

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
    private readonly ToolTip toolTipConcepts = new();
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
        labelConcepts.Text = SummaryTexts.ConceptAttributesLabel;
        toolTipConcepts.SetToolTip(checkedConcepts, SummaryTexts.ConceptAttributesHelp);
        toolTipConcepts.SetToolTip(labelConcepts, SummaryTexts.ConceptAttributesHelp);
        // ItemCheck comes before the item changes: the ctrl gets the list after it.
        checkedConcepts.ItemCheck += (_, _) => { if (!_refreshing) checkedConcepts.BeginInvoke(ConceptsToCtrl); };
        buttonFolder.AlignToTextBox(textFolder);
        buttonClearFolder.AlignToTextBox(textFolder);
        // The texts wrap with the width: the parts are placed again. ClientSizeChanged, not Resize: the scroll bar appearing or going
        // changes the width there is without changing the size of the panel.
        panelForm.AutoScrollMargin = new Size(0, LogicalToDeviceUnits(12));
        panelForm.ClientSizeChanged += (_, _) => { if (_summaryOffsets != null) LayoutSections(); };
        panelForm.VisibleChanged += (_, _) => { if (_summaryOffsets != null && panelForm.Visible) LayoutSections(); };
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
            LayoutSections();
        }
        finally
        {
            _refreshing = false;
        }
    }

    // Where the controls of the summary are, from «Summary», as the Designer has them: the block moves as a whole.
    private Dictionary<Control, int> _summaryOffsets;

    private bool _layingOut;

    // The parts below the fields of the selection, one under the other with what they show: a part without text or hidden takes no
    // room (the Designer keeps room for the doubtful notes, the estimate and the progress, which are seldom shown).
    private void LayoutSections()
    {
        // Placing the parts can show or hide the scroll bar, which changes the width there is and comes back here
        // (ClientSizeChanged): they are placed again, here, with the new width.
        if (_layingOut)
            return;
        _layingOut = true;
        try
        {
            for (var pass = 0; pass < 3; pass++)
            {
                var width = panelForm.ClientSize.Width;
                LayoutSectionsOnce();
                if (panelForm.ClientSize.Width == width)
                    break;
            }
        }
        finally
        {
            _layingOut = false;
        }
    }

    private void LayoutSectionsOnce()
    {
        var gap = LogicalToDeviceUnits(8);
        var sectionGap = LogicalToDeviceUnits(18);
        var scroll = panelForm.AutoScrollPosition.Y;
        var left = labelCount.Left;
        var right = labelDescription.Right;
        _summaryOffsets ??= new Control[] { labelSubject, textSubject, labelWords, numericWords, labelLanguage, comboLanguage, labelModel,
            comboModel, labelConcepts, checkedConcepts }.ToDictionary(c => c, c => c.Top - labelSummary.Top);

        panelForm.SuspendLayout();
        try
        {
            // The counts, with «Review selection...» at their right and the link to «Note relevance» below them.
            var y = textKeywords.Bottom - scroll + LogicalToDeviceUnits(14);
            buttonReview.Top = y + scroll;
            y = Place(labelCount, y, left, buttonReview.Left - gap - left);
            // From the state, not from Visible: it reads false while the section is not on screen.
            if (linkRelevance.Text.Length > 0)
            {
                linkRelevance.Top = y + LogicalToDeviceUnits(2) + scroll;
                y = linkRelevance.Bottom - scroll;
            }
            y = Math.Max(y, buttonReview.Bottom - scroll);

            // The doubtful notes that can be asked to the AI, only when there are.
            var classifyButton = _ctrl.IsClassifying ? buttonCancelClassify : _ctrl.CanClassify ? buttonClassify : null;
            var pending = labelPending.Text.Length > 0;
            labelPending.Visible = pending;
            if (pending || classifyButton != null)
            {
                y += gap;
                if (classifyButton != null)
                    classifyButton.Top = y + scroll;
                y = pending ? Place(labelPending, y, left, (classifyButton?.Left ?? right + gap) - gap - left) : y;
                if (_ctrl.IsClassifying)
                {
                    progressClassify.SetBounds(left, y + LogicalToDeviceUnits(4) + scroll, (classifyButton?.Left ?? right + gap) - gap - left, progressClassify.Height);
                    y = progressClassify.Bottom - scroll;
                }
                if (classifyButton != null)
                    y = Math.Max(y, classifyButton.Bottom - scroll);
            }

            // «Summary» and its fields, as a block.
            y += sectionGap;
            labelSummary.Top = y + scroll;
            foreach (var (control, offset) in _summaryOffsets)
                control.Top = labelSummary.Top + offset;
            y = checkedConcepts.Bottom - scroll + gap;

            // The estimate, the progress and its text, only with something to show; then the buttons.
            labelEstimate.Visible = labelEstimate.Text.Length > 0;
            if (labelEstimate.Visible)
                y = Place(labelEstimate, y, left, right - left) + gap;
            if (_ctrl.IsRunning)
            {
                progressSummary.Top = y + scroll;
                y = progressSummary.Bottom - scroll + LogicalToDeviceUnits(4);
            }
            labelProgress.Visible = labelProgress.Text.Length > 0;
            if (labelProgress.Visible)
                y = Place(labelProgress, y, left, right - left) + gap;
            // The buttons, not anchored: at the right of the width there is, with the margin of the left, as a row (their gaps are the
            // Designer's). With an anchor, «Generate» was left partly under the scroll bar when it appeared.
            var shift = panelForm.ClientSize.Width - left - buttonGenerate.Right;
            foreach (var button in new Control[] { buttonCancelSummary, buttonEstimate, buttonGenerate })
                button.Location = new Point(button.Left + shift, y + scroll);
            linkOpenNote.Top = y + (buttonGenerate.Height - linkOpenNote.Height) / 2 + scroll;
        }
        finally
        {
            panelForm.ResumeLayout();
        }

        // A label as high as its text in that width; returns where it ends (without the scroll).
        int Place(Label label, int top, int x, int width)
        {
            var height = TextRenderer.MeasureText(label.Text.Length == 0 ? " " : label.Text, label.Font, new Size(Math.Max(width, 1), int.MaxValue),
                TextFormatFlags.WordBreak).Height;
            label.SetBounds(x, top + scroll, width, height);
            return top + height;
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
        if (!checkedConcepts.Items.OfType<PeriodConceptRules.Choice>().SequenceEqual(_ctrl.ConceptChoices))
        {
            checkedConcepts.Items.Clear();
            checkedConcepts.Items.AddRange(_ctrl.ConceptChoices.ToArray<object>());
            // Columns as wide as the longest «Note type · Attribute» (with its check box), never wider than the list.
            var widest = _ctrl.ConceptChoices.Select(c => TextRenderer.MeasureText(c.Label, checkedConcepts.Font).Width).DefaultIfEmpty(0).Max();
            checkedConcepts.ColumnWidth = Math.Clamp(widest + LogicalToDeviceUnits(30), LogicalToDeviceUnits(150),
                Math.Max(checkedConcepts.ClientSize.Width - LogicalToDeviceUnits(4), LogicalToDeviceUnits(150)));
        }
        for (var i = 0; i < checkedConcepts.Items.Count; i++)
            checkedConcepts.SetItemChecked(i, _ctrl.ConceptAttributeIds.Contains(((PeriodConceptRules.Choice)checkedConcepts.Items[i]).KAttributeId));
        labelConcepts.Text = _ctrl.ConceptChoices.Count == 0
            ? $"{SummaryTexts.ConceptAttributesLabel}: the repository has no attributes that group the notes"
            : SummaryTexts.ConceptAttributesLabel;

        var running = _ctrl.IsRunning;
        textSubject.Enabled = numericWords.Enabled = comboLanguage.Enabled = comboModel.Enabled = checkedConcepts.Enabled = !running;
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
        ConceptsToCtrl();
    }

    private void ConceptsToCtrl() => _ctrl.ConceptAttributeIds = checkedConcepts.CheckedItems.OfType<PeriodConceptRules.Choice>().Select(c => c.KAttributeId).ToList();

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

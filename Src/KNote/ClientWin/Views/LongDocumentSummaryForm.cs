using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Utils;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

// KNote Atlas, stage 2: «Summarize document» (2.1a2) and «Translate document» (2.1b2), the view of LongDocumentSummaryCtrl. Not modal:
// the run is in the Store and goes on if the window is closed; opened again, it shows it. Language (a list or free text), length (of a
// summary) or the translation not finished with «Continue» (of a translation), and model; «Estimate», «Generate», its progress, «Cancel»
// and, at the end, «Open the note».
public partial class LongDocumentSummaryForm : KntForm, IViewLongDocumentSummary
{
    private readonly LongDocumentSummaryCtrl _ctrl;
    private bool _loading;

    public LongDocumentSummaryForm()
    {
        InitializeComponent();
    }

    public LongDocumentSummaryForm(LongDocumentSummaryCtrl ctrl) : this()
    {
        _ctrl = ctrl;
        labelIntro.Text = SummaryTexts.IntroOf(ctrl.Kind);
        labelLanguageHelp.Text = SummaryTexts.LanguageHelpOf(ctrl.Kind);
        buttonContinue.Text = SummaryTexts.Continue;
        labelLength.Visible = comboLength.Visible = !ctrl.IsTranslation;
        buttonEstimate.Text = SummaryTexts.Estimate;
        buttonGenerate.Text = SummaryTexts.Generate;
        buttonCancelRun.Text = SummaryTexts.Cancel;
        buttonOpenNote.Text = SummaryTexts.OpenNote;
        buttonTranscribeFirst.Text = SummaryTexts.TranscribeFirst;
        comboLanguage.Items.AddRange(SummaryTexts.Languages.Cast<object>().ToArray());
        comboLength.Items.AddRange(SummaryTexts.Lengths.Select(SummaryTexts.LengthText).Cast<object>().ToArray());
        comboLanguage.TextChanged += (_, _) => { if (!_loading) _ctrl.Language = comboLanguage.Text; };
        comboLength.SelectedIndexChanged += (_, _) => { if (!_loading && comboLength.SelectedIndex >= 0) _ctrl.Length = SummaryTexts.Lengths[comboLength.SelectedIndex]; };
        comboModel.SelectedIndexChanged += (_, _) => { if (!_loading && comboModel.SelectedIndex >= 0) _ctrl.Provider = _ctrl.Providers[comboModel.SelectedIndex]; };
    }

    public override void ShowView()
    {
        CenterOnManagementWindow();
        Show();
    }

    // Centered on the main window (KNoteManagement), whichever window opened it: CenterParent only centers a modal window.
    private void CenterOnManagementWindow()
    {
        var main = Application.OpenForms.OfType<KNoteManagementForm>()
            .FirstOrDefault(f => f.Visible && f.WindowState != FormWindowState.Minimized);
        if (main != null)
            WindowPlacement.CenterOn(this, main);
        else
            StartPosition = FormStartPosition.CenterScreen;
    }

    protected override void ModelToControls()
    {
        _loading = true;
        Text = $"{SummaryTexts.TitleOf(_ctrl.Kind)} · {_ctrl.ServiceRef?.Name}";
        labelFile.Text = _ctrl.FileName;
        comboLanguage.Text = _ctrl.Language;
        comboLength.SelectedIndex = Math.Max(0, SummaryTexts.Lengths.ToList().IndexOf(_ctrl.Length));
        comboModel.Items.Clear();
        comboModel.Items.AddRange(_ctrl.Providers.Select(p => (object)$"{p.Alias} ({p.Model})").ToArray());
        comboModel.SelectedIndex = _ctrl.Provider == null ? -1 : _ctrl.Providers.IndexOf(_ctrl.Provider);
        _loading = false;

        var estimate = _ctrl.Estimate;
        labelEstimate.ForeColor = _ctrl.EstimateError != null ? Color.Firebrick : SystemColors.ControlText;
        labelEstimate.Text = _ctrl.EstimateError
            ?? (estimate == null ? "" : string.Join(Environment.NewLine, SummaryTexts.EstimateLines(estimate, _ctrl.Provider?.Provider)));
        buttonTranscribeFirst.Visible = estimate?.MissingPages?.Count > 0;
        labelUnfinished.Text = _ctrl.Unfinished is { } unfinished ? SummaryTexts.Unfinished(unfinished) : "";

        RefreshProgress();
    }

    public void RefreshProgress()
    {
        if (IsDisposed)
            return;

        var progress = _ctrl.Progress;
        var running = _ctrl.IsRunning;
        progressRun.Visible = running;
        progressRun.Value = (int)Math.Round(SummaryTexts.Share(progress) * 100);
        labelProgress.Text = progress == null ? (_ctrl.OtherRunning ? SummaryTexts.OtherRunning : "")
            : running ? $"{SummaryTexts.Progress(progress)}{Environment.NewLine}{SummaryTexts.RunningOf(_ctrl.Kind)}"
            : _ctrl.OtherRunning ? $"{SummaryTexts.Progress(progress)}{Environment.NewLine}{(_ctrl.IndexRunning ? SummaryTexts.IndexRunning : SummaryTexts.OtherRunning)}"
            : SummaryTexts.Progress(progress);
        labelProgress.ForeColor = progress?.Phase == EnumDocumentProcessPhase.Failed ? Color.Firebrick : SystemColors.ControlText;

        comboLanguage.Enabled = comboLength.Enabled = comboModel.Enabled = buttonEstimate.Enabled = !running;
        buttonGenerate.Enabled = !running && !_ctrl.OtherRunning && _ctrl.Provider != null;
        buttonCancelRun.Visible = running;
        buttonOpenNote.Visible = !running && SummaryTexts.HasNote(progress);
        labelUnfinished.Visible = buttonContinue.Visible = _ctrl.Unfinished != null;
        buttonContinue.Enabled = buttonGenerate.Enabled;
    }

    private async void buttonEstimate_Click(object sender, EventArgs e)
    {
        buttonEstimate.Enabled = false;
        labelEstimate.ForeColor = SystemColors.GrayText;
        labelEstimate.Text = "Estimating…";
        try
        {
            await _ctrl.EstimateAsync();
        }
        finally
        {
            if (!IsDisposed)
                buttonEstimate.Enabled = !_ctrl.IsRunning;
        }
    }

    private async void buttonGenerate_Click(object sender, EventArgs e)
    {
        buttonGenerate.Enabled = false;
        if (!await _ctrl.GenerateAsync() && !IsDisposed)
            RefreshProgress();
    }

    private async void buttonContinue_Click(object sender, EventArgs e)
    {
        buttonContinue.Enabled = buttonGenerate.Enabled = false;
        if (!await _ctrl.ContinueAsync() && !IsDisposed)
            RefreshProgress();
    }

    private void buttonCancelRun_Click(object sender, EventArgs e) => _ctrl.Cancel();

    private async void buttonOpenNote_Click(object sender, EventArgs e) => await _ctrl.OpenNoteAsync();

    private void buttonTranscribeFirst_Click(object sender, EventArgs e) => _ctrl.RequestTranscription();

    private void buttonClose_Click(object sender, EventArgs e) => Close();

    protected override void OnUserClosing(FormClosingEventArgs e) => _ctrl.Finalize();
}

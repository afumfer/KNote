using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

// Section Â«Process pending resourcesÂ» of the KNote Atlas window (RepositoryResourcesCtrl): what «Process pending resources» would process and
// running it, with the choice for the descriptions written by people and its progress. Always used embedded (PanelView())
// inside RepositoryEditorForm's TabPage, never shown as a window. The run is the Store's: closing the editor does not stop it.
public partial class RepositoryResourcesForm : KntForm, IViewResourceProcessing
{
    #region Private fields

    private readonly RepositoryResourcesCtrl _ctrl;

    #endregion

    #region Constructor

    public RepositoryResourcesForm(RepositoryResourcesCtrl ctrl)
    {
        InitializeComponent();

        _ctrl = ctrl;
        labelDescription.Text = ResourceProcessingTexts.Description;
        radioSkip.Text = ResourceProcessingTexts.PersonChoiceSkip;
        radioLock.Text = ResourceProcessingTexts.PersonChoiceLock;
        radioReplace.Text = ResourceProcessingTexts.PersonChoiceReplace;
    }

    #endregion

    #region IViewResourceProcessing implementation

    public Control PanelView()
    {
        return panelForm;
    }

    public override void RefreshView()
    {
        labelCounts.Text = _ctrl.CountsText;
        labelLastRun.Text = _ctrl.LastRunText;
        labelPersonPhotos.Text = _ctrl.PersonPhotosText;
        labelPersonPhotos.Visible = radioSkip.Visible = radioLock.Visible = radioReplace.Visible = _ctrl.HasPersonPhotos;
        labelWarning.Text = _ctrl.WarningText;

        RefreshProgress();
    }

    public void RefreshProgress()
    {
        var progress = _ctrl.Progress;
        labelProgress.Text = _ctrl.ProgressText;
        progressBarRun.Visible = _ctrl.IsRunning;
        if (progress != null && progress.Phase == EnumResourceProcessingPhase.Processing && progress.PhotosTotal > 0)
        {
            progressBarRun.Style = ProgressBarStyle.Continuous;
            progressBarRun.Maximum = progress.PhotosTotal;
            progressBarRun.Value = Math.Min(progress.PhotosDone, progress.PhotosTotal);
        }
        else
            progressBarRun.Style = ProgressBarStyle.Marquee;

        var errors = _ctrl.ErrorLines;
        listErrors.Visible = errors.Count > 0;
        listErrors.Items.Clear();
        foreach (var line in errors)
            listErrors.Items.Add(line);

        radioSkip.Enabled = radioLock.Enabled = radioReplace.Enabled = !_ctrl.IsRunning;
        buttonRun.Enabled = _ctrl.CanRun;
        buttonCancelRun.Visible = _ctrl.IsRunning;
    }

    #endregion

    #region Form event handlers

    private async void buttonRun_Click(object sender, EventArgs e)
    {
        await _ctrl.RunAsync();
    }

    private void buttonCancelRun_Click(object sender, EventArgs e)
    {
        _ctrl.CancelRun();
    }

    private void radioPersonDescriptions_CheckedChanged(object sender, EventArgs e)
    {
        if (sender is not RadioButton { Checked: true })
            return;
        _ctrl.PersonDescriptions = radioReplace.Checked ? EnumPersonDescriptions.Replace
            : radioLock.Checked ? EnumPersonDescriptions.Lock
            : EnumPersonDescriptions.Skip;
        buttonRun.Enabled = _ctrl.CanRun;
    }

    #endregion
}

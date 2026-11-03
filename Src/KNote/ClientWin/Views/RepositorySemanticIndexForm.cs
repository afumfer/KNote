using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

// "Semantic index" tab of the repository editor (RepositorySemanticIndexCtrl): the status of the repository's index and
// creating, updating or rebuilding it, with its progress. Always used embedded (PanelView()) inside RepositoryEditorForm's
// TabPage, never shown as a window. The run is the Store's: closing the editor does not stop it.
public partial class RepositorySemanticIndexForm : KntForm, IViewSemanticIndex
{
    #region Private fields

    private readonly RepositorySemanticIndexCtrl _ctrl;

    #endregion

    #region Constructor

    public RepositorySemanticIndexForm(RepositorySemanticIndexCtrl ctrl)
    {
        InitializeComponent();

        _ctrl = ctrl;
    }

    #endregion

    #region IViewSemanticIndex implementation

    public Control PanelView()
    {
        return panelForm;
    }

    public override void RefreshView()
    {
        labelStatus.Text = _ctrl.StatusText;
        labelCounts.Text = _ctrl.CountsText;
        labelLastCreation.Text = _ctrl.LastCreationText;
        labelWarning.Text = _ctrl.WarningText;

        listOtherIndexes.Items.Clear();
        foreach (var index in _ctrl.Status?.OtherIndexes ?? new List<SemanticIndexModelDto>())
            listOtherIndexes.Items.Add(new OtherIndexItem(index));
        labelOtherIndexes.Visible = listOtherIndexes.Visible = buttonDeleteIndex.Visible = listOtherIndexes.Items.Count > 0;

        RefreshProgress();
    }

    public void RefreshProgress()
    {
        var progress = _ctrl.Progress;
        labelProgress.Text = _ctrl.ProgressText;
        progressBarIndex.Visible = _ctrl.IsRunning;
        if (progress != null && progress.Phase == EnumSemanticIndexPhase.Indexing && progress.NotesTotal > 0)
        {
            progressBarIndex.Style = ProgressBarStyle.Continuous;
            progressBarIndex.Maximum = progress.NotesTotal;
            progressBarIndex.Value = Math.Min(progress.NotesDone, progress.NotesTotal);
        }
        else
            progressBarIndex.Style = ProgressBarStyle.Marquee;

        var errors = _ctrl.ErrorLines;
        listErrors.Visible = errors.Count > 0;
        listErrors.Items.Clear();
        foreach (var line in errors)
            listErrors.Items.Add(line);

        buttonRun.Text = _ctrl.RunText;
        buttonRun.Enabled = _ctrl.CanUpdate;
        buttonRebuild.Enabled = _ctrl.CanRebuild;
        buttonCancelRun.Visible = _ctrl.IsRunning;
        buttonDeleteIndex.Enabled = listOtherIndexes.SelectedItem != null;
    }

    #endregion

    #region Form event handlers

    private void buttonRun_Click(object sender, EventArgs e)
    {
        _ctrl.Run(rebuild: false);
    }

    private void buttonRebuild_Click(object sender, EventArgs e)
    {
        _ctrl.Run(rebuild: true);
    }

    private void buttonCancelRun_Click(object sender, EventArgs e)
    {
        _ctrl.CancelRun();
    }

    private async void buttonDeleteIndex_Click(object sender, EventArgs e)
    {
        if (listOtherIndexes.SelectedItem is OtherIndexItem item)
            await _ctrl.DeleteIndexAsync(item.Index.IndexKey);
    }

    private void listOtherIndexes_SelectedIndexChanged(object sender, EventArgs e)
    {
        buttonDeleteIndex.Enabled = listOtherIndexes.SelectedItem != null;
    }

    #endregion

    #region Private types

    private sealed record OtherIndexItem(SemanticIndexModelDto Index)
    {
        public override string ToString() => $"{Index.IndexKey} · {Index.Notes} notes · {Index.Chunks} fragments";
    }

    #endregion
}

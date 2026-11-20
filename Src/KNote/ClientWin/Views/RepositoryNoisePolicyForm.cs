using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model.Core;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

// Section «Noise policy» of the KNote Atlas window (RepositoryNoisePolicyCtrl, 2.2): the note types (check boxes) and the folders
// (a list with «Add…», the folder selector, and «Remove») whose notes never go into a summary, the recount of the notes it leaves
// out (and «Count again»), «Discard changes» and «Save policy». Who can't change it sees it. Always used embedded (PanelView()) inside AtlasForm.
public partial class RepositoryNoisePolicyForm : KntForm, IViewNoisePolicy
{
    #region Private fields

    private readonly RepositoryNoisePolicyCtrl _ctrl;
    private readonly List<Guid> _folderIds = new();
    private bool _refreshing;

    #endregion

    #region Constructor

    public RepositoryNoisePolicyForm(RepositoryNoisePolicyCtrl ctrl)
    {
        InitializeComponent();

        _ctrl = ctrl;
        labelDescription.Text = NoiseRules.Help;
        checkedListTypes.DisplayMember = nameof(NoteTypeDto.Name);
    }

    #endregion

    #region IViewNoisePolicy implementation

    public Control PanelView()
    {
        return panelForm;
    }

    public override void RefreshView()
    {
        _refreshing = true;
        try
        {
            var canSave = _ctrl.CanSave;
            labelReadOnly.Text = _ctrl.ReadOnlyText;
            labelReadOnly.Visible = !canSave;

            var noteTypes = _ctrl.NoteTypes;
            if (!checkedListTypes.Items.Cast<NoteTypeDto>().SequenceEqual(noteTypes))
            {
                checkedListTypes.Items.Clear();
                foreach (var noteType in noteTypes)
                    checkedListTypes.Items.Add(noteType);
            }
            for (var i = 0; i < checkedListTypes.Items.Count; i++)
                checkedListTypes.SetItemChecked(i, _ctrl.IsExcluded(((NoteTypeDto)checkedListTypes.Items[i]).NoteTypeId));
            labelNoTypes.Visible = _ctrl.Policy != null && noteTypes.Count == 0;

            var selectedId = SelectedFolderId;
            var folders = _ctrl.Folders;
            listFolders.BeginUpdate();
            listFolders.Items.Clear();
            _folderIds.Clear();
            foreach (var (folderId, path) in folders)
            {
                _folderIds.Add(folderId);
                listFolders.Items.Add(path);
            }
            listFolders.SelectedIndex = selectedId is Guid id ? _folderIds.IndexOf(id) : -1;
            listFolders.EndUpdate();
            labelFolders.Text = $"Folders left out ({folders.Count})";

            buttonAddFolder.Enabled = canSave && _ctrl.Policy != null;
            buttonSave.Visible = buttonDiscard.Visible = canSave;
        }
        finally
        {
            _refreshing = false;
        }

        RefreshCount();
    }

    public void RefreshCount()
    {
        labelCount.Text = _ctrl.CountText;
        buttonCountAgain.Enabled = _ctrl.CanCountAgain;
        buttonSave.Enabled = buttonDiscard.Enabled = _ctrl.IsChanged;
        buttonRemoveFolder.Enabled = _ctrl.CanSave && SelectedFolderId != null;
    }

    #endregion

    #region Form event handlers

    private void checkedListTypes_ItemCheck(object sender, ItemCheckEventArgs e)
    {
        if (_refreshing)
            return;
        // Who can't change it only sees it.
        if (!_ctrl.CanSave)
        {
            e.NewValue = e.CurrentValue;
            return;
        }

        // After the check box changes (the recount draws the list again). The list's BeginInvoke, not the form's: the form is
        // only used embedded (PanelView), so it never creates its own window handle and its BeginInvoke throws.
        var noteTypeId = ((NoteTypeDto)checkedListTypes.Items[e.Index]).NoteTypeId;
        var excluded = e.NewValue == CheckState.Checked;
        checkedListTypes.BeginInvoke(async () => await _ctrl.SetNoteTypeExcludedAsync(noteTypeId, excluded));
    }

    private async void buttonAddFolder_Click(object sender, EventArgs e)
    {
        await _ctrl.ChooseAndAddFolderAsync();
    }

    private async void buttonRemoveFolder_Click(object sender, EventArgs e)
    {
        if (SelectedFolderId is Guid folderId)
            await _ctrl.RemoveFolderAsync(folderId);
    }

    private void listFolders_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (!_refreshing)
            buttonRemoveFolder.Enabled = _ctrl.CanSave && SelectedFolderId != null;
    }

    private async void listFolders_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Delete && _ctrl.CanSave && SelectedFolderId is Guid folderId)
        {
            e.Handled = true;
            await _ctrl.RemoveFolderAsync(folderId);
        }
    }

    private async void buttonCountAgain_Click(object sender, EventArgs e)
    {
        await _ctrl.CountAgainAsync();
    }

    private async void buttonDiscard_Click(object sender, EventArgs e)
    {
        await _ctrl.DiscardAsync();
    }

    private async void buttonSave_Click(object sender, EventArgs e)
    {
        Cursor = Cursors.WaitCursor;
        try
        {
            await _ctrl.SaveAsync();
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    #endregion

    #region Private methods

    private Guid? SelectedFolderId =>
        listFolders.SelectedIndex >= 0 && listFolders.SelectedIndex < _folderIds.Count ? _folderIds[listFolders.SelectedIndex] : null;

    #endregion
}

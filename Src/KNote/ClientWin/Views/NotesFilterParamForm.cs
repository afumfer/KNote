using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Views;

public partial class NotesFilterParamForm : Form, IViewEmbeddable
{
    #region Private fields

    // Sentinel item for "no note type" (NotesFilterDto.NoteTypeId is nullable): a real NoteTypeDto
    // so comboNoteType.DisplayMember="Name" works uniformly for every item.
    private static readonly NoteTypeDto NoNoteTypeItem = new() { NoteTypeId = Guid.Empty, Name = "(none)" };

    private readonly NotesFilterParamCtrl _ctrl;
    private bool _viewFinalized = false;

    private Guid? _folderId;
    private readonly List<AtrFilterDto> _attributesFilter = new();

    #endregion

    #region Constructor

    public NotesFilterParamForm(NotesFilterParamCtrl ctrl)
    {
        InitializeComponent();

        _ctrl = ctrl;
    }

    #endregion

    #region IViewEmbeddable implementation

    public Control PanelView()
    {
        return panelForm;
    }

    public void ShowView()
    {
        this.Show();
    }

    public Result<EControllerResult> ShowModalView()
    {
        var res = _ctrl.DialogResultToControllerResult(this.ShowDialog());
        return res;
    }

    public void RefreshView()
    {
        PersonalizeControls();
    }

    public void OnClosingView()
    {
        _viewFinalized = true;
        this.Close();
    }

    public void ConfigureEmbededMode()
    {
        TopLevel = false;
        Dock = DockStyle.Fill;
        FormBorderStyle = FormBorderStyle.None;
        panelBottom.Visible = false;
    }

    public void ConfigureWindowMode()
    {
        TopLevel = true;
        Dock = DockStyle.None;
        FormBorderStyle = FormBorderStyle.Sizable;
        panelBottom.Visible = true;
        StartPosition = FormStartPosition.CenterScreen;
    }

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Asterisk)
    {
        return MessageBox.Show(info, caption, buttons, icon);
    }

    #endregion

    #region Form events handlers

    private void NotesFilterParamForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!_viewFinalized)
            _ctrl.Finalize();
    }

    private void buttonClean_Click(object sender, EventArgs e)
    {
        CleanView();
    }

    private void buttonFilter_Click(object sender, EventArgs e)
    {
        var selectedNoteType = comboNoteType.SelectedItem as NoteTypeDto;

        var notesFilter = new NotesFilterDto
        {
            Topic = string.IsNullOrWhiteSpace(textTopic.Text) ? null : textTopic.Text,
            Description = string.IsNullOrWhiteSpace(textDescription.Text) ? null : textDescription.Text,
            Tags = string.IsNullOrWhiteSpace(textTags.Text) ? null : textTags.Text,
            NoteTypeId = selectedNoteType == null || selectedNoteType.NoteTypeId == Guid.Empty ? null : selectedNoteType.NoteTypeId,
            FolderId = _folderId,
            AttributesFilter = new List<AtrFilterDto>(_attributesFilter)
        };

        var filter = new SelectedNotesInServiceRef
        {
            ServiceRef = comboRepositories.SelectedItem as ServiceRef,
            NotesFilter = notesFilter
        };

        _ctrl.NotifyFilterApplied(filter);
    }

    private async void comboRepositories_SelectedIndexChanged(object sender, EventArgs e)
    {
        var serviceRef = comboRepositories.SelectedItem as ServiceRef;

        await _ctrl.LoadNoteTypes(serviceRef?.Service);
        await _ctrl.LoadKAttributes(serviceRef?.Service);

        PopulateNoteTypes();

        _folderId = null;
        textFolder.Text = "";
        _attributesFilter.Clear();
        listViewAttributes.Items.Clear();
    }

    private void buttonFolderSelect_Click(object sender, EventArgs e)
    {
        var serviceRef = comboRepositories.SelectedItem as ServiceRef;
        if (serviceRef == null)
            return;

        var folderSelector = new FoldersSelectorCtrl(_ctrl.Store);
        folderSelector.ServicesRef = new List<ServiceRef> { serviceRef };
        var res = folderSelector.RunModal();
        if (res.Entity == EControllerResult.Executed && folderSelector.SelectedEntity?.FolderInfo != null)
        {
            _folderId = folderSelector.SelectedEntity.FolderInfo.FolderId;
            textFolder.Text = folderSelector.SelectedEntity.FolderInfo.Name;
        }
    }

    private void buttonFolderClear_Click(object sender, EventArgs e)
    {
        _folderId = null;
        textFolder.Text = "";
    }

    private void buttonAddAttribute_Click(object sender, EventArgs e)
    {
        using var dlg = new NoteAttributeFilterSelectorForm(_ctrl.KAttributes);
        if (dlg.ShowDialog(this) == DialogResult.OK && dlg.Result != null)
        {
            _attributesFilter.Add(dlg.Result);
            AddAttributeRow(dlg.Result);
        }
    }

    private void buttonRemoveAttribute_Click(object sender, EventArgs e)
    {
        if (listViewAttributes.SelectedItems.Count == 0)
            return;

        var item = listViewAttributes.SelectedItems[0];
        if (item.Tag is AtrFilterDto atr)
            _attributesFilter.Remove(atr);

        listViewAttributes.Items.Remove(item);
    }

    private void buttonAccept_Click(object sender, EventArgs e)
    {
        _ctrl.Finalize();
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
        _ctrl.Finalize();
    }

    #endregion

    #region Private methods

    private void PersonalizeControls()
    {
        comboRepositories.Items.Clear();
        foreach (var serviceRef in _ctrl.Store.GetAllServiceRef())
            comboRepositories.Items.Add(serviceRef);
        comboRepositories.ValueMember = "IdServiceRef";
        comboRepositories.DisplayMember = "Alias";
        comboRepositories.SelectedIndex = comboRepositories.Items.Count > 0 ? 0 : -1;
    }

    private void PopulateNoteTypes()
    {
        comboNoteType.DisplayMember = "Name";
        comboNoteType.Items.Clear();
        comboNoteType.Items.Add(NoNoteTypeItem);
        foreach (var noteType in _ctrl.NoteTypes)
            comboNoteType.Items.Add(noteType);
        comboNoteType.SelectedIndex = 0;
    }

    private void AddAttributeRow(AtrFilterDto atr)
    {
        var item = new ListViewItem(atr.AtrName) { Tag = atr };
        item.SubItems.Add(atr.Value);
        listViewAttributes.Items.Add(item);
    }

    private void CleanView()
    {
        textTopic.Text = "";
        textDescription.Text = "";
        textTags.Text = "";
        comboNoteType.SelectedIndex = comboNoteType.Items.Count > 0 ? 0 : -1;
        _folderId = null;
        textFolder.Text = "";
        _attributesFilter.Clear();
        listViewAttributes.Items.Clear();
    }

    #endregion
}

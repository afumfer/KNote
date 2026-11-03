using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Utils;
using KntIcons;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Views;

public partial class NotesFilterParamForm : KntForm, IViewEmbeddable
{
    #region Private fields

    // Sentinel item for "no note type" (NotesFilterDto.NoteTypeId is nullable): a real NoteTypeDto
    // so comboNoteType.DisplayMember="Name" works uniformly for every item.
    private static readonly NoteTypeDto NoNoteTypeItem = new() { NoteTypeId = Guid.Empty, Name = "(none)" };

    private readonly NotesFilterParamCtrl _ctrl;

    private Guid? _folderId;
    private readonly List<AtrFilterDto> _attributesFilter = new();

    // Primary/growing column for ListViewColumnResizer - "Value" is the more variable-length
    // column; "Attribute" (a name picked from a fixed selector) stays at its designer width.
    private const int PrimaryColumnIndex = 1;

    private ListViewColumnSorter _sorter;
    // «Talks about…» shown (as in the Designer). Not the controls' Visible: false while the form is not shown.
    private bool _meaningTextShown = true;

    #endregion

    #region Constructor

    public NotesFilterParamForm(NotesFilterParamCtrl ctrl)
    {
        InitializeComponent();

        buttonFolderClear.AlignToTextBox(textFolder);
        buttonFolderSelect.AlignToTextBox(textFolder);

        buttonAddAttribute.SetKntIcon(KntIcon.Add);
        buttonRemoveAttribute.SetKntIcon(KntIcon.Remove);

        _ctrl = ctrl;

        _sorter = ListViewSortHelper.Attach(listViewAttributes);
        listViewAttributes.Resize += (s, e) => ListViewColumnResizer.Resize(listViewAttributes, PrimaryColumnIndex);

        // Until the repository is known to search by meaning (RefreshMeaningTextAsync).
        SetMeaningTextVisible(false);
    }

    #endregion

    #region IViewEmbeddable implementation

    public Control PanelView()
    {
        return panelForm;
    }

    public override void RefreshView()
    {
        PersonalizeControls();
    }

    public override void ConfigureEmbededMode()
    {
        TopLevel = false;
        Dock = DockStyle.Fill;
        FormBorderStyle = FormBorderStyle.None;
        panelBottom.Visible = false;
    }

    public override void ConfigureWindowMode()
    {
        TopLevel = true;
        Dock = DockStyle.None;
        FormBorderStyle = FormBorderStyle.Sizable;
        panelBottom.Visible = true;
        StartPosition = FormStartPosition.CenterScreen;
    }

    #endregion

    #region Form events handlers

    protected override void OnUserClosing(FormClosingEventArgs e)
        => _ctrl.Finalize();

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
            IncludeChildFolders = _folderId != null && checkIncludeChildFolders.Checked,
            AttributesFilter = new List<AtrFilterDto>(_attributesFilter),
            MeaningText = _meaningTextShown && !string.IsNullOrWhiteSpace(textMeaningText.Text) ? textMeaningText.Text.Trim() : null
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

        ClearFolderSelection();
        _attributesFilter.Clear();
        listViewAttributes.Items.Clear();

        await RefreshMeaningTextAsync(serviceRef);
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
            checkIncludeChildFolders.Enabled = true;
        }
    }

    private void buttonFolderClear_Click(object sender, EventArgs e)
    {
        ClearFolderSelection();
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
        ListViewSelectionHelper.SelectFirst(listViewAttributes, _sorter);
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
        comboRepositories.DisplayMember = "Name";
        comboRepositories.SelectedIndex = comboRepositories.Items.Count > 0 ? 0 : -1;

        ListViewColumnResizer.Resize(listViewAttributes, PrimaryColumnIndex);
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
        // AtrFilterDto is a transient filter criterion, not a persisted entity with its own id, so a
        // synthetic key is generated here purely so ListViewSelectionHelper can select this row back.
        var item = new ListViewItem(atr.AtrName) { Name = Guid.NewGuid().ToString(), Tag = atr };
        item.SubItems.Add(atr.Value);
        listViewAttributes.Items.Add(item);
        ListViewSelectionHelper.SelectByKey(listViewAttributes, item.Name, _sorter);
    }

    // «Talks about…» (KNote Atlas: by meaning among the notes of the other criteria) only where the repository chosen can
    // search by meaning. Hidden, the attributes list takes its room.
    private async Task RefreshMeaningTextAsync(ServiceRef serviceRef)
    {
        var canSearchByMeaning = await _ctrl.Store.CanSearchByMeaningAsync(serviceRef);
        if (IsDisposed || comboRepositories.SelectedItem != serviceRef)
            return; // another repository chosen meanwhile
        SetMeaningTextVisible(canSearchByMeaning);
    }

    private void SetMeaningTextVisible(bool visible)
    {
        if (_meaningTextShown == visible)
            return;
        // From where the field is (anchored to the bottom, it moves with the form): above it, or down to its bottom.
        listViewAttributes.Height = (visible ? labelMeaningText.Top - LogicalToDeviceUnits(8) : textMeaningText.Bottom) - listViewAttributes.Top;
        labelMeaningText.Visible = visible;
        textMeaningText.Visible = visible;
        _meaningTextShown = visible;
    }

    private void CleanView()
    {
        textTopic.Text = "";
        textMeaningText.Text = "";
        textDescription.Text = "";
        textTags.Text = "";
        comboNoteType.SelectedIndex = comboNoteType.Items.Count > 0 ? 0 : -1;
        ClearFolderSelection();
        _attributesFilter.Clear();
        listViewAttributes.Items.Clear();
    }

    // Include-subfolders only makes sense once a folder is actually selected, so it's cleared and
    // disabled whenever the folder selection itself is cleared (repository change, "X", "Clean").
    private void ClearFolderSelection()
    {
        _folderId = null;
        textFolder.Text = "";
        checkIncludeChildFolders.Checked = false;
        checkIncludeChildFolders.Enabled = false;
    }

    #endregion
}

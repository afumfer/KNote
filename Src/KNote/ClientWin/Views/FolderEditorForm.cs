using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

public partial class FolderEditorForm : KntEditorForm, IViewEditor<FolderDto>
{
    #region Private fields

    private readonly FolderEditorCtrl _ctrl;
    private Guid? _selectedParentFolderId;
    private FolderDto _selectedParentFolder;

    // What ModelToControls() last parsed from Model.OrderNotes - used by ControlsToModel() to
    // preserve the cached Auto column/direction when the user leaves the mode combo on "Auto"
    // without touching the (disabled) column/direction combos, and by the info label to describe
    // the current Auto state.
    private NoteOrderCriteria _loadedOrderCriteria = new NoteOrderCriteria(NoteOrderMode.Default, NoteOrderCriteria.DefaultColumn, true);

    // Column combo items: (property name stored in OrderNotes, friendly label). Keep in sync with
    // the DataPropertyName/HeaderText pairs configured for dataGridNotes in NotesSelectorForm.
    private static readonly (string Column, string Label)[] OrderableColumns =
    {
        ("NoteNumber", "Number"),
        ("Topic", "Topic"),
        ("Priority", "Priority"),
        ("Tags", "Tags"),
        ("InternalTags", "Status"),
        ("ModificationDateTime", "Modification date"),
        ("CreationDateTime", "Creation date"),
    };

    #endregion

    #region Constructor

    public FolderEditorForm(FolderEditorCtrl ctrl)
    {

        InitializeComponent();

        _ctrl = ctrl;

        comboOrderMode.Items.AddRange(new object[]
        {
            "By note number (default)",
            "Fixed order",
            "Follow last order used in the notes selector"
        });

        comboOrderColumn.Items.AddRange(OrderableColumns.Select(c => (object)c.Label).ToArray());

        comboOrderDirection.Items.AddRange(new object[] { "Ascending", "Descending" });
    }

    #endregion

    #region Form events handler

    private async void buttonAccept_Click(object sender, EventArgs e)
    {
        await AcceptEditionAsync();
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
        TryCancelEdition();
    }

    protected override Task<bool> SaveModelAsync()
        => _ctrl.SaveModel();

    protected override void CancelEdition()
        => _ctrl.CancelEdition();

    private void buttonFolderSearch_Click(object sender, EventArgs e)
    {
        var folder = _ctrl.GetFolder();
        if (folder != null)
        {
            _selectedParentFolderId = folder.FolderId;
            _selectedParentFolder = folder.GetSimpleDto<FolderDto>();
            textParentFolder.Text = folder?.Name;
        }
    }

    private void textParentFolder_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Delete)
        {
            _selectedParentFolder = null;
            _selectedParentFolderId = null;
            textParentFolder.Text = "(root)";
        }
    }

    private void comboOrderMode_SelectedIndexChanged(object sender, EventArgs e)
    {
        var isFixed = comboOrderMode.SelectedIndex == (int)NoteOrderMode.Fixed;
        comboOrderColumn.Enabled = isFixed;
        comboOrderDirection.Enabled = isFixed;
        UpdateOrderNotesInfoLabel();
    }

    private void comboOrder_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateOrderNotesInfoLabel();
    }

    // SelectedIndexChanged also fires for the programmatic SelectedIndex assignments in
    // SetOrderNotesControls (called from ModelToControls on load), so it must not mark the form as
    // modified. SelectionChangeCommitted fires only for an actual user-driven selection.
    private void comboOrder_SelectionChangeCommitted(object sender, EventArgs e)
    {
        FormIsDirty = true;
    }

    #endregion

    #region Private methods

    protected override void ModelToControls()
    {
        textName.Text = _ctrl.Model.Name;
        textNumber.Text = "#" + _ctrl.Model.FolderNumber.ToString();
        textTags.Text = _ctrl.Model.Tags;
        textOrder.Text = _ctrl.Model.Order.ToString();
        SetOrderNotesControls(NoteOrderCriteria.Parse(_ctrl.Model.OrderNotes));
        textParentFolder.Text = (_ctrl.Model.ParentFolderDto?.Name == null) ? "(root)" : _ctrl.Model.ParentFolderDto?.Name;
        _selectedParentFolderId = _ctrl.Model.ParentId;
        _selectedParentFolder = _ctrl.Model.ParentFolderDto;
    }

    protected override void ControlsToModel()
    {
        _ctrl.Model.Name = textName.Text;
        _ctrl.Model.Tags = textTags.Text;
        int o;
        if (int.TryParse(textOrder.Text, out o))
            _ctrl.Model.Order = o;

        _ctrl.Model.OrderNotes = GetOrderNotesFromControls().Format();
        _ctrl.Model.ParentId = _selectedParentFolderId;
        _ctrl.Model.ParentFolderDto = _selectedParentFolder;
    }

    private void SetOrderNotesControls(NoteOrderCriteria criteria)
    {
        _loadedOrderCriteria = criteria;

        var colIndex = Array.FindIndex(OrderableColumns, c => c.Column == criteria.EffectiveColumn);
        comboOrderColumn.SelectedIndex = colIndex >= 0 ? colIndex : 0;
        comboOrderDirection.SelectedIndex = criteria.Ascending ? 0 : 1;

        comboOrderColumn.Enabled = criteria.Mode == NoteOrderMode.Fixed;
        comboOrderDirection.Enabled = criteria.Mode == NoteOrderMode.Fixed;

        // Setting SelectedIndex last so its change handler recomputes the info label with the
        // column/direction combos already in their final state.
        comboOrderMode.SelectedIndex = (int)criteria.Mode;
        UpdateOrderNotesInfoLabel();
    }

    private NoteOrderCriteria GetOrderNotesFromControls()
    {
        var mode = (NoteOrderMode)comboOrderMode.SelectedIndex;

        if (mode == NoteOrderMode.Fixed)
        {
            var column = OrderableColumns[comboOrderColumn.SelectedIndex].Column;
            var ascending = comboOrderDirection.SelectedIndex == 0;
            return new NoteOrderCriteria(NoteOrderMode.Fixed, column, ascending);
        }

        if (mode == NoteOrderMode.Auto)
        {
            // Preserve whatever last-used order was cached for this folder - the column/direction
            // combos are disabled (and irrelevant) while Auto is selected.
            var cachedColumn = _loadedOrderCriteria.Mode == NoteOrderMode.Auto ? _loadedOrderCriteria.Column : null;
            return new NoteOrderCriteria(NoteOrderMode.Auto, cachedColumn, _loadedOrderCriteria.Ascending);
        }

        return new NoteOrderCriteria(NoteOrderMode.Default, NoteOrderCriteria.DefaultColumn, true);
    }

    private void UpdateOrderNotesInfoLabel()
    {
        var mode = (NoteOrderMode)comboOrderMode.SelectedIndex;

        switch (mode)
        {
            case NoteOrderMode.Fixed:
                var column = OrderableColumns[comboOrderColumn.SelectedIndex].Label;
                var direction = comboOrderDirection.SelectedIndex == 0 ? "ascending" : "descending";
                labelOrderNotesInfo.Text = $"Notes in this folder will always be shown ordered by: {column} ({direction}).";
                break;

            case NoteOrderMode.Auto:
                if (_loadedOrderCriteria.Mode == NoteOrderMode.Auto && _loadedOrderCriteria.Column != null)
                {
                    var cachedLabel = OrderableColumns.FirstOrDefault(c => c.Column == _loadedOrderCriteria.Column).Label ?? _loadedOrderCriteria.Column;
                    var cachedDirection = _loadedOrderCriteria.Ascending ? "ascending" : "descending";
                    labelOrderNotesInfo.Text = $"The current order for this folder is: {cachedLabel} ({cachedDirection}) - it was updated automatically the last time you sorted this folder's notes in the selector.";
                }
                else
                {
                    labelOrderNotesInfo.Text = "This folder will follow the last order you apply to its notes in the selector. None has been set yet, so note number will be used meanwhile.";
                }
                break;

            default:
                labelOrderNotesInfo.Text = "Notes in this folder will be shown ordered by note number.";
                break;
        }
    }

    #endregion
}

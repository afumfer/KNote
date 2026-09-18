using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Utils;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

/// <summary>
/// TraceNote types tab content of the repository administration screen: a ListView of the
/// repository's trace note types plus add/delete/edit buttons ([+]/[-]/[...]). Same shape as
/// NoteTypesManageForm. Always used embedded (PanelView()) inside RepositoryEditorForm's TabPage,
/// never shown as a standalone window.
/// </summary>
public partial class TraceNoteTypesManageForm : KntForm, IViewManageList<TraceNoteTypeDto>
{
    #region Private fields

    private readonly TraceNoteTypesManageCtrl _ctrl;

    // Primary/growing column for ListViewColumnResizer - "Description" is the entity's descriptive,
    // most variable-length column; "Name" stays at its designer width.
    private const int PrimaryColumnIndex = 1;

    private ListViewColumnSorter _sorter;

    #endregion

    #region Constructor

    public TraceNoteTypesManageForm(TraceNoteTypesManageCtrl ctrl)
    {
        InitializeComponent();

        _ctrl = ctrl;

        // This form's Load event never fires: it's never Show()n/ShowDialog()d, only embedded via
        // PanelView() into RepositoryEditorForm's TabPage (panelForm gets reparented out of this
        // Form entirely). Personalizing the ListView here instead - a plain property setter, safe
        // before the control's window handle exists - is what actually makes it apply.
        ListViewStyle.ApplyStandard(listViewTraceNoteTypes);
        _sorter = ListViewSortHelper.Attach(listViewTraceNoteTypes);
    }

    #endregion

    #region IViewManageList implementation

    public Control PanelView()
    {
        return panelForm;
    }

    public override void RefreshView()
    {
        listViewTraceNoteTypes.Clear();

        listViewTraceNoteTypes.Columns.Add("Name", 200, HorizontalAlignment.Left);
        listViewTraceNoteTypes.Columns.Add("Description", -2, HorizontalAlignment.Left);

        if (_ctrl.ListEntities != null)
        {
            foreach (var item in _ctrl.ListEntities)
                listViewTraceNoteTypes.Items.Add(TraceNoteTypeDtoToListViewItem(item));
        }

        ListViewSortHelper.ApplyInitialOrder(listViewTraceNoteTypes, _sorter);

        // Reparenting into RepositoryEditorForm's TabPage doesn't reliably raise Resize the first
        // time the panel becomes visible, so size the primary column explicitly right after populating.
        ListViewColumnResizer.Resize(listViewTraceNoteTypes, PrimaryColumnIndex);
    }

    public void AddItem(TraceNoteTypeDto item)
    {
        listViewTraceNoteTypes.Items.Add(TraceNoteTypeDtoToListViewItem(item));
        ListViewSelectionHelper.SelectByKey(listViewTraceNoteTypes, item.TraceNoteTypeId.ToString(), _sorter);
    }

    public void UpdateItem(TraceNoteTypeDto item)
    {
        var listItem = listViewTraceNoteTypes.Items[item.TraceNoteTypeId.ToString()];
        if (listItem == null)
            return;

        listItem.Text = item.Name;
        listItem.SubItems[1].Text = item.Description;

        ListViewSelectionHelper.SelectByKey(listViewTraceNoteTypes, item.TraceNoteTypeId.ToString(), _sorter);
    }

    public void RemoveItem(TraceNoteTypeDto item)
    {
        listViewTraceNoteTypes.Items[item.TraceNoteTypeId.ToString()]?.Remove();
        ListViewSelectionHelper.SelectFirst(listViewTraceNoteTypes, _sorter);
    }

    #endregion

    #region Form event handlers

    private async void buttonAdd_Click(object sender, EventArgs e)
    {
        await _ctrl.AddItemAsync();
    }

    private async void buttonDelete_Click(object sender, EventArgs e)
    {
        var item = SelectedItem();
        if (item == null)
        {
            KntMessageBox.Show("There is no trace note type selected.", KntConst.AppName);
            return;
        }
        await _ctrl.DeleteItemAsync(item);
    }

    private async void buttonEdit_Click(object sender, EventArgs e)
    {
        await EditSelected();
    }

    private async void listViewTraceNoteTypes_DoubleClick(object sender, EventArgs e)
    {
        await EditSelected();
    }

    private void listViewTraceNoteTypes_Resize(object sender, EventArgs e)
    {
        ListViewColumnResizer.Resize(listViewTraceNoteTypes, PrimaryColumnIndex);
    }

    #endregion

    #region Private methods

    private async Task EditSelected()
    {
        var item = SelectedItem();
        if (item == null)
        {
            KntMessageBox.Show("There is no trace note type selected.", KntConst.AppName);
            return;
        }
        await _ctrl.EditItemAsync(item);
    }

    private TraceNoteTypeDto SelectedItem()
    {
        if (listViewTraceNoteTypes.SelectedItems.Count == 0)
            return null;

        var id = Guid.Parse(listViewTraceNoteTypes.SelectedItems[0].Name);
        return _ctrl.ListEntities?.FirstOrDefault(_ => _.TraceNoteTypeId == id);
    }

    private ListViewItem TraceNoteTypeDtoToListViewItem(TraceNoteTypeDto type)
    {
        var item = new ListViewItem(type.Name) { Name = type.TraceNoteTypeId.ToString() };
        item.SubItems.Add(type.Description);
        return item;
    }

    #endregion
}

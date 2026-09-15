using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Utils;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

/// <summary>
/// Attributes tab content of the repository administration screen: a ListView of the repository's
/// custom attributes (across all note types) plus add/delete/edit buttons ([+]/[-]/[...]), the same
/// shape as NoteTypesManageForm. Always used embedded (PanelView()) inside RepositoryEditorForm's
/// TabPage, never shown as a standalone window - so, like NoteTypesManageForm, ListView setup
/// happens in the constructor, not Load (which never fires for a form that's never Show()n).
/// </summary>
public partial class KAttributesManageForm : Form, IViewManageList<KAttributeInfoDto>
{
    #region Private fields

    private readonly KAttributesManageCtrl _ctrl;

    // Primary/growing column for ListViewColumnResizer - "Name" is the attribute's own identifying
    // column (the example the functional request itself calls out); the others stay at their
    // designer width. Columns are Note type(0), Order(1), Name(2), Data type(3), Required(4).
    private const int PrimaryColumnIndex = 2;

    private ListViewColumnSorter _sorter;

    #endregion

    #region Constructor

    public KAttributesManageForm(KAttributesManageCtrl ctrl)
    {
        InitializeComponent();

        _ctrl = ctrl;

        PersonalizeListView(listViewAttributes);

        // "Order" (column 2) sorts correctly with no custom comparer - ListViewColumnSorter's default
        // already compares numeric-looking columns numerically, not as text.
        _sorter = ListViewSortHelper.Attach(listViewAttributes);
    }

    #endregion

    #region IViewManageList implementation

    public Control PanelView()
    {
        return panelForm;
    }

    // Empty by design, not by omission: as the class doc above explains, only panelForm gets
    // reparented into RepositoryEditorForm's TabPage - this Form itself is never Show()n, so there is
    // no TopLevel/Dock/FormBorderStyle switch to make here (contrast with FoldersSelectorForm/
    // NoteEditorForm, which embed by making the whole Form non-TopLevel instead). Kept only to
    // satisfy IViewEmbeddable (pulled in via IViewManageList<T>).
    public void ConfigureEmbededMode()
    {
    }

    public void ConfigureWindowMode()
    {
    }

    public void ShowView()
    {
        this.Show();
    }

    public Result<EControllerResult> ShowModalView()
    {
        return _ctrl.DialogResultToControllerResult(this.ShowDialog());
    }

    public void OnClosingView()
    {
        this.Close();
    }

    public void RefreshView()
    {
        RebuildList();
    }

    public void AddItem(KAttributeInfoDto item)
    {
        // Rebuild (not a targeted Items.Add) so the new row lands in the right sorted position -
        // see RebuildList's use of ListViewSortHelper.ApplyInitialOrder.
        RebuildList();
        ListViewSelectionHelper.SelectByKey(listViewAttributes, item.KAttributeId.ToString(), _sorter);
    }

    public void UpdateItem(KAttributeInfoDto item)
    {
        // Rebuild rather than patching the row in place: editing Order or Note type can move this
        // item to a different position in the sorted list, not just change its own text.
        RebuildList();
        ListViewSelectionHelper.SelectByKey(listViewAttributes, item.KAttributeId.ToString(), _sorter);
    }

    public void RemoveItem(KAttributeInfoDto item)
    {
        RebuildList();
        ListViewSelectionHelper.SelectFirst(listViewAttributes, _sorter);
    }

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        return MessageBox.Show(info, caption, buttons, icon);
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
            MessageBox.Show("There is no attribute selected.", KntConst.AppName);
            return;
        }
        await _ctrl.DeleteItemAsync(item);
    }

    private async void buttonEdit_Click(object sender, EventArgs e)
    {
        await EditSelected();
    }

    private async void listViewAttributes_DoubleClick(object sender, EventArgs e)
    {
        await EditSelected();
    }

    private void listViewAttributes_Resize(object sender, EventArgs e)
    {
        ListViewColumnResizer.Resize(listViewAttributes, PrimaryColumnIndex);
    }

    #endregion

    #region Private methods

    private async Task EditSelected()
    {
        var item = SelectedItem();
        if (item == null)
        {
            MessageBox.Show("There is no attribute selected.", KntConst.AppName);
            return;
        }
        await _ctrl.EditItemAsync(item);
    }

    private KAttributeInfoDto SelectedItem()
    {
        if (listViewAttributes.SelectedItems.Count == 0)
            return null;

        var id = Guid.Parse(listViewAttributes.SelectedItems[0].Name);
        return _ctrl.ListEntities?.FirstOrDefault(_ => _.KAttributeId == id);
    }

    private void RebuildList()
    {
        listViewAttributes.Clear();

        // Only the primary column (see PrimaryColumnIndex) is ever resized dynamically - every other
        // column needs a real, fixed pixel width here: a "-2" (native auto-size) width on a column
        // nothing else manages is what used to leave "Required" collapsed and the other columns
        // shrunk, with empty space left over on the right.
        listViewAttributes.Columns.Add("Note type", 150, HorizontalAlignment.Left);
        listViewAttributes.Columns.Add("Order", 50, HorizontalAlignment.Left);
        listViewAttributes.Columns.Add("Name", 150, HorizontalAlignment.Left);
        listViewAttributes.Columns.Add("Data type", 100, HorizontalAlignment.Left);
        listViewAttributes.Columns.Add("Required", 70, HorizontalAlignment.Left);

        if (_ctrl.ListEntities != null)
        {
            foreach (var item in _ctrl.ListEntities)
                listViewAttributes.Items.Add(AttributeToListViewItem(item));
        }

        // ApplyInitialOrder gives every ListView+CRUD screen the same default (ascending by column 0,
        // then 1, then 2, ...) - here Note type, then Order, then Name, matching the column order.
        ListViewSortHelper.ApplyInitialOrder(listViewAttributes, _sorter);

        // Reparenting into RepositoryEditorForm's TabPage doesn't reliably raise Resize the first
        // time the panel becomes visible, so size the primary column explicitly right after populating.
        ListViewColumnResizer.Resize(listViewAttributes, PrimaryColumnIndex);
    }

    private ListViewItem AttributeToListViewItem(KAttributeInfoDto item)
    {
        var listItem = new ListViewItem(item.NoteTypeDto?.Name) { Name = item.KAttributeId.ToString() };
        listItem.SubItems.Add(item.Order.ToString());
        listItem.SubItems.Add(item.Name);
        listItem.SubItems.Add(KntConst.KAttributes[item.KAttributeDataType]);
        listItem.SubItems.Add(item.RequiredValueYesNo);
        return listItem;
    }

    private void PersonalizeListView(ListView listView)
    {
        listView.View = View.Details;
        listView.LabelEdit = false;
        listView.AllowColumnReorder = false;
        listView.CheckBoxes = false;
        listView.FullRowSelect = true;
        listView.GridLines = true;
        listView.Sorting = SortOrder.None;
    }

    #endregion
}

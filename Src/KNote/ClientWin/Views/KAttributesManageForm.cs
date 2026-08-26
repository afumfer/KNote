using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
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

    #endregion

    #region Constructor

    public KAttributesManageForm(KAttributesManageCtrl ctrl)
    {
        InitializeComponent();

        _ctrl = ctrl;

        PersonalizeListView(listViewAttributes);
    }

    #endregion

    #region IViewManageList implementation

    public Control PanelView()
    {
        return panelForm;
    }

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
        // see RebuildList's sort order (Note type, Order, Name), matching the column order.
        RebuildList();
    }

    public void UpdateItem(KAttributeInfoDto item)
    {
        // Rebuild rather than patching the row in place: editing Order or Note type can move this
        // item to a different position in the sorted list, not just change its own text.
        RebuildList();
    }

    public void RemoveItem(KAttributeInfoDto item)
    {
        RebuildList();
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
        SizeLastColumn(listViewAttributes);
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

        // Column order mirrors the list's own sort order (Note type, then Order, then Name) so
        // grouping by note type reads naturally left to right.
        listViewAttributes.Columns.Add("Note type", 150, HorizontalAlignment.Left);
        listViewAttributes.Columns.Add("Name", 150, HorizontalAlignment.Left);
        listViewAttributes.Columns.Add("Order", 50, HorizontalAlignment.Left);
        listViewAttributes.Columns.Add("Data type", 100, HorizontalAlignment.Left);
        listViewAttributes.Columns.Add("Required", -2, HorizontalAlignment.Left);

        if (_ctrl.ListEntities == null)
            return;

        foreach (var item in SortedEntities(_ctrl.ListEntities))
            listViewAttributes.Items.Add(AttributeToListViewItem(item));
    }

    private static IEnumerable<KAttributeInfoDto> SortedEntities(IEnumerable<KAttributeInfoDto> entities)
    {
        return entities
            .OrderBy(_ => _.NoteTypeDto?.Name, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(_ => _.Order)
            .ThenBy(_ => _.Name, StringComparer.CurrentCultureIgnoreCase);
    }

    private ListViewItem AttributeToListViewItem(KAttributeInfoDto item)
    {
        var listItem = new ListViewItem(item.NoteTypeDto?.Name) { Name = item.KAttributeId.ToString() };
        listItem.SubItems.Add(item.Name);
        listItem.SubItems.Add(item.Order.ToString());
        listItem.SubItems.Add(KntConst.KAttributes[item.KAttributeDataType]);
        listItem.SubItems.Add(item.RequiredValueYesNo);
        return listItem;
    }

    private void SizeLastColumn(ListView lv)
    {
        // Hack for control undeterminated error (same as NoteTypesManageForm).
        try
        {
            lv.Columns[lv.Columns.Count - 1].Width = -2;
        }
        catch (Exception) { }
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

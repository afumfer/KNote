using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Utils;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

/// <summary>
/// Users tab content of the repository administration screen: a ListView of the repository's users
/// plus add/delete/edit buttons ([+]/[-]/[...]), the same shape as NoteTypesManageForm/
/// KAttributesManageForm. Always used embedded (PanelView()) inside RepositoryEditorForm's TabPage,
/// never shown as a standalone window - so ListView setup happens in the constructor, not Load
/// (which never fires for a form that's never Show()n).
/// </summary>
public partial class UsersManageForm : KntForm, IViewManageList<UserDto>
{
    #region Private fields

    private readonly UsersManageCtrl _ctrl;

    // Primary/growing column for ListViewColumnResizer - "Full name" is the entity's identifying,
    // most variable-length column; the others (User name/Email/Roles) stay at their designer width.
    private const int PrimaryColumnIndex = 1;

    private ListViewColumnSorter _sorter;

    #endregion

    #region Constructor

    public UsersManageForm(UsersManageCtrl ctrl)
    {
        InitializeComponent();

        _ctrl = ctrl;

        PersonalizeListView(listViewUsers);
        _sorter = ListViewSortHelper.Attach(listViewUsers);
    }

    #endregion

    #region IViewManageList implementation

    public Control PanelView()
    {
        return panelForm;
    }

    public override void RefreshView()
    {
        listViewUsers.Clear();

        // Only the primary column (see PrimaryColumnIndex) is ever resized dynamically - every other
        // column needs a real, fixed pixel width here: a "-2" (native auto-size) width on a column
        // nothing else manages is what used to leave "Roles" collapsed to ~0px.
        listViewUsers.Columns.Add("User name", 130, HorizontalAlignment.Left);
        listViewUsers.Columns.Add("Full name", 180, HorizontalAlignment.Left);
        listViewUsers.Columns.Add("Email", 180, HorizontalAlignment.Left);
        listViewUsers.Columns.Add("Roles", 200, HorizontalAlignment.Left);

        if (_ctrl.ListEntities != null)
        {
            foreach (var item in _ctrl.ListEntities)
                listViewUsers.Items.Add(UserToListViewItem(item));
        }

        ListViewSortHelper.ApplyInitialOrder(listViewUsers, _sorter);

        // Reparenting into RepositoryEditorForm's TabPage doesn't reliably raise Resize the first
        // time the panel becomes visible, so size the primary column explicitly right after populating.
        ListViewColumnResizer.Resize(listViewUsers, PrimaryColumnIndex);
    }

    public void AddItem(UserDto item)
    {
        listViewUsers.Items.Add(UserToListViewItem(item));
        ListViewSelectionHelper.SelectByKey(listViewUsers, item.UserId.ToString(), _sorter);
    }

    public void UpdateItem(UserDto item)
    {
        var listItem = listViewUsers.Items[item.UserId.ToString()];
        if (listItem == null)
            return;

        listItem.Text = item.UserName;
        listItem.SubItems[1].Text = item.FullName;
        listItem.SubItems[2].Text = item.EMail;
        listItem.SubItems[3].Text = item.RoleDefinition;

        ListViewSelectionHelper.SelectByKey(listViewUsers, item.UserId.ToString(), _sorter);
    }

    public void RemoveItem(UserDto item)
    {
        listViewUsers.Items[item.UserId.ToString()]?.Remove();
        ListViewSelectionHelper.SelectFirst(listViewUsers, _sorter);
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
            KntMessageBox.Show("There is no user selected.", KntConst.AppName);
            return;
        }
        await _ctrl.DeleteItemAsync(item);
    }

    private async void buttonEdit_Click(object sender, EventArgs e)
    {
        await EditSelected();
    }

    private async void listViewUsers_DoubleClick(object sender, EventArgs e)
    {
        await EditSelected();
    }

    private void listViewUsers_Resize(object sender, EventArgs e)
    {
        ListViewColumnResizer.Resize(listViewUsers, PrimaryColumnIndex);
    }

    #endregion

    #region Private methods

    private async Task EditSelected()
    {
        var item = SelectedItem();
        if (item == null)
        {
            KntMessageBox.Show("There is no user selected.", KntConst.AppName);
            return;
        }
        await _ctrl.EditItemAsync(item);
    }

    private UserDto SelectedItem()
    {
        if (listViewUsers.SelectedItems.Count == 0)
            return null;

        var id = Guid.Parse(listViewUsers.SelectedItems[0].Name);
        return _ctrl.ListEntities?.FirstOrDefault(_ => _.UserId == id);
    }

    private ListViewItem UserToListViewItem(UserDto item)
    {
        var listItem = new ListViewItem(item.UserName) { Name = item.UserId.ToString() };
        listItem.SubItems.Add(item.FullName);
        listItem.SubItems.Add(item.EMail);
        listItem.SubItems.Add(item.RoleDefinition);
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

using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

public partial class UsersSelectorForm : Form, IViewEmbeddable
{
    #region Private fields

    private readonly UsersSelectorCtrl _ctrl;
    private bool _viewFinalized = false;

    #endregion

    #region Constructor

    public UsersSelectorForm(UsersSelectorCtrl ctrl)
    {
        InitializeComponent();

        _ctrl = ctrl;
    }

    #endregion

    #region ISelectorView interface

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
        _viewFinalized = true;
        this.Close();
    }

    public void RefreshView()
    {
        if (_ctrl.ListEntities == null)
            return;
        else
        {
            listViewUsers.Clear();

            foreach (var user in _ctrl.ListEntities)
            {
                var item = UserDtoToListViewItem(user);
                listViewUsers.Items.Add(item);
                if (_ctrl.SelectedEntity != null && user.UserId == _ctrl.SelectedEntity.UserId)
                    item.Selected = true;
            }

            listViewUsers.Columns.Add("User name", 100, HorizontalAlignment.Left);
            listViewUsers.Columns.Add("Full name", 160, HorizontalAlignment.Left);
            listViewUsers.Columns.Add("Email", 160, HorizontalAlignment.Left);
        }
    }

    private ListViewItem UserDtoToListViewItem(UserDto user)
    {
        var itemList = new ListViewItem(user.UserName);
        itemList.Name = user.UserId.ToString();
        itemList.SubItems.Add(user.FullName?.ToString());
        itemList.SubItems.Add(user.EMail?.ToString());
        return itemList;
    }

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        return MessageBox.Show(info, caption, buttons, icon);
    }

    public Control PanelView()
    {
        return panelForm;
    }

    // UsersSelectorCtrl only ever runs in window mode (a modal RunModal() picker, e.g. from
    // MessageEditorCtrl.SelectUser); EmbededMode is never set true for it, so ConfigureEmbededMode()
    // never actually executes. ConfigureWindowMode() does run on every open but has nothing to
    // configure here. Kept empty only to satisfy IViewEmbeddable.
    public void ConfigureEmbededMode()
    {
    }

    public void ConfigureWindowMode()
    {
    }

    #endregion

    #region Form events handlers

    private void UsersSelectorForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!_viewFinalized)
            _ctrl.Finalize();
    }

    private void buttonAccept_Click(object sender, EventArgs e)
    {
        _ctrl.Accept();
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
        _ctrl.Cancel();
    }

    private void listViewUsers_SelectedIndexChanged(object sender, EventArgs e)
    {
        OnSelectedItemChanged();
    }

    private void listViewUsers_Resize(object sender, EventArgs e)
    {
        SizeLastColumn((ListView)sender);
    }

    private void UsersSelectorForm_Load(object sender, EventArgs e)
    {
        PersonalizeListView(listViewUsers);
    }

    #endregion

    #region Private methods

    private void SizeLastColumn(ListView lv)
    {
        // Hack for control undeterminated error
        try
        {
            lv.Columns[lv.Columns.Count - 1].Width = -2;
        }
        catch (Exception) { }
    }

    private void OnSelectedItemChanged()
    {
        try
        {
            if (_ctrl.ListEntities == null)
                return;

            if (listViewUsers.SelectedItems.Count > 0)
            {
                var selectedItem = Guid.Parse(listViewUsers.SelectedItems[0].Name);
                _ctrl.SelectedEntity = _ctrl.ListEntities.Where(_ => _.UserId == selectedItem).SingleOrDefault();
                _ctrl.NotifySelectedEntity();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"OnSelectedItemChanged error: {ex.Message}");
        }
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

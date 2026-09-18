using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.ClientWin.Utils;

namespace KNote.ClientWin.Views;

public partial class UsersSelectorForm : KntForm, IViewEmbeddable
{
    #region Private fields

    private readonly UsersSelectorCtrl _ctrl;

    // Primary/growing column for ListViewColumnResizer - the last one, which used to be stretched
    // with the old "Width = -2" hack.
    private const int PrimaryColumnIndex = 2;

    #endregion

    #region Constructor

    public UsersSelectorForm(UsersSelectorCtrl ctrl)
    {
        InitializeComponent();

        _ctrl = ctrl;
    }

    #endregion

    #region ISelectorView interface

    public override void RefreshView()
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

    public Control PanelView()
    {
        return panelForm;
    }

    #endregion

    #region Form events handlers

    protected override void OnUserClosing(FormClosingEventArgs e)
        => _ctrl.Finalize();

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
        ListViewColumnResizer.Resize(listViewUsers, PrimaryColumnIndex);
    }

    private void UsersSelectorForm_Load(object sender, EventArgs e)
    {
        ListViewStyle.ApplyStandard(listViewUsers);
    }

    #endregion

    #region Private methods

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
            KntMessageBox.Show($"OnSelectedItemChanged error: {ex.Message}");
        }
    }

    #endregion
}

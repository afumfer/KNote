using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Views;

/// <summary>
/// KNoteAIAssistant plan (Phase 4): standalone "Manage AI providers" window, opened directly from
/// the Tools menu (not embedded in a tab, unlike NoteTypesManageForm/UsersManageForm which live
/// inside RepositoryEditorForm). ConfigureWindowMode()/ConfigureEmbededMode() are still
/// implemented (the IViewEmbeddable contract), but only window mode is actually exercised today.
/// </summary>
public partial class AiProvidersManageForm : Form, IViewManageList<AiProviderRef>
{
    #region Private fields

    private readonly AiProvidersManageCtrl _ctrl;

    #endregion

    #region Constructor

    public AiProvidersManageForm(AiProvidersManageCtrl ctrl)
    {
        InitializeComponent();

        _ctrl = ctrl;

        PersonalizeListView(listViewProviders);
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
        StartPosition = FormStartPosition.CenterParent;
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
        listViewProviders.Clear();

        listViewProviders.Columns.Add("Alias", 160, HorizontalAlignment.Left);
        listViewProviders.Columns.Add("Provider", 100, HorizontalAlignment.Left);
        listViewProviders.Columns.Add("Model", -2, HorizontalAlignment.Left);

        if (_ctrl.ListEntities == null)
            return;

        foreach (var item in _ctrl.ListEntities)
            listViewProviders.Items.Add(AiProviderRefToListViewItem(item));
    }

    // The list is small and Alias (the ListViewItem key) can change on edit, so every mutation
    // just rebuilds the whole view instead of patching a single row in place.
    public void AddItem(AiProviderRef item) => RefreshView();

    public void UpdateItem(AiProviderRef item) => RefreshView();

    public void RemoveItem(AiProviderRef item) => RefreshView();

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
            MessageBox.Show("There is no AI provider selected.", KntConst.AppName);
            return;
        }
        await _ctrl.DeleteItemAsync(item);
    }

    private async void buttonEdit_Click(object sender, EventArgs e)
    {
        await EditSelected();
    }

    private async void listViewProviders_DoubleClick(object sender, EventArgs e)
    {
        await EditSelected();
    }

    private void listViewProviders_Resize(object sender, EventArgs e)
    {
        SizeLastColumn(listViewProviders);
    }

    #endregion

    #region Private methods

    private async Task EditSelected()
    {
        var item = SelectedItem();
        if (item == null)
        {
            MessageBox.Show("There is no AI provider selected.", KntConst.AppName);
            return;
        }
        await _ctrl.EditItemAsync(item);
    }

    private AiProviderRef SelectedItem()
    {
        if (listViewProviders.SelectedItems.Count == 0)
            return null;

        var alias = listViewProviders.SelectedItems[0].Name;
        return _ctrl.ListEntities?.FirstOrDefault(_ => _.Alias == alias);
    }

    private ListViewItem AiProviderRefToListViewItem(AiProviderRef providerRef)
    {
        var item = new ListViewItem(providerRef.Alias) { Name = providerRef.Alias };
        item.SubItems.Add(providerRef.Provider);
        item.SubItems.Add(providerRef.Model);
        return item;
    }

    private void SizeLastColumn(ListView lv)
    {
        // Hack for control undeterminated error (same as NoteTypesManageForm/NoteTypesSelectorForm).
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

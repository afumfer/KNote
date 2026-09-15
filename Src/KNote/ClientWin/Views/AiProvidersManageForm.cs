using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Utils;
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

    // Primary/growing column for ListViewColumnResizer - "Model" is the most variable-length column
    // (model ids can be long); "Alias"/"Provider" stay at their designer width.
    private const int PrimaryColumnIndex = 2;

    private ListViewColumnSorter _sorter;

    #endregion

    #region Constructor

    public AiProvidersManageForm(AiProvidersManageCtrl ctrl)
    {
        InitializeComponent();

        _ctrl = ctrl;

        PersonalizeListView(listViewProviders);
        _sorter = ListViewSortHelper.Attach(listViewProviders);
    }

    #endregion

    #region IViewManageList implementation

    public Control PanelView()
    {
        return panelForm;
    }

    // AiProvidersManageCtrl only ever runs in window mode (Tools menu -> RunModal()); EmbededMode is
    // never set true for it, so ConfigureEmbededMode() never actually executes. ConfigureWindowMode()
    // does run on every open but has nothing to configure here. Kept empty only to satisfy
    // IViewEmbeddable (pulled in via IViewManageList<T>).
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

        if (_ctrl.ListEntities != null)
        {
            foreach (var item in _ctrl.ListEntities)
                listViewProviders.Items.Add(AiProviderRefToListViewItem(item));
        }

        ListViewSortHelper.ApplyInitialOrder(listViewProviders, _sorter);

        // Reparenting into RepositoryEditorForm's TabPage doesn't reliably raise Resize the first
        // time the panel becomes visible, so size the primary column explicitly right after populating.
        ListViewColumnResizer.Resize(listViewProviders, PrimaryColumnIndex);
    }

    // The list is small and Alias (the ListViewItem key) can change on edit, so every mutation
    // just rebuilds the whole view instead of patching a single row in place.
    public void AddItem(AiProviderRef item)
    {
        RefreshView();
        ListViewSelectionHelper.SelectByKey(listViewProviders, item.Alias, _sorter);
    }

    public void UpdateItem(AiProviderRef item)
    {
        RefreshView();
        ListViewSelectionHelper.SelectByKey(listViewProviders, item.Alias, _sorter);
    }

    public void RemoveItem(AiProviderRef item)
    {
        RefreshView();
        ListViewSelectionHelper.SelectFirst(listViewProviders, _sorter);
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
        ListViewColumnResizer.Resize(listViewProviders, PrimaryColumnIndex);
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

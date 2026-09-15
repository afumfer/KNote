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
public partial class TraceNoteTypesManageForm : Form, IViewManageList<TraceNoteTypeDto>
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
        PersonalizeListView(listViewTraceNoteTypes);
        _sorter = ListViewSortHelper.Attach(listViewTraceNoteTypes);
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
            MessageBox.Show("There is no trace note type selected.", KntConst.AppName);
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
            MessageBox.Show("There is no trace note type selected.", KntConst.AppName);
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

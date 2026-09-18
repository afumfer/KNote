using System.Data;

using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.ClientWin.Utils;

namespace KNote.ClientWin.Views;

public partial class NoteTypesSelectorForm : KntForm, IViewEmbeddable
{
    #region Private fields

    private readonly NoteTypesSelectorCtrl _ctrl;
    private bool _viewFinalized = false;

    #endregion

    #region Constructor

    public NoteTypesSelectorForm(NoteTypesSelectorCtrl ctrl)
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
            listViewNoteTypes.Clear();

            foreach (var type in _ctrl.ListEntities)
            {
                listViewNoteTypes.Items.Add(NoteTypeDtoToListViewItem(type));
            }
            
            listViewNoteTypes.Columns.Add("Name", 120, HorizontalAlignment.Left);
            listViewNoteTypes.Columns.Add("Description", 240, HorizontalAlignment.Left);
        }
    }

    private ListViewItem NoteTypeDtoToListViewItem(NoteTypeDto type)
    {
        var itemList = new ListViewItem(type.Name);
        itemList.Name = type.NoteTypeId.ToString();
        itemList.SubItems.Add(type.Description?.ToString());
        return itemList;            
    }

    public Control PanelView()
    {
        return panelForm;
    }

    // NoteTypesSelectorCtrl only ever runs in window mode (a modal RunModal() picker, e.g. from
    // NoteEditorCtrl.RequestChangeNoteType); EmbededMode is never set true for it, so
    // ConfigureEmbededMode() never actually executes. ConfigureWindowMode() does run on every open
    // but has nothing to configure here. Kept empty only to satisfy IViewEmbeddable.
    public void ConfigureEmbededMode()
    {
    }

    public void ConfigureWindowMode()
    {
    }

    #endregion

    #region Form events handlers

    private void NoteTypesSelectorForm_FormClosing(object sender, FormClosingEventArgs e)
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

    private void listViewNoteTypes_SelectedIndexChanged(object sender, EventArgs e)
    {
        OnSelectedItemChanged();
    }

    private void listViewNoteTypes_Resize(object sender, EventArgs e)
    {
        SizeLastColumn((ListView)sender);
    }

    private void NoteTypesSelectorForm_Load(object sender, EventArgs e)
    {
        PersonalizeListView(listViewNoteTypes);
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

            if (listViewNoteTypes.SelectedItems.Count > 0)
            {                
                var selectedItem = Guid.Parse(listViewNoteTypes.SelectedItems[0].Name);
                _ctrl.SelectedEntity = _ctrl.ListEntities.Where(_ => _.NoteTypeId == selectedItem).SingleOrDefault();
                _ctrl.NotifySelectedEntity();
            }
        }
        catch (Exception ex)
        {
            KntMessageBox.Show($"OnSelectedItemChanged error: {ex.Message}");
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

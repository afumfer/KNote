using System.Data;

using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model.Dto;
using KNote.ClientWin.Utils;

namespace KNote.ClientWin.Views;

public partial class NoteTypesSelectorForm : KntForm, IViewEmbeddable
{
    #region Private fields

    private readonly NoteTypesSelectorCtrl _ctrl;

    // Primary/growing column for ListViewColumnResizer - the last one, which used to be stretched
    // with the old "Width = -2" hack.
    private const int PrimaryColumnIndex = 1;

    #endregion

    #region Constructor

    public NoteTypesSelectorForm(NoteTypesSelectorCtrl ctrl)
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

    private void listViewNoteTypes_SelectedIndexChanged(object sender, EventArgs e)
    {
        OnSelectedItemChanged();
    }

    private void listViewNoteTypes_Resize(object sender, EventArgs e)
    {
        ListViewColumnResizer.Resize(listViewNoteTypes, PrimaryColumnIndex);
    }

    private void NoteTypesSelectorForm_Load(object sender, EventArgs e)
    {
        ListViewStyle.ApplyStandard(listViewNoteTypes);
    }

    #endregion

    #region Private methods

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

    #endregion 
}

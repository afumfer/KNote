using System.Data;

using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

public partial class NotesSelectorForm : Form, IViewSelector<NoteInfoDto>
{
    #region Private fields 

    private readonly NotesSelectorCtrl _ctrl;
    private bool _viewFinalized = false;        
    private UInt32 _countRepetition = 0;
    private bool _skipSelectionChanged = false;        
    private BindingSource _source = new BindingSource();   
    private SortOrder _sortOrder;

    #endregion

    #region Protected properties

    protected int OrderColNumber
    {
        get { return _ctrl.Store.AppConfig.ColOrderNotes; }
        set { _ctrl.Store.AppConfig.ColOrderNotes = value; }
    }

    protected bool AscendigOrderNotes
    {
        get { return _ctrl.Store.AppConfig.AscendigOrderNotes; }
        set { _ctrl.Store.AppConfig.AscendigOrderNotes = value; }
    }

    #endregion

    #region Constructor

    public NotesSelectorForm(NotesSelectorCtrl ctrl)
    {
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();

        _ctrl = ctrl;        
    }

    #endregion 

    #region ISelectorView interface 

    public Control PanelView()
    {
        return panelForm;
    }

    public void ShowView()
    {
        this.Show();
    }

    Result<EComponentResult> IViewBase.ShowModalView()
    {
        return _ctrl.DialogResultToComponentResult(this.ShowDialog());
    }

    public void RefreshView()
    {
        if (_ctrl.ListEntities == null)
            return;

        CoonfigureGridStd();

        if (OrderColNumber == 0)
        {
            OrderColNumber = 1;
            AscendigOrderNotes = true;                    
        }                
        _sortOrder = getDefaultSortOrder();

        RefreshDataGridNotes();

        // Hack for refresh column 0 in modal form.
        dataGridNotes.Columns[0].Visible = false;
    }

    public void OnClosingView()
    {
        _viewFinalized = true;
        this.Close();
    }

    public void ConfigureEmbededMode()
    {
        TopLevel = false;
        Dock = DockStyle.Fill;
        FormBorderStyle = FormBorderStyle.None;
        panelBottom.Visible = false;
        panelDataGridNotes.Dock = DockStyle.Fill;
        dataGridNotes.BorderStyle = BorderStyle.None;
        dataGridNotes.Dock = DockStyle.Fill;        
    }

    public void ConfigureWindowMode()
    {
        TopLevel = true;
        Dock = DockStyle.None;
        FormBorderStyle = FormBorderStyle.Sizable;
        panelBottom.Visible = true;
        StartPosition = FormStartPosition.CenterScreen;
        dataGridNotes.Dock = DockStyle.Fill;        
    }

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        return MessageBox.Show(info, caption, buttons, icon);
    }

    #region Extension methods ...

    public object SelectItem(NoteInfoDto item)
    {
        throw new NotImplementedException();
    }

    public void AddItem(NoteInfoDto item)
    {
        // In this case item is not used, the update is resolved with databindig 
        RefreshDataGridNotes();

        int index = 0;
        if (_ctrl.SelectedEntity != null)
        {
            foreach (DataGridViewRow r in dataGridNotes.Rows)
            {
                if (_ctrl.SelectedEntity.NoteId == (Guid)r.Cells["NoteId"].Value)
                {
                    index = r.Index;
                    break;
                }
            }
        }
        dataGridNotes.ClearSelection();
        dataGridNotes.Rows[index].Selected = true;
    }

    public void DeleteItem(NoteInfoDto item)
    {
        // In this case item is not used, the update is resolved with databindig 
        RefreshDataGridNotes();

        if (_ctrl.ListEntities.Count == 0)
            return;

        GridSelectFirstElement(false);
    }

    public void RefreshItem(NoteInfoDto item)
    {
        dataGridNotes.Refresh();
    }

    #endregion

    #endregion

    #region Form events handlers 

    private void NotesSelectorForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!_viewFinalized)
            _ctrl.Finalize();
    }

    private void dataGridNotes_SelectionChanged(object sender, EventArgs e)
    {
        OnSelectedNoteItemChanged();
    }

    private void dataGridNotes_DoubleClick(object sender, EventArgs e)
    {
        ActiveCurrentRow(false);
        _ctrl.NotifySelectedEntityDoubleClick();
    }

    private void dataGridNotes_KeyUp(object sender, KeyEventArgs e)
    {
        _countRepetition = 0;            
    }

    private void dataGridNotes_KeyDown(object sender, KeyEventArgs e)
    {
        _countRepetition++;
    }

    private void dataGridNotes_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
    {
        OrderColNumber = e.ColumnIndex;
        _sortOrder = getSortOrder(OrderColNumber);            
        RefreshDataGridNotes();
    }

    private void buttonAccept_Click(object sender, EventArgs e)
    {
        _ctrl.Accept();
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
        _ctrl.Cancel();
    }

    #endregion

    #region Private methods

    private void RefreshDataGridNotes()
    {
        _skipSelectionChanged = true;
        
        if (_sortOrder == SortOrder.Descending)
            _source.DataSource = _ctrl.ListEntities.OrderByDescending(o => o.GetType().GetProperty(dataGridNotes.Columns[OrderColNumber].Name).GetValue(o));
        else if (_sortOrder == SortOrder.Ascending)
            _source.DataSource = _ctrl.ListEntities.OrderBy(o => o.GetType().GetProperty(dataGridNotes.Columns[OrderColNumber].Name).GetValue(o));

        // DataSource has changed, so we need to refresh the grid definition
        CoonfigureGridStd();

        dataGridNotes.Columns[OrderColNumber].HeaderCell.SortGlyphDirection = _sortOrder;

        if (_ctrl.ListEntities.Count > 0)            
            ActiveCurrentRow();
            
        _skipSelectionChanged = false;            
    }

    private void OnSelectedNoteItemChanged()
    {        
        if (_skipSelectionChanged || _countRepetition > 5)
            return;
        try
        {
            if (_ctrl.ListEntities == null)
                return;            
            if (dataGridNotes.SelectedRows.Count > 0)                                    
                ActiveCurrentRow();
                
        }
        catch (Exception ex)
        {
            MessageBox.Show($"OnSelectedNoteItemChanged error: {ex.Message}");
        }
    }

    private void ActiveCurrentRow(bool notifySelectedEntity = true)
    {
        var sr = dataGridNotes.SelectedRows[0];
        _ctrl.SelectedEntity = DataGridViewRowToNoteInfo(sr);
        if(notifySelectedEntity)            
            _ctrl.NotifySelectedEntity();
    }

    private void CoonfigureGridStd()
    {
        if (dataGridNotes.Columns.Count > 1)
            return;

        _source.DataSource = new List<NoteInfoDto>();
        dataGridNotes.DataSource = _source;

        dataGridNotes.Columns[0].DataPropertyName = "NoteId";       
        dataGridNotes.Columns[0].Visible = false;
        
        dataGridNotes.Columns[1].DataPropertyName = "NoteNumber";
        dataGridNotes.Columns[1].Width = 80;
        dataGridNotes.Columns[1].HeaderText = "Number";
        dataGridNotes.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        if(_ctrl.Store.AppConfig.CompactViewNoteslist || _ctrl.HiddenColumns.Contains("NoteNumber"))
            dataGridNotes.Columns[1].Visible = false;

        dataGridNotes.Columns[2].DataPropertyName = "Topic";
        dataGridNotes.Columns[2].Width = 450;
        dataGridNotes.Columns[2].HeaderText = "Topic";

        dataGridNotes.Columns[3].DataPropertyName = "Priority";
        dataGridNotes.Columns[3].Width = 70;
        dataGridNotes.Columns[3].HeaderText = "Priority";
        dataGridNotes.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        if (_ctrl.HiddenColumns.Contains("Priority"))
            dataGridNotes.Columns[3].Visible = false;

        dataGridNotes.Columns[4].DataPropertyName = "Tags";
        dataGridNotes.Columns[4].Width = 160;
        dataGridNotes.Columns[4].HeaderText = "Tags";
        if (_ctrl.HiddenColumns.Contains("Tags"))
            dataGridNotes.Columns[4].Visible = false;

        dataGridNotes.Columns[5].DataPropertyName = "InternalTags";
        dataGridNotes.Columns[5].Width = 150;
        dataGridNotes.Columns[5].HeaderText = "Status";
        if (_ctrl.HiddenColumns.Contains("InternalTags"))
            dataGridNotes.Columns[5].Visible = false;

        dataGridNotes.Columns[6].DataPropertyName = "ModificationDateTime";
        dataGridNotes.Columns[6].Width = 130;
        dataGridNotes.Columns[6].HeaderText = "Modification date";
        dataGridNotes.Columns[6].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        if (_ctrl.Store.AppConfig.CompactViewNoteslist || _ctrl.HiddenColumns.Contains("ModificationDateTime"))
            dataGridNotes.Columns[6].Visible = false;

        dataGridNotes.Columns[7].DataPropertyName = "CreationDateTime";
        dataGridNotes.Columns[7].Width = 130;
        dataGridNotes.Columns[7].HeaderText = "Creation date";
        dataGridNotes.Columns[7].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        if (_ctrl.Store.AppConfig.CompactViewNoteslist || _ctrl.HiddenColumns.Contains("CreationDateTime"))
            dataGridNotes.Columns[7].Visible = false;

        dataGridNotes.Columns[8].DataPropertyName = "Description";
        dataGridNotes.Columns[8].Visible = false;            

        dataGridNotes.Columns[9].DataPropertyName = "ContentType";
        if (_ctrl.Store.AppConfig.CompactViewNoteslist || _ctrl.HiddenColumns.Contains("ContentType"))
            dataGridNotes.Columns[9].Visible = false;

        dataGridNotes.Columns[10].DataPropertyName = "Script";
        dataGridNotes.Columns[10].Visible = false;
            
        dataGridNotes.Columns[11].DataPropertyName = "FolderId";
        dataGridNotes.Columns[11].Visible = false;
            
        dataGridNotes.Columns[12].DataPropertyName = "NoteTypeId";
        dataGridNotes.Columns[12].Visible = false;
    }

    private NoteInfoDto DataGridViewRowToNoteInfo(DataGridViewRow dgr)
    {            
        if (dgr == null)
            return null;
        else
        {
            var n = new NoteInfoDto();

            n.NoteId = (Guid)dgr.Cells["NoteId"].Value;
            n.NoteNumber = (int)dgr.Cells["NoteNumber"].Value;
            n.Topic = (string)dgr.Cells["Topic"].Value;
            n.Priority = (int)dgr.Cells["Priority"].Value;
            n.Tags = (string)dgr.Cells["Tags"].Value;                
            n.ModificationDateTime = (DateTime)dgr.Cells["ModificationDateTime"].Value;
            n.CreationDateTime = (DateTime)dgr.Cells["CreationDateTime"].Value;
            n.Description = (string)dgr.Cells["Description"].Value;
            n.ContentType = (string)dgr.Cells["ContentType"].Value;
            n.Script = (string)dgr.Cells["Script"].Value;
            n.InternalTags = (string)dgr.Cells["InternalTags"].Value;                                
            n.FolderId = (Guid)dgr.Cells["FolderId"].Value;
            if (dgr.Cells[12].Value != null)
                n.NoteTypeId = (Guid)dgr.Cells["NoteTypeId"].Value;
            else
                n.NoteTypeId = null;
            return n;
        }
    }

    public List<NoteInfoDto> GetSelectedListItem()
    {
        var listNoteInfo = new List<NoteInfoDto>();

        foreach(var dg in dataGridNotes.SelectedRows)            
            listNoteInfo.Add(DataGridViewRowToNoteInfo((DataGridViewRow)dg));
            
        return listNoteInfo;
    }

    private void GridSelectFirstElement(bool skipSelectionChanged = true)
    {
        _skipSelectionChanged = skipSelectionChanged;
        dataGridNotes.ClearSelection();         
        dataGridNotes.Rows[0].Selected = true;
        _skipSelectionChanged = false;           
    }

    private SortOrder getDefaultSortOrder()
    {
        if (AscendigOrderNotes)
        {
            dataGridNotes.Columns[OrderColNumber].HeaderCell.SortGlyphDirection = SortOrder.Ascending;                
            return SortOrder.Ascending;
        }
        else
        {
            dataGridNotes.Columns[OrderColNumber].HeaderCell.SortGlyphDirection = SortOrder.Descending;                
            return SortOrder.Descending;
        }
    }

    private SortOrder getSortOrder(int columnIndex)
    {
        if (dataGridNotes.Columns[columnIndex].HeaderCell.SortGlyphDirection == SortOrder.None ||
            dataGridNotes.Columns[columnIndex].HeaderCell.SortGlyphDirection == SortOrder.Descending)
        {
            dataGridNotes.Columns[columnIndex].HeaderCell.SortGlyphDirection = SortOrder.Ascending;
            AscendigOrderNotes = true;
            return SortOrder.Ascending;
        }
        else
        {
            dataGridNotes.Columns[columnIndex].HeaderCell.SortGlyphDirection = SortOrder.Descending;
            AscendigOrderNotes = false;
            return SortOrder.Descending;
        }
    }

    #endregion

    #region Extensions

    // TODO: Esto es más código repetido, hay que pasar a una clase base 
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        this.SuspendLayout();

        if (_ctrl.Extensions.Keys.Count > 0)
            foreach (string s in _ctrl.Extensions.Keys)
                if (s.StartsWith("--"))
                    contextMenu.Items.Add("-", null, extension_Click);
                else
                    contextMenu.Items.Add(s, null, extension_Click);

        this.ResumeLayout();
    }

    private void extension_Click(object sender, EventArgs e)
    {
        ToolStripMenuItem menuSel;
        menuSel = (ToolStripMenuItem)sender;

        _ctrl.Extensions[menuSel.Text](this, new ComponentEventArgs<NoteInfoDto>(_ctrl.SelectedEntity));
    }

    #endregion
}

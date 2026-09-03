using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using System.Data;

namespace KNote.ClientWin.Views;

public partial class NotesSelectorForm : Form, IViewSelector<NoteMinimalDto>
{
    #region Private fields 

    private readonly NotesSelectorCtrl _ctrl;
    private bool _viewFinalized = false;        
    private UInt32 _countRepetition = 0;
    private bool _skipSelectionChanged = false;        
    private BindingSource _source = new BindingSource();
    private SortOrder _sortOrder;
    private string _textFilter = "";

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
        InitializeComponent();

        _ctrl = ctrl;

        SetUndoFilterButtonIcon();
    }

    // Resources\undo_16.png embedded as a resource (KNote.ClientWin.KNote.ClientWin.csproj) rather
    // than wired through the Designer's .resx machinery, since this button is built by hand here,
    // not via the Forms Designer. Falls back to "X" if the resource can't be found/loaded, so a
    // packaging mistake degrades gracefully instead of leaving the button unlabeled.
    private void SetUndoFilterButtonIcon()
    {
        try
        {
            using var iconStream = System.Reflection.Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("KNote.ClientWin.Resources.undo_16.png");
            if (iconStream != null)
            {
                buttonUndoFilter.Image = Image.FromStream(iconStream);
                buttonUndoFilter.Text = "";
                return;
            }
        }
        catch (Exception)
        {
            // fall through to the text fallback below
        }
        buttonUndoFilter.Text = "X";
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

    Result<EControllerResult> IViewBase.ShowModalView()
    {
        return _ctrl.DialogResultToControllerResult(this.ShowDialog());
    }

    public void RefreshView()
    {
        if (!string.IsNullOrEmpty(_ctrl.ViewTitle))
            this.Text = _ctrl.ViewTitle;

        if (_ctrl.ListEntities == null)
            return;

        // A fresh load (this is only reached from NotesSelectorCtrl's Load*Entities, not from
        // sort/AddItem/DeleteItem refreshes) starts from the caller's collection - any leftover
        // second filter from a previous open would otherwise silently hide rows.
        panelTextFilter.Visible = _ctrl.EnableTextFilter;
        _textFilter = "";
        textFilter.Text = "";

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
        panelDataGridNotes.Padding = new Padding(0);
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
        panelDataGridNotes.Dock = DockStyle.Fill;
        panelDataGridNotes.Padding = new Padding(3); // independent/modal selector only
        dataGridNotes.Dock = DockStyle.Fill;
    }

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        return MessageBox.Show(info, caption, buttons, icon);
    }

    #region Extension methods ...

    public object SelectItem(NoteMinimalDto item)
    {
        throw new NotImplementedException();
    }

    public void AddItem(NoteMinimalDto item)
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

    public void DeleteItem(NoteMinimalDto item)
    {
        // In this case item is not used, the update is resolved with databindig 
        RefreshDataGridNotes();

        if (_ctrl.ListEntities.Count == 0)
            return;

        GridSelectFirstElement(false);
    }

    public void RefreshItem(NoteMinimalDto item)
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

    private void textFilter_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
            return;

        e.SuppressKeyPress = true; // avoid the "ding" (Enter is not a normal input char here)
        _textFilter = textFilter.Text;
        RefreshDataGridNotes();
    }

    private void buttonUndoFilter_Click(object sender, EventArgs e)
    {
        textFilter.Text = "";
        _textFilter = "";
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

        var entities = GetFilteredEntities();

        if (_sortOrder == SortOrder.Descending)
            _source.DataSource = entities.OrderByDescending(o => o.GetType().GetProperty(dataGridNotes.Columns[OrderColNumber].Name).GetValue(o));
        else if (_sortOrder == SortOrder.Ascending)
            _source.DataSource = entities.OrderBy(o => o.GetType().GetProperty(dataGridNotes.Columns[OrderColNumber].Name).GetValue(o));

        // DataSource has changed, so we need to refresh the grid definition
        CoonfigureGridStd();

        dataGridNotes.Columns[OrderColNumber].HeaderCell.SortGlyphDirection = _sortOrder;

        // Checks the grid's actual row count, not _ctrl.ListEntities.Count: with a second filter
        // applied, the two can differ, and ActiveCurrentRow() would throw on an empty grid.
        if (dataGridNotes.Rows.Count > 0)
            ActiveCurrentRow();

        _skipSelectionChanged = false;
    }

    // Second, in-memory filter over the already-loaded ListEntities (NotesSelectorCtrl.EnableTextFilter) -
    // never re-queries the repository. A leading '#' followed by digits matches NoteNumber exactly;
    // anything else is matched as a case-insensitive substring of Topic or Tags.
    private IEnumerable<NoteMinimalDto> GetFilteredEntities()
    {
        if (string.IsNullOrWhiteSpace(_textFilter))
            return _ctrl.ListEntities;

        var filter = _textFilter.Trim();

        if (filter.StartsWith('#') && int.TryParse(filter.AsSpan(1), out var noteNumber))
            return _ctrl.ListEntities.Where(n => n.NoteNumber == noteNumber);

        return _ctrl.ListEntities.Where(n =>
            (n.Topic?.Contains(filter, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (n.Tags?.Contains(filter, StringComparison.OrdinalIgnoreCase) ?? false));
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

        _source.DataSource = new List<NoteMinimalDto>();
        dataGridNotes.DataSource = _source;

        // ColumnHeadersHeightSizeMode=AutoSize (Designer) grows the header row's height to fit
        // wrapped text instead of growing the column's width - so headers wrap to two lines
        // whenever the text doesn't fit, regardless of AutoSizeMode on the columns below.
        dataGridNotes.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;

        dataGridNotes.Columns[0].DataPropertyName = "NoteId";
        dataGridNotes.Columns[0].Visible = false;
        
        dataGridNotes.Columns[1].DataPropertyName = "NoteNumber";
        dataGridNotes.Columns[1].Width = 80; // room for the sort glyph next to right-aligned numbers
        dataGridNotes.Columns[1].HeaderText = "Number";        
        dataGridNotes.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        if(_ctrl.Store.AppConfig.CompactViewNoteslist || IsColumnHidden("NoteNumber"))
            dataGridNotes.Columns[1].Visible = false;

        dataGridNotes.Columns[2].DataPropertyName = "Topic";
        dataGridNotes.Columns[2].MinimumWidth = 380;

        if(_ctrl.EmbededMode == false) // ### Hack for selector view
            dataGridNotes.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        dataGridNotes.Columns[2].Resizable = DataGridViewTriState.True;
        dataGridNotes.Columns[2].HeaderText = "Topic";        

        dataGridNotes.Columns[3].DataPropertyName = "Priority";
        dataGridNotes.Columns[3].Width = 70;
        dataGridNotes.Columns[3].HeaderText = "Priority";
        dataGridNotes.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        if (IsColumnHidden("Priority"))
            dataGridNotes.Columns[3].Visible = false;

        dataGridNotes.Columns[4].DataPropertyName = "Tags";
        dataGridNotes.Columns[4].Width = 140;
        dataGridNotes.Columns[4].HeaderText = "Tags";
        if (IsColumnHidden("Tags"))
            dataGridNotes.Columns[4].Visible = false;

        dataGridNotes.Columns[5].DataPropertyName = "InternalTags";
        dataGridNotes.Columns[5].Width = 150;
        dataGridNotes.Columns[5].HeaderText = "Status";
        if (IsColumnHidden("InternalTags"))
            dataGridNotes.Columns[5].Visible = false;

        dataGridNotes.Columns[6].DataPropertyName = "ModificationDateTime";
        dataGridNotes.Columns[6].Width = 160;
        dataGridNotes.Columns[6].HeaderText = "Modification date";
        dataGridNotes.Columns[6].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        // Fixed pixel widths don't grow with the header font at higher Windows scale factors
        // (DataGridView column Width isn't rescaled by AutoScaleMode like a Control's own bounds
        // are), so the header text was wrapping to two lines at 150%+. AllCells sizes the column
        // to whatever the header/cell text actually needs at the current DPI, so it never wraps.
        dataGridNotes.Columns[6].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        if (_ctrl.Store.AppConfig.CompactViewNoteslist || IsColumnHidden("ModificationDateTime"))
            dataGridNotes.Columns[6].Visible = false;

        dataGridNotes.Columns[7].DataPropertyName = "CreationDateTime";
        dataGridNotes.Columns[7].Width = 150;
        dataGridNotes.Columns[7].HeaderText = "Creation date";
        dataGridNotes.Columns[7].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        dataGridNotes.Columns[7].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        if (_ctrl.Store.AppConfig.CompactViewNoteslist || IsColumnHidden("CreationDateTime"))
            dataGridNotes.Columns[7].Visible = false;

        dataGridNotes.Columns[8].DataPropertyName = "FolderId";
        dataGridNotes.Columns[8].Visible = false;

        foreach (DataGridViewColumn col in dataGridNotes.Columns)
        {
            if (col.Name != "Topic")
            {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            }
        }
    }

    // _ctrl.HiddenColumns is a free-form comma-separated string (e.g. "Priority, InternalTags,
    // ModificationDateTime"); a plain HiddenColumns.Contains("Tags") also matches inside
    // "InternalTags", incorrectly hiding the Tags column whenever InternalTags is hidden. Tokenizes
    // and compares whole entries instead.
    private bool IsColumnHidden(string columnName)
    {
        return _ctrl.HiddenColumns
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Contains(columnName, StringComparer.OrdinalIgnoreCase);
    }

    private NoteMinimalDto DataGridViewRowToNoteInfo(DataGridViewRow dgr)
    {            
        if (dgr == null)
            return null;
        else
        {
            var n = new NoteMinimalDto();

            n.NoteId = (Guid)dgr.Cells["NoteId"].Value;
            n.NoteNumber = (int)dgr.Cells["NoteNumber"].Value;
            n.Topic = (string)dgr.Cells["Topic"].Value;
            n.Priority = (int)dgr.Cells["Priority"].Value;
            n.Tags = (string)dgr.Cells["Tags"].Value;                
            n.ModificationDateTime = (DateTime)dgr.Cells["ModificationDateTime"].Value;
            n.CreationDateTime = (DateTime)dgr.Cells["CreationDateTime"].Value;
            n.InternalTags = (string)dgr.Cells["InternalTags"].Value;                                
            n.FolderId = (Guid)dgr.Cells["FolderId"].Value;
            return n;
        }
    }

    public List<NoteMinimalDto> GetSelectedListItem()
    {
        var listNoteInfo = new List<NoteMinimalDto>();

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

        _ctrl.Extensions[menuSel.Text](this, new ControllerEventArgs<NoteMinimalDto>(_ctrl.SelectedEntity));
    }

    #endregion
}

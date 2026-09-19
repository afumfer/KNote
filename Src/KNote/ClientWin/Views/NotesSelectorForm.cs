using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model.Core;
using KNote.Model.Dto;
using System.Data;
using KNote.ClientWin.Utils;

namespace KNote.ClientWin.Views;

public partial class NotesSelectorForm : KntForm, IViewSelector<NoteMinimalDto>
{
    #region Private fields 

    private readonly NotesSelectorCtrl _ctrl;
    private UInt32 _countRepetition = 0;
    private bool _skipSelectionChanged = false;        
    private BindingSource _source = new BindingSource();
    private SortOrder _sortOrder;
    private string _textFilter = "";
    private int _topicPreferredWidth;      // width the user gave to Topic by dragging its header edge
    private bool _applyingColumnWidths;    // true while the code (not the user) is changing column widths
    private bool _columnWidthsTracked;     // user resizes are only recorded once the grid is fully set up
    private bool _dateColumnsFitted;
    // Widths persisted in AppConfig.NotesListColumnWidths - only used (read and written) when embedded.
    private Dictionary<string, int> _savedColumnWidths = new();

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

        dataGridNotes.SizeChanged += (s, e) => FitTopicColumn();
        dataGridNotes.ColumnWidthChanged += dataGridNotes_ColumnWidthChanged;
        dataGridNotes.DataBindingComplete += dataGridNotes_DataBindingComplete;

        SetUndoFilterButtonIcon();
    }

    // Resources\Icons\undo_16.png embedded as a resource (KNote.ClientWin.KNote.ClientWin.csproj) rather
    // than wired through the Designer's .resx machinery, since this button is built by hand here,
    // not via the Forms Designer. Falls back to "X" if the resource can't be found/loaded, so a
    // packaging mistake degrades gracefully instead of leaving the button unlabeled.
    private void SetUndoFilterButtonIcon()
    {
        try
        {
            using var iconStream = System.Reflection.Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("KNote.ClientWin.Resources.Icons.undo_16.png");
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

    public override void RefreshView()
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

        // Folder mode (a specific folder selected in the tree) applies that folder's own default
        // order every time it's opened, taking precedence over the global last-clicked-column state
        // used by Filters/Search mode. Those other modes are untouched (Folder == null there).
        if (_ctrl.Folder != null)
        {
            var criteria = NoteOrderCriteria.Parse(_ctrl.Folder.OrderNotes);
            var colIndex = FindColumnIndex(criteria.EffectiveColumn);
            if (colIndex < 0)
                colIndex = FindColumnIndex(NoteOrderCriteria.DefaultColumn);

            OrderColNumber = colIndex >= 0 ? colIndex : 1;
            AscendigOrderNotes = criteria.Ascending;
        }
        else if (OrderColNumber == 0)
        {
            OrderColNumber = 1;
            AscendigOrderNotes = true;
        }
        _sortOrder = getDefaultSortOrder();

        RefreshDataGridNotes();

        // Hack for refresh column 0 in modal form.
        dataGridNotes.Columns[0].Visible = false;
    }

    public override void ConfigureEmbededMode()
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

    public override void ConfigureWindowMode()
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

    protected override void OnUserClosing(FormClosingEventArgs e)
        => _ctrl.Finalize();

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

        // Only in Folder mode, and only when the folder is set to "follow my last order in the
        // selector" (Auto), does a header click get persisted back to Folder.OrderNotes. Fixed and
        // Default folders, and Filters/Search mode (Folder == null), only get the visual re-sort above.
        if (_ctrl.Folder != null && NoteOrderCriteria.Parse(_ctrl.Folder.OrderNotes).Mode == NoteOrderMode.Auto)
        {
            var columnName = dataGridNotes.Columns[OrderColNumber].Name;
            var newCriteria = new NoteOrderCriteria(NoteOrderMode.Auto, columnName, _sortOrder == SortOrder.Ascending);
            _ = _ctrl.PersistFolderOrderNotesAsync(newCriteria.Format());
        }
    }

    // Only user-driven changes matter here: the code's own adjustments (fill of Topic, fit of the date
    // columns, restore of saved widths) are wrapped in _applyingColumnWidths.
    private void dataGridNotes_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
    {
        if (_applyingColumnWidths || !_columnWidthsTracked)
            return;

        if (e.Column.Name == "Topic")
            _topicPreferredWidth = e.Column.Width;

        if (_ctrl.EmbededMode && e.Column.Visible)
            SaveColumnWidth(e.Column);

        // Other columns changed size: Topic absorbs the difference (it is refitted itself only on
        // grid resize / data binding, so a width the user just gave it is left alone).
        if (e.Column.Name != "Topic")
            FitTopicColumn();
    }

    private void SaveColumnWidth(DataGridViewColumn column)
    {
        _savedColumnWidths[column.Name] = column.Width;
        _ctrl.Store.AppConfig.NotesListColumnWidths = ColumnWidthSettings.Format(_savedColumnWidths);
    }

    // Rows (and so the vertical scrollbar) only exist once the data is bound, hence Topic is refitted here too.
    private void dataGridNotes_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
    {
        FitDateColumns();
        FitTopicColumn();
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

        // Setting SortGlyphDirection here doesn't reliably paint the glyph the very first time a
        // folder's notes are shown: at that point dataGridNotes is still mid-layout (this Ctrl's
        // OnInitialized() runs before KNoteManagmentForm.LinkComponents() has finished docking/
        // resizing this grid's panel into its final container), so the header paints once more with
        // the sort state it had before this assignment. The data itself sorts correctly regardless -
        // only the glyph is affected. Reapplying it once the message queue goes idle - i.e. once all
        // of that startup layout work has actually finished - fixes the first-time-only glyph.
        var colAtIdle = OrderColNumber;
        var orderAtIdle = _sortOrder;
        EventHandler onIdle = null;
        onIdle = (s, e) =>
        {
            Application.Idle -= onIdle;
            if (!dataGridNotes.IsDisposed && colAtIdle < dataGridNotes.Columns.Count)
            {
                dataGridNotes.Columns[colAtIdle].HeaderCell.SortGlyphDirection = orderAtIdle;
                dataGridNotes.Refresh();
            }
        };
        Application.Idle += onIdle;

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
            KntMessageBox.Show($"OnSelectedNoteItemChanged error: {ex.Message}");
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

        // Not AutoSizeMode.Fill: that would forbid the user from widening Topic. Instead FitTopicColumn
        // stretches it over any free space on the right whenever the grid is resized.
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
        // Widths of the two date columns are fitted to their content once data is loaded
        // (FitDateColumns); a fixed pixel width doesn't follow the font/DPI and wastes space.
        if (_ctrl.Store.AppConfig.CompactViewNoteslist || IsColumnHidden("ModificationDateTime"))
            dataGridNotes.Columns[6].Visible = false;

        dataGridNotes.Columns[7].DataPropertyName = "CreationDateTime";
        dataGridNotes.Columns[7].Width = 150;
        dataGridNotes.Columns[7].HeaderText = "Creation date";
        dataGridNotes.Columns[7].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
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

        if (_ctrl.EmbededMode)
            ApplySavedColumnWidths();

        FitTopicColumn();
        _columnWidthsTracked = true;
    }

    // Restores the widths the user left in the embedded list. Hidden columns are skipped (their saved
    // entry is kept in _savedColumnWidths, so it comes back if they are shown again).
    private void ApplySavedColumnWidths()
    {
        _savedColumnWidths = ColumnWidthSettings.Parse(_ctrl.Store.AppConfig.NotesListColumnWidths);

        _applyingColumnWidths = true;
        try
        {
            foreach (DataGridViewColumn col in dataGridNotes.Columns)
                if (col.Visible && _savedColumnWidths.TryGetValue(col.Name, out var width))
                    col.Width = Math.Max(width, col.MinimumWidth);
        }
        finally { _applyingColumnWidths = false; }

        if (_savedColumnWidths.TryGetValue("Topic", out var topicWidth))
            _topicPreferredWidth = topicWidth;
    }

    // One-off: sizes the date columns to their header/cell text (both wrap-free), so they take no more
    // room than needed. Done once so it doesn't undo a width the user later sets by hand.
    private void FitDateColumns()
    {
        if (_dateColumnsFitted || dataGridNotes.Rows.Count == 0)
            return;

        _applyingColumnWidths = true;
        try
        {
            foreach (var name in new[] { "ModificationDateTime", "CreationDateTime" })
            {
                var col = dataGridNotes.Columns[name];
                if (col.Visible && !_savedColumnWidths.ContainsKey(name))
                    dataGridNotes.AutoResizeColumn(col.Index, DataGridViewAutoSizeColumnMode.AllCells);
            }
        }
        finally { _applyingColumnWidths = false; }
        _dateColumnsFitted = true;
    }

    // Topic gets the widest of: its minimum, the width the user chose, or whatever space the other
    // (fixed-width) visible columns leave free, so no empty area shows on the right.
    private void FitTopicColumn()
    {
        if (dataGridNotes.Columns.Count < 3 || dataGridNotes.ClientSize.Width <= 0)
            return;

        var topic = dataGridNotes.Columns["Topic"];
        var othersWidth = dataGridNotes.Columns.Cast<DataGridViewColumn>()
            .Where(c => c != topic && c.Visible)
            .Sum(c => c.Width);

        var rowsHeight = dataGridNotes.Rows.GetRowsHeight(DataGridViewElementStates.Visible)
            + dataGridNotes.ColumnHeadersHeight;
        var vScrollWidth = rowsHeight > dataGridNotes.ClientSize.Height ? SystemInformation.VerticalScrollBarWidth : 0;

        // The trailing "- 1" is needed: DataGridView shows the horizontal scrollbar as soon as the
        // columns' total width equals (not just exceeds) the available width.
        var free = dataGridNotes.ClientSize.Width - othersWidth - vScrollWidth - 1;
        var width = Math.Max(topic.MinimumWidth, Math.Max(_topicPreferredWidth, free));

        if (topic.Width == width)
            return;

        _applyingColumnWidths = true;
        try { topic.Width = width; }
        finally { _applyingColumnWidths = false; }
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

    private int FindColumnIndex(string columnName)
    {
        for (int i = 0; i < dataGridNotes.Columns.Count; i++)
            if (dataGridNotes.Columns[i].Name == columnName)
                return i;
        return -1;
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

    // Well-known extension label recognized below to keep its context-menu item's Checked state in
    // sync with _ctrl.EnableTextFilter - the generic Extensions mechanism only supports fire-and-forget
    // clicks, so this is the one entry that also needs its checkmark refreshed each time the menu opens.
    public const string ToggleTextFilterMenuText = "Show list filter";

    private ToolStripMenuItem _menuToggleTextFilter;

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
                {
                    var item = (ToolStripMenuItem)contextMenu.Items.Add(s, null, extension_Click);
                    if (s == ToggleTextFilterMenuText)
                        _menuToggleTextFilter = item;
                }

        if (_menuToggleTextFilter != null)
            contextMenu.Opening += contextMenu_Opening;

        this.ResumeLayout();
    }

    private void contextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
    {
        _menuToggleTextFilter.Checked = _ctrl.EnableTextFilter;
    }

    private void extension_Click(object sender, EventArgs e)
    {
        ToolStripMenuItem menuSel;
        menuSel = (ToolStripMenuItem)sender;

        _ctrl.Extensions[menuSel.Text](this, new ControllerEventArgs<NoteMinimalDto>(_ctrl.SelectedEntity));
    }

    #endregion
}

using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model.Core;
using KNote.Model.Dto;
using KNote.Reports;
using System.Data;
using KNote.ClientWin.Utils;
using KNote.Model;
using KntIcons;

namespace KNote.ClientWin.Views;

public partial class NotesSelectorForm : KntForm, IViewNotesSelector
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
    // Widths persisted in State.ManagementWindow.NotesList.ColumnWidths - only used (read and written) when embedded.
    private Dictionary<string, int> _savedColumnWidths = new();
    // KNote Atlas: the columns of a list found by meaning (not bound: their values come from NoteSearchHitDto, see
    // dataGridNotes_CellFormatting) and the notice of the notes not up to date, with «Update index».
    private const string SimilarityColumn = "Similarity";
    private const string FragmentColumn = "Fragment";
    private Panel _panelMeaningNotice;
    private Label _labelMeaningNotice;
    private Button _buttonUpdateIndex;
    // KNote Atlas, stage 4 (D14): the photos or the documents found by meaning, a row for each one (ResourceSearchHitDto), with
    // their thumbnail (photos), title and page, «Gallery» (the photos in a window, ResourceGalleryForm) and opening the resource.
    private const string ThumbnailColumn = "Thumbnail";
    private const string ResourceColumn = "Resource";
    private const string PageColumn = "Page";
    private const int ThumbnailSide = 48;
    private Button _buttonGallery;
    private ResourceGalleryForm _gallery;
    private readonly Dictionary<Guid, Image> _thumbnails = new();
    private int _thumbnailsVersion;
    private ToolStripMenuItem _menuOpenResource;
    private ToolStripMenuItem _menuOpenNote;
    private ToolStripSeparator _menuResourceSeparator;

    #endregion

    #region Protected properties

    protected int OrderColNumber
    {
        get { return _ctrl.Store.State.ManagementWindow.NotesList.SortColumn; }
        set { _ctrl.Store.State.ManagementWindow.NotesList.SortColumn = value; }
    }

    protected bool AscendigOrderNotes
    {
        get { return _ctrl.Store.State.ManagementWindow.NotesList.SortAscending; }
        set { _ctrl.Store.State.ManagementWindow.NotesList.SortAscending = value; }
    }

    #endregion

    #region Constructor

    public NotesSelectorForm(NotesSelectorCtrl ctrl)
    {
        InitializeComponent();

        _ctrl = ctrl;

        DataGridViewStyle.ApplyDarkIfNeeded(dataGridNotes);
        dataGridNotes.SizeChanged += (s, e) => FitTopicColumn();
        dataGridNotes.ColumnWidthChanged += dataGridNotes_ColumnWidthChanged;
        dataGridNotes.DataBindingComplete += dataGridNotes_DataBindingComplete;
        _ctrl.Store.Events.Subscribe<NotesListViewOptionsChanged>(OnNotesListViewOptionsChanged);
        _ctrl.Store.Events.Subscribe<SemanticIndexProgressChanged>(OnIndexProgressChanged);

        buttonUndoFilter.SetKntIcon(KntIcon.FilterClear);
        buttonUndoFilter.AlignToTextBox(textFilter);

        dataGridNotes.CellFormatting += dataGridNotes_CellFormatting;
        CreateMeaningNotice();
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
        ApplyColumnVisibility();
        FitTopicColumn();

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
        else if (_ctrl.ByMeaning)
        {
            // Found by meaning: the most similar first (the user can sort by another column afterwards).
            OrderColNumber = FindColumnIndex(SimilarityColumn);
            AscendigOrderNotes = false;
        }
        else if (OrderColNumber == 0 || IsMeaningColumn(OrderColNumber) || IsResourceColumn(OrderColNumber))
        {
            OrderColNumber = 1;
            AscendigOrderNotes = true;
        }
        _sortOrder = getDefaultSortOrder();

        // Still the note selected before this load: reloading the same list (Refresh notes list, tab switch,
        // after moving notes...) keeps it selected; a different list doesn't contain it and starts at row 0.
        RefreshDataGridNotes(_ctrl.SelectedEntity?.NoteId);

        RefreshMeaningNotice();
        RefreshResources();

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
        // A photo or a document found: the resource (its note, with «Open note»).
        if (_ctrl.ByResources && SelectedHit() is ResourceSearchHitDto hit)
        {
            OpenResource(hit);
            return;
        }
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
        if (IsResourceColumn(e.ColumnIndex))
            return;
        OrderColNumber = e.ColumnIndex;
        _sortOrder = getSortOrder(OrderColNumber);
        RefreshDataGridNotes();

        // Only in Folder mode, and only when the folder is set to "follow my last order in the
        // selector" (Auto), does a header click get persisted back to Folder.OrderNotes. Fixed and
        // Default folders, and Filters/Search mode (Folder == null), only get the visual re-sort above.
        if (_ctrl.Folder != null)
        {
            var columnName = dataGridNotes.Columns[OrderColNumber].Name;
            var orderNotes = NoteOrderCriteria.Parse(_ctrl.Folder.OrderNotes).FormatUserOrder(columnName, _sortOrder == SortOrder.Ascending);
            if (orderNotes != null)
                _ = _ctrl.PersistFolderOrderNotesAsync(orderNotes);
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
        _ctrl.Store.State.ManagementWindow.NotesList.ColumnWidths = ColumnWidthSettings.Format(_savedColumnWidths);
    }

    // Rows (and so the vertical scrollbar) only exist once the data is bound, hence Topic is refitted here too.
    private void dataGridNotes_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
    {
        FitDateColumns();
        FitTopicColumn();
        // The photos found: rows as high as their thumbnail.
        if (_ctrl.ResourceKind == EnumResourceKind.Photos)
            foreach (DataGridViewRow row in dataGridNotes.Rows)
                row.Height = LogicalToDeviceUnits(ThumbnailSide + 4);
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

    private void RefreshDataGridNotes(Guid? noteIdToSelect = null)
    {
        _skipSelectionChanged = true;

        // Columns first: the sort key below reads them, and configuring binds an empty list (it would
        // replace the data if it ran afterwards).
        CoonfigureGridStd();

        var entities = GetFilteredEntities();

        // Always a List<NoteMinimalDto>, never a lazy LINQ query: from an *empty* IEnumerable the
        // BindingSource can't tell the item type, and the grid then replaces all its configured columns
        // with a single "Current" one (FitDateColumns etc. would no longer find them).
        // The key read from each note's own type: Similarity and Fragment only exist in the notes found by meaning
        // (NoteSearchHitDto; another note, e.g. one just added to such a list, has none).
        var orderColumn = dataGridNotes.Columns[OrderColNumber].Name;
        if (_sortOrder == SortOrder.Descending)
            _source.DataSource = entities.OrderByDescending(o => o.GetType().GetProperty(orderColumn)?.GetValue(o)).ToList();
        else if (_sortOrder == SortOrder.Ascending)
            _source.DataSource = entities.OrderBy(o => o.GetType().GetProperty(orderColumn)?.GetValue(o)).ToList();

        dataGridNotes.Columns[OrderColNumber].HeaderCell.SortGlyphDirection = _sortOrder;

        // Setting SortGlyphDirection here doesn't reliably paint the glyph the very first time a
        // folder's notes are shown: at that point dataGridNotes is still mid-layout (this Ctrl's
        // OnInitialized() runs before KNoteManagementForm.LinkComponents() has finished docking/
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
        {
            if (noteIdToSelect != null)
                SelectRow(noteIdToSelect.Value);
            ActiveCurrentRow();
        }

        _skipSelectionChanged = false;
    }

    // Selects (and scrolls to) the row of a note, if it is in the grid.
    private void SelectRow(Guid noteId)
    {
        var row = dataGridNotes.Rows.Cast<DataGridViewRow>().FirstOrDefault(r => (Guid)r.Cells["NoteId"].Value == noteId);
        var firstVisibleColumn = dataGridNotes.Columns.GetFirstColumn(DataGridViewElementStates.Visible);
        if (row == null || firstVisibleColumn == null)
            return;

        dataGridNotes.CurrentCell = row.Cells[firstVisibleColumn.Index];
        dataGridNotes.ClearSelection();
        row.Selected = true;
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
        // The gallery shows the same photo selected.
        if (_gallery is { IsDisposed: false } && BoundItem(sr.Index) is ResourceSearchHitDto hit)
            _gallery.SelectHit(hit);
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
        dataGridNotes.Columns[1].Width = LogicalToDeviceUnits(80); // room for the sort glyph next to right-aligned numbers
        dataGridNotes.Columns[1].HeaderText = "Number";        
        dataGridNotes.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

        dataGridNotes.Columns[2].DataPropertyName = "Topic";
        dataGridNotes.Columns[2].MinimumWidth = LogicalToDeviceUnits(380);

        // Not AutoSizeMode.Fill: that would forbid the user from widening Topic. Instead FitTopicColumn
        // stretches it over any free space on the right whenever the grid is resized.
        dataGridNotes.Columns[2].Resizable = DataGridViewTriState.True;
        dataGridNotes.Columns[2].HeaderText = "Topic";        

        dataGridNotes.Columns[3].DataPropertyName = "Priority";
        dataGridNotes.Columns[3].Width = LogicalToDeviceUnits(70);
        dataGridNotes.Columns[3].HeaderText = "Priority";
        dataGridNotes.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

        dataGridNotes.Columns[4].DataPropertyName = "Tags";
        dataGridNotes.Columns[4].Width = LogicalToDeviceUnits(140);
        dataGridNotes.Columns[4].HeaderText = "Tags";

        dataGridNotes.Columns[5].DataPropertyName = "InternalTags";
        dataGridNotes.Columns[5].Width = LogicalToDeviceUnits(150);
        dataGridNotes.Columns[5].HeaderText = "Status";

        dataGridNotes.Columns[6].DataPropertyName = "ModificationDateTime";
        dataGridNotes.Columns[6].Width = LogicalToDeviceUnits(160);
        dataGridNotes.Columns[6].HeaderText = "Modification date";
        dataGridNotes.Columns[6].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        // Widths of the two date columns are fitted to their content once data is loaded
        // (FitDateColumns); a fixed pixel width doesn't follow the font/DPI and wastes space.

        dataGridNotes.Columns[7].DataPropertyName = "CreationDateTime";
        dataGridNotes.Columns[7].Width = LogicalToDeviceUnits(150);
        dataGridNotes.Columns[7].HeaderText = "Creation date";
        dataGridNotes.Columns[7].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

        dataGridNotes.Columns[8].DataPropertyName = "FolderId";
        dataGridNotes.Columns[8].Visible = false;

        // The columns stay as configured from now on: with AutoGenerateColumns, binding another list generated them
        // again after the unbound ones below, and their indexes (OrderColNumber, Columns[0]...) moved.
        dataGridNotes.AutoGenerateColumns = false;

        // Sorted by the code, as the others (Programmatic: the grid cannot sort an unbound column of a bound list).
        var similarity = new DataGridViewTextBoxColumn
        {
            Name = SimilarityColumn, HeaderText = "Similarity", Width = LogicalToDeviceUnits(80),
            SortMode = DataGridViewColumnSortMode.Programmatic, Visible = false
        };
        similarity.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        dataGridNotes.Columns.Add(similarity);
        similarity.DisplayIndex = dataGridNotes.Columns["Topic"].DisplayIndex + 1;
        dataGridNotes.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = FragmentColumn, HeaderText = "Fragment", Width = LogicalToDeviceUnits(360),
            SortMode = DataGridViewColumnSortMode.Programmatic, Visible = false
        });

        // KNote Atlas, stage 4 (D14): the resource of each row of a search of photos or documents (not sorted by their header).
        var thumbnail = new DataGridViewImageColumn
        {
            Name = ThumbnailColumn, HeaderText = "", Width = LogicalToDeviceUnits(ThumbnailSide + 8),
            ImageLayout = DataGridViewImageCellLayout.Zoom, SortMode = DataGridViewColumnSortMode.NotSortable, Visible = false
        };
        thumbnail.DefaultCellStyle.NullValue = null;
        dataGridNotes.Columns.Add(thumbnail);
        thumbnail.DisplayIndex = dataGridNotes.Columns["NoteNumber"].DisplayIndex;
        var resourceColumn = new DataGridViewTextBoxColumn
        {
            Name = ResourceColumn, HeaderText = "Resource", Width = LogicalToDeviceUnits(220),
            SortMode = DataGridViewColumnSortMode.NotSortable, Visible = false
        };
        dataGridNotes.Columns.Add(resourceColumn);
        resourceColumn.DisplayIndex = dataGridNotes.Columns["Topic"].DisplayIndex;
        var page = new DataGridViewTextBoxColumn
        {
            Name = PageColumn, HeaderText = "Page", Width = LogicalToDeviceUnits(50),
            SortMode = DataGridViewColumnSortMode.NotSortable, Visible = false
        };
        page.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        dataGridNotes.Columns.Add(page);
        page.DisplayIndex = resourceColumn.DisplayIndex + 1;

        foreach (DataGridViewColumn col in dataGridNotes.Columns)
        {
            if (col.Name != "Topic")
            {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            }
        }

        ApplyColumnVisibility();

        if (_ctrl.EmbededMode)
            ApplySavedColumnWidths();

        FitTopicColumn();
        _columnWidthsTracked = true;
    }

    // Which columns are shown: the "Compact view in notes list" option (Number and both dates) plus the
    // caller's HiddenColumns. Also re-run when that option changes, see OnNotesListViewOptionsChanged.
    private void ApplyColumnVisibility()
    {
        var compact = _ctrl.Store.State.ManagementWindow.NotesList.CompactView;

        dataGridNotes.Columns["NoteNumber"].Visible = !(compact || IsColumnHidden("NoteNumber"));
        dataGridNotes.Columns["Priority"].Visible = !IsColumnHidden("Priority");
        dataGridNotes.Columns["Tags"].Visible = !IsColumnHidden("Tags");
        dataGridNotes.Columns["InternalTags"].Visible = !IsColumnHidden("InternalTags");
        dataGridNotes.Columns["ModificationDateTime"].Visible = !(compact || IsColumnHidden("ModificationDateTime"));
        dataGridNotes.Columns["CreationDateTime"].Visible = !(compact || IsColumnHidden("CreationDateTime"));
        dataGridNotes.Columns[SimilarityColumn].Visible = _ctrl.ByMeaning;
        dataGridNotes.Columns[FragmentColumn].Visible = _ctrl.ByMeaning;
        dataGridNotes.Columns[ThumbnailColumn].Visible = _ctrl.ResourceKind == EnumResourceKind.Photos;
        dataGridNotes.Columns[ResourceColumn].Visible = _ctrl.ByResources;
        dataGridNotes.Columns[PageColumn].Visible = _ctrl.ResourceKind == EnumResourceKind.Documents;
    }

    private bool IsMeaningColumn(int columnIndex) =>
        columnIndex >= 0 && columnIndex < dataGridNotes.Columns.Count
        && dataGridNotes.Columns[columnIndex].Name is SimilarityColumn or FragmentColumn;

    private bool IsResourceColumn(int columnIndex) =>
        columnIndex >= 0 && columnIndex < dataGridNotes.Columns.Count
        && dataGridNotes.Columns[columnIndex].Name is ThumbnailColumn or ResourceColumn or PageColumn;

    private void dataGridNotes_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0)
            return;
        if (IsResourceColumn(e.ColumnIndex))
        {
            var found = BoundItem(e.RowIndex) as ResourceSearchHitDto;
            e.Value = dataGridNotes.Columns[e.ColumnIndex].Name switch
            {
                ThumbnailColumn => found == null ? null : _thumbnails.GetValueOrDefault(found.Resource.ResourceId),
                ResourceColumn => found?.Resource.GetTitle() ?? "",
                _ => found?.FragmentPage?.ToString() ?? ""
            };
            e.FormattingApplied = true;
            return;
        }
        if (!IsMeaningColumn(e.ColumnIndex))
            return;
        var hit = BoundItem(e.RowIndex) as NoteSearchHitDto;
        // A list of resources: the fragment alone (its resource and its page have their columns).
        e.Value = hit == null ? "" : dataGridNotes.Columns[e.ColumnIndex].Name == SimilarityColumn ? hit.Similarity.ToString("0.00")
            : hit is ResourceSearchHitDto ? hit.Fragment : hit.FragmentShown();
        e.FormattingApplied = true;
    }

    // The notice above the list: the notes not up to date (4.6b: why, and «Update index» for an Admin, enabled when no run of KNote
    // Atlas is going on) and, with photos found, how many and «Gallery». The notes whose documents have no file here don't show it.
    private void RefreshMeaningNotice()
    {
        var pending = _ctrl.ByMeaning && SemanticIndexTexts.ShowsPendingNotice(_ctrl.PendingNotes, _ctrl.NotesWithoutFiles);
        var photos = _ctrl.ResourceKind == EnumResourceKind.Photos;
        var count = _ctrl.ResourceHits.Count();
        var canRun = _ctrl.CanRunIndex;
        _labelMeaningNotice.Text = pending
            ? SemanticIndexTexts.PendingInSearch(_ctrl.PendingNotes, _ctrl.NotesForFullRun, _ctrl.NotesWithoutFiles, canRun)
                + (_ctrl.IndexRunning ? " The index is being brought up to date: the search is repeated when it ends." : "")
            : photos ? $"{ResourceProcessingTexts.Photos(count)} found." : "";
        _buttonUpdateIndex.Visible = pending && canRun;
        _buttonUpdateIndex.Enabled = !_ctrl.Store.AtlasJobRunning;
        _buttonGallery.Visible = photos && count > 0;
        _panelMeaningNotice.Visible = pending || photos;
    }

    // A run of the index starts or ends: «Update index» is enabled again, and the notice says it.
    private void OnIndexProgressChanged(SemanticIndexProgressChanged message)
    {
        if (!IsDisposed && message.Job.Progress?.IsFinished != false)
            RefreshMeaningNotice();
    }

    // A list of resources: their thumbnails, one after another (the list may change meanwhile: a new version stops it), and the
    // gallery, if it is open, with the same photos.
    private void RefreshResources()
    {
        var version = ++_thumbnailsVersion;
        if (_ctrl.ResourceKind == EnumResourceKind.Photos)
            _ = LoadThumbnailsAsync(version);
        if (_gallery is { IsDisposed: false })
        {
            if (_ctrl.ResourceKind == EnumResourceKind.Photos)
                _gallery.Reload();
            else
                _gallery.Close();
        }
    }

    private async Task LoadThumbnailsAsync(int version)
    {
        foreach (var hit in _ctrl.ResourceHits.ToList())
        {
            if (version != _thumbnailsVersion || IsDisposed)
                return;
            if (_thumbnails.ContainsKey(hit.Resource.ResourceId))
                continue;
            var bytes = await _ctrl.GetThumbnailAsync(hit, LogicalToDeviceUnits(ThumbnailSide) * 2);
            if (version != _thumbnailsVersion || IsDisposed || bytes == null)
                continue;
            using var stream = new MemoryStream(bytes);
            _thumbnails[hit.Resource.ResourceId] = Image.FromStream(stream);
            dataGridNotes.InvalidateColumn(dataGridNotes.Columns[ThumbnailColumn].Index);
        }
    }

    private ResourceSearchHitDto SelectedHit() =>
        dataGridNotes.SelectedRows.Count > 0 ? BoundItem(dataGridNotes.SelectedRows[0].Index) as ResourceSearchHitDto : null;

    // The note (or resource found) of a row, read from the list bound. While the list is being replaced by a shorter one, the grid
    // still formats the current cell of the old one (a row that is not there any more): DataGridViewRow.DataBoundItem threw then.
    private object BoundItem(int rowIndex) =>
        rowIndex >= 0 && _source.List is { } list && rowIndex < list.Count ? list[rowIndex] : null;

    /// <summary>Selects the row of a resource found (from the gallery).</summary>
    public void SelectHit(ResourceSearchHitDto hit)
    {
        var row = dataGridNotes.Rows.Cast<DataGridViewRow>().FirstOrDefault(r => BoundItem(r.Index) == hit);
        var firstVisibleColumn = dataGridNotes.Columns.GetFirstColumn(DataGridViewElementStates.Visible);
        if (row == null || firstVisibleColumn == null || row.Selected)
            return;
        dataGridNotes.CurrentCell = row.Cells[firstVisibleColumn.Index];
        dataGridNotes.ClearSelection();
        row.Selected = true;
    }

    /// <summary>
    /// Opens a resource found: a photo or a PDF (at the page of its fragment) in a window of the app, another document with its
    /// program.
    /// </summary>
    public void OpenResource(ResourceSearchHitDto hit)
    {
        var (url, inApp) = _ctrl.ResourceToOpen(hit);
        if (url == null)
        {
            ShowInfo(AiDocumentRules.FileNotAvailableMessage, KntConst.AppName);
            return;
        }
        try
        {
            if (inApp)
                new DocumentPageForm(url, hit.FragmentPage is int page ? $"{hit.Resource.GetTitle()} - {DocumentTexts.PageHeading(page)}" : hit.Resource.GetTitle())
                    .Show(FindForm());
            else
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            ShowInfo(ex.Message, KntConst.AppName);
        }
    }

    /// <summary>Opens the note of a resource found, as a double click on a note does (from the gallery too).</summary>
    public void OpenNote(ResourceSearchHitDto hit)
    {
        SelectHit(hit);
        if (dataGridNotes.SelectedRows.Count == 0)
            return;
        ActiveCurrentRow(false);
        _ctrl.NotifySelectedEntityDoubleClick();
    }

    private void ShowGallery()
    {
        if (_gallery is { IsDisposed: false })
        {
            _gallery.Activate();
            return;
        }
        _gallery = new ResourceGalleryForm(_ctrl, this);
        _gallery.Show(FindForm());
        if (SelectedHit() is ResourceSearchHitDto hit)
            _gallery.SelectHit(hit);
    }

    // Above the list (and its text filter): the notice, and «Update index», which searches again.
    private void CreateMeaningNotice()
    {
        _labelMeaningNotice = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true };
        // «Update index» (4.6b): the full indexing in the background (its progress in the status bar), and the search again when it ends.
        var buttonUpdateIndex = _buttonUpdateIndex = NoticeButton(SemanticIndexTexts.UpdateIndex, KntIcon.Refresh, SemanticIndexTexts.UpdateIndexHelp);
        buttonUpdateIndex.Click += (s, e) =>
        {
            _ctrl.RunFullIndex();
            RefreshMeaningNotice();
        };
        _buttonGallery = NoticeButton("Gallery", KntIcon.Image, "The photos found, with big thumbnails, in a window");
        _buttonGallery.Visible = false;
        _buttonGallery.Click += (s, e) => ShowGallery();
        _panelMeaningNotice = new Panel
        {
            Dock = DockStyle.Top, Height = LogicalToDeviceUnits(40), Visible = false,
            Padding = new Padding(LogicalToDeviceUnits(4), LogicalToDeviceUnits(2), LogicalToDeviceUnits(4), LogicalToDeviceUnits(2))
        };
        // Docked right: the last one added is the rightmost (z-order); the label fills what is left.
        _panelMeaningNotice.Controls.Add(_labelMeaningNotice);
        _panelMeaningNotice.Controls.Add(_buttonGallery);
        _panelMeaningNotice.Controls.Add(new Panel { Dock = DockStyle.Right, Width = LogicalToDeviceUnits(6) });
        _panelMeaningNotice.Controls.Add(buttonUpdateIndex);
        panelForm.Controls.Add(_panelMeaningNotice);
    }

    // A button of the notice, with its icon before its text.
    private Button NoticeButton(string text, KntIcon icon, string tooltip)
    {
        var button = new Button
        {
            Dock = DockStyle.Right, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Text = text,
            TextImageRelation = TextImageRelation.ImageBeforeText, UseVisualStyleBackColor = true,
            Padding = new Padding(LogicalToDeviceUnits(2), 0, LogicalToDeviceUnits(2), 0)
        };
        button.SetKntIcon(icon);
        new ToolTip().SetToolTip(button, tooltip);
        return button;
    }

    private void OnNotesListViewOptionsChanged(NotesListViewOptionsChanged message)
    {
        if (IsHandleCreated && InvokeRequired)
            BeginInvoke(RefreshColumnsLayout);
        else
            RefreshColumnsLayout();
    }

    // Applies a changed column layout option to the already-configured grid. Columns that become
    // visible get their saved width back (embedded) or are fitted to their content (dates).
    private void RefreshColumnsLayout()
    {
        if (IsDisposed || dataGridNotes.Columns.Count < 3)
            return; // grid not configured yet: CoonfigureGridStd will read the current option

        ApplyColumnVisibility();

        if (_ctrl.EmbededMode)
            ApplySavedColumnWidths();

        _dateColumnsFitted = false;
        FitDateColumns();
        FitTopicColumn();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _ctrl.Store.Events.Unsubscribe<NotesListViewOptionsChanged>(OnNotesListViewOptionsChanged);
        _ctrl.Store.Events.Unsubscribe<SemanticIndexProgressChanged>(OnIndexProgressChanged);
        base.OnFormClosed(e);
    }

    // Restores the widths the user left in the embedded list. Hidden columns are skipped (their saved
    // entry is kept in _savedColumnWidths, so it comes back if they are shown again).
    private void ApplySavedColumnWidths()
    {
        _savedColumnWidths = ColumnWidthSettings.Parse(_ctrl.Store.State.ManagementWindow.NotesList.ColumnWidths);

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

    // Read straight from the grid, so it is what the user sees: visible columns in display order, rows
    // in their current (sorted, text-filtered) order and each cell's formatted text (e.g. dates in the
    // grid's own format).
    public NotesListSnapshot GetDisplayedNotes()
    {
        var snapshot = new NotesListSnapshot
        {
            TextFilter = _textFilter?.Trim() ?? "",
            LoadedCount = _ctrl.ListEntities?.Count ?? 0
        };

        if (dataGridNotes.Columns.Count <= 1)
            return snapshot; // grid not configured yet: nothing loaded

        // Not the fragment of the notes found by meaning: each row would be long (as the Web's reports).
        var columns = dataGridNotes.Columns.Cast<DataGridViewColumn>()
            .Where(c => c.Visible && c.Name != FragmentColumn)
            .OrderBy(c => c.DisplayIndex)
            .ToList();

        foreach (var col in columns)
            snapshot.Columns.Add(new NotesListColumn(col.Name, col.HeaderText,
                col.DefaultCellStyle.Alignment == DataGridViewContentAlignment.MiddleRight, col.Width));

        foreach (DataGridViewRow row in dataGridNotes.Rows)
        {
            snapshot.Rows.Add(columns.Select(c => row.Cells[c.Index].FormattedValue?.ToString() ?? "").ToArray());
            snapshot.NoteIds.Add((Guid)row.Cells["NoteId"].Value);
        }

        return snapshot;
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

        // KNote Atlas, stage 4 (D14): a resource found, and its note.
        _menuOpenResource = new ToolStripMenuItem("Open resource", null, (s, a) => { if (SelectedHit() is ResourceSearchHitDto hit) OpenResource(hit); });
        _menuOpenNote = new ToolStripMenuItem("Open note", null, (s, a) => { if (SelectedHit() is ResourceSearchHitDto hit) OpenNote(hit); });
        _menuResourceSeparator = new ToolStripSeparator();
        contextMenu.Items.Insert(0, _menuResourceSeparator);
        contextMenu.Items.Insert(0, _menuOpenNote);
        contextMenu.Items.Insert(0, _menuOpenResource);
        contextMenu.Opening += contextMenu_Opening;

        this.ResumeLayout();
    }

    private void contextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_menuToggleTextFilter != null)
            _menuToggleTextFilter.Checked = _ctrl.EnableTextFilter;
        _menuOpenResource.Visible = _menuOpenNote.Visible = _ctrl.ByResources;
        _menuResourceSeparator.Visible = _ctrl.ByResources && contextMenu.Items.Count > 3;
    }

    private void extension_Click(object sender, EventArgs e)
    {
        ToolStripMenuItem menuSel;
        menuSel = (ToolStripMenuItem)sender;

        _ctrl.Extensions[menuSel.Text](this, new ControllerEventArgs<NoteMinimalDto>(_ctrl.SelectedEntity));
    }

    #endregion
}

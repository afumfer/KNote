using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Utils;
using KntIcons;
using KNote.Model;

namespace KNote.ClientWin.Views;

public partial class AppInfoAlarmsForm : KntForm, IViewAppInfoAlarms
{
    #region Private fields

    private readonly AppInfoAlarmsCtrl _ctrl;

    // Primary/growing column for ListViewColumnResizer - the last one, which used to be stretched
    // with the old "Width = -2" hack.
    private const int PrimaryColumnIndex = 4;
    private ListViewColumnSorter _sorter;

    #endregion

    #region Constructor

    public AppInfoAlarmsForm(AppInfoAlarmsCtrl ctrl)
    {
        InitializeComponent();

        _ctrl = ctrl;
        this.Text = $"{KntConst.AppName} - Application info alarms";
        _sorter = ListViewSortHelper.Attach(listViewAlarms);

        // Applied here, before the window is ever shown, rather than in Load: this form starts
        // life as StartPosition=CenterScreen (see Designer), and switching a form away from
        // CenterScreen/CenterParent after the fact (e.g. from its own Load handler) is unreliable -
        // unlike Manual/WindowsDefaultLocation forms (such as KNoteManagementForm), whose Location can
        // be safely restored in Load because nothing else is competing to (re)position them.
        // The saved position can be off every current screen (monitor or Windows scale changed since it
        // was saved), and may legitimately be negative (a monitor left of / above the primary one).
        var bounds = _ctrl.Store.State.AppInfoAlarmsWindow.Bounds;
        if (bounds.Width > 0)
            Size = new Size(bounds.Width, bounds.Height);
        if (bounds.X != 0 || bounds.Y != 0)
        {
            StartPosition = FormStartPosition.Manual;
            Bounds = WindowPlacement.EnsureVisible(new Rectangle(new Point(bounds.X, bounds.Y), Size));
        }
    }

    private Icon _windowIcon;

    protected override Icon WindowIcon => _windowIcon ??= KntIconProvider.CreateIcon(KntIcon.Alarm, DeviceDpi);

    #endregion

    #region IViewAppInfoAlarms implementation

    public void HideView()
    {
        this.Hide();
    }

    public void ActivateView()
    {
        if (this.WindowState == FormWindowState.Minimized)
            this.WindowState = FormWindowState.Normal;
        this.Show();
        this.Activate();
    }

    public Control PanelView()
    {
        return panelForm;
    }

    public void AddOrUpdateRow(AppInfoAlarmRow row)
    {
        var key = row.KMessageId.ToString();

        var existingItem = listViewAlarms.Items[key];
        if (existingItem != null)
            UpdateListViewItem(existingItem, row);
        else
            listViewAlarms.Items.Add(RowToListViewItem(row));

        // Keeps rows in column order (left to right) until the user clicks a header, and re-applies
        // whichever column/direction they already chose afterwards - see ListViewSortHelper.
        ListViewSortHelper.ApplyInitialOrder(listViewAlarms, _sorter);
    }

    public void RemoveRow(Guid kMessageId)
    {
        listViewAlarms.Items.RemoveByKey(kMessageId.ToString());
        ListViewSortHelper.ApplyInitialOrder(listViewAlarms, _sorter);
    }

    #endregion

    #region Form events handlers

    private void AppInfoAlarmsForm_Load(object sender, EventArgs e)
    {
        PersonalizeListView(listViewAlarms);
    }

    // This panel is meant to stay alive for the whole session (like the tray icon) so it keeps
    // accumulating rows in the background - closing the window (X button) only hides it. It is
    // only really closed when KNoteManagementCtrl.Finalize() cascades into it at app shutdown.
    protected override void OnUserClosing(FormClosingEventArgs e)
    {
        e.Cancel = true;
        SaveWindowBounds();
        this.Hide();
    }

    // Move/Resize are wired in InitializeComponent, and WinForms raises them from inside it when the
    // form is auto-scaled (display scale other than 100%) - i.e. before the constructor has assigned
    // _ctrl. Those early events carry no user-chosen bounds, so they are ignored.
    private void AppInfoAlarmsForm_Move(object sender, EventArgs e)
    {
        if (_ctrl != null && WindowState == FormWindowState.Normal)
        {
            _ctrl.Store.State.AppInfoAlarmsWindow.Bounds.X = Location.X;
            _ctrl.Store.State.AppInfoAlarmsWindow.Bounds.Y = Location.Y;
        }
    }

    private void AppInfoAlarmsForm_Resize(object sender, EventArgs e)
    {
        if (_ctrl != null && WindowState == FormWindowState.Normal)
        {
            _ctrl.Store.State.AppInfoAlarmsWindow.Bounds.Width = Width;
            _ctrl.Store.State.AppInfoAlarmsWindow.Bounds.Height = Height;
        }
        ListViewColumnResizer.Resize(listViewAlarms, PrimaryColumnIndex);
    }

    // Fires once when the user releases the mouse after dragging/resizing (as opposed to Move/Resize,
    // which fire continuously) - the right moment to actually write the already-updated in-memory
    // values to disk without spamming it during the drag itself.
    private void AppInfoAlarmsForm_ResizeEnd(object sender, EventArgs e)
    {
        SaveWindowBounds();
    }

    private void SaveWindowBounds()
    {
        if (WindowState != FormWindowState.Normal)
            return;

        _ctrl.Store.State.AppInfoAlarmsWindow.Bounds.X = Location.X;
        _ctrl.Store.State.AppInfoAlarmsWindow.Bounds.Y = Location.Y;
        _ctrl.Store.State.AppInfoAlarmsWindow.Bounds.Width = Width;
        _ctrl.Store.State.AppInfoAlarmsWindow.Bounds.Height = Height;
        _ctrl.Store.SaveConfig();
    }

    private void listViewAlarms_DoubleClick(object sender, EventArgs e)
    {
        var row = GetSelectedRow();
        if (row != null)
            _ctrl.RequestOpenNote(row);
    }

    private void menuRemoveFromList_Click(object sender, EventArgs e)
    {
        var row = GetSelectedRow();
        if (row != null)
            _ctrl.RemoveRow(row.KMessageId);
    }

    #endregion

    #region Private methods

    // Looks up the row via the Ctrl's own canonical list (Store.State.AppInfoAlarmsWindow.Rows) rather
    // than a private Form-side index, the same convention NoteTypesSelectorForm uses against
    // _ctrl.ListEntities.
    private AppInfoAlarmRow GetSelectedRow()
    {
        if (listViewAlarms.SelectedItems.Count == 0)
            return null;

        var selectedId = Guid.Parse(listViewAlarms.SelectedItems[0].Name);
        return _ctrl.Store.State.AppInfoAlarmsWindow.Rows.FirstOrDefault(r => r.KMessageId == selectedId);
    }

    private ListViewItem RowToListViewItem(AppInfoAlarmRow row)
    {
        var item = new ListViewItem(row.NotifiedAt.ToString("dd/MM/yyyy HH:mm"));
        item.Name = row.KMessageId.ToString();
        UpdateListViewItem(item, row, addSubItems: true);
        return item;
    }

    private void UpdateListViewItem(ListViewItem item, AppInfoAlarmRow row, bool addSubItems = false)
    {
        item.Text = row.NotifiedAt.ToString("dd/MM/yyyy HH:mm");
        if (addSubItems)
        {
            item.SubItems.Add(row.NoteTopic);
            item.SubItems.Add(row.RepositoryAlias);
            item.SubItems.Add(row.UserFullName);
            item.SubItems.Add(row.Comment);
        }
        else
        {
            item.SubItems[1].Text = row.NoteTopic;
            item.SubItems[2].Text = row.RepositoryAlias;
            item.SubItems[3].Text = row.UserFullName;
            item.SubItems[4].Text = row.Comment;
        }
    }

    private void PersonalizeListView(ListView listView)
    {
        ListViewStyle.ApplyStandard(listView);
        listView.MultiSelect = false;

        listView.Columns.Add("Notified", LogicalToDeviceUnits(130), HorizontalAlignment.Left);
        listView.Columns.Add("Note", LogicalToDeviceUnits(200), HorizontalAlignment.Left);
        listView.Columns.Add("Repository", LogicalToDeviceUnits(100), HorizontalAlignment.Left);
        listView.Columns.Add("User", LogicalToDeviceUnits(120), HorizontalAlignment.Left);
        listView.Columns.Add("Comment", LogicalToDeviceUnits(250), HorizontalAlignment.Left);
    }

    #endregion
}

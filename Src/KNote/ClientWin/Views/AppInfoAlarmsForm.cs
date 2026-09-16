using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Utils;
using KNote.Model;

namespace KNote.ClientWin.Views;

public partial class AppInfoAlarmsForm : Form, IViewAppInfoAlarms
{
    #region Private fields

    private readonly AppInfoAlarmsCtrl _ctrl;
    private bool _viewFinalized = false;
    private ListViewColumnSorter _sorter;

    #endregion

    #region Constructor

    public AppInfoAlarmsForm(AppInfoAlarmsCtrl ctrl)
    {
        InitializeComponent();

        _ctrl = ctrl;
        this.Text = $"{KntConst.AppName} - Application info alarms";
        SetWindowIcon();
        _sorter = ListViewSortHelper.Attach(listViewAlarms);

        // Applied here, before the window is ever shown, rather than in Load: this form starts
        // life as StartPosition=CenterScreen (see Designer), and switching a form away from
        // CenterScreen/CenterParent after the fact (e.g. from its own Load handler) is unreliable -
        // unlike Manual/WindowsDefaultLocation forms (such as KNoteManagmentForm), whose Location can
        // be safely restored in Load because nothing else is competing to (re)position them.
        var cfg = _ctrl.Store.AppConfig;
        if (cfg.AppInfoAlarmsWidth > 0)
            Size = new Size(cfg.AppInfoAlarmsWidth, cfg.AppInfoAlarmsHeight);
        if (cfg.AppInfoAlarmsLocX > 0 || cfg.AppInfoAlarmsLocY > 0)
        {
            StartPosition = FormStartPosition.Manual;
            Location = new Point(cfg.AppInfoAlarmsLocX, cfg.AppInfoAlarmsLocY);
        }
    }

    // Resources\Icons\alarm_24.png embedded as a resource (KNote.ClientWin.csproj) rather than wired
    // through the Designer's .resx machinery, same technique already used by
    // NotesSelectorForm.SetUndoFilterButtonIcon. Falls back to the default form icon if the resource
    // can't be found/loaded.
    private void SetWindowIcon()
    {
        try
        {
            using var iconStream = System.Reflection.Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("KNote.ClientWin.Resources.Icons.alarm_24.png");
            if (iconStream == null)
                return;

            using var bitmap = new Bitmap(iconStream);
            // Icon.FromHandle wraps a native HICON it doesn't own, so Clone() (a managed copy) is
            // kept and the native handle is deliberately left unreleased - a one-time, single-icon
            // leak for the process's lifetime, not worth a DestroyIcon P/Invoke here.
            using var icon = Icon.FromHandle(bitmap.GetHicon());
            this.Icon = (Icon)icon.Clone();
        }
        catch (Exception)
        {
            // Keep the default form icon.
        }
    }

    #endregion

    #region IViewAppInfoAlarms implementation

    public void ShowView()
    {
        this.Show();
    }

    public Result<EControllerResult> ShowModalView()
    {
        return _ctrl.DialogResultToControllerResult(this.ShowDialog());
    }

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

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        return MessageBox.Show(info, caption, buttons, icon);
    }

    public void RefreshView()
    {
    }

    public void OnClosingView()
    {
        _viewFinalized = true;
        this.Close();
    }

    public Control PanelView()
    {
        return panelForm;
    }

    // AppInfoAlarmsCtrl only ever runs in window mode, kept alive for the whole session.
    public void ConfigureEmbededMode()
    {
    }

    public void ConfigureWindowMode()
    {
    }

    public void AddOrUpdateRow(AppInfoAlarmRowConfig row)
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

    private void AppInfoAlarmsForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        // This panel is meant to stay alive for the whole session (like the tray icon) so it keeps
        // accumulating rows in the background - closing the window (X button) only hides it. It is
        // only really closed when KNoteManagmentCtrl.Finalize() cascades into it at app shutdown.
        if (!_viewFinalized)
        {
            e.Cancel = true;
            SaveWindowBounds();
            this.Hide();
        }
    }

    private void AppInfoAlarmsForm_Move(object sender, EventArgs e)
    {
        if (WindowState == FormWindowState.Normal)
        {
            _ctrl.Store.AppConfig.AppInfoAlarmsLocX = Location.X;
            _ctrl.Store.AppConfig.AppInfoAlarmsLocY = Location.Y;
        }
    }

    private void AppInfoAlarmsForm_Resize(object sender, EventArgs e)
    {
        if (WindowState == FormWindowState.Normal)
        {
            _ctrl.Store.AppConfig.AppInfoAlarmsWidth = Width;
            _ctrl.Store.AppConfig.AppInfoAlarmsHeight = Height;
        }
        SizeLastColumn(listViewAlarms);
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

        _ctrl.Store.AppConfig.AppInfoAlarmsLocX = Location.X;
        _ctrl.Store.AppConfig.AppInfoAlarmsLocY = Location.Y;
        _ctrl.Store.AppConfig.AppInfoAlarmsWidth = Width;
        _ctrl.Store.AppConfig.AppInfoAlarmsHeight = Height;
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

    // Looks up the row via the Ctrl's own canonical list (Store.AppConfig.AppInfoAlarmsRows) rather
    // than a private Form-side index, the same convention NoteTypesSelectorForm uses against
    // _ctrl.ListEntities.
    private AppInfoAlarmRowConfig GetSelectedRow()
    {
        if (listViewAlarms.SelectedItems.Count == 0)
            return null;

        var selectedId = Guid.Parse(listViewAlarms.SelectedItems[0].Name);
        return _ctrl.Store.AppConfig.AppInfoAlarmsRows.FirstOrDefault(r => r.KMessageId == selectedId);
    }

    private void SizeLastColumn(ListView lv)
    {
        // Hack for control undeterminated error
        try
        {
            lv.Columns[lv.Columns.Count - 1].Width = -2;
        }
        catch (Exception) { }
    }

    private ListViewItem RowToListViewItem(AppInfoAlarmRowConfig row)
    {
        var item = new ListViewItem(row.NotifiedAt.ToString("dd/MM/yyyy HH:mm"));
        item.Name = row.KMessageId.ToString();
        UpdateListViewItem(item, row, addSubItems: true);
        return item;
    }

    private void UpdateListViewItem(ListViewItem item, AppInfoAlarmRowConfig row, bool addSubItems = false)
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
        listView.View = View.Details;
        listView.LabelEdit = false;
        listView.AllowColumnReorder = false;
        listView.CheckBoxes = false;
        listView.FullRowSelect = true;
        listView.GridLines = true;
        listView.Sorting = SortOrder.None;
        listView.MultiSelect = false;

        listView.Columns.Add("Notified", 130, HorizontalAlignment.Left);
        listView.Columns.Add("Note", 200, HorizontalAlignment.Left);
        listView.Columns.Add("Repository", 100, HorizontalAlignment.Left);
        listView.Columns.Add("User", 120, HorizontalAlignment.Left);
        listView.Columns.Add("Comment", 250, HorizontalAlignment.Left);
    }

    #endregion
}

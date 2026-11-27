using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Utils;
using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

// «Repository changes» (RepositoryChangesCtrl, the history, H28): a modal window built here and not in the Designer. On top the
// filters (the period, what changed, the action, the user and the origin: any change reads the list again); in the middle the
// changes (when, who, from, what and the change), 100 at a time with «Show more»; at the foot «Open note» (also a double click),
// «See this version» (any change of a note) and «Recover deleted notes ...» (Staff and up), with the note of the row chosen selected
// there. Each row says in «State», with the tone of its row and the legend under the list, what can be brought back of it.
public class RepositoryChangesForm : KntForm, IViewRepositoryChanges
{
    private sealed record Choice(string Value, string Text)
    {
        public override string ToString() => Text;
    }

    #region Private fields

    private readonly RepositoryChangesCtrl _ctrl;
    private readonly DateTimePicker _from;
    private readonly DateTimePicker _to;
    private readonly ComboBox _what;
    private readonly ComboBox _action;
    private readonly ComboBox _who;
    private readonly ComboBox _origin;
    private readonly ListView _list;
    private readonly Label _status;
    private readonly Button _open;
    private readonly Button _seeVersion;
    private readonly Button _more;
    private readonly Button _deletedNotes;
    private readonly HistoryStatesLegend _legend;
    private bool _settingControls;
    private int _version;
    private const int ChangeColumnIndex = 4;

    #endregion

    #region Constructor

    public RepositoryChangesForm(RepositoryChangesCtrl ctrl)
    {
        _ctrl = ctrl;

        SuspendLayout();
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(1000, 600);
        MinimumSize = new Size(640, 360);
        ShowInTaskbar = false;
        MinimizeBox = false;

        _from = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 110 };
        _to = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 110 };
        _what = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 130 };
        _action = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 100 };
        _who = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 130 };
        _origin = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 100 };
        _action.Items.AddRange(new object[]
        {
            new Choice("", "All"), new Choice(nameof(EnumHistoryAction.Created), "Created"), new Choice(nameof(EnumHistoryAction.Updated), "Updated"),
            new Choice(nameof(EnumHistoryAction.Deleted), "Deleted"), new Choice(nameof(EnumHistoryAction.Restored), "Restored"),
            new Choice(nameof(EnumHistoryAction.Removed), "Removed")
        });
        _origin.Items.Add(new Choice("", "Any"));
        foreach (var origin in Enum.GetNames<EnumHistoryOrigin>())
            _origin.Items.Add(new Choice(origin, origin));

        var filters = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8, 8, 8, 4), WrapContents = true };
        void AddFilter(string label, Control control)
        {
            filters.Controls.Add(new Label { Text = label, AutoSize = true, Margin = new Padding(6, 7, 3, 3) });
            control.Margin = new Padding(0, 3, 6, 3);
            filters.Controls.Add(control);
        }
        AddFilter("Period from:", _from);
        AddFilter("to:", _to);
        AddFilter("What:", _what);
        AddFilter("Action:", _action);
        AddFilter("Who:", _who);
        AddFilter("From:", _origin);
        _from.ValueChanged += async (s, e) => await FilterChangedAsync();
        _to.ValueChanged += async (s, e) => await FilterChangedAsync();
        foreach (var combo in new[] { _what, _action, _who, _origin })
            combo.SelectedIndexChanged += async (s, e) => await FilterChangedAsync();

        _list = new ListView { Dock = DockStyle.Fill, MultiSelect = false, HideSelection = false, FullRowSelect = true, ShowItemToolTips = true };
        ListViewStyle.ApplyStandard(_list);
        _list.Columns.Add("When", 150, HorizontalAlignment.Left);
        _list.Columns.Add("Who", 100, HorizontalAlignment.Left);
        _list.Columns.Add("From", 80, HorizontalAlignment.Left);
        _list.Columns.Add("What", 230, HorizontalAlignment.Left);
        _list.Columns.Add("Change", 400, HorizontalAlignment.Left);
        _list.Columns.Add("State", 170, HorizontalAlignment.Left);
        _list.Resize += (s, e) => ListViewColumnResizer.Resize(_list, ChangeColumnIndex);
        _list.SelectedIndexChanged += (s, e) => UpdateButtons();
        _list.DoubleClick += async (s, e) => await _ctrl.OpenNoteAsync(Selected());

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, AutoSize = true, Padding = new Padding(6) };
        var close = FooterButtons.Create("Close");
        close.DialogResult = DialogResult.OK;
        _deletedNotes = FooterButtons.Create("Recover deleted notes ...");
        var refresh = FooterButtons.Create("Refresh");
        _more = FooterButtons.Create("Show more");
        _seeVersion = FooterButtons.Create("See this version");
        _open = FooterButtons.Create("Open note");
        _status = new Label { AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 8, 12, 3), ForeColor = SystemColors.GrayText };
        _open.Click += async (s, e) => await _ctrl.OpenNoteAsync(Selected());
        _seeVersion.Click += async (s, e) => await _ctrl.ShowVersionAsync(Selected());
        _more.Click += async (s, e) => await MoreAsync();
        refresh.Click += async (s, e) => await ReloadAsync();
        // With the note of the row chosen selected there, if it is a deletion.
        _deletedNotes.Click += async (s, e) => await _ctrl.OpenDeletedNotesAsync(Selected());
        buttons.Controls.Add(close);
        buttons.Controls.Add(_deletedNotes);
        buttons.Controls.Add(refresh);
        buttons.Controls.Add(_more);
        buttons.Controls.Add(_seeVersion);
        buttons.Controls.Add(_open);
        buttons.Controls.Add(_status);
        CancelButton = close;

        // What the states of the rows mean (the tones of their rows), between the list and the buttons.
        _legend = new HistoryStatesLegend { Dock = DockStyle.Bottom, Padding = new Padding(10, 2, 8, 2) };

        Controls.Add(_list);
        Controls.Add(_legend);
        Controls.Add(buttons);
        Controls.Add(filters);
        ResumeLayout(true);
    }

    #endregion

    #region IViewRepositoryChanges implementation

    public override void RefreshView()
    {
        Text = _ctrl.Title;
        _settingControls = true;
        try
        {
            _from.Value = _ctrl.Query.From.Date;
            _to.Value = _ctrl.Query.To.Date;
            FillChoices(_what, new[] { new Choice("", "All") }
                .Concat(HistoryRules.ChangeKinds(_ctrl.StructureIncluded).Select(k => new Choice(k.EntityName, k.Text))), _ctrl.Query.EntityName);
            FillChoices(_who, new[] { new Choice("", "Everybody") }.Concat(_ctrl.Users.Select(u => new Choice(u, u))), _ctrl.Query.UserName);
            SelectChoice(_action, _ctrl.Query.ActionType);
            SelectChoice(_origin, _ctrl.Query.Origin);
        }
        finally
        {
            _settingControls = false;
        }

        _list.BeginUpdate();
        _list.Items.Clear();
        foreach (var row in _ctrl.Rows)
        {
            var item = new ListViewItem(HistoryRules.WhenText(row)) { Name = row.KLogId.ToString(), ToolTipText = row.RegistryMessage };
            item.SubItems.Add(row.UserName ?? "");
            item.SubItems.Add(row.Origin ?? "");
            item.SubItems.Add(HistoryRules.WhatText(row));
            item.SubItems.Add(row.RegistryMessage ?? "");
            item.SubItems.Add(_ctrl.StateText(row));
            HistoryRowColors.Paint(item, _ctrl.StateOf(row));
            _list.Items.Add(item);
        }
        _list.EndUpdate();
        _status.Text = _ctrl.Summary;
        _more.Visible = _ctrl.HasMore;
        _deletedNotes.Visible = _ctrl.CanRecover;
        _legend.SetStates(_ctrl.States);
        ListViewColumnResizer.Resize(_list, ChangeColumnIndex);
        UpdateButtons();
    }

    #endregion

    #region Form event handlers

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        FooterButtons.FitTexts(this);
        RefreshView();
    }

    protected override void OnUserClosing(FormClosingEventArgs e) => _ctrl.Finalize();

    #endregion

    private static void FillChoices(ComboBox combo, IEnumerable<Choice> choices, string selected)
    {
        combo.BeginUpdate();
        combo.Items.Clear();
        foreach (var choice in choices)
            combo.Items.Add(choice);
        combo.EndUpdate();
        SelectChoice(combo, selected);
    }

    private static void SelectChoice(ComboBox combo, string value)
    {
        var index = combo.Items.Cast<Choice>().ToList().FindIndex(c => c.Value == (value ?? ""));
        combo.SelectedIndex = Math.Max(0, index);
    }

    private static string Value(ComboBox combo) => (combo.SelectedItem as Choice)?.Value is { Length: > 0 } value ? value : null;

    private KLogDto Selected() => _list.SelectedItems.Count == 0
        ? null
        : _ctrl.Rows.FirstOrDefault(r => r.KLogId.ToString() == _list.SelectedItems[0].Name);

    private void UpdateButtons()
    {
        var row = Selected();
        _open.Enabled = _ctrl.CanOpen(row);
        _seeVersion.Enabled = _ctrl.CanSeeVersion(row);
    }

    private async Task FilterChangedAsync()
    {
        if (_settingControls)
            return;
        _ctrl.Query.From = _from.Value.Date;
        _ctrl.Query.To = _to.Value.Date;
        _ctrl.Query.EntityName = Value(_what);
        _ctrl.Query.ActionType = Value(_action);
        _ctrl.Query.UserName = Value(_who);
        _ctrl.Query.Origin = Value(_origin);
        await ReloadAsync();
    }

    // The list again with the filters; a filter changed meanwhile wins.
    private async Task ReloadAsync()
    {
        var version = ++_version;
        _status.Text = "Loading the changes…";
        // What it showed, greyed out until the new list arrives: the filter was heard.
        _list.Enabled = false;
        var loaded = await _ctrl.LoadAsync();
        if (IsDisposed || version != _version)
            return;
        _list.Enabled = true;
        if (!loaded)
        {
            _status.Text = "";
            ShowInfo(_ctrl.Error);
            return;
        }
        RefreshView();
    }

    private async Task MoreAsync()
    {
        _more.Enabled = false;
        try
        {
            if (await _ctrl.LoadMoreAsync())
                RefreshView();
            else
                ShowInfo(_ctrl.Error);
        }
        finally
        {
            if (!IsDisposed)
                _more.Enabled = true;
        }
    }
}

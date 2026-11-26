using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Utils;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

// «Recover deleted notes» (DeletedNotesCtrl, the history, H24): a modal window built here and not in the Designer. The deleted notes
// of the repository (number, topic, folder, when and by whom), «See this version», «Recover» (it then says what didn't come back and
// offers to open the note) and, for an Admin, «Remove permanently» (H27).
public class DeletedNotesForm : KntForm, IViewDeletedNotes
{
    #region Private fields

    private readonly DeletedNotesCtrl _ctrl;
    private readonly ListView _list;
    private readonly Label _status;
    private readonly Label _subtitle;
    private readonly Button _seeVersion;
    private readonly Button _recover;
    private readonly Button _remove;
    private const int TopicColumnIndex = 1;

    #endregion

    #region Constructor

    public DeletedNotesForm(DeletedNotesCtrl ctrl)
    {
        _ctrl = ctrl;

        SuspendLayout();
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(860, 520);
        MinimumSize = new Size(560, 320);
        ShowInTaskbar = false;
        MinimizeBox = false;

        _subtitle = new Label { AutoSize = true, Dock = DockStyle.Top, ForeColor = SystemColors.GrayText, Padding = new Padding(10, 8, 10, 4) };

        _list = new ListView { Dock = DockStyle.Fill, MultiSelect = false, HideSelection = false, FullRowSelect = true };
        ListViewStyle.ApplyStandard(_list);
        _list.Columns.Add("#", 60, HorizontalAlignment.Right);
        _list.Columns.Add("Topic", 320, HorizontalAlignment.Left);
        _list.Columns.Add("Folder", 180, HorizontalAlignment.Left);
        _list.Columns.Add("Deleted", 130, HorizontalAlignment.Left);
        _list.Columns.Add("By", 100, HorizontalAlignment.Left);
        _list.Resize += (s, e) => ListViewColumnResizer.Resize(_list, TopicColumnIndex);
        _list.SelectedIndexChanged += (s, e) => UpdateButtons();
        _list.DoubleClick += async (s, e) => await _ctrl.ShowVersionAsync(Selected());

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, AutoSize = true, Padding = new Padding(6) };
        var close = FooterButtons.Create("Close");
        close.DialogResult = DialogResult.OK;
        _remove = FooterButtons.Create("Remove permanently");
        _remove.Visible = _ctrl.CanRemove;
        _recover = FooterButtons.Create("Recover");
        _seeVersion = FooterButtons.Create("See this version");
        _status = new Label { AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 8, 12, 3), ForeColor = SystemColors.GrayText };
        _seeVersion.Click += async (s, e) => await _ctrl.ShowVersionAsync(Selected());
        _recover.Click += async (s, e) => await RecoverAsync();
        _remove.Click += async (s, e) => await RemoveAsync();
        buttons.Controls.Add(close);
        buttons.Controls.Add(_remove);
        buttons.Controls.Add(_recover);
        buttons.Controls.Add(_seeVersion);
        buttons.Controls.Add(_status);
        CancelButton = close;

        Controls.Add(_list);
        Controls.Add(buttons);
        Controls.Add(_subtitle);
        ResumeLayout(true);
    }

    #endregion

    #region IViewDeletedNotes implementation

    public override void RefreshView()
    {
        Text = _ctrl.Title;
        _subtitle.Text = _ctrl.Subtitle;

        _list.BeginUpdate();
        _list.Items.Clear();
        foreach (var note in _ctrl.Notes)
        {
            var item = new ListViewItem(note.NoteNumber > 0 ? note.NoteNumber.ToString() : "") { Name = note.NoteId.ToString(), ToolTipText = note.Topic };
            item.SubItems.Add(note.Topic ?? "");
            item.SubItems.Add(note.FolderPath ?? "");
            item.SubItems.Add(note.DeletedAt.ToString("g"));
            item.SubItems.Add(note.DeletedBy ?? "");
            _list.Items.Add(item);
        }
        _list.EndUpdate();
        _status.Text = _ctrl.Notes.Count == 0 ? "No deleted notes to recover."
            : _ctrl.Notes.Count == 1 ? "1 deleted note" : $"{_ctrl.Notes.Count} deleted notes";
        ListViewColumnResizer.Resize(_list, TopicColumnIndex);
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

    private DeletedNoteDto Selected() => _list.SelectedItems.Count == 0
        ? null
        : _ctrl.Notes.FirstOrDefault(n => n.NoteId.ToString() == _list.SelectedItems[0].Name);

    private void UpdateButtons()
    {
        var selected = Selected() != null;
        _seeVersion.Enabled = selected;
        _recover.Enabled = selected;
        _remove.Enabled = selected;
    }

    private async Task RecoverAsync()
    {
        var note = Selected();
        if (note == null)
            return;
        _recover.Enabled = false;
        try
        {
            if (!await _ctrl.RecoverAsync(note))
            {
                ShowInfo(_ctrl.Error);
                return;
            }
            RefreshView();
            var recovered = _ctrl.Recovered;
            var message = $"Recovered: #{recovered.NoteNumber} {recovered.Topic}." + Environment.NewLine + Environment.NewLine
                + string.Join(Environment.NewLine, recovered.Notices) + Environment.NewLine + Environment.NewLine + "Open the note?";
            if (ShowInfo(message, "Recover deleted notes", MessageBoxButtons.YesNo) == DialogResult.Yes)
                await _ctrl.OpenRecoveredAsync();
        }
        finally
        {
            if (!IsDisposed)
                UpdateButtons();
        }
    }

    private async Task RemoveAsync()
    {
        var note = Selected();
        if (note == null || !_ctrl.CanRemove)
            return;
        if (ShowInfo($"Remove #{note.NoteNumber} {note.Topic} permanently? Its versions are emptied and it can no longer be recovered.",
                "Remove permanently", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;
        if (await _ctrl.RemoveAsync(note))
            RefreshView();
        else
            ShowInfo(_ctrl.Error);
    }
}

using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Utils;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

// «Review selection» (ReviewSelectionCtrl, 2.3b): a modal window with a tab for each list of the selection (In, Doubtful, Out), each
// one a list with a box per note (checked, it goes into the summary), its number, topic, folder, date and reason. Double click opens
// the note. «Accept» keeps the boxes changed (the window stays if they could not be kept); «Cancel» keeps nothing.
public partial class ReviewSelectionForm : KntForm, IViewReviewSelection
{
    #region Private fields

    private static readonly EnumSelectionDecision[] Decisions = { EnumSelectionDecision.In, EnumSelectionDecision.Doubtful, EnumSelectionDecision.Out };

    private readonly ReviewSelectionCtrl _ctrl;
    private readonly List<ListView> _lists = new();
    private bool _filling;

    #endregion

    #region Constructor

    public ReviewSelectionForm(ReviewSelectionCtrl ctrl)
    {
        InitializeComponent();

        _ctrl = ctrl;
        foreach (var decision in Decisions)
        {
            var notes = _ctrl.NotesOf(decision);
            var page = new TabPage($"{ReviewSelectionCtrl.ListText(decision)} ({notes.Count})");
            var list = new ListView { Dock = DockStyle.Fill, View = View.Details, HideSelection = false, MultiSelect = false };
            list.Columns.Add("#", LogicalToDeviceUnits(60), HorizontalAlignment.Right);
            list.Columns.Add("Topic", LogicalToDeviceUnits(220));
            list.Columns.Add("Folder", LogicalToDeviceUnits(120));
            list.Columns.Add("Modified", LogicalToDeviceUnits(90));
            list.Columns.Add("Reason", LogicalToDeviceUnits(420));
            // It sets the boxes (and the full row): without checkBoxes, it would take them away.
            ListViewStyle.ApplyStandard(list, checkBoxes: true);
            list.ItemChecked += list_ItemChecked;
            list.DoubleClick += list_DoubleClick;
            page.Controls.Add(list);
            tabLists.TabPages.Add(page);
            _lists.Add(list);
        }
    }

    #endregion

    #region IViewReviewSelection implementation

    public override void RefreshView()
    {
        _filling = true;
        try
        {
            Text = $"Review selection · {_ctrl.CountText}";
            for (var i = 0; i < Decisions.Length; i++)
            {
                var list = _lists[i];
                list.BeginUpdate();
                list.Items.Clear();
                foreach (var note in _ctrl.NotesOf(Decisions[i]))
                {
                    var item = new ListViewItem(note.NoteNumber.ToString()) { Tag = note, Checked = _ctrl.IsChecked(note) };
                    item.SubItems.Add(note.Topic ?? "");
                    item.SubItems.Add(note.FolderName ?? "");
                    item.SubItems.Add(note.ModificationDateTime.ToShortDateString());
                    item.SubItems.Add(note.Reason ?? "");
                    list.Items.Add(item);
                }
                list.EndUpdate();
            }
        }
        finally
        {
            _filling = false;
        }
        labelChanges.Text = _ctrl.ChangesText;
    }

    #endregion

    #region Form event handlers

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        RefreshView();
    }

    private void list_ItemChecked(object sender, ItemCheckedEventArgs e)
    {
        if (_filling || e.Item.Tag is not NoteSelectionDto note)
            return;
        _ctrl.SetChecked(note.NoteId, e.Item.Checked);
        labelChanges.Text = _ctrl.ChangesText;
    }

    private async void list_DoubleClick(object sender, EventArgs e)
    {
        if (sender is ListView { SelectedItems.Count: > 0 } list && list.SelectedItems[0].Tag is NoteSelectionDto note)
            await _ctrl.OpenNoteAsync(note.NoteId);
    }

    private async void buttonAccept_Click(object sender, EventArgs e)
    {
        buttonAccept.Enabled = buttonCancel.Enabled = false;
        try
        {
            if (!await _ctrl.AcceptAsync())
                return;
            DialogResult = DialogResult.OK;
            Close();
        }
        finally
        {
            if (!IsDisposed)
                buttonAccept.Enabled = buttonCancel.Enabled = true;
        }
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    #endregion
}

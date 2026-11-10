using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Views;

// KNote Atlas (Tools › KNote Atlas, 4.3b): everything of the AI of a repository, a section for each part in the list on the
// left (a section the user can't use is grayed out and says who can). The AI personalization (chapters 19–21) is this
// form's own tabs: the repository's profile, its glossary and the instructions of the tasks show the state of their notes,
// which are written in the note editor; «What the AI receives» shows the result of the three. The other sections are the
// embedded views of their controllers (AtlasCtrl.ModelsCtrl, SemanticIndexCtrl, ResourcesCtrl).
public partial class AtlasForm : KntForm, IViewBase
{
    private readonly AtlasCtrl _ctrl;
    private readonly ToolTip _toolTipSections = new();
    private bool _closingResolved;

    public AtlasForm()
    {
        InitializeComponent();
    }

    public AtlasForm(AtlasCtrl ctrl) : this()
    {
        _ctrl = ctrl;

        comboTasks.DisplayMember = nameof(AiTask.Name);
        comboTasks.DataSource = AiTaskCatalog.Tasks.ToList();
        comboTasks.Enabled = AiTaskCatalog.Tasks.Count > 1;
        comboTasks.SelectedIndexChanged += comboTasks_SelectedIndexChanged;

        panelModels.Controls.Add(_ctrl.ModelsCtrl.View.PanelView());
        panelSemanticIndex.Controls.Add(_ctrl.SemanticIndexCtrl.View.PanelView());
        panelResources.Controls.Add(_ctrl.ResourcesCtrl.View.PanelView());

        // The height of the text and a margin, at any scale.
        listSections.ItemHeight = listSections.Font.Height + LogicalToDeviceUnits(10);
        foreach (var section in AtlasCtrl.Sections)
            listSections.Items.Add(section);
        listSections.DrawItem += listSections_DrawItem;
        listSections.SelectedIndexChanged += listSections_SelectedIndexChanged;
        listSections.MouseMove += listSections_MouseMove;
    }

    private AtlasTaskPromptStatusDto SelectedTask =>
        listTasks.SelectedItems.Count > 0 ? listTasks.SelectedItems[0].Tag as AtlasTaskPromptStatusDto : _ctrl.Tasks.FirstOrDefault();

    protected override void ModelToControls()
    {
        Text = $"KNote Atlas · {_ctrl.RepositoryName}";
        ShowRepositories();
        ShowSections();
        ShowProfile();
        ShowGlossary();
        ShowTasks();
        ShowComposed();
    }

    private void ShowRepositories()
    {
        comboRepositories.SelectedIndexChanged -= comboRepositories_SelectedIndexChanged;

        var repositories = _ctrl.Repositories;
        if (!comboRepositories.Items.Cast<ServiceRef>().SequenceEqual(repositories))
        {
            comboRepositories.DataSource = null;
            comboRepositories.DisplayMember = nameof(ServiceRef.Name);
            comboRepositories.DataSource = repositories;
        }
        comboRepositories.SelectedItem = _ctrl.ServiceRef;
        comboRepositories.Enabled = comboRepositories.Items.Count > 1;

        comboRepositories.SelectedIndexChanged += comboRepositories_SelectedIndexChanged;
    }

    private void ShowProfile()
    {
        var info = _ctrl.Profile;
        var note = info?.Note;
        if (info == null)
            labelProfile.Text = "";
        else if (note == null)
            labelProfile.Text = "This repository has no profile yet.";
        else
            labelProfile.Text = $"Note:  #{note.NoteNumber} {note.Topic}\r\n" +
                $"Length:  {info.Words} words (recommended: up to {info.MaxWords}){(info.Words > info.MaxWords ? "" : "  ✔")}\r\n" +
                $"Modified:  {note.ModificationDateTime:g}" +
                (info.Notices.Count > 0 ? "\r\n\r\n" + string.Join("\r\n", info.Notices) : "");

        buttonCreateProfile.Visible = info != null && note == null;
        buttonCreateProfile.Enabled = _ctrl.CanCreateProfile;
        buttonOpenProfile.Visible = note != null;
        buttonFillProfile.Enabled = info != null && _ctrl.CanFillProfile;
    }

    private void ShowGlossary()
    {
        var info = _ctrl.Glossary;
        var note = info?.Note;
        if (info == null)
            labelGlossary.Text = "";
        else if (note == null)
            labelGlossary.Text = "This repository has no glossary yet.";
        else
        {
            var problems = info.Issues.Concat(info.Notices).ToList();
            labelGlossary.Text = $"Note:  #{note.NoteNumber} {note.Topic}\r\n" +
                $"Terms:  {info.Terms}\r\n" +
                $"Modified:  {note.ModificationDateTime:g}" +
                (problems.Count > 0 ? "\r\n\r\n" + string.Join("\r\n", problems.Take(8)) + (problems.Count > 8 ? $"\r\n… and {problems.Count - 8} more." : "") : "");
        }

        buttonCreateGlossary.Visible = info != null && note == null;
        buttonCreateGlossary.Enabled = _ctrl.CanCreateGlossary;
        buttonOpenGlossary.Visible = note != null;
    }

    private void ShowTasks()
    {
        var selectedKey = SelectedTask?.TaskKey;

        listTasks.BeginUpdate();
        listTasks.Items.Clear();
        foreach (var task in _ctrl.Tasks)
        {
            var item = new ListViewItem(task.TaskName) { Tag = task };
            item.SubItems.Add(State(task));
            listTasks.Items.Add(item);
            if (task.TaskKey == selectedKey)
                item.Selected = true;
        }
        if (listTasks.SelectedItems.Count == 0 && listTasks.Items.Count > 0)
            listTasks.Items[0].Selected = true;
        listTasks.EndUpdate();

        labelTaskNotices.Text = string.Join("\r\n", _ctrl.Tasks.SelectMany(t => t.Notices));
        buttonCustomize.Enabled = _ctrl.CanCustomizeTask;
    }

    private static string State(AtlasTaskPromptStatusDto task) => task.State switch
    {
        EnumAtlasTaskPromptState.Added => $"Added (#{task.Note?.NoteNumber})",
        EnumAtlasTaskPromptState.Replaced => $"Replaced (#{task.Note?.NoteNumber})",
        _ => "KNote's"
    };

    private void ShowComposed()
    {
        var composed = _ctrl.Composed;
        textTest.Enabled = _ctrl.CanCompose;
        buttonTry.Enabled = _ctrl.CanCompose;
        labelSummary.Text = composed != null ? AiPromptComposer.Summary(composed)
            : _ctrl.CanCompose ? "" : "Seeing what the AI receives needs the role Staff or higher.";
        textPrompt.Text = (composed?.Text ?? "").ReplaceLineEndings("\r\n");
    }

    private Panel PanelOf(EnumAtlasSection section) => section switch
    {
        EnumAtlasSection.Models => panelModels,
        EnumAtlasSection.SemanticIndex => panelSemanticIndex,
        EnumAtlasSection.Resources => panelResources,
        _ => panelPersonalization
    };

    private void ShowSections()
    {
        listSections.SelectedIndexChanged -= listSections_SelectedIndexChanged;
        listSections.SelectedItem = _ctrl.Section;
        listSections.SelectedIndexChanged += listSections_SelectedIndexChanged;
        listSections.Invalidate();

        foreach (var section in AtlasCtrl.Sections)
            PanelOf(section).Visible = section == _ctrl.Section;
    }

    private void listSections_SelectedIndexChanged(object sender, EventArgs e)
    {
        // A section the user can't use is not shown: the list goes back to the one there was.
        if (listSections.SelectedItem is EnumAtlasSection section && _ctrl.IsAvailable(section))
            _ctrl.Section = section;
        ShowSections();
    }

    private void listSections_DrawItem(object sender, DrawItemEventArgs e)
    {
        if (e.Index < 0)
            return;

        var section = (EnumAtlasSection)listSections.Items[e.Index];
        var available = _ctrl.IsAvailable(section);
        var selected = (e.State & DrawItemState.Selected) != 0;
        e.DrawBackground();
        var color = !available ? SystemColors.GrayText : selected ? SystemColors.HighlightText : listSections.ForeColor;
        var text = AtlasCtrl.SectionText(section) + (available ? "" : " (Admin)");
        var bounds = new Rectangle(e.Bounds.X + LogicalToDeviceUnits(8), e.Bounds.Y, e.Bounds.Width - LogicalToDeviceUnits(8), e.Bounds.Height);
        TextRenderer.DrawText(e.Graphics, text, listSections.Font, bounds, color,
            TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
        e.DrawFocusRectangle();
    }

    // Who can use a section that is grayed out.
    private void listSections_MouseMove(object sender, MouseEventArgs e)
    {
        var index = listSections.IndexFromPoint(e.Location);
        var hint = index >= 0 && listSections.Items[index] is EnumAtlasSection section && !_ctrl.IsAvailable(section)
            ? AtlasCtrl.WhoCanUse(section) : "";
        if (_toolTipSections.GetToolTip(listSections) != hint)
            _toolTipSections.SetToolTip(listSections, hint);
    }

    protected override void OnUserClosing(FormClosingEventArgs e)
    {
        // The AI models not saved: save them, discard them or stay (async, so the closing waits for the answer).
        if (!_closingResolved && _ctrl.HasPendingChanges)
        {
            e.Cancel = true;
            BeginInvoke(async () =>
            {
                if (await _ctrl.ResolvePendingChangesAsync())
                {
                    _closingResolved = true;
                    Close();
                }
            });
            return;
        }
        _ctrl.Finalize();
    }

    private async void comboRepositories_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (comboRepositories.SelectedItem is ServiceRef repository && repository != _ctrl.ServiceRef)
        {
            var changed = false;
            await RunAsync(async () => changed = await _ctrl.SelectRepositoryAsync(repository));
            // The AI models not saved kept the repository.
            if (!changed)
                ShowRepositories();
        }
    }

    private async void buttonCreateProfile_Click(object sender, EventArgs e) => await RunAsync(_ctrl.CreateProfileAsync);

    private async void buttonOpenProfile_Click(object sender, EventArgs e) =>
        await RunAsync(() => _ctrl.OpenInEditorAsync(_ctrl.Profile.Note.NoteId));

    private async void buttonFillProfile_Click(object sender, EventArgs e) => await RunAsync(_ctrl.FillProfileAsync);

    private async void buttonCreateGlossary_Click(object sender, EventArgs e) => await RunAsync(_ctrl.CreateGlossaryAsync);

    private async void buttonOpenGlossary_Click(object sender, EventArgs e) =>
        await RunAsync(() => _ctrl.OpenInEditorAsync(_ctrl.Glossary.Note.NoteId));

    private void buttonKNote_Click(object sender, EventArgs e)
    {
        var task = SelectedTask;
        if (task != null)
            new AiPromptViewForm($"KNote's instructions · {task.TaskName}", AtlasCtrl.KNoteInstructions(task.TaskKey)).ShowDialog(this);
    }

    private async void buttonCustomize_Click(object sender, EventArgs e)
    {
        var task = SelectedTask;
        if (task != null)
            await RunAsync(() => _ctrl.CustomizeTaskAsync(task.TaskKey));
    }

    private async void comboTasks_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (comboTasks.SelectedItem is AiTask task && task.Key != _ctrl.TaskKey)
        {
            _ctrl.TaskKey = task.Key;
            await RunAsync(_ctrl.ComposeAsync);
        }
    }

    private async void buttonTry_Click(object sender, EventArgs e)
    {
        _ctrl.TestText = textTest.Text;
        await RunAsync(_ctrl.ComposeAsync);
    }

    private void buttonClose_Click(object sender, EventArgs e) => Close();

    private async Task RunAsync(Func<Task> action)
    {
        Cursor = Cursors.WaitCursor;
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            ShowInfo(ex.Message);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }
}

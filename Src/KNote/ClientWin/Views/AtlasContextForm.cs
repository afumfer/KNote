using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Views;

// KNote Atlas (chapters 19–21): the context the AI receives in a repository, a tab for each part. The repository's
// profile, its glossary and the instructions of the tasks show the state of their notes, which are written in the note
// editor; «What the AI receives» shows the result of the three.
public partial class AtlasContextForm : KntForm, IViewBase
{
    private readonly AtlasContextCtrl _ctrl;

    public AtlasContextForm()
    {
        InitializeComponent();
    }

    public AtlasContextForm(AtlasContextCtrl ctrl) : this()
    {
        _ctrl = ctrl;

        comboTasks.DisplayMember = nameof(AiTask.Name);
        comboTasks.DataSource = AiTaskCatalog.Tasks.ToList();
        comboTasks.Enabled = AiTaskCatalog.Tasks.Count > 1;
        comboTasks.SelectedIndexChanged += comboTasks_SelectedIndexChanged;
    }

    private AtlasTaskPromptStatusDto SelectedTask =>
        listTasks.SelectedItems.Count > 0 ? listTasks.SelectedItems[0].Tag as AtlasTaskPromptStatusDto : _ctrl.Tasks.FirstOrDefault();

    protected override void ModelToControls()
    {
        Text = $"KNote Atlas · {_ctrl.RepositoryName}";
        ShowRepositories();
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

    protected override void OnUserClosing(FormClosingEventArgs e) => _ctrl.Finalize();

    private async void comboRepositories_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (comboRepositories.SelectedItem is ServiceRef repository && repository != _ctrl.ServiceRef)
            await RunAsync(() => _ctrl.SelectRepositoryAsync(repository));
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
            new AiPromptViewForm($"KNote's instructions · {task.TaskName}", AtlasContextCtrl.KNoteInstructions(task.TaskKey)).ShowDialog(this);
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

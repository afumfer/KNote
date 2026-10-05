using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Tests.Fakes;

/// <summary>IViewFileExport test double for the file export use cases (e.g. NotesListCsvExportCtrl).</summary>
internal class FakeFileExportView : IViewFileExport
{
    public string LastShownInfo { get; private set; }
    public DialogResult ShowInfoAnswer { get; set; } = DialogResult.No;
    public Func<string>? PromptForSaveFileImpl { get; set; }
    public int PromptForSaveFileCallCount { get; private set; }

    public string PromptForSaveFile(string title, string filter, string initialDirectory, string fileName)
    {
        PromptForSaveFileCallCount++;
        return (PromptForSaveFileImpl ?? throw new NotSupportedException($"{nameof(PromptForSaveFile)} not configured for this test"))();
    }

    public void ShowView() { }
    public Result<EControllerResult> ShowModalView() => new(EControllerResult.Executed);
    public void RefreshView() { }
    public void OnClosingView() { }

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        LastShownInfo = info;
        return ShowInfoAnswer;
    }
}

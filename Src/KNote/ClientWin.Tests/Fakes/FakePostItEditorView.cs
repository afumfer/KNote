using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests.Fakes;

/// <summary>IViewPostItEditor&lt;NoteDto&gt; test double for PostItEditorCtrl tests.</summary>
internal class FakePostItEditorView : IViewPostItEditor<NoteDto>
{
    public string LastShownInfo { get; private set; }

    public void ShowView() { }
    public Result<EControllerResult> ShowModalView() => new(EControllerResult.Executed);
    public void RefreshView() { }
    public void OnClosingView() { }

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        LastShownInfo = info;
        return DialogResult.OK;
    }

    public void RefreshModel() { }
    public void HideView() { }
    public void ActivateView() { }
    public Task RefreshFolderAndRepositoryDisplayAsync() => Task.CompletedTask;
}

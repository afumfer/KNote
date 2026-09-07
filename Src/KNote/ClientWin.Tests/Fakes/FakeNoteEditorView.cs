using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests.Fakes;

/// <summary>IViewNoteEditorEmbeddable&lt;NoteExtendedDto&gt; test double for NoteEditorCtrl tests.</summary>
internal class FakeNoteEditorView : IViewNoteEditorEmbeddable<NoteExtendedDto>
{
    public string LastShownInfo { get; private set; }
    public DialogResult ConfirmationResult { get; set; } = DialogResult.Yes;

    public void ShowView() { }
    public Result<EControllerResult> ShowModalView() => new(EControllerResult.Executed);
    public void RefreshView() { }
    public void OnClosingView() { }

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        LastShownInfo = info;
        return buttons == MessageBoxButtons.YesNo ? ConfirmationResult : DialogResult.OK;
    }

    public Control PanelView() => new Panel();
    public void ConfigureEmbededMode() { }
    public void ConfigureWindowMode() { }
    public void CleanView() { }
    public void RefreshModel() { }
    public void RefreshViewOnlyRequiredCtrl() { }

    public Func<Task>? RefreshFolderAndRepositoryDisplayAsyncImpl { get; set; }
    public Task RefreshFolderAndRepositoryDisplayAsync() =>
        (RefreshFolderAndRepositoryDisplayAsyncImpl ?? throw new NotSupportedException($"{nameof(RefreshFolderAndRepositoryDisplayAsync)} not configured for this test"))();
}

using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests.Fakes;

/// <summary>IViewEditor&lt;UserDto&gt; test double for UserEditorCtrl tests.</summary>
internal class FakeUserEditorView : IViewEditor<UserDto>
{
    public string? LastShownInfo { get; private set; }
    public DialogResult NextShowInfoResult { get; set; } = DialogResult.Yes;
    public Func<Result<EControllerResult>>? ShowModalViewImpl { get; set; }

    public void ShowView() { }

    public Result<EControllerResult> ShowModalView() =>
        (ShowModalViewImpl ?? (() => new Result<EControllerResult>(EControllerResult.Executed)))();

    public void RefreshView() { }
    public void OnClosingView() { }

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        LastShownInfo = info;
        return NextShowInfoResult;
    }

    public void CleanView() { }
    public void RefreshModel() { }
}

using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Tests.Fakes;

/// <summary>IViewBase test double for KntScriptConsoleCtrl tests (its view has no dedicated
/// IView* interface - it manages IViewBase directly, see ClientWin/CLAUDE.md).</summary>
internal class FakeKntScriptConsoleView : IViewBase
{
    public int ShowViewCallCount { get; private set; }
    public string? LastShownInfo { get; private set; }
    public DialogResult NextShowInfoResult { get; set; } = DialogResult.OK;

    public void ShowView() => ShowViewCallCount++;

    public Result<EControllerResult> ShowModalView() => new(EControllerResult.Executed);

    public void RefreshView() { }
    public void OnClosingView() { }

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        LastShownInfo = info;
        return NextShowInfoResult;
    }
}

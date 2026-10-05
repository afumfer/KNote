using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Tests.Fakes;

/// <summary>Plain IViewBase test double that counts how many times it was shown.</summary>
internal class FakeBaseView : IViewBase
{
    public int ShowViewCallCount { get; private set; }
    public int ShowModalViewCallCount { get; private set; }
    public string LastShownInfo { get; private set; }

    public void ShowView() => ShowViewCallCount++;

    public Result<EControllerResult> ShowModalView()
    {
        ShowModalViewCallCount++;
        return new Result<EControllerResult>(EControllerResult.Executed);
    }

    public void RefreshView() { }
    public void OnClosingView() { }

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        LastShownInfo = info;
        return DialogResult.OK;
    }
}

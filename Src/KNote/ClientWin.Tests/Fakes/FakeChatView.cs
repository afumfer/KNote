using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Tests.Fakes;

/// <summary>IViewChat test double for KntChatCtrl tests.</summary>
internal class FakeChatView : IViewChat
{
    public int ShowViewCallCount { get; private set; }
    public List<bool> VisibleViewCalls { get; } = new();
    public string? LastShownInfo { get; private set; }
    public int ShowInfoCallCount { get; private set; }

    public void ShowView() => ShowViewCallCount++;

    public Result<EControllerResult> ShowModalView() => new(EControllerResult.Executed);

    public void RefreshView() { }
    public void OnClosingView() { }

    public void VisibleView(bool visible) => VisibleViewCalls.Add(visible);

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        ShowInfoCallCount++;
        LastShownInfo = info;
        return DialogResult.OK;
    }
}

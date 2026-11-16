using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Tests.Fakes;

/// <summary>IViewLongDocumentSummary test double for LongDocumentSummaryCtrl tests.</summary>
internal class FakeLongDocumentSummaryView : IViewLongDocumentSummary
{
    public int ShowViewCallCount { get; private set; }
    public int RefreshViewCallCount { get; private set; }
    public int RefreshProgressCallCount { get; private set; }
    public string? LastShownInfo { get; private set; }
    public DialogResult NextShowInfoResult { get; set; } = DialogResult.OK;

    public void ShowView() => ShowViewCallCount++;
    public Result<EControllerResult> ShowModalView() => new(EControllerResult.Executed);
    public void RefreshView() => RefreshViewCallCount++;
    public void RefreshProgress() => RefreshProgressCallCount++;
    public void OnClosingView() { }

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        LastShownInfo = info;
        return NextShowInfoResult;
    }
}

using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Tests.Fakes;

/// <summary>IViewPeriodSummary test double (RepositoryPeriodSummaryCtrl): keeps every message shown and counts the refreshes.</summary>
internal class FakePeriodSummaryView : IViewPeriodSummary
{
    public List<string> ShownInfo { get; } = new();
    public int RefreshViewCount { get; private set; }

    public void ShowView() { }
    public Result<EControllerResult> ShowModalView() => new(EControllerResult.Executed);
    public void RefreshView() => RefreshViewCount++;
    public void OnClosingView() { }

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        ShownInfo.Add(info);
        return DialogResult.OK;
    }

    public Control PanelView() => throw new NotSupportedException($"{nameof(PanelView)} not configured for this test");
    public void ConfigureEmbededMode() { }
    public void ConfigureWindowMode() { }
}

/// <summary>IViewReviewSelection test double (ReviewSelectionCtrl): keeps every message shown.</summary>
internal class FakeReviewSelectionView : IViewReviewSelection
{
    public List<string> ShownInfo { get; } = new();

    public void ShowView() { }
    public Result<EControllerResult> ShowModalView() => new(EControllerResult.Executed);
    public void RefreshView() { }
    public void OnClosingView() { }

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        ShownInfo.Add(info);
        return DialogResult.OK;
    }
}

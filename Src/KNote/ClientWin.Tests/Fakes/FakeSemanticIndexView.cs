using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Tests.Fakes;

/// <summary>
/// IViewSemanticIndex test double (RepositorySemanticIndexCtrl): keeps every message shown, answers the questions with
/// ShowInfoImpl (OK when not set) and counts the refreshes.
/// </summary>
internal class FakeSemanticIndexView : IViewSemanticIndex
{
    public List<string> ShownInfo { get; } = new();
    public Func<string, MessageBoxButtons, DialogResult>? ShowInfoImpl { get; set; }
    public int RefreshViewCount { get; private set; }
    public int RefreshProgressCount { get; private set; }

    public void ShowView() { }
    public Result<EControllerResult> ShowModalView() => new(EControllerResult.Executed);
    public void RefreshView() => RefreshViewCount++;
    public void RefreshProgress() => RefreshProgressCount++;
    public void OnClosingView() { }

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        ShownInfo.Add(info);
        return ShowInfoImpl?.Invoke(info, buttons) ?? DialogResult.OK;
    }

    public Control PanelView() => throw new NotSupportedException($"{nameof(PanelView)} not configured for this test");
    public void ConfigureEmbededMode() { }
    public void ConfigureWindowMode() { }
}

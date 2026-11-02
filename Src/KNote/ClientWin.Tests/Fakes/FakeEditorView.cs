using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Tests.Fakes;

/// <summary>
/// IViewEditor&lt;T&gt; test double for editors whose view only maps the model (UserProfileCtrl,
/// ChangePasswordCtrl): keeps every message shown, and runs RefreshModelImpl as the view would copy its controls.
/// Also an IViewEditorEmbeddable&lt;T&gt;, for the editors shown in a tab of another one (RepositoryAiModelsCtrl).
/// </summary>
internal class FakeEditorView<T> : IViewEditor<T>, IViewEditorEmbeddable<T>
{
    public List<string> ShownInfo { get; } = new();
    public string? LastShownInfo => ShownInfo.LastOrDefault();
    public Action? RefreshModelImpl { get; set; }
    // The user's answer to a question (MessageBoxButtons.YesNo...); OK when not set.
    public Func<string, MessageBoxButtons, DialogResult>? ShowInfoImpl { get; set; }
    public int RefreshViewCount { get; private set; }

    public void ShowView() { }

    public Result<EControllerResult> ShowModalView() => new(EControllerResult.Executed);

    public void RefreshView() => RefreshViewCount++;
    public void OnClosingView() { }

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        ShownInfo.Add(info);
        return ShowInfoImpl?.Invoke(info, buttons) ?? DialogResult.OK;
    }

    public void RefreshModel() => RefreshModelImpl?.Invoke();

    public Control PanelView() => throw new NotSupportedException($"{nameof(PanelView)} not configured for this test");
    public void ConfigureEmbededMode() { }
    public void ConfigureWindowMode() { }
    public void CleanView() { }
    public void RefreshViewOnlyRequiredCtrl() => RefreshView();
}

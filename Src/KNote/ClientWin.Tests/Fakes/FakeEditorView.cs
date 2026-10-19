using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Tests.Fakes;

/// <summary>
/// IViewEditor&lt;T&gt; test double for editors whose view only maps the model (UserProfileCtrl,
/// ChangePasswordCtrl): keeps every message shown, and runs RefreshModelImpl as the view would copy its controls.
/// </summary>
internal class FakeEditorView<T> : IViewEditor<T>
{
    public List<string> ShownInfo { get; } = new();
    public string? LastShownInfo => ShownInfo.LastOrDefault();
    public Action? RefreshModelImpl { get; set; }

    public void ShowView() { }

    public Result<EControllerResult> ShowModalView() => new(EControllerResult.Executed);

    public void RefreshView() { }
    public void OnClosingView() { }

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        ShownInfo.Add(info);
        return DialogResult.OK;
    }

    public void RefreshModel() => RefreshModelImpl?.Invoke();
}

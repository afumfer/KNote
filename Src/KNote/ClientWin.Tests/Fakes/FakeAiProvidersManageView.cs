using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Tests.Fakes;

/// <summary>
/// IViewManageList&lt;AiProviderRef&gt; test double for the AI providers list opened from another use case: ShowModalImpl
/// does what the user would do while it is open (add, edit or remove providers).
/// </summary>
internal class FakeAiProvidersManageView : IViewManageList<AiProviderRef>
{
    public Action? ShowModalImpl { get; set; }
    public int ShowModalCount { get; private set; }

    public Control? PanelView() => null;
    public void ConfigureEmbededMode() { }
    public void ConfigureWindowMode() { }
    public void ShowView() { }
    public void OnClosingView() { }
    public void RefreshView() { }

    public Result<EControllerResult> ShowModalView()
    {
        ShowModalCount++;
        ShowModalImpl?.Invoke();
        return new(EControllerResult.Executed);
    }

    public void AddItem(AiProviderRef item) { }
    public void UpdateItem(AiProviderRef item) { }
    public void RemoveItem(AiProviderRef item) { }

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
        => DialogResult.OK;
}

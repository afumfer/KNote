using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Tests.Fakes;

/// <summary>IViewEditor&lt;RepositoryRef&gt; test double for RepositoryEditorCtrl tests.</summary>
internal class FakeRepositoryEditorView : IViewEditor<RepositoryRef>
{
    public string? LastShownInfo { get; private set; }

    public void ShowView() { }

    public Result<EControllerResult> ShowModalView() => new(EControllerResult.Executed);

    public void RefreshView() { }
    public void OnClosingView() { }

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        LastShownInfo = info;
        return DialogResult.OK;
    }

    public void CleanView() { }
    public void RefreshModel() { }
}

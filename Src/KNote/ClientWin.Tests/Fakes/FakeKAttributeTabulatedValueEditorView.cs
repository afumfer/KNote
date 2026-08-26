using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests.Fakes;

/// <summary>IViewEditor&lt;KAttributeTabulatedValueDto&gt; test double for AttributeEditorCtrl's tabulated-value flows.</summary>
internal class FakeKAttributeTabulatedValueEditorView : IViewEditor<KAttributeTabulatedValueDto>
{
    public string? LastShownInfo { get; private set; }
    public Func<Result<EControllerResult>>? ShowModalViewImpl { get; set; }

    public void ShowView() { }

    public Result<EControllerResult> ShowModalView() =>
        (ShowModalViewImpl ?? (() => new Result<EControllerResult>(EControllerResult.Executed)))();

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

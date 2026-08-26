using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests.Fakes;

/// <summary>IViewManageList&lt;KAttributeInfoDto&gt; test double for KAttributesManageCtrl tests.</summary>
internal class FakeKAttributesManageView : IViewManageList<KAttributeInfoDto>
{
    public string? LastShownInfo { get; private set; }
    public bool RefreshViewCalled { get; private set; }
    public List<KAttributeInfoDto> AddedItems { get; } = new();
    public List<KAttributeInfoDto> UpdatedItems { get; } = new();
    public List<KAttributeInfoDto> RemovedItems { get; } = new();

    public Control? PanelView() => null;
    public void ConfigureEmbededMode() { }
    public void ConfigureWindowMode() { }
    public void ShowView() { }
    public Result<EControllerResult> ShowModalView() => new(EControllerResult.Executed);
    public void OnClosingView() { }

    public void RefreshView() => RefreshViewCalled = true;
    public void AddItem(KAttributeInfoDto item) => AddedItems.Add(item);
    public void UpdateItem(KAttributeInfoDto item) => UpdatedItems.Add(item);
    public void RemoveItem(KAttributeInfoDto item) => RemovedItems.Add(item);

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        LastShownInfo = info;
        return DialogResult.OK;
    }
}

using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests.Fakes;

/// <summary>IViewManageList&lt;UserDto&gt; test double for UsersManageCtrl tests.</summary>
internal class FakeUsersManageView : IViewManageList<UserDto>
{
    public string? LastShownInfo { get; private set; }
    public bool RefreshViewCalled { get; private set; }
    public List<UserDto> AddedItems { get; } = new();
    public List<UserDto> UpdatedItems { get; } = new();
    public List<UserDto> RemovedItems { get; } = new();

    public Control? PanelView() => null;
    public void ConfigureEmbededMode() { }
    public void ConfigureWindowMode() { }
    public void ShowView() { }
    public Result<EControllerResult> ShowModalView() => new(EControllerResult.Executed);
    public void OnClosingView() { }

    public void RefreshView() => RefreshViewCalled = true;
    public void AddItem(UserDto item) => AddedItems.Add(item);
    public void UpdateItem(UserDto item) => UpdatedItems.Add(item);
    public void RemoveItem(UserDto item) => RemovedItems.Add(item);

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        LastShownInfo = info;
        return DialogResult.OK;
    }
}

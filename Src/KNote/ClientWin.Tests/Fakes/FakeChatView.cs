using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests.Fakes;

/// <summary>IViewChatRoom test double for KntChatCtrl tests: records what the ctrl asks it to show.</summary>
internal class FakeChatView : IViewChatRoom
{
    public int ShowViewCallCount { get; private set; }
    public List<bool> VisibleViewCalls { get; } = new();
    public string? LastShownInfo { get; private set; }
    public int ShowInfoCallCount { get; private set; }
    public int RefreshThreadsCallCount { get; private set; }
    public int RefreshConversationCallCount { get; private set; }
    public int RefreshConnectionStateCallCount { get; private set; }
    public int RefreshOnlineUsersCallCount { get; private set; }
    public List<ChatRoomMessageDto> AppendedMessages { get; } = new();

    public void ShowView() => ShowViewCallCount++;

    public Result<EControllerResult> ShowModalView() => new(EControllerResult.Executed);

    public void RefreshView() { }
    public void OnClosingView() { }

    public void VisibleView(bool visible) => VisibleViewCalls.Add(visible);

    public void RefreshThreads() => RefreshThreadsCallCount++;
    public void RefreshConversation() => RefreshConversationCallCount++;
    public void AppendMessage(ChatRoomMessageDto message) => AppendedMessages.Add(message);
    public void RefreshOnlineUsers() => RefreshOnlineUsersCallCount++;
    public void RefreshConnectionState() => RefreshConnectionStateCallCount++;

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        ShowInfoCallCount++;
        LastShownInfo = info;
        return DialogResult.OK;
    }
}

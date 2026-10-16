using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Utils;
using KNote.Model;
using KNote.Model.Dto;
using KntIcons;

namespace KNote.ClientWin.Views;

// The chat room (KntChatCtrl), laid out as the AI assistant: the conversation of the thread shown (WebView2, see
// ChatRoomWebRenderer) over the message box, and the threads on the right. Closing it only hides it: the ctrl keeps
// receiving messages while the app is open, and shows it again when one arrives.
public partial class KntChatForm : KntForm, IViewChatRoom
{
    #region Private fields

    private readonly KntChatCtrl _ctrl;
    private readonly ChatRoomWebRenderer _conversation;
    // The menu items with a keyboard shortcut, by its text (AiChatHtml.ShortcutText).
    private readonly Dictionary<string, ToolStripMenuItem> _menuShortcuts;
    private const string ViewCaptionText = "Chat room";
    private const string NoThreadText = "Choose a thread on the right to read it and write in it, or start a new one with New thread.";
    // Part of the window the threads panel takes the first time (later, the width the user left).
    private const double ThreadsPanelShare = 0.25;
    // The threads list is being filled or its selection set from code: not the user choosing a thread.
    private bool _syncingThreads;
    private bool _boundsRestored;
    private bool _panelsLaidOut;
    // The thread on screen, to tell a new thread from the same one with new messages.
    private Guid? _shownThreadId;

    #endregion

    #region Constructor

    public KntChatForm(KntChatCtrl ctrl)
    {
        InitializeComponent();

        buttonSend.SetKntIcon(KntIcon.Send);
        buttonNewThread.SetKntIcon(KntIcon.Add);
        buttonUnfollow.SetKntIcon(KntIcon.Close);

        _ctrl = ctrl;

        ListViewStyle.ApplyStandard(listViewThreads);
        listViewThreads.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        listViewThreads.ShowItemToolTips = true;
        listViewThreads.Columns.Add("Thread", LogicalToDeviceUnits(140), HorizontalAlignment.Left);
        listViewThreads.Columns.Add("New", LogicalToDeviceUnits(38), HorizontalAlignment.Right);
        listViewThreads.Columns.Add("Activity", LogicalToDeviceUnits(70), HorizontalAlignment.Left);
        ListViewColumnResizer.Attach(listViewThreads, 0);

        _menuShortcuts = MenuItems(menuChatRoom.Items)
            .Where(item => item.ShortcutKeys != Keys.None)
            .ToDictionary(item => AiChatHtml.ShortcutText(item.ShortcutKeys));
        _conversation = new ChatRoomWebRenderer(kntEditViewConversation, _ctrl.Store.KNoteWebViewStyle, _menuShortcuts.Keys);
        _conversation.ShortcutPressed += Conversation_ShortcutPressed;
        kntEditViewConversation.NavigationBorder = true;

        // The Actions menu repeats the buttons, running the same handlers: its items follow their enabled state.
        LinkMenuToControl(menuSend, buttonSend);
        LinkMenuToControl(menuNewThread, buttonNewThread);
        LinkMenuToControl(menuUnfollow, buttonUnfollow);

        // Anchor=Right is not reliable for controls nested inside a SplitContainer panel when AutoScaleMode rescales
        // the form (see KNoteAIAssistantForm): the header's own Resize places them.
        panelConversationHeader.Resize += (s, e) => AlignConversationHeader();
        panelMessageHeader.Resize += (s, e) => AlignControlsRight(panelMessageHeader, 6, 6, buttonSend);
        panelThreadsHeader.Resize += (s, e) => AlignControlsRight(panelThreadsHeader, 6, 6, buttonNewThread);
        AlignConversationHeader();
        AlignControlsRight(panelMessageHeader, 6, 6, buttonSend);
        AlignControlsRight(panelThreadsHeader, 6, 6, buttonNewThread);
    }

    #endregion

    #region IViewChatRoom implementation

    public override void ShowView()
    {
        // Before the first Show(): StartPosition is still CenterScreen then (see KNoteAIAssistantForm).
        if (!_boundsRestored)
        {
            RestoreWindowBounds();
            _boundsRestored = true;
        }

        Show();

        if (!_panelsLaidOut)
        {
            LayOutPanels();
            _panelsLaidOut = true;
        }

        RefreshView();
    }

    public override void RefreshView()
    {
        RefreshConnectionState();
        RefreshThreads();
        RefreshConversation();
    }

    public void VisibleView(bool visible)
    {
        if (visible)
        {
            if (!Visible)
                Show();
            if (WindowState == FormWindowState.Minimized)
                WindowState = FormWindowState.Normal;
        }
        else
            Hide();
    }

    public void RefreshThreads()
    {
        _syncingThreads = true;
        listViewThreads.BeginUpdate();
        try
        {
            listViewThreads.Items.Clear();
            foreach (var thread in _ctrl.Threads)
            {
                var unread = _ctrl.UnreadCount(thread.ThreadId);
                var followed = _ctrl.IsFollowed(thread.ThreadId);
                var item = new ListViewItem(thread.Topic)
                {
                    Tag = thread.ThreadId,
                    ToolTipText = $"#{thread.NoteNumber} {thread.Topic}{(followed ? " (followed: its new messages arrive here)" : "")}",
                    Selected = thread.ThreadId == _ctrl.CurrentThreadId
                };
                item.SubItems.Add(unread > 0 ? (unread > 99 ? "99+" : unread.ToString()) : "");
                item.SubItems.Add(ActivityText(thread.ModificationDateTime));
                if (unread > 0)
                    item.Font = new Font(listViewThreads.Font, FontStyle.Bold);
                listViewThreads.Items.Add(item);
            }
            listViewThreads.SelectedItems.Cast<ListViewItem>().FirstOrDefault()?.EnsureVisible();
        }
        finally
        {
            listViewThreads.EndUpdate();
            _syncingThreads = false;
        }

        labelThreads.Text = _ctrl.Threads.Count > 0 ? $"Threads ({_ctrl.Threads.Count}):" : "Threads:";
    }

    public void RefreshConversation()
    {
        var thread = _ctrl.CurrentThread;
        _shownThreadId = thread?.ThreadId;

        labelThread.Text = thread == null ? "No thread selected" : $"#{thread.NoteNumber}  {thread.Topic}";
        Text = thread == null ? ViewCaptionText : $"{ViewCaptionText} - {thread.Topic}";
        RefreshOnlineUsers();
        UpdateActions();
        ShowConversation();
    }

    public void AppendMessage(ChatRoomMessageDto message)
    {
        if (message.ThreadId != _shownThreadId || !_conversation.Append(message))
            ShowConversation();
    }

    public void RefreshOnlineUsers()
    {
        var thread = _ctrl.CurrentThread;
        labelOnline.Text = thread == null || thread.OnlineUserNames.Count == 0
            ? ""
            : $"● {thread.OnlineUserNames.Count} online: {string.Join(", ", thread.OnlineUserNames)}";
        AlignConversationHeader();
    }

    public void RefreshConnectionState()
    {
        toolStripStatusConnection.Text = _ctrl.ConnectionState switch
        {
            ChatRoomConnectionState.Connected => " Connected",
            ChatRoomConnectionState.Connecting => " Connecting ...",
            ChatRoomConnectionState.Reconnecting => " Reconnecting ...",
            ChatRoomConnectionState.NotAuthorized => " Not allowed: sign in again from Options",
            _ => string.IsNullOrEmpty(_ctrl.ConnectionError) ? " Disconnected" : $" Disconnected: {_ctrl.ConnectionError}"
        };
        toolStripStatusUser.Text = string.IsNullOrEmpty(_ctrl.UserName) ? "Not signed in" : _ctrl.UserName;
        toolStripStatusServer.Text = _ctrl.ServerUrl;
        UpdateActions();
    }

    #endregion

    #region Form events handlers

    private void ChatForm_Load(object sender, EventArgs e)
    {
        UpdateActions();
    }

    // The app is closing (the ctrl finalizes the view): the window's layout is kept if it was on screen.
    public override void OnClosingView()
    {
        if (Visible)
            SaveWindowState();
        base.OnClosingView();
    }

    protected override void OnUserClosing(FormClosingEventArgs e)
    {
        SaveWindowState();

        if (_ctrl.AutoCloseCtrlOnViewExit)
            _ctrl.Finalize();
        // Not at Windows shutdown: cancelling would make Windows warn that the app is blocking it.
        else if (e.CloseReason != CloseReason.WindowsShutDown)
        {
            Hide();
            e.Cancel = true;
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _conversation.Dispose();
        base.OnFormClosed(e);
    }

    private async void buttonSend_Click(object sender, EventArgs e)
    {
        if (!buttonSend.Enabled || string.IsNullOrWhiteSpace(textMessage.Text))
            return;

        var text = textMessage.Text;
        buttonSend.Enabled = false;
        try
        {
            // The text stays in the box until it is sent: a failure doesn't lose it.
            if (await _ctrl.SendMessageAsync(text))
                textMessage.Text = "";
        }
        catch (Exception ex)
        {
            KntMessageBox.Show(ex.Message);
        }
        finally
        {
            UpdateActions();
            ActiveControl = textMessage;
        }
    }

    // Enter sends, Shift+Enter is a new line (as in the Web chat room).
    private void textMessage_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter || e.Shift || e.Control || e.Alt)
            return;

        e.SuppressKeyPress = true;
        buttonSend_Click(sender, e);
    }

    private async void buttonNewThread_Click(object sender, EventArgs e)
    {
        using var form = new ChatRoomThreadNewForm();
        if (form.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            await _ctrl.CreateThreadAsync(form.Topic);
            ActiveControl = textMessage;
        }
        catch (Exception ex)
        {
            KntMessageBox.Show(ex.Message);
        }
    }

    private async void buttonUnfollow_Click(object sender, EventArgs e)
    {
        try
        {
            await _ctrl.UnfollowCurrentThreadAsync();
        }
        catch (Exception ex)
        {
            KntMessageBox.Show(ex.Message);
        }
    }

    private async void menuReconnect_Click(object sender, EventArgs e)
    {
        try
        {
            await _ctrl.ReconnectAsync();
        }
        catch (Exception ex)
        {
            KntMessageBox.Show(ex.Message);
        }
    }

    private async void listViewThreads_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
    {
        if (_syncingThreads || !e.IsSelected || e.Item.Tag is not Guid threadId || threadId == _ctrl.CurrentThreadId)
            return;

        try
        {
            if (!await _ctrl.SelectThreadAsync(threadId))
                BeginInvoke(RefreshThreads);
            else
                ActiveControl = textMessage;
        }
        catch (Exception ex)
        {
            KntMessageBox.Show(ex.Message);
        }
    }

    // A menu shortcut pressed while the conversation had the focus: run it as the menu would have, only if the item
    // and its menus are enabled.
    private void Conversation_ShortcutPressed(object sender, string shortcut)
    {
        if (!_menuShortcuts.TryGetValue(shortcut, out var item))
            return;

        for (ToolStripItem i = item; i != null; i = i.OwnerItem)
            if (!i.Enabled)
                return;

        item.PerformClick();
    }

    #endregion

    #region Private methods

    private static IEnumerable<ToolStripMenuItem> MenuItems(ToolStripItemCollection items)
        => items.OfType<ToolStripMenuItem>().SelectMany(item => MenuItems(item.DropDownItems).Prepend(item));

    private static void LinkMenuToControl(ToolStripMenuItem item, Control control)
    {
        item.Enabled = control.Enabled;
        control.EnabledChanged += (s, e) => item.Enabled = control.Enabled;
    }

    private static void AlignControlsRight(Control header, int rightMargin, int spacing, params Control[] controlsLeftToRight)
    {
        int right = header.Width - rightMargin;
        for (int i = controlsLeftToRight.Length - 1; i >= 0; i--)
        {
            Control c = controlsLeftToRight[i];
            c.Left = right - c.Width;
            right = c.Left - spacing;
        }
    }

    // The time of today's activity, the date of older one: short enough for a narrow column.
    private static string ActivityText(DateTime modification) =>
        modification.Date == DateTime.Today ? modification.ToString("t") : modification.ToString("d");

    // The online users and the unfollow button on the right; the thread's title takes the rest.
    private void AlignConversationHeader()
    {
        AlignControlsRight(panelConversationHeader, 6, 10, labelOnline, buttonUnfollow);
        labelThread.Width = Math.Max(LogicalToDeviceUnits(80), labelOnline.Left - labelThread.Left - LogicalToDeviceUnits(10));
    }

    private void UpdateActions()
    {
        var connected = _ctrl.ConnectionState == ChatRoomConnectionState.Connected;
        var thread = _ctrl.CurrentThread != null;
        buttonSend.Enabled = connected && thread;
        textMessage.Enabled = connected && thread;
        buttonNewThread.Enabled = connected;
        buttonUnfollow.Enabled = connected && thread;
        buttonUnfollow.Visible = thread;
        menuReconnect.Enabled = _ctrl.ConnectionState is ChatRoomConnectionState.Disconnected or ChatRoomConnectionState.NotAuthorized;
    }

    // For the synchronous refreshes.
    private async void ShowConversation()
    {
        try
        {
            var thread = _ctrl.CurrentThread;
            await _conversation.LoadAsync(thread?.Messages ?? new List<ChatRoomMessageDto>(), _ctrl.UserName,
                thread == null ? NoThreadText : "No messages yet: write the first one.");
            // Loading the page moves the focus into WebView2: back to the message box, to type in it right away.
            if (textMessage.Enabled && ContainsFocus)
                ActiveControl = textMessage;
        }
        catch (Exception ex)
        {
            KntMessageBox.Show(ex.Message);
        }
    }

    // The saved position can be off every current screen (monitor or Windows scale changed since it was saved).
    private void RestoreWindowBounds()
    {
        var bounds = _ctrl.Store.State.ChatRoomWindow.Bounds;
        if (bounds.Width > 0)
            Size = new Size(bounds.Width, bounds.Height);
        if (bounds.X != 0 || bounds.Y != 0)
        {
            StartPosition = FormStartPosition.Manual;
            Bounds = WindowPlacement.EnsureVisible(new Rectangle(new Point(bounds.X, bounds.Y), Size));
        }
    }

    // In code: the Designer's SplitterDistance is not rescaled with the window. The threads take a quarter of the
    // window the first time; then, the width the user left.
    private void LayOutPanels()
    {
        var state = _ctrl.Store.State.ChatRoomWindow;
        var threadsWidth = state.ThreadsPanelWidth > 0 ? state.ThreadsPanelWidth : (int)(splitThreads.Width * ThreadsPanelShare);
        SetSplitterDistance(splitThreads, splitThreads.Width - splitThreads.SplitterWidth - threadsWidth);
        var messageHeight = state.MessagePanelHeight > 0 ? state.MessagePanelHeight : LogicalToDeviceUnits(124);
        SetSplitterDistance(splitChat, splitChat.Height - splitChat.SplitterWidth - messageHeight);
    }

    // RestoreBounds keeps the normal-state bounds while the window is maximized or minimized. Minimized, the panels
    // may have been laid out to the collapsed window: their sizes aren't kept.
    private void SaveWindowState()
    {
        var state = _ctrl.Store.State.ChatRoomWindow;
        var bounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
        state.Bounds.X = bounds.X;
        state.Bounds.Y = bounds.Y;
        state.Bounds.Width = bounds.Width;
        state.Bounds.Height = bounds.Height;
        if (WindowState != FormWindowState.Minimized)
        {
            state.ThreadsPanelWidth = splitThreads.Panel2.Width;
            state.MessagePanelHeight = splitChat.Panel2.Height;
        }
        _ctrl.Store.SaveConfig();
    }

    // Kept within the panels' minimum sizes: the saved size may not fit a smaller window.
    private static void SetSplitterDistance(SplitContainer split, int distance)
    {
        int length = split.Orientation == Orientation.Vertical ? split.Width : split.Height;
        int max = length - split.SplitterWidth - split.Panel2MinSize;
        split.SplitterDistance = Math.Max(split.Panel1MinSize, Math.Min(distance, max));
    }

    #endregion
}

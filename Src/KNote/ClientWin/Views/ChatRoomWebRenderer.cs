using System.Diagnostics;
using KNote.ClientWin.Core;
using KNote.Model.Dto;
using KntWebView;
using Microsoft.Web.WebView2.Core;

namespace KNote.ClientWin.Views;

// Conversation of a chat room thread on the WebView2 of a KntEditView. The page (ChatRoomHtml.Page) is loaded with
// the thread's messages when a thread is shown, and each new message is added to it by script: reloading the page
// for every message would flicker and get slower as the conversation grows.
internal sealed class ChatRoomWebRenderer : IDisposable
{
    private static readonly TimeSpan PageLoadTimeout = TimeSpan.FromSeconds(10);

    private readonly KntEditView _editView;
    private readonly string _markdownStyle;
    private readonly IReadOnlyCollection<string> _shortcuts;

    private CoreWebView2 _core;
    private TaskCompletionSource _pageLoaded;
    // The page is loaded and shows exactly the thread: scripts can add to it.
    private bool _pageInSync;
    private string _userName;
    private DateTime? _lastSent;

    // shortcuts: the host's keyboard shortcuts (AiChatHtml.ShortcutText), raised by ShortcutPressed when they are
    // pressed while the conversation has the focus.
    public ChatRoomWebRenderer(KntEditView editView, string markdownStyle, IReadOnlyCollection<string> shortcuts)
    {
        _editView = editView;
        _markdownStyle = markdownStyle;
        _shortcuts = shortcuts;
        _editView.WebViewControl.NavigationCompleted += (s, e) => _pageLoaded?.TrySetResult();
    }

    // A host shortcut pressed while the conversation had the focus: the keys go to the browser, not to the window.
    // Raised outside the WebView2 callback, so its handler may open a modal dialog.
    public event EventHandler<string> ShortcutPressed;

    // Shows the messages, replacing whatever the view had. userName: the app's user (their messages on the right).
    public async Task LoadAsync(IReadOnlyList<ChatRoomMessageDto> messages, string userName, string emptyText)
    {
        _userName = userName;
        _lastSent = messages.Count > 0 ? messages[^1].SentDateTime : null;
        _pageInSync = false;
        // Completed from the NavigationCompleted callback of WebView2: what follows must not run inside it.
        var pageLoaded = _pageLoaded = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        await _editView.ShowNavigationContent(ChatRoomHtml.Page(messages, userName, _markdownStyle, emptyText, _shortcuts));

        // Out of a possible WebView2 callback before going on: the caller may show a message box.
        await Task.Yield();

        // Null when WebView2 could not be initialized: KntEditView has already reported it.
        var core = _editView.WebViewControl.CoreWebView2;
        if (core == null)
            return;
        HookEvents(core);

        _pageInSync = await Task.WhenAny(pageLoaded.Task, Task.Delay(PageLoadTimeout)) == pageLoaded.Task;
    }

    // A new message at the end of the conversation shown. False if the page couldn't take it (the caller reloads it).
    public bool Append(ChatRoomMessageDto message)
    {
        if (!_pageInSync || _core == null)
            return false;

        var html = ChatRoomHtml.Message(message, _userName, _lastSent);
        _lastSent = message.SentDateTime;
        _ = RunAsync(_core, ChatRoomHtml.ScriptCall("append", html));
        return true;
    }

    public void Dispose()
    {
        if (_core != null)
        {
            _core.NavigationStarting -= Core_NavigationStarting;
            _core.NewWindowRequested -= Core_NewWindowRequested;
            _core.WebMessageReceived -= Core_WebMessageReceived;
        }
    }

    // Not awaited: WebView2 runs the scripts in the order they are sent, and awaiting them would resume the caller
    // inside a WebView2 callback.
    private static async Task RunAsync(CoreWebView2 core, string script)
    {
        try
        {
            await core.ExecuteScriptAsync(script);
        }
        catch (Exception)
        {
            // The page is being replaced, or its browser process failed (KntEditView reports the latter).
        }
    }

    private void HookEvents(CoreWebView2 core)
    {
        if (core == _core)
            return;

        _core = core;
        core.NavigationStarting += Core_NavigationStarting;
        core.NewWindowRequested += Core_NewWindowRequested;
        core.WebMessageReceived += Core_WebMessageReceived;
    }

    // The page only posts the host's shortcuts (see ChatRoomHtml); anything else is ignored.
    private void Core_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        string shortcut;
        try
        {
            shortcut = e.TryGetWebMessageAsString();
        }
        catch (ArgumentException)
        {
            return;  // Not a string.
        }

        if (_shortcuts.Contains(shortcut))
            _editView.BeginInvoke(() => ShortcutPressed?.Invoke(this, shortcut));
    }

    // The conversation is a page built in memory: a web or mail link of a message opens in the default browser
    // instead of navigating the view away from it. The page itself (NavigateToString) is neither, so it goes on loading.
    private void Core_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
    {
        if (!IsExternalLink(e.Uri))
            return;

        e.Cancel = true;
        OpenExternal(e.Uri);
    }

    private void Core_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
    {
        e.Handled = true;
        if (IsExternalLink(e.Uri))
            OpenExternal(e.Uri);
    }

    private static bool IsExternalLink(string uri)
        => Uri.TryCreate(uri, UriKind.Absolute, out var parsed)
            && (parsed.Scheme == Uri.UriSchemeHttps || parsed.Scheme == Uri.UriSchemeHttp || parsed.Scheme == Uri.UriSchemeMailto);

    private static void OpenExternal(string uri)
    {
        try
        {
            Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
        }
        catch (Exception)
        {
            // No application for it: the click just does nothing (no message box inside a WebView2 callback).
        }
    }
}

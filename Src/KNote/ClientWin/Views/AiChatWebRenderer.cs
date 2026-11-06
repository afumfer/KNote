using System.Diagnostics;
using KNote.ClientWin.Core;
using KNote.Model.Dto;
using KntWebView;
using Microsoft.Web.WebView2.Core;

namespace KNote.ClientWin.Views;

// Chat view of the AI assistant on the WebView2 of a KntEditView. The page (AiChatHtml.Page) is loaded once
// with the conversation so far, and each new turn is added to it by script, the answer being repainted as it
// streams in: reloading the whole page for every chunk would flicker, lose the scroll position and get slower
// as the conversation grows.
internal sealed class AiChatWebRenderer : IDisposable
{
    // Often enough for the answer to flow, seldom enough not to convert and replace it for every chunk.
    private const int RepaintIntervalMs = 100;

    private static readonly TimeSpan PageLoadTimeout = TimeSpan.FromSeconds(10);

    private readonly KntEditView _editView;
    private readonly string _markdownStyle;
    private readonly IReadOnlyCollection<string> _shortcuts;
    private readonly System.Windows.Forms.Timer _repaintTimer;

    private CoreWebView2 _core;
    private TaskCompletionSource _pageLoaded;
    // The page is loaded and shows exactly the conversation: scripts can add to it.
    private bool _pageInSync;

    private string _providerAlias;
    private Func<string> _answerSoFar;
    private Func<string> _reasoningSoFar;
    private string _painted;
    private string _activity;
    private bool _turnRunning;
    private bool _showModelInfo = true;

    // shortcuts: the host's keyboard shortcuts (AiChatHtml.ShortcutText), raised by ShortcutPressed when they
    // are pressed while the chat has the focus.
    public AiChatWebRenderer(KntEditView editView, string markdownStyle, IReadOnlyCollection<string> shortcuts)
    {
        _editView = editView;
        _markdownStyle = markdownStyle;
        _shortcuts = shortcuts;
        _repaintTimer = new System.Windows.Forms.Timer { Interval = RepaintIntervalMs };
        _repaintTimer.Tick += (s, e) => RepaintAnswer();
        _editView.WebViewControl.NavigationCompleted += (s, e) => _pageLoaded?.TrySetResult();
    }

    // A host shortcut pressed while the chat had the focus: the keys go to the browser, not to the window.
    // Raised outside the WebView2 callback, so its handler may open a modal dialog.
    public event EventHandler<string> ShortcutPressed;

    // The answer to a card of actions. Raised outside the WebView2 callback, as ShortcutPressed.
    public event EventHandler<AiChatHtml.ActionsAnswer> ActionsAnswered;

    // Shows the whole conversation, replacing whatever the view had. providerAlias: the provider of its answers.
    public async Task LoadAsync(IEnumerable<AiChatTurnDto> turns, string providerAlias)
    {
        _providerAlias = providerAlias;
        _repaintTimer.Stop();
        _answerSoFar = null;
        _pageInSync = false;
        // Completed from the NavigationCompleted callback of WebView2: what follows must not run inside it.
        var pageLoaded = _pageLoaded = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        await _editView.ShowNavigationContent(AiChatHtml.Page(turns, providerAlias, _markdownStyle, _showModelInfo, _shortcuts));

        // Out of a possible WebView2 callback before going on: the caller may show a message box (see the
        // WebView2 notes of ClientWin/CLAUDE.md).
        await Task.Yield();

        // Null when WebView2 could not be initialized: KntEditView has already reported it.
        var core = _editView.WebViewControl.CoreWebView2;
        if (core == null)
            return;
        HookEvents(core);

        _pageInSync = await Task.WhenAny(pageLoaded.Task, Task.Delay(PageLoadTimeout)) == pageLoaded.Task;
    }

    // A new turn: the question and, below it, the place of its answer (a typing indicator until it arrives).
    // turns is the conversation before this turn, to reload the page first if it is not in step with it
    // (after a failed turn). answerSoFar, for a streamed answer, returns the text received so far: it is
    // repainted periodically until EndTurn/FailTurn, as reasoningSoFar, with what has arrived of the reasoning.
    public async Task BeginTurnAsync(IEnumerable<AiChatTurnDto> turns, string prompt, string providerAlias, Func<string> answerSoFar,
        Func<string> reasoningSoFar = null)
    {
        if (!_pageInSync)
            await LoadAsync(turns, providerAlias);

        _providerAlias = providerAlias;
        _answerSoFar = answerSoFar;
        _reasoningSoFar = reasoningSoFar;
        _painted = null;
        _activity = null;
        _turnRunning = true;

        // Without a prompt, a turn the app sends by itself.
        Run(AiChatHtml.ScriptCall("append", (prompt == null ? "" : AiChatHtml.UserMessage(prompt)) + AiChatHtml.PendingAnswer(providerAlias, null)));

        if (answerSoFar != null || reasoningSoFar != null)
            _repaintTimer.Start();
    }

    // The conversation changed in what is already painted (a card rejected by a new message): the next turn loads it again.
    public void Invalidate() => _pageInSync = false;

    // The last answer again, once its card is answered.
    public void ReplaceLastTurn(AiChatTurnDto turn) =>
        Run(AiChatHtml.ScriptCall("replaceLast", AiChatHtml.AssistantMessage(turn, _providerAlias)));

    // What the model is doing (null: nothing to say) under the answer arriving.
    public void ShowActivity(string activity)
    {
        _activity = activity;
        if (_turnRunning)
            PaintPending(_answerSoFar?.Invoke(), _reasoningSoFar?.Invoke());
    }

    private void PaintPending(string answer, string reasoning) =>
        Run(AiChatHtml.ScriptCall("replaceLast", AiChatHtml.PendingAnswer(_providerAlias, answer, _activity, reasoning)));

    public void EndTurn(AiChatTurnDto turn)
    {
        _repaintTimer.Stop();
        _turnRunning = false;
        _answerSoFar = null;
        Run(AiChatHtml.ScriptCall("replaceLast", AiChatHtml.AssistantMessage(turn, _providerAlias)));
    }

    // The turn stays on screen, marked as failed, until the next one reloads the page without it: the ctrl
    // has already removed it from the conversation.
    public void FailTurn()
    {
        _repaintTimer.Stop();
        _turnRunning = false;
        Run(AiChatHtml.ScriptCall("replaceLast", AiChatHtml.FailedAnswer(_providerAlias, _answerSoFar?.Invoke())));
        _answerSoFar = null;
        _pageInSync = false;
    }

    // Shows or hides the usage line of the answers, on the page shown (no reload) and on the next loads.
    public void ShowModelInfo(bool visible)
    {
        _showModelInfo = visible;
        Run($"window.kntChat.showModelInfo({(visible ? "true" : "false")});");
    }

    public void Dispose()
    {
        _repaintTimer.Dispose();
        if (_core != null)
        {
            _core.NavigationStarting -= Core_NavigationStarting;
            _core.NewWindowRequested -= Core_NewWindowRequested;
            _core.WebMessageReceived -= Core_WebMessageReceived;
        }
    }

    private void RepaintAnswer()
    {
        var answer = _answerSoFar?.Invoke();
        var reasoning = _reasoningSoFar?.Invoke();
        var painted = answer + "\0" + reasoning;
        if ((string.IsNullOrEmpty(answer) && string.IsNullOrEmpty(reasoning)) || painted == _painted)
            return;

        _painted = painted;
        PaintPending(answer, reasoning);
    }

    // Not awaited: WebView2 runs the scripts in the order they are sent, and awaiting them would resume the
    // caller inside a WebView2 callback.
    private void Run(string script)
    {
        if (!_pageInSync || _core == null)
            return;

        _ = RunAsync(_core, script);
    }

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

    // The page only posts the host's shortcuts and the answer to a card (see AiChatHtml); anything else is ignored.
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

        if (AiChatHtml.TryReadActionsAnswer(shortcut, out var answer))
            _editView.BeginInvoke(() => ActionsAnswered?.Invoke(this, answer));
        else if (_shortcuts.Contains(shortcut))
            _editView.BeginInvoke(() => ShortcutPressed?.Invoke(this, shortcut));
    }

    // The chat is a page built in memory: a web or mail link of an answer opens in the default browser
    // instead of navigating the view away from the conversation. The page itself (NavigateToString) is
    // neither, so it goes on loading.
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

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace KNote.Client.AppStoreService;

/// <summary>
/// The windows of the app, without its menus (KntWindowLayout), each one brought to the front if it is already open:
/// - where a note is viewed (notes/view/{id}), edited (notes/edit/{id}) or created (notes/new): one per note and use
///   (a new note gets a new window, which becomes the note's edit window once saved);
/// - the AI assistant (aiassistant/window) and the chat room (chat/window): one for each.
/// The windows of the app tell each other when a note is saved (NoteSaved), so that the lists show it changed.
/// </summary>
public class KntWindows : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private readonly NavigationManager _navigationManager;
    private readonly IStore _store;
    private IJSObjectReference? _module;
    private IJSObjectReference? _listener;
    private DotNetObjectReference<KntWindows>? _dotNetRef;

    public KntWindows(IJSRuntime js, NavigationManager navigationManager, IStore store)
    {
        _js = js;
        _navigationManager = navigationManager;
        _store = store;
    }

    /// <summary>A note was saved in another window of the app (its id).</summary>
    public event Action<Guid>? NoteSaved;

    /// <summary>Hears NoteSaved. Called by the layouts: it also loads the script beforehand, since a window has to
    /// be opened right after the click that asks for it (or the browser blocks it).</summary>
    public async Task StartAsync()
    {
        var module = await GetModuleAsync();
        if (_listener != null)
            return;

        _dotNetRef = DotNetObjectReference.Create(this);
        _listener = await module.InvokeAsync<IJSObjectReference>("listenNoteSaved", _dotNetRef);
    }

    public Task<bool> OpenViewAsync(Guid noteId) =>
        OpenAsync($"notes/view/{noteId}", $"knt-note-view-{noteId}", 900, 800);

    public Task<bool> OpenEditAsync(Guid noteId) =>
        OpenAsync(EditPath(noteId), EditWindowName(noteId), 1200, 900);

    /// <summary>A new note, in the folder selected in the app (AppState.SelectedFolder, the tree panel's), if any.</summary>
    public Task<bool> OpenNewAsync()
    {
        var appState = _store.AppState;
        var path = appState.IsSelectedFolder() ? $"notes/new?folderId={appState.SelectedFolder!.FolderId}" : "notes/new";
        return OpenAsync(path, $"knt-note-new-{Guid.NewGuid():N}", 1200, 900);
    }

    /// <summary>The AI assistant, resuming the session given (a saved one), if any.</summary>
    public Task<bool> OpenAiAssistantAsync(Guid? sessionId) =>
        OpenAsync(WithId("aiassistant/window", "sessionId", sessionId), "knt-aiassistant", 1100, 850, "AI Assistant");

    /// <summary>The chat room, showing the thread given, if any.</summary>
    public Task<bool> OpenChatRoomAsync(Guid? threadId) =>
        OpenAsync(WithId("chat/window", "threadId", threadId), "knt-chatroom", 1100, 800, "Chat room");

    /// <summary>A new note was saved in this window: it becomes the note's edit window (OpenEditAsync brings it to the
    /// front, and reloading it loads the note).</summary>
    public async Task BecomeEditWindowAsync(Guid noteId) =>
        await (await GetModuleAsync()).InvokeVoidAsync("renameWindow", EditWindowName(noteId),
            _navigationManager.ToAbsoluteUri(EditPath(noteId)).ToString());

    /// <summary>Closes this window (or, if the browser doesn't let it, goes to Home).</summary>
    public async Task CloseAsync() =>
        await (await GetModuleAsync()).InvokeVoidAsync("closeWindow", _navigationManager.ToAbsoluteUri("index").ToString());

    /// <summary>Asks before the window is closed while the page's HasUnsavedChanges() says so. Dispose the
    /// result (InvokeVoidAsync("dispose")) when the page is disposed.</summary>
    public async Task<IJSObjectReference> WatchUnsavedChangesAsync<TPage>(DotNetObjectReference<TPage> page) where TPage : class =>
        await (await GetModuleAsync()).InvokeAsync<IJSObjectReference>("watchUnsavedChanges", page);

    /// <summary>Tells the other windows of the app that a note was saved.</summary>
    public async Task NotifyNoteSavedAsync(Guid noteId) =>
        await (await GetModuleAsync()).InvokeVoidAsync("notifyNoteSaved", noteId.ToString());

    [JSInvokable]
    public void OnNoteSaved(string noteId)
    {
        if (Guid.TryParse(noteId, out var id))
            NoteSaved?.Invoke(id);
    }

    private static string EditPath(Guid noteId) => $"notes/edit/{noteId}";

    private static string EditWindowName(Guid noteId) => $"knt-note-edit-{noteId}";

    private static string WithId(string path, string name, Guid? id) =>
        id is Guid value && value != Guid.Empty ? $"{path}?{name}={value}" : path;

    // title: what the window shows, for the notice when the browser blocks it.
    private async Task<bool> OpenAsync(string path, string name, int width, int height, string title = "Note")
    {
        var url = _navigationManager.ToAbsoluteUri(path).ToString();
        var opened = await (await GetModuleAsync()).InvokeAsync<bool>("openWindow", url, name, width, height);
        if (!opened)
            _store.AppState.NotifyError($"{title} window", "The browser blocked the window: allow pop-ups for this site.");
        return opened;
    }

    private async Task<IJSObjectReference> GetModuleAsync() =>
        _module ??= await _js.InvokeAsync<IJSObjectReference>("import", "./js/kntWindows.js");

    public async ValueTask DisposeAsync()
    {
        if (_listener != null)
        {
            await _listener.InvokeVoidAsync("dispose");
            await _listener.DisposeAsync();
        }
        if (_module != null)
            await _module.DisposeAsync();
        _dotNetRef?.Dispose();
    }
}

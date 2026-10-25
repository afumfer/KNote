using System.Security.Claims;
using System.Text.Json;
using KNote.Client.Helpers;
using KNote.Model;
using KNote.Model.Dto;
using Microsoft.JSInterop;

namespace KNote.Client.AppStoreService;

/// <summary>A notice of the alarms panel: the alarm (its current data) and when this browser was told about it.</summary>
public class AlarmNotice
{
    public AlarmNoticeDto Notice { get; set; } = new();

    public DateTime NotifiedAt { get; set; }
}

/// <summary>
/// The Application info alarms of the user, as notices that stay in the alarms panel until the user removes them
/// (AlarmNoticesPanel, opened from the header's bell). Started by MainLayout for a Staff user once signed in (and
/// stopped when signing out); the windows of the app (KntWindowLayout) don't start it. While started, it checks the
/// Server's alarms (api/alarms/check, each due alarm is given once, to one check) every so often (the Server says how
/// often). The notices are kept in the browser (localStorage, one list per repository and user: just the alarm's id
/// and when it was notified, the rest is asked to the Server, which leaves out the ones that no longer are the user's
/// notices), like ClientWin keeps them in the user's folder; so they are shared by the tabs of the app, which hear the
/// changes of the others (the storage event, js/kntAlarms.js). A new notice is told with Arrived, or as a notification
/// of the system while the app is not on screen (if the user allowed it). Its changes are told with Changed.
/// </summary>
public class AlarmNotices : IAsyncDisposable
{
    private const string StorageKeyPrefix = "KNoteAlarmNotices";
    private const int DefaultCheckSeconds = 60;

    private readonly IJSRuntime _js;
    private readonly IStore _store;
    private readonly DotNetObjectReference<AlarmNotices> _dotNetRef;
    private IJSObjectReference? _module;
    private IJSObjectReference? _storageListener;
    private CancellationTokenSource? _running;
    private string? _storageKey;

    public AlarmNotices(IJSRuntime js, IStore store)
    {
        _js = js;
        _store = store;
        _dotNetRef = DotNetObjectReference.Create(this);
    }

    #region State

    public bool Started => _running != null;

    // Most recent first.
    public List<AlarmNotice> Notices { get; private set; } = new();

    // Notification.permission: "default" (not asked yet), "granted", "denied" or "unsupported".
    public string NotificationPermission { get; private set; } = "unsupported";

    public event Action? Changed;

    // New notices, while the app is on screen (otherwise they are a notification of the system, if allowed).
    public event Action<List<AlarmNotice>>? Arrived;

    #endregion

    #region Start and stop

    /// <summary>Starts for the user, if they are signed in and a Staff user (the alarms are checked by Staff users,
    /// as in ClientWin), in the session's repository; otherwise stops.</summary>
    public async Task StartForUserAsync(ClaimsPrincipal user)
    {
        var repositoryId = user.FindFirst(KntConst.RepositoryClaimType)?.Value;
        if (user.Identity?.IsAuthenticated == true && !string.IsNullOrEmpty(user.Identity.Name) && repositoryId != null
            && (user.IsInRole("Staff") || user.IsInRole("ProjectManager") || user.IsInRole("Admin")))
            await StartAsync($"{StorageKeyPrefix}:{repositoryId}:{user.Identity.Name}");
        else
            await StopAsync();
    }

    private async Task StartAsync(string storageKey)
    {
        if (Started && _storageKey == storageKey)
            return;
        await StopAsync();

        _storageKey = storageKey;
        _running = new CancellationTokenSource();
        var module = await GetModuleAsync();
        if (module != null)
        {
            NotificationPermission = await module.InvokeAsync<string>("permission");
            _storageListener = await module.InvokeAsync<IJSObjectReference>("watchStorage", _dotNetRef, storageKey);
        }

        await LoadAsync(_running.Token);
        _ = CheckAsync(_running.Token);
    }

    public async Task StopAsync()
    {
        _running?.Cancel();
        _running = null;
        _storageKey = null;
        Notices = new();
        if (_storageListener != null)
        {
            await _storageListener.InvokeVoidAsync("dispose");
            await _storageListener.DisposeAsync();
            _storageListener = null;
        }
        NotifyChanged();
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        if (_module != null)
            await _module.DisposeAsync();
        _dotNetRef.Dispose();
    }

    #endregion

    #region Notices

    public async Task RemoveAsync(Guid messageId)
    {
        Notices = Notices.Where(n => n.Notice.KMessageId != messageId).ToList();
        var rows = await ReadRowsAsync();
        rows.RemoveAll(r => r.KMessageId == messageId);
        await WriteRowsAsync(rows);
        NotifyChanged();
    }

    public async Task RemoveAllAsync()
    {
        Notices = new();
        await WriteRowsAsync(new());
        NotifyChanged();
    }

    /// <summary>Asks the user to allow the notifications of the system (it must follow a click).</summary>
    public async Task RequestNotificationPermissionAsync()
    {
        if (await GetModuleAsync() is { } module)
            NotificationPermission = await module.InvokeAsync<string>("requestPermission");
        NotifyChanged();
    }

    // Another tab of the app changed the notices: this one shows them as they are now.
    [JSInvokable]
    public async Task OnStorageChanged()
    {
        if (_running is not { } running)
            return;

        var rows = await ReadRowsAsync();
        var kept = rows.Select(r => r.KMessageId).ToHashSet();
        var notices = Notices.Where(n => kept.Contains(n.Notice.KMessageId)).ToList();
        var missing = rows.Where(r => notices.All(n => n.Notice.KMessageId != r.KMessageId)).ToList();
        if (missing.Count > 0)
        {
            var res = await _store.Alarms.GetNoticesAsync(missing.Select(r => r.KMessageId).ToList(), running.Token);
            if (running.IsCancellationRequested)
                return;
            if (res.IsValid)
                notices.AddRange(res.Entity.Select(n => new AlarmNotice { Notice = n, NotifiedAt = missing.First(r => r.KMessageId == n.KMessageId).NotifiedAt }));
        }
        Notices = Sorted(notices);
        NotifyChanged();
    }

    // The notices kept, with their current data; those that no longer are the user's notices are forgotten.
    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        var rows = await ReadRowsAsync();
        if (rows.Count == 0)
        {
            NotifyChanged();
            return;
        }

        var res = await _store.Alarms.GetNoticesAsync(rows.Select(r => r.KMessageId).ToList(), cancellationToken);
        if (cancellationToken.IsCancellationRequested || !res.IsValid)
            return;

        Notices = Sorted(res.Entity.Select(n => new AlarmNotice { Notice = n, NotifiedAt = rows.First(r => r.KMessageId == n.KMessageId).NotifiedAt }));
        if (Notices.Count != rows.Count)
            await WriteRowsAsync(rows.Where(r => Notices.Any(n => n.Notice.KMessageId == r.KMessageId)).ToList());
        NotifyChanged();
    }

    // Every so often (the Server says how often), while started; a check that fails is tried again the next time.
    private async Task CheckAsync(CancellationToken cancellationToken)
    {
        var seconds = DefaultCheckSeconds;
        while (!cancellationToken.IsCancellationRequested)
        {
            var res = await _store.Alarms.CheckAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested)
                return;
            if (res.IsValid)
            {
                seconds = res.Entity.CheckSeconds > 0 ? res.Entity.CheckSeconds : DefaultCheckSeconds;
                try
                {
                    if (res.Entity.Notices.Count > 0)
                        await AddAsync(res.Entity.Notices);
                }
                catch (JSException)
                {
                    // The page is going away, or its script couldn't be run: the notices of this check are lost.
                }
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(seconds), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    private async Task AddAsync(List<AlarmNoticeDto> due)
    {
        var now = DateTime.Now;
        var rows = await ReadRowsAsync();
        var added = due.Where(n => rows.All(r => r.KMessageId != n.KMessageId))
            .Select(n => new AlarmNotice { Notice = n, NotifiedAt = now })
            .ToList();
        if (added.Count == 0)
            return;

        rows.AddRange(added.Select(n => new NoticeRow(n.Notice.KMessageId, n.NotifiedAt)));
        await WriteRowsAsync(rows);
        Notices = Sorted(Notices.Where(n => added.All(a => a.Notice.KMessageId != n.Notice.KMessageId)).Concat(added));
        NotifyChanged();

        var shownBySystem = false;
        if (await GetModuleAsync() is { } module)
            foreach (var notice in added)
                shownBySystem |= await module.InvokeAsync<bool>("notify", Title(notice.Notice), notice.Notice.Comment ?? "", notice.Notice.KMessageId.ToString());
        if (!shownBySystem)
            Arrived?.Invoke(added);
    }

    public static string Title(AlarmNoticeDto notice) => $"#{notice.NoteNumber} · {notice.NoteTopic}";

    private static List<AlarmNotice> Sorted(IEnumerable<AlarmNotice> notices) =>
        notices.OrderByDescending(n => n.NotifiedAt).ThenByDescending(n => n.Notice.AlarmDateTime).ToList();

    #endregion

    #region Browser storage

    private record NoticeRow(Guid KMessageId, DateTime NotifiedAt);

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private async Task<List<NoticeRow>> ReadRowsAsync()
    {
        if (_storageKey == null)
            return new();
        try
        {
            var json = await _js.LocalStorageGetItem(_storageKey);
            return string.IsNullOrEmpty(json) ? new() : JsonSerializer.Deserialize<List<NoticeRow>>(json, JsonOptions) ?? new();
        }
        catch (Exception ex) when (ex is JsonException or JSException)
        {
            return new();
        }
    }

    private async Task WriteRowsAsync(List<NoticeRow> rows)
    {
        if (_storageKey == null)
            return;
        try
        {
            if (rows.Count == 0)
                await _js.LocalStorageRemoveItem(_storageKey);
            else
                await _js.LocalStorageSetItem(_storageKey, JsonSerializer.Serialize(rows, JsonOptions));
        }
        catch (JSException)
        {
            // Storage not available (e.g. blocked by the browser): the notices are kept in this tab only.
        }
    }

    #endregion

    private async Task<IJSObjectReference?> GetModuleAsync() =>
        _module ??= await _js.ImportModuleAsync("./js/kntAlarms.js", _store.AppState);

    private void NotifyChanged() => Changed?.Invoke();
}

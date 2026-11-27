using System;
using System.Threading;
using KNote.Model;

namespace KNote.Service.Core;

/// <summary>
/// Where the changes come from, for the history (H11): IKntService.HistoryOrigin, the host's, unless a scope opened with Begin
/// says otherwise for what runs inside it (the assistant's actions and tools, KntScript, the message broker, background work on
/// a shared service). The scope follows the async call that opened it (AsyncLocal), as KntAuthorizationBypass, so it never
/// reaches other requests or the user's own edits in the meantime. Public: ClientWin opens it around a script and KNote.Ai
/// around its tools.
/// </summary>
public static class KntHistoryOrigin
{
    private static readonly AsyncLocal<EnumHistoryOrigin?> _origin = new();

    public static EnumHistoryOrigin Current(IKntService service) => _origin.Value ?? service?.HistoryOrigin ?? EnumHistoryOrigin.KNote;

    public static IDisposable Begin(EnumHistoryOrigin origin)
    {
        var previous = _origin.Value;
        _origin.Value = origin;
        return new Scope(previous);
    }

    private sealed class Scope : IDisposable
    {
        private readonly EnumHistoryOrigin? _previous;
        private bool _disposed;

        public Scope(EnumHistoryOrigin? previous)
        {
            _previous = previous;
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _origin.Value = _previous;
            _disposed = true;
        }
    }
}

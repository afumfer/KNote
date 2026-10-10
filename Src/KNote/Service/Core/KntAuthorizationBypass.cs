using System;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace KNote.Service.Core;

/// <summary>
/// Lets a command that has already passed its own authorization run other commands without checking the role
/// they require, for as long as the returned scope lives (see KntCommandServiceBase.ValidateAuthorizationAsync).
/// Only the role check is skipped: those commands still validate their params, apply their rules and raise
/// CommandExecuting/CommandExecuted. The scope follows the async call that opened it (AsyncLocal), so it never
/// reaches other requests or threads. Internal: only Service code can open it, never ClientWin, Server or
/// KntScript - keep its uses few and explicit (search for KntAuthorizationBypass).
/// </summary>
internal static class KntAuthorizationBypass
{
    private static readonly AsyncLocal<string> _reason = new();

    internal static bool IsActive => _reason.Value != null;

    internal static IDisposable Begin(IKntService service, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A reason is required to bypass the authorization.", nameof(reason));

        service.Logger?.LogInformation("Authorization bypass for '{reason}' (user '{user}', repository '{repository}').",
            reason, service.UserIdentityName, service.RepositoryRef?.Alias);

        var previous = _reason.Value;
        _reason.Value = reason;
        return new Scope(previous);
    }

    private sealed class Scope : IDisposable
    {
        private readonly string _previous;
        private bool _disposed;

        public Scope(string previous)
        {
            _previous = previous;
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _reason.Value = _previous;
            _disposed = true;
        }
    }
}

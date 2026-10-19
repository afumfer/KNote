using System;
using System.Security.Claims;
using System.Threading.Tasks;
using KNote.Service.Core;
using Microsoft.Extensions.Caching.Memory;

namespace KNote.Server.Helpers;

/// <summary>
/// Closes the sessions opened before a password change. Every token carries the fingerprint of the user's password
/// when it was issued (ClaimType, Users.GetPasswordStampAsync) and a token whose fingerprint is no longer the user's
/// is refused (Program: JwtBearerEvents.OnTokenValidated), as is one without it (issued before this check existed).
/// The fingerprints are cached for a short time, so a request doesn't read the user each time: a password changed
/// through this Server is forgotten at once (Refresh/Forget); one changed elsewhere (ClientWin, on the same
/// database) counts after CacheDuration at most.
/// </summary>
public class KntPasswordStamps
{
    public const string ClaimType = "knt_pwd";

    public static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(1);

    private readonly IMemoryCache _cache;

    public KntPasswordStamps(IMemoryCache cache)
    {
        _cache = cache;
    }

    // The user's current fingerprint, or null if the user is not registered.
    public async Task<string> GetAsync(IKntService service, string userName)
    {
        return await _cache.GetOrCreateAsync(Key(userName), async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            var res = await service.Users.GetPasswordStampAsync(userName);
            return res.IsValid ? res.Entity : null;
        });
    }

    // Read again, for a token issued now (sign in, register, password changed).
    public async Task<string> RefreshAsync(IKntService service, string userName)
    {
        Forget(userName);
        return await GetAsync(service, userName);
    }

    public void Forget(string userName) => _cache.Remove(Key(userName));

    public async Task<bool> IsValidAsync(ClaimsPrincipal principal, IKntService service)
    {
        var userName = principal?.Identity?.Name;
        var stamp = principal?.FindFirst(ClaimType)?.Value;
        if (string.IsNullOrEmpty(userName) || stamp == null)
            return false;

        var current = await GetAsync(service, userName);
        return current != null && string.Equals(current, stamp, StringComparison.Ordinal);
    }

    private static string Key(string userName) => $"knt-password-stamp:{userName}";
}

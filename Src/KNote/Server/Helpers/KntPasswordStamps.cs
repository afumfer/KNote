using System;
using System.Security.Claims;
using System.Threading.Tasks;
using KNote.Service.Core;
using Microsoft.Extensions.Caching.Memory;

namespace KNote.Server.Helpers;

/// <summary>
/// Closes the sessions opened before a password change. Every token carries the fingerprint of the user's password
/// in the session's repository when it was issued (ClaimType, Users.GetPasswordStampAsync) and a token whose
/// fingerprint is no longer the user's there is refused (Program: JwtBearerEvents.OnTokenValidated), as is one without
/// it (issued before this check existed). The fingerprints are cached per repository and user for a short time, so a
/// request doesn't read the user each time: a password changed through this Server is forgotten at once
/// (Refresh/Forget); one changed elsewhere (ClientWin, on the same database) counts after CacheDuration at most.
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

    // The user's current fingerprint in the repository of service, or null if the user is not registered there.
    public async Task<string> GetAsync(Guid repositoryId, IKntService service, string userName)
    {
        return await _cache.GetOrCreateAsync(Key(repositoryId, userName), async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            var res = await service.Users.GetPasswordStampAsync(userName);
            return res.IsValid ? res.Entity : null;
        });
    }

    // Read again, for a token issued now (sign in, register, password changed).
    public async Task<string> RefreshAsync(Guid repositoryId, IKntService service, string userName)
    {
        Forget(repositoryId, userName);
        return await GetAsync(repositoryId, service, userName);
    }

    public void Forget(Guid repositoryId, string userName) => _cache.Remove(Key(repositoryId, userName));

    // Whether the token's fingerprint is the user's current one in the session's repository. A service of that
    // repository is created only when the fingerprint isn't cached.
    public async Task<bool> IsValidAsync(ClaimsPrincipal principal, KntRepositoryEntry repository, KntRepositories repositories)
    {
        var userName = principal?.Identity?.Name;
        var stamp = principal?.FindFirst(ClaimType)?.Value;
        if (string.IsNullOrEmpty(userName) || stamp == null)
            return false;

        if (!_cache.TryGetValue(Key(repository.RepositoryId, userName), out string current))
        {
            using var service = repositories.CreateService(repository, null, null);
            current = await GetAsync(repository.RepositoryId, service, userName);
        }
        return current != null && string.Equals(current, stamp, StringComparison.Ordinal);
    }

    private static string Key(Guid repositoryId, string userName) => $"knt-password-stamp:{repositoryId}:{userName}";
}

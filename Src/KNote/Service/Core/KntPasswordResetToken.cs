using System;
using System.Security.Cryptography;
using System.Text;

namespace KNote.Service.Core;

/// <summary>
/// The token of a "set a new password" link: the repository (its RepositoryId: a Server serves several), the user,
/// when it expires and the fingerprint of the user's password (KntPasswordHasher.Stamp), signed with a key only its
/// issuer has (the Server's secret). Nothing is stored: the signature proves who issued it, and it can be used once, as
/// setting the password changes the fingerprint (so does any other change of the password meanwhile). The tokens of
/// before the repository was in them ("v1") are no longer valid.
/// </summary>
internal static class KntPasswordResetToken
{
    private const string Version = "v2";

    public static string Create(Guid repositoryId, Guid userId, string passwordStamp, DateTime expiresUtc, string signingKey)
    {
        var payload = $"{Version}|{repositoryId:N}|{userId:N}|{new DateTimeOffset(expiresUtc).ToUnixTimeSeconds()}|{passwordStamp}";
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        return $"{Base64Url(payloadBytes)}.{Base64Url(Sign(payloadBytes, signingKey))}";
    }

    /// <summary>
    /// The repository, user, expiry and password fingerprint of a token, if it was signed with signingKey (they are
    /// checked by the caller).
    /// </summary>
    public static bool TryRead(string token, string signingKey, out Guid repositoryId, out Guid userId, out DateTime expiresUtc, out string passwordStamp)
    {
        repositoryId = Guid.Empty;
        userId = Guid.Empty;
        expiresUtc = DateTime.MinValue;
        passwordStamp = null;

        var parts = token?.Split('.');
        if (parts == null || parts.Length != 2)
            return false;

        byte[] payloadBytes, signature;
        try
        {
            payloadBytes = FromBase64Url(parts[0]);
            signature = FromBase64Url(parts[1]);
        }
        catch (FormatException)
        {
            return false;
        }

        if (!CryptographicOperations.FixedTimeEquals(signature, Sign(payloadBytes, signingKey)))
            return false;

        var fields = Encoding.UTF8.GetString(payloadBytes).Split('|');
        if (fields.Length != 5 || fields[0] != Version || !Guid.TryParse(fields[1], out repositoryId) || !Guid.TryParse(fields[2], out userId)
            || !long.TryParse(fields[3], out var expires))
            return false;

        expiresUtc = DateTimeOffset.FromUnixTimeSeconds(expires).UtcDateTime;
        passwordStamp = fields[4];
        return true;
    }

    // A key of its own, derived from the issuer's one: the same secret also signs other things (the Server's JWT).
    private static byte[] Sign(byte[] payload, string signingKey)
    {
        var key = HMACSHA256.HashData(Encoding.UTF8.GetBytes(signingKey ?? ""), Encoding.UTF8.GetBytes("knote-password-reset"));
        return HMACSHA256.HashData(key, payload);
    }

    private static string Base64Url(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] FromBase64Url(string text)
    {
        var base64 = text.Replace('-', '+').Replace('_', '/');
        return Convert.FromBase64String(base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '='));
    }
}

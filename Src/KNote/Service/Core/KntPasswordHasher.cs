using System;
using System.Security.Cryptography;
using System.Text;
using KNote.Model.Dto;

namespace KNote.Service.Core;

/// <summary>
/// How passwords are stored (HMACSHA512 of the password, keyed with a random 128-byte salt) and checked. The only
/// place that does it: the user commands (create, register, authenticate, set and change the password) use it.
/// </summary>
internal static class KntPasswordHasher
{
    public static void CreateHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        if (password == null) throw new ArgumentNullException("password");
        if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

        using (var hmac = new HMACSHA512())
        {
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }
    }

    public static bool Verify(string password, byte[] storedHash, byte[] storedSalt)
    {
        if (password == null) throw new ArgumentNullException("password");
        if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
        if (storedHash == null) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "storedHash");
        if (storedHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "storedHash");
        if (storedSalt == null) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "storedSalt");
        if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "storedSalt");

        using (var hmac = new HMACSHA512(storedSalt))
        {
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
        }
    }

    /// <summary>
    /// A short fingerprint of a stored password hash, which changes whenever the password does (a new hash, with
    /// a new salt, even for the same password). The Server puts it in its tokens to close the sessions opened
    /// before a password change. Empty for a user without a password.
    /// </summary>
    public static string Stamp(byte[] passwordHash)
    {
        if (passwordHash == null)
            return "";

        return Convert.ToBase64String(SHA256.HashData(passwordHash), 0, 12);
    }

    /// <summary>
    /// Whether currentPassword is the password of user, before the user changes something of their own account
    /// that asks for it. A user without a password yet (one created by an Admin without it, who could only have
    /// signed in to ClientWin with the Windows account) needs none: the change gives them their first one.
    /// </summary>
    public static bool IsCurrentPassword(UserInternalDto user, string currentPassword)
    {
        if (user.PasswordHash == null)
            return true;
        if (string.IsNullOrWhiteSpace(currentPassword))
            return false;

        return Verify(currentPassword, user.PasswordHash, user.PasswordSalt);
    }
}

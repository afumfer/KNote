using System.Security.Cryptography;
using System.Text;

namespace KNote.ClientWin.Core;

/// <summary>
/// Protects secrets with Windows DPAPI for the current user ("dpapi:" + base64), so they can only be
/// decrypted by the same Windows user on the same machine.
/// </summary>
public sealed class DpapiSecretProtector : ISecretProtector
{
    public const string Prefix = "dpapi:";

    // Not a secret: ties the encrypted values to this application/format.
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("KNote.AppUserSettings.v2");

    public string Protect(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        var encrypted = ProtectedData.Protect(Encoding.UTF8.GetBytes(plainText), Entropy, DataProtectionScope.CurrentUser);
        return Prefix + Convert.ToBase64String(encrypted);
    }

    public bool IsProtected(string storedValue)
        => !string.IsNullOrEmpty(storedValue) && storedValue.StartsWith(Prefix, StringComparison.Ordinal);

    public bool TryUnprotect(string storedValue, out string plainText)
    {
        if (string.IsNullOrEmpty(storedValue) || !storedValue.StartsWith(Prefix, StringComparison.Ordinal))
        {
            plainText = storedValue;
            return true;
        }

        try
        {
            var encrypted = Convert.FromBase64String(storedValue.Substring(Prefix.Length));
            plainText = Encoding.UTF8.GetString(ProtectedData.Unprotect(encrypted, Entropy, DataProtectionScope.CurrentUser));
            return true;
        }
        catch (Exception ex) when (ex is CryptographicException or FormatException)
        {
            plainText = null;
            return false;
        }
    }
}

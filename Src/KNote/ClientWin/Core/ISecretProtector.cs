namespace KNote.ClientWin.Core;

/// <summary>
/// Encrypts/decrypts the secrets stored in KNoteData.config. The stored form is self-describing, so a
/// value that is not in it (a plain-text secret from an old config, or one typed by hand) is still read.
/// </summary>
public interface ISecretProtector
{
    /// <summary>Returns the stored form of a secret; null or empty is returned unchanged.</summary>
    string Protect(string plainText);

    /// <summary>
    /// True when <paramref name="storedValue"/> was decrypted or was not protected in the first place
    /// (returned as is). False when it is protected but cannot be decrypted - written by another Windows
    /// user or machine, or corrupted - and <paramref name="plainText"/> is then null.
    /// </summary>
    bool TryUnprotect(string storedValue, out string plainText);
}

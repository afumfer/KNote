using KNote.ClientWin.Core;

namespace KNote.ClientWin.Tests.Fakes;

/// <summary>
/// Deterministic, reversible stand-in for DPAPI ("fake:" + reversed text). Values listed in
/// <see cref="Undecryptable"/> behave like secrets written by another Windows user.
/// </summary>
internal class FakeSecretProtector : ISecretProtector
{
    public const string Prefix = "fake:";

    public HashSet<string> Undecryptable { get; } = new();

    public string Protect(string plainText)
        => string.IsNullOrEmpty(plainText) ? plainText : Prefix + new string(plainText.Reverse().ToArray());

    public bool TryUnprotect(string storedValue, out string plainText)
    {
        if (string.IsNullOrEmpty(storedValue) || !storedValue.StartsWith(Prefix, StringComparison.Ordinal))
        {
            plainText = storedValue;
            return true;
        }

        if (Undecryptable.Contains(storedValue))
        {
            plainText = null!;
            return false;
        }

        plainText = new string(storedValue.Substring(Prefix.Length).Reverse().ToArray());
        return true;
    }
}

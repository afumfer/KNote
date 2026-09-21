using System.Xml.Serialization;
using KNote.Model;

namespace KNote.ClientWin.Core;

/// <summary>
/// Knows which values of <see cref="AppUserSettings"/> are secrets (SMTP password, AI API keys, and the
/// connection strings that carry a password) and encrypts them for KNoteData.config / decrypts them on
/// load. In memory the settings always hold plain text; only the file holds the encrypted form.
/// </summary>
public sealed record SecretsReadResult(IReadOnlyList<string> Undecryptable, bool FoundPlainText);

public sealed class AppUserSettingsSecrets
{
    private readonly ISecretProtector _protector;

    public AppUserSettingsSecrets(ISecretProtector protector)
    {
        _protector = protector ?? throw new ArgumentNullException(nameof(protector));
    }

    /// <summary>Returns a copy of the settings with the secrets encrypted, ready to serialize. The input is untouched.</summary>
    public AppUserSettings Protect(AppUserSettings settings)
    {
        var serializer = new XmlSerializer(typeof(AppUserSettings));
        using var buffer = new MemoryStream();
        serializer.Serialize(buffer, settings);
        buffer.Position = 0;
        var copy = (AppUserSettings)serializer.Deserialize(buffer);

        foreach (var secret in Secrets(copy))
        {
            var value = secret.Get();
            if (secret.OnlyWhenPasswordInside && !ConnectionStringSecrets.HasPassword(value))
                continue;
            secret.Set(_protector.Protect(value));
        }

        return copy;
    }

    /// <summary>
    /// Decrypts, in place, the secrets of settings just read from disk. A secret that cannot be decrypted
    /// is cleared and reported so the caller can tell the user to enter it again. Secrets found in plain
    /// text (an old config, or one edited by hand) are kept as they are and flagged, so the caller can
    /// rewrite the file with them encrypted.
    /// </summary>
    public SecretsReadResult Unprotect(AppUserSettings settings)
    {
        var undecryptable = new List<string>();
        var foundPlainText = false;

        foreach (var secret in Secrets(settings))
        {
            var stored = secret.Get();

            if (!_protector.IsProtected(stored))
            {
                if (!string.IsNullOrEmpty(stored) && (!secret.OnlyWhenPasswordInside || ConnectionStringSecrets.HasPassword(stored)))
                    foundPlainText = true;
                continue;
            }

            if (_protector.TryUnprotect(stored, out var plainText))
                secret.Set(plainText);
            else
            {
                secret.Set(null);
                undecryptable.Add(secret.Description);
            }
        }

        return new SecretsReadResult(undecryptable, foundPlainText);
    }

    private sealed record Secret(string Description, Func<string> Get, Action<string> Set, bool OnlyWhenPasswordInside = false);

    private static IEnumerable<Secret> Secrets(AppUserSettings settings)
    {
        var email = settings.Notifications.Email;
        yield return new Secret("e-mail account password", () => email.Password, v => email.Password = v);

        foreach (var provider in settings.Ai.Providers)
        {
            var p = provider;
            yield return new Secret($"API key of AI provider '{p.Alias}'", () => p.ApiKey, v => p.ApiKey = v);
        }

        foreach (var repository in settings.Repositories.Items)
        {
            var r = repository;
            yield return new Secret($"connection string of repository '{r.Alias}'",
                () => r.ConnectionString, v => r.ConnectionString = v, OnlyWhenPasswordInside: true);
        }

        var assistant = settings.Repositories.Assistant;
        if (assistant != null)
            yield return new Secret("connection string of the assistant repository",
                () => assistant.ConnectionString, v => assistant.ConnectionString = v, OnlyWhenPasswordInside: true);
    }
}

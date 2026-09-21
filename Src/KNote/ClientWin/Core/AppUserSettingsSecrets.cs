using System.Xml.Serialization;
using KNote.Model;

namespace KNote.ClientWin.Core;

/// <summary>
/// Knows which values of <see cref="AppUserSettings"/> are secrets (SMTP password, AI API keys, and the
/// connection strings that carry a password) and encrypts them for KNoteData.config / decrypts them on
/// load. In memory the settings always hold plain text; only the file holds the encrypted form.
/// </summary>
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
    /// Decrypts, in place, the secrets of settings just read from disk (plain-text ones are kept as they
    /// are and get encrypted on the next save). A secret that cannot be decrypted is cleared, and its
    /// description is returned so the caller can tell the user to enter it again.
    /// </summary>
    public IReadOnlyList<string> Unprotect(AppUserSettings settings)
    {
        var failed = new List<string>();

        foreach (var secret in Secrets(settings))
        {
            if (_protector.TryUnprotect(secret.Get(), out var plainText))
                secret.Set(plainText);
            else
            {
                secret.Set(null);
                failed.Add(secret.Description);
            }
        }

        return failed;
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

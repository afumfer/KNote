using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace KNote.Server.Local;

/// <summary>
/// KNote Local's configuration file (knote-local.json) as a source of the configuration, with its protected values
/// ("protected:...", see KntSecretProtector) decrypted when it is read (also when it is read again after it changes).
/// A value that can't be decrypted (keys of another user or computer) is left empty and its key is in UnreadableKeys,
/// so that the server starts and says which secrets have to be entered again.
/// </summary>
public class KntProtectedJsonConfigurationSource : JsonConfigurationSource
{
    public KntSecretProtector Protector { get; init; }

    public override IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        EnsureDefaults(builder);
        return new KntProtectedJsonConfigurationProvider(this);
    }
}

public class KntProtectedJsonConfigurationProvider : JsonConfigurationProvider
{
    private readonly KntSecretProtector _protector;

    public KntProtectedJsonConfigurationProvider(KntProtectedJsonConfigurationSource source) : base(source)
    {
        _protector = source.Protector;
    }

    public IReadOnlyList<string> UnreadableKeys { get; private set; } = [];

    public override void Load(Stream stream)
    {
        base.Load(stream);

        var unreadable = new List<string>();
        foreach (var key in Data.Keys.Where(k => KntSecretProtector.IsProtected(Data[k])).ToList())
        {
            try
            {
                Data[key] = _protector.Unprotect(Data[key]);
            }
            catch (CryptographicException)
            {
                Data[key] = "";
                unreadable.Add(key);
            }
        }
        UnreadableKeys = unreadable;
    }
}

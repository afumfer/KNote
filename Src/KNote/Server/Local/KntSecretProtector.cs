using System;
using System.IO;
using Microsoft.AspNetCore.DataProtection;

namespace KNote.Server.Local;

/// <summary>
/// Encrypts the secrets of KNote Local's configuration (the sessions' signing secret, the AI providers' API keys, the SMTP
/// password, the passwords in SQL Server connection strings) with ASP.NET Core Data Protection. A protected value is kept
/// as "protected:..." (KntProtectedJsonConfigurationProvider decrypts it when the configuration is read). The keys are in
/// the data folder's Keys (so they survive reinstalling) under a fixed application name; on Windows they are themselves
/// protected with DPAPI for the user (explicitly: with a keys folder of its own, Data Protection doesn't do it by itself),
/// on macOS and Linux only by the permissions of the user's profile.
/// </summary>
public class KntSecretProtector
{
    public const string Prefix = "protected:";
    private const string ApplicationName = "KNote";
    private const string Purpose = "KNote.Local.Settings";

    private readonly IDataProtector _protector;

    public KntSecretProtector(string keysFolder)
    {
        var provider = DataProtectionProvider.Create(new DirectoryInfo(keysFolder), builder =>
        {
            builder.SetApplicationName(ApplicationName);
            if (OperatingSystem.IsWindows())
                builder.ProtectKeysWithDpapi();
        });
        _protector = provider.CreateProtector(Purpose);
    }

    public static bool IsProtected(string value) => value?.StartsWith(Prefix, StringComparison.Ordinal) == true;

    // An empty value stays empty (nothing to keep secret).
    public string Protect(string plainText) =>
        string.IsNullOrEmpty(plainText) || IsProtected(plainText) ? plainText : Prefix + _protector.Protect(plainText);

    // A value that isn't protected (typed by hand) is returned as it is. Throws CryptographicException when it can't be
    // decrypted (keys of another user or computer).
    public string Unprotect(string value) =>
        IsProtected(value) ? _protector.Unprotect(value[Prefix.Length..]) : value;
}

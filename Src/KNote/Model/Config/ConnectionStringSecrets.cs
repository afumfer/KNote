using System.Text.RegularExpressions;

namespace KNote.Model;

// A repository connection string is a secret only when it carries credentials (SQL authentication:
// Password=... / Pwd=...). SQLite and Trusted_Connection strings are not, and stay readable on disk.
public static class ConnectionStringSecrets
{
    private static readonly Regex PasswordPart = new(
        @"(?<key>\b(?:password|pwd))\s*=\s*(?:""[^""]*""|'[^']*'|[^;]*)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static bool HasPassword(string connectionString)
        => !string.IsNullOrEmpty(connectionString) && PasswordPart.IsMatch(connectionString);

    // Same connection string with the password value emptied ("Password=;"), the rest untouched.
    public static string WithoutPassword(string connectionString)
        => string.IsNullOrEmpty(connectionString)
            ? connectionString
            : PasswordPart.Replace(connectionString, "${key}=");
}

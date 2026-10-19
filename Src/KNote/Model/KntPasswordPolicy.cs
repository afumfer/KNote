using System;

namespace KNote.Model;

/// <summary>
/// The rules a new password must follow, wherever one is set: registration, an Admin creating a user or setting
/// its password, and the user changing their own. The service checks them; a UI may check them too, to answer
/// sooner. Passwords already stored are not checked again: they keep working until they are changed.
/// </summary>
public static class KntPasswordPolicy
{
    public const int MinLength = 8;
    public const int MaxLength = 128;

    /// <summary>
    /// Why password can't be used by the user userName, or null when it can.
    /// </summary>
    public static string Validate(string password, string userName = null)
    {
        if (string.IsNullOrWhiteSpace(password))
            return "Password is required.";
        if (password.Length < MinLength)
            return $"The password must have at least {MinLength} characters.";
        if (password.Length > MaxLength)
            return $"The password can't have more than {MaxLength} characters.";
        if (!string.IsNullOrEmpty(userName) && string.Equals(password, userName, StringComparison.OrdinalIgnoreCase))
            return "The password can't be the user name.";

        return null;
    }
}

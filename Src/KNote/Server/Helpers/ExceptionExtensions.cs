using System;

namespace KNote.Server.Helpers;

public static class ExceptionExtensions
{
    /// <summary>
    /// The service layer wraps every command exception in a generic KntServiceException
    /// ("KNote service error. (...)"), so the message worth returning to an API client is the innermost one.
    /// </summary>
    public static string GetRootMessage(this Exception ex)
    {
        return ex.GetBaseException().Message;
    }

    /// <summary>
    /// Error message for the catch blocks of the API controllers: "Generic error: " + root cause message.
    /// </summary>
    public static string ToApiErrorMessage(this Exception ex)
    {
        return "Generic error: " + ex.GetRootMessage();
    }
}

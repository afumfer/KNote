using KNote.Model;

namespace KNote.ClientWin.Core;

/// <summary>
/// Stops a batch of operations (moving notes, changing tags...) at the first one the Service layer refuses
/// for lack of role (ResultBase.NotAuthorized): the rest would be refused the same way, the role being the
/// same for every note of the repository. A TaskCanceledException, so it ends the batch through the same
/// path as the user's own Cancel; the message is the refusal's, to tell the user.
/// </summary>
public class KntNotAuthorizedException : TaskCanceledException
{
    public KntNotAuthorizedException(string message) : base(message)
    {
    }

    // Throws when a Service result was refused for lack of role.
    public static void ThrowIfNotAuthorized(ResultBase result)
    {
        if (result?.NotAuthorized == true)
            throw new KntNotAuthorizedException(result.ErrorMessage);
    }
}

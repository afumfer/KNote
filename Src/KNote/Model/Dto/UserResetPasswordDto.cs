namespace KNote.Model.Dto;

/// <summary>
/// A new password set with the token of the link sent by email (POST api/users/reset-password,
/// Users.ResetPasswordAsync).
/// </summary>
public class UserResetPasswordDto
{
    // The link's token: the user's token in each repository where the new password is set (see the Server's
    // KntPasswordResetLinkToken).
    public string Token { get; set; }
    public string NewPassword { get; set; }
}

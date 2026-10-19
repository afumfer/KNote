namespace KNote.Model.Dto;

/// <summary>
/// "Forgot your password?": who asks for a link to set a new one (POST api/users/forgot-password).
/// </summary>
public class UserForgotPasswordDto
{
    public string UserNameOrEMail { get; set; }
}

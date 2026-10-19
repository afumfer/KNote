namespace KNote.Model.Dto;

/// <summary>
/// An Admin setting the password of another user (Users.SetPasswordAsync, PUT api/users/{id}/password).
/// </summary>
public class UserSetPasswordDto
{
    public string NewPassword { get; set; }
}

namespace KNote.Model.Dto;

/// <summary>
/// A user changing their own password (Users.ChangePasswordAsync). The confirmation of the new one is the UI's.
/// </summary>
public class UserChangePasswordDto
{
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
}

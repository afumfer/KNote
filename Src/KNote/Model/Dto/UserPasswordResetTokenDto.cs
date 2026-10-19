using System;

namespace KNote.Model.Dto;

/// <summary>
/// A token to set a new password (Users.CreatePasswordResetAsync) and the user it is for, to send it to them. Only
/// for the one who sends it (the Server): never part of an answer.
/// </summary>
public class UserPasswordResetTokenDto
{
    public string UserName { get; set; }
    public string FullName { get; set; }
    public string EMail { get; set; }
    public string Token { get; set; }
    public DateTime ExpiresUtc { get; set; }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace KNote.Model.Dto;

public class UserTokenDto
{
    public bool success { get; set; }
    public string token { get; set; }
    public string uid { get; set; }
    public string error { get; set; }

    // The sign in without a repository, when the credentials are valid in several repositories of the Server: they
    // (success is false and there is no token), to sign in again with one of them (UserCredentialsDto.RepositoryId).
    // Also the repositories where a new password was set with a reset link, when they are several (without a token).
    public List<RepositoryInfoDto> Repositories { get; set; }

    // The user whose new password was set in several repositories (Repositories), to sign in with it.
    public string UserName { get; set; }

    // Something the user should know about what was done, e.g. the repositories where a new password couldn't be set.
    public string Notice { get; set; }
}

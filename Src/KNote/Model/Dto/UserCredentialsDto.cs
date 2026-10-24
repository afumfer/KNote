using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto;

public class UserCredentialsDto
{
    public string UserName { get; set; }
    public string Password { get; set; }

    // The repository to sign in to, of those the sign in answered (UserTokenDto.Repositories). Without it, the
    // credentials are checked in every repository of the Server.
    public Guid? RepositoryId { get; set; }
}

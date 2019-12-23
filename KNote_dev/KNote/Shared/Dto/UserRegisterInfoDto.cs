using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Shared.Dto
{ 
    public class UserRegisterInfoDto : UserInfoDto
    {
        public UserInfoDto UserInfo { get; set; }
        public string Password { get; set; }
    }
}

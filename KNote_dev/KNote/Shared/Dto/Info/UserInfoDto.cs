using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Shared.Dto.Info
{
    public class UserInfoDto : KntModelBase
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string EMail { get; set; }
        public string FullName { get; set; }
        public string RoleDefinition { get; set; }
        public bool Disabled { get; set; }                
    }
}

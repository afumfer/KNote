using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Shared.Dto
{
    public class UserDto : KntModelBase
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string EMail { get; set; }
        public string FullName { get; set; }
        public string RoleDefinition { get; set; }
        public bool Disabled { get; set; }
        public List<KMessageInfoDto> MessagesInfo { get; set; }
    }
}

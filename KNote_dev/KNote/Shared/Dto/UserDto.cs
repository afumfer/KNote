using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Shared.Dto
{
    public class UserDto : UserBasicEditDto
    {
        public List<KMessageInfoDto> MessagesInfo { get; set; }
    }
}

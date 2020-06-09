using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// TODO: !!! sustituir Info por Dto
using KNote.Shared.Dto.Info;

namespace KNote.Shared.Dto
{
    public class UserDto : UserBasicEditDto
    {
        // TODO: !!! sustituir Info por Dto
        public List<KMessageInfoDto> MessagesInfo { get; set; }
    }
}

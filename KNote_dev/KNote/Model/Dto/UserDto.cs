using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto
{
    public class UserDto : UserBaseDto
    {
        public List<KMessageDto> MessagesInfo { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Shared.Dto
{ 
    public class UserRegisterDto : UserInfoDto
    {        
        [Required]
        public string Password { get; set; }
    }
}

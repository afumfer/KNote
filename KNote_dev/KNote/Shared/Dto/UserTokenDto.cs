using System;
using System.Collections.Generic;
using System.Text;

namespace KNote.Shared.Dto
{
    public class UserTokenDto
    {
        public bool success { get; set; }
        public string token { get; set; }
        public string uid { get; set; }
        public string error { get; set; }
    }
}

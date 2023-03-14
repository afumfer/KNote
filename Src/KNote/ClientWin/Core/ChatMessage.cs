using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.ClientWin.Core
{
    public class ChatMessage
    {
        public string Prompt { get; set; }
        public string Role { get; set; }
        public int Tokens { get; set; }
    }
}

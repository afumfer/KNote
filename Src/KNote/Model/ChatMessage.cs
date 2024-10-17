using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model;

public class KntChatMessage
{
    public string Prompt { get; set; }
    public string Role { get; set; }
    public int? Tokens { get; set; }
}

public class KntChatMessageOutput
{
    public string Prompt { get; set; }
    public string Role { get; set; }
    public int? PromptTokens { get; set; }
    public int? CompletionTokens { get; set; }
    public int? TotalTokens { get; set; }
}
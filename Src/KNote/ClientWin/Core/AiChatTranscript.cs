using System.Text;

namespace KNote.ClientWin.Core;

// The conversation of the AI assistant as Markdown: what its Markdown view shows and what is saved as a note.
// Same layout as the turns KNoteAIAssistantCtrl.StreamCompletionAsync streams ("**User:**"/"**Assistant:**"),
// so a streamed answer continues it seamlessly; the usage line of each answer is optional.
public static class AiChatTranscript
{
    public static string Markdown(IEnumerable<AiChatTurn> turns, bool includeModelInfo)
    {
        var text = new StringBuilder();
        foreach (var turn in turns)
        {
            text.Append($"**User:** \r\n{turn.Prompt}\r\n\r\n**Assistant:** \r\n{turn.Answer}\r\n\r\n");
            if (includeModelInfo)
                text.Append($"*({turn.UsageSummary})*\r\n\r\n");
        }
        return text.ToString();
    }
}

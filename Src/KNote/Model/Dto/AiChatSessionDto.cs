using System;
using System.Collections.Generic;

namespace KNote.Model.Dto;

// An AI assistant conversation as it is persisted (one note per session, see IKntAiSessionService). Shared by
// ClientWin and the Web client, so both resume the sessions the other one saved.

// One question and its answer, with what it cost.
public class AiChatTurnDto
{
    public string Prompt { get; set; } = "";

    public string Answer { get; set; } = "";

    public long? InputTokens { get; set; }

    public long? OutputTokens { get; set; }

    public long TotalTokens { get; set; }

    // The tokens were estimated, not reported by the provider.
    public bool TokensEstimated { get; set; }

    // The answer stopped at the output token limit, not because the model had finished.
    public bool Truncated { get; set; }

    public TimeSpan ProcessingTime { get; set; }
}

// A session as listed in the sessions panel.
public class AiChatSessionInfoDto
{
    // Guid.Empty for a session not saved yet.
    public Guid NoteId { get; set; }

    public int NoteNumber { get; set; }

    public string Topic { get; set; }

    public DateTime CreationDateTime { get; set; }

    public DateTime ModificationDateTime { get; set; }
}

public class AiChatSessionDto : AiChatSessionInfoDto
{
    // EnumAiProvider value and model the session was held with.
    public string Provider { get; set; }

    public string Model { get; set; }

    public List<AiChatTurnDto> Turns { get; set; } = new();
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;

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

    // Of the output tokens, the ones spent on reasoning, when the provider tells them apart (Ollama does not).
    public long? ReasoningTokens { get; set; }

    // How the model thought the answer, while the conversation is open: never saved, nor sent back to the model or to the
    // server with the turns. The app that receives the answer through the API puts it together from the reasoning events.
    [JsonIgnore]
    public string Reasoning { get; set; }

    // How long it reasoned before writing or using its tools. Not saved.
    public TimeSpan? ReasoningTime { get; set; }

    // What the assistant did or proposed in this turn (the record of the conversation, D13).
    public List<AiActionDto> Actions { get; set; } = new();

    // Sent by the app after a card, not by the user (D3): its prompt is not shown.
    public bool Automatic { get; set; }

    // The tokens were estimated, not reported by the provider.
    public bool TokensEstimated { get; set; }

    // The answer stopped at the output token limit, not because the model had finished.
    public bool Truncated { get; set; }

    public TimeSpan ProcessingTime { get; set; }

    public const string TruncatedNotice = "The answer was cut: it reached the output token limit.";

    // While the model reasons; the title of its reasoning until the answer is complete.
    public const string ThinkingText = "Thinking…";
    public const string ReasoningText = "Reasoning";

    // The title of the reasoning of an answer: "Thought for 63 s".
    public string ReasoningTitle() =>
        ReasoningTime is TimeSpan time ? $"Thought for {Math.Max(1, (int)Math.Round(time.TotalSeconds))} s" : ReasoningText;

    // What the answer cost, in one line: "3 in · 2 out · 5 tokens · 1.2 s".
    public string UsageSummary()
    {
        string tokens;
        if (TokensEstimated)
            tokens = $"~{TotalTokens} tokens (estimated)";
        else if (InputTokens.HasValue || OutputTokens.HasValue)
            tokens = $"{InputTokens ?? 0} in · {OutputTokens ?? 0} out · {(ReasoningTokens is long reasoning ? $"{reasoning} reasoning · " : "")}{TotalTokens} tokens";
        else
            tokens = $"{TotalTokens} tokens";

        return $"{tokens} · {ProcessingTime.TotalSeconds.ToString("0.0", CultureInfo.InvariantCulture)} s";
    }
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

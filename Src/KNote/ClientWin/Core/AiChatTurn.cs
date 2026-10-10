using KNote.Model.Dto;

namespace KNote.ClientWin.Core;

// One question and its answer in a KNoteAIAssistantCtrl conversation, with what it cost, for the views
// that show each turn on its own (the chat view of KNoteAIAssistantForm, see AiChatHtml).
public sealed record AiChatTurn(string Prompt, string Answer, string ProviderAlias, TimeSpan ProcessingTime)
{
    public long? InputTokens { get; init; }

    public long? OutputTokens { get; init; }

    public long TotalTokens { get; init; }

    // Stream mode only estimates the tokens (see KNoteAIAssistantCtrl.StreamCompletionAsync).
    public bool TokensEstimated { get; init; }

    // The answer stopped at the output token limit, not because the model had finished.
    public bool Truncated { get; init; }

    public const string TruncatedNotice = AiChatTurnDto.TruncatedNotice;

    // The shared form of a turn (KNote.Ai's AiChatTurnStreamer, the persisted sessions), which has no provider:
    // a session is held with a single one.
    public static AiChatTurn FromDto(AiChatTurnDto turn, string providerAlias) =>
        new(turn.Prompt ?? "", turn.Answer ?? "", providerAlias, turn.ProcessingTime)
        {
            InputTokens = turn.InputTokens,
            OutputTokens = turn.OutputTokens,
            TotalTokens = turn.TotalTokens,
            TokensEstimated = turn.TokensEstimated,
            Truncated = turn.Truncated
        };

    public AiChatTurnDto ToDto() => new()
    {
        Prompt = Prompt,
        Answer = Answer,
        InputTokens = InputTokens,
        OutputTokens = OutputTokens,
        TotalTokens = TotalTokens,
        TokensEstimated = TokensEstimated,
        Truncated = Truncated,
        ProcessingTime = ProcessingTime
    };

    // What the answer cost, in one line: "3 in · 2 out · 5 tokens · 1.2 s" (the same as the Web assistant).
    public string UsageSummary => ToDto().UsageSummary();
}

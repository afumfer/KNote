using System.Globalization;

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

    public const string TruncatedNotice = "The answer was cut: it reached the output token limit.";

    // What the answer cost, in one line: "3 in · 2 out · 5 tokens · 1.2 s".
    public string UsageSummary
    {
        get
        {
            string tokens;
            if (TokensEstimated)
                tokens = $"~{TotalTokens} tokens (estimated)";
            else if (InputTokens.HasValue || OutputTokens.HasValue)
                tokens = $"{InputTokens ?? 0} in · {OutputTokens ?? 0} out · {TotalTokens} tokens";
            else
                tokens = $"{TotalTokens} tokens";

            return $"{tokens} · {ProcessingTime.TotalSeconds.ToString("0.0", CultureInfo.InvariantCulture)} s";
        }
    }
}

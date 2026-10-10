using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using KNote.Model.Dto;
using Microsoft.Extensions.AI;

namespace KNote.Ai;

// One turn of an assistant conversation, streamed: the system prompt and the turns so far are sent with the new
// prompt, and the answer comes back as AiChatStreamEventDto - Delta pieces as they are written, Tool when the
// model calls a tool, and Completed at the end with the whole turn and what it cost. The conversation is not
// kept here: the caller owns it (and persists it, see IKntAiSessionService).
public static class AiChatTurnStreamer
{
    public static List<ChatMessage> BuildMessages(string systemPrompt, IEnumerable<AiChatTurnDto> history, string prompt)
    {
        var messages = new List<ChatMessage>();
        if (!string.IsNullOrWhiteSpace(systemPrompt))
            messages.Add(new ChatMessage(ChatRole.System, systemPrompt));

        foreach (var turn in history ?? [])
        {
            messages.Add(new ChatMessage(ChatRole.User, turn.Prompt ?? ""));
            messages.Add(new ChatMessage(ChatRole.Assistant, turn.Answer ?? ""));
        }

        messages.Add(new ChatMessage(ChatRole.User, prompt ?? ""));
        return messages;
    }

    public static async IAsyncEnumerable<AiChatStreamEventDto> StreamAsync(IChatClient chatClient, string systemPrompt,
        IEnumerable<AiChatTurnDto> history, string prompt, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var messages = BuildMessages(systemPrompt, history, prompt);
        var answer = new StringBuilder();
        var stopwatch = Stopwatch.StartNew();
        ChatFinishReason? finishReason = null;
        long inputTokens = 0, outputTokens = 0, totalTokens = 0;
        var usageReported = false;

        await foreach (var update in chatClient.GetStreamingResponseAsync(messages, cancellationToken: cancellationToken))
        {
            // The last one given: the intermediate steps of a tool call carry their own.
            finishReason = update.FinishReason ?? finishReason;

            foreach (var content in update.Contents)
            {
                switch (content)
                {
                    // One per call to the model, and a tool call makes several: they add up.
                    case UsageContent usage:
                        usageReported = true;
                        inputTokens += usage.Details.InputTokenCount ?? 0;
                        outputTokens += usage.Details.OutputTokenCount ?? 0;
                        totalTokens += usage.Details.TotalTokenCount
                            ?? (usage.Details.InputTokenCount ?? 0) + (usage.Details.OutputTokenCount ?? 0);
                        break;
                    case FunctionCallContent call:
                        yield return new AiChatStreamEventDto { Type = AiChatStreamEventTypes.Tool, Text = call.Name };
                        break;
                }
            }

            var text = update.Text;
            if (string.IsNullOrEmpty(text))
                continue;
            answer.Append(text);
            yield return new AiChatStreamEventDto { Type = AiChatStreamEventTypes.Delta, Text = text };
        }

        stopwatch.Stop();

        var turn = new AiChatTurnDto
        {
            Prompt = prompt ?? "",
            Answer = answer.ToString(),
            Truncated = finishReason == ChatFinishReason.Length,
            ProcessingTime = stopwatch.Elapsed
        };
        if (usageReported)
        {
            turn.InputTokens = inputTokens;
            turn.OutputTokens = outputTokens;
            turn.TotalTokens = totalTokens;
        }
        else
        {
            // Not every provider reports usage when streaming: a rough estimate instead (~4 characters a token).
            turn.TotalTokens = (turn.Prompt.Length + turn.Answer.Length) / 4;
            turn.TokensEstimated = true;
        }

        yield return new AiChatStreamEventDto { Type = AiChatStreamEventTypes.Completed, Turn = turn };
    }
}

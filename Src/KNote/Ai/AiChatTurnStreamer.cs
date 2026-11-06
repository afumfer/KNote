using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using KNote.Model;
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

            // What happened, as it happened: the call to the tool and how it ended. With only the text of the answers, the
            // model learns that doing something is saying so, and stops calling the tools (measured with qwen3.5:9b).
            foreach (var action in turn.Actions)
            {
                var callId = action.ActionId.ToString("N");
                var arguments = new Dictionary<string, object> { ["noteNumber"] = action.NoteNumber };
                messages.Add(new ChatMessage(ChatRole.Assistant, [new FunctionCallContent(callId, action.Kind, arguments)]));
                messages.Add(new ChatMessage(ChatRole.Tool, [new FunctionResultContent(callId, AiActionRules.Outcome(action))]));
            }

            // A turn can end in the card, with no text: an empty message is not sent, some providers refuse it.
            if (turn.Actions.Count == 0 || !string.IsNullOrEmpty(turn.Answer))
                messages.Add(new ChatMessage(ChatRole.Assistant, turn.Answer ?? ""));
        }

        messages.Add(new ChatMessage(ChatRole.User, prompt ?? ""));
        return messages;
    }

    // tools: the ones of this turn (KNoteAiTools.GetToolsAsync), when they are not the ones the client was built with.
    public static async IAsyncEnumerable<AiChatStreamEventDto> StreamAsync(IChatClient chatClient, string systemPrompt,
        IEnumerable<AiChatTurnDto> history, string prompt, IList<AITool> tools = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var messages = BuildMessages(systemPrompt, history, prompt);
        var answer = new StringBuilder();
        var stopwatch = Stopwatch.StartNew();
        ChatFinishReason? finishReason = null;
        long inputTokens = 0, outputTokens = 0, totalTokens = 0, reasoningTokens = 0;
        var usageReported = false;
        var reasoningTokensReported = false;

        // The reasoning of the whole turn: a tool call makes the model reason again, after what it thought before.
        var reasoning = new StringBuilder();
        var reasoningTime = TimeSpan.Zero;
        TimeSpan? reasoningStart = null;

        void EndReasoning()
        {
            if (reasoningStart is TimeSpan start)
                reasoningTime += stopwatch.Elapsed - start;
            reasoningStart = null;
        }

        var options = tools != null ? new ChatOptions { Tools = tools } : null;

        await foreach (var update in chatClient.GetStreamingResponseAsync(messages, options, cancellationToken))
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
                        if (usage.Details.ReasoningTokenCount is long tokens)
                        {
                            reasoningTokensReported = true;
                            reasoningTokens += tokens;
                        }
                        break;
                    case FunctionCallContent call:
                        EndReasoning();
                        yield return new AiChatStreamEventDto { Type = AiChatStreamEventTypes.Tool, Text = call.Name };
                        break;
                    // Some providers only send it encrypted, without a text.
                    case TextReasoningContent thought when !string.IsNullOrEmpty(thought.Text):
                        var piece = reasoningStart == null && reasoning.Length > 0 ? "\n\n" + thought.Text : thought.Text;
                        reasoningStart ??= stopwatch.Elapsed;
                        reasoning.Append(piece);
                        yield return new AiChatStreamEventDto { Type = AiChatStreamEventTypes.Reasoning, Text = piece };
                        break;
                }
            }

            var text = update.Text;
            if (string.IsNullOrEmpty(text))
                continue;
            EndReasoning();
            answer.Append(text);
            yield return new AiChatStreamEventDto { Type = AiChatStreamEventTypes.Delta, Text = text };
        }

        EndReasoning();
        stopwatch.Stop();

        var turn = new AiChatTurnDto
        {
            Prompt = prompt ?? "",
            Answer = answer.ToString(),
            Reasoning = reasoning.Length > 0 ? reasoning.ToString().Trim() : null,
            ReasoningTime = reasoning.Length > 0 ? reasoningTime : null,
            ReasoningTokens = reasoningTokensReported ? reasoningTokens : null,
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

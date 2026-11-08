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
    // attachments: the ones of this message. The images go only in it: in the next turns the model works with what it answered.
    public static List<ChatMessage> BuildMessages(string systemPrompt, IEnumerable<AiChatTurnDto> history, string prompt,
        AiTurnAttachments attachments = null)
    {
        var messages = new List<ChatMessage>();
        if (!string.IsNullOrWhiteSpace(systemPrompt))
            messages.Add(new ChatMessage(ChatRole.System, systemPrompt));

        foreach (var turn in history ?? [])
        {
            messages.Add(new ChatMessage(ChatRole.User, AiAttachmentRules.PromptWithAttachments(turn.Prompt ?? "", turn.Attachments)));

            // What happened, as it happened: the call to the tool and how it ended. With only the text of the answers, the
            // model learns that doing something is saying so, and stops calling the tools (measured with qwen3.5:9b).
            foreach (var action in turn.Actions)
            {
                var callId = action.ActionId.ToString("N");
                messages.Add(new ChatMessage(ChatRole.Assistant, [new FunctionCallContent(callId, action.Kind, CallArguments(action))]));
                messages.Add(new ChatMessage(ChatRole.Tool, [new FunctionResultContent(callId, AiActionRules.Outcome(action))]));
            }

            // A turn can end in the card, with no text: an empty message is not sent, some providers refuse it.
            if (turn.Actions.Count == 0 || !string.IsNullOrEmpty(turn.Answer))
                messages.Add(new ChatMessage(ChatRole.Assistant, turn.Answer ?? ""));
        }

        var text = AiAttachmentRules.PromptWithAttachments(prompt ?? "", attachments?.All, attachments?.Seen ?? []);
        messages.Add(new ChatMessage(ChatRole.User, [new TextContent(text), .. attachments?.Images ?? []]));
        return messages;
    }

    // A model repeats the calls it sees: with only the note, set_tags has nothing required left and it called it without tags
    // (measured with qwen3.5:9b). The tags come with the turns of the running conversation; a saved one does not keep them.
    private static Dictionary<string, object> CallArguments(AiActionDto action)
    {
        var arguments = new Dictionary<string, object>();
        if (action.NoteNumber is int number)
            arguments["noteNumber"] = number;

        if (action.Kind == AiActionRules.SetTags)
        {
            if (action.AddTags?.Count > 0)
                arguments["add"] = action.AddTags;
            if (action.RemoveTags?.Count > 0)
                arguments["remove"] = action.RemoveTags;
        }

        return arguments;
    }

    // tools: the ones of this turn (KNoteAiTools.GetToolsAsync), when they are not the ones the client was built with.
    public static async IAsyncEnumerable<AiChatStreamEventDto> StreamAsync(IChatClient chatClient, string systemPrompt,
        IEnumerable<AiChatTurnDto> history, string prompt, IList<AITool> tools = null, AiTurnAttachments attachments = null,
        AiToolImages toolImages = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var messages = BuildMessages(systemPrompt, history, prompt, attachments);
        var answer = new StringBuilder();
        var stopwatch = Stopwatch.StartNew();
        ChatFinishReason? finishReason = null;
        long inputTokens = 0, outputTokens = 0, totalTokens = 0, reasoningTokens = 0;
        var usageReported = false;
        var reasoningTokensReported = false;

        // The reasoning of the whole turn: a tool call makes the model reason again, after what it thought before.
        var reasoning = new StringBuilder();
        var reasoningTime = TimeSpan.Zero;

        // The pages the provider cites (OpenAI, Anthropic): added at the end of the answer, as its sources.
        var citations = new List<(string Url, string Title)>();
        TimeSpan? reasoningStart = null;

        void EndReasoning()
        {
            if (reasoningStart is TimeSpan start)
                reasoningTime += stopwatch.Elapsed - start;
            reasoningStart = null;
        }

        var options = tools != null ? new ChatOptions { Tools = tools } : null;
        if (toolImages != null)
        {
            options ??= new ChatOptions();
            options.AdditionalProperties = new AdditionalPropertiesDictionary { [AiToolImagesChatClient.PropertyName] = toolImages };
        }

        await foreach (var update in chatClient.GetStreamingResponseAsync(messages, options, cancellationToken))
        {
            // The last one given: the intermediate steps of a tool call carry their own.
            finishReason = update.FinishReason ?? finishReason;

            foreach (var content in update.Contents)
            {
                foreach (var citation in content.Annotations?.OfType<CitationAnnotation>() ?? [])
                    if (citation.Url != null)
                        citations.Add((citation.Url.ToString(), citation.Title));

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
                    // The provider's own search (HostedWebSearchTool).
                    case WebSearchToolCallContent:
                        EndReasoning();
                        yield return new AiChatStreamEventDto { Type = AiChatStreamEventTypes.Tool, Text = AiWebSearchRules.WebSearchName };
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

        var sources = AiWebSearchRules.SourcesSection(answer.ToString(), citations);
        if (sources.Length > 0)
        {
            answer.Append(sources);
            yield return new AiChatStreamEventDto { Type = AiChatStreamEventTypes.Delta, Text = sources };
        }

        stopwatch.Stop();

        var turn = new AiChatTurnDto
        {
            Prompt = prompt ?? "",
            Attachments = attachments?.All.ToList() ?? new(),
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

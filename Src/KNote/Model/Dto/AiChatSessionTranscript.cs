using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KNote.Model.Dto;

// The Description of an AI assistant session note: the conversation as Markdown, readable (and editable) like
// any other note, with an HTML comment before each message that Markdown viewers don't show and that lets
// Parse get the turns back exactly:
//
//   <!-- knt-ai:user -->
//   **User:**
//   (the prompt)
//
//   <!-- knt-ai:assistant {"totalTokens":5,...} -->
//   **Assistant:**
//   (the answer)
//
//   <!-- knt-ai:actions -->
//   - ✔ What the assistant did
//
// The assistant marker carries the usage of the answer as JSON, and what it did (the lines below the actions marker are for
// who reads the note: they are written from that JSON, and ignored when reading). A proposal left without an answer is read
// as not confirmed: nothing runs it afterwards. A message line that is itself exactly one of
// these markers would split the turn there; text before the first marker is ignored.
public static class AiChatSessionTranscript
{
    private const string UserMarker = "<!-- knt-ai:user -->";
    private const string AssistantMarkerStart = "<!-- knt-ai:assistant";
    private const string ActionsMarker = "<!-- knt-ai:actions -->";
    private const string MarkerEnd = "-->";
    private const string UserHeader = "**User:**";
    private const string AssistantHeader = "**Assistant:**";

    private static readonly JsonSerializerOptions UsageJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string Write(IEnumerable<AiChatTurnDto> turns)
    {
        var text = new StringBuilder();
        foreach (var turn in turns)
        {
            var usage = JsonSerializer.Serialize(new TurnUsage
            {
                InputTokens = turn.InputTokens,
                OutputTokens = turn.OutputTokens,
                TotalTokens = turn.TotalTokens,
                ReasoningTokens = turn.ReasoningTokens,
                Automatic = turn.Automatic ? true : null,
                Actions = turn.Actions?.Count > 0 ? turn.Actions.Select(Saved).ToList() : null,
                TokensEstimated = turn.TokensEstimated,
                Truncated = turn.Truncated,
                ProcessingTime = turn.ProcessingTime
            }, UsageJsonOptions);

            text.Append($"{UserMarker}\r\n{UserHeader} \r\n{Content(turn.Prompt)}\r\n\r\n");
            text.Append($"{AssistantMarkerStart} {usage} {MarkerEnd}\r\n{AssistantHeader} \r\n{Content(turn.Answer)}\r\n\r\n");

            if (turn.Actions?.Count > 0)
            {
                text.Append($"{ActionsMarker}\r\n");
                foreach (var action in turn.Actions)
                    text.Append($"- {Content(AiActionRules.Line(action))}\r\n");
                text.Append("\r\n");
            }
        }
        return text.ToString();
    }

    public static List<AiChatTurnDto> Parse(string transcript)
    {
        var turns = new List<AiChatTurnDto>();
        if (string.IsNullOrEmpty(transcript))
            return turns;

        AiChatTurnDto turn = null;
        List<string> section = null;

        void CloseSection(bool isAnswer)
        {
            if (turn == null || section == null)
                return;
            var content = SectionContent(section, isAnswer ? AssistantHeader : UserHeader);
            if (isAnswer)
                turn.Answer = content;
            else
                turn.Prompt = content;
        }

        var inAnswer = false;
        foreach (var line in transcript.ReplaceLineEndings("\n").Split('\n'))
        {
            if (line == ActionsMarker)
            {
                CloseSection(inAnswer);
                section = null;
            }
            else if (line == UserMarker)
            {
                CloseSection(inAnswer);
                turn = new AiChatTurnDto();
                turns.Add(turn);
                section = new List<string>();
                inAnswer = false;
            }
            else if (turn != null && line.StartsWith(AssistantMarkerStart) && line.EndsWith(MarkerEnd))
            {
                CloseSection(inAnswer);
                ApplyUsage(turn, line.Substring(AssistantMarkerStart.Length, line.Length - AssistantMarkerStart.Length - MarkerEnd.Length));
                section = new List<string>();
                inAnswer = true;
            }
            else
                section?.Add(line);
        }
        CloseSection(inAnswer);

        return turns;
    }

    private static string Content(string text) =>
        (text ?? "").ReplaceLineEndings("\r\n").TrimEnd('\r', '\n');

    // The lines of a message without its "**User:**"/"**Assistant:**" header and the blank lines that separate
    // it from the next one.
    private static string SectionContent(List<string> lines, string header)
    {
        var start = lines.Count > 0 && lines[0].Trim() == header ? 1 : 0;
        var end = lines.Count;
        while (end > start && string.IsNullOrWhiteSpace(lines[end - 1]))
            end--;
        return string.Join("\r\n", lines.Skip(start).Take(end - start));
    }

    private static void ApplyUsage(AiChatTurnDto turn, string json)
    {
        try
        {
            var usage = JsonSerializer.Deserialize<TurnUsage>(json.Trim(), UsageJsonOptions);
            if (usage == null)
                return;
            turn.InputTokens = usage.InputTokens;
            turn.OutputTokens = usage.OutputTokens;
            turn.TotalTokens = usage.TotalTokens;
            turn.ReasoningTokens = usage.ReasoningTokens;
            turn.Automatic = usage.Automatic == true;
            turn.Actions = usage.Actions ?? new List<AiActionDto>();
            foreach (var action in turn.Actions.Where(a => a.Status == EnumAiActionStatus.Proposed))
                action.Status = EnumAiActionStatus.NotConfirmed;
            turn.TokensEstimated = usage.TokensEstimated;
            turn.Truncated = usage.Truncated;
            turn.ProcessingTime = usage.ProcessingTime;
        }
        catch (JsonException)
        {
            // Usage is informative only: a hand-edited marker must not lose the conversation.
        }
    }

    // Without the data to run it: it is not run again.
    private static AiActionDto Saved(AiActionDto action) => new()
    {
        ActionId = action.ActionId,
        Kind = action.Kind,
        Status = action.Status,
        Text = action.Text,
        Result = action.Result,
        NoteId = action.NoteId,
        NoteNumber = action.NoteNumber
    };

    private class TurnUsage
    {
        public bool? Automatic { get; set; }
        public List<AiActionDto> Actions { get; set; }
        public long? InputTokens { get; set; }
        public long? OutputTokens { get; set; }
        public long TotalTokens { get; set; }
        public long? ReasoningTokens { get; set; }
        public bool TokensEstimated { get; set; }
        public bool Truncated { get; set; }
        public TimeSpan ProcessingTime { get; set; }
    }
}

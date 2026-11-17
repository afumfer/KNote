using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using KNote.Model.Dto;

namespace KNote.Model.Core;

/// <summary>
/// The text of «Note relevance» (@KNoteRelevance, KNote Atlas, stage 2, chapter 30, D10): two tables people can read, the
/// corrections of the selection made by people and the classification of the doubtful notes by the AI (2.4), and a hidden mark in
/// the last cell of each row with what KNote needs (it reads that, never the columns). Written by KNote; the rows are by note
/// number, the people's corrections the newest first.
/// </summary>
public static class RelevanceNote
{
    public const string CorrectionsHeading = "## Corrections by people";
    public const string AiHeading = "## AI classification";

    private const string CorrectionMarker = "knt-relevance";
    private const string AiMarker = "knt-relevance-ai";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };
    private static readonly Regex Mark = new(@"<!--\s*(knt-relevance(?:-ai)?)\s+(\{.*?\})\s*-->", RegexOptions.Compiled);

    public const string Intro =
        "KNote keeps here the corrections of the selection of the summaries («Review selection» in KNote Atlas › Period summary) and " +
        "what the AI decided about the notes that were doubtful when it was asked. An answer of the AI is not used once its note " +
        "changes (the note is doubtful again), and it goes away the next time KNote writes this note. Don't edit it by hand: KNote writes it.";

    private sealed record CorrectionMark(Guid NoteId, EnumSelectionDecision Decision, string User, DateTime Date);

    private sealed record AiMark(Guid NoteId, EnumSelectionDecision Decision, string Hash, string Model, DateTime Date);

    /// <summary>The corrections and the answers of the AI of the text; the rows without a valid mark are ignored, and a note only once
    /// (its first row).</summary>
    public static (List<RelevanceCorrectionDto> Corrections, List<RelevanceAiAnswerDto> AiAnswers) Parse(string markdown)
    {
        var corrections = new List<RelevanceCorrectionDto>();
        var answers = new List<RelevanceAiAnswerDto>();
        foreach (var line in (markdown ?? "").Replace("\r\n", "\n").Split('\n'))
        {
            var match = Mark.Match(line);
            if (!match.Success || !line.TrimStart().StartsWith('|'))
                continue;
            var cells = Cells(line);
            try
            {
                if (match.Groups[1].Value == CorrectionMarker)
                {
                    var mark = JsonSerializer.Deserialize<CorrectionMark>(match.Groups[2].Value, JsonOptions);
                    if (mark == null || mark.NoteId == Guid.Empty || corrections.Any(c => c.NoteId == mark.NoteId))
                        continue;
                    corrections.Add(new RelevanceCorrectionDto
                    {
                        NoteId = mark.NoteId, NoteNumber = Number(cells), Topic = Cell(cells, 1), Decision = mark.Decision,
                        UserName = mark.User, Date = mark.Date
                    });
                }
                else
                {
                    var mark = JsonSerializer.Deserialize<AiMark>(match.Groups[2].Value, JsonOptions);
                    if (mark == null || mark.NoteId == Guid.Empty || answers.Any(a => a.NoteId == mark.NoteId))
                        continue;
                    answers.Add(new RelevanceAiAnswerDto
                    {
                        NoteId = mark.NoteId, NoteNumber = Number(cells), Topic = Cell(cells, 1), Decision = mark.Decision,
                        Reason = Cell(cells, 3), ContentHash = mark.Hash, Model = mark.Model, Date = mark.Date
                    });
                }
            }
            catch (JsonException)
            {
            }
        }
        return (corrections, answers);
    }

    /// <summary>The text with these corrections (the newest first) and answers of the AI (by number).</summary>
    public static string Write(IEnumerable<RelevanceCorrectionDto> corrections, IEnumerable<RelevanceAiAnswerDto> aiAnswers)
    {
        var text = new StringBuilder();
        text.Append(Intro).Append("\n\n");

        text.Append(CorrectionsHeading).Append("\n\n");
        text.Append("| Note | Topic | Decision | Who | Date |\n|---|---|---|---|---|\n");
        foreach (var c in (corrections ?? Enumerable.Empty<RelevanceCorrectionDto>()).OrderByDescending(c => c.Date).ThenBy(c => c.NoteNumber))
        {
            var mark = JsonSerializer.Serialize(new CorrectionMark(c.NoteId, c.Decision, c.UserName, c.Date), JsonOptions);
            text.Append($"| #{c.NoteNumber} | {Escape(c.Topic)} | {c.Decision} | {Escape(c.UserName)} | {c.Date:yyyy-MM-dd} <!-- {CorrectionMarker} {mark} --> |\n");
        }

        text.Append('\n').Append(AiHeading).Append("\n\n");
        text.Append("| Note | Topic | Decision | Reason |\n|---|---|---|---|\n");
        foreach (var a in (aiAnswers ?? Enumerable.Empty<RelevanceAiAnswerDto>()).OrderBy(a => a.NoteNumber))
        {
            var mark = JsonSerializer.Serialize(new AiMark(a.NoteId, a.Decision, a.ContentHash, a.Model, a.Date), JsonOptions);
            text.Append($"| #{a.NoteNumber} | {Escape(a.Topic)} | {a.Decision} | {Escape(a.Reason)} <!-- {AiMarker} {mark} --> |\n");
        }
        return text.ToString();
    }

    /// <summary>The corrections after the changes of «Review selection»: each change replaces the note's correction (its number and
    /// topic from the selection) or takes it away (Decision null).</summary>
    public static List<RelevanceCorrectionDto> Apply(IEnumerable<RelevanceCorrectionDto> corrections,
        IEnumerable<RelevanceCorrectionChangeDto> changes, Func<Guid, (int NoteNumber, string Topic)?> noteOf, string userName, DateTime now)
    {
        var result = (corrections ?? Enumerable.Empty<RelevanceCorrectionDto>()).ToDictionary(c => c.NoteId);
        foreach (var change in changes ?? Enumerable.Empty<RelevanceCorrectionChangeDto>())
        {
            if (change.Decision is not (EnumSelectionDecision.In or EnumSelectionDecision.Out))
            {
                result.Remove(change.NoteId);
                continue;
            }
            var note = noteOf(change.NoteId);
            if (note == null)
                continue;
            result[change.NoteId] = new RelevanceCorrectionDto
            {
                NoteId = change.NoteId, NoteNumber = note.Value.NoteNumber, Topic = note.Value.Topic, Decision = change.Decision.Value,
                UserName = userName, Date = now
            };
        }
        return result.Values.ToList();
    }

    /// <summary>The answers of the AI after a new batch: each one replaces the note's one.</summary>
    public static List<RelevanceAiAnswerDto> ApplyAiAnswers(IEnumerable<RelevanceAiAnswerDto> answers, IEnumerable<RelevanceAiAnswerDto> newAnswers)
    {
        var result = (answers ?? Enumerable.Empty<RelevanceAiAnswerDto>()).ToDictionary(a => a.NoteId);
        foreach (var answer in newAnswers ?? Enumerable.Empty<RelevanceAiAnswerDto>())
            if (answer.Decision is EnumSelectionDecision.In or EnumSelectionDecision.Out)
                result[answer.NoteId] = answer;
        return result.Values.ToList();
    }

    // A cell's text in one line, without the separators of the table.
    private static string Escape(string text) =>
        (text ?? "").Replace("\r", " ").Replace("\n", " ").Replace("|", "\\|").Replace("<!--", "<!- -").Trim();

    private static List<string> Cells(string line)
    {
        var withoutMark = Mark.Replace(line, "");
        var cells = Regex.Split(withoutMark.Trim().Trim('|'), @"(?<!\\)\|").Select(c => c.Trim().Replace("\\|", "|")).ToList();
        return cells;
    }

    private static string Cell(List<string> cells, int index) => index < cells.Count ? cells[index] : "";

    private static int Number(List<string> cells) => int.TryParse(Cell(cells, 0).TrimStart('#'), out var number) ? number : 0;
}

using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;
using KNote.Service.Core;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace KNote.Ai;

/// <summary>
/// Asks the AI whether a batch of doubtful notes of the selection of a summary goes in or is noise (KNote Atlas, stage 2, 2.4, D9,
/// C1-C6): the task ClassifyNote, without tools (AiResourceTask), with the notes as data (NoteClassificationRules.Request) and the
/// glossary terms of their topics; its answers, read line by line, are kept in «Note relevance» with each note's fingerprint and the
/// model. A note it did not answer stays doubtful. One batch per call: the apps ask the batches one by one, with their progress and
/// their cancellation between batches; the summary of a period (2.5) will call it too.
/// </summary>
public static class AiNoteClassifier
{
    /// <summary>The answers kept for the batch (at most NoteClassificationRules.BatchSize notes).</summary>
    public static async Task<Result<List<RelevanceAiAnswerDto>>> ClassifyAsync(IKntService service, IChatClient chatClient, AiProviderRef provider,
        IReadOnlyList<Guid> noteIds, ILogger logger = null, CancellationToken cancellationToken = default)
    {
        var result = new Result<List<RelevanceAiAnswerDto>>(new List<RelevanceAiAnswerDto>());
        var ids = (noteIds ?? Array.Empty<Guid>()).Distinct().Take(NoteClassificationRules.BatchSize).ToList();
        if (ids.Count == 0)
            return result;

        var notes = await service.Summaries.GetNotesToClassifyAsync(ids);
        if (!notes.IsValid)
            return Failed(result, notes);
        if (notes.Entity.Count == 0)
            return result;

        var system = await AiResourceTask.SystemPromptAsync(service, AiTaskCatalog.ClassifyNote, notes.Entity.Select(n => n.Topic), logger);
        List<ChatMessage> messages =
        [
            new(ChatRole.System, system),
            new(ChatRole.User, NoteClassificationRules.Request(notes.Entity))
        ];
        var answer = await AiResourceTask.AnswerAsync(chatClient, provider, messages, cancellationToken, NoteClassificationRules.MaxOutputTokens);

        var answers = NoteClassificationRules.Parse(answer, notes.Entity, provider.Model, DateTime.Now);
        if (answers.Count < notes.Entity.Count)
            logger?.LogInformation("The AI answered {answered} of {asked} doubtful notes.", answers.Count, notes.Entity.Count);
        if (answers.Count == 0)
            return result;

        var saved = await service.Summaries.SaveRelevanceAiAnswersAsync(new RelevanceAiAnswersSaveDto { Answers = answers });
        if (!saved.IsValid)
            return Failed(result, saved);
        result.Entity = saved.Entity;
        return result;
    }

    private static Result<List<RelevanceAiAnswerDto>> Failed(Result<List<RelevanceAiAnswerDto>> result, ResultBase failed)
    {
        result.AddListErrorMessage(failed.ListErrorMessage);
        if (failed.NotAuthorized)
            result.NotAuthorized = true;
        return result;
    }
}

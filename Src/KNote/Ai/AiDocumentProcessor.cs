using System.Diagnostics;
using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;
using KNote.Service.Core;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace KNote.Ai;

/// <summary>
/// Summarizes a long document of a note (KNote Atlas, stage 2, CU-43, D21), the same in the Server and in ClientWin: the document
/// in pieces (its pages, or parts cut by paragraphs), read by AiSummaryEngine with the task SummarizeLongDocument, and the summary
/// kept in a new note (IKntSummaryService.SaveDocumentSummaryAsync). Canceled or failed, nothing is kept. The chat client is made
/// with the context KNote fixes (AiSummaryContext): <c>createChatClient</c> builds it for the provider it receives.
/// </summary>
public static class AiDocumentProcessor
{
    /// <summary>What the summary of the document would take: pages, calls and, with the pace of the model, its time (D18).</summary>
    public static async Task<Result<DocumentProcessEstimateDto>> EstimateAsync(IKntService service, AiProviderRef provider,
        DocumentProcessRequestDto request, ILogger logger = null, CancellationToken cancellationToken = default)
    {
        var result = new Result<DocumentProcessEstimateDto>();

        var prepared = await service.Summaries.PrepareDocumentAsync(request.ResourceId);
        if (!prepared.IsValid)
        {
            result.AddListErrorMessage(prepared.ListErrorMessage);
            return result;
        }
        var document = prepared.Entity;
        var pieces = SummaryPieces.FromDocument(document.Text, document.Paged).Pieces;

        var contextTokens = await AiSummaryContext.TokensAsync(provider, cancellationToken);
        var system = await SystemPromptAsync(service, document, logger);
        var (batch, final) = AiSummaryEngine.Budgets(contextTokens, system.Length, SummaryBatches.OutputTokens((int)request.Length));
        var tooSmall = AiSummaryEngine.ContextTooSmall(contextTokens, batch, final);
        if (tooSmall != null)
        {
            result.AddErrorMessage(tooSmall);
            return result;
        }
        var plan = SummaryEstimate.For(pieces, batch, final);

        var state = await service.Summaries.GetStateAsync();
        double? pace = state.IsValid && state.Entity?.SecondsPer1000Characters.TryGetValue(provider.Model ?? "", out var known) == true ? known : null;

        result.Entity = new DocumentProcessEstimateDto
        {
            FileName = document.FileName,
            NoteNumber = document.NoteNumber,
            Pages = document.PagesCount,
            Paged = document.Paged,
            Characters = pieces.Sum(p => p.Text.Length),
            Batches = plan.Batches,
            Calls = plan.Calls,
            ContextTokens = contextTokens,
            MissingPages = document.MissingPages,
            Model = provider.Model,
            IsPaid = provider.IsPaid,
            Seconds = SummaryEstimate.Seconds(plan.CharactersRead, pace)
        };
        return result;
    }

    /// <summary>
    /// Summarizes the document and keeps the summary in a new note. Entity: how it ended (Completed with the note, Canceled or
    /// Failed with why); progress, as it goes.
    /// </summary>
    public static async Task<Result<DocumentProcessProgressDto>> SummarizeAsync(IKntService service, Func<AiProviderRef, IChatClient> createChatClient,
        AiProviderRef provider, DocumentProcessRequestDto request, IProgress<DocumentProcessProgressDto> progress = null, ILogger logger = null,
        CancellationToken cancellationToken = default)
    {
        var clock = Stopwatch.StartNew();
        var current = new DocumentProcessProgressDto
        {
            ResourceId = request.ResourceId,
            Model = provider.Model,
            Phase = EnumDocumentProcessPhase.Preparing,
            StartedDateTime = DateTime.Now
        };
        void Report(EnumDocumentProcessPhase phase, int done, int total)
        {
            current.Phase = phase;
            current.CallsDone = done;
            current.CallsTotal = total;
            current.ElapsedSeconds = clock.Elapsed.TotalSeconds;
            current.RemainingSeconds = done > 0 && total > done ? current.ElapsedSeconds / done * (total - done) : null;
            progress?.Report(current.Copy());
        }

        try
        {
            Report(EnumDocumentProcessPhase.Preparing, 0, 0);
            var prepared = await service.Summaries.PrepareDocumentAsync(request.ResourceId);
            if (!prepared.IsValid)
                return Failed(current, prepared.ErrorMessage, progress);
            var document = prepared.Entity;
            current.FileName = document.FileName;
            var pieces = SummaryPieces.FromDocument(document.Text, document.Paged).Pieces;

            var contextTokens = await AiSummaryContext.TokensAsync(provider, cancellationToken);
            var chatClient = createChatClient(AiSummaryContext.With(provider, contextTokens));
            var system = await SystemPromptAsync(service, document, logger);

            var requests = new AiSummaryEngine.Requests(
                Batch: batch => LongDocumentRules.BatchRequest(document.FileName, batch),
                Join: notes => LongDocumentRules.JoinRequest(document.FileName, notes, document.Paged),
                Final: notes => LongDocumentRules.FinalRequest(document.FileName, document.PagesCount, document.Paged, notes, request.Length, request.Language),
                FinalOutputTokens: SummaryBatches.OutputTokens((int)request.Length));
            var outcome = await AiSummaryEngine.RunAsync(chatClient, provider, system, contextTokens, pieces, requests,
                step => Report(step.Phase, step.CallsDone, step.CallsTotal), cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            Report(EnumDocumentProcessPhase.Saving, outcome.Calls, outcome.Calls);
            var saved = await service.Summaries.SaveDocumentSummaryAsync(new DocumentSummarySaveDto
            {
                ResourceId = document.ResourceId,
                ExpectedResourceName = document.ResourceName,
                Model = provider.Model,
                Language = request.Language,
                Length = request.Length,
                Summary = outcome.Text,
                PagesCount = document.PagesCount,
                MissingPages = document.MissingPages,
                CharactersRead = outcome.CharactersRead,
                Seconds = clock.Elapsed.TotalSeconds
            });
            if (!saved.IsValid)
                return Failed(current, saved.ErrorMessage, progress);

            current.NoteId = saved.Entity.NoteId;
            current.NoteNumber = saved.Entity.NoteNumber;
            Report(EnumDocumentProcessPhase.Completed, outcome.Calls, outcome.Calls);
            return new Result<DocumentProcessProgressDto>(current.Copy());
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            current.Phase = EnumDocumentProcessPhase.Canceled;
            current.RemainingSeconds = null;
            current.ElapsedSeconds = clock.Elapsed.TotalSeconds;
            progress?.Report(current.Copy());
            return new Result<DocumentProcessProgressDto>(current.Copy());
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger?.LogWarning(ex, "KNote Atlas: the summary of the resource {resourceId} failed.", request.ResourceId);
            return Failed(current, ex.Message, progress);
        }
    }

    // What the AI receives for the task, with the glossary terms of the document.
    private static Task<string> SystemPromptAsync(IKntService service, DocumentToSummarizeDto document, ILogger logger) =>
        AiResourceTask.SystemPromptAsync(service, AiTaskCatalog.SummarizeLongDocument, [document.NoteTopic, document.FileName, document.Text], logger);

    private static Result<DocumentProcessProgressDto> Failed(DocumentProcessProgressDto current, string message, IProgress<DocumentProcessProgressDto> progress)
    {
        current.Phase = EnumDocumentProcessPhase.Failed;
        current.Message = message;
        current.RemainingSeconds = null;
        progress?.Report(current.Copy());
        var result = new Result<DocumentProcessProgressDto>(current.Copy());
        result.AddErrorMessage(message);
        return result;
    }
}

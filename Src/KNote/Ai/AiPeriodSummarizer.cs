using System.Diagnostics;
using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;
using KNote.Service.Core;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace KNote.Ai;

/// <summary>
/// The summary of a period or of a subject (KNote Atlas, stage 2, 2.5, D11, P1-P7), the same in the Server and in ClientWin: one more
/// kind of the jobs of the summaries (AiDocumentProcessor.ProcessAsync with EnumDocumentProcessKind.PeriodSummary). It prepares the
/// selection; asks the AI first about the doubtful notes left (AiNoteClassifier, at most NoteClassificationRules.MaxPerRun: the
/// others go in, P4) and prepares it again with its answers; reads the notes that go in as pieces (PeriodSummaryRules) with
/// AiSummaryEngine and the task SummarizePeriod; and keeps the summary in a new note of «KNote Summary». Canceled or failed, the summary
/// is not kept (the answers of the AI about the doubtful notes, already kept, stay).
/// </summary>
public static class AiPeriodSummarizer
{
    /// <summary>What the summary would take: the notes, the doubtful ones to ask about, the calls and, with the pace of the model, its time.</summary>
    public static async Task<Result<DocumentProcessEstimateDto>> EstimateAsync(IKntService service, AiProviderRef provider,
        DocumentProcessRequestDto request, ILogger logger = null, CancellationToken cancellationToken = default)
    {
        var result = new Result<DocumentProcessEstimateDto>();
        var period = Period(request, result);
        if (period == null)
            return result;

        var prepared = await service.Summaries.PreparePeriodAsync(period, cancellationToken);
        if (!prepared.IsValid)
        {
            result.AddListErrorMessage(prepared.ListErrorMessage);
            return result;
        }
        if (prepared.Entity.Notes.Count == 0)
        {
            result.AddErrorMessage(PeriodSummaryRules.NoNotes);
            return result;
        }

        var subject = Subject(period);
        var pieces = PeriodSummaryRules.Pieces(prepared.Entity.Notes);
        var contextTokens = await AiSummaryContext.TokensAsync(provider, cancellationToken);
        var system = await SystemPromptAsync(service, subject, prepared.Entity.Notes, logger);
        var (batch, final) = AiSummaryEngine.Budgets(contextTokens, system.Length, SummaryBatches.OutputTokens(PeriodSummaryRules.ClampWords(period.Words)));
        var tooSmall = AiSummaryEngine.ContextTooSmall(contextTokens, batch, final);
        if (tooSmall != null)
        {
            result.AddErrorMessage(tooSmall);
            return result;
        }
        var plan = SummaryEstimate.For(pieces, batch, final);
        var doubtful = NoteClassificationRules.ToClassify(prepared.Entity.Selection).Count;

        var state = await service.Summaries.GetStateAsync();
        double? pace = state.IsValid && state.Entity?.PeriodSecondsPer1000Characters?.TryGetValue(provider.Model ?? "", out var known) == true ? known : null;

        result.Entity = new DocumentProcessEstimateDto
        {
            Kind = EnumDocumentProcessKind.PeriodSummary,
            FileName = subject,
            Notes = prepared.Entity.Notes.Count,
            DoubtfulToClassify = doubtful,
            Characters = pieces.Sum(p => p.Text.Length),
            Batches = plan.Batches,
            Calls = plan.Calls + NoteClassificationRules.Batches(Enumerable.Range(0, doubtful).Select(_ => Guid.NewGuid())).Count,
            ContextTokens = contextTokens,
            Model = provider.Model,
            IsPaid = provider.IsPaid,
            Seconds = SummaryEstimate.Seconds(plan.CharactersRead, pace)
        };
        return result;
    }

    /// <summary>Makes the summary and keeps it in a new note. Entity: how it ended (Completed with the note, Canceled or Failed with why).</summary>
    public static async Task<Result<DocumentProcessProgressDto>> SummarizeAsync(IKntService service, Func<AiProviderRef, IChatClient> createChatClient,
        AiProviderRef provider, DocumentProcessRequestDto request, IProgress<DocumentProcessProgressDto> progress = null, ILogger logger = null,
        CancellationToken cancellationToken = default)
    {
        var clock = Stopwatch.StartNew();
        var check = new Result();
        var period = Period(request, check);
        var current = new DocumentProcessProgressDto
        {
            Kind = EnumDocumentProcessKind.PeriodSummary,
            FileName = period == null ? null : Subject(period),
            Model = provider.Model,
            Phase = EnumDocumentProcessPhase.Preparing,
            StartedDateTime = DateTime.Now
        };
        if (period == null)
            return Failed(current, check.ErrorMessage, progress);

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
            var prepared = await service.Summaries.PreparePeriodAsync(period, cancellationToken);
            if (!prepared.IsValid)
                return Failed(current, prepared.ErrorMessage, progress);

            // The doubtful notes left, asked to the AI first (P4); then the selection with its answers.
            var batches = NoteClassificationRules.Batches(NoteClassificationRules.ToClassify(prepared.Entity.Selection).Select(n => n.NoteId));
            var classifyCalls = batches.Count;
            if (classifyCalls > 0)
            {
                var classifier = createChatClient(provider);
                for (var i = 0; i < batches.Count; i++)
                {
                    Report(EnumDocumentProcessPhase.Classifying, i, classifyCalls);
                    var answers = await AiNoteClassifier.ClassifyAsync(service, classifier, provider, batches[i], logger, cancellationToken);
                    if (!answers.IsValid)
                        logger?.LogWarning("KNote Atlas: a batch of doubtful notes could not be classified: {error}", answers.ErrorMessage);
                }
                prepared = await service.Summaries.PreparePeriodAsync(period, cancellationToken);
                if (!prepared.IsValid)
                    return Failed(current, prepared.ErrorMessage, progress);
            }
            if (prepared.Entity.Notes.Count == 0)
                return Failed(current, PeriodSummaryRules.NoNotes, progress);

            var notes = prepared.Entity.Notes;
            var pieces = PeriodSummaryRules.Pieces(notes);
            var words = PeriodSummaryRules.ClampWords(period.Words);
            var contextTokens = await AiSummaryContext.TokensAsync(provider, cancellationToken);
            var chatClient = createChatClient(AiSummaryContext.With(provider, contextTokens));
            var system = await SystemPromptAsync(service, current.FileName, notes, logger);

            var selection = period.Selection;
            var requests = new AiSummaryEngine.Requests(
                Batch: batch => PeriodSummaryRules.BatchRequest(current.FileName, batch),
                Join: joined => PeriodSummaryRules.JoinRequest(current.FileName, joined),
                Final: joined => PeriodSummaryRules.FinalRequest(current.FileName, selection.From, selection.To, notes.Count, joined, words, request.Language),
                FinalOutputTokens: SummaryBatches.OutputTokens(words));
            var outcome = await AiSummaryEngine.RunAsync(chatClient, provider, system, contextTokens, pieces, requests,
                step => Report(step.Phase, classifyCalls + step.CallsDone, classifyCalls + step.CallsTotal), cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            var calls = classifyCalls + outcome.Calls;
            Report(EnumDocumentProcessPhase.Saving, calls, calls);
            var saved = await service.Summaries.SavePeriodSummaryAsync(new PeriodSummarySaveDto
            {
                Request = period,
                Model = provider.Model,
                Language = request.Language,
                Summary = outcome.Text,
                NotesRead = notes.Count,
                NotesLeftOut = prepared.Entity.Selection.Out,
                KNoteNotes = prepared.Entity.Selection.KNoteNotes,
                CharactersRead = outcome.CharactersRead,
                Seconds = clock.Elapsed.TotalSeconds
            });
            if (!saved.IsValid)
                return Failed(current, saved.ErrorMessage, progress);

            current.NoteId = saved.Entity.NoteId;
            current.NoteNumber = saved.Entity.NoteNumber;
            Report(EnumDocumentProcessPhase.Completed, calls, calls);
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
            logger?.LogWarning(ex, "KNote Atlas: the period summary «{subject}» failed.", current.FileName);
            return Failed(current, ex.Message, progress);
        }
    }

    // The request of the period, with its length kept between the limits.
    private static PeriodSummaryRequestDto Period(DocumentProcessRequestDto request, ResultBase result)
    {
        if (request?.Period?.Selection == null)
        {
            result.AddErrorMessage("The period of the summary is required.");
            return null;
        }
        request.Period.Words = PeriodSummaryRules.ClampWords(request.Period.Words);
        return request.Period;
    }

    private static string Subject(PeriodSummaryRequestDto period) =>
        string.IsNullOrWhiteSpace(period.Subject) ? PeriodSummaryRules.DefaultSubject(period.Selection) : period.Subject.Trim();

    // What the AI receives for the task, with the glossary terms of the notes.
    private static Task<string> SystemPromptAsync(IKntService service, string subject, IEnumerable<PeriodNoteDto> notes, ILogger logger) =>
        AiResourceTask.SystemPromptAsync(service, AiTaskCatalog.SummarizePeriod,
            new[] { subject }.Concat(notes.Select(n => n.Topic)).Concat(notes.Select(n => n.Text)), logger);

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

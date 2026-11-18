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
        var plan = Plan(prepared.Entity.Notes, period.Words, contextTokens, system.Length);
        if (plan.TooSmall != null)
        {
            result.AddErrorMessage(plan.TooSmall);
            return result;
        }
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
            Sections = plan.Sections.Count,
            Batches = plan.SectionPlans.Sum(p => p.Batches),
            Calls = plan.Calls + NoteClassificationRules.Batches(Enumerable.Range(0, doubtful).Select(_ => Guid.NewGuid())).Count,
            ContextTokens = contextTokens,
            Model = provider.Model,
            IsPaid = provider.IsPaid,
            Seconds = SummaryEstimate.Seconds(plan.CharactersRead, pace)
        };
        return result;
    }

    // How a summary is written (2.6, D13): its parts, the calls of each one, the executive summary and what is open (two calls with the
    // notes of all the parts, as many as fit), and the budgets of the calls in this context.
    private sealed record WritingPlan(List<PeriodSection> Sections, List<SummaryEstimate.Plan> SectionPlans, int ExecutiveWords, int PendingWords,
        int BatchCharacters, int SummaryCharacters, string TooSmall)
    {
        public int Calls => SectionPlans.Sum(p => p.Calls) + 2;

        // The notes of the parts are read twice more, by the executive summary and by what is open.
        public int CharactersRead => SectionPlans.Sum(p => p.CharactersRead) + 2 * Math.Min(SummaryCharacters, SectionPlans.Sum(p => p.Batches) * SummaryEstimate.NotesCharacters);
    }

    private static WritingPlan Plan(IReadOnlyList<PeriodNoteDto> notes, int words, int contextTokens, int systemCharacters)
    {
        var sections = PeriodSummarySections.Plan(notes, words);
        var (executiveWords, pendingWords, _) = PeriodSummarySections.Words(words);
        var finalWords = sections.Select(s => s.Words).Append(executiveWords).Append(pendingWords).Max();
        var (batch, final) = AiSummaryEngine.Budgets(contextTokens, systemCharacters, SummaryBatches.OutputTokens(finalWords));
        var tooSmall = AiSummaryEngine.ContextTooSmall(contextTokens, batch, final);
        var summary = AiSummaryEngine.Budgets(contextTokens, systemCharacters, SummaryBatches.OutputTokens(Math.Max(executiveWords, pendingWords))).Final;
        var plans = tooSmall != null ? new List<SummaryEstimate.Plan>() : sections
            .Select(s => SummaryEstimate.For(PeriodSummaryRules.Pieces(s.Notes), batch,
                AiSummaryEngine.Budgets(contextTokens, systemCharacters, SummaryBatches.OutputTokens(s.Words)).Final))
            .ToList();
        return new WritingPlan(sections, plans, executiveWords, pendingWords, batch, summary, tooSmall);
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
            var subject = current.FileName;
            var contextTokens = await AiSummaryContext.TokensAsync(provider, cancellationToken);
            var chatClient = createChatClient(AiSummaryContext.With(provider, contextTokens));
            var system = await SystemPromptAsync(service, subject, notes, logger);
            var plan = Plan(notes, period.Words, contextTokens, system.Length);
            if (plan.TooSmall != null)
                return Failed(current, plan.TooSmall, progress);

            // «What happened», a part at a time (D13): each one, the engine with its notes.
            var selection = period.Selection;
            var done = 0;
            var read = 0;
            var sections = new List<PeriodSummarySectionDto>();
            var partNotes = new List<string>();
            for (var i = 0; i < plan.Sections.Count; i++)
            {
                var section = plan.Sections[i];
                var before = done;
                var after = plan.SectionPlans.Skip(i + 1).Sum(p => p.Calls) + 2;
                var requests = new AiSummaryEngine.Requests(
                    Batch: batch => PeriodSummaryRules.BatchRequest(subject, batch),
                    Join: joined => PeriodSummaryRules.JoinRequest(subject, joined),
                    Final: joined => PeriodSummaryRules.SectionRequest(subject, section.Title, selection.From, selection.To, section.Notes.Count, joined,
                        section.Words, request.Language),
                    FinalOutputTokens: SummaryBatches.OutputTokens(section.Words));
                var outcome = await AiSummaryEngine.RunAsync(chatClient, provider, system, contextTokens, PeriodSummaryRules.Pieces(section.Notes), requests,
                    step => Report(step.Phase, classifyCalls + before + step.CallsDone, classifyCalls + before + step.CallsTotal + after), cancellationToken);
                done += outcome.Calls;
                read += outcome.CharactersRead;
                partNotes.AddRange(outcome.Notes);
                sections.Add(new PeriodSummarySectionDto { Title = section.Title, Text = outcome.Text, NoteNumbers = section.Notes.Select(n => n.NoteNumber).ToList() });
            }

            // The executive summary and what is open, from the notes of all the parts (joined if they don't fit together).
            var joinedNotes = await AiSummaryEngine.JoinAsync(chatClient, provider, system, partNotes, plan.BatchCharacters, plan.SummaryCharacters,
                joined => PeriodSummaryRules.JoinRequest(subject, joined),
                (joins, planned) => Report(EnumDocumentProcessPhase.Joining, classifyCalls + done + joins, classifyCalls + done + planned + 2), cancellationToken);
            done += joinedNotes.Calls;
            read += joinedNotes.CharactersRead;

            async Task<string> WriteAsync(string partRequest, int words, int left)
            {
                Report(EnumDocumentProcessPhase.Writing, classifyCalls + done, classifyCalls + done + left);
                var text = await AiSummaryEngine.AskAsync(chatClient, provider, system, partRequest, SummaryBatches.OutputTokens(words), cancellationToken);
                done++;
                read += partRequest.Length;
                return text;
            }
            var executive = await WriteAsync(PeriodSummaryRules.ExecutiveRequest(subject, selection.From, selection.To, notes.Count, joinedNotes.Notes,
                plan.ExecutiveWords, request.Language), plan.ExecutiveWords, 2);
            var pending = await WriteAsync(PeriodSummaryRules.PendingRequest(subject, selection.From, selection.To, notes.Count, joinedNotes.Notes,
                plan.PendingWords, request.Language), plan.PendingWords, 1);

            cancellationToken.ThrowIfCancellationRequested();
            var calls = classifyCalls + done;
            Report(EnumDocumentProcessPhase.Saving, calls, calls);
            var saved = await service.Summaries.SavePeriodSummaryAsync(new PeriodSummarySaveDto
            {
                Request = period,
                Model = provider.Model,
                Language = request.Language,
                Executive = executive,
                Sections = sections,
                Pending = pending,
                Prepared = prepared.Entity,
                Calls = calls,
                CharactersRead = read,
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

using System.Diagnostics;
using System.Text;
using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Service.ServicesCommands;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace KNote.Ai;

/// <summary>
/// Summarizes or translates a long document of a note (KNote Atlas, stage 2, CU-43, D21), the same in the Server and in ClientWin.
/// The summary: the document in pieces (its pages, or parts cut by paragraphs), read by AiSummaryEngine with the task
/// SummarizeLongDocument, and kept in a new note (IKntSummaryService.SaveDocumentSummaryAsync); canceled or failed, nothing is kept.
/// The translation (2.1b): part by part (DocumentTranslationRules) with the task TranslateDocument and the glossary terms of each part,
/// kept as it goes in the document of a new note (IKntSummaryService.SaveDocumentTranslationAsync); canceled or failed, what was
/// translated stays and it can go on. The chat client is made with the context KNote fixes (AiSummaryContext): <c>createChatClient</c>
/// builds it for the provider it receives.
/// </summary>
public static class AiDocumentProcessor
{
    /// <summary>The summary or the translation the request asks for (the jobs of both apps).</summary>
    public static Task<Result<DocumentProcessProgressDto>> ProcessAsync(IKntService service, Func<AiProviderRef, IChatClient> createChatClient,
        AiProviderRef provider, DocumentProcessRequestDto request, IProgress<DocumentProcessProgressDto> progress = null, ILogger logger = null,
        CancellationToken cancellationToken = default) =>
        request.Kind == EnumDocumentProcessKind.Translate
            ? TranslateAsync(service, createChatClient, provider, request, progress, logger, cancellationToken)
            : SummarizeAsync(service, createChatClient, provider, request, progress, logger, cancellationToken);

    /// <summary>What the summary or the translation of the document would take: pages, calls and, with the pace of the model, its time (D18).</summary>
    public static async Task<Result<DocumentProcessEstimateDto>> EstimateAsync(IKntService service, AiProviderRef provider,
        DocumentProcessRequestDto request, ILogger logger = null, CancellationToken cancellationToken = default)
    {
        if (request.Kind == EnumDocumentProcessKind.Translate)
            return await EstimateTranslationAsync(service, provider, request, logger, cancellationToken);

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
            Kind = EnumDocumentProcessKind.Summarize,
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
            Kind = EnumDocumentProcessKind.Summarize,
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

    #region Translation (2.1b)

    // What a translation will do: the document, its parts, where it starts (the next part of one that goes on), in which language,
    // the context and the calls.
    private sealed record TranslationPlan(DocumentToSummarizeDto Document, List<TranslationPart> Parts, int From, DocumentTranslationDto Continued,
        string Language, int ContextTokens, List<(int From, int To)> Calls)
    {
        public int CallsWithText => Calls.Count(c => DocumentTranslationRules.HasText(Parts, c));
    }

    private static async Task<(TranslationPlan Plan, Result Error)> PlanTranslationAsync(IKntService service, AiProviderRef provider,
        DocumentProcessRequestDto request, ILogger logger, CancellationToken cancellationToken)
    {
        var error = new Result();
        var prepared = await service.Summaries.PrepareDocumentAsync(request.ResourceId);
        if (!prepared.IsValid)
        {
            error.AddListErrorMessage(prepared.ListErrorMessage);
            if (prepared.NotAuthorized)
                error.NotAuthorized = true;
            return (null, error);
        }
        var document = prepared.Entity;
        var parts = DocumentTranslationRules.Parts(document.Text, document.Paged);

        var language = request.Language?.Trim();
        DocumentTranslationDto continued = null;
        if (request.ContinueNoteId is Guid noteId)
        {
            var translations = await service.Summaries.GetDocumentTranslationsAsync(request.ResourceId);
            continued = translations.IsValid ? translations.Entity?.FirstOrDefault(t => t.NoteId == noteId) : null;
            if (continued == null || continued.ResourceName != document.ResourceName || continued.Parts != parts.Count)
                error.AddErrorMessage(KntSummaryTexts.TranslationChanged);
            else if (continued.Finished)
                error.AddErrorMessage("The translation is finished.");
            else
                language = continued.Language;
        }
        if (error.IsValid && string.IsNullOrWhiteSpace(language))
            error.AddErrorMessage(KntSummaryTexts.LanguageRequired);
        if (!error.IsValid)
            return (null, error);

        // The longest system prompt (with the glossary terms of the whole document): the one of each part fits in it.
        var contextTokens = await AiSummaryContext.TokensAsync(provider, cancellationToken);
        var system = await TranslationPromptAsync(service, document, document.Text, logger);
        var callCharacters = DocumentTranslationRules.CallCharacters(contextTokens, system.Length);
        var tooSmall = DocumentTranslationRules.ContextTooSmall(contextTokens, callCharacters);
        if (tooSmall != null)
        {
            error.AddErrorMessage(tooSmall);
            return (null, error);
        }

        var from = continued?.NextPart ?? 0;
        var calls = DocumentTranslationRules.Calls(parts, from, document.Paged, callCharacters);
        return (new TranslationPlan(document, parts, from, continued, language, contextTokens, calls), null);
    }

    private static async Task<Result<DocumentProcessEstimateDto>> EstimateTranslationAsync(IKntService service, AiProviderRef provider,
        DocumentProcessRequestDto request, ILogger logger, CancellationToken cancellationToken)
    {
        var result = new Result<DocumentProcessEstimateDto>();
        var (plan, error) = await PlanTranslationAsync(service, provider, request, logger, cancellationToken);
        if (error != null)
        {
            result.AddListErrorMessage(error.ListErrorMessage);
            result.NotAuthorized = error.NotAuthorized;
            return result;
        }

        var document = plan.Document;
        var characters = DocumentTranslationRules.CharactersFrom(plan.Parts, plan.From);
        var state = await service.Summaries.GetStateAsync();
        double? pace = state.IsValid && state.Entity?.TranslationSecondsPer1000Characters?.TryGetValue(provider.Model ?? "", out var known) == true ? known : null;

        result.Entity = new DocumentProcessEstimateDto
        {
            Kind = EnumDocumentProcessKind.Translate,
            FileName = document.FileName,
            NoteNumber = document.NoteNumber,
            Pages = document.PagesCount,
            Paged = document.Paged,
            Characters = characters,
            Batches = plan.CallsWithText,
            Calls = plan.CallsWithText,
            ContextTokens = plan.ContextTokens,
            MissingPages = document.MissingPages,
            Model = provider.Model,
            IsPaid = provider.IsPaid,
            Seconds = SummaryEstimate.Seconds(characters, pace),
            FromPage = plan.Continued != null ? DocumentTranslationRules.PageOf(plan.Parts, plan.From) : null
        };
        return result;
    }

    /// <summary>
    /// Translates the document (or goes on with a translation of it), part by part, and keeps it as it goes. Entity: how it ended
    /// (Completed, Canceled or Failed, with the note of the translation if something was kept); progress, as it goes.
    /// </summary>
    public static async Task<Result<DocumentProcessProgressDto>> TranslateAsync(IKntService service, Func<AiProviderRef, IChatClient> createChatClient,
        AiProviderRef provider, DocumentProcessRequestDto request, IProgress<DocumentProcessProgressDto> progress = null, ILogger logger = null,
        CancellationToken cancellationToken = default)
    {
        var clock = Stopwatch.StartNew();
        var current = new DocumentProcessProgressDto
        {
            Kind = EnumDocumentProcessKind.Translate,
            ResourceId = request.ResourceId,
            Model = provider.Model,
            Phase = EnumDocumentProcessPhase.Preparing,
            StartedDateTime = DateTime.Now
        };
        void Report(EnumDocumentProcessPhase phase, int done, int total, int? page = null)
        {
            current.Phase = phase;
            current.CallsDone = done;
            current.CallsTotal = total;
            current.Page = page;
            current.ElapsedSeconds = clock.Elapsed.TotalSeconds;
            current.RemainingSeconds = done > 0 && total > done ? current.ElapsedSeconds / done * (total - done) : null;
            progress?.Report(current.Copy());
        }

        TranslationPlan plan = null;
        var pending = new StringBuilder();
        var pendingFrom = 0;
        var next = 0;
        var translated = 0;
        var lastSave = TimeSpan.Zero;

        // What was translated since the last time, added to the translation (the first time, its note is created).
        async Task SaveAsync()
        {
            if (plan == null || next == pendingFrom)
                return;
            var saved = await service.Summaries.SaveDocumentTranslationAsync(new DocumentTranslationSaveDto
            {
                ResourceId = plan.Document.ResourceId,
                ExpectedResourceName = plan.Document.ResourceName,
                NoteId = current.NoteId,
                Model = provider.Model,
                Language = plan.Language,
                Paged = plan.Document.Paged,
                PagesCount = plan.Document.PagesCount,
                MissingPages = plan.Document.MissingPages,
                Parts = plan.Parts.Count,
                FromPart = pendingFrom,
                NextPart = next,
                NextPage = DocumentTranslationRules.PageOf(plan.Parts, next),
                Text = pending.ToString(),
                CharactersTranslated = translated,
                Seconds = clock.Elapsed.TotalSeconds
            });
            if (!saved.IsValid)
                throw new InvalidOperationException(saved.ErrorMessage);
            current.NoteId = saved.Entity.NoteId;
            current.NoteNumber = saved.Entity.NoteNumber;
            pending.Clear();
            pendingFrom = next;
            lastSave = clock.Elapsed;
        }

        // Canceled or failed, what was translated is kept: it can go on.
        async Task<string> KeepWhatWasDoneAsync()
        {
            try
            {
                await SaveAsync();
                return null;
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "KNote Atlas: what was translated of the resource {resourceId} could not be kept.", request.ResourceId);
                return ex.Message;
            }
        }

        try
        {
            Report(EnumDocumentProcessPhase.Preparing, 0, 0);
            var (planned, error) = await PlanTranslationAsync(service, provider, request, logger, cancellationToken);
            if (error != null)
                return Failed(current, error.ErrorMessage, progress);
            plan = planned;
            pendingFrom = next = plan.From;
            current.FileName = plan.Document.FileName;
            current.PagesCount = plan.Document.Paged ? plan.Document.PagesCount : 0;
            current.NoteId = plan.Continued?.NoteId;
            current.NoteNumber = plan.Continued?.NoteNumber;

            var chatClient = createChatClient(AiSummaryContext.With(provider, plan.ContextTokens));
            var callsTotal = plan.CallsWithText;
            var callsDone = 0;
            foreach (var call in plan.Calls)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string translation = null;
                if (DocumentTranslationRules.HasText(plan.Parts, call))
                {
                    Report(EnumDocumentProcessPhase.Translating, callsDone, callsTotal, DocumentTranslationRules.PageOf(plan.Parts, call.From));
                    var source = DocumentTranslationRules.SourceText(plan.Parts, call);
                    var system = await TranslationPromptAsync(service, plan.Document, source, logger);
                    var ask = DocumentTranslationRules.Request(plan.Document.FileName, plan.Parts[call.From].Page, source, plan.Language);
                    translation = await TranslatePartAsync(chatClient, provider, system, ask, source, cancellationToken);
                    translated += source.Length;
                    callsDone++;
                }
                pending.Append(DocumentTranslationRules.PartText(plan.Parts, call, translation));
                next = call.To;

                if (pending.Length >= DocumentTranslationRules.SaveEveryCharacters || clock.Elapsed - lastSave >= DocumentTranslationRules.SaveEvery)
                    await SaveAsync();
            }

            Report(EnumDocumentProcessPhase.Saving, callsDone, callsTotal);
            await SaveAsync();
            Report(EnumDocumentProcessPhase.Completed, callsDone, callsTotal);
            return new Result<DocumentProcessProgressDto>(current.Copy());
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            var notKept = await KeepWhatWasDoneAsync();
            if (notKept != null)
                return Failed(current, notKept, progress);
            current.Phase = EnumDocumentProcessPhase.Canceled;
            current.RemainingSeconds = null;
            current.ElapsedSeconds = clock.Elapsed.TotalSeconds;
            progress?.Report(current.Copy());
            return new Result<DocumentProcessProgressDto>(current.Copy());
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger?.LogWarning(ex, "KNote Atlas: the translation of the resource {resourceId} failed.", request.ResourceId);
            await KeepWhatWasDoneAsync();
            return Failed(current, ex.Message, progress);
        }
    }

    // A part translated: asked again once if it comes back clearly shorter than its original; if still short, it is said in the translation.
    private static async Task<string> TranslatePartAsync(IChatClient chatClient, AiProviderRef provider, string systemPrompt, string request,
        string source, CancellationToken cancellationToken)
    {
        var maxOutputTokens = DocumentTranslationRules.OutputTokens(source.Length);
        async Task<string> AskAsync()
        {
            List<ChatMessage> messages = [new(ChatRole.System, systemPrompt), new(ChatRole.User, request)];
            return DocumentTranslationRules.CleanAnswer(await AiResourceTask.AnswerAsync(chatClient, provider, messages, cancellationToken, maxOutputTokens));
        }

        var answer = await AskAsync();
        if (answer.Length > 0 && !DocumentTranslationRules.LooksIncomplete(source, answer))
            return answer;

        var again = await AskAsync();
        if (again.Length > answer.Length)
            answer = again;
        return answer.Length > 0 && !DocumentTranslationRules.LooksIncomplete(source, answer)
            ? answer
            : $"{answer}\n\n{DocumentTranslationRules.IncompleteLine}".Trim();
    }

    // What the AI receives to translate, with the glossary terms of the text (D21).
    private static Task<string> TranslationPromptAsync(IKntService service, DocumentToSummarizeDto document, string text, ILogger logger) =>
        AiResourceTask.SystemPromptAsync(service, AiTaskCatalog.TranslateDocument, [document.NoteTopic, document.FileName, text], logger);

    #endregion

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

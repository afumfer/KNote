using System.Collections.Concurrent;
using System.Diagnostics;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace KNote.Ai;

/// <summary>
/// «Process pending resources» (KNote Atlas, stage 4, D10), the same in the Server (KntAtlasJobs) and in ClientWin
/// (Store.ResourceProcessing), with the roles of the user who runs it (preparing the run is an Admin command): the photos of the
/// repository one after another with AiPhotoDescriber; then (4.4c) the documents not read yet, read without AI; then (4.5) the
/// scanned pages to transcribe with AiPageTranscriber, also the ones of the documents just read; then the documents to summarize
/// with AiDocumentSummarizer, so that a scanned one is summarized once its pages are transcribed. Canceling stops at once; what
/// is done is kept (each page, when it is transcribed) and the rest stays pending for the next run. A file that is not in this
/// installation is counted, not an error, as a scanned document without text (its summary waits for its transcription) and one
/// that can't be read. A resource or a page that fails is tried again in the next run, up to MaxFailures times in this process.
/// </summary>
public static class AiResourceProcessor
{
    public const int MaxFailures = 3;

    // By resource and page (0: the resource itself).
    private static readonly ConcurrentDictionary<(Guid Resource, int Page), int> Failures = new();

    private sealed record Paces(double? Photo, double? Document, double? Page);

    public static async Task<Result<ResourceProcessingProgressDto>> ProcessAsync(IKntService service, IChatClient chatClient,
        AiProviderRef provider, ResourceProcessingOptionsDto options, IProgress<ResourceProcessingProgressDto> progress = null,
        ILogger logger = null, CancellationToken cancellationToken = default)
    {
        var result = new Result<ResourceProcessingProgressDto>();
        var run = new ResourceProcessingProgressDto
        {
            Phase = EnumResourceProcessingPhase.Preparing,
            Model = provider?.Model,
            StartedDateTime = DateTime.Now
        };
        progress?.Report(run.Copy());

        var status = await service.ResourceContent.GetProcessingStatusAsync();
        var known = status.IsValid ? status.Entity : null;
        var paces = new Paces(known?.SecondsPerPhoto, known?.SecondsPerDocument, known?.SecondsPerPage);
        // The pages of the documents not read yet are known once they are read.
        var pagesExpected = known != null && options != null ? ResourceProcessingTexts.ToProcess(known, options).Pages : 0;
        var plan = await service.ResourceContent.PrepareProcessingAsync(options);
        if (!plan.IsValid)
        {
            run.Phase = EnumResourceProcessingPhase.Failed;
            run.Message = plan.ErrorMessage;
            result.AddListErrorMessage(plan.ListErrorMessage);
            result.Entity = run;
            return result;
        }

        var photos = plan.Entity.PhotosToDescribe.Where(p => CanTry(p.ResourceId)).ToList();
        var toRead = plan.Entity.DocumentsToRead.Where(d => CanTry(d.ResourceId)).ToList();
        var documents = plan.Entity.DocumentsToSummarize.Where(d => CanTry(d.ResourceId)).ToList();
        run.Phase = EnumResourceProcessingPhase.Processing;
        run.PhotosTotal = photos.Count;
        run.PhotosLocked = plan.Entity.Locked;
        run.DocumentsToReadTotal = toRead.Count;
        run.DocumentsTotal = documents.Count;
        run.RemainingSeconds = ResourceProcessingRules.EstimateSeconds(photos.Count, paces.Photo, documents.Count, paces.Document,
            pagesExpected, paces.Page);
        progress?.Report(run.Copy());

        var clock = Stopwatch.StartNew();
        var photoClock = Stopwatch.StartNew();
        run.Step = EnumResourceProcessingStep.Photos;
        foreach (var photo in photos)
        {
            if (cancellationToken.IsCancellationRequested)
                break;
            await TryAsync(run, photo.GetTitle(), (photo.ResourceId, 0), logger, cancellationToken, async () =>
            {
                var described = await AiPhotoDescriber.DescribeAsync(service, chatClient, provider, photo.ResourceId,
                    plan.Entity.ReplacePersonDescriptions, logger, cancellationToken);
                if (described.IsValid)
                    run.PhotosDescribed++;
                else if (described.ListErrorMessage.Contains(AiPhotoRules.FileNotAvailableMessage))
                    run.NotAvailable++;
                else
                    return described.ErrorMessage;
                return null;
            });
            run.PhotosDone++;
            Report(run, progress, clock, paces, pagesExpected, photoClock.Elapsed.TotalSeconds);
        }
        photoClock.Stop();

        // Read first, without AI (also the locked ones: their text is for the search); what can't be read is not summarized.
        var skip = new HashSet<Guid>();
        run.Step = EnumResourceProcessingStep.ReadingDocuments;
        foreach (var document in toRead)
        {
            if (cancellationToken.IsCancellationRequested)
                break;
            await TryAsync(run, ResourceInfoDto.FileNameOf(document.Name), (document.ResourceId, 0), logger, cancellationToken, async () =>
            {
                var read = await service.ResourceContent.ReadDocumentAsync(document.ResourceId);
                if (read.IsValid && read.Entity.State == EnumExtractedState.Unreadable)
                {
                    run.DocumentsUnreadable++;
                    skip.Add(document.ResourceId);
                }
                else if (!read.IsValid && read.ListErrorMessage.Contains(AiDocumentRules.FileNotAvailableMessage))
                {
                    run.NotAvailable++;
                    skip.Add(document.ResourceId);
                }
                else if (!read.IsValid)
                    return read.ErrorMessage;
                return null;
            });
            run.DocumentsRead++;
            Report(run, progress, clock, paces, pagesExpected, 0);
        }

        // The scanned pages, also the ones of the documents just read: each page is kept when it is transcribed.
        var pageClock = new Stopwatch();
        if (options.ScannedPages && !cancellationToken.IsCancellationRequested)
        {
            var scanned = await service.ResourceContent.GetScannedDocumentsAsync();
            if (!scanned.IsValid)
                Fail(run, (Guid.Empty, 0), "Scanned pages", scanned.ErrorMessage);
            var pages = (scanned.Entity ?? new())
                .Where(d => !skip.Contains(d.ResourceId))
                .SelectMany(d => d.PagesToTranscribe(options.OcrPages).Where(p => CanTry(d.ResourceId, p)).Select(p => (Document: d, Page: p)))
                .ToList();
            run.Step = EnumResourceProcessingStep.Pages;
            run.PagesTotal = pages.Count;
            pageClock.Start();
            Report(run, progress, clock, paces, pagesExpected, 0);
            foreach (var (document, page) in pages)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                if (!skip.Contains(document.ResourceId))
                    await TryAsync(run, $"{ResourceInfoDto.FileNameOf(document.Name)}, page {page}", (document.ResourceId, page), logger,
                        cancellationToken, async () =>
                        {
                            var transcribed = await AiPageTranscriber.TranscribeAsync(service, chatClient, provider, document.ResourceId, page,
                                logger, cancellationToken);
                            var errors = transcribed.ListErrorMessage;
                            if (transcribed.IsValid)
                                run.PagesTranscribed++;
                            else if (errors.Contains(AiDocumentRules.FileNotAvailableMessage))
                            {
                                // Nor its other pages: counted once for the document.
                                run.NotAvailable++;
                                skip.Add(document.ResourceId);
                            }
                            else if (!errors.Contains(AiTranscriptionRules.NotPendingMessage))
                                return transcribed.ErrorMessage;
                            return null;
                        });
                run.PagesDone++;
                Report(run, progress, clock, paces, pagesExpected, pageClock.Elapsed.TotalSeconds);
            }
            pageClock.Stop();
        }

        var documentClock = Stopwatch.StartNew();
        run.Step = EnumResourceProcessingStep.Documents;
        foreach (var document in documents)
        {
            if (cancellationToken.IsCancellationRequested)
                break;
            if (!skip.Contains(document.ResourceId))
                await TryAsync(run, document.GetTitle(), (document.ResourceId, 0), logger, cancellationToken, async () =>
                {
                    var summarized = await AiDocumentSummarizer.SummarizeAsync(service, chatClient, provider, document.ResourceId,
                        plan.Entity.ReplacePersonDescriptions, logger, cancellationToken);
                    var errors = summarized.ListErrorMessage;
                    if (summarized.IsValid)
                        run.DocumentsSummarized++;
                    else if (errors.Contains(AiDocumentRules.FileNotAvailableMessage))
                        run.NotAvailable++;
                    else if (errors.Contains(AiDocumentRules.NeedsTranscriptionMessage))
                        run.DocumentsWaiting++;
                    else if (errors.Contains(AiDocumentRules.UnreadableMessage))
                        run.DocumentsUnreadable++;
                    else
                        return summarized.ErrorMessage;
                    return null;
                });
            run.DocumentsDone++;
            Report(run, progress, clock, paces, pagesExpected, documentClock.Elapsed.TotalSeconds);
        }
        documentClock.Stop();

        run.ElapsedSeconds = clock.Elapsed.TotalSeconds;
        run.RemainingSeconds = null;
        run.Phase = cancellationToken.IsCancellationRequested ? EnumResourceProcessingPhase.Canceled : EnumResourceProcessingPhase.Completed;

        var saved = await service.ResourceContent.SaveProcessingRunAsync(new ResourceProcessingRunDto
        {
            PhotosDescribed = run.PhotosDescribed,
            PhotoSeconds = photoClock.Elapsed.TotalSeconds,
            DocumentsSummarized = run.DocumentsSummarized,
            DocumentSeconds = documentClock.Elapsed.TotalSeconds,
            PagesTranscribed = run.PagesTranscribed,
            PageSeconds = pageClock.Elapsed.TotalSeconds
        });
        if (!saved.IsValid)
            logger?.LogWarning("«Process pending resources»: the pace of the run could not be kept: {error}", saved.ErrorMessage);

        progress?.Report(run.Copy());
        result.Entity = run;
        return result;
    }

    private static bool CanTry(Guid resourceId, int page = 0) => Failures.GetValueOrDefault((resourceId, page)) < MaxFailures;

    // One resource or page: its error (the text returned, or an exception) is counted and tried again in the next run.
    private static async Task TryAsync(ResourceProcessingProgressDto run, string title, (Guid Resource, int Page) key, ILogger logger,
        CancellationToken cancellationToken, Func<Task<string>> process)
    {
        try
        {
            var error = await process();
            if (error == null)
                Failures.TryRemove(key, out _);
            else
                Fail(run, key, title, error);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "«Process pending resources»: the resource {resourceId} (page {page}) could not be processed.", key.Resource, key.Page);
            Fail(run, key, title, ex.Message);
        }
    }

    // What is left: the step being done with its own pace, and the next ones with the last run's (the pages, before they are
    // known, with the ones counted when the run started).
    private static void Report(ResourceProcessingProgressDto run, IProgress<ResourceProcessingProgressDto> progress, Stopwatch clock,
        Paces paces, int pagesExpected, double stepSeconds)
    {
        run.ElapsedSeconds = clock.Elapsed.TotalSeconds;
        var documents = ResourceProcessingRules.EstimateSeconds(run.DocumentsTotal - run.DocumentsDone, paces.Document);
        var pagesAhead = ResourceProcessingRules.EstimateSeconds(pagesExpected, paces.Page);
        run.RemainingSeconds = run.Step switch
        {
            EnumResourceProcessingStep.Photos => Sum(
                ResourceProcessingRules.RemainingSeconds(run.PhotosTotal - run.PhotosDone, run.PhotosDescribed, stepSeconds, paces.Photo),
                pagesAhead, documents),
            EnumResourceProcessingStep.ReadingDocuments => Sum(pagesAhead, documents),
            EnumResourceProcessingStep.Pages => Sum(
                ResourceProcessingRules.RemainingSeconds(run.PagesTotal - run.PagesDone, run.PagesTranscribed, stepSeconds, paces.Page), documents),
            _ => ResourceProcessingRules.RemainingSeconds(run.DocumentsTotal - run.DocumentsDone, run.DocumentsSummarized, stepSeconds, paces.Document)
        };
        progress?.Report(run.Copy());
    }

    private static double? Sum(params double?[] seconds) =>
        seconds.All(s => s.HasValue) ? seconds.Sum(s => s!.Value) : null;

    private static void Fail(ResourceProcessingProgressDto run, (Guid Resource, int Page) key, string title, string error)
    {
        Failures.AddOrUpdate(key, 1, (_, failures) => failures + 1);
        run.ErrorsCount++;
        if (run.Errors.Count < ResourceProcessingProgressDto.MaxErrors)
            run.Errors.Add($"{title}: {error}");
    }
}

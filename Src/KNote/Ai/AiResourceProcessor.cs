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
/// repository one after another with AiPhotoDescriber; then (4.4c) the documents not read yet, read without AI; then the
/// documents to summarize with AiDocumentSummarizer. Canceling stops at once; what is done is kept and the rest stays pending for
/// the next run. A file that is not in this installation is counted, not an error, as a scanned document without text (its
/// summary waits for its transcription) and one that can't be read. A resource that fails is tried again in the next run, up to
/// MaxFailures times in this process.
/// </summary>
public static class AiResourceProcessor
{
    public const int MaxFailures = 3;

    private static readonly ConcurrentDictionary<Guid, int> Failures = new();

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
        var photoPace = status.IsValid ? status.Entity?.SecondsPerPhoto : null;
        var documentPace = status.IsValid ? status.Entity?.SecondsPerDocument : null;
        var plan = await service.ResourceContent.PrepareProcessingAsync(options);
        if (!plan.IsValid)
        {
            run.Phase = EnumResourceProcessingPhase.Failed;
            run.Message = plan.ErrorMessage;
            result.AddListErrorMessage(plan.ListErrorMessage);
            result.Entity = run;
            return result;
        }

        var photos = plan.Entity.PhotosToDescribe.Where(p => Failures.GetValueOrDefault(p.ResourceId) < MaxFailures).ToList();
        var toRead = plan.Entity.DocumentsToRead.Where(d => Failures.GetValueOrDefault(d.ResourceId) < MaxFailures).ToList();
        var documents = plan.Entity.DocumentsToSummarize.Where(d => Failures.GetValueOrDefault(d.ResourceId) < MaxFailures).ToList();
        run.Phase = EnumResourceProcessingPhase.Processing;
        run.PhotosTotal = photos.Count;
        run.PhotosLocked = plan.Entity.Locked;
        run.DocumentsToReadTotal = toRead.Count;
        run.DocumentsTotal = documents.Count;
        run.RemainingSeconds = ResourceProcessingRules.EstimateSeconds(photos.Count, photoPace, documents.Count, documentPace);
        progress?.Report(run.Copy());

        var clock = Stopwatch.StartNew();
        var photoClock = Stopwatch.StartNew();
        run.Step = EnumResourceProcessingStep.Photos;
        foreach (var photo in photos)
        {
            if (cancellationToken.IsCancellationRequested)
                break;
            await TryAsync(run, photo.GetTitle(), photo.ResourceId, logger, cancellationToken, async () =>
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
            Report(run, progress, clock, photoPace, documentPace, photoClock.Elapsed.TotalSeconds);
        }
        photoClock.Stop();

        // Read first, without AI (also the locked ones: their text is for the search); what can't be read is not summarized.
        var skip = new HashSet<Guid>();
        run.Step = EnumResourceProcessingStep.ReadingDocuments;
        foreach (var document in toRead)
        {
            if (cancellationToken.IsCancellationRequested)
                break;
            await TryAsync(run, ResourceInfoDto.FileNameOf(document.Name), document.ResourceId, logger, cancellationToken, async () =>
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
            Report(run, progress, clock, photoPace, documentPace, 0);
        }

        var documentClock = Stopwatch.StartNew();
        run.Step = EnumResourceProcessingStep.Documents;
        foreach (var document in documents)
        {
            if (cancellationToken.IsCancellationRequested)
                break;
            if (!skip.Contains(document.ResourceId))
                await TryAsync(run, document.GetTitle(), document.ResourceId, logger, cancellationToken, async () =>
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
            Report(run, progress, clock, photoPace, documentPace, documentClock.Elapsed.TotalSeconds);
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
            DocumentSeconds = documentClock.Elapsed.TotalSeconds
        });
        if (!saved.IsValid)
            logger?.LogWarning("«Process pending resources»: the pace of the run could not be kept: {error}", saved.ErrorMessage);

        progress?.Report(run.Copy());
        result.Entity = run;
        return result;
    }

    // One resource: its error (the text returned, or an exception) is counted and tried again in the next run.
    private static async Task TryAsync(ResourceProcessingProgressDto run, string title, Guid resourceId, ILogger logger,
        CancellationToken cancellationToken, Func<Task<string>> process)
    {
        try
        {
            var error = await process();
            if (error == null)
                Failures.TryRemove(resourceId, out _);
            else
                Fail(run, resourceId, title, error);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "«Process pending resources»: the resource {resourceId} could not be processed.", resourceId);
            Fail(run, resourceId, title, ex.Message);
        }
    }

    // What is left: the step being done with its own pace, and the next ones with the last run's.
    private static void Report(ResourceProcessingProgressDto run, IProgress<ResourceProcessingProgressDto> progress, Stopwatch clock,
        double? photoPace, double? documentPace, double stepSeconds)
    {
        run.ElapsedSeconds = clock.Elapsed.TotalSeconds;
        var documentsLeft = run.DocumentsTotal - run.DocumentsDone;
        run.RemainingSeconds = run.Step switch
        {
            EnumResourceProcessingStep.Photos =>
                ResourceProcessingRules.RemainingSeconds(run.PhotosTotal - run.PhotosDone, run.PhotosDescribed, stepSeconds, photoPace)
                + ResourceProcessingRules.EstimateSeconds(documentsLeft, documentPace),
            EnumResourceProcessingStep.ReadingDocuments => ResourceProcessingRules.EstimateSeconds(documentsLeft, documentPace),
            _ => ResourceProcessingRules.RemainingSeconds(documentsLeft, run.DocumentsSummarized, stepSeconds, documentPace)
        };
        progress?.Report(run.Copy());
    }

    private static void Fail(ResourceProcessingProgressDto run, Guid resourceId, string title, string error)
    {
        Failures.AddOrUpdate(resourceId, 1, (_, failures) => failures + 1);
        run.ErrorsCount++;
        if (run.Errors.Count < ResourceProcessingProgressDto.MaxErrors)
            run.Errors.Add($"{title}: {error}");
    }
}

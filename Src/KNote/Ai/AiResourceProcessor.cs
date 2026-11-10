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
/// (Store.ResourceProcessing): the photos of the repository one after another with AiPhotoDescriber, with the roles of the
/// user who runs it (preparing the run is an Admin command). Canceling stops at once; what is done is kept and the rest stays
/// pending for the next run. A photo whose file is not in this installation is counted, not an error. A photo that fails is
/// tried again in the next run, up to MaxFailures times in this process.
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
        var lastPace = status.IsValid ? status.Entity?.SecondsPerPhoto : null;
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
        run.Phase = EnumResourceProcessingPhase.Processing;
        run.PhotosTotal = photos.Count;
        run.PhotosLocked = plan.Entity.Locked;
        run.RemainingSeconds = ResourceProcessingRules.EstimateSeconds(photos.Count, lastPace);
        progress?.Report(run.Copy());

        var clock = Stopwatch.StartNew();
        foreach (var photo in photos)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                var described = await AiPhotoDescriber.DescribeAsync(service, chatClient, provider, photo.ResourceId,
                    plan.Entity.ReplacePersonDescriptions, logger, cancellationToken);
                if (described.IsValid)
                {
                    run.PhotosDescribed++;
                    Failures.TryRemove(photo.ResourceId, out _);
                }
                else if (described.ListErrorMessage.Contains(AiPhotoRules.FileNotAvailableMessage))
                    run.NotAvailable++;
                else
                    Fail(run, photo, described.ErrorMessage);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "«Process pending resources»: the photo {resourceId} could not be described.", photo.ResourceId);
                Fail(run, photo, ex.Message);
            }

            run.PhotosDone++;
            run.ElapsedSeconds = clock.Elapsed.TotalSeconds;
            run.RemainingSeconds = ResourceProcessingRules.RemainingSeconds(run.PhotosTotal - run.PhotosDone, run.PhotosDescribed,
                run.ElapsedSeconds, lastPace);
            progress?.Report(run.Copy());
        }

        run.ElapsedSeconds = clock.Elapsed.TotalSeconds;
        run.RemainingSeconds = null;
        run.Phase = cancellationToken.IsCancellationRequested ? EnumResourceProcessingPhase.Canceled : EnumResourceProcessingPhase.Completed;

        var saved = await service.ResourceContent.SaveProcessingRunAsync(new ResourceProcessingRunDto
        {
            PhotosDescribed = run.PhotosDescribed,
            Seconds = run.ElapsedSeconds
        });
        if (!saved.IsValid)
            logger?.LogWarning("«Process pending resources»: the pace of the run could not be kept: {error}", saved.ErrorMessage);

        progress?.Report(run.Copy());
        result.Entity = run;
        return result;
    }

    private static void Fail(ResourceProcessingProgressDto run, ResourceInfoDto photo, string error)
    {
        Failures.AddOrUpdate(photo.ResourceId, 1, (_, failures) => failures + 1);
        run.ErrorsCount++;
        if (run.Errors.Count < ResourceProcessingProgressDto.MaxErrors)
            run.Errors.Add($"{photo.GetTitle()}: {error}");
    }
}

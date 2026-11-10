using System;
using System.Collections.Generic;

namespace KNote.Model.Dto;

/// <summary>
/// The resources of a repository that «Process pending resources» would process (KNote Atlas, stage 4, D10). For now the
/// photos: the pending ones (without a description of their own) and the ones with a description a person wrote and did not
/// lock (old descriptions: they are not told apart from one written by hand, so it is chosen what to do with them). The
/// estimate uses the pace of the last run; without one, only the counts.
/// </summary>
public class ResourceProcessingStatusDto
{
    public int PendingPhotos { get; set; }
    public int PersonPhotos { get; set; }
    public DateTime? LastRunDateTime { get; set; }
    public double? SecondsPerPhoto { get; set; }
}

/// <summary>What «Process pending resources» is asked to do (POST api/resources/process).</summary>
public class ResourceProcessingOptionsDto
{
    public bool Photos { get; set; } = true;
    public EnumPersonDescriptions PersonDescriptions { get; set; } = EnumPersonDescriptions.Skip;

    public override string ToString() => $"Photos: {Photos}, descriptions written by people: {PersonDescriptions}";
}

/// <summary>
/// What a run will do, decided when it starts (IKntResourceContentService.PrepareProcessingAsync): the photos to describe
/// (with ReplacePersonDescriptions, also the ones a person described) and how many were locked to keep them.
/// </summary>
public class ResourceProcessingPlanDto
{
    public List<ResourceInfoDto> PhotosToDescribe { get; set; } = new();
    public bool ReplacePersonDescriptions { get; set; }
    public int Locked { get; set; }
}

/// <summary>How a run ended, to keep its pace for the estimates (IKntResourceContentService.SaveProcessingRunAsync).</summary>
public class ResourceProcessingRunDto
{
    public int PhotosDescribed { get; set; }
    public double Seconds { get; set; }

    public override string ToString() => $"{PhotosDescribed} photos in {Seconds:0} s";
}

/// <summary>What KNote keeps of the runs of a repository (SystemValues AI / AI_RESOURCES_STATE, as JSON).</summary>
public class ResourceProcessingStateDto
{
    public DateTime? LastRunDateTime { get; set; }
    public double? SecondsPerPhoto { get; set; }
}

public enum EnumResourceProcessingPhase
{
    Queued,
    Preparing,
    Processing,
    Completed,
    Canceled,
    Failed
}

/// <summary>
/// The progress of a run of «Process pending resources», in the Server (KntAtlasJobs, GET api/resources/process/progress)
/// and in ClientWin (Store.ResourceProcessing).
/// </summary>
public class ResourceProcessingProgressDto
{
    public const int MaxErrors = 20;

    public Guid RepositoryId { get; set; }
    public string RepositoryName { get; set; }
    public EnumResourceProcessingPhase Phase { get; set; }
    public int QueuePosition { get; set; }
    public string RequestedBy { get; set; }
    public string Model { get; set; }
    public DateTime? StartedDateTime { get; set; }
    public double ElapsedSeconds { get; set; }
    public double? RemainingSeconds { get; set; }
    public int PhotosTotal { get; set; }
    public int PhotosDone { get; set; }
    public int PhotosDescribed { get; set; }
    public int PhotosLocked { get; set; }
    // Photos whose file is not in this installation: not an error, they are described where the file is.
    public int NotAvailable { get; set; }
    public int ErrorsCount { get; set; }
    // The first MaxErrors errors.
    public List<string> Errors { get; set; } = new();
    // Why the run failed (Phase Failed).
    public string Message { get; set; }

    public bool IsFinished => Phase is EnumResourceProcessingPhase.Completed or EnumResourceProcessingPhase.Canceled or EnumResourceProcessingPhase.Failed;

    public ResourceProcessingProgressDto Copy()
    {
        var copy = (ResourceProcessingProgressDto)MemberwiseClone();
        copy.Errors = new List<string>(Errors);
        return copy;
    }
}

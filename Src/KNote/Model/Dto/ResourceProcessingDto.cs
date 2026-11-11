using System;
using System.Collections.Generic;

namespace KNote.Model.Dto;

/// <summary>
/// The resources of a repository that «Process pending resources» would process (KNote Atlas, stage 4, D10): the photos and the
/// documents pending (without a description of their own) and the ones with a description a person wrote and did not lock (old
/// descriptions: they are not told apart from one written by hand, so it is chosen what to do with them), and the documents not
/// read yet (all of them, also the locked ones: their text is for the search). The estimate uses the pace of the last run;
/// without one, only the counts.
/// </summary>
public class ResourceProcessingStatusDto
{
    public int PendingPhotos { get; set; }
    public int PersonPhotos { get; set; }
    public int PendingDocuments { get; set; }
    public int PersonDocuments { get; set; }
    public int DocumentsToRead { get; set; }
    public DateTime? LastRunDateTime { get; set; }
    public double? SecondsPerPhoto { get; set; }
    public double? SecondsPerDocument { get; set; }
}

/// <summary>What «Process pending resources» is asked to do (POST api/resources/process).</summary>
public class ResourceProcessingOptionsDto
{
    public bool Photos { get; set; } = true;
    // Read the documents not read yet and summarize the pending ones (4.4c).
    public bool Documents { get; set; } = true;
    public EnumPersonDescriptions PersonDescriptions { get; set; } = EnumPersonDescriptions.Skip;

    public bool Any => Photos || Documents;

    public override string ToString() => $"Photos: {Photos}, documents: {Documents}, descriptions written by people: {PersonDescriptions}";
}

/// <summary>
/// What a run will do, decided when it starts (IKntResourceContentService.PrepareProcessingAsync): the photos to describe
/// (with ReplacePersonDescriptions, also the ones a person described) and how many were locked to keep them.
/// </summary>
public class ResourceProcessingPlanDto
{
    public List<ResourceInfoDto> PhotosToDescribe { get; set; } = new();
    public List<ResourceInfoDto> DocumentsToSummarize { get; set; } = new();
    // The documents not read yet (their text is read without AI, before the summaries).
    public List<ResourceTextDto> DocumentsToRead { get; set; } = new();
    public bool ReplacePersonDescriptions { get; set; }
    public int Locked { get; set; }
}

/// <summary>How a run ended, to keep its pace for the estimates (IKntResourceContentService.SaveProcessingRunAsync).</summary>
public class ResourceProcessingRunDto
{
    public int PhotosDescribed { get; set; }
    public double PhotoSeconds { get; set; }
    public int DocumentsSummarized { get; set; }
    public double DocumentSeconds { get; set; }

    public override string ToString() =>
        $"{PhotosDescribed} photos in {PhotoSeconds:0} s, {DocumentsSummarized} documents in {DocumentSeconds:0} s";
}

/// <summary>What KNote keeps of the runs of a repository (SystemValues AI / AI_RESOURCES_STATE, as JSON).</summary>
public class ResourceProcessingStateDto
{
    public DateTime? LastRunDateTime { get; set; }
    public double? SecondsPerPhoto { get; set; }
    public double? SecondsPerDocument { get; set; }
}

/// <summary>What a run is doing (Phase Processing).</summary>
public enum EnumResourceProcessingStep
{
    Photos,
    ReadingDocuments,
    Documents
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
    public EnumResourceProcessingStep Step { get; set; }
    public int PhotosTotal { get; set; }
    public int PhotosDone { get; set; }
    public int PhotosDescribed { get; set; }
    public int PhotosLocked { get; set; }
    // Documents (4.4c): the ones to read (without AI) and the ones to summarize.
    public int DocumentsToReadTotal { get; set; }
    public int DocumentsRead { get; set; }
    public int DocumentsTotal { get; set; }
    public int DocumentsDone { get; set; }
    public int DocumentsSummarized { get; set; }
    // Scanned documents without text: their summary waits for the transcription of their pages (stage 4.5).
    public int DocumentsWaiting { get; set; }
    // Documents that can't be read (protected, encrypted, damaged): not tried again.
    public int DocumentsUnreadable { get; set; }
    // Photos and documents whose file is not in this installation: not an error, they are processed where the file is.
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

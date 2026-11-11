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
    // The scanned pages pending of a transcription (4.5), and how many of them have an OCR layer (counted in PendingPages).
    public int PendingPages { get; set; }
    public int OcrPages { get; set; }
    public int ScannedDocuments { get; set; }
    public DateTime? LastRunDateTime { get; set; }
    public double? SecondsPerPhoto { get; set; }
    public double? SecondsPerDocument { get; set; }
    public double? SecondsPerPage { get; set; }
}

/// <summary>What «Process pending resources» is asked to do (POST api/resources/process).</summary>
public class ResourceProcessingOptionsDto
{
    public bool Photos { get; set; } = true;
    // Read the documents not read yet and summarize the pending ones (4.4c).
    public bool Documents { get; set; } = true;
    // Transcribe the scanned pages (4.5) and, with OcrPages, also the ones with an OCR layer.
    public bool ScannedPages { get; set; } = true;
    public bool OcrPages { get; set; } = true;
    public EnumPersonDescriptions PersonDescriptions { get; set; } = EnumPersonDescriptions.Skip;

    public bool Any => Photos || Documents || ScannedPages;

    // The photos and the scanned pages need a model that sees images.
    public bool NeedsImages => Photos || ScannedPages;

    public override string ToString() =>
        $"Photos: {Photos}, documents: {Documents}, scanned pages: {ScannedPages} (with an OCR layer: {OcrPages}), descriptions written by people: {PersonDescriptions}";
}

/// <summary>A scanned document with pages pending of a transcription (4.5): their numbers, and the ones with an OCR layer.</summary>
public class ScannedDocumentDto
{
    public Guid ResourceId { get; set; }
    public Guid NoteId { get; set; }
    public string Name { get; set; }
    public string FileType { get; set; }
    public int PagesCount { get; set; }
    public List<int> PendingPages { get; set; } = new();
    public List<int> OcrPages { get; set; } = new();

    /// <summary>The pages to transcribe: all the pending ones, or only the ones without an OCR layer.</summary>
    public List<int> PagesToTranscribe(bool withOcrLayer) =>
        withOcrLayer ? new List<int>(PendingPages) : PendingPages.FindAll(p => !OcrPages.Contains(p));
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
    public int PagesTranscribed { get; set; }
    public double PageSeconds { get; set; }

    public override string ToString() =>
        $"{PhotosDescribed} photos in {PhotoSeconds:0} s, {DocumentsSummarized} documents in {DocumentSeconds:0} s, {PagesTranscribed} pages in {PageSeconds:0} s";
}

/// <summary>What KNote keeps of the runs of a repository (SystemValues AI / AI_RESOURCES_STATE, as JSON).</summary>
public class ResourceProcessingStateDto
{
    public DateTime? LastRunDateTime { get; set; }
    public double? SecondsPerPhoto { get; set; }
    public double? SecondsPerDocument { get; set; }
    public double? SecondsPerPage { get; set; }
}

/// <summary>What a run is doing (Phase Processing), in this order: the documents transcribed are summarized in the same run.</summary>
public enum EnumResourceProcessingStep
{
    Photos,
    ReadingDocuments,
    Pages,
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
    // Scanned pages (4.5): the ones to transcribe (known once the documents are read), done and transcribed.
    public int PagesTotal { get; set; }
    public int PagesDone { get; set; }
    public int PagesTranscribed { get; set; }
    // Scanned documents without text: their summary waits for the transcription of their pages.
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

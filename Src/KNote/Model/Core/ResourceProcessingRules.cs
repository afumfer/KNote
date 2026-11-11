using System;
using System.Collections.Generic;
using System.Linq;
using KNote.Model.Dto;

namespace KNote.Model;

/// <summary>The counts of «Process pending resources»: what it would describe or summarize, and what a person described.</summary>
public readonly record struct ResourceCounts(int PendingPhotos, int PersonPhotos, int PendingDocuments, int PersonDocuments);

/// <summary>
/// What «Process pending resources» does (KNote Atlas, stage 4, D10): the same as «Describe with AI» in a note
/// (AiPhotoRules.PlanForNote, AiDocumentRules.PlanForNote) over the photos and the documents of the repository, the documents
/// to read, and its estimate with the pace of the last run.
/// </summary>
public static class ResourceProcessingRules
{
    /// <summary>The counts of the status: pending photos and documents, and the ones with a description a person wrote and didn't lock.</summary>
    public static ResourceCounts Count(IEnumerable<ResourceInfoDto> resources)
    {
        int pendingPhotos = 0, personPhotos = 0, pendingDocuments = 0, personDocuments = 0;
        foreach (var resource in resources ?? Enumerable.Empty<ResourceInfoDto>())
        {
            var photo = AiPhotoRules.IsPhoto(resource);
            var action = AiDocumentRules.PlanResourceForNote(resource, EnumPersonDescriptions.Skip);
            if (action == EnumPhotoAction.Describe)
            {
                if (photo) pendingPhotos++;
                else pendingDocuments++;
            }
            else if (AiDocumentRules.PlanResourceForNote(resource, EnumPersonDescriptions.Lock) == EnumPhotoAction.Lock)
            {
                if (photo) personPhotos++;
                else personDocuments++;
            }
        }
        return new ResourceCounts(pendingPhotos, personPhotos, pendingDocuments, personDocuments);
    }

    /// <summary>The photos to describe, the documents to summarize and the ones to lock, as chosen.</summary>
    public static (List<ResourceInfoDto> Describe, List<ResourceInfoDto> Summarize, List<ResourceInfoDto> Lock) Plan(
        IEnumerable<ResourceInfoDto> resources, ResourceProcessingOptionsDto options)
    {
        var describe = new List<ResourceInfoDto>();
        var summarize = new List<ResourceInfoDto>();
        var lockThem = new List<ResourceInfoDto>();
        if (options == null)
            return (describe, summarize, lockThem);

        foreach (var resource in resources ?? Enumerable.Empty<ResourceInfoDto>())
        {
            var photo = AiPhotoRules.IsPhoto(resource);
            if (photo ? !options.Photos : !options.Documents)
                continue;
            var action = AiDocumentRules.PlanResourceForNote(resource, options.PersonDescriptions);
            if (action == EnumPhotoAction.Describe)
                (photo ? describe : summarize).Add(resource);
            else if (action == EnumPhotoAction.Lock)
                lockThem.Add(resource);
        }
        return (describe, summarize, lockThem);
    }

    /// <summary>The documents not read yet, of the states of the resources of the repository.</summary>
    public static List<ResourceTextDto> DocumentsToRead(IEnumerable<ResourceTextDto> states) =>
        (states ?? Enumerable.Empty<ResourceTextDto>())
            .Where(s => s.State == EnumExtractedState.NotRead && DocumentTextRules.IsDocument(s.FileType, s.Name))
            .ToList();

    /// <summary>The seconds that the items would take with the pace of the last run; null without one.</summary>
    public static double? EstimateSeconds(int items, double? secondsPerItem) =>
        items == 0 ? 0 : secondsPerItem is > 0 ? items * secondsPerItem.Value : null;

    /// <summary>The seconds of photos and documents; null if a pace that is needed is missing (only the counts, then).</summary>
    public static double? EstimateSeconds(int photos, double? secondsPerPhoto, int documents, double? secondsPerDocument) =>
        EstimateSeconds(photos, secondsPerPhoto) is double p && EstimateSeconds(documents, secondsPerDocument) is double d ? p + d : null;

    /// <summary>The pace of a run that did some items (null: it didn't do enough to say).</summary>
    public static double? Pace(int itemsDone, double seconds) =>
        itemsDone > 0 && seconds > 0 ? seconds / itemsDone : null;

    /// <summary>What is left of a run, with its own pace once it has done some items.</summary>
    public static double? RemainingSeconds(int itemsLeft, int itemsDone, double elapsedSeconds, double? lastPace) =>
        EstimateSeconds(itemsLeft, Pace(itemsDone, elapsedSeconds) ?? lastPace);
}

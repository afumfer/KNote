using System;
using System.Collections.Generic;
using System.Linq;
using KNote.Model.Dto;

namespace KNote.Model;

/// <summary>
/// What «Process pending resources» does (KNote Atlas, stage 4, D10): the same as «Describe photos» in a note
/// (AiPhotoRules.PlanForNote) over the photos of the repository, and its estimate with the pace of the last run.
/// </summary>
public static class ResourceProcessingRules
{
    /// <summary>The counts of the status: pending photos and photos with a description a person wrote and didn't lock.</summary>
    public static (int Pending, int Person) Count(IEnumerable<ResourceInfoDto> resources)
    {
        int pending = 0, person = 0;
        foreach (var resource in resources ?? Enumerable.Empty<ResourceInfoDto>())
        {
            if (AiPhotoRules.PlanForNote(resource, EnumPersonDescriptions.Skip) == EnumPhotoAction.Describe)
                pending++;
            else if (AiPhotoRules.PlanForNote(resource, EnumPersonDescriptions.Lock) == EnumPhotoAction.Lock)
                person++;
        }
        return (pending, person);
    }

    /// <summary>The photos to describe and the ones to lock, as chosen.</summary>
    public static (List<ResourceInfoDto> Describe, List<ResourceInfoDto> Lock) Plan(IEnumerable<ResourceInfoDto> resources,
        ResourceProcessingOptionsDto options)
    {
        var describe = new List<ResourceInfoDto>();
        var lockThem = new List<ResourceInfoDto>();
        if (options?.Photos != true)
            return (describe, lockThem);

        foreach (var resource in resources ?? Enumerable.Empty<ResourceInfoDto>())
        {
            var action = AiPhotoRules.PlanForNote(resource, options.PersonDescriptions);
            if (action == EnumPhotoAction.Describe)
                describe.Add(resource);
            else if (action == EnumPhotoAction.Lock)
                lockThem.Add(resource);
        }
        return (describe, lockThem);
    }

    /// <summary>The seconds that the photos would take with the pace of the last run; null without one.</summary>
    public static double? EstimateSeconds(int photos, double? secondsPerPhoto) =>
        secondsPerPhoto is > 0 ? photos * secondsPerPhoto.Value : null;

    /// <summary>The pace of a run that described some photos (null: it didn't describe enough to say).</summary>
    public static double? SecondsPerPhoto(int photosDescribed, double seconds) =>
        photosDescribed > 0 && seconds > 0 ? seconds / photosDescribed : null;

    /// <summary>What is left of a run, with its own pace once it has described some photos.</summary>
    public static double? RemainingSeconds(int photosLeft, int photosDescribed, double elapsedSeconds, double? lastSecondsPerPhoto) =>
        EstimateSeconds(photosLeft, SecondsPerPhoto(photosDescribed, elapsedSeconds) ?? lastSecondsPerPhoto);
}

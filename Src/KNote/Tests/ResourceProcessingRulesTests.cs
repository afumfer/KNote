using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// ResourceProcessingRules and ResourceProcessingTexts (KNote Atlas, stage 4, 4.3): what «Process pending resources» counts,
/// what it describes and locks as chosen (the same as «Describe photos» in a note), and its estimate.
/// </summary>
[TestClass]
public class ResourceProcessingRulesTests
{
    private static ResourceInfoDto Resource(string name, string? description = null, string? fileType = null, bool locked = false, string? model = null) =>
        new() { ResourceId = Guid.NewGuid(), Name = name, FileType = fileType, Description = description ?? name, DescriptionByUser = locked, DescriptionModel = model };

    private static readonly ResourceInfoDto Pending = Resource("photo.jpg");
    private static readonly ResourceInfoDto ByPerson = Resource("grandma.jpg", "La abuela Carmen en Cádiz.");
    private static readonly ResourceInfoDto ByTheAi = Resource("beach.jpg", "A beach.", model: "qwen3.5:9b");
    private static readonly ResourceInfoDto Locked = Resource("locked.jpg", "Mine.", locked: true);
    private static readonly ResourceInfoDto Document = Resource("manual.pdf", fileType: "application/pdf");

    private static readonly ResourceInfoDto[] All = [Pending, ByPerson, ByTheAi, Locked, Document];

    [TestMethod]
    public void Count_GivesThePendingPhotos_AndTheOnesAPersonDescribed()
    {
        var (pending, person) = ResourceProcessingRules.Count(All);

        Assert.AreEqual(1, pending);
        Assert.AreEqual(1, person);
    }

    [TestMethod]
    [DataRow(EnumPersonDescriptions.Skip, 1, 0)]
    [DataRow(EnumPersonDescriptions.Lock, 1, 1)]
    [DataRow(EnumPersonDescriptions.Replace, 2, 0)]
    public void Plan_DescribesAndLocks_AsChosen(EnumPersonDescriptions choice, int describe, int lockThem)
    {
        var (toDescribe, toLock) = ResourceProcessingRules.Plan(All, new ResourceProcessingOptionsDto { PersonDescriptions = choice });

        Assert.AreEqual(describe, toDescribe.Count);
        Assert.AreEqual(lockThem, toLock.Count);
        Assert.IsTrue(toDescribe.Contains(Pending));
        Assert.IsFalse(toDescribe.Contains(ByTheAi), "already described by the AI");
        Assert.IsFalse(toDescribe.Contains(Locked));
        Assert.IsFalse(toDescribe.Contains(Document));
    }

    [TestMethod]
    public void Plan_WithoutPhotos_DoesNothing()
    {
        var (toDescribe, toLock) = ResourceProcessingRules.Plan(All, new ResourceProcessingOptionsDto { Photos = false, PersonDescriptions = EnumPersonDescriptions.Lock });

        Assert.AreEqual(0, toDescribe.Count);
        Assert.AreEqual(0, toLock.Count);
    }

    [TestMethod]
    public void TheEstimate_NeedsTheTimeOfARun()
    {
        Assert.IsNull(ResourceProcessingRules.EstimateSeconds(10, null), "before the first run, only the counts");
        Assert.AreEqual(30.0, ResourceProcessingRules.EstimateSeconds(10, 3.0));
        Assert.IsNull(ResourceProcessingRules.SecondsPerPhoto(0, 12), "a run that described nothing says nothing of the time");
        Assert.AreEqual(2.5, ResourceProcessingRules.SecondsPerPhoto(4, 10));
        // Once it has described some, the run goes with its own time.
        Assert.AreEqual(20.0, ResourceProcessingRules.RemainingSeconds(10, 5, 10, 9.0));
        Assert.AreEqual(90.0, ResourceProcessingRules.RemainingSeconds(10, 0, 10, 9.0));
    }

    [TestMethod]
    public void RunConfirmation_SaysHowManyWithWhichModel_HowLong_AndIfItIsPaid()
    {
        var status = new ResourceProcessingStatusDto { PendingPhotos = 10, PersonPhotos = 2, SecondsPerPhoto = 3 };
        var paid = new ResourceDescriberDto { Model = "gpt-test", IsPaid = true, SupportsImages = true };

        var replace = ResourceProcessingTexts.RunConfirmation(status, EnumPersonDescriptions.Replace, paid);
        var keep = ResourceProcessingTexts.RunConfirmation(status, EnumPersonDescriptions.Lock, new ResourceDescriberDto { Model = "qwen3.5:9b" });

        StringAssert.Contains(replace, "The AI describes 12 photos with the model gpt-test.");
        StringAssert.Contains(replace, "It should take about 36 s.");
        StringAssert.Contains(replace, "gpt-test is a paid model");
        StringAssert.Contains(keep, "describes 10 photos");
        StringAssert.Contains(keep, "2 descriptions written by people are locked");
        Assert.IsFalse(keep.Contains("paid"));
    }

    [TestMethod]
    public void Progress_SaysWhatWasDone()
    {
        var done = new ResourceProcessingProgressDto
        {
            Phase = EnumResourceProcessingPhase.Completed, PhotosDescribed = 3, PhotosLocked = 2, NotAvailable = 1, ElapsedSeconds = 9
        };

        Assert.AreEqual("Done: 3 photos described in 9 s · 2 locked · 1 not available here.", ResourceProcessingTexts.Progress(done));
        Assert.IsNull(ResourceProcessingTexts.Errors(done));
    }
}

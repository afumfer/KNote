using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// ResourceProcessingRules and ResourceProcessingTexts (KNote Atlas, stage 4, 4.3 and 4.4c): what «Process pending resources»
/// counts, what it describes, summarizes and locks as chosen (the same as «Describe with AI» in a note), the documents it reads,
/// and its estimate.
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
    private static readonly ResourceInfoDto DocumentByPerson = Resource("letter.docx", "Carta de la abuela.");
    private static readonly ResourceInfoDto OtherFile = Resource("data.zip");

    private static readonly ResourceInfoDto[] All = [Pending, ByPerson, ByTheAi, Locked, Document, DocumentByPerson, OtherFile];

    [TestMethod]
    public void Count_GivesThePendingPhotosAndDocuments_AndTheOnesAPersonDescribed()
    {
        Assert.AreEqual(new ResourceCounts(1, 1, 1, 1), ResourceProcessingRules.Count(All));
    }

    [TestMethod]
    [DataRow(EnumPersonDescriptions.Skip, 1, 1, 0)]
    [DataRow(EnumPersonDescriptions.Lock, 1, 1, 2)]
    [DataRow(EnumPersonDescriptions.Replace, 2, 2, 0)]
    public void Plan_DescribesSummarizesAndLocks_AsChosen(EnumPersonDescriptions choice, int describe, int summarize, int lockThem)
    {
        var (toDescribe, toSummarize, toLock) = ResourceProcessingRules.Plan(All, new ResourceProcessingOptionsDto { PersonDescriptions = choice });

        Assert.AreEqual((describe, summarize, lockThem), (toDescribe.Count, toSummarize.Count, toLock.Count));
        Assert.IsTrue(toDescribe.Contains(Pending));
        Assert.IsTrue(toSummarize.Contains(Document));
        Assert.IsFalse(toDescribe.Contains(ByTheAi), "already described by the AI");
        Assert.IsFalse(toDescribe.Contains(Locked));
        Assert.IsFalse(toDescribe.Concat(toSummarize).Contains(OtherFile), "not a photo nor a document");
    }

    [TestMethod]
    public void Plan_OnlyOfTheKindsChosen()
    {
        var (photos, documents, locks) = ResourceProcessingRules.Plan(All, new ResourceProcessingOptionsDto { Photos = false, PersonDescriptions = EnumPersonDescriptions.Lock });
        var (onlyPhotos, noDocuments, _) = ResourceProcessingRules.Plan(All, new ResourceProcessingOptionsDto { Documents = false });

        Assert.AreEqual((0, 1), (photos.Count, documents.Count));
        CollectionAssert.AreEqual(new[] { DocumentByPerson }, locks);
        Assert.AreEqual((1, 0), (onlyPhotos.Count, noDocuments.Count));
    }

    [TestMethod]
    public void TheDocumentsToRead_AreTheOnesNotReadYet()
    {
        var states = new List<ResourceTextDto>
        {
            new() { Name = "a.pdf", State = EnumExtractedState.NotRead },
            new() { Name = "b.pdf", State = EnumExtractedState.Complete },
            new() { Name = "photo.png", FileType = "image/png", State = EnumExtractedState.NotRead },
            new() { Name = "c.odt", State = EnumExtractedState.NotRead }
        };

        CollectionAssert.AreEqual(new[] { "a.pdf", "c.odt" }, ResourceProcessingRules.DocumentsToRead(states).Select(s => s.Name).ToList());
    }

    [TestMethod]
    public void TheEstimate_NeedsTheTimeOfARun()
    {
        Assert.IsNull(ResourceProcessingRules.EstimateSeconds(10, null), "before the first run, only the counts");
        Assert.AreEqual(30.0, ResourceProcessingRules.EstimateSeconds(10, 3.0));
        Assert.AreEqual(36.0, ResourceProcessingRules.EstimateSeconds(10, 3.0, 3, 2.0));
        Assert.IsNull(ResourceProcessingRules.EstimateSeconds(10, 3.0, 3, null), "documents without a pace of their own");
        Assert.AreEqual(30.0, ResourceProcessingRules.EstimateSeconds(10, 3.0, 0, null), "no documents, no pace needed");
        Assert.IsNull(ResourceProcessingRules.Pace(0, 12), "a run that did nothing says nothing of the time");
        Assert.AreEqual(2.5, ResourceProcessingRules.Pace(4, 10));
        // Once it has done some, the run goes with its own time.
        Assert.AreEqual(20.0, ResourceProcessingRules.RemainingSeconds(10, 5, 10, 9.0));
        Assert.AreEqual(90.0, ResourceProcessingRules.RemainingSeconds(10, 0, 10, 9.0));
    }

    [TestMethod]
    public void RunConfirmation_SaysHowManyWithWhichModel_HowLong_AndIfItIsPaid()
    {
        var status = new ResourceProcessingStatusDto
        {
            PendingPhotos = 10, PersonPhotos = 2, SecondsPerPhoto = 3, PendingDocuments = 4, PersonDocuments = 1, SecondsPerDocument = 2, DocumentsToRead = 6
        };
        var paid = new ResourceDescriberDto { Model = "gpt-test", IsPaid = true, SupportsImages = true };

        var replace = ResourceProcessingTexts.RunConfirmation(status, new ResourceProcessingOptionsDto { PersonDescriptions = EnumPersonDescriptions.Replace }, paid);
        var keep = ResourceProcessingTexts.RunConfirmation(status,
            new ResourceProcessingOptionsDto { Documents = false, PersonDescriptions = EnumPersonDescriptions.Lock }, new ResourceDescriberDto { Model = "qwen3.5:9b" });

        StringAssert.Contains(replace, "The AI describes 12 photos and summarizes 5 documents with the model gpt-test.");
        StringAssert.Contains(replace, "KNote reads the text of 6 documents first (without the AI).");
        StringAssert.Contains(replace, "It should take about 46 s.");
        StringAssert.Contains(replace, "gpt-test is a paid model: these photos and documents are sent");
        StringAssert.Contains(keep, "describes 10 photos with");
        StringAssert.Contains(keep, "2 descriptions written by people are locked");
        Assert.IsFalse(keep.Contains("documents"));
        Assert.IsFalse(keep.Contains("paid"));
    }

    [TestMethod]
    public void HasWork_AlsoReadingDocuments()
    {
        var onlyToRead = new ResourceProcessingStatusDto { DocumentsToRead = 2 };

        Assert.IsTrue(ResourceProcessingTexts.HasWork(onlyToRead, new ResourceProcessingOptionsDto()));
        Assert.IsFalse(ResourceProcessingTexts.HasWork(onlyToRead, new ResourceProcessingOptionsDto { Documents = false }));
        Assert.IsTrue(ResourceProcessingTexts.HasWork(new ResourceProcessingStatusDto { PersonDocuments = 1 },
            new ResourceProcessingOptionsDto { PersonDescriptions = EnumPersonDescriptions.Lock }));
    }

    [TestMethod]
    public void Progress_SaysWhatWasDone()
    {
        var photos = new ResourceProcessingProgressDto
        {
            Phase = EnumResourceProcessingPhase.Completed, PhotosTotal = 3, PhotosDescribed = 3, PhotosLocked = 2, NotAvailable = 1, ElapsedSeconds = 9
        };
        var documents = new ResourceProcessingProgressDto
        {
            Phase = EnumResourceProcessingPhase.Completed, DocumentsTotal = 4, DocumentsSummarized = 2, DocumentsToReadTotal = 3, DocumentsRead = 3,
            DocumentsWaiting = 1, ElapsedSeconds = 9
        };
        var reading = new ResourceProcessingProgressDto
        {
            Phase = EnumResourceProcessingPhase.Processing, Step = EnumResourceProcessingStep.ReadingDocuments, DocumentsRead = 1, DocumentsToReadTotal = 3
        };

        Assert.AreEqual("Done: 3 photos described in 9 s · 2 locked · 1 not available here.", ResourceProcessingTexts.Progress(photos));
        Assert.AreEqual("Done: 2 documents summarized, 3 documents read in 9 s · 1 document scanned, waiting for transcription.",
            ResourceProcessingTexts.Progress(documents));
        Assert.AreEqual("Reading documents: 1 of 3", ResourceProcessingTexts.Progress(reading));
        Assert.AreEqual((1, 3), ResourceProcessingTexts.Bar(reading));
        Assert.IsNull(ResourceProcessingTexts.Errors(photos));
    }
}

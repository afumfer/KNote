using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Tests;

[TestClass]
public class AiAttachmentRulesTests
{
    private static AiChatAttachmentDto File(string name) => new() { ResourceId = Guid.NewGuid(), FileName = name };

    [TestMethod]
    [DataRow("image/png", true)]
    [DataRow("image/jpeg", true)]
    [DataRow("image/gif", true)]
    [DataRow("image/webp", true)]
    [DataRow("image/svg+xml", false)]
    [DataRow("application/pdf", false)]
    [DataRow(null, false)]
    public void IsImage_OnlyTheTypesAModelAndAPageCanShow(string contentType, bool expected) =>
        Assert.AreEqual(expected, AiAttachmentRules.IsImage(contentType));

    [TestMethod]
    public void ContentTypeOf_ComesFromTheExtension_AndAnyOtherFileIsBinary()
    {
        Assert.AreEqual("image/jpeg", AiAttachmentRules.ContentTypeOf("photo.JPG"));
        Assert.AreEqual("application/octet-stream", AiAttachmentRules.ContentTypeOf("manual.pdf"));
    }

    [TestMethod]
    [DataRow("photo.jpg", "photo.jpg")]
    [DataRow(@"C:\Users\me\photo.jpg", "photo.jpg")]
    [DataRow("../../secret.txt", "secret.txt")]
    [DataRow("a<b>c|d.png", "a_b_c_d.png")]
    [DataRow(".png", "file.png")]
    [DataRow("", "file")]
    [DataRow(null, "file")]
    public void SafeFileName_LeavesNoFoldersNorCharactersThatBreakAFileName(string name, string expected) =>
        Assert.AreEqual(expected, AiAttachmentRules.SafeFileName(name));

    [TestMethod]
    public void SafeFileName_KeepsTheEndOfALongName_WhereItsExtensionIs()
    {
        var name = AiAttachmentRules.SafeFileName(new string('a', 300) + ".png");

        Assert.AreEqual(120, name.Length);
        StringAssert.EndsWith(name, ".png");
    }

    [TestMethod]
    public void SizeError_RefusesAnEmptyFileAndOneOverTheLimitOfTheResources()
    {
        Assert.IsNull(AiAttachmentRules.SizeError("a.png", 1));
        Assert.IsNull(AiAttachmentRules.SizeError("a.png", KntConst.MaxLenResourceFile));
        StringAssert.Contains(AiAttachmentRules.SizeError("a.png", 0), "empty");
        StringAssert.Contains(AiAttachmentRules.SizeError("a.png", KntConst.MaxLenResourceFile + 1L), "too big");
    }

    [TestMethod]
    public void FindAttachment_ByItsName_WithoutMinding_TheCase()
    {
        var photo = File("Photo.jpg");

        var (found, error) = AiAttachmentRules.FindAttachment(new[] { File("a.png"), photo }, new[] { photo }, " photo.jpg ");

        Assert.AreSame(photo, found);
        Assert.IsNull(error);
    }

    [TestMethod]
    public void FindAttachment_WithoutAName_TakesTheOnlyOneOfTheLastMessage()
    {
        var earlier = File("old.png");
        var last = File("new.png");

        Assert.AreSame(last, AiAttachmentRules.FindAttachment(new[] { earlier, last }, new[] { last }, null).File);
    }

    [TestMethod]
    public void FindAttachment_ThatCouldBeSeveral_OrDoesNotExist_GivesTheOptions()
    {
        var a = File("a.png");
        var b = File("b.png");

        StringAssert.Contains(AiAttachmentRules.FindAttachment(new[] { a, b }, new[] { a, b }, null).Error, "a.png, b.png");
        StringAssert.Contains(AiAttachmentRules.FindAttachment(new[] { a, b }, new[] { b }, "c.png").Error, "a.png, b.png");
        StringAssert.Contains(AiAttachmentRules.FindAttachment(Array.Empty<AiChatAttachmentDto>(), Array.Empty<AiChatAttachmentDto>(), null).Error, "no files");
    }

    [TestMethod]
    public void PickImages_WithoutSayingWhich_TakesTheFirstOnes()
    {
        CollectionAssert.AreEqual(new[] { 0, 1, 2 }, AiAttachmentRules.PickImages(7, null).Positions);
        CollectionAssert.AreEqual(new[] { 0, 1 }, AiAttachmentRules.PickImages(2, Array.Empty<int>()).Positions);
    }

    [TestMethod]
    public void PickImages_TheOnesAskedFor_CountedFromOne_AndNotMoreThanTheLimit()
    {
        CollectionAssert.AreEqual(new[] { 4, 6 }, AiAttachmentRules.PickImages(7, new[] { 5, 7, 5 }).Positions);
        Assert.AreEqual(AiAttachmentRules.MaxImagesPerLook, AiAttachmentRules.PickImages(7, new[] { 1, 2, 3, 4, 5 }).Positions.Count);
    }

    [TestMethod]
    public void PickImages_ThatDoNotExist_SaysHowManyThereAre()
    {
        StringAssert.Contains(AiAttachmentRules.PickImages(2, new[] { 3 }).Error, "there are 2");
        Assert.IsNotNull(AiAttachmentRules.PickImages(2, new[] { 0 }).Error);
    }

    [TestMethod]
    public void PromptWithAttachments_WithoutFiles_IsThePrompt()
    {
        Assert.AreEqual("Question", AiAttachmentRules.PromptWithAttachments("Question", null));
        Assert.AreEqual("Question", AiAttachmentRules.PromptWithAttachments("Question", Array.Empty<AiChatAttachmentDto>()));
    }

    [TestMethod]
    public void PromptWithAttachments_InTheHistory_NamesThem()
    {
        var text = AiAttachmentRules.PromptWithAttachments("Question", new[] { File("a.jpg"), File("b.pdf") });

        Assert.AreEqual("Question\n\n[Attached to this message: a.jpg, b.pdf]", text);
    }

    [TestMethod]
    public void PromptWithAttachments_InTheMessageSent_SaysWhichOnesTheModelSees()
    {
        var image = File("a.jpg");
        var pdf = File("b.pdf");

        var text = AiAttachmentRules.PromptWithAttachments("Question", new[] { image, pdf }, new[] { image.ResourceId });

        StringAssert.Contains(text, "[Images attached, which you can see: a.jpg]");
        StringAssert.Contains(text, "[Files attached, whose content you cannot see: b.pdf]");
    }

    [TestMethod]
    public void PromptWithAttachments_ForAModelWithoutVision_SaysItSeesNone()
    {
        var text = AiAttachmentRules.PromptWithAttachments("Question", new[] { File("a.jpg") }, Array.Empty<Guid>());

        StringAssert.Contains(text, "you cannot see: a.jpg");
        Assert.IsFalse(text.Contains("which you can see"), text);
    }
}

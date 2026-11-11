using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// AiDocumentRules (KNote Atlas, stage 4, 4.4c): which documents are summarized and what «Describe with AI» does with each one
/// (as the photos), the beginning of the text the model receives (the pages with their number, up to 16,000 characters) and the
/// request, whose last line names the topic of the note (measured: without it, the summaries came in the document's language).
/// </summary>
[TestClass]
public class AiDocumentRulesTests
{
    private static ResourceInfoDto Resource(string name, string? description = null, bool locked = false, string? model = null) =>
        new() { ResourceId = Guid.NewGuid(), Name = $"{Guid.NewGuid()}_{name}", Description = description ?? name, DescriptionByUser = locked, DescriptionModel = model };

    [TestMethod]
    public void PlanForNote_AsThePhotos()
    {
        Assert.AreEqual(EnumPhotoAction.Describe, AiDocumentRules.PlanForNote(Resource("manual.pdf"), EnumPersonDescriptions.Skip));
        Assert.AreEqual(EnumPhotoAction.Skip, AiDocumentRules.PlanForNote(Resource("manual.pdf", "A manual.", model: "qwen3.5:9b"), EnumPersonDescriptions.Replace));
        Assert.AreEqual(EnumPhotoAction.Skip, AiDocumentRules.PlanForNote(Resource("manual.pdf", "Mine.", locked: true), EnumPersonDescriptions.Replace));
        Assert.AreEqual(EnumPhotoAction.Lock, AiDocumentRules.PlanForNote(Resource("letter.docx", "Carta de la abuela."), EnumPersonDescriptions.Lock));
        Assert.AreEqual(EnumPhotoAction.Skip, AiDocumentRules.PlanForNote(Resource("photo.png"), EnumPersonDescriptions.Replace), "a photo is not a document");
        Assert.AreEqual(EnumPhotoAction.Describe, AiDocumentRules.PlanResourceForNote(Resource("photo.png"), EnumPersonDescriptions.Skip), "but it is described");
        Assert.IsFalse(AiDocumentRules.CanSummarize(Resource("manual.pdf", "Mine.", locked: true)));
        Assert.IsTrue(AiDocumentRules.CanSummarize(Resource("manual.pdf", "A manual.", model: "qwen3.5:9b")), "again, maybe with another model");
    }

    [TestMethod]
    public void DocumentText_GivesThePagesWithTheirNumber_UpToTheMax()
    {
        var text = string.Join(DocumentTextRules.PageSeparator, "First page.", "\u0001", "Third page with OCR.", new string('x', 100));

        Assert.AreEqual("[Page 1]\nFirst page.\n\n[Page 3]\nThird page with OCR.\n\n[Page 4]\n" + new string('x', 100),
            AiDocumentRules.DocumentText(text, paged: true));
        Assert.AreEqual("One letter.", AiDocumentRules.DocumentText("One letter.", paged: false));
        Assert.AreEqual("", AiDocumentRules.DocumentText("\u0001", paged: true), "a scan without text: nothing to summarize");
        Assert.IsTrue(AiDocumentRules.DocumentText(string.Join(" ", Enumerable.Repeat("word", 10_000)), false).Length <= AiDocumentRules.MaxDocumentTextLength + 1);
    }

    [TestMethod]
    public void Request_NamesTheTopicInItsLastLine()
    {
        var request = AiDocumentRules.Request("Manuales del QL", "Ampliaciones.", "q68.pdf", 13, "The Q68 manual.", "Lo trajo Lucía.");

        StringAssert.StartsWith(request, "The note this document belongs to:\n---\nTopic: Manuales del QL\n\nAmpliaciones.\n---");
        StringAssert.Contains(request, "What a person wrote of this document (it is true: keep its names and facts):\nLo trajo Lucía.");
        StringAssert.Contains(request, "The document: q68.pdf, 13 pages. Its beginning:\n---\nThe Q68 manual.\n---");
        StringAssert.EndsWith(request, "Summarize this document in two or three sentences, without mentioning the note. " +
            "Write in the language of the topic of the note («Manuales del QL»).");
    }

    [TestMethod]
    public void Request_WithoutANote_IsInTheLanguageOfTheDocument()
    {
        var request = AiDocumentRules.Request("", "", "letter.docx", 1, "Dear friend.");

        Assert.IsFalse(request.Contains("The note"));
        Assert.IsFalse(request.Contains("pages"));
        StringAssert.EndsWith(request, "Summarize this document in two or three sentences. Write in the language of the document.");
        StringAssert.EndsWith(AiDocumentRules.Request("", "Notas en español.", "a.pdf", 1, "Text."), "Write in the language of the text of the note.");
    }
}

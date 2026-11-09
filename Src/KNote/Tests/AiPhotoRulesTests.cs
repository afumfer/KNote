using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// AiPhotoRules (KNote Atlas, stage 4, 4.2): which resources are photos, what «Describe photos» does with each one, the
/// request of the model and how its answer is cleaned; and AiProviderSelection.ForRepository.
/// </summary>
[TestClass]
public class AiPhotoRulesTests
{
    private static ResourceInfoDto Resource(string name, string fileType = null, string description = null, bool locked = false, string model = null) =>
        new() { Name = name, FileType = fileType, Description = description ?? name, DescriptionByUser = locked, DescriptionModel = model };

    [TestMethod]
    [DataRow("photo.jpg", null, true)]
    [DataRow("photo.JPEG", "", true)]
    [DataRow("scan.bin", "image/png", true)]
    [DataRow("photo.webp", "image/webp", true)]
    [DataRow("manual.pdf", "application/pdf", false)]
    [DataRow("photo.bmp", "image/bmp", false)]
    public void IsPhoto(string name, string fileType, bool expected)
    {
        Assert.AreEqual(expected, AiPhotoRules.IsPhoto(fileType, name));
    }

    [TestMethod]
    public void PlanForNote_DescribesThePhotosWithoutADescriptionOfTheirOwn_AndOnlyThem()
    {
        var choice = EnumPersonDescriptions.Skip;
        Assert.AreEqual(EnumPhotoAction.Describe, AiPhotoRules.PlanForNote(Resource("photo.png"), choice));
        Assert.AreEqual(EnumPhotoAction.Describe, AiPhotoRules.PlanForNote(Resource("photo.png", description: ""), choice));
        Assert.AreEqual(EnumPhotoAction.Skip, AiPhotoRules.PlanForNote(Resource("manual.pdf"), choice));
        Assert.AreEqual(EnumPhotoAction.Skip, AiPhotoRules.PlanForNote(Resource("photo.png", locked: true), choice));
        Assert.AreEqual(EnumPhotoAction.Skip, AiPhotoRules.PlanForNote(Resource("photo.png", description: "A QL.", model: "qwen3.5:9b"), choice));
    }

    [TestMethod]
    [DataRow(EnumPersonDescriptions.Replace, EnumPhotoAction.Describe)]
    [DataRow(EnumPersonDescriptions.Lock, EnumPhotoAction.Lock)]
    [DataRow(EnumPersonDescriptions.Skip, EnumPhotoAction.Skip)]
    public void PlanForNote_APhotoAPersonDescribed_IsAsChosen(EnumPersonDescriptions choice, EnumPhotoAction expected)
    {
        var photo = Resource("photo.png", description: "Grandma Carmen in Cádiz.");

        Assert.IsTrue(AiPhotoRules.HasPersonDescription(photo));
        Assert.AreEqual(expected, AiPhotoRules.PlanForNote(photo, choice));
    }

    [TestMethod]
    public void CanDescribe_APhotoNotLocked_AlsoOneTheAiDescribed()
    {
        Assert.IsTrue(AiPhotoRules.CanDescribe(Resource("photo.png", description: "A QL.", model: "qwen3.5:9b")));
        Assert.IsFalse(AiPhotoRules.CanDescribe(Resource("photo.png", locked: true)));
        Assert.IsFalse(AiPhotoRules.CanDescribe(Resource("manual.pdf")));
    }

    [TestMethod]
    public void Request_WithANote_GivesItsContext_AndAsksForItsLanguage()
    {
        var request = AiPhotoRules.Request("Viaje a Cádiz", "Restauramos el ordenador de la abuela Carmen.", ["Carmen y Lucía en la playa."]);

        StringAssert.Contains(request, "Topic: Viaje a Cádiz");
        StringAssert.Contains(request, "Restauramos el ordenador de la abuela Carmen.");
        StringAssert.Contains(request, "- Carmen y Lucía en la playa.");
        StringAssert.Contains(request, "Write in the language of the note");
    }

    [TestMethod]
    public void Request_GivesWhatAPersonWroteOfThisPhoto()
    {
        var request = AiPhotoRules.Request("Cumpleaños", "", [], "La abuela Carmen es la señora del chaleco amarillo.");
        var withoutNote = AiPhotoRules.Request(null, null, [], "La abuela Carmen.");

        StringAssert.Contains(request, "What a person wrote of this photo (it is true: keep its names and facts):\nLa abuela Carmen es la señora del chaleco amarillo.\n\nDescribe this photo. Write in the language of the note");
        Assert.IsFalse(withoutNote.Contains("The note this photo belongs to"));
        StringAssert.EndsWith(withoutNote, "Write in the language of what the person wrote of it.");
    }

    [TestMethod]
    public void PhotoDescription_IsOnlyWhatAPersonWrote_NotTheFileName_NorTheAIs()
    {
        var own = Resource("photo.png", description: "Abuela Carmen");
        var fileName = Resource("photo.png", description: "photo.png");
        var ai = Resource("photo.png", description: "A beach.", model: "qwen3.5:9b");

        Assert.AreEqual("Abuela Carmen", AiPhotoRules.PhotoDescription(own));
        Assert.IsNull(AiPhotoRules.PhotoDescription(fileName));
        Assert.IsNull(AiPhotoRules.PhotoDescription(ai));
    }

    [TestMethod]
    public void Request_WithoutANote_DoesNotMentionOne()
    {
        var request = AiPhotoRules.Request(null, "  ", []);

        Assert.AreEqual("Describe this photo. Write in English.", request);
    }

    [TestMethod]
    public void NoteText_IsCutAtAWord()
    {
        var text = AiPhotoRules.NoteText(string.Join(" ", Enumerable.Repeat("palabra", 400)));

        Assert.IsLessThanOrEqualTo(AiPhotoRules.MaxNoteTextLength + 1, text.Length);
        Assert.EndsWith("palabra…", text);
    }

    [TestMethod]
    public void PersonDescriptions_AreTheLockedOnesWrittenByPeople()
    {
        var descriptions = AiPhotoRules.PersonDescriptions(
        [
            Resource("a.png", description: "Written and locked.", locked: true),
            Resource("b.png", description: "Written, not locked."),
            Resource("c.png", description: "By the AI and locked.", locked: true, model: "qwen3.5:9b"),
            Resource("d.png", locked: true)
        ]);

        CollectionAssert.AreEqual(new[] { "Written and locked." }, descriptions);
    }

    [TestMethod]
    [DataRow("A Sinclair QL on a desk.", "A Sinclair QL on a desk.")]
    [DataRow("**Description:** A QL\non a desk.", "A QL on a desk.")]
    [DataRow("Descripción: Un QL sobre una mesa.", "Un QL sobre una mesa.")]
    [DataRow("\"A QL on a desk.\"", "A QL on a desk.")]
    [DataRow("   ", "")]
    public void CleanDescription(string answer, string expected)
    {
        Assert.AreEqual(expected, AiPhotoRules.CleanDescription(answer));
    }

    [TestMethod]
    public void CleanDescription_ALongAnswer_IsCutAtASentence()
    {
        var sentence = "A Sinclair QL with its two microdrives on a white desk next to a monitor. ";
        var clean = AiPhotoRules.CleanDescription(string.Concat(Enumerable.Repeat(sentence, 20)));

        Assert.IsLessThanOrEqualTo(AiPhotoRules.MaxDescriptionLength, clean.Length);
        Assert.EndsWith("monitor.", clean);
    }

    [TestMethod]
    public void ForRepository_IsTheProviderOfTheRecommendedModel_OrTheFallback()
    {
        var small = new AiProviderRef { Alias = "Small", Provider = EnumAiProvider.Ollama, Model = "qwen3.5:9b" };
        var big = new AiProviderRef { Alias = "Big", Provider = EnumAiProvider.Ollama, Model = "qwen3.6:35b-a3b" };
        var providers = new[] { small, big };

        Assert.AreSame(big, AiProviderSelection.ForRepository(providers, " QWEN3.6:35b-a3b ", small));
        Assert.AreSame(small, AiProviderSelection.ForRepository(providers, "gpt-5", small));
        Assert.AreSame(small, AiProviderSelection.ForRepository(providers, null, small));
        Assert.IsNull(AiProviderSelection.ForRepository(Array.Empty<AiProviderRef>(), "qwen3.5:9b", null));
    }
}

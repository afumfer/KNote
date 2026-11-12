using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// ResourceDescriptionRules (KNote Atlas, stage 4, D12): what a description says, its title, the lock while editing, what
/// is saved, who wrote it and the text for a link.
/// </summary>
[TestClass]
public class ResourceDescriptionRulesTests
{
    private const string Guid1 = "dc47b0c0-8d49-4580-8bb2-12112e3f4a5b";

    [TestMethod]
    [DataRow(null, "photo.png", false)]
    [DataRow("  ", "photo.png", false)]
    [DataRow("photo.png", "photo.png", false)]
    [DataRow("PHOTO", "photo.png", false)]
    [DataRow("Image inserted from clipboard.", "ClipboardImg_x.png", false)]
    // The web keeps the file name with its spaces, accents and symbols replaced, and the description with the original one.
    [DataRow("mi foto (1).png", "mi_foto__1_.png", false)]
    [DataRow("canción.jpg", "cancion.jpg", false)]
    [DataRow("report.pdf", Guid1 + "_report.pdf", false)]
    [DataRow("A Sinclair QL on a desk.", "photo.png", true)]
    [DataRow("写真", "__.png", true)]
    public void HasOwnDescription(string description, string name, bool expected)
    {
        Assert.AreEqual(expected, ResourceDescriptionRules.HasOwnDescription(description, name));
    }

    [TestMethod]
    [DataRow("A Sinclair QL on a desk. A screwdriver lies next to it.", "A Sinclair QL on a desk")]
    [DataRow("Is it a QL? The keyboard says so.", "Is it a QL?")]
    [DataRow("The Atari 520ST v1.2 board", "The Atari 520ST v1.2 board")]
    [DataRow("# Heading line\nSecond line.", "Heading line")]
    [DataRow("Un coche de policía blanco estacionado en una carretera al atardecer, con el cielo naranja.",
        "Un coche de policía blanco estacionado en una carretera al…")]
    public void Title_IsTheFirstSentence_CutAtAWord(string description, string expected)
    {
        Assert.AreEqual(expected, ResourceDescriptionRules.Title(description, "photo.png"));
    }

    [TestMethod]
    public void Title_WithoutADescriptionOfItsOwn_IsTheFileName()
    {
        Assert.AreEqual("photo.png", ResourceDescriptionRules.Title("photo.png", Guid1 + "_photo.png"));
        Assert.AreEqual("photo.png", new ResourceInfoDto { Name = Guid1 + "_photo.png", Description = "" }.GetTitle());
    }

    [TestMethod]
    public void ForLink_ReplacesWhatWouldBreakTheLink()
    {
        Assert.AreEqual("The ’QL’ and ”its” ‹box› (manual) a/b c", ResourceDescriptionRules.ForLink("The 'QL' and \"its\" <box> [manual] a\\b\r\nc"));
    }

    [TestMethod]
    [DataRow("photo.png", false, "photo.png", false, DisplayName = "Not changed")]
    [DataRow("photo.png", false, "A desk with a QL.", true, DisplayName = "A text of a person")]
    [DataRow("photo.png", false, "", false, DisplayName = "Emptied")]
    [DataRow("photo.png", true, "", true, DisplayName = "Locked stays locked")]
    [DataRow("An AI text.", false, "An AI text.", false, DisplayName = "The AI text, not changed")]
    public void LockWhileEditing(string loaded, bool loadedLock, string text, bool expected)
    {
        Assert.AreEqual(expected, ResourceDescriptionRules.LockWhileEditing(loaded, loadedLock, text, "photo.png"));
    }

    [TestMethod]
    public void Decide_ANewResource_KeepsItsLockAndHasNoModel()
    {
        var decided = ResourceDescriptionRules.Decide(null, null, "A desk.", true, null, "photo.png");

        Assert.AreEqual(new ResourceDescriptionState("A desk.", true, null), decided);
    }

    [TestMethod]
    public void Decide_AnEditorThatDidntChangeTheText_KeepsWhatIsSaved()
    {
        // The AI wrote meanwhile: the editor loaded the file name.
        var stored = new ResourceDescriptionState("A QL on a desk.", false, "qwen3.5:9b");

        var decided = ResourceDescriptionRules.Decide(stored, "photo.png", "photo.png", false, "photo.png", "photo.png");

        Assert.AreEqual(stored, decided);
    }

    [TestMethod]
    public void Decide_LockingAnAiDescriptionWithoutChangingIt_KeepsItsModel()
    {
        var stored = new ResourceDescriptionState("A QL on a desk.", false, "qwen3.5:9b");

        var decided = ResourceDescriptionRules.Decide(stored, "photo.png", "A QL on a desk.", true, "A QL on a desk.", "photo.png");

        Assert.AreEqual(new ResourceDescriptionState("A QL on a desk.", true, "qwen3.5:9b"), decided);
    }

    [TestMethod]
    public void Decide_TheCheckboxWins_AlsoWhenTheTextChanges()
    {
        var stored = new ResourceDescriptionState("Grandma Carmen in Cádiz.", true, null);

        var decided = ResourceDescriptionRules.Decide(stored, "photo.png", "Carmen and Lucía in Cádiz.", false, "Grandma Carmen in Cádiz.", "photo.png");

        Assert.AreEqual(new ResourceDescriptionState("Carmen and Lucía in Cádiz.", false, null), decided);
    }

    [TestMethod]
    public void Decide_AnEditorChangingAnAiText_ClearsTheModel()
    {
        var stored = new ResourceDescriptionState("A QL on a desk.", false, "qwen3.5:9b");

        var decided = ResourceDescriptionRules.Decide(stored, "photo.png", "My QL on my desk.", true, "A QL on a desk.", "photo.png");

        Assert.AreEqual(new ResourceDescriptionState("My QL on my desk.", true, null), decided);
    }

    [TestMethod]
    [DataRow("My QL.", true)]
    [DataRow("photo.png", false)]
    [DataRow("", false)]
    public void Decide_WhoIsNotAnEditor_ChangingTheTextToOneOfItsOwn_LocksIt(string description, bool expectedLock)
    {
        var stored = new ResourceDescriptionState("A QL on a desk.", false, "qwen3.5:9b");

        var decided = ResourceDescriptionRules.Decide(stored, "photo.png", description, false, null, "photo.png");

        Assert.AreEqual(new ResourceDescriptionState(description, expectedLock, null), decided);
    }

    [TestMethod]
    public void Decide_AnotherFile_ClearsTheModel()
    {
        var stored = new ResourceDescriptionState("A QL on a desk.", false, "qwen3.5:9b");

        var decided = ResourceDescriptionRules.Decide(stored, Guid1 + "_photo.png", "A QL on a desk.", false, "A QL on a desk.", Guid1 + "_other.png");

        Assert.AreEqual(new ResourceDescriptionState("A QL on a desk.", false, null), decided);
    }

    [TestMethod]
    public void Decide_OnlyLineEndsOrSpacesChanged_IsNotAChange()
    {
        var stored = new ResourceDescriptionState("Line one.\r\nLine two.", false, "qwen3.5:9b");

        var decided = ResourceDescriptionRules.Decide(stored, "photo.png", " Line one.\nLine two. ", false, "Line one.\r\nLine two.", "photo.png");

        Assert.AreEqual(stored, decided);
    }

    [TestMethod]
    [DataRow("A QL.", false, "qwen3.5:9b", "Written by the AI (qwen3.5:9b).", false)]
    [DataRow("A QL.", true, null, "Written by a person.", false)]
    [DataRow("A QL.", false, null, "Written by a person, not locked: the AI may rewrite it and this text would be lost.", true)]
    [DataRow("photo.png", true, null, "Locked without a description: the AI won't describe this resource.", false)]
    [DataRow("photo.png", false, null, "", false)]
    public void Author(string description, bool locked, string model, string expected, bool atRisk)
    {
        Assert.AreEqual(expected, ResourceDescriptionRules.Author(description, locked, model, "photo.png"));
        Assert.AreEqual(atRisk, ResourceDescriptionRules.IsAtRisk(description, locked, model, "photo.png"));
    }

    [TestMethod]
    public void ModelWhileEditing_IsTheLoadedOneWhileTheTextIsTheSame()
    {
        Assert.AreEqual("qwen3.5:9b", ResourceDescriptionRules.ModelWhileEditing("A QL.", "qwen3.5:9b", "A QL."));
        Assert.IsNull(ResourceDescriptionRules.ModelWhileEditing("A QL.", "qwen3.5:9b", "My QL."));
    }
}

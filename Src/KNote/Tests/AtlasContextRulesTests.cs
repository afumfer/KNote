using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// The pure rules of the context of KNote Atlas (stage 1, delivery 1.1): the Markdown of the notes people write for the AI
/// (AtlasMarkdown), the glossary (GlossaryTable) and what the AI receives in layers (AiPromptComposer).
/// </summary>
[TestClass]
public class AtlasContextRulesTests
{
    #region AtlasMarkdown

    [TestMethod]
    public void Clean_RemovesTheComments_AndTheEmptySections()
    {
        var clean = AtlasMarkdown.Clean(AtlasTemplates.Profile.Replace("## Conventions\n\n", "## Conventions\n\nTags in English.\n\n"));

        Assert.AreEqual("## Conventions\n\nTags in English.", clean);
    }

    [TestMethod]
    public void Clean_ATemplateLeftAsItIs_GivesNothing()
    {
        Assert.AreEqual("", AtlasMarkdown.Clean(AtlasTemplates.Profile));
        Assert.AreEqual("", AtlasMarkdown.Clean(AtlasTemplates.TaskPrompt(AiTaskCatalog.Assistant)));
    }

    [TestMethod]
    public void Clean_KeepsAHeading_WhoseSubsectionsHaveText()
    {
        var clean = AtlasMarkdown.Clean("## Folders\n\n### Projects\n\nThe team's projects.\n\n### Empty\n\n## Note types\n");

        Assert.AreEqual("## Folders\n\n### Projects\n\nThe team's projects.", clean);
    }

    [TestMethod]
    public void TrimToWords_CutsAtAWord_AndSaysSo()
    {
        Assert.AreEqual(("one two […]", true), AtlasMarkdown.TrimToWords("one two three", 2));
        Assert.AreEqual(("one two", false), AtlasMarkdown.TrimToWords("one two", 2));
        Assert.AreEqual(3, AtlasMarkdown.CountWords(" one\ntwo  three "));
    }

    [TestMethod]
    public void AddMissingLines_AddsOnlyWhatIsMissing_AtTheEndOfItsSection()
    {
        var text = "## Folders\n\n- Home: everything\n\n## Note types\n\n- Task:\n";

        var added = AtlasMarkdown.AddMissingLines(text, "folders", new[] { "- Home:", "- Home › Projects:" });

        Assert.AreEqual("## Folders\n\n- Home: everything\n- Home › Projects:\n\n## Note types\n\n- Task:\n", added);
        Assert.AreEqual(added, AtlasMarkdown.AddMissingLines(added, "Folders", new[] { "- Home:", "- Home › Projects:" }), "nothing twice");
    }

    [TestMethod]
    public void AddMissingLines_AfterTheHelpOfTheTemplate_AndWithoutTheSection_AtTheEnd()
    {
        var added = AtlasMarkdown.AddMissingLines(AtlasTemplates.Profile, AtlasTemplates.FoldersSection, new[] { "- Home:" });
        StringAssert.Contains(added, "adds the folders missing here. -->\n\n- Home:\n\n## Note types");

        var appended = AtlasMarkdown.AddMissingLines("Some text.", "Folders", new[] { "- Home:" });
        Assert.AreEqual("Some text.\n\n## Folders\n\n- Home:\n", appended);
    }

    [TestMethod]
    public void AddMissingLines_KeepsTheLineEndingsOfTheNote()
    {
        var text = AtlasTemplates.NoteText("## Folders\n\n- Home: everything\n\n## Note types\n");

        var added = AtlasMarkdown.AddMissingLines(text, "Folders", new[] { "- Home:", "- Projects:" });

        Assert.AreEqual("## Folders\r\n\r\n- Home: everything\r\n- Projects:\r\n\r\n## Note types\r\n", added);
        Assert.AreEqual(text, AtlasMarkdown.AddMissingLines(text, "Folders", new[] { "- Home:" }), "nothing to add, nothing changes");
    }

    [TestMethod]
    public void Clean_AndParse_ReadTheNotesWithCrLf()
    {
        Assert.AreEqual("## Purpose\n\nRetro.", AtlasMarkdown.Clean(AtlasTemplates.NoteText("<!-- help -->\n## Purpose\n\nRetro.\n\n## Empty\n")));
        Assert.AreEqual(3, GlossaryTable.Parse(AtlasTemplates.NoteText(Glossary)).Terms.Count);
    }

    #endregion

    #region GlossaryTable

    private const string Glossary =
        "| Term | Meaning | Also written as |\n" +
        "|---|---|---|\n" +
        "| CPC | Amstrad Colour Personal Computer | Amstrad CPC, CPC464 |\n" +
        "| Varilla | The billing module | módulo de facturas |\n" +
        "| Tía Lola | Grandma Carmen's sister | Lola |\n" +
        "| | Without a term | |\n" +
        "| QL | |\n" +
        "| cpc | Repeated | |\n";

    [TestMethod]
    public void Parse_ReadsTheTerms_AndTellsTheProblemsOfItsRows()
    {
        var (terms, issues) = GlossaryTable.Parse(Glossary);

        CollectionAssert.AreEqual(new[] { "CPC", "Varilla", "Tía Lola" }, terms.Select(t => t.Term).ToList());
        CollectionAssert.AreEqual(new[] { "Amstrad CPC", "CPC464" }, terms[0].AlsoWrittenAs);
        CollectionAssert.AreEqual(new[]
        {
            "Row 4: without a term.", "Row 5: «QL» has no meaning.", "Row 6: «cpc» is repeated (row 1)."
        }, issues);
    }

    [TestMethod]
    public void Find_ByWholeWord_WithoutCaseNorAccents_AlsoByTheVariants()
    {
        var terms = GlossaryTable.Parse(Glossary).Terms;

        Assert.AreEqual(0, GlossaryTable.Find(terms, new[] { "The CPCs and the varillas" }).Count, "whole words only");
        CollectionAssert.AreEqual(new[] { "CPC", "Tía Lola" },
            GlossaryTable.Find(terms, new[] { "My cpc464 is with tia lola" }).Select(t => t.Term).ToList());
        CollectionAssert.AreEqual(new[] { "Varilla" },
            GlossaryTable.Find(terms, new[] { "El MODULO DE FACTURAS falla" }).Select(t => t.Term).ToList());
    }

    [TestMethod]
    public void Find_InTheOrderTheyFirstAppear_SoAConversationOnlyAddsTermsAtTheEnd()
    {
        var terms = GlossaryTable.Parse(Glossary).Terms;

        var first = GlossaryTable.Find(terms, new[] { "Lola restored a CPC" });
        var later = GlossaryTable.Find(terms, new[] { "Lola restored a CPC", "Done.", "And the Varilla? And the CPC?" });

        CollectionAssert.AreEqual(new[] { "Tía Lola", "CPC" }, first.Select(t => t.Term).ToList());
        CollectionAssert.AreEqual(new[] { "Tía Lola", "CPC", "Varilla" }, later.Select(t => t.Term).ToList());
        Assert.AreEqual(1, GlossaryTable.Find(terms, new[] { "Lola restored a CPC" }, max: 1).Count);
    }

    #endregion

    #region AiPromptComposer

    private static AiComposedPromptDto Compose(string? taskPrompt = null, bool replaces = false, string? instructionsOverride = null,
        string? profile = null, IReadOnlyList<GlossaryTermDto>? terms = null) =>
        AiPromptComposer.Compose(new AiPromptComposer.Parts
        {
            RepositoryName = "Retro collection",
            UserName = "Armando",
            Role = EnumRoles.Staff,
            Today = new DateTime(2026, 11, 4),
            TaskPromptText = taskPrompt,
            TaskPromptReplaces = replaces,
            TaskPromptNoteNumber = 12,
            InstructionsOverride = instructionsOverride,
            ProfileText = profile,
            ProfileNoteNumber = 10,
            GlossaryTerms = terms ?? Array.Empty<GlossaryTermDto>(),
            GlossaryNoteNumber = 11
        });

    [TestMethod]
    public void Compose_WithoutTheNotes_HasTheRulesAndKNotesInstructions()
    {
        var composed = Compose();

        CollectionAssert.AreEqual(new[] { AiPromptComposer.RulesLayer, AiPromptComposer.InstructionsLayer }, composed.Layers.Select(l => l.Name).ToList());
        StringAssert.StartsWith(composed.Text, "# KNote rules\n\n- You work in KNote, a manager of notes and tasks, in the repository «Retro collection». " +
            "The user is Armando, with the role Staff. Today is Wednesday, 2026-11-04.");
        StringAssert.Contains(composed.Text, "# Instructions\n\n" + AiTaskCatalog.Assistant.DefaultInstructions);
        Assert.IsTrue(composed.Words > 0 && composed.ApproxTokens > 0);
    }

    [TestMethod]
    public void Compose_TheLayersInTheirOrder_AndTheProfileClean()
    {
        var terms = GlossaryTable.Parse(Glossary).Terms.Take(1).ToList();

        var composed = Compose(profile: "<!-- help -->\n## Purpose\n\nA collection of computers.\n\n## Empty\n", terms: terms);

        CollectionAssert.AreEqual(new[]
        {
            AiPromptComposer.RulesLayer, AiPromptComposer.InstructionsLayer, AiPromptComposer.ProfileLayer, AiPromptComposer.GlossaryLayer
        }, composed.Layers.Select(l => l.Name).ToList());
        StringAssert.Contains(composed.Text, "# Repository profile\n\n## Purpose\n\nA collection of computers.\n\n# Glossary");
        StringAssert.Contains(composed.Text, "- **CPC** (also written as Amstrad CPC, CPC464): Amstrad Colour Personal Computer");
        Assert.AreEqual("#10", composed.Layers[2].Source);
        Assert.IsFalse(composed.Text.Contains("help"));
    }

    [TestMethod]
    public void Compose_AddAndReplace_NeverReplaceTheRules()
    {
        var added = Compose(taskPrompt: "<!-- mine -->\nAlways in Spanish.");
        var replaced = Compose(taskPrompt: "Only short answers.", replaces: true);

        StringAssert.Contains(added.Text, AiTaskCatalog.Assistant.DefaultInstructions + "\n\nAlways in Spanish.");
        Assert.AreEqual("KNote + #12", added.Layers[1].Source);
        Assert.AreEqual("Only short answers.", replaced.Layers[1].Text);
        Assert.AreEqual(AiPromptComposer.RulesLayer, replaced.Layers[0].Name, "the rules always stay");
        Assert.AreEqual(AiTaskCatalog.Assistant.DefaultInstructions, Compose(taskPrompt: AtlasTemplates.TaskPrompt(AiTaskCatalog.Assistant)).Layers[1].Text,
            "a note left as its template adds nothing");
    }

    [TestMethod]
    public void Compose_AConversationThatReplacesTheInstructions_ReplacesOnlyThem()
    {
        var composed = Compose(taskPrompt: "Always in Spanish.", instructionsOverride: "You are a translator.", profile: "## Purpose\n\nRetro.");

        Assert.AreEqual("You are a translator.", composed.Layers[1].Text);
        Assert.AreEqual(AiPromptComposer.RulesLayer, composed.Layers[0].Name);
        Assert.AreEqual(AiPromptComposer.ProfileLayer, composed.Layers[2].Name);
    }

    [TestMethod]
    public void Compose_ALongProfile_IsCut()
    {
        var composed = Compose(profile: string.Join(" ", Enumerable.Repeat("word", AiPromptComposer.MaxProfileWords + 10)));

        Assert.IsTrue(composed.ProfileCut);
        StringAssert.EndsWith(composed.Layers[2].Text, $"(The profile was cut: it is longer than {AiPromptComposer.MaxProfileWords} words.)");
        StringAssert.Contains(AiPromptComposer.Summary(composed), "The profile was cut");
    }

    [TestMethod]
    public void AiTaskCatalog_FindsATask_ByItsKeyOrName()
    {
        Assert.AreSame(AiTaskCatalog.Assistant, AiTaskCatalog.Find("assistant"));
        Assert.AreSame(AiTaskCatalog.Assistant, AiTaskCatalog.Find(" Assistant "));
        Assert.IsNull(AiTaskCatalog.Find("Summary"));
    }

    #endregion
}

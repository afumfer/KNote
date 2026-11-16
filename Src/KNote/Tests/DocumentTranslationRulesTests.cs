using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// The rules of the translation of a long document (KNote Atlas, stage 2, 2.1b, D21): its parts (fixed by its text alone), the calls
/// (a page each, or what fits), what the context allows, the translation in the text of its note (a heading before each page, 2.1b2),
/// a part that comes back too short, and the note of the translation with the hidden comment that lets it go on.
/// </summary>
[TestClass]
public class DocumentTranslationRulesTests
{
    private const char Page = DocumentTextRules.PageSeparator;

    [TestMethod]
    public void APagedDocument_GivesEveryPage_TheLongOnesCut_AndTheEmptyAndScannedOnesToo()
    {
        var longPage = string.Join("\n\n", Enumerable.Repeat(new string('a', 1_400), 4));
        var text = $"First{Page}{DocumentTextRules.PendingMark}{Page}{longPage}{Page}{Page}Last";

        var parts = DocumentTranslationRules.Parts(text, paged: true);

        CollectionAssert.AreEqual(new[] { 1, 2, 3, 3, 4, 5 }, parts.Select(p => p.Page).ToArray());
        CollectionAssert.AreEqual(new[] { true, true, true, false, true, true }, parts.Select(p => p.FirstOfPage).ToArray());
        Assert.IsTrue(parts[1].Missing, "the scanned page without text");
        Assert.IsFalse(parts[4].Missing, "an empty page is not scanned");
        Assert.IsTrue(parts.All(p => p.Text.Length <= DocumentTranslationRules.MaxPartLength));
    }

    [TestMethod]
    public void ADocumentWithoutPages_IsCutByParagraphs()
    {
        var text = string.Join("\n\n", Enumerable.Repeat(new string('b', 1_400), 5));

        var parts = DocumentTranslationRules.Parts(text, paged: false);

        Assert.HasCount(3, parts);
        Assert.IsTrue(parts.All(p => p.Page == 0));
    }

    [TestMethod]
    public void WithPages_EachPageIsACall_WithoutThem_ThePartsThatFitTogether()
    {
        var paged = DocumentTranslationRules.Parts($"One{Page}Two{Page}Three", paged: true);
        var unpaged = DocumentTranslationRules.Parts(string.Join("\n\n", Enumerable.Repeat(new string('c', 2_900), 4)), paged: false);

        var pagedCalls = DocumentTranslationRules.Calls(paged, 0, paged: true, callCharacters: 8_000);
        var unpagedCalls = DocumentTranslationRules.Calls(unpaged, 0, paged: false, callCharacters: 8_000);
        var goingOn = DocumentTranslationRules.Calls(paged, 1, paged: true, callCharacters: 8_000);

        CollectionAssert.AreEqual(new[] { (0, 1), (1, 2), (2, 3) }, pagedCalls);
        CollectionAssert.AreEqual(new[] { (0, 2), (2, 4) }, unpagedCalls, "two parts of 2,900 fit in 8,000, three don't");
        CollectionAssert.AreEqual(new[] { (1, 2), (2, 3) }, goingOn, "from the part where it was left");
    }

    [TestMethod]
    public void TheContext_LimitsWhatACallTranslates_AndTooSmallIsSaid()
    {
        Assert.AreEqual(DocumentTranslationRules.MaxCallCharacters, DocumentTranslationRules.CallCharacters(16_384, 2_000));
        var small = DocumentTranslationRules.CallCharacters(4_096, 2_000);
        Assert.IsLessThan(DocumentTranslationRules.MaxCallCharacters, small);
        Assert.IsNull(DocumentTranslationRules.ContextTooSmall(4_096, small), "4,096 tokens still translate a part");
        Assert.IsNotNull(DocumentTranslationRules.ContextTooSmall(2_048, DocumentTranslationRules.CallCharacters(2_048, 2_000)));

        // A call and its translation fit in the context.
        var tokens = small / 4.0 + DocumentTranslationRules.OutputTokens(small) + 2_000 / 4.0 + SummaryBatches.RequestOverheadTokens;
        Assert.IsLessThanOrEqualTo(4_096, tokens);
    }

    [TestMethod]
    public void TheTranslation_HasAHeadingBeforeEachPage_AndSaysTheScannedOnes()
    {
        var parts = DocumentTranslationRules.Parts($"One{Page}{DocumentTextRules.PendingMark}{Page}Three", paged: true);

        var text = DocumentTranslationRules.PartText(parts, (0, 1), "Uno")
            + DocumentTranslationRules.PartText(parts, (1, 2), null)
            + DocumentTranslationRules.PartText(parts, (2, 3), "Tres");

        Assert.AreEqual($"#### p. 1\n\nUno\n\n#### p. 2\n\n{DocumentTranslationRules.MissingPageText(2)}\n\n#### p. 3\n\nTres", text);
    }

    [TestMethod]
    public void APartOfALongPage_GoesOnInTheSamePage_AndWithoutPagesThereAreNoHeadings()
    {
        var parts = new List<TranslationPart> { new(1, "a", true, false), new(1, "b", false, false), new(2, "c", true, false) };
        var unpaged = new List<TranslationPart> { new(0, "a", true, false), new(0, "b", false, false) };

        Assert.AreEqual("#### p. 1\n\n", DocumentTranslationRules.Separator(parts, 0));
        Assert.AreEqual("\n\n", DocumentTranslationRules.Separator(parts, 1));
        Assert.AreEqual("\n\n#### p. 2\n\n", DocumentTranslationRules.Separator(parts, 2));
        Assert.AreEqual("", DocumentTranslationRules.Separator(unpaged, 0));
        Assert.AreEqual("\n\n", DocumentTranslationRules.Separator(unpaged, 1));
    }

    [TestMethod]
    public void ATranslationClearlyShorter_LooksIncomplete()
    {
        var original = new string('x', 1_000);

        Assert.IsTrue(DocumentTranslationRules.LooksIncomplete(original, new string('y', 300)));
        Assert.IsFalse(DocumentTranslationRules.LooksIncomplete(original, new string('y', 800)));
        Assert.IsFalse(DocumentTranslationRules.LooksIncomplete("A short title", "T"), "short parts are not checked");
    }

    [TestMethod]
    public void TheAnswer_LosesItsControlCharacters_SoItAddsNoPages()
    {
        Assert.AreEqual("Uno\nDos", DocumentTranslationRules.CleanAnswer($"  Uno\r\n{Page}Dos\u0001 "));
        Assert.AreEqual("Uno\n\nDos", DocumentTranslationRules.CleanAnswer("<text>\nUno\n\nDos\n</text>"), "the marks of the request, if the model copied them");
    }

    [TestMethod]
    public void TheRequest_EndsWithTheLanguage_AndCitesThePage()
    {
        var request = DocumentTranslationRules.Request("manual.pdf", 12, "The QL boots.", " Spanish ");

        StringAssert.StartsWith(request, "Translate into Spanish the text of this part of the document «manual.pdf» (p. 12), between <text> and </text>.");
        StringAssert.Contains(request, "<text>\nThe QL boots.\n</text>");
        StringAssert.EndsWith(request, "Translate the text between <text> and </text> into Spanish.");
    }

    // 2.1b2: shown as Markdown, the lines of a paragraph are joined; KNote keeps them (a program listing stays line by line).
    [TestMethod]
    public void TheLinesOfATranslation_AreKept_ButNotInsideACodeBlock()
    {
        var translation = "100 REMark AVANCE\n110 DIM P(11)\n\nUn párrafo.\n```\n10 PRINT\n20 GOTO 10\n```\nFin  \nya cortada";

        Assert.AreEqual("100 REMark AVANCE  \n110 DIM P(11)\n\nUn párrafo.\n```\n10 PRINT\n20 GOTO 10\n```\nFin  \nya cortada",
            DocumentTranslationRules.KeepLines(translation));
        var parts = new List<TranslationPart> { new(1, "a\nb", true, false) };
        Assert.AreEqual("#### p. 1\n\nUno  \nDos", DocumentTranslationRules.PartText(parts, (0, 1), "Uno\nDos"));
    }

    [TestMethod]
    public void TheNote_SaysHowFarItWent_AndItsHiddenCommentIsReadBack()
    {
        var translation = new DocumentTranslationDto
        {
            Format = DocumentTranslationDto.CurrentFormat, ResourceId = Guid.NewGuid(), ResourceName = "x_manual.pdf", Language = "Spanish",
            Model = "qwen3.5:9b", Paged = true, PagesCount = 138, Parts = 150, NextPart = 45, NextPage = 41
        };

        var text = DocumentTranslationRules.NoteDescription(translation, "manual.pdf", 12, [7, 8], new DateTime(2026, 11, 15), "#### p. 1\n\nUno");
        var read = DocumentTranslationRules.ReadMarker(text);

        StringAssert.Contains(text, "of the note #12 into **Spanish**");
        StringAssert.Contains(text, "Translated: pages 1–40 of 138. Not finished");
        StringAssert.Contains(text, "not translated: 7–8.");
        StringAssert.Contains(text, "\n\n#### p. 1\n\nUno\n\n<!-- knt-translation ", "the translation, between the header and the comment");
        Assert.AreEqual(translation.ResourceId, read.ResourceId);
        Assert.AreEqual(45, read.NextPart);
        Assert.AreEqual(41, read.NextPage);
        Assert.IsFalse(read.Finished);

        translation.NextPart = 150;
        translation.NextPage = null;
        StringAssert.Contains(DocumentTranslationRules.NoteDescription(translation, "manual.pdf", 12, [], DateTime.Now, ""), "Translated: all the 138 pages.");
    }

    [TestMethod]
    public void TheTranslationOfTheNote_IsItsTextWithoutHeaderAndComment_WithWhatSomebodyChanged()
    {
        var translation = new DocumentTranslationDto { Format = DocumentTranslationDto.CurrentFormat, ResourceId = Guid.NewGuid(), Language = "Spanish", Parts = 3, NextPart = 1 };
        var text = DocumentTranslationRules.NoteDescription(translation, "manual.pdf", 12, [], DateTime.Now, "#### p. 1\n\nUno\n\n> Una cita del texto");

        var edited = text.Replace("Uno", "Uno, corregido").Replace("\n", "\r\n");

        Assert.AreEqual("#### p. 1\n\nUno, corregido\n\n> Una cita del texto", DocumentTranslationRules.Body(edited));
        Assert.AreEqual("", DocumentTranslationRules.Body(DocumentTranslationRules.NoteDescription(translation, "manual.pdf", 12, [], DateTime.Now, "")));
    }

    [TestMethod]
    public void ANoteWithoutTheComment_OrWithItBroken_CantGoOn()
    {
        Assert.IsNull(DocumentTranslationRules.ReadMarker("A note written by hand."));
        Assert.IsNull(DocumentTranslationRules.ReadMarker("<!-- knt-translation {not json} -->"));
        Assert.IsNull(DocumentTranslationRules.ReadMarker("<!-- knt-translation {\"language\":\"Spanish\"} -->"), "without its document");
        Assert.IsNull(DocumentTranslationRules.ReadMarker($"<!-- knt-translation {{\"resourceId\":\"{Guid.NewGuid()}\",\"parts\":3}} -->"),
            "of 2.1b1, with the translation in a document attached");
    }

    [TestMethod]
    public void TheLanguage_CantCloseTheHiddenComment()
    {
        var translation = new DocumentTranslationDto { Format = DocumentTranslationDto.CurrentFormat, ResourceId = Guid.NewGuid(), Language = "Spanish --> <b>", Parts = 1 };

        var marker = DocumentTranslationRules.Marker(translation);

        Assert.AreEqual(marker.Length - 3, marker.IndexOf("-->", StringComparison.Ordinal), "only its own end");
        Assert.AreEqual("Spanish --> <b>", DocumentTranslationRules.ReadMarker(marker).Language);
    }
}

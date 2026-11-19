using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// The document of a period summary (KNote Atlas, stage 2, 2.6, D13, D15-D17, R1-R6): the parts of «What happened» (by folder up to its
/// second level, at most 8, the words shared by their notes, a long part cut by date) and the words of each part; the citations of notes
/// not read, without a link and counted; the relations with the notes in the order they are cited; the index by concepts (the
/// combinations of the attributes chosen, PeriodConceptRulesTests); the statistics; and the document in the order of the chapter 35, with what the AI wrote and what KNote calculates.
/// </summary>
[TestClass]
public class PeriodSummaryDocumentTests
{
    private static PeriodNoteDto Note(int number, string folder, int day, string tags = null, string text = "Text.", double score = 1) => new()
    {
        NoteId = Guid.NewGuid(), NoteNumber = number, Topic = $"Note {number}", FolderPath = folder, ModificationDateTime = new DateTime(2026, 11, day),
        Tags = tags, Text = text, Score = score
    };

    [TestMethod]
    public void TheWords_AreSharedBetweenTheParts()
    {
        Assert.AreEqual((200, 300, 1500), PeriodSummarySections.Words(2000));
        Assert.AreEqual((400, 800, 8800), PeriodSummarySections.Words(10000), "the executive summary and what is open, at most");
        Assert.AreEqual((100, 150, 750), PeriodSummarySections.Words(1000));
        var (executive, pending, narrative) = PeriodSummarySections.Words(100);
        Assert.IsLessThanOrEqualTo(100, executive + pending + narrative, "a very short one keeps its shares");
    }

    [TestMethod]
    public void TheParts_AreByFolderUpToItsSecondLevel_WithTheWordsOfTheirNotes()
    {
        var notes = new[]
        {
            Note(1, @"Home\Work\KNote", 10), Note(2, @"Home\Work", 11), Note(3, @"Home\Work\Server", 12), Note(4, @"Home\Garden", 13)
        };

        var sections = PeriodSummarySections.Plan(notes, 2000);

        CollectionAssert.AreEqual(new[] { @"Home\Garden", @"Home\Work" }, sections.Select(s => s.Title).ToList());
        CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, sections[1].Notes.Select(n => n.NoteNumber).ToList());
        Assert.AreEqual((375, 1125), (sections[0].Words, sections[1].Words), "1,500 words of «What happened» by their notes");
        Assert.AreEqual("Home", PeriodSummarySections.GroupOf("Home"));
    }

    [TestMethod]
    public void MoreThan8Groups_TheSmallestGoTogether_AndALongPartIsCutByDate()
    {
        var notes = Enumerable.Range(1, 9).Select(i => Note(i, $@"Home\F{i}", 10)).Concat(new[] { Note(10, @"Home\F1", 12), Note(11, @"Home\F1", 11) }).ToList();

        var sections = PeriodSummarySections.Plan(notes, 2000);
        var cut = PeriodSummarySections.Plan(Enumerable.Range(1, 6).Select(i => Note(i, "Home", 16 - i)), 10000);

        Assert.HasCount(PeriodSummarySections.MaxSections, sections);
        Assert.AreEqual(@"Home\F1", sections[0].Title, "the largest group is kept");
        Assert.AreEqual(PeriodSummarySections.OtherFolders, sections[^1].Title);
        Assert.HasCount(2, sections[^1].Notes);
        CollectionAssert.AreEqual(new[] { "Home (part 1 of 4)", "Home (part 2 of 4)", "Home (part 3 of 4)", "Home (part 4 of 4)" }, cut.Select(s => s.Title).ToList());
        CollectionAssert.AreEqual(new[] { 6, 5, 4, 3, 2, 1 }, cut.SelectMany(s => s.Notes).Select(n => n.NoteNumber).ToList(), "by date");
        Assert.IsTrue(cut.All(s => s.Words <= PeriodSummarySections.MaxSectionWords));
    }

    [TestMethod]
    public void ACitationOfANoteNotRead_IsLeftWithoutALink_AndCounted()
    {
        var check = SummaryCitationRules.Check("Chosen (#7, #77). Then #912 and #77 again; C#5 and #urgent are not citations.", new HashSet<int> { 7 });

        CollectionAssert.AreEqual(new[] { 77, 912 }, check.NotRecognized);
        Assert.AreEqual("Chosen (#7, #⁠77). Then #⁠912 and #⁠77 again; C#5 and #urgent are not citations.", check.Text);
        Assert.AreEqual(1, NoteCitationLinker.CitedNumbers(check.Text).Count, "only #7 is a link");
        StringAssert.Contains(NoteCitationLinker.Link(check.Text, n => $"/n/{n}"), "href=\"/n/7\"");
        Assert.AreEqual("2 citations not recognized: #⁠77, #⁠912", SummaryCitationRules.NotRecognizedText(check.NotRecognized));
        Assert.IsNull(SummaryCitationRules.NotRecognizedText(new List<int>()));
    }

    [TestMethod]
    public void TheSources_AreInTheOrderTheyAreCited_ThenTheOthers_WithTheirScore()
    {
        var summary = Summary(Note(1, @"Home\A", 10, score: 0.5), Note(2, @"Home\A", 11, score: 2), Note(3, @"Home\B", 12));
        summary.Executive = "The key is #3 and #2.";
        summary.Sections[0].Text = "Then #2 and #99.";

        var sources = PeriodSummaryDocument.Sources(summary);

        Assert.AreEqual("3:1, 2:2, 1:3", string.Join(", ", sources.Select(s => $"{s.NoteNumber}:{s.Order}")));
        Assert.AreEqual(2, sources.Single(s => s.NoteNumber == 2).Weight);
    }

    [TestMethod]
    public void TheDocument_HasItsPartsInOrder_WithWhatKNoteCalculates()
    {
        var work = Note(12, @"Home\Work", 10, tags: "server", text: "We chose the R750.", score: 3);
        work.TasksStarted = 1;
        work.TasksOpen = 1;
        work.EstimatedTime = 2.5;
        work.Tasks = new List<string> { "Buy the server" };
        work.Attributes = new Dictionary<string, string> { ["Status"] = "In progress", ["Billable"] = "Yes" };
        var garden = Note(15, @"Home\Garden", 11, text: "Pruned | the roses.");
        garden.Attributes = new Dictionary<string, string> { ["Billable"] = "Yes" };
        var summary = Summary(garden, work);
        summary.Prepared.ConceptAttributes = new List<string> { "Status", "Billable" };
        summary.Prepared.Selection = new NoteSelectionResultDto
        {
            KNoteNotes = 4,
            Notes =
            {
                new NoteSelectionDto { NoteId = work.NoteId, NoteNumber = 12, Topic = "Note 12", Decision = EnumSelectionDecision.In, Reason = "In: 2 tasks" },
                new NoteSelectionDto { NoteId = garden.NoteId, NoteNumber = 15, Topic = "Note 15", Decision = EnumSelectionDecision.In, Reason = "In: long" },
                new NoteSelectionDto { NoteId = Guid.NewGuid(), NoteNumber = 20, Topic = "Empty | note", Decision = EnumSelectionDecision.Out, Reason = "Out: too short" }
            }
        };
        summary.Executive = "## Executive summary\n\nThe server was chosen (#12).";
        summary.Sections = new List<PeriodSummarySectionDto>
        {
            new() { Title = @"Home\Garden", Text = "Roses pruned (#15).", NoteNumbers = { 15 } },
            new() { Title = @"Home\Work", Text = "The R750 (#12, #99).\n## Purchase\nDone.", NoteNumbers = { 12 } }
        };
        summary.Pending = "";
        summary.Calls = 7;
        summary.Seconds = 200;

        var text = PeriodSummaryDocument.Build(summary, null, new DateTime(2026, 11, 17));

        var headings = text.Split('\n').Where(l => l.StartsWith("## ") || l.StartsWith("### ")).ToList();
        CollectionAssert.AreEqual(new[] { "## Executive summary", "## Index", "## What happened", "### Home\\Garden", "### Home\\Work",
            "## Open and at risk", "## Statistics", "## Notes included", "## Notes left out" }, headings, "the title of the AI is not kept; its heading, at level 4");
        StringAssert.Contains(text, "> 2 notes read, 1 note left out (and 4 of KNote's own) · qwen3.5:9b · 2026-11-17 · Spanish\n>\n" +
            "> 7 calls to the model, 3 min · 1 citation not recognized: #⁠99");
        StringAssert.Contains(text, "The R750 (#12, #⁠99).\n#### Purchase\nDone.");
        StringAssert.Contains(text, "## Open and at risk\n\n" + PeriodSummaryDocument.NothingWritten);
        StringAssert.Contains(text, "- Home\\Work: 1 note");
        StringAssert.Contains(text, " · concepts by **Status**, **Billable**\n>\n> 2 notes read");
        StringAssert.Contains(text, "**By concept**\n\n| Status | Notes | Tasks | Started / ended / open | Notes cited |\n|---|---:|---:|---|---|\n" +
            "| In progress | 1 | 1 | 1 / 0 / 1 | #12 |\n\n1 note without a value of Status.\n\n" +
            "| Billable | Notes | Tasks | Started / ended / open | Notes cited |\n|---|---:|---:|---|---|\n" +
            "| Yes | 2 | 1 | 1 / 0 / 1 | #12, #15 |\n\n## What happened", "a table per attribute");
        Assert.DoesNotContain("server", text.Split("## What happened")[0].Split("**By concept**")[1], "the tags are not concepts");
        StringAssert.Contains(PeriodSummaryRules.PieceText(work), "Modified: 2026-11-10 · Tags: server\nAttributes: Status = In progress · Billable = Yes\n", "the AI reads them");

        summary.Prepared.ConceptAttributes = new List<string>();
        StringAssert.Contains(PeriodSummaryDocument.Build(summary, null, new DateTime(2026, 11, 17)), "**By concept**\n\n" + PeriodConceptRules.NoneChosen + "\n\n");
        StringAssert.Contains(text, "| Home\\Work | 1 | 1 | 0 | 1 | 2.5 |  |");
        StringAssert.Contains(text, "| **Total** | 2 | 1 | 0 | 1 | 2.5 |  |");
        StringAssert.Contains(text, "| #15 | Note 15 | Home\\Garden | 2026-11-11 | In: long |\n| #12 | Note 12 | Home\\Work | 2026-11-10 | In: 2 tasks |");
        StringAssert.Contains(text, "| #20 | Empty \\| note | Out: too short |");
        StringAssert.EndsWith(text, "And 4 notes of KNote's own, which are never summarized.");

        var original = PeriodSummaryDocument.Original(summary);
        StringAssert.StartsWith(original, "## Executive summary\n\n## Executive summary\n\nThe server was chosen (#12).", "as the AI wrote it");
        StringAssert.Contains(original, "### Home\\Work\n\nThe R750 (#12, #99).\n## Purchase\nDone.");
    }

    // A summary of these notes, with a part of the first folder.
    private static PeriodSummarySaveDto Summary(params PeriodNoteDto[] notes) => new()
    {
        Request = new PeriodSummaryRequestDto { Selection = new NoteSelectionRequestDto { From = new DateTime(2026, 11, 10), To = new DateTime(2026, 11, 16) } },
        Model = "qwen3.5:9b", Language = "Spanish",
        Prepared = new PeriodSummaryPreparedDto { Notes = notes.ToList(), Selection = new NoteSelectionResultDto() },
        Sections = { new PeriodSummarySectionDto { Title = "Home", Text = "", NoteNumbers = notes.Select(n => n.NoteNumber).ToList() } }
    };
}

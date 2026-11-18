using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// The summary of a period (KNote Atlas, stage 2, 2.5, D11, D14, P2, P3): the notes as pieces with their label #123, by folder and by
/// date, a long one in several pieces; what is read of each note (its data, text, tasks with activity and resources); the activity of
/// a task in the period; the subject by default; the requests, with the language last; the header of the note; and the length kept
/// between its limits.
/// </summary>
[TestClass]
public class PeriodSummaryRulesTests
{
    private static PeriodNoteDto Note(int number, string folder, DateTime modified, string text = "Text.") => new()
    {
        NoteId = Guid.NewGuid(), NoteNumber = number, Topic = $"Note {number}", FolderPath = folder, ModificationDateTime = modified, Text = text
    };

    [TestMethod]
    public void ThePieces_AreByFolderAndDate_WithTheirLabel_ALongNoteInSeveral()
    {
        var notes = new[]
        {
            Note(3, @"Home\Work", new DateTime(2026, 11, 12)),
            Note(1, @"Home\Work", new DateTime(2026, 11, 10)),
            Note(2, @"Home\Garden", new DateTime(2026, 11, 14), string.Join(" ", Enumerable.Range(1, 2000).Select(i => $"word{i}")))
        };

        var pieces = PeriodSummaryRules.Pieces(notes);

        var labels = pieces.Select(p => p.Label).ToList();
        var long2 = labels.Count(l => l == "#2");
        Assert.IsGreaterThan(1, long2, "a long note in several pieces");
        CollectionAssert.AreEqual(Enumerable.Repeat("#2", long2).Concat(new[] { "#1", "#3" }).ToList(), labels, "Garden before Work; by date in Work");
        Assert.IsTrue(pieces.All(p => p.Text.Length <= SummaryPieces.MaxPieceLength));
    }

    [TestMethod]
    public void WhatIsReadOfANote_ItsData_ItsText_ItsTasksAndResources()
    {
        var note = Note(12, @"Home\Projects", new DateTime(2026, 11, 10), "We chose the new server.");
        note.Tags = "work, hardware";
        note.Tasks.Add("started 2026-11-09, ended 2026-11-11 — Buy it");
        note.Resources.Add("offer.pdf: The offer of Dell for the R750.");

        var text = PeriodSummaryRules.PieceText(note);

        Assert.AreEqual("Topic: Note 12\nFolder: Home\\Projects · Modified: 2026-11-10 · Tags: work, hardware\n\nWe chose the new server.\n\n" +
            "Tasks with activity in the period:\n- started 2026-11-09, ended 2026-11-11 — Buy it\n\nResources:\n- offer.pdf: The offer of Dell for the R750.", text);
    }

    [TestMethod]
    public void ATask_HasActivity_WhenItStartsOrEndsInThePeriod()
    {
        var from = new DateTime(2026, 11, 10);
        var to = new DateTime(2026, 11, 17);
        NoteTaskDto Task(DateTime? start, DateTime? end, DateTime? created = null) =>
            new() { StartDate = start, EndDate = end, CreationDateTime = created ?? new DateTime(2026, 1, 1), Description = "Do it" };

        Assert.IsTrue(PeriodSummaryRules.HasActivity(Task(new DateTime(2026, 11, 12), null), from, to));
        Assert.IsTrue(PeriodSummaryRules.HasActivity(Task(new DateTime(2026, 10, 1), new DateTime(2026, 11, 16, 20, 0, 0)), from, to));
        Assert.IsTrue(PeriodSummaryRules.HasActivity(Task(null, null, new DateTime(2026, 11, 11)), from, to), "without StartDate, its creation");
        Assert.IsFalse(PeriodSummaryRules.HasActivity(Task(new DateTime(2026, 10, 1), null), from, to), "it goes on, without activity");
        Assert.IsFalse(PeriodSummaryRules.HasActivity(Task(new DateTime(2026, 10, 1), to), from, to), "to is not included");
        StringAssert.StartsWith(PeriodSummaryRules.TaskText(Task(new DateTime(2026, 11, 12), null)), "started 2026-11-12, open — Do it");
    }

    [TestMethod]
    public void TheSubject_ByDefault_HasThePeriodAndTheSubjectOfTheFilters()
    {
        var week = new NoteSelectionRequestDto { From = new DateTime(2026, 11, 10), To = new DateTime(2026, 11, 16) };
        var topic = new NoteSelectionRequestDto { From = NoteSelectionRequestDto.Beginning, To = new DateTime(2026, 11, 16), MeaningText = "the QL repair" };

        Assert.AreEqual("KNote summary: 10/11/2026 - 16/11/2026", PeriodSummaryRules.DefaultSubject(week));
        Assert.AreEqual("KNote summary: the QL repair, until 16/11/2026", PeriodSummaryRules.DefaultSubject(topic));
        Assert.AreEqual("until 2026-11-16", PeriodSummaryRules.PeriodText(topic.From, topic.To));
    }

    [TestMethod]
    public void TheRequests_CiteTheNotes_AndTheLanguageGoesLast()
    {
        var pieces = PeriodSummaryRules.Pieces(new[] { Note(7, "Home", new DateTime(2026, 11, 10)) });

        var from = new DateTime(2026, 11, 10);
        var to = new DateTime(2026, 11, 16);
        var notes = new[] { "- A note #7" };

        var batch = PeriodSummaryRules.BatchRequest("Week 46", pieces);
        var section = PeriodSummaryRules.SectionRequest("Week 46", @"Home\Work", from, to, 3, notes, 600, " Spanish ");
        var executive = PeriodSummaryRules.ExecutiveRequest("Week 46", from, to, 1, notes, 200, null);
        var pending = PeriodSummaryRules.PendingRequest("Week 46", from, to, 12, notes, 300, "English");

        StringAssert.Contains(batch, "[#7]\nTopic: Note 7");
        StringAssert.Contains(batch, "as #123");
        StringAssert.Contains(section, "the 3 notes of «Home\\Work» of the period 2026-11-10 – 2026-11-16");
        StringAssert.Contains(section, "about 600 words");
        StringAssert.Contains(section, "- A note #7");
        StringAssert.EndsWith(section, "Write it in Spanish.");
        StringAssert.Contains(executive, "the 1 note of the period");
        StringAssert.EndsWith(executive, "Write it in the language of the notes.");
        StringAssert.Contains(pending, "at most 300 words");
        StringAssert.EndsWith(pending, "Write it in English.");
        Assert.AreEqual(PeriodSummaryRules.MinWords, PeriodSummaryRules.ClampWords(10));
        Assert.AreEqual(PeriodSummaryRules.DefaultWords, PeriodSummaryRules.ClampWords(0));
        Assert.AreEqual(10000, PeriodSummaryRules.ClampWords(50000));
        Assert.AreEqual("KNote summary- 10-11-2026 - 16-11-2026 (original).md", PeriodSummaryRules.OriginalFileName("KNote summary: 10/11/2026 - 16/11/2026"));
    }

    [TestMethod]
    public void APart_IsKeptWithoutItsTitle_AndWithItsHeadingsAtLevel4()
    {
        var text = "## Executive summary\n\nThe server was chosen (#7).\n\n# Details\n### More\n```\n# a comment in code\n```\n##### Deep";

        Assert.AreEqual("The server was chosen (#7).\n\n#### Details\n#### More\n```\n# a comment in code\n```\n##### Deep", PeriodSummaryRules.CleanPart(text));
        Assert.AreEqual("#hashtag stays", PeriodSummaryRules.CleanPart("#hashtag stays"));
    }

    [TestMethod]
    public void TheTasksOfANote_AreCountedForTheStatistics()
    {
        var from = new DateTime(2026, 11, 10);
        var to = new DateTime(2026, 11, 17);
        var note = Note(1, "Home", from);
        var tasks = new[]
        {
            new NoteTaskDto { StartDate = new DateTime(2026, 11, 11), EndDate = new DateTime(2026, 11, 12), EstimatedTime = 2, SpentTime = 3.5 },
            new NoteTaskDto { StartDate = new DateTime(2026, 11, 13), EstimatedTime = 1 },
            new NoteTaskDto { StartDate = new DateTime(2026, 10, 1), EndDate = new DateTime(2026, 11, 14) },
            new NoteTaskDto { StartDate = new DateTime(2026, 11, 15), EndDate = new DateTime(2026, 11, 20) }
        };

        PeriodSummaryRules.SetTaskCounts(note, tasks, from, to);

        Assert.AreEqual((3, 2, 2), (note.TasksStarted, note.TasksEnded, note.TasksOpen), "open: without an end, or ended after the period");
        Assert.AreEqual(3.0, note.EstimatedTime);
        Assert.AreEqual(3.5, note.SpentTime);
        PeriodSummaryRules.SetTaskCounts(note, new[] { new NoteTaskDto { StartDate = from } }, from, to);
        Assert.IsNull(note.EstimatedTime, "no task has it");
    }
}

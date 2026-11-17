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

        var batch = PeriodSummaryRules.BatchRequest("Week 46", pieces);
        var final = PeriodSummaryRules.FinalRequest("Week 46", new DateTime(2026, 11, 10), new DateTime(2026, 11, 16), 1, new[] { "- A note #7" }, 50000, " Spanish ");

        StringAssert.Contains(batch, "[#7]\nTopic: Note 7");
        StringAssert.Contains(batch, "as #123");
        StringAssert.Contains(final, $"about {PeriodSummaryRules.MaxWords} words");
        StringAssert.EndsWith(final, "Write the summary in Spanish.");
        Assert.AreEqual(PeriodSummaryRules.MinWords, PeriodSummaryRules.ClampWords(10));
        Assert.AreEqual(PeriodSummaryRules.DefaultWords, PeriodSummaryRules.ClampWords(0));
    }

    [TestMethod]
    public void TheNote_HasAHeaderWithWhatItCameFrom()
    {
        var summary = new PeriodSummarySaveDto
        {
            Request = new PeriodSummaryRequestDto { Selection = new NoteSelectionRequestDto { From = new DateTime(2026, 11, 10), To = new DateTime(2026, 11, 16), Keywords = "QL" } },
            Model = "qwen3.5:9b", Language = "Spanish", Summary = " The summary. ", NotesRead = 12, NotesLeftOut = 3, KNoteNotes = 5
        };

        var text = PeriodSummaryRules.NoteDescription(summary, @"Home\Projects", new DateTime(2026, 11, 17));

        Assert.AreEqual("> Summary of the period **2026-11-10 – 2026-11-16** · folder **Home\\Projects** and its subfolders · keywords «QL»\n>\n" +
            "> 12 notes read, 3 notes left out (and 5 of KNote's own) · qwen3.5:9b · 2026-11-17 · Spanish\n\nThe summary.", text);
        Assert.AreEqual("KNote summary- 10-11-2026 - 16-11-2026 (original).md", PeriodSummaryRules.OriginalFileName("KNote summary: 10/11/2026 - 16/11/2026"));
    }
}

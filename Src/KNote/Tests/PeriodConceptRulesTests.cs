using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// The index «By concept» of a period summary (PeriodConceptRules): only the attributes that group can be chosen, each one with its note
/// type, and the ones of the same name are one table; their values as they are grouped (yes/no, the values of a TagsValue sorted); a
/// table per attribute, each note in one row of it, with its notes and its tasks; the notes without a value apart; and beyond MaxRows,
/// the other values together.
/// </summary>
[TestClass]
public class PeriodConceptRulesTests
{
    private static readonly Guid StatusId = Guid.NewGuid();
    private static readonly Guid StatusOfTasksId = Guid.NewGuid();
    private static readonly Guid PlanningId = Guid.NewGuid();
    private static readonly Guid DueId = Guid.NewGuid();
    private static readonly Guid BillableId = Guid.NewGuid();
    private static readonly Guid LabelsId = Guid.NewGuid();
    private static readonly Guid TaskTypeId = Guid.NewGuid();

    private static readonly List<KAttributeInfoDto> Attributes = new()
    {
        new() { KAttributeId = PlanningId, Name = "Planning", KAttributeDataType = EnumKAttributeDataType.TabulatedValue, Order = 2 },
        new() { KAttributeId = StatusId, Name = "Status", KAttributeDataType = EnumKAttributeDataType.TabulatedValue, Order = 1 },
        // The same name in a note type: a choice of its own, the same column.
        new() { KAttributeId = StatusOfTasksId, Name = "status ", KAttributeDataType = EnumKAttributeDataType.String, Order = 5, NoteTypeId = TaskTypeId,
            NoteTypeDto = new NoteTypeDto { NoteTypeId = TaskTypeId, Name = "Task" } },
        new() { KAttributeId = DueId, Name = "Due date", KAttributeDataType = EnumKAttributeDataType.DateTime, Order = 3 },
        new() { KAttributeId = BillableId, Name = "Billable", KAttributeDataType = EnumKAttributeDataType.Bool, Order = 4 },
        new() { KAttributeId = LabelsId, Name = "Labels", KAttributeDataType = EnumKAttributeDataType.TagsValue, Order = 6 },
        new() { KAttributeId = Guid.NewGuid(), Name = "Old", KAttributeDataType = EnumKAttributeDataType.String, Order = 0, Disabled = true },
        new() { KAttributeId = Guid.NewGuid(), Name = "Notes", KAttributeDataType = EnumKAttributeDataType.TextArea, Order = 7 }
    };

    private static PeriodNoteDto Note(int number, params (string Name, string Value)[] values) => new()
    {
        NoteId = Guid.NewGuid(), NoteNumber = number, Topic = $"Note {number}",
        Attributes = values.ToDictionary(v => v.Name, v => v.Value)
    };

    [TestMethod]
    public void OnlyTheAttributesThatGroup_CanBeChosen_WithTheirNoteType_InTheirOrder()
    {
        var choices = PeriodConceptRules.Choices(Attributes);

        CollectionAssert.AreEqual(new[] { "- · Status", "- · Planning", "- · Billable", "- · Labels", "Task · status" }, choices.Select(c => c.Label).ToList(),
            "no dates, long texts or disabled ones; the ones of every note first");
        Assert.AreEqual("Task · status", choices[^1].ToString(), "as the desktop list shows it");
        CollectionAssert.AreEqual(new[] { StatusId, LabelsId }, PeriodConceptRules.Chosen(new[] { LabelsId, DueId, StatusId, Guid.NewGuid() }, choices),
            "in the order of the choices; the ones that cannot be chosen are left out");
        Assert.IsEmpty(PeriodConceptRules.Chosen(null, choices));
    }

    [TestMethod]
    public void TheColumns_AreTheNames_OnceEach()
    {
        CollectionAssert.AreEqual(new[] { "Status", "Billable" }, PeriodConceptRules.Columns(Attributes, new[] { StatusOfTasksId, BillableId, StatusId, DueId }),
            "two of the same name, one column with the name of the first");
        CollectionAssert.AreEqual(new[] { "status" }, PeriodConceptRules.Columns(Attributes, new[] { StatusOfTasksId }));
    }

    [TestMethod]
    public void TheValues_AreAsTheyAreGrouped()
    {
        Assert.AreEqual("Yes", PeriodConceptRules.Value(EnumKAttributeDataType.Bool, "True"));
        Assert.AreEqual("No", PeriodConceptRules.Value(EnumKAttributeDataType.Bool, "false"));
        Assert.AreEqual("alpha, Beta", PeriodConceptRules.Value(EnumKAttributeDataType.TagsValue, "Beta, alpha, , beta"));
        Assert.AreEqual("Two lines", PeriodConceptRules.Value(EnumKAttributeDataType.String, " Two\r\nlines ").Replace("  ", " "));
        Assert.IsNull(PeriodConceptRules.Value(EnumKAttributeDataType.String, "  "));
        Assert.IsNull(PeriodConceptRules.Value(EnumKAttributeDataType.TagsValue, " , "));
    }

    [TestMethod]
    public void TheValuesOfEachNote_AreByName_AndInTheOrderChosen()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        var values = new[]
        {
            new NoteAttributeValueDto { NoteId = first, KAttributeId = BillableId, Value = "True" },
            new NoteAttributeValueDto { NoteId = first, KAttributeId = StatusId, Value = "Open" },
            new NoteAttributeValueDto { NoteId = first, KAttributeId = DueId, Value = "2026-11-20" },
            new NoteAttributeValueDto { NoteId = second, KAttributeId = StatusOfTasksId, Value = "Done" },
            new NoteAttributeValueDto { NoteId = second, KAttributeId = BillableId, Value = " " }
        };

        var byNote = PeriodConceptRules.ValuesByNote(values, Attributes, new[] { BillableId, StatusOfTasksId, StatusId });
        var onlyOfEveryNote = PeriodConceptRules.ValuesByNote(values, Attributes, new[] { StatusId });

        Assert.AreEqual("Status=Open, Billable=Yes", string.Join(", ", byNote[first].Select(v => $"{v.Key}={v.Value}")));
        Assert.AreEqual("Status=Done", string.Join(", ", byNote[second].Select(v => $"{v.Key}={v.Value}")), "the attribute of the same name of its type");
        Assert.IsFalse(onlyOfEveryNote.ContainsKey(second), "the one of its type was not chosen");
    }

    [TestMethod]
    public void EachAttribute_HasItsTable_WithARowPerValue_ItsTasks_AndTheNotesWithoutAValueApart()
    {
        var notes = new[]
        {
            Note(1, ("Status", "Open"), ("Planning", "Q4")),
            Note(2, ("Status", "Open"), ("Planning", "Q1")),
            Note(3, ("Status", "open"), ("Planning", "q4")),
            Note(4, ("Status", "Done")),
            Note(5),
            Note(6, ("Labels", "a, b"))
        };
        notes[0].Tasks = new List<string> { "a", "b" };
        notes[0].TasksStarted = 2;
        notes[1].Tasks = new List<string> { "c" };
        notes[1].TasksEnded = 1;
        notes[1].TasksOpen = 1;

        var tables = PeriodConceptRules.Tables(notes, new[] { "Status", "Planning" });

        CollectionAssert.AreEqual(new[] { "Status", "Planning" }, tables.Select(t => t.Attribute).ToList(), "a table per attribute, in their order");
        var status = tables[0];
        Assert.AreEqual("Open 1,2,3 | Done 4", string.Join(" | ", status.Rows.Select(r => $"{r.Value} {string.Join(",", r.NoteNumbers)}")),
            "without case, written as the first one");
        Assert.AreEqual((3, 2, 1, 1), (status.Rows[0].Tasks, status.Rows[0].TasksStarted, status.Rows[0].TasksEnded, status.Rows[0].TasksOpen));
        Assert.AreEqual(2, status.NotesWithoutValue, "#5 and #6, which has only an attribute not chosen");
        var planning = tables[1];
        Assert.AreEqual("Q4 1,3 | Q1 2", string.Join(" | ", planning.Rows.Select(r => $"{r.Value} {string.Join(",", r.NoteNumbers)}")));
        Assert.AreEqual(3, planning.NotesWithoutValue, "#4 has a status but no planning");
        Assert.IsNull(status.Others);
        Assert.IsEmpty(PeriodConceptRules.Tables(notes, new List<string>()));
    }

    [TestMethod]
    public void BeyondTheMaximum_TheOtherValues_GoTogether()
    {
        var notes = Enumerable.Range(1, PeriodConceptRules.MaxRows + 3).Select(i => Note(i, ("Status", $"S{i:00}"))).ToList();
        notes.Add(Note(100, ("Status", "S01")));

        var table = PeriodConceptRules.Table(notes, "Status");

        Assert.HasCount(PeriodConceptRules.MaxRows, table.Rows);
        CollectionAssert.AreEqual(new[] { 1, 100 }, table.Rows[0].NoteNumbers, "the value with more notes first");
        Assert.AreEqual(3, table.OtherValues);
        CollectionAssert.AreEqual(new[] { 31, 32, 33 }, table.Others.NoteNumbers);
    }
}

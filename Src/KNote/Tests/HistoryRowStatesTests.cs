using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// The states of the rows of the lists of the history (Model/Core/HistoryRowStates): «Repository changes» (the note there, in the
/// deleted notes until a day, gone, or the structure only noted) and the «History» tab of a note (restorable, with later changes,
/// the note now, or not restorable); their tones, texts and legend.
/// </summary>
[TestClass]
public class HistoryRowStatesTests
{
    private static readonly DateTime Now = new(2026, 11, 25, 12, 0, 0);

    [TestMethod]
    public void ForChange_ByTheNoteAsItIsNow()
    {
        var note = new KLogDto { KLogId = Guid.NewGuid(), EntityName = HistoryEntities.Note, ActionType = "Deleted" };
        var folder = new KLogDto { KLogId = Guid.NewGuid(), EntityName = HistoryEntities.Folder, ActionType = "Updated" };
        var until = Now.Date.AddDays(30);

        Assert.AreEqual(EnumHistoryRowState.None, HistoryRowStates.ForChange(note, noteExists: true, null, canRecover: true));
        Assert.AreEqual(EnumHistoryRowState.Recoverable, HistoryRowStates.ForChange(note, false, until, canRecover: true));
        Assert.AreEqual(EnumHistoryRowState.RecoverableByStaff, HistoryRowStates.ForChange(note, false, until, canRecover: false));
        Assert.AreEqual(EnumHistoryRowState.NotRecoverable, HistoryRowStates.ForChange(note, false, null, canRecover: true));
        Assert.AreEqual(EnumHistoryRowState.OnlyNoted, HistoryRowStates.ForChange(folder, false, null, canRecover: true));
        Assert.AreEqual(EnumHistoryRowState.None, HistoryRowStates.ForChange(null, false, null, true));
    }

    [TestMethod]
    public void ForNoteHistory_RestorableLaterChangesTheNoteNowOrNot()
    {
        var created = Row(1, "Created", copy: false, EnumHistoryFields.None);
        var first = Row(2, "Updated", copy: true, EnumHistoryFields.Topic);
        var emptied = Row(3, "Updated", copy: false, EnumHistoryFields.Description); // its copy emptied by the retention
        var next = Row(4, "Updated", copy: true, EnumHistoryFields.Topic);
        var last = Row(5, "Updated", copy: true, EnumHistoryFields.Description);
        var rows = new[] { last, next, emptied, first, created };
        var restorable = HistoryRules.Restorable(rows);

        Assert.AreEqual(EnumHistoryRowState.None, HistoryRowStates.ForNoteHistory(rows, created, restorable), "the copy of the next row");
        Assert.AreEqual(EnumHistoryRowState.LaterChanges, HistoryRowStates.ForNoteHistory(rows, first, restorable));
        Assert.AreEqual(EnumHistoryRowState.CurrentVersion, HistoryRowStates.ForNoteHistory(rows, last, restorable));
        Assert.AreEqual(EnumHistoryRowState.NotRestorable,
            HistoryRowStates.ForNoteHistory(rows, first, new HashSet<Guid>()), "no copy after it, and not the last change");
    }

    [TestMethod]
    public void ForNoteHistory_ADeletionOrARemoval_CantBeRestored()
    {
        var created = Row(1, "Created", copy: false, EnumHistoryFields.None);
        var deleted = Row(2, "Deleted", copy: true, EnumHistoryFields.None);
        var removed = Row(3, "Removed", copy: false, EnumHistoryFields.None);
        var rows = new[] { removed, deleted, created };
        var all = rows.Select(r => r.KLogId).ToHashSet();

        Assert.AreEqual(EnumHistoryRowState.NotRestorable, HistoryRowStates.ForNoteHistory(rows, deleted, all));
        Assert.AreEqual(EnumHistoryRowState.NotRestorable, HistoryRowStates.ForNoteHistory(rows, removed, all));
    }

    [TestMethod]
    public void Tones_TextsAndTheLegend()
    {
        Assert.AreEqual(EnumHistoryRowTone.Amber, HistoryRowStates.Tone(EnumHistoryRowState.Recoverable));
        Assert.AreEqual(EnumHistoryRowTone.Amber, HistoryRowStates.Tone(EnumHistoryRowState.RecoverableByStaff));
        Assert.AreEqual(EnumHistoryRowTone.Amber, HistoryRowStates.Tone(EnumHistoryRowState.LaterChanges));
        Assert.AreEqual(EnumHistoryRowTone.Orange, HistoryRowStates.Tone(EnumHistoryRowState.NotRecoverable));
        Assert.AreEqual(EnumHistoryRowTone.Orange, HistoryRowStates.Tone(EnumHistoryRowState.NotRestorable));
        Assert.AreEqual(EnumHistoryRowTone.Grey, HistoryRowStates.Tone(EnumHistoryRowState.OnlyNoted));
        Assert.AreEqual(EnumHistoryRowTone.Grey, HistoryRowStates.Tone(EnumHistoryRowState.CurrentVersion));
        Assert.AreEqual(EnumHistoryRowTone.None, HistoryRowStates.Tone(EnumHistoryRowState.None));

        var culture = System.Globalization.CultureInfo.GetCultureInfo("es-ES");
        Assert.AreEqual("Recoverable until 4/12/2026", HistoryRowStates.Text(EnumHistoryRowState.Recoverable, new DateTime(2026, 12, 4), culture));
        Assert.AreEqual("Recoverable by Staff and up", HistoryRowStates.Text(EnumHistoryRowState.RecoverableByStaff));
        Assert.AreEqual("", HistoryRowStates.Text(EnumHistoryRowState.None));
        foreach (var state in Enum.GetValues<EnumHistoryRowState>().Where(s => s != EnumHistoryRowState.None))
        {
            Assert.AreNotEqual("", HistoryRowStates.Text(state), state.ToString());
            Assert.AreNotEqual("", HistoryRowStates.Legend(state), state.ToString());
        }

        CollectionAssert.AreEqual(new[] { EnumHistoryRowState.Recoverable, EnumHistoryRowState.OnlyNoted },
            HistoryRowStates.InLegend(new[]
            {
                EnumHistoryRowState.OnlyNoted, EnumHistoryRowState.None, EnumHistoryRowState.Recoverable, EnumHistoryRowState.OnlyNoted
            }), "once each, in order, without None");
        Assert.AreEqual(0, HistoryRowStates.InLegend(null).Count);
    }

    private static KLogDto Row(int minute, string action, bool copy, EnumHistoryFields fields) => new()
    {
        KLogId = Guid.NewGuid(), EntityName = HistoryEntities.Note, ActionType = action, Fields = fields, PreviousContentSize = copy ? 10 : null,
        RegistryDateTime = Now.AddMinutes(minute), LastDateTime = Now.AddMinutes(minute).AddSeconds(20)
    };
}

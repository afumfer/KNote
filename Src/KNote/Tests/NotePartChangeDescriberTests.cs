using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// NotePartChangeDescriber (the history of the notes, H.2a): what changed in a task, a resource, an alarm or a relation of a
/// note, how two changes of the same session add up, and their lines in the row of the note.
/// </summary>
[TestClass]
public class NotePartChangeDescriberTests
{
    private static readonly Guid Armando = Guid.NewGuid();
    private static readonly Guid Marta = Guid.NewGuid();
    private static readonly Guid Restoration = Guid.NewGuid();

    private static HistoryNames Names()
    {
        var names = new HistoryNames();
        names.Users[Armando] = "Armando";
        names.Users[Marta] = "Marta";
        names.TraceNoteTypes[Restoration] = "Restoration";
        return names;
    }

    private static NoteTaskDto Task() => new()
    {
        NoteTaskId = Guid.NewGuid(), NoteId = Guid.NewGuid(), UserId = Armando, Description = "Clean the keyboard\r\nWith alcohol.",
        Priority = 1, StartDate = new DateTime(2026, 11, 20)
    };

    private static string Line(HistoryPartChangeDto change, EnumHistoryFields part) =>
        NoteChangeDescriber.Render(NotePartChangeDescriber.Details(part, change));

    [TestMethod]
    public void ATask_AddedResolvedReopenedAndRemoved()
    {
        var before = Task();
        var resolved = Task();
        resolved.NoteTaskId = before.NoteTaskId;
        resolved.EndDate = new DateTime(2026, 11, 25, 10, 30, 0);

        Assert.AreEqual("Task added: «Clean the keyboard»", Line(NotePartChangeDescriber.Task(null, before, Names()), EnumHistoryFields.Tasks));
        Assert.AreEqual("Task resolved: «Clean the keyboard»", Line(NotePartChangeDescriber.Task(before, resolved, Names()), EnumHistoryFields.Tasks));
        Assert.AreEqual("Task reopened: «Clean the keyboard»", Line(NotePartChangeDescriber.Task(resolved, before, Names()), EnumHistoryFields.Tasks));
        Assert.AreEqual("Task removed: «Clean the keyboard»", Line(NotePartChangeDescriber.Task(before, null, Names()), EnumHistoryFields.Tasks));
    }

    [TestMethod]
    public void ATaskChanged_SaysWhat_AndKeepsItsPreviousDescription()
    {
        var before = Task();
        var after = Task();
        after.NoteTaskId = before.NoteTaskId;
        after.Description = "Clean the keyboard and the case";
        after.UserId = Marta;
        after.SpentTime = 1.5;

        var change = NotePartChangeDescriber.Task(before, after, Names());

        Assert.AreEqual("Task changed: «Clean the keyboard and the case» (description, user, spent time)", Line(change, EnumHistoryFields.Tasks));
        Assert.AreEqual("Clean the keyboard\r\nWith alcohol.", change.Changes.Single(c => c.Field == HistoryPartFields.Description).Before);
        var user = change.Changes.Single(c => c.Field == HistoryPartFields.User);
        Assert.AreEqual("Armando → Marta", $"{user.Before} → {user.After}");
    }

    [TestMethod]
    public void ATaskSavedAsItWas_IsNoChange()
    {
        var before = Task();
        var after = Task();
        after.NoteTaskId = before.NoteTaskId;
        after.Description = "Clean the keyboard\nWith alcohol.  ";
        after.StartDate = before.StartDate!.Value.AddMilliseconds(300);
        after.ModificationDateTime = DateTime.Now;

        Assert.IsNull(NotePartChangeDescriber.Task(before, after, Names()));
    }

    [TestMethod]
    public void TheDescriptionBefore_IsCutTo4KB()
    {
        var before = Task();
        before.Description = new string('x', 10000);
        var after = Task();
        after.NoteTaskId = before.NoteTaskId;

        var kept = NotePartChangeDescriber.Task(before, after, Names()).Changes.Single(c => c.Field == HistoryPartFields.Description).Before;

        Assert.AreEqual(NotePartChangeDescriber.MaxTaskDescriptionLength, kept.Length);
    }

    [TestMethod]
    public void AResource_RenamedReplacedDescribedLocked()
    {
        var before = new ResourceInfoDto { ResourceId = Guid.NewGuid(), Name = $"{Guid.NewGuid()}_cpc.jpg", Description = "The board" };
        ResourceInfoDto With(Action<ResourceInfoDto> change)
        {
            var after = new ResourceInfoDto { ResourceId = before.ResourceId, Name = before.Name, Description = before.Description };
            change(after);
            return after;
        }

        Assert.AreEqual("Resource added: cpc.jpg", Line(NotePartChangeDescriber.Resource(null, before, true), EnumHistoryFields.Resources));
        Assert.AreEqual("Resource replaced: cpc.jpg → cpc-new.jpg",
            Line(NotePartChangeDescriber.Resource(before, With(r => r.Name = $"{Guid.NewGuid()}_cpc-new.jpg"), newFile: true), EnumHistoryFields.Resources));
        Assert.AreEqual("Resource renamed: cpc.jpg → board.jpg",
            Line(NotePartChangeDescriber.Resource(before, With(r => r.Name = r.Name.Replace("cpc.jpg", "board.jpg")), newFile: false), EnumHistoryFields.Resources));
        Assert.AreEqual("Resource description changed: cpc.jpg",
            Line(NotePartChangeDescriber.Resource(before, With(r => r.Description = "The main board"), false), EnumHistoryFields.Resources));
        Assert.AreEqual("Resource locked: cpc.jpg",
            Line(NotePartChangeDescriber.Resource(before, With(r => r.DescriptionByUser = true), false), EnumHistoryFields.Resources));
        Assert.AreEqual("Resource changed: cpc.jpg (description, locked)",
            Line(NotePartChangeDescriber.Resource(before, With(r => { r.Description = "x"; r.DescriptionByUser = true; }), false), EnumHistoryFields.Resources));
        Assert.IsNull(NotePartChangeDescriber.Resource(before, With(r => r.Order = 3), false), "the order is presentation");
    }

    [TestMethod]
    public void AnAlarm_AddedChangedRemoved()
    {
        var before = new KMessageDto
        {
            KMessageId = Guid.NewGuid(), NoteId = Guid.NewGuid(), UserId = Armando, AlarmActivated = true,
            AlarmDateTime = new DateTime(2026, 11, 26, 9, 0, 0), AlarmType = EnumAlarmType.Standard, NotificationType = EnumNotificationType.AppInfo
        };
        var after = new KMessageDto
        {
            KMessageId = before.KMessageId, NoteId = before.NoteId, UserId = Armando, AlarmActivated = true,
            AlarmDateTime = new DateTime(2026, 11, 27, 9, 0, 0), AlarmType = EnumAlarmType.Standard, NotificationType = EnumNotificationType.Email
        };

        Assert.AreEqual("Alarm added: 2026-11-26 09:00", Line(NotePartChangeDescriber.Alarm(null, before, Names()), EnumHistoryFields.Alarms));
        Assert.AreEqual("Alarm changed: 2026-11-27 09:00 (date, notification)",
            Line(NotePartChangeDescriber.Alarm(before, after, Names()), EnumHistoryFields.Alarms));
        Assert.AreEqual("Alarm removed: 2026-11-26 09:00", Line(NotePartChangeDescriber.Alarm(before, null, Names()), EnumHistoryFields.Alarms));
    }

    [TestMethod]
    public void ARelation_WithItsType_AndRemovedBecauseTheOtherNoteWasDeleted()
    {
        var relation = new TraceNoteDto { TraceNoteId = Guid.NewGuid(), FromId = Guid.NewGuid(), ToId = Guid.NewGuid(), TraceNoteTypeId = Restoration };
        var untyped = new TraceNoteDto { TraceNoteId = Guid.NewGuid(), FromId = Guid.NewGuid(), ToId = Guid.NewGuid() };

        Assert.AreEqual("Related to #12 Amstrad PCW (Restoration)",
            Line(NotePartChangeDescriber.Relation(null, relation, "#12 Amstrad PCW", Names()), EnumHistoryFields.Relations));
        Assert.AreEqual("Related to #12 Amstrad PCW",
            Line(NotePartChangeDescriber.Relation(null, untyped, "#12 Amstrad PCW", Names()), EnumHistoryFields.Relations));
        Assert.AreEqual("Relation removed: #549 Old note (note deleted)",
            Line(NotePartChangeDescriber.Relation(relation, null, "#549 Old note", Names(), otherNoteDeleted: true), EnumHistoryFields.Relations));
    }

    [TestMethod]
    public void InASession_AddedThenRemovedIsNothing_AndChangedBackIsNothing()
    {
        var task = Task();
        var added = NotePartChangeDescriber.Task(null, task, Names());
        var removed = NotePartChangeDescriber.Task(task, null, Names());
        Assert.IsNull(NotePartChangeDescriber.MergeList(new() { added }, new() { removed }));

        var resolvedTask = Task();
        resolvedTask.NoteTaskId = task.NoteTaskId;
        resolvedTask.EndDate = DateTime.Today;
        var resolve = NotePartChangeDescriber.Task(task, resolvedTask, Names());
        var reopen = NotePartChangeDescriber.Task(resolvedTask, task, Names());
        Assert.IsNull(NotePartChangeDescriber.MergeList(new() { resolve }, new() { reopen }));

        var addedThenResolved = NotePartChangeDescriber.MergeList(new() { added }, new() { resolve });
        Assert.AreEqual(HistoryPartActions.Added, addedThenResolved.Single().Action);
    }

    [TestMethod]
    public void ASession_KeepsItsParts_WhenItsOwnDataIsDescribedAgain()
    {
        var withTask = NotePartChangeDescriber.Details(EnumHistoryFields.Tasks, NotePartChangeDescriber.Task(null, Task(), Names()));
        var version = new NoteVersionDto { Topic = "A", Description = "Text", FolderId = Guid.NewGuid() };
        var edited = new NoteVersionDto { Topic = "B", Description = "Text", FolderId = version.FolderId };

        var session = NotePartChangeDescriber.KeepParts(NoteChangeDescriber.Describe(version, edited, new HistoryNames()), withTask);

        Assert.AreEqual(EnumHistoryFields.Topic | EnumHistoryFields.Tasks, session.Fields);
        Assert.AreEqual("Topic: «A» → «B» · Task added: «Clean the keyboard»", NoteChangeDescriber.Render(session));
    }

    [TestMethod]
    public void ADeletedNote_SaysWhatItHad()
    {
        var details = new NoteChangeDetailsDto { Deleted = new HistoryDeletedDto { Folder = "Equipment", Tasks = 2, Resources = 1, Relations = 3 } };

        Assert.AreEqual("Note deleted from Equipment · With 2 tasks, 1 resource, 3 relations", NoteChangeDescriber.Render(details));
    }
}

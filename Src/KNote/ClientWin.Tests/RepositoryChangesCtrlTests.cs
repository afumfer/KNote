using KNote.ClientWin.Controllers;
using KNote.ClientWin.Tests.Fakes;
using KNote.ClientWin.Tests.Helpers;
using KNote.ClientWin.Utils;
using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// RepositoryChangesCtrl («Repository changes», H28): it asks for the changes with the filters of the view, the next page with
/// «Show more», the users of the period, and offers «Open note» only for notes that exist (what the Service says of each row),
/// «See this version» for any change of a note, and the deleted note of a row to select in «Recover deleted notes».
/// </summary>
[TestClass]
public class RepositoryChangesCtrlTests
{
    // With the day the history started (the controller reads it the first time).
    private static FakeKntService NewService()
    {
        var service = new FakeKntService();
        service.HistoryFake.GetRetentionAsyncImpl = () => Task.FromResult(new Result<HistoryRetentionInfoDto>(
            new HistoryRetentionInfoDto { Since = new DateTime(2026, 11, 25) }));
        return service;
    }

    private static KLogDto Row(string entity, string action) => new()
    {
        KLogId = Guid.NewGuid(), EntityId = Guid.NewGuid(), EntityName = entity, ActionType = action, UserName = "marta",
        RegistryDateTime = DateTime.Today, LastDateTime = DateTime.Today, EntityTitle = "#12 CPC"
    };

    [TestMethod]
    public async Task ItAsksWithTheFilters_AndSaysWhatEachRowAllows()
    {
        var service = NewService();
        var alive = Row(HistoryEntities.Note, "Updated");
        var deleted = Row(HistoryEntities.Note, "Deleted");
        deleted.PreviousContentSize = 120;
        var emptied = Row(HistoryEntities.Note, "Deleted");
        var removed = Row(HistoryEntities.Note, "Removed");
        var folder = Row(HistoryEntities.Folder, "Updated");
        var folderDeleted = Row(HistoryEntities.Folder, "Deleted");
        RepositoryChangesQueryDto asked = null;
        service.HistoryFake.GetRepositoryChangesAsyncImpl = query =>
        {
            asked = new RepositoryChangesQueryDto { UserName = query.UserName, ActionType = query.ActionType, Skip = query.Skip };
            return Task.FromResult(new Result<RepositoryChangesPageDto>(new RepositoryChangesPageDto
            {
                Rows = { alive, deleted, emptied, removed, folder, folderDeleted }, Total = 6, StructureIncluded = true,
                ExistingNotes = { alive.EntityId }
            }));
        };
        service.HistoryFake.GetRepositoryChangesUsersAsyncImpl = _ => Task.FromResult(new Result<List<string>>(new List<string> { "marta" }));
        var ctrl = new RepositoryChangesCtrl(TestStoreFactory.CreateEmpty(), service);
        ctrl.Query.UserName = "armando";
        ctrl.Query.ActionType = "Deleted";

        Assert.IsTrue(await ctrl.LoadAsync());

        Assert.AreEqual("armando", asked.UserName);
        Assert.AreEqual("Deleted", asked.ActionType);
        Assert.AreEqual(6, ctrl.Rows.Count);
        Assert.IsTrue(ctrl.StructureIncluded);
        CollectionAssert.AreEqual(new[] { "marta", "armando" }, ctrl.Users, "the user chosen stays in the list");
        Assert.IsTrue(ctrl.CanOpen(alive));
        Assert.IsFalse(ctrl.CanOpen(deleted));
        Assert.IsFalse(ctrl.CanOpen(folder));
        Assert.IsTrue(ctrl.CanSeeVersion(alive), "any change of a note has its version");
        Assert.IsTrue(ctrl.CanSeeVersion(deleted), "as it was deleted: its copy");
        Assert.IsFalse(ctrl.CanSeeVersion(emptied), "its copy is gone and the note doesn't exist");
        Assert.IsFalse(ctrl.CanSeeVersion(removed));
        Assert.IsFalse(ctrl.CanSeeVersion(folder));
        Assert.AreEqual(deleted.EntityId, RepositoryChangesCtrl.DeletedNoteOf(deleted), "selected in «Recover deleted notes»");
        Assert.IsNull(RepositoryChangesCtrl.DeletedNoteOf(alive));
        Assert.IsNull(RepositoryChangesCtrl.DeletedNoteOf(folderDeleted), "a folder is not recovered there");
        Assert.IsNull(RepositoryChangesCtrl.DeletedNoteOf(null));
        Assert.IsFalse(ctrl.HasMore);
        Assert.AreEqual(new DateTime(2026, 11, 25), ctrl.Since);
        StringAssert.StartsWith(ctrl.Summary, "6 changes from ");
    }

    [TestMethod]
    public async Task ShowMore_AsksForTheNextPage()
    {
        var service = NewService();
        var skips = new List<int>();
        service.HistoryFake.GetRepositoryChangesAsyncImpl = query =>
        {
            skips.Add(query.Skip);
            return Task.FromResult(new Result<RepositoryChangesPageDto>(new RepositoryChangesPageDto
            {
                Rows = { Row(HistoryEntities.Note, "Updated") }, Total = 2, Skip = query.Skip
            }));
        };
        service.HistoryFake.GetRepositoryChangesUsersAsyncImpl = _ => Task.FromResult(new Result<List<string>>(new List<string>()));
        var ctrl = new RepositoryChangesCtrl(TestStoreFactory.CreateEmpty(), service);

        await ctrl.LoadAsync();
        Assert.IsTrue(ctrl.HasMore);
        Assert.IsTrue(await ctrl.LoadMoreAsync());

        CollectionAssert.AreEqual(new[] { 0, 1 }, skips);
        Assert.AreEqual(2, ctrl.Rows.Count);
        Assert.IsFalse(ctrl.HasMore);
        Assert.AreEqual(0, ctrl.Query.Skip, "the filters keep the first page");
    }

    [TestMethod]
    public async Task ChangesThatCantBeRead_GiveTheirError()
    {
        var service = NewService();
        service.HistoryFake.GetRepositoryChangesAsyncImpl = _ =>
        {
            var result = new Result<RepositoryChangesPageDto>();
            result.AddErrorMessage("The database is not available.");
            return Task.FromResult(result);
        };
        var ctrl = new RepositoryChangesCtrl(TestStoreFactory.CreateEmpty(), service);

        Assert.IsFalse(await ctrl.LoadAsync());
        StringAssert.Contains(ctrl.Error, "not available");
    }

    [TestMethod]
    public async Task EachRow_HasTheStateTheServiceGives_AndAGuestCantRecover()
    {
        var service = NewService();
        var alive = Row(HistoryEntities.Note, "Updated");
        var deleted = Row(HistoryEntities.Note, "Deleted");
        var folder = Row(HistoryEntities.Folder, "Updated");
        var until = new DateTime(2026, 12, 27);
        service.HistoryFake.GetRepositoryChangesAsyncImpl = _ => Task.FromResult(new Result<RepositoryChangesPageDto>(new RepositoryChangesPageDto
        {
            Rows = { alive, deleted, folder }, Total = 3, ExistingNotes = { alive.EntityId }, CanRecover = false,
            States =
            {
                new HistoryRowStateDto { KLogId = deleted.KLogId, State = EnumHistoryRowState.RecoverableByStaff, Until = until },
                new HistoryRowStateDto { KLogId = folder.KLogId, State = EnumHistoryRowState.OnlyNoted }
            }
        }));
        service.HistoryFake.GetRepositoryChangesUsersAsyncImpl = _ => Task.FromResult(new Result<List<string>>(new List<string>()));
        var ctrl = new RepositoryChangesCtrl(TestStoreFactory.CreateEmpty(), service);

        Assert.IsTrue(await ctrl.LoadAsync());

        Assert.IsFalse(ctrl.CanRecover, "no «Recover deleted notes ...»");
        Assert.AreEqual(EnumHistoryRowState.None, ctrl.StateOf(alive));
        Assert.AreEqual("", ctrl.StateText(alive));
        Assert.AreEqual(EnumHistoryRowState.RecoverableByStaff, ctrl.StateOf(deleted));
        Assert.AreEqual("Recoverable by Staff and up", ctrl.StateText(deleted));
        Assert.AreEqual(EnumHistoryRowState.OnlyNoted, ctrl.StateOf(folder));
        CollectionAssert.AreEquivalent(new[] { EnumHistoryRowState.RecoverableByStaff, EnumHistoryRowState.OnlyNoted }, ctrl.States.ToList());
        Assert.AreEqual(EnumHistoryRowTone.Grey, HistoryRowStates.Tone(ctrl.StateOf(folder)));
        Assert.AreNotEqual(HistoryRowColors.For(EnumHistoryRowTone.Amber, dark: false), HistoryRowColors.For(EnumHistoryRowTone.Amber, dark: true),
            "each tone has its colour for the dark mode");
        Assert.IsTrue(HistoryRowColors.For(EnumHistoryRowTone.None).IsEmpty);
    }
}

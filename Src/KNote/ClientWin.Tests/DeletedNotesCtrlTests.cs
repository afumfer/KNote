using KNote.ClientWin.Controllers;
using KNote.ClientWin.Tests.Fakes;
using KNote.ClientWin.Tests.Helpers;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// DeletedNotesCtrl («Recover deleted notes», H24, H27): it reads the deleted notes and how long they are kept, recovers one (and
/// reads the list again, so that the main window refreshes its own), and «Remove permanently» only for an Admin of the repository.
/// </summary>
[TestClass]
public class DeletedNotesCtrlTests
{
    private static DeletedNoteDto Deleted(int number) => new()
    {
        NoteId = Guid.NewGuid(), NoteNumber = number, Topic = $"Note {number}", FolderPath = "Home", DeletedAt = new DateTime(2026, 11, 25, 10, 0, 0)
    };

    [TestMethod]
    public async Task ItReadsTheDeletedNotes_AndHowLongTheyAreKept()
    {
        var service = new FakeKntService();
        service.HistoryFake.GetRetentionAsyncImpl = () => Task.FromResult(new Result<HistoryRetentionInfoDto>(
            new HistoryRetentionInfoDto { Retention = new HistoryRetentionDto { DeletedNotesDays = 7 } }));
        service.HistoryFake.GetDeletedNotesAsyncImpl = () => Task.FromResult(new Result<List<DeletedNoteDto>>(new List<DeletedNoteDto> { Deleted(12) }));
        var ctrl = new DeletedNotesCtrl(TestStoreFactory.CreateEmpty(), service);

        Assert.IsTrue(await ctrl.LoadAsync());

        Assert.AreEqual(12, ctrl.Notes.Single().NoteNumber);
        Assert.AreEqual(7, ctrl.KeptDays);
        StringAssert.StartsWith(ctrl.Subtitle, "Deleted notes are kept 7 days.");
    }

    [TestMethod]
    public async Task Recovering_ReadsTheListAgain_AndSaysSo()
    {
        var service = new FakeKntService();
        var note = Deleted(12);
        var list = new List<DeletedNoteDto> { note };
        service.HistoryFake.GetRetentionAsyncImpl = () => Task.FromResult(new Result<HistoryRetentionInfoDto>(new HistoryRetentionInfoDto()));
        service.HistoryFake.GetDeletedNotesAsyncImpl = () => Task.FromResult(new Result<List<DeletedNoteDto>>(list.ToList()));
        service.HistoryFake.RecoverNoteAsyncImpl = id =>
        {
            list.RemoveAll(n => n.NoteId == id);
            return Task.FromResult(new Result<NoteRecoverResultDto>(new NoteRecoverResultDto
            {
                NoteId = id, NoteNumber = 12, Topic = "Note 12", Notices = { "Its tasks, resources, alarms and relations are not recovered." }
            }));
        };
        var ctrl = new DeletedNotesCtrl(TestStoreFactory.CreateEmpty(), service);
        await ctrl.LoadAsync();

        Assert.IsTrue(await ctrl.RecoverAsync(ctrl.Notes.Single()));

        Assert.IsTrue(ctrl.AnyRecovered);
        Assert.AreEqual(12, ctrl.Recovered.NoteNumber);
        Assert.AreEqual(0, ctrl.Notes.Count);
    }

    [TestMethod]
    public async Task ARecoveryThatFails_GivesItsError()
    {
        var service = new FakeKntService();
        service.HistoryFake.RecoverNoteAsyncImpl = _ =>
        {
            var result = new Result<NoteRecoverResultDto>();
            result.AddErrorMessage("This note is not among the deleted notes that can be recovered.");
            return Task.FromResult(result);
        };
        var ctrl = new DeletedNotesCtrl(TestStoreFactory.CreateEmpty(), service);

        Assert.IsFalse(await ctrl.RecoverAsync(Deleted(3)));

        Assert.IsFalse(ctrl.AnyRecovered);
        StringAssert.Contains(ctrl.Error, "not among the deleted notes");
    }

    [TestMethod]
    public async Task OnlyAnAdmin_RemovesPermanently()
    {
        var service = new FakeKntService();
        var removed = 0;
        service.HistoryFake.RemoveDeletedNoteAsyncImpl = _ => { removed++; return Task.FromResult(new Result()); };
        service.HistoryFake.GetRetentionAsyncImpl = () => Task.FromResult(new Result<HistoryRetentionInfoDto>(new HistoryRetentionInfoDto()));
        service.HistoryFake.GetDeletedNotesAsyncImpl = () => Task.FromResult(new Result<List<DeletedNoteDto>>(new List<DeletedNoteDto>()));

        var store = TestStoreFactory.CreateEmpty();
        store.Security.SetRepositoryRole(service, EnumRoles.Staff);
        var staff = new DeletedNotesCtrl(store, service);
        Assert.IsTrue(staff.IsAuthorized(), "Staff opens it");
        Assert.IsFalse(staff.CanRemove);
        Assert.IsFalse(await staff.RemoveAsync(Deleted(5)));
        Assert.AreEqual(0, removed);

        var admin = new DeletedNotesCtrl(TestStoreFactory.CreateEmpty().GrantAdmin(service), service);
        Assert.IsTrue(admin.CanRemove);
        Assert.IsTrue(await admin.RemoveAsync(Deleted(5)));
        Assert.AreEqual(1, removed);
    }
}

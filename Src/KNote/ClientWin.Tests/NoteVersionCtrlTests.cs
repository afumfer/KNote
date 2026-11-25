using KNote.ClientWin.Controllers;
using KNote.ClientWin.Tests.Fakes;
using KNote.ClientWin.Tests.Helpers;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// NoteVersionCtrl (the history of a note, H.3, «See this version»): it reads the version of the row from the Service and gives the
/// window its title; a version that can't be read gives its error and no window.
/// </summary>
[TestClass]
public class NoteVersionCtrlTests
{
    private static readonly KLogDto Row = new()
    {
        KLogId = Guid.NewGuid(), UserName = "marta", RegistryDateTime = new DateTime(2026, 11, 25, 10, 5, 0), LastDateTime = new DateTime(2026, 11, 25, 10, 31, 0)
    };

    [TestMethod]
    public async Task ItReadsTheVersionOfTheRow()
    {
        var service = new FakeKntService();
        var noteId = Guid.NewGuid();
        (Guid NoteId, Guid KLogId) asked = default;
        service.HistoryFake.GetNoteVersionAsyncImpl = (note, klog) =>
        {
            asked = (note, klog);
            return Task.FromResult(new Result<NoteVersionViewDto>(new NoteVersionViewDto { Topic = "Amstrad CPC 464", Notice = "Includes later changes" }));
        };
        var ctrl = new NoteVersionCtrl(TestStoreFactory.CreateEmpty(), service, noteId, Row, "#12 · CPC");

        Assert.IsTrue(await ctrl.LoadAsync());

        Assert.AreEqual((noteId, Row.KLogId), asked);
        Assert.AreEqual("Amstrad CPC 464", ctrl.Version.Topic);
        Assert.AreEqual("Version of #12 · CPC", ctrl.Title);
        StringAssert.StartsWith(ctrl.Subtitle, "As it was after the change of ");
        StringAssert.EndsWith(ctrl.Subtitle, " by marta");
    }

    [TestMethod]
    public async Task AVersionThatCantBeRead_GivesItsError()
    {
        var service = new FakeKntService();
        service.HistoryFake.GetNoteVersionAsyncImpl = (_, _) =>
        {
            var result = new Result<NoteVersionViewDto>();
            result.AddErrorMessage("The copy of this version is no longer kept.");
            return Task.FromResult(result);
        };
        var ctrl = new NoteVersionCtrl(TestStoreFactory.CreateEmpty(), service, Guid.NewGuid(), Row, null);

        Assert.IsFalse(await ctrl.LoadAsync());

        Assert.IsNull(ctrl.Version);
        StringAssert.Contains(ctrl.Error, "no longer kept");
        Assert.AreEqual("Version of the note", ctrl.Title);
    }
}

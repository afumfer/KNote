using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Regression coverage for CtrlNoteEditorBase.SaveModel's re-entrancy guard (see CtrlViewBase.cs):
/// MessagesManagmentCtrl's autosave timer calls Store.SaveActiveNotes(), which calls SaveModel() on
/// every open NoteEditorCtrl/PostItEditorCtrl, while a manual save (toolbar button, Ctrl+S, closing
/// the form, ...) may already be awaiting its own SaveModel() call on that same controller instance.
/// Both run on the WinForms UI thread, so this is never true multi-threading, but the network/DB
/// "await" inside SaveModelCore() yields back to the message loop, letting the second trigger start
/// a second, overlapping SaveModelCore() before the first has finished.
///
/// This is exercised here through NoteEditorCtrl (the fakes already support it), but the guard
/// itself lives once in the shared CtrlNoteEditorBase base class, so this equally covers
/// PostItEditorCtrl and FolderEditorCtrl - proving the mechanism, not re-testing per subclass.
/// </summary>
[TestClass]
public class SaveModelReentrancyTests
{
    private static (NoteEditorCtrl ctrl, FakeNoteEditorView view, FakeKntService service) CreateCtrl()
    {
        var factoryViews = new TestFactoryViews();
        var view = new FakeNoteEditorView();
        factoryViews.Registry.Register<NoteEditorCtrl, IViewNoteEditorEmbeddable<NoteExtendedDto>>(c => view);

        var store = new Store(factoryViews);
        var ctrl = new NoteEditorCtrl(store);
        var service = new FakeKntService();

        return (ctrl, view, service);
    }

    private static NoteExtendedDto CreateValidNote() => new()
    {
        NoteId = Guid.NewGuid(),
        Topic = "Test topic",
        FolderId = Guid.NewGuid(),
        CreationDateTime = DateTime.Now,
        ModificationDateTime = DateTime.Now,
    };

    [TestMethod]
    public async Task SaveModel_CalledWhileAlreadySaving_DoesNotStartASecondSave()
    {
        var (ctrl, _, service) = CreateCtrl();
        var note = CreateValidNote();
        service.NotesFake.GetExtendedAsyncImpl = _ => Task.FromResult(new Result<NoteExtendedDto>(note));
        await ctrl.LoadModelById(service, note.NoteId);
        ctrl.Model.Topic = "Changed, so the model is dirty";

        var saveCallCount = 0;
        var pendingSave = new TaskCompletionSource<Result<NoteExtendedDto>>();
        service.NotesFake.SaveExtendedAsyncImpl = _ =>
        {
            saveCallCount++;
            return pendingSave.Task;
        };

        // Simulates a manual save (toolbar button) starting, but not yet completing - e.g. still
        // waiting on the network/DB round trip.
        var firstSave = ctrl.SaveModel();

        // Simulates the autosave timer firing while the manual save above is still in flight.
        var secondSave = ctrl.SaveModel();

        Assert.AreEqual(1, saveCallCount,
            "A second concurrent SaveModel() call must await the in-flight save, not start a redundant, overlapping one.");
        Assert.IsFalse(firstSave.IsCompleted);
        Assert.IsFalse(secondSave.IsCompleted);

        // Let the single, shared save complete.
        pendingSave.SetResult(new Result<NoteExtendedDto>(note));

        Assert.IsTrue(await firstSave);
        Assert.IsTrue(await secondSave);
        Assert.AreEqual(1, saveCallCount, "Still only one save should have happened in total.");
    }

    [TestMethod]
    public async Task SaveModel_CalledAfterAPreviousSaveCompleted_StartsANewSave()
    {
        var (ctrl, _, service) = CreateCtrl();
        var note = CreateValidNote();
        service.NotesFake.GetExtendedAsyncImpl = _ => Task.FromResult(new Result<NoteExtendedDto>(note));
        await ctrl.LoadModelById(service, note.NoteId);

        var saveCallCount = 0;
        service.NotesFake.SaveExtendedAsyncImpl = _ =>
        {
            saveCallCount++;
            return Task.FromResult(new Result<NoteExtendedDto>(note));
        };

        ctrl.Model.Topic = "First change";
        Assert.IsTrue(await ctrl.SaveModel());

        ctrl.Model.Topic = "Second change";
        Assert.IsTrue(await ctrl.SaveModel());

        // The guard must not "stick" after the in-flight save it was guarding against has
        // completed - a later, independent save has to go through normally.
        Assert.AreEqual(2, saveCallCount);
    }
}

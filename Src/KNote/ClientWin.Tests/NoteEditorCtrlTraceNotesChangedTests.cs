using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Tests for NoteEditorCtrl's TraceNotesChanged subscription (Store.Events): an already-open note
/// editor adds the relations created elsewhere (e.g. KNoteManagementCtrl.TraceSelectedNotes) to its
/// "Trace notes" lists, without losing the user's own pending changes to them. The fake service
/// completes synchronously, so the async void handler has finished by the time Publish returns.
/// </summary>
[TestClass]
public class NoteEditorCtrlTraceNotesChangedTests
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

    private static async Task<NoteExtendedDto> LoadNoteAsync(NoteEditorCtrl ctrl, FakeKntService service, params TraceNoteDto[] traceNotesTo)
    {
        var note = new NoteExtendedDto { NoteId = Guid.NewGuid(), Topic = "Test topic" };
        note.TraceNotesTo.AddRange(traceNotesTo);
        service.NotesFake.GetExtendedAsyncImpl = _ => Task.FromResult(new Result<NoteExtendedDto>(note));
        await ctrl.LoadModelById(service, note.NoteId, false);
        return note;
    }

    private static void SetStoredTraceNotes(FakeKntService service, List<TraceNoteDto> from, List<TraceNoteDto> to)
    {
        service.NotesFake.GetTraceNotesFromAsyncImpl = _ => Task.FromResult(new Result<List<TraceNoteDto>>(from));
        service.NotesFake.GetTraceNotesToAsyncImpl = _ => Task.FromResult(new Result<List<TraceNoteDto>>(to));
    }

    [TestMethod]
    public async Task TraceNotesChanged_ForThisNote_AddsNewRelationsAndRefreshesTraceNotesDisplay()
    {
        var (ctrl, view, service) = CreateCtrl();
        var note = await LoadNoteAsync(ctrl, service);
        var newFrom = new TraceNoteDto { TraceNoteId = Guid.NewGuid(), FromId = Guid.NewGuid(), ToId = note.NoteId };
        var newTo = new TraceNoteDto { TraceNoteId = Guid.NewGuid(), FromId = note.NoteId, ToId = Guid.NewGuid() };
        SetStoredTraceNotes(service, new() { newFrom }, new() { newTo });
        var refreshed = false;
        view.RefreshTraceNotesDisplayAsyncImpl = () => { refreshed = true; return Task.CompletedTask; };

        ctrl.Store.Events.Publish(new TraceNotesChanged(new[] { note.NoteId }));

        Assert.IsTrue(refreshed);
        CollectionAssert.AreEqual(new[] { newFrom.TraceNoteId }, ctrl.Model.TraceNotesFrom.Select(t => t.TraceNoteId).ToArray());
        CollectionAssert.AreEqual(new[] { newTo.TraceNoteId }, ctrl.Model.TraceNotesTo.Select(t => t.TraceNoteId).ToArray());
        Assert.IsFalse(newFrom.IsDirty());
        Assert.IsFalse(newTo.IsDirty());
    }

    [TestMethod]
    public async Task TraceNotesChanged_KeepsPendingLocalChanges()
    {
        var (ctrl, view, service) = CreateCtrl();
        var existing = new TraceNoteDto { TraceNoteId = Guid.NewGuid(), ToId = Guid.NewGuid() };
        var note = await LoadNoteAsync(ctrl, service, existing);
        existing.FromId = note.NoteId;
        // Pending, unsaved change in this editor: the existing relation has been removed.
        existing.SetIsDeleted(true);

        var storedExisting = new TraceNoteDto { TraceNoteId = existing.TraceNoteId, FromId = note.NoteId, ToId = existing.ToId };
        var newTo = new TraceNoteDto { TraceNoteId = Guid.NewGuid(), FromId = note.NoteId, ToId = Guid.NewGuid() };
        SetStoredTraceNotes(service, new(), new() { storedExisting, newTo });
        view.RefreshTraceNotesDisplayAsyncImpl = () => Task.CompletedTask;

        ctrl.Store.Events.Publish(new TraceNotesChanged(new[] { note.NoteId }));

        Assert.AreEqual(2, ctrl.Model.TraceNotesTo.Count);
        Assert.AreSame(existing, ctrl.Model.TraceNotesTo.Single(t => t.TraceNoteId == existing.TraceNoteId));
        Assert.IsTrue(existing.IsDeleted());
        Assert.IsTrue(ctrl.Model.TraceNotesTo.Any(t => t.TraceNoteId == newTo.TraceNoteId));
    }

    [TestMethod]
    public async Task TraceNotesChanged_ForOtherNotes_IsIgnored()
    {
        var (ctrl, view, service) = CreateCtrl();
        await LoadNoteAsync(ctrl, service);
        // Service and view delegates left unconfigured: touching either would throw.

        ctrl.Store.Events.Publish(new TraceNotesChanged(new[] { Guid.NewGuid() }));

        Assert.AreEqual(0, ctrl.Model.TraceNotesTo.Count);
        Assert.IsNull(view.LastShownInfo);
    }

    [TestMethod]
    public async Task TraceNotesChanged_NothingNew_DoesNotRefreshView()
    {
        var (ctrl, view, service) = CreateCtrl();
        var existing = new TraceNoteDto { TraceNoteId = Guid.NewGuid(), ToId = Guid.NewGuid() };
        var note = await LoadNoteAsync(ctrl, service, existing);
        SetStoredTraceNotes(service, new(), new() { new TraceNoteDto { TraceNoteId = existing.TraceNoteId, FromId = note.NoteId, ToId = existing.ToId } });
        // View delegate left unconfigured: a refresh would throw and be shown through ShowInfo.

        ctrl.Store.Events.Publish(new TraceNotesChanged(new[] { note.NoteId }));

        Assert.AreEqual(1, ctrl.Model.TraceNotesTo.Count);
        Assert.IsNull(view.LastShownInfo);
    }

    // Closing the editor window finalizes the controller without disposing it.
    [TestMethod]
    public async Task TraceNotesChanged_AfterFinalize_IsIgnored()
    {
        var (ctrl, view, service) = CreateCtrl();
        var note = await LoadNoteAsync(ctrl, service);
        ctrl.Finalize();

        ctrl.Store.Events.Publish(new TraceNotesChanged(new[] { note.NoteId }));

        Assert.AreEqual(0, ctrl.Model.TraceNotesTo.Count);
        Assert.IsNull(view.LastShownInfo);
    }

    [TestMethod]
    public async Task TraceNotesChanged_AfterDispose_IsIgnored()
    {
        var (ctrl, view, service) = CreateCtrl();
        var note = await LoadNoteAsync(ctrl, service);
        ctrl.Dispose();

        ctrl.Store.Events.Publish(new TraceNotesChanged(new[] { note.NoteId }));

        Assert.AreEqual(0, ctrl.Model.TraceNotesTo.Count);
        Assert.IsNull(view.LastShownInfo);
    }
}

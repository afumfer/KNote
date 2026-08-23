using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Fase 3b of the ClientWin architecture refactor moved the NoteEditorCtrl/PostItEditorCtrl
/// coordination (self-closing when their note is deleted elsewhere, and the note&lt;-&gt;post-it
/// handoff) off Store's special-cased relay and onto Store.Events. These tests exercise that
/// coordination directly through public API (FinalizeAndPostItEdit/FinalizeAndExtendEdit,
/// publishing EntityDeleted on the bus) without needing a real Service/DB/View.
/// </summary>
[TestClass]
public class NotePostItHandoffTests
{
    [TestMethod]
    public void NoteEditorCtrl_FinalizeAndPostItEdit_PublishesPostItEditRequested()
    {
        var store = new Store(factoryViews: null!);
        var ctrl = new NoteEditorCtrl(store);
        PostItEditRequested? received = null;
        store.Events.Subscribe<PostItEditRequested>(msg => received = msg);

        ctrl.FinalizeAndPostItEdit();

        Assert.IsNotNull(received);
    }

    [TestMethod]
    public void PostItEditorCtrl_FinalizeAndExtendEdit_PublishesExtendedEditRequested()
    {
        var store = new Store(factoryViews: null!);
        var ctrl = new PostItEditorCtrl(store);
        ExtendedEditRequested? received = null;
        store.Events.Subscribe<ExtendedEditRequested>(msg => received = msg);

        ctrl.FinalizeAndExtendEdit();

        Assert.IsNotNull(received);
    }

    [TestMethod]
    public void NoteEditorCtrl_ClosesWhenItsNoteIsDeletedElsewhere()
    {
        var store = new Store(factoryViews: null!);
        var ctrl = new NoteEditorCtrl(store);
        var noteId = Guid.NewGuid();
        ctrl.Model.NoteId = noteId;

        store.Events.Publish(new EntityDeleted<NoteExtendedDto>(new NoteExtendedDto { NoteId = noteId }));

        Assert.AreEqual(EControllerState.Finalized, ctrl.ControllerState);
    }

    [TestMethod]
    public void NoteEditorCtrl_InEmbededMode_DoesNotCloseWhenItsNoteIsDeletedElsewhere()
    {
        var store = new Store(factoryViews: null!);
        var ctrl = new NoteEditorCtrl(store) { EmbededMode = true };
        var noteId = Guid.NewGuid();
        ctrl.Model.NoteId = noteId;

        store.Events.Publish(new EntityDeleted<NoteExtendedDto>(new NoteExtendedDto { NoteId = noteId }));

        Assert.AreNotEqual(EControllerState.Finalized, ctrl.ControllerState);
    }

    [TestMethod]
    public void NoteEditorCtrl_DoesNotCloseWhenADifferentNoteIsDeletedElsewhere()
    {
        var store = new Store(factoryViews: null!);
        var ctrl = new NoteEditorCtrl(store);
        ctrl.Model.NoteId = Guid.NewGuid();

        store.Events.Publish(new EntityDeleted<NoteExtendedDto>(new NoteExtendedDto { NoteId = Guid.NewGuid() }));

        Assert.AreNotEqual(EControllerState.Finalized, ctrl.ControllerState);
    }

    [TestMethod]
    public void PostItEditorCtrl_ClosesWhenItsNoteIsDeletedElsewhere()
    {
        var store = new Store(factoryViews: null!);
        var ctrl = new PostItEditorCtrl(store);
        var noteId = Guid.NewGuid();
        ctrl.Model.NoteId = noteId;

        store.Events.Publish(new EntityDeleted<NoteExtendedDto>(new NoteExtendedDto { NoteId = noteId }));

        Assert.AreEqual(EControllerState.Finalized, ctrl.ControllerState);
    }
}

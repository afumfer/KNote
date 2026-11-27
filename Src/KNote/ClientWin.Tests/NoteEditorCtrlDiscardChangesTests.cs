using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// «Undo» of the note editor (NoteEditorCtrl.DiscardChangesAsync): after confirming, everything not saved is discarded, also what the
/// editor keeps in the model until the note is saved (resources, tasks, relations...): a note that exists is read again from the
/// service; a new one goes back to blank, in its folder. Answering No changes nothing.
/// </summary>
[TestClass]
public class NoteEditorCtrlDiscardChangesTests
{
    private static (NoteEditorCtrl ctrl, FakeNoteEditorView view, FakeKntService service) CreateCtrl()
    {
        var factoryViews = new TestFactoryViews();
        var view = new FakeNoteEditorView();
        factoryViews.Registry.Register<NoteEditorCtrl, IViewNoteEditorEmbeddable<NoteExtendedDto>>(c => view);
        var ctrl = new NoteEditorCtrl(new Store(factoryViews));
        return (ctrl, view, new FakeKntService());
    }

    // As stored: a new copy each time it is read, as the database gives it.
    private static NoteExtendedDto Stored(Guid noteId, Guid resourceId)
    {
        var note = new NoteExtendedDto { NoteId = noteId, NoteNumber = 7, Topic = "Stored topic", Description = "Stored text", Tags = "" };
        note.Resources.Add(new ResourceDto { ResourceId = resourceId, NoteId = noteId, Name = "a.png", Description = "Stored resource" });
        return note;
    }

    [TestMethod]
    public async Task Discard_ReadsTheNoteAgain_AndForgetsWhatTheEditorKeptInTheModel()
    {
        var (ctrl, view, service) = CreateCtrl();
        var noteId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var reads = 0;
        service.NotesFake.GetExtendedAsyncImpl = _ =>
        {
            reads++;
            return Task.FromResult(new Result<NoteExtendedDto>(Stored(noteId, resourceId)));
        };
        Assert.IsTrue(await ctrl.LoadModelById(service, noteId, false));

        // Changes not saved: the text, a resource edited in place, a task and a relation added.
        ctrl.Model.Topic = "Changed topic";
        ctrl.Model.Resources[0].Description = "Changed resource";
        ctrl.Model.Tasks.Add(new NoteTaskDto { NoteTaskId = Guid.NewGuid(), NoteId = noteId, Description = "New task" });
        ctrl.Model.TraceNotesTo.Add(new TraceNoteDto { TraceNoteId = Guid.NewGuid(), FromId = noteId, ToId = Guid.NewGuid() });
        Assert.IsTrue(ctrl.Model.IsDirty());
        var refreshes = view.RefreshViewCallCount;

        Assert.IsTrue(await ctrl.DiscardChangesAsync());

        Assert.AreEqual(NoteEditorCtrl.DiscardChangesQuestion, view.LastShownInfo);
        Assert.AreEqual(2, reads, "read again from the service");
        Assert.AreEqual("Stored topic", ctrl.Model.Topic);
        Assert.AreEqual("Stored resource", ctrl.Model.Resources.Single().Description);
        Assert.AreEqual(0, ctrl.Model.Tasks.Count);
        Assert.AreEqual(0, ctrl.Model.TraceNotesTo.Count);
        Assert.IsFalse(ctrl.Model.IsDirty());
        Assert.AreEqual(refreshes + 1, view.RefreshViewCallCount, "the editor drawn again once");
    }

    [TestMethod]
    public async Task AnsweringNo_ChangesNothing()
    {
        var (ctrl, view, service) = CreateCtrl();
        var noteId = Guid.NewGuid();
        var reads = 0;
        service.NotesFake.GetExtendedAsyncImpl = _ =>
        {
            reads++;
            return Task.FromResult(new Result<NoteExtendedDto>(Stored(noteId, Guid.NewGuid())));
        };
        await ctrl.LoadModelById(service, noteId, false);
        ctrl.Model.Topic = "Changed topic";
        view.ConfirmationResult = DialogResult.No;

        Assert.IsFalse(await ctrl.DiscardChangesAsync());

        Assert.AreEqual(1, reads);
        Assert.AreEqual("Changed topic", ctrl.Model.Topic);
        Assert.IsTrue(ctrl.Model.IsDirty());
    }

    [TestMethod]
    public async Task ANewNote_GoesBackToBlank_InItsFolder()
    {
        var (ctrl, view, service) = CreateCtrl();
        var home = new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 1, Name = "Home" };
        var projects = new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 5, Name = "Projects" };
        service.NotesFake.NewExtendedAsyncImpl = _ => Task.FromResult(new Result<NoteExtendedDto>(new NoteExtendedDto()));
        service.FoldersFake.GetByNumberAsyncImpl = _ => Task.FromResult(new Result<FolderDto>(home));
        Assert.IsTrue(await ctrl.NewModel(service));
        ctrl.Model.FolderId = projects.FolderId;
        ctrl.Model.FolderDto = projects;
        ctrl.Model.SetIsDirty(false);
        ctrl.Model.Topic = "My topic";
        ctrl.Model.Description = "Some text";
        ctrl.Model.Tasks.Add(new NoteTaskDto { NoteTaskId = Guid.NewGuid(), Description = "New task" });

        Assert.IsTrue(await ctrl.DiscardChangesAsync());

        Assert.AreEqual(NoteEditorCtrl.DiscardNewNoteQuestion, view.LastShownInfo);
        Assert.AreEqual(Guid.Empty, ctrl.Model.NoteId, "still a new note");
        Assert.AreEqual("New topic ...", ctrl.Model.Topic);
        Assert.AreEqual("", ctrl.Model.Description);
        Assert.AreEqual(0, ctrl.Model.Tasks.Count);
        Assert.AreEqual(projects.FolderId, ctrl.Model.FolderId, "in the folder it had");
        Assert.IsFalse(ctrl.Model.IsDirty());
    }

    [TestMethod]
    public async Task ANoteThatCantBeReadAgain_SaysWhy_AndKeepsTheChanges()
    {
        var (ctrl, view, service) = CreateCtrl();
        var noteId = Guid.NewGuid();
        var deleted = false;
        service.NotesFake.GetExtendedAsyncImpl = _ =>
        {
            if (!deleted)
                return Task.FromResult(new Result<NoteExtendedDto>(Stored(noteId, Guid.NewGuid())));
            var gone = new Result<NoteExtendedDto>();
            gone.AddErrorMessage("The note doesn't exist.");
            return Task.FromResult(gone);
        };
        await ctrl.LoadModelById(service, noteId, false);
        ctrl.Model.Topic = "Changed topic";
        deleted = true;

        Assert.IsFalse(await ctrl.DiscardChangesAsync());

        Assert.AreEqual("The note doesn't exist.", view.LastShownInfo);
        Assert.AreEqual("Changed topic", ctrl.Model.Topic);
    }
}

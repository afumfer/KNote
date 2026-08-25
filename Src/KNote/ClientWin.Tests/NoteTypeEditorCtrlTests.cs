using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Tests for NoteTypeEditorCtrl: the single note type add/edit popup used by NoteTypesManageCtrl
/// (repository administration - Note types tab). Every save/delete persists immediately (AutoDBSave
/// stays true, the CtrlEditorBase default) since there is no parent "Save" to stage into.
/// </summary>
[TestClass]
public class NoteTypeEditorCtrlTests
{
    private static (NoteTypeEditorCtrl ctrl, FakeNoteTypeEditorView view, FakeKntService service) CreateCtrl()
    {
        var factoryViews = new TestFactoryViews();
        var view = new FakeNoteTypeEditorView();
        factoryViews.Registry.Register<NoteTypeEditorCtrl, IViewEditor<NoteTypeDto>>(c => view);

        var store = new Store(factoryViews) { AppUserName = "jdoe" };
        var ctrl = new NoteTypeEditorCtrl(store);
        var service = new FakeKntService();

        return (ctrl, view, service);
    }

    [TestMethod]
    public async Task NewModel_CreatesEmptyModel()
    {
        var (ctrl, _, service) = CreateCtrl();

        await ctrl.NewModel(service);

        Assert.AreEqual(Guid.Empty, ctrl.Model.NoteTypeId);
    }

    [TestMethod]
    public async Task SaveModel_NotDirty_ReturnsTrueWithoutCallingService()
    {
        var (ctrl, _, service) = CreateCtrl();
        await ctrl.NewModel(service);
        // Model is untouched (not dirty): SaveModel must short-circuit before calling the service.

        var saveCalled = false;
        service.NoteTypesFake.SaveAsyncImpl = e => { saveCalled = true; return Task.FromResult(new Result<NoteTypeDto>(e)); };

        var saved = await ctrl.SaveModel();

        Assert.IsTrue(saved);
        Assert.IsFalse(saveCalled);
    }

    [TestMethod]
    public async Task SaveModel_NewNoteType_CallsSaveAsync_ReturnsTrue_FiresAddedEntity()
    {
        var (ctrl, _, service) = CreateCtrl();
        await ctrl.NewModel(service);
        ctrl.Model.Name = "Task";
        ctrl.Model.Description = "A task note type";

        service.NoteTypesFake.SaveAsyncImpl = e =>
        {
            e.NoteTypeId = Guid.NewGuid();
            return Task.FromResult(new Result<NoteTypeDto>(e));
        };
        NoteTypeDto? addedEntity = null;
        ctrl.AddedEntity += (s, e) => addedEntity = e.Entity;

        var saved = await ctrl.SaveModel();

        Assert.IsTrue(saved);
        Assert.IsNotNull(addedEntity);
        Assert.AreEqual("Task", addedEntity!.Name);
        Assert.AreNotEqual(Guid.Empty, addedEntity.NoteTypeId);
    }

    [TestMethod]
    public async Task SaveModel_ExistingNoteType_CallsSaveAsync_ReturnsTrue_FiresSavedEntity_NotAddedEntity()
    {
        var (ctrl, _, service) = CreateCtrl();
        var existingId = Guid.NewGuid();
        service.NoteTypesFake.GetAsyncImpl = _ => Task.FromResult(new Result<NoteTypeDto>(new NoteTypeDto { NoteTypeId = existingId, Name = "Task" }));
        await ctrl.LoadModelById(service, existingId, false);
        ctrl.Model.Description = "Updated description";

        service.NoteTypesFake.SaveAsyncImpl = e => Task.FromResult(new Result<NoteTypeDto>(e));
        NoteTypeDto? savedEntity = null;
        NoteTypeDto? addedEntity = null;
        ctrl.SavedEntity += (s, e) => savedEntity = e.Entity;
        ctrl.AddedEntity += (s, e) => addedEntity = e.Entity;

        var saved = await ctrl.SaveModel();

        Assert.IsTrue(saved);
        Assert.IsNotNull(savedEntity);
        Assert.IsNull(addedEntity);
    }

    [TestMethod]
    public async Task SaveModel_ServiceReturnsInvalidResult_ReturnsFalseAndShowsError()
    {
        var (ctrl, view, service) = CreateCtrl();
        await ctrl.NewModel(service);
        ctrl.Model.Name = "Task";

        var invalidResult = new Result<NoteTypeDto>();
        invalidResult.AddErrorMessage("Note type \"Task\" already exists");
        service.NoteTypesFake.SaveAsyncImpl = _ => Task.FromResult(invalidResult);

        var saved = await ctrl.SaveModel();

        Assert.IsFalse(saved);
        Assert.AreEqual("Note type \"Task\" already exists", view.LastShownInfo);
    }

    [TestMethod]
    public async Task SaveModel_ServiceThrowsWrappedException_ShowsRootExceptionMessage()
    {
        var (ctrl, view, service) = CreateCtrl();
        await ctrl.NewModel(service);
        ctrl.Model.Name = "Task";

        // Same wrapping chain as other services: KntServiceBase.ExecuteCommand wraps the real DB
        // error (a UNIQUE constraint violation on NoteType.Name) into a generic service exception.
        service.NoteTypesFake.SaveAsyncImpl = _ => throw new Exception(
            "KNote service error. (KntNoteTypeSaveAsyncCommand). ",
            new Exception("UNIQUE constraint failed: NoteTypes.Name"));

        var saved = await ctrl.SaveModel();

        Assert.IsFalse(saved);
        Assert.AreEqual("UNIQUE constraint failed: NoteTypes.Name", view.LastShownInfo);
    }

    [TestMethod]
    public async Task DeleteModel_Confirmed_CallsDeleteAsync_ReturnsTrue_FiresDeletedEntity()
    {
        var (ctrl, view, service) = CreateCtrl();
        view.NextShowInfoResult = DialogResult.Yes;
        var id = Guid.NewGuid();
        service.NoteTypesFake.DeleteAsyncImpl = _ => Task.FromResult(new Result<NoteTypeDto>(new NoteTypeDto { NoteTypeId = id }));
        NoteTypeDto? deletedEntity = null;
        ctrl.DeletedEntity += (s, e) => deletedEntity = e.Entity;

        var deleted = await ctrl.DeleteModel(service, id);

        Assert.IsTrue(deleted);
        Assert.IsNotNull(deletedEntity);
    }

    [TestMethod]
    public async Task DeleteModel_ServiceRejectsBusinessRule_ReturnsFalseAndShowsErrorMessage()
    {
        var (ctrl, view, service) = CreateCtrl();
        view.NextShowInfoResult = DialogResult.Yes;

        // The "still in use by notes" business rule lives in the Service layer now
        // (KntNoteTypeDeleteAsyncCommand), shared with Server/Blazor - ClientWin just needs to
        // surface whatever Result.ErrorMessage the service comes back with.
        var invalidResult = new Result<NoteTypeDto>();
        invalidResult.AddErrorMessage("Can't delete this note type: 2 note(s) still use it.");
        service.NoteTypesFake.DeleteAsyncImpl = _ => Task.FromResult(invalidResult);

        var deleted = await ctrl.DeleteModel(service, Guid.NewGuid());

        Assert.IsFalse(deleted);
        Assert.AreEqual("Can't delete this note type: 2 note(s) still use it.", view.LastShownInfo);
    }

    [TestMethod]
    public async Task DeleteModel_NotConfirmed_DoesNotCallDeleteAsync_ReturnsFalse()
    {
        var (ctrl, view, service) = CreateCtrl();
        view.NextShowInfoResult = DialogResult.No;
        var deleteCalled = false;
        service.NoteTypesFake.DeleteAsyncImpl = id => { deleteCalled = true; return Task.FromResult(new Result<NoteTypeDto>(new NoteTypeDto { NoteTypeId = id })); };

        var deleted = await ctrl.DeleteModel(service, Guid.NewGuid());

        Assert.IsFalse(deleted);
        Assert.IsFalse(deleteCalled);
    }

    [TestMethod]
    public async Task DeleteModel_ServiceThrowsWrappedException_ShowsRootExceptionMessage()
    {
        var (ctrl, view, service) = CreateCtrl();
        view.NextShowInfoResult = DialogResult.Yes;

        // A note type still referenced by KAttributes fails at the DB level (FK constraint) rather
        // than being pre-checked, wrapped the same way as the save path.
        service.NoteTypesFake.DeleteAsyncImpl = _ => throw new Exception(
            "KNote service error. (KntNoteTypeDeleteAsyncCommand). ",
            new Exception("FOREIGN KEY constraint failed"));

        var deleted = await ctrl.DeleteModel(service, Guid.NewGuid());

        Assert.IsFalse(deleted);
        Assert.AreEqual("FOREIGN KEY constraint failed", view.LastShownInfo);
    }
}

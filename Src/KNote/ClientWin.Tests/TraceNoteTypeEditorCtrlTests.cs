using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Tests for TraceNoteTypeEditorCtrl: the single trace note type add/edit popup used by
/// TraceNoteTypesManageCtrl (repository administration - Trace note types tab). Every save/delete
/// persists immediately (AutoDBSave stays true, the CtrlEditorBase default) since there is no
/// parent "Save" to stage into. Mirrors NoteTypeEditorCtrlTests.
/// </summary>
[TestClass]
public class TraceNoteTypeEditorCtrlTests
{
    private static (TraceNoteTypeEditorCtrl ctrl, FakeTraceNoteTypeEditorView view, FakeKntService service) CreateCtrl()
    {
        var factoryViews = new TestFactoryViews();
        var view = new FakeTraceNoteTypeEditorView();
        factoryViews.Registry.Register<TraceNoteTypeEditorCtrl, IViewEditor<TraceNoteTypeDto>>(c => view);

        var store = new Store(factoryViews) { AppUserName = "jdoe" };
        var ctrl = new TraceNoteTypeEditorCtrl(store);
        var service = new FakeKntService();

        return (ctrl, view, service);
    }

    [TestMethod]
    public async Task NewModel_CreatesEmptyModel()
    {
        var (ctrl, _, service) = CreateCtrl();

        await ctrl.NewModel(service);

        Assert.AreEqual(Guid.Empty, ctrl.Model.TraceNoteTypeId);
    }

    [TestMethod]
    public async Task SaveModel_NotDirty_ReturnsTrueWithoutCallingService()
    {
        var (ctrl, _, service) = CreateCtrl();
        await ctrl.NewModel(service);
        // Model is untouched (not dirty): SaveModel must short-circuit before calling the service.

        var saveCalled = false;
        service.TraceNoteTypesFake.SaveAsyncImpl = e => { saveCalled = true; return Task.FromResult(new Result<TraceNoteTypeDto>(e)); };

        var saved = await ctrl.SaveModel();

        Assert.IsTrue(saved);
        Assert.IsFalse(saveCalled);
    }

    [TestMethod]
    public async Task SaveModel_NewTraceNoteType_CallsSaveAsync_ReturnsTrue_FiresAddedEntity()
    {
        var (ctrl, _, service) = CreateCtrl();
        await ctrl.NewModel(service);
        ctrl.Model.Name = "Blocks";
        ctrl.Model.Description = "A relation of type blocks";

        service.TraceNoteTypesFake.SaveAsyncImpl = e =>
        {
            e.TraceNoteTypeId = Guid.NewGuid();
            return Task.FromResult(new Result<TraceNoteTypeDto>(e));
        };
        TraceNoteTypeDto? addedEntity = null;
        ctrl.AddedEntity += (s, e) => addedEntity = e.Entity;

        var saved = await ctrl.SaveModel();

        Assert.IsTrue(saved);
        Assert.IsNotNull(addedEntity);
        Assert.AreEqual("Blocks", addedEntity!.Name);
        Assert.AreNotEqual(Guid.Empty, addedEntity.TraceNoteTypeId);
    }

    [TestMethod]
    public async Task SaveModel_ExistingTraceNoteType_CallsSaveAsync_ReturnsTrue_FiresSavedEntity_NotAddedEntity()
    {
        var (ctrl, _, service) = CreateCtrl();
        var existingId = Guid.NewGuid();
        service.TraceNoteTypesFake.GetAsyncImpl = _ => Task.FromResult(new Result<TraceNoteTypeDto>(new TraceNoteTypeDto { TraceNoteTypeId = existingId, Name = "Blocks" }));
        await ctrl.LoadModelById(service, existingId, false);
        ctrl.Model.Description = "Updated description";

        service.TraceNoteTypesFake.SaveAsyncImpl = e => Task.FromResult(new Result<TraceNoteTypeDto>(e));
        TraceNoteTypeDto? savedEntity = null;
        TraceNoteTypeDto? addedEntity = null;
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
        ctrl.Model.Name = "Blocks";

        var invalidResult = new Result<TraceNoteTypeDto>();
        invalidResult.AddErrorMessage("Trace note type \"Blocks\" already exists");
        service.TraceNoteTypesFake.SaveAsyncImpl = _ => Task.FromResult(invalidResult);

        var saved = await ctrl.SaveModel();

        Assert.IsFalse(saved);
        Assert.AreEqual("Trace note type \"Blocks\" already exists", view.LastShownInfo);
    }

    [TestMethod]
    public async Task SaveModel_ServiceThrowsWrappedException_ShowsRootExceptionMessage()
    {
        var (ctrl, view, service) = CreateCtrl();
        await ctrl.NewModel(service);
        ctrl.Model.Name = "Blocks";

        // Same wrapping chain as other services: KntServiceBase.ExecuteCommand wraps the real DB
        // error (a UNIQUE constraint violation on TraceNoteType.Name) into a generic service exception.
        service.TraceNoteTypesFake.SaveAsyncImpl = _ => throw new Exception(
            "KNote service error. (KntTraceNoteTypeSaveAsyncCommand). ",
            new Exception("UNIQUE constraint failed: TraceNoteTypes.Name"));

        var saved = await ctrl.SaveModel();

        Assert.IsFalse(saved);
        Assert.AreEqual("UNIQUE constraint failed: TraceNoteTypes.Name", view.LastShownInfo);
    }

    [TestMethod]
    public async Task DeleteModel_Confirmed_CallsDeleteAsync_ReturnsTrue_FiresDeletedEntity()
    {
        var (ctrl, view, service) = CreateCtrl();
        view.NextShowInfoResult = DialogResult.Yes;
        var id = Guid.NewGuid();
        service.TraceNoteTypesFake.DeleteAsyncImpl = _ => Task.FromResult(new Result<TraceNoteTypeDto>(new TraceNoteTypeDto { TraceNoteTypeId = id }));
        TraceNoteTypeDto? deletedEntity = null;
        ctrl.DeletedEntity += (s, e) => deletedEntity = e.Entity;

        var deleted = await ctrl.DeleteModel(service, id);

        Assert.IsTrue(deleted);
        Assert.IsNotNull(deletedEntity);
    }

    [TestMethod]
    public async Task DeleteModel_NotConfirmed_DoesNotCallDeleteAsync_ReturnsFalse()
    {
        var (ctrl, view, service) = CreateCtrl();
        view.NextShowInfoResult = DialogResult.No;
        var deleteCalled = false;
        service.TraceNoteTypesFake.DeleteAsyncImpl = id => { deleteCalled = true; return Task.FromResult(new Result<TraceNoteTypeDto>(new TraceNoteTypeDto { TraceNoteTypeId = id })); };

        var deleted = await ctrl.DeleteModel(service, Guid.NewGuid());

        Assert.IsFalse(deleted);
        Assert.IsFalse(deleteCalled);
    }

    [TestMethod]
    public async Task DeleteModel_ServiceThrowsWrappedException_ShowsRootExceptionMessage()
    {
        var (ctrl, view, service) = CreateCtrl();
        view.NextShowInfoResult = DialogResult.Yes;

        // A trace note type still referenced by TraceNotes fails at the DB level (FK constraint)
        // rather than being pre-checked - same unwrap as SaveModel, mirroring
        // NoteTypeEditorCtrlTests' equivalent for a NoteType still used by KAttributes.
        service.TraceNoteTypesFake.DeleteAsyncImpl = _ => throw new Exception(
            "KNote service error. (KntTraceNoteTypeDeleteAsyncCommand). ",
            new Exception("FOREIGN KEY constraint failed"));

        var deleted = await ctrl.DeleteModel(service, Guid.NewGuid());

        Assert.IsFalse(deleted);
        Assert.AreEqual("FOREIGN KEY constraint failed", view.LastShownInfo);
    }
}

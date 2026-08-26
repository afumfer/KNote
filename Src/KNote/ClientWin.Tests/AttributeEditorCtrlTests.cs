using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Tests for AttributeEditorCtrl: the single attribute add/edit popup used by
/// KAttributesManageCtrl (repository administration - Attributes tab), including its nested,
/// staged (AutoDBSave=false) tabulated-value sub-editing (NewTabulatedValue/EditTabulatedValue/
/// DeleteTabulatedValue), which mirrors NoteEditorCtrl's Alarms/Tasks handling.
/// </summary>
[TestClass]
public class AttributeEditorCtrlTests
{
    private static (AttributeEditorCtrl ctrl, FakeAttributeEditorView view, FakeKntService service) CreateCtrl(
        Func<KAttributeTabulatedValueEditorCtrl, Result<EControllerResult>>? tabValueShowModalViewImpl = null)
    {
        var factoryViews = new TestFactoryViews();
        var view = new FakeAttributeEditorView();
        factoryViews.Registry.Register<AttributeEditorCtrl, IViewEditor<KAttributeDto>>(c => view);
        factoryViews.Registry.Register<KAttributeTabulatedValueEditorCtrl, IViewEditor<KAttributeTabulatedValueDto>>(c => new FakeKAttributeTabulatedValueEditorView
        {
            ShowModalViewImpl = tabValueShowModalViewImpl != null ? () => tabValueShowModalViewImpl(c) : null
        });

        var store = new Store(factoryViews) { AppUserName = "jdoe" };
        var ctrl = new AttributeEditorCtrl(store);
        var service = new FakeKntService();

        return (ctrl, view, service);
    }

    [TestMethod]
    public async Task NewModel_CreatesEmptyModel_AndLoadsNoteTypes()
    {
        var (ctrl, _, service) = CreateCtrl();
        var noteTypes = new List<NoteTypeDto> { new() { Name = "Task" } };
        service.NoteTypesFake.GetAllAsyncImpl = () => Task.FromResult(new Result<List<NoteTypeDto>>(noteTypes));

        await ctrl.NewModel(service);

        Assert.AreEqual(Guid.Empty, ctrl.Model.KAttributeId);
        Assert.AreEqual(1, ctrl.NoteTypes.Count);
    }

    [TestMethod]
    public async Task SaveModel_NotDirty_ReturnsTrueWithoutCallingService()
    {
        var (ctrl, _, service) = CreateCtrl();
        service.NoteTypesFake.GetAllAsyncImpl = () => Task.FromResult(new Result<List<NoteTypeDto>>(new List<NoteTypeDto>()));
        await ctrl.NewModel(service);

        var saveCalled = false;
        service.KAttributesFake.SaveAsyncImpl = e => { saveCalled = true; return Task.FromResult(new Result<KAttributeDto>(e)); };

        var saved = await ctrl.SaveModel();

        Assert.IsTrue(saved);
        Assert.IsFalse(saveCalled);
    }

    [TestMethod]
    public async Task SaveModel_NewAttribute_CallsSaveAsync_ReturnsTrue_FiresAddedEntity()
    {
        var (ctrl, _, service) = CreateCtrl();
        service.NoteTypesFake.GetAllAsyncImpl = () => Task.FromResult(new Result<List<NoteTypeDto>>(new List<NoteTypeDto>()));
        await ctrl.NewModel(service);
        ctrl.Model.Name = "Priority";

        service.KAttributesFake.SaveAsyncImpl = e =>
        {
            e.KAttributeId = Guid.NewGuid();
            return Task.FromResult(new Result<KAttributeDto>(e));
        };
        KAttributeDto? addedEntity = null;
        ctrl.AddedEntity += (s, e) => addedEntity = e.Entity;

        var saved = await ctrl.SaveModel();

        Assert.IsTrue(saved);
        Assert.IsNotNull(addedEntity);
        Assert.AreEqual("Priority", addedEntity!.Name);
    }

    [TestMethod]
    public async Task SaveModel_ExistingAttribute_FiresSavedEntity_NotAddedEntity()
    {
        var (ctrl, _, service) = CreateCtrl();
        var existingId = Guid.NewGuid();
        service.NoteTypesFake.GetAllAsyncImpl = () => Task.FromResult(new Result<List<NoteTypeDto>>(new List<NoteTypeDto>()));
        service.KAttributesFake.GetAsyncImpl = _ => Task.FromResult(new Result<KAttributeDto>(new KAttributeDto { KAttributeId = existingId, Name = "Priority" }));
        await ctrl.LoadModelById(service, existingId, false);
        ctrl.Model.Description = "Updated";

        service.KAttributesFake.SaveAsyncImpl = e => Task.FromResult(new Result<KAttributeDto>(e));
        KAttributeDto? savedEntity = null;
        KAttributeDto? addedEntity = null;
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
        service.NoteTypesFake.GetAllAsyncImpl = () => Task.FromResult(new Result<List<NoteTypeDto>>(new List<NoteTypeDto>()));
        await ctrl.NewModel(service);
        ctrl.Model.Name = "Priority";

        var invalidResult = new Result<KAttributeDto>();
        invalidResult.AddErrorMessage("Attribute \"Priority\" already exists");
        service.KAttributesFake.SaveAsyncImpl = _ => Task.FromResult(invalidResult);

        var saved = await ctrl.SaveModel();

        Assert.IsFalse(saved);
        Assert.AreEqual("Attribute \"Priority\" already exists", view.LastShownInfo);
    }

    [TestMethod]
    public async Task SaveModel_ServiceThrowsWrappedException_ShowsRootExceptionMessage()
    {
        var (ctrl, view, service) = CreateCtrl();
        service.NoteTypesFake.GetAllAsyncImpl = () => Task.FromResult(new Result<List<NoteTypeDto>>(new List<NoteTypeDto>()));
        await ctrl.NewModel(service);
        ctrl.Model.Name = "Priority";

        service.KAttributesFake.SaveAsyncImpl = _ => throw new Exception(
            "KNote service error. (KntKAttributesSaveAsyncCommand). ",
            new Exception("UNIQUE constraint failed: KAttributes.Name"));

        var saved = await ctrl.SaveModel();

        Assert.IsFalse(saved);
        Assert.AreEqual("UNIQUE constraint failed: KAttributes.Name", view.LastShownInfo);
    }

    [TestMethod]
    public async Task DeleteModel_Confirmed_CallsDeleteAsync_ReturnsTrue_FiresDeletedEntity()
    {
        var (ctrl, view, service) = CreateCtrl();
        view.NextShowInfoResult = DialogResult.Yes;
        var id = Guid.NewGuid();
        service.KAttributesFake.DeleteAsyncImpl = _ => Task.FromResult(new Result<KAttributeInfoDto>(new KAttributeInfoDto { KAttributeId = id, Name = "Priority" }));
        KAttributeDto? deletedEntity = null;
        ctrl.DeletedEntity += (s, e) => deletedEntity = e.Entity;

        var deleted = await ctrl.DeleteModel(service, id);

        Assert.IsTrue(deleted);
        Assert.IsNotNull(deletedEntity);
        Assert.AreEqual("Priority", deletedEntity!.Name);
    }

    [TestMethod]
    public async Task DeleteModel_NotConfirmed_DoesNotCallDeleteAsync_ReturnsFalse()
    {
        var (ctrl, view, service) = CreateCtrl();
        view.NextShowInfoResult = DialogResult.No;
        var deleteCalled = false;
        service.KAttributesFake.DeleteAsyncImpl = id => { deleteCalled = true; return Task.FromResult(new Result<KAttributeInfoDto>(new KAttributeInfoDto { KAttributeId = id })); };

        var deleted = await ctrl.DeleteModel(service, Guid.NewGuid());

        Assert.IsFalse(deleted);
        Assert.IsFalse(deleteCalled);
    }

    [TestMethod]
    public async Task DeleteModel_ServiceRejectsBusinessRule_ReturnsFalseAndShowsErrorMessage()
    {
        var (ctrl, view, service) = CreateCtrl();
        view.NextShowInfoResult = DialogResult.Yes;

        // The "still in use by notes" business rule lives in the Service layer
        // (KntKAttributesDeleteAsyncCommand), shared with Server/Blazor - ClientWin just needs to
        // surface whatever Result.ErrorMessage the service comes back with.
        var invalidResult = new Result<KAttributeInfoDto>();
        invalidResult.AddErrorMessage("Can't delete this attribute: 2 note(s) still use it.");
        service.KAttributesFake.DeleteAsyncImpl = _ => Task.FromResult(invalidResult);

        var deleted = await ctrl.DeleteModel(service, Guid.NewGuid());

        Assert.IsFalse(deleted);
        Assert.AreEqual("Can't delete this attribute: 2 note(s) still use it.", view.LastShownInfo);
    }

    [TestMethod]
    public async Task DeleteModel_ServiceThrowsWrappedException_ShowsRootExceptionMessage()
    {
        var (ctrl, view, service) = CreateCtrl();
        view.NextShowInfoResult = DialogResult.Yes;

        // An attribute still referenced elsewhere (a scenario other than "in use by notes", which
        // is now pre-checked as a business rule) fails at the DB level rather than being caught.
        service.KAttributesFake.DeleteAsyncImpl = _ => throw new Exception(
            "KNote service error. (KntKAttributesDeleteAsyncCommand). ",
            new Exception("FOREIGN KEY constraint failed"));

        var deleted = await ctrl.DeleteModel(service, Guid.NewGuid());

        Assert.IsFalse(deleted);
        Assert.AreEqual("FOREIGN KEY constraint failed", view.LastShownInfo);
    }

    [TestMethod]
    public async Task NewTabulatedValue_EditorExecuted_AddsToModelKAttributeValues()
    {
        var (ctrl, _, service) = CreateCtrl(tabValueCtrl =>
        {
            tabValueCtrl.Model.Value = "High";
            return new Result<EControllerResult>(EControllerResult.Executed);
        });
        service.NoteTypesFake.GetAllAsyncImpl = () => Task.FromResult(new Result<List<NoteTypeDto>>(new List<NoteTypeDto>()));
        await ctrl.NewModel(service);

        var added = await ctrl.NewTabulatedValue();

        Assert.IsNotNull(added);
        Assert.AreEqual("High", added!.Value);
        Assert.AreEqual(1, ctrl.Model.KAttributeValues.Count);
    }

    [TestMethod]
    public async Task NewTabulatedValue_EditorCanceled_ReturnsNull_DoesNotAddToList()
    {
        var (ctrl, _, service) = CreateCtrl(_ => new Result<EControllerResult>(EControllerResult.Canceled));
        service.NoteTypesFake.GetAllAsyncImpl = () => Task.FromResult(new Result<List<NoteTypeDto>>(new List<NoteTypeDto>()));
        await ctrl.NewModel(service);

        var added = await ctrl.NewTabulatedValue();

        Assert.IsNull(added);
        Assert.AreEqual(0, ctrl.Model.KAttributeValues.Count);
    }

    [TestMethod]
    public async Task EditTabulatedValue_EditorExecuted_UpdatesValueInPlace()
    {
        var (ctrl, _, service) = CreateCtrl(tabValueCtrl =>
        {
            tabValueCtrl.Model.Value = "Updated";
            return new Result<EControllerResult>(EControllerResult.Executed);
        });
        service.NoteTypesFake.GetAllAsyncImpl = () => Task.FromResult(new Result<List<NoteTypeDto>>(new List<NoteTypeDto>()));
        await ctrl.NewModel(service);
        var existingValue = new KAttributeTabulatedValueDto { KAttributeTabulatedValueId = Guid.NewGuid(), Value = "Original" };
        ctrl.Model.KAttributeValues.Add(existingValue);

        var edited = ctrl.EditTabulatedValue(existingValue.KAttributeTabulatedValueId);

        Assert.IsNotNull(edited);
        Assert.AreEqual("Updated", edited!.Value);
        Assert.AreEqual(1, ctrl.Model.KAttributeValues.Count);
    }

    [TestMethod]
    public async Task DeleteTabulatedValue_RemovesFromModelKAttributeValues()
    {
        var (ctrl, _, service) = CreateCtrl();
        service.NoteTypesFake.GetAllAsyncImpl = () => Task.FromResult(new Result<List<NoteTypeDto>>(new List<NoteTypeDto>()));
        await ctrl.NewModel(service);
        var existingValue = new KAttributeTabulatedValueDto { KAttributeTabulatedValueId = Guid.NewGuid(), Value = "Original" };
        ctrl.Model.KAttributeValues.Add(existingValue);

        var deleted = ctrl.DeleteTabulatedValue(existingValue.KAttributeTabulatedValueId);

        Assert.IsTrue(deleted);
        Assert.AreEqual(0, ctrl.Model.KAttributeValues.Count);
    }
}

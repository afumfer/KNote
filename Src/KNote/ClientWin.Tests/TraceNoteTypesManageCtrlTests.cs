using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Tests for TraceNoteTypesManageCtrl: the Trace note types tab of the repository administration
/// screen (RepositoryEditorCtrl). Covers the list load and the orchestration of the
/// TraceNoteTypeEditorCtrl popup for add/edit/delete - TraceNoteTypeEditorCtrl's own save/delete
/// logic is covered separately in TraceNoteTypeEditorCtrlTests. Mirrors NoteTypesManageCtrlTests.
/// </summary>
[TestClass]
public class TraceNoteTypesManageCtrlTests
{
    private static (TraceNoteTypesManageCtrl ctrl, FakeTraceNoteTypesManageView view, FakeKntService service) CreateCtrl(
        Func<TraceNoteTypeEditorCtrl, Result<EControllerResult>>? editorShowModalViewImpl = null)
    {
        var factoryViews = new TestFactoryViews();
        var manageView = new FakeTraceNoteTypesManageView();
        factoryViews.Registry.Register<TraceNoteTypesManageCtrl, IViewManageList<TraceNoteTypeDto>>(c => manageView);
        factoryViews.Registry.Register<TraceNoteTypeEditorCtrl, IViewEditor<TraceNoteTypeDto>>(c => new FakeTraceNoteTypeEditorView
        {
            ShowModalViewImpl = editorShowModalViewImpl != null ? () => editorShowModalViewImpl(c) : null
        });

        var store = new Store(factoryViews) { AppUserName = "jdoe" };
        var ctrl = new TraceNoteTypesManageCtrl(store);
        var service = new FakeKntService();

        return (ctrl, manageView, service);
    }

    [TestMethod]
    public async Task LoadEntitiesAsync_Success_PopulatesListEntities_CallsRefreshView()
    {
        var (ctrl, view, service) = CreateCtrl();
        var types = new List<TraceNoteTypeDto> { new() { Name = "Blocks" }, new() { Name = "Relates to" } };
        service.TraceNoteTypesFake.GetAllAsyncImpl = () => Task.FromResult(new Result<List<TraceNoteTypeDto>>(types));

        var loaded = await ctrl.LoadEntitiesAsync(service);

        Assert.IsTrue(loaded);
        Assert.AreEqual(2, ctrl.ListEntities.Count);
        Assert.IsTrue(view.RefreshViewCalled);
    }

    [TestMethod]
    public async Task LoadEntitiesAsync_InvalidResult_ReturnsFalseAndShowsError()
    {
        var (ctrl, view, service) = CreateCtrl();
        var invalidResult = new Result<List<TraceNoteTypeDto>>();
        invalidResult.AddErrorMessage("DB unavailable");
        service.TraceNoteTypesFake.GetAllAsyncImpl = () => Task.FromResult(invalidResult);

        var loaded = await ctrl.LoadEntitiesAsync(service);

        Assert.IsFalse(loaded);
        Assert.AreEqual("DB unavailable", view.LastShownInfo);
    }

    [TestMethod]
    public async Task AddItemAsync_EditorExecuted_AddsToListEntitiesAndView()
    {
        var (ctrl, view, service) = CreateCtrl(editorCtrl =>
        {
            editorCtrl.Model.Name = "Blocks";
            return new Result<EControllerResult>(EControllerResult.Executed);
        });
        service.TraceNoteTypesFake.GetAllAsyncImpl = () => Task.FromResult(new Result<List<TraceNoteTypeDto>>(new List<TraceNoteTypeDto>()));
        await ctrl.LoadEntitiesAsync(service);

        var added = await ctrl.AddItemAsync();

        Assert.IsTrue(added);
        Assert.AreEqual(1, ctrl.ListEntities.Count);
        Assert.AreEqual("Blocks", ctrl.ListEntities[0].Name);
        Assert.AreEqual(1, view.AddedItems.Count);
    }

    [TestMethod]
    public async Task AddItemAsync_EditorCanceled_DoesNotAddToListOrView()
    {
        var (ctrl, view, service) = CreateCtrl(_ => new Result<EControllerResult>(EControllerResult.Canceled));
        service.TraceNoteTypesFake.GetAllAsyncImpl = () => Task.FromResult(new Result<List<TraceNoteTypeDto>>(new List<TraceNoteTypeDto>()));
        await ctrl.LoadEntitiesAsync(service);

        var added = await ctrl.AddItemAsync();

        Assert.IsFalse(added);
        Assert.AreEqual(0, ctrl.ListEntities.Count);
        Assert.AreEqual(0, view.AddedItems.Count);
    }

    [TestMethod]
    public async Task EditItemAsync_EditorExecuted_ReplacesInListEntitiesAndCallsViewUpdateItem()
    {
        var existingId = Guid.NewGuid();
        var existing = new TraceNoteTypeDto { TraceNoteTypeId = existingId, Name = "Blocks" };

        var (ctrl, view, service) = CreateCtrl(editorCtrl =>
        {
            editorCtrl.Model.Description = "Updated";
            return new Result<EControllerResult>(EControllerResult.Executed);
        });
        service.TraceNoteTypesFake.GetAllAsyncImpl = () => Task.FromResult(new Result<List<TraceNoteTypeDto>>(new List<TraceNoteTypeDto> { existing }));
        service.TraceNoteTypesFake.GetAsyncImpl = id => Task.FromResult(new Result<TraceNoteTypeDto>(new TraceNoteTypeDto { TraceNoteTypeId = id, Name = "Blocks" }));
        await ctrl.LoadEntitiesAsync(service);

        var edited = await ctrl.EditItemAsync(existing);

        Assert.IsTrue(edited);
        Assert.AreEqual(1, ctrl.ListEntities.Count);
        Assert.AreEqual("Updated", ctrl.ListEntities[0].Description);
        Assert.AreEqual(1, view.UpdatedItems.Count);
    }

    [TestMethod]
    public async Task DeleteItemAsync_Confirmed_RemovesFromListEntitiesAndCallsViewRemoveItem()
    {
        var existingId = Guid.NewGuid();
        var existing = new TraceNoteTypeDto { TraceNoteTypeId = existingId, Name = "Blocks" };

        var (ctrl, view, service) = CreateCtrl();
        service.TraceNoteTypesFake.GetAllAsyncImpl = () => Task.FromResult(new Result<List<TraceNoteTypeDto>>(new List<TraceNoteTypeDto> { existing }));
        service.TraceNoteTypesFake.DeleteAsyncImpl = id => Task.FromResult(new Result<TraceNoteTypeDto>(new TraceNoteTypeDto { TraceNoteTypeId = id }));
        await ctrl.LoadEntitiesAsync(service);

        var deleted = await ctrl.DeleteItemAsync(existing);

        Assert.IsTrue(deleted);
        Assert.AreEqual(0, ctrl.ListEntities.Count);
        Assert.AreEqual(1, view.RemovedItems.Count);
    }

    [TestMethod]
    public async Task AddItemAsync_EditorExecuted_FiresListChanged()
    {
        var (ctrl, _, service) = CreateCtrl(editorCtrl =>
        {
            editorCtrl.Model.Name = "Blocks";
            return new Result<EControllerResult>(EControllerResult.Executed);
        });
        service.TraceNoteTypesFake.GetAllAsyncImpl = () => Task.FromResult(new Result<List<TraceNoteTypeDto>>(new List<TraceNoteTypeDto>()));
        await ctrl.LoadEntitiesAsync(service);
        var listChangedRaised = false;
        ctrl.ListChanged += (s, e) => listChangedRaised = true;

        await ctrl.AddItemAsync();

        Assert.IsTrue(listChangedRaised);
    }

    [TestMethod]
    public async Task AddItemAsync_EditorCanceled_DoesNotFireListChanged()
    {
        var (ctrl, _, service) = CreateCtrl(_ => new Result<EControllerResult>(EControllerResult.Canceled));
        service.TraceNoteTypesFake.GetAllAsyncImpl = () => Task.FromResult(new Result<List<TraceNoteTypeDto>>(new List<TraceNoteTypeDto>()));
        await ctrl.LoadEntitiesAsync(service);
        var listChangedRaised = false;
        ctrl.ListChanged += (s, e) => listChangedRaised = true;

        await ctrl.AddItemAsync();

        Assert.IsFalse(listChangedRaised);
    }

    [TestMethod]
    public async Task DeleteItemAsync_Confirmed_FiresListChanged()
    {
        var existingId = Guid.NewGuid();
        var existing = new TraceNoteTypeDto { TraceNoteTypeId = existingId, Name = "Blocks" };

        var (ctrl, _, service) = CreateCtrl();
        service.TraceNoteTypesFake.GetAllAsyncImpl = () => Task.FromResult(new Result<List<TraceNoteTypeDto>>(new List<TraceNoteTypeDto> { existing }));
        service.TraceNoteTypesFake.DeleteAsyncImpl = id => Task.FromResult(new Result<TraceNoteTypeDto>(new TraceNoteTypeDto { TraceNoteTypeId = id }));
        await ctrl.LoadEntitiesAsync(service);
        var listChangedRaised = false;
        ctrl.ListChanged += (s, e) => listChangedRaised = true;

        await ctrl.DeleteItemAsync(existing);

        Assert.IsTrue(listChangedRaised);
    }
}

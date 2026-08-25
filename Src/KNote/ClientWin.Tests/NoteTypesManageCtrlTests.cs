using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Tests for NoteTypesManageCtrl: the Note types tab of the repository administration screen
/// (RepositoryEditorCtrl). Covers the list load and the orchestration of the NoteTypeEditorCtrl
/// popup for add/edit/delete - NoteTypeEditorCtrl's own save/delete logic is covered separately in
/// NoteTypeEditorCtrlTests.
/// </summary>
[TestClass]
public class NoteTypesManageCtrlTests
{
    private static (NoteTypesManageCtrl ctrl, FakeNoteTypesManageView view, FakeKntService service) CreateCtrl(
        Func<NoteTypeEditorCtrl, Result<EControllerResult>>? editorShowModalViewImpl = null)
    {
        var factoryViews = new TestFactoryViews();
        var manageView = new FakeNoteTypesManageView();
        factoryViews.Registry.Register<NoteTypesManageCtrl, IViewManageList<NoteTypeDto>>(c => manageView);
        factoryViews.Registry.Register<NoteTypeEditorCtrl, IViewEditor<NoteTypeDto>>(c => new FakeNoteTypeEditorView
        {
            ShowModalViewImpl = editorShowModalViewImpl != null ? () => editorShowModalViewImpl(c) : null
        });

        var store = new Store(factoryViews) { AppUserName = "jdoe" };
        var ctrl = new NoteTypesManageCtrl(store);
        var service = new FakeKntService();

        return (ctrl, manageView, service);
    }

    [TestMethod]
    public async Task LoadEntitiesAsync_Success_PopulatesListEntities_CallsRefreshView()
    {
        var (ctrl, view, service) = CreateCtrl();
        var types = new List<NoteTypeDto> { new() { Name = "Task" }, new() { Name = "Idea" } };
        service.NoteTypesFake.GetAllAsyncImpl = () => Task.FromResult(new Result<List<NoteTypeDto>>(types));

        var loaded = await ctrl.LoadEntitiesAsync(service);

        Assert.IsTrue(loaded);
        Assert.AreEqual(2, ctrl.ListEntities.Count);
        Assert.IsTrue(view.RefreshViewCalled);
    }

    [TestMethod]
    public async Task LoadEntitiesAsync_InvalidResult_ReturnsFalseAndShowsError()
    {
        var (ctrl, view, service) = CreateCtrl();
        var invalidResult = new Result<List<NoteTypeDto>>();
        invalidResult.AddErrorMessage("DB unavailable");
        service.NoteTypesFake.GetAllAsyncImpl = () => Task.FromResult(invalidResult);

        var loaded = await ctrl.LoadEntitiesAsync(service);

        Assert.IsFalse(loaded);
        Assert.AreEqual("DB unavailable", view.LastShownInfo);
    }

    [TestMethod]
    public async Task AddItemAsync_EditorExecuted_AddsToListEntitiesAndView()
    {
        var (ctrl, view, service) = CreateCtrl(editorCtrl =>
        {
            editorCtrl.Model.Name = "Task";
            return new Result<EControllerResult>(EControllerResult.Executed);
        });
        service.NoteTypesFake.GetAllAsyncImpl = () => Task.FromResult(new Result<List<NoteTypeDto>>(new List<NoteTypeDto>()));
        await ctrl.LoadEntitiesAsync(service);

        var added = await ctrl.AddItemAsync();

        Assert.IsTrue(added);
        Assert.AreEqual(1, ctrl.ListEntities.Count);
        Assert.AreEqual("Task", ctrl.ListEntities[0].Name);
        Assert.AreEqual(1, view.AddedItems.Count);
    }

    [TestMethod]
    public async Task AddItemAsync_EditorCanceled_DoesNotAddToListOrView()
    {
        var (ctrl, view, service) = CreateCtrl(_ => new Result<EControllerResult>(EControllerResult.Canceled));
        service.NoteTypesFake.GetAllAsyncImpl = () => Task.FromResult(new Result<List<NoteTypeDto>>(new List<NoteTypeDto>()));
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
        var existing = new NoteTypeDto { NoteTypeId = existingId, Name = "Task" };

        var (ctrl, view, service) = CreateCtrl(editorCtrl =>
        {
            editorCtrl.Model.Description = "Updated";
            return new Result<EControllerResult>(EControllerResult.Executed);
        });
        service.NoteTypesFake.GetAllAsyncImpl = () => Task.FromResult(new Result<List<NoteTypeDto>>(new List<NoteTypeDto> { existing }));
        service.NoteTypesFake.GetAsyncImpl = id => Task.FromResult(new Result<NoteTypeDto>(new NoteTypeDto { NoteTypeId = id, Name = "Task" }));
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
        var existing = new NoteTypeDto { NoteTypeId = existingId, Name = "Task" };

        var (ctrl, view, service) = CreateCtrl();
        service.NoteTypesFake.GetAllAsyncImpl = () => Task.FromResult(new Result<List<NoteTypeDto>>(new List<NoteTypeDto> { existing }));
        service.NoteTypesFake.DeleteAsyncImpl = id => Task.FromResult(new Result<NoteTypeDto>(new NoteTypeDto { NoteTypeId = id }));
        await ctrl.LoadEntitiesAsync(service);

        var deleted = await ctrl.DeleteItemAsync(existing);

        Assert.IsTrue(deleted);
        Assert.AreEqual(0, ctrl.ListEntities.Count);
        Assert.AreEqual(1, view.RemovedItems.Count);
    }
}

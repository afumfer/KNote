using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Tests for KAttributesManageCtrl: the Attributes tab of the repository administration screen
/// (RepositoryEditorCtrl). Covers the list load and the orchestration of the AttributeEditorCtrl
/// popup for add/edit/delete - AttributeEditorCtrl's own save/delete logic is covered separately in
/// AttributeEditorCtrlTests.
/// </summary>
[TestClass]
public class KAttributesManageCtrlTests
{
    private static (KAttributesManageCtrl ctrl, FakeKAttributesManageView view, FakeKntService service) CreateCtrl(
        Func<AttributeEditorCtrl, Result<EControllerResult>>? editorShowModalViewImpl = null)
    {
        var factoryViews = new TestFactoryViews();
        var manageView = new FakeKAttributesManageView();
        factoryViews.Registry.Register<KAttributesManageCtrl, IViewManageList<KAttributeInfoDto>>(c => manageView);
        factoryViews.Registry.Register<AttributeEditorCtrl, IViewEditor<KAttributeDto>>(c => new FakeAttributeEditorView
        {
            ShowModalViewImpl = editorShowModalViewImpl != null ? () => editorShowModalViewImpl(c) : null
        });

        var store = new Store(factoryViews) { AppUserName = "jdoe" };
        var ctrl = new KAttributesManageCtrl(store);
        var service = new FakeKntService();
        // AttributeEditorCtrl.NewModel/LoadModelById always loads the note-type picker list.
        service.NoteTypesFake.GetAllAsyncImpl = () => Task.FromResult(new Result<List<NoteTypeDto>>(new List<NoteTypeDto>()));

        return (ctrl, manageView, service);
    }

    [TestMethod]
    public async Task LoadEntitiesAsync_Success_PopulatesListEntities_CallsRefreshView()
    {
        var (ctrl, view, service) = CreateCtrl();
        var attributes = new List<KAttributeInfoDto> { new() { Name = "Priority" }, new() { Name = "Status" } };
        service.KAttributesFake.GetAllAsyncImpl = () => Task.FromResult(new Result<List<KAttributeInfoDto>>(attributes));

        var loaded = await ctrl.LoadEntitiesAsync(service);

        Assert.IsTrue(loaded);
        Assert.AreEqual(2, ctrl.ListEntities.Count);
        Assert.IsTrue(view.RefreshViewCalled);
    }

    [TestMethod]
    public async Task LoadEntitiesAsync_InvalidResult_ReturnsFalseAndShowsError()
    {
        var (ctrl, view, service) = CreateCtrl();
        var invalidResult = new Result<List<KAttributeInfoDto>>();
        invalidResult.AddErrorMessage("DB unavailable");
        service.KAttributesFake.GetAllAsyncImpl = () => Task.FromResult(invalidResult);

        var loaded = await ctrl.LoadEntitiesAsync(service);

        Assert.IsFalse(loaded);
        Assert.AreEqual("DB unavailable", view.LastShownInfo);
    }

    [TestMethod]
    public async Task AddItemAsync_EditorExecuted_AddsToListEntitiesAndView()
    {
        var (ctrl, view, service) = CreateCtrl(editorCtrl =>
        {
            editorCtrl.Model.Name = "Priority";
            return new Result<EControllerResult>(EControllerResult.Executed);
        });
        service.KAttributesFake.GetAllAsyncImpl = () => Task.FromResult(new Result<List<KAttributeInfoDto>>(new List<KAttributeInfoDto>()));
        await ctrl.LoadEntitiesAsync(service);

        var added = await ctrl.AddItemAsync();

        Assert.IsTrue(added);
        Assert.AreEqual(1, ctrl.ListEntities.Count);
        Assert.AreEqual("Priority", ctrl.ListEntities[0].Name);
        Assert.AreEqual(1, view.AddedItems.Count);
    }

    [TestMethod]
    public async Task AddItemAsync_EditorCanceled_DoesNotAddToListOrView()
    {
        var (ctrl, view, service) = CreateCtrl(_ => new Result<EControllerResult>(EControllerResult.Canceled));
        service.KAttributesFake.GetAllAsyncImpl = () => Task.FromResult(new Result<List<KAttributeInfoDto>>(new List<KAttributeInfoDto>()));
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
        var existing = new KAttributeInfoDto { KAttributeId = existingId, Name = "Priority" };

        var (ctrl, view, service) = CreateCtrl(editorCtrl =>
        {
            editorCtrl.Model.Description = "Updated";
            return new Result<EControllerResult>(EControllerResult.Executed);
        });
        service.KAttributesFake.GetAllAsyncImpl = () => Task.FromResult(new Result<List<KAttributeInfoDto>>(new List<KAttributeInfoDto> { existing }));
        service.KAttributesFake.GetAsyncImpl = id => Task.FromResult(new Result<KAttributeDto>(new KAttributeDto { KAttributeId = id, Name = "Priority" }));
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
        var existing = new KAttributeInfoDto { KAttributeId = existingId, Name = "Priority" };

        var (ctrl, view, service) = CreateCtrl();
        service.KAttributesFake.GetAllAsyncImpl = () => Task.FromResult(new Result<List<KAttributeInfoDto>>(new List<KAttributeInfoDto> { existing }));
        service.KAttributesFake.DeleteAsyncImpl = id => Task.FromResult(new Result<KAttributeInfoDto>(new KAttributeInfoDto { KAttributeId = id }));
        await ctrl.LoadEntitiesAsync(service);

        var deleted = await ctrl.DeleteItemAsync(existing);

        Assert.IsTrue(deleted);
        Assert.AreEqual(0, ctrl.ListEntities.Count);
        Assert.AreEqual(1, view.RemovedItems.Count);
    }
}

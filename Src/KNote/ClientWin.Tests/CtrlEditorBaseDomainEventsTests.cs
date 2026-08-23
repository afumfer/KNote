using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Service.Core;
using System.ComponentModel.DataAnnotations;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Verifies that CtrlEditorBase.OnSavedEntity/OnAddedEntity/OnDeletedEntity publish to
/// Store.Events (Fase 3 of the ClientWin architecture refactor), in addition to raising the
/// existing SavedEntity/AddedEntity/DeletedEntity CLR events untouched by this phase.
/// </summary>
[TestClass]
public class CtrlEditorBaseDomainEventsTests
{
    private class FakeEntity : SmartModelDtoBase
    {
        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext) => [];
    }

    private class FakeView : IViewEditor<FakeEntity>
    {
        public void ShowView() { }
        public Result<EControllerResult> ShowModalView() => new(EControllerResult.Executed);
        public void RefreshView() { }
        public void OnClosingView() { }
        public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information) => DialogResult.OK;
        public void CleanView() { }
        public void RefreshModel() { }
    }

    private class FakeEditorCtrl : CtrlEditorBase<IViewEditor<FakeEntity>, FakeEntity>
    {
        public FakeEditorCtrl(Store store) : base(store) { }

        protected override IViewEditor<FakeEntity> CreateView() => new FakeView();

        public override Task<bool> LoadModelById(IKntService service, Guid id, bool refreshView = true) => Task.FromResult(true);
        public override Task<bool> NewModel(IKntService service) => Task.FromResult(true);
        public override Task<bool> SaveModel() => Task.FromResult(true);
        public override Task<bool> DeleteModel(IKntService service, Guid id) => Task.FromResult(true);
        public override Task<bool> DeleteModel() => Task.FromResult(true);

        public void TriggerSavedEntity(FakeEntity entity) => OnSavedEntity(entity);
        public void TriggerAddedEntity(FakeEntity entity) => OnAddedEntity(entity);
        public void TriggerDeletedEntity(FakeEntity entity) => OnDeletedEntity(entity);
    }

    [TestMethod]
    public void OnSavedEntity_PublishesEntitySavedOnStoreEvents()
    {
        var store = new Store(factoryViews: null!);
        var ctrl = new FakeEditorCtrl(store);
        EntitySaved<FakeEntity>? received = null;
        store.Events.Subscribe<EntitySaved<FakeEntity>>(msg => received = msg);
        var entity = new FakeEntity();

        ctrl.TriggerSavedEntity(entity);

        Assert.IsNotNull(received);
        Assert.AreSame(entity, received.Entity);
    }

    [TestMethod]
    public void OnAddedEntity_PublishesEntityAddedOnStoreEvents()
    {
        var store = new Store(factoryViews: null!);
        var ctrl = new FakeEditorCtrl(store);
        EntityAdded<FakeEntity>? received = null;
        store.Events.Subscribe<EntityAdded<FakeEntity>>(msg => received = msg);
        var entity = new FakeEntity();

        ctrl.TriggerAddedEntity(entity);

        Assert.IsNotNull(received);
        Assert.AreSame(entity, received.Entity);
    }

    [TestMethod]
    public void OnDeletedEntity_PublishesEntityDeletedOnStoreEvents()
    {
        var store = new Store(factoryViews: null!);
        var ctrl = new FakeEditorCtrl(store);
        EntityDeleted<FakeEntity>? received = null;
        store.Events.Subscribe<EntityDeleted<FakeEntity>>(msg => received = msg);
        var entity = new FakeEntity();

        ctrl.TriggerDeletedEntity(entity);

        Assert.IsNotNull(received);
        Assert.AreSame(entity, received.Entity);
    }

    [TestMethod]
    public void OnSavedEntity_AlsoStillRaisesExistingSavedEntityClrEvent()
    {
        // The existing CLR event (used today by Store.AddController's NoteEditorCtrl/
        // PostItEditorCtrl special-casing) must keep working unchanged in this phase.
        var store = new Store(factoryViews: null!);
        var ctrl = new FakeEditorCtrl(store);
        FakeEntity? received = null;
        ctrl.SavedEntity += (_, e) => received = e.Entity;
        var entity = new FakeEntity();

        ctrl.TriggerSavedEntity(entity);

        Assert.AreSame(entity, received);
    }
}

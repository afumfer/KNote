using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

/// <summary>
/// KNoteAIAssistant plan (Phase 4): maintenance screen for Store.AppConfig.AiProviderRefs (the
/// provider/model/apiKey/host collection consumed by KNoteAIAssistantCtrl's provider picker).
/// Unlike NoteTypesManageCtrl/UsersManageCtrl (embedded tabs of RepositoryEditorCtrl, backed by
/// IKntService), this is shown standalone (window mode, the default for CtrlViewEmbeddableBase)
/// straight from the Tools menu, and ListEntities is Store.AppConfig.AiProviderRefs itself (no
/// service call needed to load it).
/// </summary>
public class AiProvidersManageCtrl : CtrlManageListBase<IViewManageList<AiProviderRef>, AiProviderRef>
{
    #region Constructor

    public AiProvidersManageCtrl(Store store) : base(store)
    {
        ControllerName = "AI providers management";
    }

    #endregion

    #region CtrlManageListBase implementation

    protected override IViewManageList<AiProviderRef> CreateView()
    {
        return Store.FactoryViews.Registry.Resolve<AiProvidersManageCtrl, IViewManageList<AiProviderRef>>(this);
    }

    public override Task<bool> LoadEntitiesAsync(IKntService service, bool refreshView = true)
    {
        // AiProviderRefs is not IKntService-backed: `service` is accepted only to satisfy the
        // CtrlManageListBase contract and is otherwise unused.
        ListEntities = Store.AppConfig.AiProviderRefs;

        if (refreshView)
            View.RefreshView();

        return Task.FromResult(true);
    }

    public override async Task<bool> AddItemAsync()
    {
        var editorCtrl = new AiProviderEditorCtrl(Store);
        await editorCtrl.NewModel();

        var res = editorCtrl.RunModal();
        if (res.Entity == EControllerResult.Executed)
        {
            // editorCtrl.SaveModel() already added it to Store.AppConfig.AiProviderRefs, which is
            // the same List<T> instance as ListEntities.
            View.AddItem(editorCtrl.Model);
            OnListChanged();
            return true;
        }
        return false;
    }

    public override Task<bool> EditItemAsync(AiProviderRef item)
    {
        var editorCtrl = new AiProviderEditorCtrl(Store);
        // refreshView must stay true here: same reason as NoteTypesManageCtrl.EditItemAsync - this
        // popup is shown via RunModal() right after, with no other wiring to populate it later.
        editorCtrl.LoadModel(null, item, true);

        var res = editorCtrl.RunModal();
        if (res.Entity == EControllerResult.Executed)
        {
            View.UpdateItem(editorCtrl.Model);
            OnListChanged();
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public override async Task<bool> DeleteItemAsync(AiProviderRef item)
    {
        var editorCtrl = new AiProviderEditorCtrl(Store);
        editorCtrl.LoadModel(null, item, false);
        var deleted = await editorCtrl.DeleteModel();

        if (deleted)
        {
            View.RemoveItem(item);
            OnListChanged();
        }
        return deleted;
    }

    #endregion
}

using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

/// <summary>
/// Single AI provider add/edit popup, used by AiProvidersManageCtrl. Unlike NoteTypeEditorCtrl/
/// RepositoryEditorCtrl, AiProviderRef is not persisted through IKntService nor identified by a
/// Guid: it lives purely in Store.AppConfig.AiProviderRefs (an in-memory List<AiProviderRef>,
/// same reference as AiProvidersManageCtrl.ListEntities) and is saved to KNoteData.config via
/// Store.SaveConfig() - same approach as OptionsEditorCtrl for the rest of AppConfig.
/// </summary>
public class AiProviderEditorCtrl : CtrlEditorBase<IViewEditor<AiProviderRef>, AiProviderRef>
{
    #region Constructor

    public AiProviderEditorCtrl(Store store) : base(store)
    {
        ControllerName = "AI provider editor";
    }

    #endregion

    #region Controller editor implementation

    protected override IViewEditor<AiProviderRef> CreateView()
    {
        return Store.FactoryViews.Registry.Resolve<AiProviderEditorCtrl, IViewEditor<AiProviderRef>>(this);
    }

    // Not applicable: AiProviderRef has no Guid identity. Use LoadModel(null, item, ...) instead.
    public override Task<bool> LoadModelById(IKntService service, Guid id, bool refreshView = true)
    {
        throw new NotImplementedException();
    }

    public override Task<bool> NewModel(IKntService service = null)
    {
        Model = new AiProviderRef();
        return Task.FromResult(true);
    }

    public override Task<bool> SaveModel()
    {
        View.RefreshModel();

        if (!Model.IsDirty())
            return Task.FromResult(true);

        var msgVal = Model.GetErrorMessage();
        if (!string.IsNullOrEmpty(msgVal))
        {
            View.ShowInfo(msgVal);
            return Task.FromResult(false);
        }

        var isNew = !Store.AppConfig.AiProviderRefs.Contains(Model);
        if (isNew)
            Store.AppConfig.AiProviderRefs.Add(Model);

        Store.SaveConfig();
        Model.SetIsDirty(false);

        if (isNew)
            OnAddedEntity(Model);
        else
            OnSavedEntity(Model);

        Finalize();
        return Task.FromResult(true);
    }

    // Not applicable: see LoadModelById. Use DeleteModel() with Model preloaded via LoadModel(...).
    public override Task<bool> DeleteModel(IKntService service, Guid id)
    {
        throw new NotImplementedException();
    }

    public override Task<bool> DeleteModel()
    {
        var result = View.ShowInfo($"Are you sure you want to delete the '{Model.Alias}' AI provider?",
            "Delete AI provider", MessageBoxButtons.YesNo);
        if (result != DialogResult.Yes)
            return Task.FromResult(false);

        Store.AppConfig.AiProviderRefs.Remove(Model);
        Store.SaveConfig();
        OnDeletedEntity(Model);
        return Task.FromResult(true);
    }

    #endregion
}

using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.ClientWin.Components;

namespace KNote.ClientWin.Core;

public abstract class ComponentEditor<TView, TEntity> : ComponentEditorBase<TView, TEntity>
    where TView : IViewConfigurable
    where TEntity : SmartModelDtoBase, new()
{
    public ComponentEditor(Store store) : base(store)
    {

    }

    public virtual FolderInfoDto GetFolder()
    {
        var folderSelector = new FoldersSelectorComponent(Store);
        var services = new List<ServiceRef>();
        services.Add(Store.GetServiceRef(Service.IdServiceRef));
        folderSelector.ServicesRef = services;
        var res = folderSelector.RunModal();
        if (res.Entity == EComponentResult.Executed)
            return folderSelector.SelectedEntity.FolderInfo;

        return null;
    }

}

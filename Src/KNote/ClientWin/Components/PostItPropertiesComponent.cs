using KNote.ClientWin.Core;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Components;

public class PostItPropertiesComponent : ComponentEditor<IEditorView<WindowDto>, WindowDto>
{
    #region Constructor 

    public PostItPropertiesComponent(Store store) : base(store)
    {
        ComponentName = "PostIt properties editor";
    }

    #endregion

    #region IEditorView implementation

    protected override IEditorView<WindowDto> CreateView()
    {
        return Store.FactoryViews.View(this);
    }        

    public async override Task<bool> NewModel(IKntService service)
    {
        Model = new WindowDto();

        return await Task.FromResult<bool>(true);
    }

    public override Task<bool> LoadModelById(IKntService service, Guid id, bool refreshView = true)
    {
        throw new NotImplementedException();
    }


    public async override Task<bool> SaveModel()
    {
        View.RefreshModel();
        Finalize();
        return await Task.FromResult<bool>(true);
    }

    public override Task<bool> DeleteModel(IKntService service, Guid id)
    {
        throw new NotImplementedException();
    }

    public override Task<bool> DeleteModel()
    {
        throw new NotImplementedException();
    }

    #endregion
}

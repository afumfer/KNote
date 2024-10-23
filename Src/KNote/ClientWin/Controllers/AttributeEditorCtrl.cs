using KNote.ClientWin.Core;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

public class AttributeEditorCtrl : CtrlEditorBase<IViewEditor<KAttributeDto>, KAttributeDto>
{
    // TODO: .... for Attribute managment

    #region Constructor 

    public AttributeEditorCtrl(Store store) : base(store)
    {
        ComponentName = "Attribute editor";
    }

    #endregion

    #region Abstract member implementations 

    protected override IViewEditor<KAttributeDto> CreateView()
    {
        return Store.FactoryViews.View(this);
    }

    public override Task<bool> LoadModelById(IKntService service, Guid id, bool refreshView = true)
    {
        throw new NotImplementedException();
    }

    public override Task<bool> NewModel(IKntService service)
    {
        throw new NotImplementedException();
    }

    public override Task<bool> SaveModel()
    {
        throw new NotImplementedException();
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

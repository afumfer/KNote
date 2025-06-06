﻿using KNote.ClientWin.Core;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

public class PostItPropertiesCtrl : CtrlEditorBase<IViewPostIt<WindowDto>, WindowDto>
{
    #region Constructor 

    public PostItPropertiesCtrl(Store store) : base(store)
    {
        ControllerName = "PostIt properties editor";
    }

    #endregion

    #region IEditorView implementation

    protected override IViewPostIt<WindowDto> CreateView()
    {
        return Store.FactoryViews.View(this);
    }        

    public override Task<bool> NewModel(IKntService service)
    {
        Model = new WindowDto();

        return Task.FromResult<bool>(true);
    }

    public override Task<bool> LoadModelById(IKntService service, Guid id, bool refreshView = true)
    {
        throw new NotImplementedException();
    }


    public override Task<bool> SaveModel()
    {
        View.RefreshModel();
        Finalize();
        return Task.FromResult<bool>(true);
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

using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

public class OptionsEditorCtrl : CtrlEditorBase<IViewEditor<OptionsModel>, OptionsModel>
{
    #region Constructor 

    public OptionsEditorCtrl(Store store) : base(store)
    {
        ControllerName = "Options editor";
    }

    #endregion 

    #region Controller editor implementation 

    protected override IViewEditor<OptionsModel> CreateView()
    {
        return Store.FactoryViews.Registry.Resolve<OptionsEditorCtrl, IViewEditor<OptionsModel>>(this);
    }

    public override Task<bool> LoadModelById(IKntService service, Guid id, bool refreshView = true)
    {
        throw new NotImplementedException();
    }

    public override Task<bool> NewModel(IKntService service = null)
    {
        throw new NotImplementedException();
    }

    public override Task<bool> SaveModel()
    {
        View.RefreshModel();

        if (!Model.IsDirty())
            return Task.FromResult(true);

        var msgVal = Model.GetErrorMessage(false);
        if (!string.IsNullOrEmpty(msgVal))
        {
            View.ShowInfo(msgVal);
            return Task.FromResult(false);
        }

        Model.ApplyTo(Store.Settings, Store.State);
        Store.SaveConfig();

        return Task.FromResult(true);
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


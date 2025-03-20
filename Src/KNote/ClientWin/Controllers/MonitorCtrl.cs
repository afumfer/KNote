using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

public class MonitorCtrl : CtrlViewBase<IViewBase>
{
    #region Constructor 

    public MonitorCtrl(Store store) : base(store)
    {
        ControllerName = "KeyNote monitor";
    }

    #endregion

    #region View 

    protected override IViewBase CreateView()
    {
        return Store.FactoryViews.View(this);
    }

    #endregion

    #region Component override methods

    protected override Result<EControllerResult> OnInitialized()
    {
        var result = base.OnInitialized();

        // TODO: pending check result correctrly

        try
        {                                                
            Store.ControllerStateChanged += Store_ComponentsStateChanged;
            Store.AddedServiceRef += Store_AddedServiceRef;                
            Store.RemovedServiceRef += Store_RemovedServiceRef;
            Store.ControllerNotification += Store_ComponentNotification;
        }
        catch (Exception ex)
        {                
            result.AddErrorMessage(ex.Message);
        }

        return result;
    }

    protected override Result<EControllerResult> OnFinalized()
    {
        Result<EControllerResult> result;

        try
        {
            result = base.OnFinalized();
            Store.ControllerStateChanged -= Store_ComponentsStateChanged;
            Store.AddedServiceRef -= Store_AddedServiceRef;                
            Store.RemovedServiceRef -= Store_RemovedServiceRef;                
        }
        catch (Exception ex)
        {
            result = new Result<EControllerResult>(EControllerResult.Error);
            result.AddErrorMessage(ex.Message);                
        }

        return result;
    }

    #endregion 

    #region Store events handlers

    private void Store_ComponentNotification(object sender, ControllerEventArgs<string> e)
    {
        var info = $"{((CtrlBase)sender).ControllerName} - {e.Entity.ToString()}";
        OnShowLog(info);
    }

    private void Store_ComponentsStateChanged(object sender, ControllerEventArgs<EControllerState> e)
    {
        var info = $"{DateTime.Now} - [ControllersStateChanged] - {sender.ToString()} - {e.Entity.ToString()} - {((CtrlBase)sender).ControllerId}";
        OnShowLog(info);
    }
   
    private void Store_RemovedServiceRef(object sender, ControllerEventArgs<ServiceRef> e)
    {
        var info = $"{DateTime.Now} - [RemovedServiceRef] - {sender.ToString()} - {e.Entity.Alias.ToString()}";
        OnShowLog(info);
    }

    private void Store_AddedServiceRef(object sender, ControllerEventArgs<ServiceRef> e)
    {
        var info = $"{DateTime.Now} - [AddedServiceRef] - {sender.ToString()} - {e.Entity.Alias.ToString()}";
        OnShowLog(info);
    }

    #endregion 

    #region Private methods

    private void OnShowLog(string info)
    {
        View.ShowInfo(info);

        if (!Store.AppConfig.LogActivated)
            return;

        using (StreamWriter outputFile = new StreamWriter(Store.AppConfig.LogFile, true))
        {
            outputFile.WriteLine(info);
        }
    }

    #endregion
}

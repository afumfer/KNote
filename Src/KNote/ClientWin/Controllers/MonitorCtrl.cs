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
        return Store.FactoryViews.Registry.Resolve<MonitorCtrl, IViewBase>(this);
    }

    #endregion

    #region Controller override methods

    protected override Result<EControllerResult> OnInitialized()
    {
        var result = base.OnInitialized();

        // TODO: pending check result correctrly

        try
        {
            // All of Store's own coordination events (controller lifecycle/state, ServiceRef
            // add/remove, the "toast" notification channel) and the service command events go
            // through the same Store.Events bus - see DomainEvents.cs.
            Store.Events.Subscribe<ControllerStateChanged>(Store_CtrlStateChanged);
            Store.Events.Subscribe<ServiceRefAdded>(Store_AddedServiceRef);
            Store.Events.Subscribe<ServiceRefRemoved>(Store_RemovedServiceRef);
            Store.Events.Subscribe<ControllerNotification>(Store_ControllerNotification);
            Store.Events.Subscribe<ServiceCommandExecuting>(Store_ServiceCommandExecuting);
            Store.Events.Subscribe<ServiceCommandExecuted>(Store_ServiceCommandExecuted);
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
            Store.Events.Unsubscribe<ControllerStateChanged>(Store_CtrlStateChanged);
            Store.Events.Unsubscribe<ServiceRefAdded>(Store_AddedServiceRef);
            Store.Events.Unsubscribe<ServiceRefRemoved>(Store_RemovedServiceRef);
            // ControllerNotification was never unsubscribed even before this migration - kept as-is.
            Store.Events.Unsubscribe<ServiceCommandExecuting>(Store_ServiceCommandExecuting);
            Store.Events.Unsubscribe<ServiceCommandExecuted>(Store_ServiceCommandExecuted);
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

    private void Store_ControllerNotification(ControllerNotification e)
    {
        var info = $"{e.Controller.ControllerName} - {e.Message}";
        OnShowLog(info);
    }

    private void Store_CtrlStateChanged(ControllerStateChanged e)
    {
        var info = $"{DateTime.Now} - [ControllersStateChanged] - {e.Controller} - {e.State} - {e.Controller.ControllerId}";
        OnShowLog(info);
    }

    private void Store_RemovedServiceRef(ServiceRefRemoved e)
    {
        var info = $"{DateTime.Now} - [RemovedServiceRef] - {e.ServiceRef.Alias}";
        OnShowLog(info);
    }

    private void Store_AddedServiceRef(ServiceRefAdded e)
    {
        var info = $"{DateTime.Now} - [AddedServiceRef] - {e.ServiceRef.Alias}";
        OnShowLog(info);
    }

    private void Store_ServiceCommandExecuting(ServiceCommandExecuting e)
    {
        var alias = Store.GetServiceRef(e.Args.Service.IdServiceRef)?.Alias;
        var info = $"{DateTime.Now} - [CommandExecuting] - {e.Args.CommandName} - {alias}";
        OnShowLog(info);
    }

    private void Store_ServiceCommandExecuted(ServiceCommandExecuted e)
    {
        var alias = Store.GetServiceRef(e.Args.Service.IdServiceRef)?.Alias;
        var info = $"{DateTime.Now} - [CommandExecuted] - {e.Args.CommandName} - {alias} - {e.Args.Outcome} - {e.Args.Duration.TotalMilliseconds:N0}ms";
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

﻿using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Service.Core;

namespace KNote.ClientWin.Components;

public class MonitorComponent : ComponentViewBase<IViewConfigurable>
{
    #region Constructor 

    public MonitorComponent(Store store) : base(store)
    {
        ComponentName = "KeyNote monitor";
    }

    #endregion

    #region View 

    protected override IViewConfigurable CreateView()
    {
        return Store.FactoryViews.View(this);
    }

    #endregion

    #region Component override methods

    protected override Result<EComponentResult> OnInitialized()
    {
        var result = base.OnInitialized();

        // TODO: pending check result correctrly

        try
        {                                                
            Store.ComponentsStateChanged += Store_ComponentsStateChanged;
            Store.AddedServiceRef += Store_AddedServiceRef;                
            Store.RemovedServiceRef += Store_RemovedServiceRef;
            Store.ComponentNotification += Store_ComponentNotification;
        }
        catch (Exception ex)
        {                
            result.AddErrorMessage(ex.Message);
        }

        return result;
    }

    protected override Result<EComponentResult> OnFinalized()
    {
        Result<EComponentResult> result;

        try
        {
            result = base.OnFinalized();
            Store.ComponentsStateChanged -= Store_ComponentsStateChanged;
            Store.AddedServiceRef -= Store_AddedServiceRef;                
            Store.RemovedServiceRef -= Store_RemovedServiceRef;                
        }
        catch (Exception ex)
        {
            result = new Result<EComponentResult>(EComponentResult.Error);
            result.AddErrorMessage(ex.Message);                
        }

        return result;
    }

    #endregion 

    #region Store events handlers

    private void Store_ComponentNotification(object sender, ComponentEventArgs<string> e)
    {
        var info = $"{((ComponentBase)sender).ComponentName} - {e.Entity.ToString()}";
        OnShowLog(info);
    }

    private void Store_ComponentsStateChanged(object sender, ComponentEventArgs<EComponentState> e)
    {
        var info = $"{DateTime.Now} - [ControllersStateChanged] - {sender.ToString()} - {e.Entity.ToString()} - {((ComponentBase)sender).ComponentId}";
        OnShowLog(info);
    }
   
    private void Store_RemovedServiceRef(object sender, ComponentEventArgs<ServiceRef> e)
    {
        var info = $"{DateTime.Now} - [RemovedServiceRef] - {sender.ToString()} - {e.Entity.Alias.ToString()}";
        OnShowLog(info);
    }

    private void Store_AddedServiceRef(object sender, ComponentEventArgs<ServiceRef> e)
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

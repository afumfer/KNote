using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//using KNote.WinApp.Infrastructure;
//using KNote.WinApp.ViewModels;

//using KNote.DomainModel.Entities;
//using KNote.DomainModel;
//using KNote.DomainModel.Base;
//using KNote.DomainModel.Infrastructure;
//using KNote.WinApp.Intrastructure;
//using AnTScript;
//using KNote.ScriptLibrary;
using System.IO;

namespace KNote.ClientWin.Controllers
{
    //public class MonitorCtrl : BaseManagmentCtrl<IManagmentView>
    //{

    //    #region Constructor

    //    public MonitorCtrl(KntContext context) : base(context)
    //    {

    //    }

    //    #endregion 

    //    #region BaseManagmentCtrl members

    //    protected override IManagmentView CreateView()
    //    {
    //        return Context.FactoryViews.View(this);
    //    }

    //    protected override Result<CtrlResult> OnStart()
    //    {
    //        var res = new Result<CtrlResult>();

    //        try
    //        {
    //            View.ShowView();
               
    //            Context.ControllersStateChanged += Context_ControllersStateChanged;
    //            Context.ActiveFolderChanged += Context_ActiveFolderChanged;
    //            // TODO: Add more context events controllers here.
    //            // ...

    //            res.Entity = CtrlResult.Executed;
    //        }
    //        catch (Exception e)
    //        {
    //            res.Entity = CtrlResult.Error;
    //            res.AddErrorMessage(e.Message);
    //        }

    //        return ResultControllerAction<CtrlResult>(res);
    //    }

    //    private void Context_ActiveFolderChanged(object sender, EntityEventArgs<FolderWithServiceRef> e)
    //    {
    //        var info = DateTime.Now + " - " + "[ActiveFolderChanged]" + " - "
    //            + sender.ToString() + " - " + e.Entity.FolderInfo.Name.ToString();
    //        OnShowLog(info);
    //    }

    //    private void Context_ControllersStateChanged(object sender, StateCtrlEventArgs e)
    //    {
    //        var info = DateTime.Now + " - " + "[ControllersStateChanged]" + " - "
    //            + sender.ToString() + " - " + e.State.ToString();
    //        OnShowLog(info); 
    //    }

    //    private void OnShowLog(string info)
    //    {            
    //        View.ShowInfo(info);

    //        if (!Context.LogActivatd)
    //            return;

    //        using (StreamWriter outputFile = new StreamWriter(Context.LogFile, true))
    //        {
    //            outputFile.WriteLine(info);
    //        }
    //    }

    //    #endregion
    //}
}

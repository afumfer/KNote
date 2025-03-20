using KNote.ClientWin.Core;
using KntScript;

namespace KNote.ClientWin.Controllers;

public class KntScriptConsoleCtrl : CtrlViewBase<IViewBase>
{
    #region Properties

    public KntSEngine KntSEngine { get; set; }
        
    public string CodeFile { get; set; }

    #endregion 

    #region Constructor 

    public KntScriptConsoleCtrl(Store store): base(store)
    {
        ControllerName = "KeyNote script console";
    }

    #endregion

    #region Component overrid methods

    protected override IViewBase CreateView()
    {            
        return Store.FactoryViews.View(this);
    }

    #endregion 
}

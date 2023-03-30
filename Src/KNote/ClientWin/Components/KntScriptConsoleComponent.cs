using KNote.ClientWin.Core;
using KntScript;

namespace KNote.ClientWin.Components;

public class KntScriptConsoleComponent : ComponentViewBase<IViewBase>
{
    #region Properties

    public KntSEngine KntSEngine { get; set; }
        
    public string CodeFile { get; set; }

    #endregion 

    #region Constructor 

    public KntScriptConsoleComponent(Store store): base(store)
    {
        ComponentName = "KeyNote script console";
    }

    #endregion

    #region Component overrid methods

    protected override IViewBase CreateView()
    {            
        return Store.FactoryViews.View(this);
    }

    #endregion 
}

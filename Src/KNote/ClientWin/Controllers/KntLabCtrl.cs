using KNote.ClientWin.Core;

namespace KNote.ClientWin.Controllers;

public class KntLabCtrl : CtrlBase
{
    #region  Constructor

    public KntLabCtrl(Store store) : base(store)
    {
        ControllerName = "KntLab Component";
    }

    #endregion

    #region View

    IViewBase _labView;
    protected IViewBase LabView
    {
        get
        {
            if (_labView == null)
                _labView = Store.FactoryViews.View(this);
            return _labView;
        }
    }

    public void ShowLabView()
    {
        LabView.ShowView();
    }

    #endregion
}

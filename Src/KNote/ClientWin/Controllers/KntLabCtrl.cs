using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Controllers;

[KntAuthorize(EnumRoles.Admin, AuthorizationScope.Application)]
public class KntLabCtrl : CtrlBase
{
    #region  Constructor

    public KntLabCtrl(Store store) : base(store)
    {
        ControllerName = "KntLab Controller";
    }

    #endregion

    #region View

    IViewBase _labView;
    protected IViewBase LabView
    {
        get
        {
            if (_labView == null)
                _labView = Store.FactoryViews.Registry.Resolve<KntLabCtrl, IViewBase>(this);
            return _labView;
        }
    }

    public void ShowLabView()
    {
        // Refused by Run() (CheckPreconditions): already finalized, nothing to show.
        if (!PreconditionsMet)
            return;

        LabView.ShowView();
    }

    #endregion
}

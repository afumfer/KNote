using KNote.ClientWin.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.ClientWin.Components;

public class KntLabComponent : ComponentBase
{
    #region  Constructor

    public KntLabComponent(Store store) : base(store)
    {
        ComponentName = "KntLab Component";
    }

    #endregion

    #region IViewBase

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

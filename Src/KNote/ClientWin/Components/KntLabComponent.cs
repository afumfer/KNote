using KNote.ClientWin.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.ClientWin.Components;

public class KntLabComponent : ComponentBase
{
    public KntLabComponent(Store store) : base(store)
    {
    }

    IViewBase _labView;

    public void ShowLabView()
    {
        if (_labView == null)
            _labView = Store.FactoryViews.View(this);

        _labView.ShowView();
    }

}

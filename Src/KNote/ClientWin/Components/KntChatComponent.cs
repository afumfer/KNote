using KNote.ClientWin.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.ClientWin.Components;

public class KntChatComponent : ComponentBase
{
    public KntChatComponent(Store store) : base(store)
    {
        ComponentName = "KntChat Component";
    }

    IViewBase _chatView;
   
    public void ShowChatView()
    {
        if (_chatView == null)
            _chatView = Store.FactoryViews.View(this);

        _chatView.ShowView();
    }

}

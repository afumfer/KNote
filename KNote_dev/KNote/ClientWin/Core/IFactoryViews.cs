using KNote.ClientWin.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace KNote.ClientWin.Core
{
    public interface IFactoryViews
    {
        IViewBase View(MonitorComponent component);
        ISelectorView<FolderWithServiceRef> View(FolderSelectorComponent component);
    }
}

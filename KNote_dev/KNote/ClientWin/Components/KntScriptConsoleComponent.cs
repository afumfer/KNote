using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KNote.ClientWin.Core;
using KNote.Model;
using KntScript;

namespace KNote.ClientWin.Components
{
    public class KntScriptConsoleComponent : ComponentViewBase<IViewConfigurable>
    {
        public KntScriptConsoleComponent(Store store): base(store)
        {

        }
        protected override IViewConfigurable CreateView()
        {            
            return Store.FactoryViews.View(this);
        }

        public KntSEngine KntSEngine { get; set; }
            
        public string CodeFile { get; set; }

    }
}

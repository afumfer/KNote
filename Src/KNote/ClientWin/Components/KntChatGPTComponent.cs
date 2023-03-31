using KNote.ClientWin.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.ClientWin.Components
{
    public class KntChatGPTComponent : ComponentViewBase<IViewBase>
    {
        public KntChatGPTComponent(Store store) : base(store)
        {
        }

        protected override IViewBase CreateView()
        {
            return Store.FactoryViews.View(this);
        }
    }
}

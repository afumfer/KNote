using System;
using System.Collections.Generic;
using System.Text;

namespace KNote.ClientWin.Core
{
    abstract public class ComponentViewBase<TView> : ComponentBase
    {
        public ComponentViewBase(Store store): base(store)
        {

        }

        private TView _view;
        public TView View
        {
            get
            {
                if (_view == null)
                {
                    _view = CreateView();
                }
                return _view;
            }
        }
        
        protected abstract TView CreateView();
             
    }
}

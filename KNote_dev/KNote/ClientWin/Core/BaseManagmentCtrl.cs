using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KNote.Model;

namespace KNote.ClientWin.Core
{
    abstract public class BaseManagmentCtrl<TView> : BaseCtrl
    {
        public BaseManagmentCtrl(KntContext context)
           : base(context)
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

        #region Abstract methods

        protected abstract TView CreateView();

        // TODO: Pendiente añadir miembros comunes a las controladoras gestionar

        #endregion

    }
}
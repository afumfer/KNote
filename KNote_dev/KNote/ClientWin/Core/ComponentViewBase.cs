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

    abstract public class ComponentSelectorViewBase<TView, TEntity> : ComponentViewBase<TView>
    {
        public ComponentSelectorViewBase(Store store) : base(store)
        {

        }

        public event EventHandler<ComponentEventArgs<TEntity>> EntitySelection;
        protected virtual void OnEntitySelection(TEntity entity)
        {
            EntitySelection?.Invoke(this, new ComponentEventArgs<TEntity>(entity));
        }

        public TEntity SelectedEntity { get; set; }

        public virtual void NotifySelectedEntity()
        {
            OnEntitySelection(SelectedEntity);
        }

        public virtual void CancelAction()
        {
            OnComponentResultChanged(EComponentResult.Canceled);
            base.Finalize();
        }

        public virtual void AcceptAction()
        {
            try
            {
                NotifySelectedEntity();
                OnComponentResultChanged(EComponentResult.Executed);
                base.Finalize();
            }
            catch (Exception)
            {
                OnStateComponentChanged(EComponentState.Error);
            }
        }

    }


}

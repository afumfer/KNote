using KNote.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace KNote.ClientWin.Core
{
    abstract public class ComponentViewBase<TView> : ComponentBase
        where TView : IViewConfigurable
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

        protected override Result OnInitialized()
        {
            var result = base.OnInitialized();

            View.RefreshView();

            if (!EmbededMode)
                View.ConfigureWindowMode();
            else
                View.ConfigureEmbededMode();
            
            return result;
        }

        public override Result Run()
        {
            var result = base.Run();

            // TODO:  Check result here 
            // ...

            View.ShowView();

            return result;
        }

        public virtual Result<EComponentResult> RunModal()
        {
            var result = base.Run();

            // TODO:  Check result here 
            // ...

            var resultView = View.ShowModalView();

            return resultView;
        }

        public Result<EComponentResult> DialogResultToComponentResult(DialogResult dialogResult)
        {
            var result = new Result<EComponentResult>();
            if (dialogResult == DialogResult.OK || dialogResult == DialogResult.Yes)
                result.Entity = EComponentResult.Executed;
            else
                result.Entity = EComponentResult.Canceled;
            return result;
        }

    }

    abstract public class ComponentSelectorViewBase<TView, TEntity> : ComponentViewBase<TView>
        where TView : IViewConfigurable
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

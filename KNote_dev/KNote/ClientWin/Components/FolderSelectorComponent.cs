using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Components
{
    public class FolderSelectorComponent : ComponentViewBase<ISelectorView<FolderWithServiceRef>>
    {

        public FolderSelectorComponent(Store store) : base(store)
        {

        }

        #region Component override methods 

        protected override ISelectorView<FolderWithServiceRef> CreateView()
        {
            return Store.FactoryViews.View(this);
        }

        protected override Result OnInitialized()
        {
            var result = base.OnInitialized();

            View.ShowView();

            return result;
        }

        #endregion 

        #region Component public members

        public List<ServiceRef> ServicesRef
        {
            get
            {
                return Store.GetAllServiceRef();
            }
        }

        public async Task<List<FolderDto>> GetTreeAsync(ServiceRef serviceRef)
        {
            return (await serviceRef.Service.Folders.GetAllAsync()).Entity;
        }

        public FolderWithServiceRef SelectedFolderWithServiceRef { get; set; }

        #endregion 

        public event EventHandler<EntityEventArgs<FolderWithServiceRef>> EntitySelection;

        public void NotifyEntitySelectionAction()
        {            
            OnEntitySelection(new EntityEventArgs<FolderWithServiceRef>(SelectedFolderWithServiceRef));
        }
        protected virtual void OnEntitySelection(EntityEventArgs<FolderWithServiceRef> e)
        {
            EntitySelection?.Invoke(this, e);
        }

        public virtual Result<EComponentResult> CancelAction()
        {
            var result = new Result<EComponentResult>();
            
            result.Entity = EComponentResult.Canceled;
            
            base.Finalize();

            return result;
        }

        public virtual Result<EComponentResult> AcceptAction()
        {
            var result = new Result<EComponentResult>();

            try
            {
                // SelectorEntityAction();
                result.Entity = EComponentResult.Executed;                
                base.Finalize();
            }
            catch (Exception ex)
            {
                result.Entity = EComponentResult.Error;
                OnStateCtrlChanged(EComponentState.Error);
                result.AddErrorMessage(ex.Message);
            }

            return result;
        }

    }
}

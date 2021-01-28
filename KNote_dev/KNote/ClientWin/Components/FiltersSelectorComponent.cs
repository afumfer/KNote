using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;

using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service;
using System.ComponentModel.DataAnnotations;

namespace KNote.ClientWin.Components
{
    public class FiltersSelectorComponent : ComponentSelectorBase<ISelectorView<NotesFilterWithServiceRef>, NotesFilterWithServiceRef>
    {
        private List<ServiceRef> _servicesRef;
        public List<ServiceRef> ServicesRef
        {
            get
            {
                if (_servicesRef == null)
                    _servicesRef = Store.GetAllServiceRef();
                return _servicesRef;
            }
            set
            {
                _servicesRef = value;
            }
        }

        public FiltersSelectorComponent(Store store): base (store)
        {
            ComponentName = "Filter param";
            
            // ListEntities == TODO: ... load all stores filters 
        }

        #region Basura ...
        //public event EventHandler<ComponentEventArgs<NotesFilterWithServiceRef>> NotesFilterSelection;
        //public void OnNotesFilterSelection()
        //{
        //    //NotesFilterSelection?.Invoke(this, new ComponentEventArgs<NotesFilterWithServiceRef>(
        //    //    new NotesFilterWithServiceRef { ServiceRef = SelectedServiceRef, NotesFilter = Model }));
        //}

        #endregion

        #region ComponentEditorBase implementation 

        protected override ISelectorView<NotesFilterWithServiceRef> CreateView()
        {
            return Store.FactoryViews.View(this);
        }

        public override Task<bool> LoadEntities(IKntService service, bool refreshView = true)
        {
            throw new NotImplementedException();
        }

        public override void SelectItem(NotesFilterWithServiceRef item)
        {
            throw new NotImplementedException();
        }

        public override void RefreshItem(NotesFilterWithServiceRef item)
        {
            throw new NotImplementedException();
        }

        public override void AddItem(NotesFilterWithServiceRef item)
        {
            throw new NotImplementedException();
        }

        public override void DeleteItem(NotesFilterWithServiceRef item)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

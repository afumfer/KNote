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
    public class FolderSelectorComponent : ComponentSelectorViewBase<ISelectorView<FolderWithServiceRef>, FolderWithServiceRef>   
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

        #region Component specific public members

        public List<ServiceRef> ServicesRef
        {
            get
            {
                return Store.GetAllServiceRef();
            }
        }

        public async Task<List<FolderDto>> GetTreeAsync(ServiceRef serviceRef)
        {
            return (await serviceRef.Service.Folders.GetTreeAsync()).Entity;
        }

        public void SelectFolder(FolderWithServiceRef folder)
        {
            View.SelectItem(folder);            
        }
        #endregion

    }
}

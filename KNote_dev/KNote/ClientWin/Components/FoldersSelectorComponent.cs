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
    public class FoldersSelectorComponent : ComponentSelectorBase<ISelectorView<FolderWithServiceRef>, FolderWithServiceRef>   
    {
        #region Constructor

        public FoldersSelectorComponent(Store store) : base(store)
        {

        }

        #endregion 

        #region Component override methods 

        protected override ISelectorView<FolderWithServiceRef> CreateView()
        {
            return Store.FactoryViews.View(this);
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
            try
            {
                return (await serviceRef.Service.Folders.GetTreeAsync()).Entity;
            }
            catch (Exception ex)
            {
                View.ShowInfo(ex.Message);
                throw;
            }
        }

        public void SelectFolder(FolderWithServiceRef folder)
        {
            View.SelectItem(folder);            
        }

        public void RefreshFolder(FolderInfoDto folder)
        {
            if (SelectedEntity.FolderInfo.FolderId == folder.FolderId)
            {
                // Refresh Selected Folder
                SelectedEntity.FolderInfo.SetSimpleDto(folder);
                // Refresh View
                View.RefreshItem(SelectedEntity);                
            }
        }

        #endregion
    }
}

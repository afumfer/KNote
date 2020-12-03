using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service;

namespace KNote.ClientWin.Components
{
    public class FoldersSelectorComponent : ComponentSelectorBase<ISelectorView<FolderWithServiceRef>, FolderWithServiceRef>
    {
        #region Properties

        public List<ServiceRef> ServicesRef
        {
            get
            {
                return Store.GetAllServiceRef();
            }
        }

        #endregion 

        #region Constructor

        public FoldersSelectorComponent(Store store) : base(store)
        {
            ListEntities = new List<FolderWithServiceRef>();
        }

        #endregion 

        #region Component override methods 

        protected override ISelectorView<FolderWithServiceRef> CreateView()
        {
            return Store.FactoryViews.View(this);
        }

        #endregion 

        #region Component methods

        public override void LoadEntities(IKntService service)
        {
            throw new NotImplementedException();
        }

        public async Task<List<FolderDto>> LoadEntities(ServiceRef serviceRef)
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

        public override void RefreshItem(FolderWithServiceRef item)
        {
            if (SelectedEntity.FolderInfo.FolderId == item.FolderInfo.FolderId)
            {
                // Refresh Selected Folder
                SelectedEntity.FolderInfo.SetSimpleDto(item.FolderInfo);
                // TODO: !!! refresh model here ...
                // ...
                // Refresh View
                View.RefreshItem(SelectedEntity);
            }
        }

        public override void SelectItem(FolderWithServiceRef folder)
        {
            // TODO: !!! refresh model here ...
            // ...
            View.SelectItem(folder);            
        }

        public override void AddItem(FolderWithServiceRef item)
        {
            // TODO: !!! refresh model here ...
            // ...

            View.AddItem(item);
        }

        public override void DeleteItem(FolderWithServiceRef item)
        {
            // TODO: !!! refresh model here ...
            // ...

            View.DeleteItem(item);
        }

        #endregion
    }
}

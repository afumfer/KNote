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
    public class FolderEditorComponent : ComponentEditorBase<IEditorView<FolderDto>, FolderDto>
    {
        private FolderDto _folderEdit;
        public FolderDto FolderDto
        {
            set
            {
                _folderEdit = value;
            }
            get
            {
                if (_folderEdit == null)
                    _folderEdit = new FolderDto();
                return _folderEdit;
            }
        }

        public FolderEditorComponent(Store store): base(store)
        {
            
        }

        protected override IEditorView<FolderDto> CreateView()
        {
            return Store.FactoryViews.View(this);
        }

        public override async void LoadModelById(IKntService service, Guid id, bool refreshView = true)
        {
            try
            {
                Service = service;

                Model = (await Service.Folders.GetAsync(id)).Entity;
                Model.SetIsDirty(false);
                if(refreshView)
                    View.RefreshView();
            }
            catch (Exception ex)
            {
                View.ShowInfo(ex.Message);
            }
        }

        public override void NewModel(IKntService service)
        {
            throw new NotImplementedException();
        }

        public override async Task<bool> SaveModel()
        {
            if (!Model.IsDirty())
                return true;

            var isNew = (Model.FolderId == Guid.Empty);

            var msgVal = Model.GetErrorMessage();
            if (!string.IsNullOrEmpty(msgVal))
            {
                View.ShowInfo(msgVal);
                return false;
            }

            try
            {
                var response = await Service.Folders.SaveAsync(Model);

                if (response.IsValid)
                {
                    Model = response.Entity;
                    Model.SetIsDirty(false);

                    if (!isNew)
                        OnSavedEntity(response.Entity);
                    else
                        OnAddedEntity(response.Entity);

                    Finalize();
                }
                else
                    View.ShowInfo(response.Message);
            }
            catch (Exception ex)
            {
                View.ShowInfo(ex.Message);
                return true;
            }

            return true;
        }

        public override Task<bool> DeleteModel(IKntService service, Guid noteId)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> DeleteModel()
        {
            throw new NotImplementedException();
        }

    }
}

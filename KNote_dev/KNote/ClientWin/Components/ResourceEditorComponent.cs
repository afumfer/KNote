using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using System.Windows.Forms;

using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service;
using System.ComponentModel.DataAnnotations;

namespace KNote.ClientWin.Components
{
    public class ResourceEditorComponent : ComponentEditorBase<IEditorView<ResourceDto>, ResourceDto>
    {
        public ResourceEditorComponent(Store store): base(store)
        {
            ComponentName = "Resource editor";
        }

        #region Abstract members implementations

        protected override IEditorView<ResourceDto> CreateView()
        {
            return Store.FactoryViews.View(this);
        }

        public override async Task<bool> NewModel(IKntService service)
        {
            Service = service;

            // TODO: call service for new model
            Model = new ResourceDto();
            Model.ResourceId = Guid.NewGuid();
            return await Task.FromResult<bool>(true);
        }

        public async override Task<bool> LoadModelById(IKntService service, Guid id, bool refreshView = true)
        {
            try
            {
                Service = service;

                var res = await Service.Notes.GetResourceAsync(id);
                if (!res.IsValid)
                    return false;

                Model = res.Entity;
                Model.SetIsDirty(false);
                if (refreshView)
                    View.RefreshView();
                return true;
            }
            catch (Exception ex)
            {
                View.ShowInfo(ex.Message);
                return false;
            }
        }

        public async override Task<bool> SaveModel()
        {
            if (!Model.IsDirty())
                return true;

            var isNew = (Model.ResourceId == Guid.Empty);

            var msgVal = Model.GetErrorMessage();
            if (!string.IsNullOrEmpty(msgVal))
            {
                View.ShowInfo(msgVal);
                return false;
            }

            try
            {
                Result<ResourceDto> response;
                if (AutoDBSave)
                {
                    response = await Service.Notes.SaveResourceAsync(Model, true);
                    Model = response.Entity;
                    Model.SetIsDirty(false);
                }
                else
                {
                    response = new Result<ResourceDto>();
                    Model.SetIsDirty(true);
                    response.Entity = Model;
                }

                if (response.IsValid)
                {
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
                return false;
            }

            return true;
        }

        public async override Task<bool> DeleteModel(IKntService service, Guid id)
        {
            var result = View.ShowInfo("Are you sure you want to delete this resource?", "Delete resource", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes || result == DialogResult.Yes)
            {
                try
                {
                    Result<ResourceDto> response;
                    if (AutoDBSave)
                        response = await service.Notes.DeleteResourceAsync(id);
                    else
                        response = new Result<ResourceDto>();

                    if (response.IsValid)
                    {
                        Model = response.Entity;
                        OnDeletedEntity(response.Entity);
                        return true;
                    }
                    else
                        View.ShowInfo(response.Message);
                }
                catch (Exception ex)
                {
                    View.ShowInfo(ex.Message);
                }
            }
            return false;
        }

        public async override Task<bool> DeleteModel()
        {
            return await DeleteModel(Service, Model.ResourceId);
        }

        public string ExtensionFileToFileType(string extension)
        {
            // TODO: study this method ...
            if (extension == ".jpg")
                return @"image/jpeg";
            if (extension == ".jpeg")
                return @"image/jpeg";
            else if (extension == ".png")
                return "image/png";
            else if (extension == ".pdf")
                return "application/pdf";
            else
                return "";
        }

        #endregion 
    }
}

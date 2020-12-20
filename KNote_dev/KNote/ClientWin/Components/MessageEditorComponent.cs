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
    public class MessageEditorComponent : ComponentEditorBase<IEditorView<KMessageDto>, KMessageDto>
    {

        public bool AutoDBSave { get; set; } = true;

        public MessageEditorComponent(Store store): base(store)
        {
            ComponentName = "Message / alarm editor";
        }

        #region Abstract member implementations 

        protected override IEditorView<KMessageDto> CreateView()
        {
            return Store.FactoryViews.View(this);
        }

        public async override Task<bool> LoadModelById(IKntService service, Guid id, bool refreshView = true)
        {
            try
            {
                Service = service;

                Model = (await Service.Notes.GetMessageAsync(id)).Entity;
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

        public void LoadModel(KMessageDto entity)
        {
            try
            {
                Model = entity;
                Model.SetIsDirty(false);                
                View.RefreshView();                
            }
            catch (Exception ex)
            {
                View.ShowInfo(ex.Message);                
            }
        }


        public override void NewModel(IKntService service)
        {
            Service = service;

            // TODO: call service for new model
            Model = new KMessageDto();
        }

        public override async Task<bool> SaveModel()
        {            
            //// For test
            //Finalize();
            //return await Task.FromResult(true);
            //// ...

            if (!Model.IsDirty())
                return true;

            var isNew = (Model.KMessageId == Guid.Empty);

            var msgVal = Model.GetErrorMessage();
            if (!string.IsNullOrEmpty(msgVal))
            {
                View.ShowInfo(msgVal);
                return false;
            }

            try
            {
                Result<KMessageDto> response;
                if (AutoDBSave)
                {
                    response = await Service.Notes.SaveMessageAsync(Model);
                    Model = response.Entity;
                    Model.SetIsDirty(false);
                }
                else
                {
                    response = new Result<KMessageDto>();
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
            var result = View.ShowInfo("Are you sure you want to delete this folder?", "Delete note", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes || result == DialogResult.Yes)
            {
                try
                {
                    Result<KMessageDto> response;
                    if (AutoDBSave)
                        response = await service.Notes.DeleteMessageAsync(id);
                    else                    
                        response = new Result<KMessageDto>();
                    
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
            return await DeleteModel(Service, Model.KMessageId);
        }


        #endregion 
    }
}

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

        public override Task<bool> LoadModelById(IKntService service, Guid id, bool refreshView = true)
        {
            throw new NotImplementedException();
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
                    response = await Service.Notes.SaveMessageAsync(Model);
                else
                    response = new Result<KMessageDto>();

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
                return false;
            }

            return true;
        }

        public override Task<bool> DeleteModel(IKntService service, Guid id)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> DeleteModel()
        {
            throw new NotImplementedException();
        }


        #endregion 
    }
}

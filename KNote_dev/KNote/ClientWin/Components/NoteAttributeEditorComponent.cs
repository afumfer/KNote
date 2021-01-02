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
    public class NoteAttributeEditorComponent : ComponentEditorBase<IEditorView<NoteKAttributeDto>, NoteKAttributeDto>
    {

        public NoteAttributeEditorComponent(Store store): base (store)
        {
            ComponentName = "Note attribute editor";
        }

        protected override IEditorView<NoteKAttributeDto> CreateView()
        {
            return Store.FactoryViews.View(this);
        }

        public override Task<bool> LoadModelById(IKntService service, Guid id, bool refreshView = true)
        {
            throw new NotImplementedException();
        //    // dummy
        //    Model = new NoteKAttributeDto { Value = "xxx" };
        //    return await Task.FromResult<bool>(true);

        //    //try
        //    //{
        //    //    Service = service;

        //    //   // Model = (await Service.Folders.GetAsync(id)).Entity;
        //    //    Model.SetIsDirty(false);
        //    //    if (refreshView)
        //    //        View.RefreshView();
        //    //    return true;
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    View.ShowInfo(ex.Message);
        //    //    return false;
        //    //}
        }

        public override void NewModel(IKntService service)
        {
            Service = service;

            // TODO: call service for new model
            Model = new NoteKAttributeDto();
        }

        public async override Task<bool> SaveModel()
        {
            //Finalize();
            //return await Task.FromResult<bool>(true);

            if (!Model.IsDirty())
                return await Task.FromResult<bool>(true); ;

            var isNew = (Model.NoteKAttributeId == Guid.Empty);

            var msgVal = Model.GetErrorMessage();
            if (!string.IsNullOrEmpty(msgVal))
            {
                View.ShowInfo(msgVal);
                return await Task.FromResult<bool>(false); ;
            }

            try
            {
                Result<NoteKAttributeDto> response;
                if (AutoDBSave)
                {
                    throw new NotImplementedException();
                    //response = await Service.Xxxxx.SaveAsync(Model);
                    //Model = response.Entity;
                    //Model.SetIsDirty(false);
                }
                else
                {
                    response = new Result<NoteKAttributeDto>();
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
                return await Task.FromResult<bool>(false);
            }

            return await Task.FromResult<bool>(true);
        }

        public override Task<bool> DeleteModel(IKntService service, Guid id)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> DeleteModel()
        {
            throw new NotImplementedException();
        }
    }
}

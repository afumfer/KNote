using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using System.Windows.Forms;

using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using System.ComponentModel.DataAnnotations;
using KNote.Service.Core;

namespace KNote.ClientWin.Components
{
    public class TaskEditorComponent : ComponentEditorBase<IEditorView<NoteTaskDto>, NoteTaskDto>
    {

        public TaskEditorComponent(Store store) : base(store)
        {
            ComponentName = "Task / comment editor";
        }


        #region Abstract member implementations 

        protected override IEditorView<NoteTaskDto> CreateView()
        {
            return Store.FactoryViews.View(this);
        }

        public override async Task<bool> NewModel(IKntService service)
        {
            Service = service;

            // TODO: call service for new model
            Model = new NoteTaskDto();
            Model.NoteTaskId = Guid.NewGuid();
            return await Task.FromResult<bool>(true);
        }

        public async override Task<bool> LoadModelById(IKntService service, Guid id, bool refreshView = true)
        {
            try
            {
                Service = service;

                var res = await Service.Notes.GetNoteTaskAsync(id);
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

        public override async Task<bool> SaveModel()
        {
            View.RefreshModel();

            if (!Model.IsDirty())
                return true;

            var isNew = (Model.NoteTaskId == Guid.Empty);

            var msgVal = Model.GetErrorMessage();
            if (!string.IsNullOrEmpty(msgVal))
            {
                View.ShowInfo(msgVal);
                return false;
            }

            try
            {
                Result<NoteTaskDto> response;
                if (AutoDBSave)
                {
                    response = await Service.Notes.SaveNoteTaskAsync(Model, true);
                    Model = response.Entity;
                    Model.SetIsDirty(false);
                }
                else
                {
                    response = new Result<NoteTaskDto>();
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
                    View.ShowInfo(response.ErrorMessage);
            }
            catch (Exception ex)
            {
                View.ShowInfo(ex.Message);
                return false;
            }

            return true;
        }

        public override async  Task<bool> DeleteModel(IKntService service, Guid id)
        {
            var result = View.ShowInfo("Are you sure you want to delete this task?", "Delete message", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes || result == DialogResult.Yes)
            {
                try
                {
                    Result<NoteTaskDto> response;
                    if (AutoDBSave)
                        response = await service.Notes.DeleteNoteTaskAsync(id);
                    else
                    {
                        var resGet = await service.Notes.GetNoteTaskAsync(id);
                        if (!resGet.IsValid)
                        {
                            response = new Result<NoteTaskDto>();
                            response.Entity = new NoteTaskDto();
                        }
                        else
                            response = resGet;
                    }
                        
                    if (response.IsValid)
                    {
                        response.Entity.SetIsDeleted(true);
                        Model = response.Entity;
                        OnDeletedEntity(Model);
                        return true;
                    }
                    else
                        View.ShowInfo(response.ErrorMessage);
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
            return await DeleteModel(Service, Model.NoteTaskId);
        }

        #endregion 
    }
}

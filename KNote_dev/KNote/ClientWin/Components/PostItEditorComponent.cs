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
using System.Linq;

namespace KNote.ClientWin.Components
{
    public class PostItEditorComponent : ComponentEditorBase<IEditorView<NoteDto>, NoteDto>
    {
        #region Constructor

        public PostItEditorComponent(Store store): base(store)
        {
            ComponentName = "PostIt editor";
        }

        #endregion 

        #region Componet specific events 

        public event EventHandler<ComponentEventArgs<ServiceWithNoteId>> ExtendedEdit;
        protected virtual void OnExtendedEdit()
        {
            ExtendedEdit?.Invoke(this, new ComponentEventArgs<ServiceWithNoteId>(new ServiceWithNoteId { Service = Service, NoteId = Model.NoteId }));
        }

        #endregion

        #region IEditorView implementation

        protected override IEditorView<NoteDto> CreateView()
        {
            return Store.FactoryViews.View(this);
        }

        #endregion 

        #region ComponentEditor override methods

        public async override Task<bool> LoadModelById(IKntService service, Guid noteId, bool refreshView = true)
        {
            try
            {
                Service = service;

                Model = (await Service.Notes.GetAsync(noteId)).Entity;
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

        public async override void NewModel(IKntService service)
        {
            try
            {
                Service = service;

                var response = await Service.Notes.NewAsync();
                Model = response.Entity;

                // Evaluate whether to put the following default values in the service layer 
                // (null values are by default, we need empty strings so that the IsDirty is 
                //  not altered after leaving the view when there are no modifications).
                Model.Topic = DateTime.Now.ToString();
                Model.Tags = "";
                Model.Description = "";

                // Context default values
                Model.FolderId = Store.ActiveFolderWithServiceRef.FolderInfo.FolderId;
                Model.FolderDto = Store.ActiveFolderWithServiceRef.FolderInfo.GetSimpleDto<FolderDto>();

                Model.SetIsDirty(false);

                View.RefreshView();
            }
            catch (Exception ex)
            {
                View.ShowInfo(ex.Message);
            }
        }

        public async override Task<bool> SaveModel()
        {
            if (!Model.IsDirty())
                return true;

            var isNew = (Model.NoteId == Guid.Empty);

            var msgVal = Model.GetErrorMessage();
            if (!string.IsNullOrEmpty(msgVal))
            {
                View.ShowInfo(msgVal);
                return false;
            }

            try
            {
                var response = await Service.Notes.SaveAsync(Model);

                if (response.IsValid)
                {
                    Model = response.Entity;

                    Model.SetIsDirty(false);
                    Model.SetIsNew(false);

                    if (!isNew)
                        OnSavedEntity(response.Entity);
                    else
                        OnAddedEntity(response.Entity);

                    View.RefreshView();
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

        public async override Task<bool> DeleteModel(IKntService service, Guid noteId)
        {
            var result = View.ShowInfo("Are you sure you want to delete this note?", "Delete note", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes || result == DialogResult.Yes)
            {
                try
                {
                    var response = await service.Notes.DeleteAsync(noteId);

                    if (response.IsValid)
                    {
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
            return await DeleteModel(Service, Model.NoteId);
        }

        #endregion 

        #region Component specific methods

        public void FinalizeAndExtendEdit()
        {            
            OnExtendedEdit();
            Finalize();
        }

        #endregion 
       
    }
}

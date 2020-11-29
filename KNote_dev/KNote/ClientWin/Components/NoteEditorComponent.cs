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
    public class NoteEditorComponent : ComponentEditorBase<IEditorView<NoteExtendedDto>, NoteExtendedDto>
    {
        #region Properties
       
        #endregion

        #region Constructor

        public NoteEditorComponent(Store store) : base(store)
        {
        }

        #endregion

        #region IEditorView implementation

        protected override IEditorView<NoteExtendedDto> CreateView()
        {
            return Store.FactoryViews.View(this);
        }

        #endregion

        #region ComponentEditorBase override methods

        public override async void LoadModelById(IKntService service, Guid noteId)
        {
            try
            {                
                Service = service;

                Model = (await Service.Notes.GetExtendedAsync(noteId)).Entity;
                Model.SetIsDirty(false);

                View.RefreshView();
            }
            catch (Exception ex)
            {
                View.ShowInfo(ex.Message);                
            }
        }
        
        public override async void NewModel(IKntService service)
        {
            try
            {
                Service = service;
                
                var response = await Service.Notes.NewExtendedAsync();
                Model = response.Entity;
                
                // Context default values
                Model.Note.FolderId = Store.ActiveFolderWithServiceRef.FolderInfo.FolderId;
                Model.Note.FolderDto = Store.ActiveFolderWithServiceRef.FolderInfo.GetSimpleDto<FolderDto>();

                Model.SetIsDirty(false);

                View.RefreshView();
            }
            catch (Exception ex)
            {
                View.ShowInfo(ex.Message);                
            }
        }

        public override async void SaveModel()
        {
            if (!Model.IsDirty())
                return;

            var isNew = (Model.Note.NoteId == Guid.Empty);
                        
            var msgVal = Model.GetErrorMessage();
            if (!string.IsNullOrEmpty(msgVal))
            {
                View.ShowInfo(msgVal);
                return;
            }

            try
            {                                
                var response = await Service.Notes.SaveExtendedAsync(Model);

                if (response.IsValid)
                {
                    Model = response.Entity;
                    
                    Model.SetIsDirty(false);

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
            }
                        
        }

        public override async Task<bool> DeleteModel()
        {
            return await DeleteModel(Service, Model.Note.NoteId);
        }

        public override async Task<bool> DeleteModel(IKntService service, Guid noteId) 
        {
            var result = View.ShowInfo("Are you sure you want to delete this note?", "Delete note", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes || result == DialogResult.Yes)
            {
                try
                {
                    var response = await service.Notes.DeleteExtendedAsync(noteId);
                    
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
            
        #endregion
    }
}

﻿using System;
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
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace KNote.ClientWin.Components
{    
    public class NoteEditorComponent : ComponentEditor<IEditorView<NoteExtendedDto>, NoteExtendedDto>
    {
        #region Properties
       
        public bool EditMode { get; set; }

        #endregion

        #region Constructor

        public NoteEditorComponent(Store store) : base(store)
        {
            ComponentName = "Note editor";
        }

        #endregion

        #region Componet specific events 

        public event EventHandler<ComponentEventArgs<ServiceWithNoteId>> PostItEdit;
        protected virtual void OnPostItEdit()
        {
            PostItEdit?.Invoke(this, new ComponentEventArgs<ServiceWithNoteId>(new ServiceWithNoteId { Service = Service, NoteId = Model.NoteId }));
        }

        #endregion

        #region IEditorView implementation

        protected override IEditorView<NoteExtendedDto> CreateView()
        {
            return Store.FactoryViews.View(this);
        }

        #endregion

        #region ComponentEditorBase override methods

        public override async Task<bool> LoadModelById(IKntService service, Guid noteId, bool refreshView = true)
        {
            try
            {                
                Service = service;

                Model = (await Service.Notes.GetExtendedAsync(noteId)).Entity;
                Model.SetIsDirty(false);
                if(refreshView)
                    View.RefreshView();
                return true;
            }
            catch (Exception ex)
            {
                View.ShowInfo(ex.Message);
                return false;
            }
        }
        
        public override async Task<bool> NewModel(IKntService service)
        {
            try
            {
                Service = service;
                
                var response = await Service.Notes.NewExtendedAsync();
                Model = response.Entity;

                // Evaluate whether to put the following default values in the service layer 
                // (null values are by default, we need empty strings so that the IsDirty is 
                //  not altered after leaving the view when there are no modifications).
                Model.Topic = "New topic ...";                
                Model.Tags = "";
                Model.Description = "";

                // Context default values
                Model.FolderId = Store.ActiveFolderWithServiceRef.FolderInfo.FolderId;
                Model.FolderDto = Store.ActiveFolderWithServiceRef.FolderInfo.GetSimpleDto<FolderDto>();

                Model.SetIsDirty(false);

                View.RefreshView();
                
                return true;
            }
            catch (Exception ex)
            {
                View.ShowInfo(ex.Message);                
            }
            return false;
        }

        public override async Task<bool> SaveModel()
        {
            View.RefreshModel();

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
                var response = await Service.Notes.SaveExtendedAsync(Model);

                if (response.IsValid)
                {
                    Model = response.Entity;
                    
                    Model.SetIsDirty(false);
                    Model.SetIsNew(false);

                    if (!isNew)
                        OnSavedEntity(response.Entity);
                    else
                        OnAddedEntity(response.Entity);
                    
                    // TODO: future version ... notify actions.
                    // NotifyMessage($"Note {Model?.NoteNumber.ToString()} saved");
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

        public override async Task<bool> DeleteModel()
        {
            return await DeleteModel(Service, Model.NoteId);
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
                        OnDeletedEntity(response.Entity);                        
                    
                    return await Task.FromResult<bool>(true);
                }
                catch (Exception ex)
                {
                    View.ShowInfo(ex.Message);
                }
            }
            return await Task.FromResult<bool>(false);
        }

        #endregion

        #region Component specific methods

        public NoteKAttributeDto EditAttribute(NoteKAttributeDto noteAttribute)
        {
            var noteAttributeEditor = new NoteAttributeEditorComponent(Store);

            noteAttributeEditor.AutoDBSave = false;  // don't save automatically
            
            noteAttributeEditor.LoadModel(Service, noteAttribute, false);

            var res = noteAttributeEditor.RunModal();
            if (res.Entity == EComponentResult.Executed)
            {
                return noteAttributeEditor.Model;
            }
            else
                return null;
        }

        public async Task<bool> RequestChangeNoteType(Guid? oldSelectedId)
        {
            var noteTypesSelector = new NoteTypesSelectorComponent(Store);
            var resCanLoadEntities = await noteTypesSelector.LoadEntities(Service, false);            
            if (resCanLoadEntities)
            {
                var res = noteTypesSelector.RunModal();
                if (res.Entity == EComponentResult.Executed)
                {
                    if (oldSelectedId == noteTypesSelector.SelectedEntity.NoteTypeId)
                        return false;
                    else                    
                        return await AplyChangeNoteType(noteTypesSelector.SelectedEntity);                    
                }                                    
            }
            else
                View.ShowInfo("Cannot load the list of note types");

            return false ;
        }

        public async Task<bool> AplyChangeNoteType(NoteTypeDto newType)
        {
            try
            {
                Model.NoteTypeId = newType?.NoteTypeId;
                Model.NoteTypeDto = newType;

                // Delete old attributes 
                Model.KAttributesDto.RemoveAll(_ => _.KAttributeNoteTypeId != null);

                if (newType == null)
                    return await Task.FromResult(true);

                // Add new attributes
                Model.KAttributesDto = await Service.Notes.CompleteNoteAttributes(Model.KAttributesDto, Model.NoteId, newType.NoteTypeId);                
                return await Task.FromResult(true);
            }
            catch (Exception)
            {
                return await Task.FromResult(false);
            }
        }

        public async Task<KMessageDto> NewMessage()
        {
            var messageEditor = new MessageEditorComponent(Store);

            messageEditor.AutoDBSave = false;  // don't save automatically
            
            await messageEditor.NewModel(Service);            
            messageEditor.Model.NoteId = Model.NoteId;
            messageEditor.Model.Comment = "(Aditional text for message)";
            var userDto = (await Service.Users.GetByUserNameAsync(Store.AppUserName)).Entity;
            messageEditor.Model.UserId = userDto.UserId;
            messageEditor.Model.UserFullName = userDto.FullName;
            messageEditor.Model.AlarmActivated = true;
            messageEditor.Model.AlarmDateTime = DateTime.Now;
            messageEditor.Model.SetIsNew(true);

            var res = messageEditor.RunModal();

            if(res.Entity == EComponentResult.Executed)
            {                
                Model.Messages.Add(messageEditor.Model);
                return messageEditor.Model;
            }            
            else
                // TODO: show error here 
                return null;            
        }

        public KMessageDto EditMessage(Guid messageId)
        {
            var messageEditor = new MessageEditorComponent(Store);

            messageEditor.AutoDBSave = false;  // don't save automatically
            
            var message = Model.Messages.Where(_ => _.KMessageId == messageId).SingleOrDefault();            
            messageEditor.LoadModel(Service, message, false);

            var res = messageEditor.RunModal();
            if (res.Entity == EComponentResult.Executed)
            {                
                return messageEditor.Model;
            }
            else
                return null;
        }

        public async Task<bool> DeleteMessage(Guid messageId)
        {
            var messageEditor = new MessageEditorComponent(Store);

            messageEditor.AutoDBSave = false;  // don't save automatically

            var res = await messageEditor.DeleteModel(Service, messageId);
            if (res)
            {
                var msgDel = Model.Messages.SingleOrDefault(t => t.KMessageId == messageId);
                if (!msgDel.IsNew())
                    Model.MessagesDeleted.Add(messageId);
                Model.Messages.Remove(msgDel);
            }

            return res;
        }

        public async Task<NoteTaskDto> NewTask()
        {
            var taskEditor = new TaskEditorComponent(Store);

            taskEditor.AutoDBSave = false;  // don't save automatically

            await taskEditor.NewModel(Service);
            taskEditor.Model.NoteId = Model.NoteId;            
            var userDto = (await Service.Users.GetByUserNameAsync(Store.AppUserName)).Entity;
            taskEditor.Model.UserId = userDto.UserId;
            taskEditor.Model.UserFullName = userDto.FullName;
            taskEditor.Model.ExpectedStartDate = DateTime.Now;
            taskEditor.Model.SetIsNew(true);

            var res = taskEditor.RunModal();
            if (res.Entity == EComponentResult.Executed)
            {
                Model.Tasks.Add(taskEditor.Model);
                return taskEditor.Model;
            }
            else                            
                return null;
            
        }

        public NoteTaskDto EditTask(Guid taskId)
        {
            var taskEditor = new TaskEditorComponent(Store);

            taskEditor.AutoDBSave = false;  // don't save automatically            

            var task = Model.Tasks.Where(_ => _.NoteTaskId == taskId).SingleOrDefault();            
            taskEditor.LoadModel(Service, task, false);

            var res = taskEditor.RunModal();
            if (res.Entity == EComponentResult.Executed)
            {
                return taskEditor.Model;
            }
            else
                return null;
        }

        public async Task<bool> DeleteTask(Guid taskId)
        {
            var taskEditor = new TaskEditorComponent(Store);
            taskEditor.AutoDBSave = false;  // don't save automatically

            var res = await taskEditor.DeleteModel(Service, taskId);
            if (res)
            {
                var tskDel = Model.Tasks.SingleOrDefault(t => t.NoteTaskId == taskId);
                if (!tskDel.IsNew())
                    Model.TasksDeleted.Add(taskId);
                Model.Tasks.Remove(tskDel);
            }

            return res;
        }

        public async Task<ResourceDto> NewResource()
        {
            var resource = new ResourceEditorComponent(Store);
            resource.AutoDBSave = false;  // don't save automatically

            await resource.NewModel(Service);
            resource.Model.NoteId = Model.NoteId;
            resource.Model.SetIsNew(true);

            var dummy = await Task.FromResult(true);

            var res = resource.RunModal();

            if (res.Entity == EComponentResult.Executed)
            {
                Model.Resources.Add(resource.Model);
                return resource.Model;
            }
            else                        
                return null;            
        }

        public ResourceDto NewResourceFromClipboard()
        {            
            try
            {
                if (!Clipboard.GetDataObject().GetDataPresent(DataFormats.Bitmap))
                {
                    View.ShowInfo("You do not have any images on the Clipboard to insert into this note.", "KaNote");
                    return null;
                }
                var bm = (Bitmap)Clipboard.GetDataObject().GetData(DataFormats.Bitmap);
                var converter = new ImageConverter();
                var newResource = new ResourceDto();

                newResource.ResourceId = Guid.NewGuid();
                newResource.NoteId = Model.NoteId;
                newResource.Description = "Image inserted from clipboard.";
                newResource.Order = 0;
                newResource.Name = "ClipboardImg_" + newResource.ResourceId.ToString() + ".png";            
                newResource.FileType = Store.ExtensionFileToFileType(".png"); 
                newResource.Container = Service.RepositoryRef.ResourcesContainer + @"\" + DateTime.Now.Year.ToString();                                
                newResource.ContentArrayBytes = (byte[])converter.ConvertTo(bm, typeof(byte[]));           
                newResource.ContentBase64 = Convert.ToBase64String(newResource.ContentArrayBytes);
                                
                GetOrSaveTmpFile(
                    Service.RepositoryRef.ResourcesContainerCacheRootPath,
                    newResource.Container,
                    newResource.Name,
                    newResource.ContentArrayBytes);

                Model.Resources.Add(newResource);
                return newResource;
            }
            catch (Exception)
            {
                return null;
            }            
        }

        public async Task<ResourceDto> EditResource(Guid resourceId)
        {
            var resourceEditor = new ResourceEditorComponent(Store);
            resourceEditor.AutoDBSave = false;  // don't save automatically
            
            var resource = Model.Resources.Where(_ => _.ResourceId == resourceId).SingleOrDefault();            
            resourceEditor.LoadModel(Service, resource, false);

            var dummy = await Task.FromResult(true);

            var res = resourceEditor.RunModal();
            if (res.Entity == EComponentResult.Executed)
            {
                return resourceEditor.Model;
            }
            else
                return null;
        }

        public async Task<bool> DeleteResource(Guid resourceId)
        {
            var resource = new ResourceEditorComponent(Store);            
            resource.AutoDBSave = false;  // don't save automatically

            var res = await resource.DeleteModel(Service, resourceId);
            if (res)
            {
                var resDel = Model.Resources.SingleOrDefault(t => t.ResourceId == resourceId);
                if (!resDel.IsNew())
                    if(!resource.AutoDBSave)
                        Model.ResourcesDeleted.Add(resourceId);
                Model.Resources.Remove(resDel);
            }

            return res;
        }

        public void FinalizeAndPostItEdit()
        {
            OnPostItEdit();
            Finalize();
        }

        public void CleanView()
        {
            View.CleanView();
        }

        public void RunScript()
        {
            Store.RunScript(Model.Script);
        }

        #endregion 
    }
}
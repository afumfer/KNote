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

namespace KNote.ClientWin.Components
{    
    public class NoteEditorComponent : ComponentEditorBase<IEditorView<NoteExtendedDto>, NoteExtendedDto>
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
        
        public override async void NewModel(IKntService service)
        {
            try
            {
                Service = service;
                
                var response = await Service.Notes.NewExtendedAsync();
                Model = response.Entity;

                // Evaluate whether to put the following default values in the service layer 
                // (null values are by default, we need empty strings so that the IsDirty is 
                //  not altered after leaving the view when there are no modifications).
                Model.Topic = "";
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

        public override async Task<bool> SaveModel()
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

        #region Other public methods

        public FolderInfoDto GetFolder()
        {
            var folderSelector = new FoldersSelectorComponent(Store);
            var services = new List<ServiceRef>();
            services.Add(Store.GetServiceRef(Service.IdServiceRef));
            folderSelector.ServicesRef = services;
            var res = folderSelector.RunModal();
            if (res.Entity == EComponentResult.Executed)
                return folderSelector.SelectedEntity.FolderInfo;

            return null;
        }

        public async Task<KMessageDto> NewMessage()
        {
            var messageEditor = new MessageEditorComponent(Store);
            messageEditor.AutoDBSave = false;  // don't save automatically
            
            messageEditor.NewModel(Service);            
            messageEditor.Model.NoteId = Model.NoteId;
            messageEditor.Model.Content = "(Aditional text for message)";
            var userDto = (await Service.Users.GetByUserNameAsync(Store.AppUserName)).Entity;
            messageEditor.Model.UserId = userDto.UserId;
            messageEditor.Model.UserFullName = userDto.FullName;
            messageEditor.Model.AlarmActivated = true;
            messageEditor.Model.AlarmDateTime = DateTime.Now;

            var res = messageEditor.RunModal();

            if(res.Entity == EComponentResult.Executed)
            {                
                Model.Messages.Add(messageEditor.Model);
                return messageEditor.Model;
            }            
            else
            {
                View.ShowInfo(res.Message);
                return null;
            }
        }

        public async Task<KMessageDto> EditMessage(KMessageDto message)
        {
            var messageEditor = new MessageEditorComponent(Store);
            messageEditor.AutoDBSave = false;  // don't save automatically

            var entityFound = await messageEditor.LoadModelById(Service, message.KMessageId, false);
            if (!entityFound)
            {
                View.ShowInfo("Message/alarm not fount.");
                return null;
            }

            messageEditor.Model.AlarmDateTime = message.AlarmDateTime;
            messageEditor.Model.AlarmType = message.AlarmType;
            messageEditor.Model.NotificationType = message.NotificationType;
            messageEditor.Model.Content = message.Content;
            messageEditor.Model.AlarmActivated = message.AlarmActivated;

            var res = messageEditor.RunModal();
            if (res.Entity == EComponentResult.Executed)
            {                
                var itemToRemove = Model.Messages.Single(m => m.KMessageId == message.KMessageId);
                Model.Messages.Remove(itemToRemove);                
                Model.Messages.Add(messageEditor.Model);
                return messageEditor.Model;
            }
            else
                return null;
        }

        public async Task<bool> DeleteMessage(KMessageDto message)
        {
            var messageEditor = new MessageEditorComponent(Store);
            messageEditor.AutoDBSave = false;  // don't save automatically

            var res = await messageEditor.DeleteModel(Service, message.KMessageId);
            if (res)
            {
                var msgDel = Model.Messages.SingleOrDefault(m => m.KMessageId == message.KMessageId 
                    && m.AlarmDateTime == message.AlarmDateTime && m.Content == message.Content && m.AlarmActivated == message.AlarmActivated
                    && m.AlarmType == message.AlarmType && m.NotificationType == message.NotificationType);
                Model.Messages.Remove(msgDel);
                if (message.KMessageId != Guid.Empty)
                    Model.MessagesDeleted.Add(message.KMessageId);
            }

            return res;
        }

        #endregion 
    }
}
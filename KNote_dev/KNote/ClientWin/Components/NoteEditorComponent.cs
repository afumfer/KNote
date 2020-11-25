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

namespace KNote.ClientWin.Components
{
    public class NoteEditorComponent : ComponentViewBase<IEditorView<NoteDto>>
    {
        #region Properties

        private NoteDto _noteEdit;
        public NoteDto NoteEdit
        {
            set
            {
                _noteEdit = value;
            }
            get
            {
                if (_noteEdit == null)
                    _noteEdit = new NoteDto();
                return _noteEdit;
            }
        }

        private List<ResourceDto> _noteEditResources;
        public List<ResourceDto> NoteEditResources
        {
            get
            {
                if (_noteEditResources == null)
                    _noteEditResources = new List<ResourceDto>();
                return _noteEditResources;
            }
        }

        private List<NoteTaskDto> _noteEditTasks;
        public List<NoteTaskDto> NoteEditTasks
        {
            get
            {
                if (_noteEditTasks == null)
                    _noteEditTasks = new List<NoteTaskDto>();
                return _noteEditTasks;
            }
        }

        private List<KMessageDto> _noteEditMessages;
        public List<KMessageDto> NoteEditMessages
        {
            get
            {
                if (_noteEditMessages == null)
                    _noteEditMessages = new List<KMessageDto>();
                return _noteEditMessages;
            }
        }

        private IKntService _service;

        #endregion

        #region Constructor

        public NoteEditorComponent(Store store) : base(store)
        {
        }

        #endregion

        #region IEditorView implementation

        protected override IEditorView<NoteDto> CreateView()
        {
            return Store.FactoryViews.View(this);
        }

        #endregion

        #region Component specific public members

        public async void LoadNoteById(FolderWithServiceRef folderWithServiceRef, Guid noteId)
        {
            try
            {
                _service = folderWithServiceRef.ServiceRef.Service;

                _noteEdit = (await _service.Notes.GetAsync(noteId)).Entity;
                _noteEditResources = (await _service.Notes.GetResourcesAsync(noteId)).Entity;
                _noteEditTasks = (await _service.Notes.GetNoteTasksAsync(noteId)).Entity;
                _noteEditMessages = (await _service.Notes.GetMessagesAsync(noteId)).Entity;

                View.RefreshView();
            }
            catch (Exception ex)
            {
                View.ShowInfo(ex.Message);
                
            }
        }

        public async void LoadNewNote(FolderWithServiceRef folderWithServiceRef)
        {
            try
            {
                _service = folderWithServiceRef.ServiceRef.Service;

                var response = await _service.Notes.NewAsync();
                _noteEdit = response.Entity;
                _noteEdit.FolderId = folderWithServiceRef.FolderInfo.FolderId;
                _noteEdit.FolderDto = folderWithServiceRef.FolderInfo.GetSimpleDto<FolderDto>();

                View.RefreshView();
            }
            catch (Exception ex)
            {
                View.ShowInfo(ex.Message);                
            }

        }

        public async void SaveNote()
        {
            var isNew = (_noteEdit.NoteId == Guid.Empty);
                        
            var msgVal = NoteEdit.GetErrorMessage();
            if (!string.IsNullOrEmpty(msgVal))
            {
                View.ShowInfo(msgVal);
                return;
            }

            try
            {
                var response = await _service.Notes.SaveAsync(NoteEdit);

                if (response.IsValid)
                {
                    _noteEdit = response.Entity;

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
        
        public async Task<bool> DeleteNote(IKntService service, Guid noteId) 
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
            
        public async Task<bool> DeleteNote()
        {
            return await DeleteNote(_service, NoteEdit.NoteId);
        }

        public void RefreshNote(NoteDto note)
        {
            NoteEdit = note;
            View.RefreshView();
        }

        public event EventHandler<ComponentEventArgs<NoteDto>> SavedEntity;
        protected virtual void OnSavedEntity(NoteDto entity)
        {
            SavedEntity?.Invoke(this, new ComponentEventArgs<NoteDto>(entity));
        }

        public event EventHandler<ComponentEventArgs<NoteDto>> AddedEntity;
        protected virtual void OnAddedEntity(NoteDto entity)
        {
            AddedEntity?.Invoke(this, new ComponentEventArgs<NoteDto>(entity));
        }

        public event EventHandler<ComponentEventArgs<NoteDto>> DeletedEntity;
        protected virtual void OnDeletedEntity(NoteDto entity)
        {
            DeletedEntity?.Invoke(this, new ComponentEventArgs<NoteDto>(entity));
        }

        #endregion 
    }
}

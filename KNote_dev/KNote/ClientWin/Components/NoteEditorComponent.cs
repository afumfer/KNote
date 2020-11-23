using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service;

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
            _service = folderWithServiceRef.ServiceRef.Service;

            _noteEdit = (await _service.Notes.GetAsync(noteId)).Entity;
            _noteEditResources = (await _service.Notes.GetResourcesAsync(noteId)).Entity;
            _noteEditTasks = (await _service.Notes.GetNoteTasksAsync(noteId)).Entity;
            _noteEditMessages = (await _service.Notes.GetMessagesAsync(noteId)).Entity;

            View.RefreshView();
        }

        public void RefreshNote(NoteDto note)
        {
            NoteEdit = note;
            View.RefreshView();
        }

        public async void SaveModelAction()
        {
            var response = await _service.Notes.SaveAsync(NoteEdit);
            _noteEdit = response.Entity;
            OnSavedEntity(response.Entity);
            View.RefreshView();            
        }

        public event EventHandler<ComponentEventArgs<NoteDto>> SavedEntity;
        protected virtual void OnSavedEntity(NoteDto entity)
        {
            SavedEntity?.Invoke(this, new ComponentEventArgs<NoteDto>(entity));
        }


        #endregion 
    }
}

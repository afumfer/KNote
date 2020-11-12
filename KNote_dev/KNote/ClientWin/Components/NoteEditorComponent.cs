using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Components
{
    public class NoteEditorComponent : ComponentViewBase<IEditorView<NoteDto>>
    {
        public NoteEditorComponent(Store store) : base(store)
        {
        }

        protected override IEditorView<NoteDto> CreateView()
        {
            return Store.FactoryViews.View(this);
        }

        #region Component specific public members

        private NoteDto _noteEdit;
        public NoteDto NoteEdit
        {
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

        public async void LoadNoteById(FolderWithServiceRef folderWithServiceRef, Guid noteId)
        {
            var service = folderWithServiceRef.ServiceRef.Service;

            _noteEdit = (await service.Notes.GetAsync(noteId)).Entity;
            _noteEditResources = (await service.Notes.GetNoteResourcesAsync(noteId)).Entity;
            _noteEditTasks = (await service.Notes.GetNoteTasksAsync(noteId)).Entity;

            View.RefreshView();
        }

        //public async void LoadNoteResourcesById(FolderWithServiceRef folderWithServiceRef, Guid noteId)
        //{
        //    var service = folderWithServiceRef.ServiceRef.Service;

        //    _noteEditResources = (await service.Notes.GetNoteResourcesAsync(noteId)).Entity;

        //    View.RefreshView();
        //}

        //public async void LoadNoteTasksById(FolderWithServiceRef folderWithServiceRef, Guid noteId)
        //{
        //    var service = folderWithServiceRef.ServiceRef.Service;

        //    _noteEditTasks = (await service.Notes.GetNoteTasksAsync(noteId)).Entity;

        //    View.RefreshView();
        //}

        #endregion 
    }
}

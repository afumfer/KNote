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

        public async void LoadNoteById(FolderWithServiceRef folderWithServiceRef, Guid noteId)
        {
            var service = folderWithServiceRef.ServiceRef.Service;

            _noteEdit = (await service.Notes.GetAsync(noteId)).Entity;

            View.RefreshView();
        }

        #endregion 
    }
}

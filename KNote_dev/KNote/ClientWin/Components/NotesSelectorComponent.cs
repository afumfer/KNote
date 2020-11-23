﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service;

namespace KNote.ClientWin.Components
{
    public class NotesSelectorComponent : ComponentSelectorViewBase<ISelectorView<NoteInfoDto>, NoteInfoDto>
    {
        #region Properties

        private IKntService _service;
        public IKntService Service
        {
            get { return _service; }
        }

        private FolderInfoDto _folder;
        public FolderInfoDto Folder
        {
            get { return _folder; }
        }

        private List<NoteInfoDto> _listNotes;
        public List<NoteInfoDto> ListNotes
        {
            get { return _listNotes; }
        }

        #endregion

        #region Constructor

        public NotesSelectorComponent(Store store) : base(store)
        {

        }

        #endregion

        #region ISelectorView implementation

        protected override ISelectorView<NoteInfoDto> CreateView()
        {
            return Store.FactoryViews.View(this);
        }

        #endregion 

        #region Component specific public members


        public async void LoadNotesByFolderAsync(FolderWithServiceRef folderWithServiceRef)
        {
            _service = folderWithServiceRef.ServiceRef.Service;
            _folder = folderWithServiceRef.FolderInfo;

            if (_folder == null)
                return;

            _listNotes = (await _service.Notes.GetByFolderAsync(_folder.FolderId)).Entity;

            View.RefreshView();

            if (_listNotes?.Count > 0)
                SelectedEntity = _listNotes[0];
            else
                SelectedEntity = null;

            NotifySelectedEntity();
        }

        public void RefreshNote(NoteInfoDto note)
        {
            if(Folder.FolderId == note.FolderId)
            {
                var updateNote = _listNotes.FirstOrDefault(_ => _.NoteId == note.NoteId);
                if(updateNote != null)
                {
                    updateNote.SetSimpleDto(note);
                    View.RefreshItem(updateNote);
                }

            }
        }

        #endregion
    }
}

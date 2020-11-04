using System;
using System.Collections.Generic;
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
        public NotesSelectorComponent(Store store) : base(store)
        {

        }

        protected override ISelectorView<NoteInfoDto> CreateView()
        {
            return Store.FactoryViews.View(this);
        }

        protected override Result OnInitialized()
        {
            var result = base.OnInitialized();

            View.ShowView();

            return result;
        }

        #region Component specific public members

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

        public async void LoadNotesByFolderAsync(FolderWithServiceRef folderWithServiceRef)
        {
            _service = folderWithServiceRef.ServiceRef.Service;
            _folder = folderWithServiceRef.FolderInfo;

            if (_folder == null)
                return;

            _listNotes = (await _service.Notes.GetByFolderAsync(_folder.FolderId)).Entity;

            View.RefreshView();            
        }

        #endregion
    }
}

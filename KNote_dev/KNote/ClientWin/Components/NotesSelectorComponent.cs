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

        public async void GetListNotesAsync(FolderWithServiceRef folderWithServiceRef)
        {
            _service = folderWithServiceRef.ServiceRef.Service;
            _folder = folderWithServiceRef.FolderInfo;

            if (_folder == null)
                return;

            _listNotes = (await _service.Notes.GetByFolderAsync(_folder.FolderId)).Entity;

            View.RefreshView();            
        }


        //protected override void LoadDataSelector()
        //{
        //    try
        //    {
        //        if (ListEntities != null)
        //            ListEntities.Clear();
        //        else
        //            ListEntities = new List<NoteItemDto>();

        //        if (DataSource != null)
        //        {
        //            if (DataSource.NotesFilter.Folder == null)
        //                ListEntities = DataSource.ServiceRef.Service
        //                    .Notes.GetNoteItemList(null).Entity;
        //            else
        //                ListEntities = DataSource.ServiceRef.Service
        //                    .Notes.GetNoteItemList(DataSource.NotesFilter.Folder.FolderId).Entity;
        //            .Notes3.GetNoteItemList(n => n.FolderId == DataSource.NotesFilter.Folder.FolderId).Entity;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string msg = "KMSG: Ha ocurrido el siguiente error al cargar la lista de notas: >> ";
        //        msg += ex.Message;
        //        ShowMessage(msg, "KMSG: Error en la carga de la lista de notas");
        //    }
        //}

        #endregion
    }
}

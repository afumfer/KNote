using System;
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
    public class NotesSelectorComponent : ComponentSelectorBase<ISelectorView<NoteInfoDto>, NoteInfoDto>
    {
        #region Properties

        public FolderInfoDto Folder
        {
            get;
            private set;
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

        #region Component virtual / abstract public members

        public override void LoadEntities(IKntService service)
        {
            throw new NotImplementedException();
        }

        public async void LoadEntities(IKntService service, FolderInfoDto folder)
        {
            try
            {
                Service = service;
                Folder = folder;

                if (Folder == null)
                    return;

                var response = await Service.Notes.GetByFolderAsync(Folder.FolderId);

                if (response.IsValid)
                {
                    ListEntities = response.Entity;

                    View.RefreshView();

                    if (ListEntities?.Count > 0)
                        SelectedEntity = ListEntities[0];
                    else
                        SelectedEntity = null;

                    NotifySelectedEntity();
                }
                else
                    View.ShowInfo(response.Message);
            }
            catch (Exception ex)
            {
                View.ShowInfo(ex.Message);                
            }
        }

        public override void SelectItem(NoteInfoDto item)
        {
            throw new NotImplementedException();
        }

        public override void RefreshItem(NoteInfoDto note)
        {
            if(Folder.FolderId == note.FolderId)
            {
                var updateNote = ListEntities.FirstOrDefault(_ => _.NoteId == note.NoteId);
                if(updateNote != null)
                {
                    updateNote.SetSimpleDto(note);
                    View.RefreshItem(updateNote);
                }

            }
        }

        public override void AddItem(NoteInfoDto note)
        {
            if (Folder.FolderId == note.FolderId)
            {                
                ListEntities.Add(note);
                View.AddItem(note);             
            }
        }

        public override void DeleteItem(NoteInfoDto note)
        {
            if (Folder.FolderId == note.FolderId)
            {                                
                ListEntities.RemoveAll( _ => _.NoteId == note.NoteId);                
                View.DeleteItem(note);             
            }
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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

        public NotesFilterDto NotesFilter
        {
            get;
            private set;
        }


        #endregion

        #region Constructor

        public NotesSelectorComponent(Store store) : base(store)
        {
            ComponentName = "Notes selector";
        }

        #endregion

        #region ISelectorView implementation

        protected override ISelectorView<NoteInfoDto> CreateView()
        {
            return Store.FactoryViews.View(this);
        }

        #endregion 

        #region Component virtual / abstract public members

        public override async Task<bool> LoadEntities(IKntService service, bool refreshView = true)
        {
            return await LoadEntities(service, null, refreshView);
        }

        public async Task<bool> LoadEntities(IKntService service, FolderInfoDto folder, bool refreshView = true)
        {
            try
            {
                Service = service;
                Folder = folder;
                NotesFilter = null;

                Guid f;
                if (Folder == null)
                    f = Guid.Empty;                     
                else 
                    f = Folder.FolderId;

                Result<List<NoteInfoDto>> response;
                if(folder == null)
                    response = await Service.Notes.GetAllAsync();
                else 
                    response = await Service.Notes.GetByFolderAsync(f);

                if (response.IsValid)
                {
                    ListEntities = response.Entity;

                    if(refreshView)
                        View.RefreshView();

                    if (ListEntities?.Count > 0)
                        SelectedEntity = ListEntities[0];
                    else
                        SelectedEntity = null;

                    NotifySelectedEntity();
                }
                else
                {
                    View.ShowInfo(response.Message);
                    return false;
                }
            }
            catch (Exception ex)
            {
                View.ShowInfo(ex.Message);
                return false;
            }

            return true;
        }

        public async Task<bool> LoadFilteredEntities(IKntService service, NotesFilterDto notesFilter, bool refreshView = true)
        {
            bool resLoad = false;
            try
            {
                // TODO: provisional, hay que buscar una solución más generalista a los estados de espera. 
                Cursor.Current = Cursors.WaitCursor;

                Result<List<NoteInfoDto>> response;
                Service = service;
                Folder = null;                                
                                
                if (string.IsNullOrEmpty(notesFilter?.TextSearch.Trim()) || service == null || notesFilter == null)
                {
                    response = new Result<List<NoteInfoDto>>();
                    response.Entity = new List<NoteInfoDto>();                    
                }
                else
                {
                    // TODO: hack for get all (app win don't have pagination) 
                    //response = await Service.Notes.GetFilter(notesFilter);   //TODO: future                    
                    notesFilter.NumRecords = 99999;
                    var xx = notesFilter.Pagination.NumRecords;
                    response = await Service.Notes.GetSearch(notesFilter);
                }

                if (response.IsValid)
                {
                    ListEntities = response.Entity;
                    NotesFilter = notesFilter;

                    if (refreshView)
                        View.RefreshView();

                    if (ListEntities?.Count > 0)
                        SelectedEntity = ListEntities[0];
                    else
                        SelectedEntity = null;

                    NotifySelectedEntity();
                    resLoad = await Task.FromResult<bool>(true);
                }
                else
                {
                    View.ShowInfo(response.Message);
                    resLoad = await Task.FromResult<bool>(false); 
                }
            }
            catch (Exception ex)
            {
                View.ShowInfo(ex.Message);
                resLoad = await Task.FromResult<bool>(false);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            return resLoad;
        }

        public override void SelectItem(NoteInfoDto item)
        {
            throw new NotImplementedException();
        }

        public override void RefreshItem(NoteInfoDto note)
        {            
            var updateNote = ListEntities?.FirstOrDefault(_ => _.NoteId == note.NoteId);
            if (updateNote == null)
                return;

            if (Folder == null)
            {
                updateNote.SetSimpleDto(note);
                View.RefreshItem(updateNote);
                return;
            }

            if (Folder.FolderId == note.FolderId)
            {
                updateNote.SetSimpleDto(note);
                View.RefreshItem(updateNote);
            }           
            else
            {
                ListEntities.RemoveAll(_ => _.NoteId == note.NoteId);
                View.DeleteItem(note);                
            }
        }

        public override void AddItem(NoteInfoDto note)
        {
            if (Folder == null)
                return;

            if (Folder.FolderId == note?.FolderId)
            {                
                ListEntities.Add(note);
                View.AddItem(note);             
            }
        }

        public override void DeleteItem(NoteInfoDto note)
        {
            var entiesFoud = ListEntities?.Where(_ => _.NoteId == note.NoteId).Select(_ => _.NoteId).ToList();
            if (entiesFoud?.Count > 0)
            {
                ListEntities.RemoveAll(_ => _.NoteId == note.NoteId);
                View.DeleteItem(note);
            }
        }

        #endregion
    }
}

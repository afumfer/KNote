using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

public class NotesSelectorCtrl : CtrlSelectorBase<IViewSelector<NoteMinimalDto>, NoteMinimalDto>
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

    public NotesSearchDto NotesSearch
    {
        get;
        private set;
    }

    public string HiddenColumns
    {
        get; set;
    }

    public string ViewTitle
    {
        get; set;
    }

    #endregion

    #region Constructor

    public NotesSelectorCtrl(Store store) : base(store)
    {
        ControllerName = "Notes selector";
        HiddenColumns = "";
    }

    #endregion

    #region ISelectorView implementation

    protected override IViewSelector<NoteMinimalDto> CreateView()
    {
        return Store.FactoryViews.View(this);
    }

    #endregion 

    #region Controller virtual / abstract public members

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

            Result<List<NoteMinimalDto>> response;
            if(folder == null)
                response = await Service.Notes.GetAllMinimalAsync();
            else 
                response = await Service.Notes.GetByFolderMinimalAsync(f);

            if (response.IsValid)
            {
                ListEntities = response.Entity;

                if(refreshView)
                    View.RefreshView();

                if (ListEntities?.Count > 0)
                    SelectedEntity = ListEntities[0];
                else
                    SelectedEntity = null;
            }
            else
            {
                View.ShowInfo(response.ErrorMessage);
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

    public async Task<bool> LoadSearchEntities(IKntService service, NotesSearchDto notesSearch, bool refreshView = true)
    {
        bool resLoad = false;
        try
        {            
            Cursor.Current = Cursors.WaitCursor;
            
            Service = service;
            Folder = null;

            Result<List<NoteMinimalDto>> response;
            if (string.IsNullOrEmpty(notesSearch?.TextSearch.Trim()) || service == null || notesSearch == null)
            {
                response = new Result<List<NoteMinimalDto>>();
                response.Entity = new List<NoteMinimalDto>();                    
            }
            else
            {
                response = await Service.Notes.GetSearchMinimal(notesSearch);                
            }

            if (response.IsValid)
            {
                ListEntities = response.Entity;
                NotesSearch = notesSearch;

                if (refreshView)
                    View.RefreshView();

                if (ListEntities?.Count > 0)
                    SelectedEntity = ListEntities[0];
                else
                    SelectedEntity = null;
            }
            else
            {
                View.ShowInfo(response.ErrorMessage);
                resLoad = false;
            }
        }
        catch (Exception ex)
        {
            View.ShowInfo(ex.Message);
            resLoad = false;
        }
        finally
        {
            Cursor.Current = Cursors.Default;
        }

        return resLoad;
    }

    public async Task<bool> LoadFilteredEntities(IKntService service, NotesFilterDto notesFilter, bool refreshView = true)
    {
        bool resLoad = false;
        try
        {
            Cursor.Current = Cursors.WaitCursor;

            Service = service;
            Folder = null;

            Result<List<NoteMinimalDto>> response;
            if (service == null || notesFilter == null)
            {
                response = new Result<List<NoteMinimalDto>>();
                response.Entity = new List<NoteMinimalDto>();
            }
            else
            {
                response = await Service.Notes.GetFilterMinimal(notesFilter);
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
            }
            else
            {
                View.ShowInfo(response.ErrorMessage);
                resLoad = false;
            }
        }
        catch (Exception ex)
        {
            View.ShowInfo(ex.Message);
            resLoad = false;
        }
        finally
        {
            Cursor.Current = Cursors.Default;
        }

        return resLoad;
    }

    public override void SelectItem(NoteMinimalDto item)
    {
        throw new NotImplementedException();
    }

    public override void RefreshItem(NoteMinimalDto note)
    {
        var updateNote = ListEntities?.FirstOrDefault(_ => _.NoteId == note.NoteId);

        if (Folder == null)
        {                
            if (updateNote != null)
            {
                updateNote.SetSimpleDto(note);
                View.RefreshItem(updateNote);
            }
        }
        else
        {
            if (Folder.FolderId == note.FolderId)
            {
                if (updateNote != null)
                {
                    updateNote.SetSimpleDto(note);
                    View.RefreshItem(updateNote);
                }
                else
                {
                    ListEntities.Add(note);
                    View.AddItem(note);
                }
            }
            else
            {
                if(updateNote != null)
                {
                    ListEntities.RemoveAll(_ => _.NoteId == note.NoteId);
                    View.DeleteItem(note);
                }
            }
        }
    }

    public override void AddItem(NoteMinimalDto note)
    {
        RefreshItem(note);
    }

    public override void DeleteItem(NoteMinimalDto note)
    {
        var entiesFoud = ListEntities?.Where(_ => _.NoteId == note.NoteId).Select(_ => _.NoteId).ToList();
        if (entiesFoud?.Count > 0)
        {
            ListEntities.RemoveAll(_ => _.NoteId == note.NoteId);
            View.DeleteItem(note);
        }
    }

    #endregion

    #region Extra methods
    
    public List<NoteMinimalDto> GetSelectedListNotesMinimal()
    {
        return View.GetSelectedListItem();
    }

    public void CleanView()
    {
        ListEntities = new List<NoteMinimalDto>();            
        View.RefreshView();
    }

    #endregion 
}

using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using Microsoft.Identity.Client;

namespace KNote.ClientWin.Controllers;

public class PostItEditorCtrl : CtrlNoteEditorBase<IViewPostIt<NoteDto>, NoteDto>
{
    #region Private fields

    private Guid _userId = Guid.Empty;

    #endregion

    #region Public properties

    public WindowDto WindowPostIt { get; set; }

    public FolderWithServiceRef FolderWithServiceRef { get; set; }

    public bool ForceAlwaysTop { get; set; }

    #endregion 

    #region Constructor, Dispose, ...

    public PostItEditorCtrl(Store store): base(store)
    {
        ControllerName = "PostIt editor";
        Store.DeletedNote += Store_DeletedNote;
    }

    public override void Dispose()
    {
        Store.DeletedNote -= Store_DeletedNote;
        base.Dispose();
    }

    #endregion 

    #region Store events 

    private void Store_DeletedNote(object sender, ControllerEventArgs<NoteExtendedDto> e)
    {
        if (e.Entity.NoteId == this.Model.NoteId)
            this.Finalize();
    }

    #endregion 

    #region Component specific events 

    public event EventHandler<ControllerEventArgs<ServiceWithNoteId>> ExtendedEdit;
    protected virtual void OnExtendedEdit()
    {
        ExtendedEdit?.Invoke(this, new ControllerEventArgs<ServiceWithNoteId>(new ServiceWithNoteId { Service = Service, NoteId = Model.NoteId }));
    }

    #endregion

    #region IEditorView implementation

    protected override IViewPostIt<NoteDto> CreateView()
    {
        return Store.FactoryViews.View(this);
    }

    #endregion 

    #region ComponentEditor override methods

    public async override Task<bool> LoadModelById(IKntService service, Guid noteId, bool refreshView = true)
    {
        try
        {
            Service = service;

            Model = (await Service.Notes.GetAsync(noteId)).Entity;
            Model.SetIsDirty(false);

            FolderWithServiceRef = new FolderWithServiceRef { ServiceRef = Store.GetServiceRef(service.IdServiceRef), FolderInfo = Model?.FolderDto };

            var resGetWindow = await Service.Notes.GetWindowAsync(Model.NoteId, await PostItGetUserId());
            if (resGetWindow.IsValid)
                WindowPostIt = resGetWindow.Entity;
            else
                WindowPostIt = await GetNewWindowPostIt();

            WindowPostIt.Visible = true;
            await Service.Notes.SaveWindowAsync(WindowPostIt);                
            WindowPostIt.SetIsDirty(false);

            if (refreshView)
                View.RefreshView();
            return true;
        }
        catch (Exception ex)
        {
            View.ShowInfo(ex.Message);
            return false;
        }
    }

    public async override Task<bool> NewModel(IKntService service)
    {
        try
        {
            Service = service;

            var response = await Service.Notes.NewAsync();
            Model = response.Entity;

            // Evaluate whether to put the following default values in the service layer 
            // (null values are by default, we need empty strings so that the IsDirty is 
            //  not altered after leaving the view when there are no modifications).
            Model.Topic = DateTime.Now.ToString();
            Model.Tags = "";
            Model.Description = "";            

            // Context default values
            if (FolderWithServiceRef == null)
                FolderWithServiceRef = Store.ActiveFolderWithServiceRef;
            Model.FolderId = FolderWithServiceRef.FolderInfo.FolderId;
            Model.FolderDto = FolderWithServiceRef.FolderInfo.GetSimpleDto<FolderDto>();

            WindowPostIt = await GetNewWindowPostIt();

            Model.SetIsDirty(true);
            WindowPostIt.SetIsDirty(true);

            View.RefreshView();

            return true;
        }
        catch (Exception ex)
        {
            View.ShowInfo(ex.Message);
        }

        return false;
    }

    public async override Task<bool> SaveModel()
    {
        View.RefreshModel();

        if (!Model.IsDirty() && !WindowPostIt.IsDirty() )
            return true;

        try
        {
            var isNew = (Model.NoteId == Guid.Empty);

            var msgVal = Model.GetErrorMessage();
            if (!string.IsNullOrEmpty(msgVal))
            {
                View.ShowInfo(msgVal);
                return false;
            }

            var response = await Service.Notes.SaveAsync(Model);

            if (response.IsValid)
            {
                Model = response.Entity;

                Model.SetIsDirty(false);
                Model.SetIsNew(false);
                    
                if (!isNew)
                    OnSavedEntity(response.Entity);
                else
                    OnAddedEntity(response.Entity);                    
            }
            else
            {
                View.ShowInfo(response.ErrorMessage);
            }

            if (WindowPostIt != null)
            {                    
                if (WindowPostIt.NoteId == Guid.Empty)
                    WindowPostIt.NoteId = Model.NoteId;
                var responseWinPostIt = await Service.Notes.SaveWindowAsync(WindowPostIt);
                WindowPostIt.SetIsDirty(false);
            }
        }
        catch (Exception ex)
        {
            View.ShowInfo(ex.Message);
            return true;
        }

        return true;
    }

    public async override Task<bool> DeleteModel(IKntService service, Guid noteId)
    {
        var result = View.ShowInfo("Are you sure you want to delete this note?", "Delete note", MessageBoxButtons.YesNo);
        if (result == DialogResult.Yes || result == DialogResult.Yes)
        {
            try
            {
                if (noteId == Guid.Empty)
                    return true;

                var response = await service.Notes.DeleteExtendedAsync(noteId);                    

                if (response.IsValid)
                {
                    OnDeletedEntity(response.Entity);
                    return true;
                }
                else
                    View.ShowInfo(response.ErrorMessage);
            }
            catch (Exception ex)
            {
                View.ShowInfo(ex.Message);
            }
        }
        return false;
    }

    public async override Task<bool> DeleteModel()
    {
        return await DeleteModel(Service, Model.NoteId);
    }

    #endregion

    #region Component specific methods

    public async Task<bool> SaveAndHide()
    {
        WindowPostIt.Visible = false;
        var res = await SaveModel();
        Finalize();
        return res;
    }

    public async Task<bool> DeleteAndFinalize()
    {
        var res = await DeleteModel();
        if (res)
            Finalize();
        return res;
    }

    public async Task<bool> ExtendedNoteEdit()
    {
        WindowPostIt.Visible = false;
        var res = await SaveModel();
        FinalizeAndExtendEdit();
        return res;
    }

    public async Task<bool> FastAlarmAndHide(string unitTime, int value)
    {            
        WindowPostIt.Visible = false;

        bool resSave = true;

        if (Model.IsNew())
            resSave = await SaveModel();

        if (resSave)
        {
            var resAlarm = await SaveFastModelAlarm(unitTime, value);

            Model.InternalTags = "(need to be updated)";
            var resSaveStatus = await SaveModel();

            if (resAlarm && resSaveStatus)
                Finalize();
            else                    
                View.ShowInfo("The alarm could not be saved.");

            return resAlarm;
        }
        else
        {                
            View.ShowInfo("This note could not be saved.");
            return false;
        }
    }

    private async Task<bool> SaveFastModelAlarm(string unitTime, int value)
    {
        DateTime? alarmDateTime = null;

        switch (unitTime)
        {
            case "m":
                alarmDateTime = DateTime.Now.AddMinutes(value);
                break;
            case "h":
                alarmDateTime = DateTime.Now.AddMinutes(value * 60);
                break;
            case "week":
                alarmDateTime = DateTime.Now.AddDays(7);
                break;
            case "month":
                alarmDateTime = DateTime.Now.AddMonths(1);
                break;
            case "year":
                alarmDateTime = DateTime.Now.AddYears(1);
                break;
            default:
                break;
        }

        var alarm = new KMessageDto
        {
            NoteId = Model.NoteId,
            UserId = await PostItGetUserId(),
            ActionType = EnumActionType.UserAlarm,
            NotificationType = EnumNotificationType.PostIt,
            AlarmType = EnumAlarmType.Standard,
            AlarmDateTime = alarmDateTime,
            AlarmMinutes = 0,
            Comment = "(Fast alarm)",
            AlarmActivated = true

        };
            
        var resSaveMsg = await Service.Notes.SaveMessageAsync(alarm, true);
            
        return resSaveMsg.IsValid;
    }

    public async Task<bool> FastTaskAndHide()
    {
        WindowPostIt.Visible = false;

        bool resSave = true;

        if (Model.IsNew())
            resSave = await SaveModel();

        if (resSave)
        {
            var resAlarm = await SaveFastModelTask();

            Model.InternalTags = "(need to be updated)";
            var resSaveStatus = await SaveModel();

            if (resAlarm && resSaveStatus)
                Finalize();
            else
                View.ShowInfo("Task could not be saved.");

            return resAlarm;
        }
        else
        {
            View.ShowInfo("This note could not be saved.");
            return false;
        }
    }

    private async Task<bool> SaveFastModelTask()
    {
        var task = new NoteTaskDto
        {
            NoteId = Model.NoteId,
            UserId = await PostItGetUserId(),
            Description = Model.Topic,
            Tags = "Automatic  task",
            Resolved = true,
            EndDate = DateTime.Now
        };

        var resSaveTask = await Service.Notes.SaveNoteTaskAsync(task, true);

        return resSaveTask.IsValid;
    }

    public void FinalizeAndExtendEdit()
    {            
        OnExtendedEdit();
        Finalize();            
    }

    public virtual WindowDto GetWindow()
    {
        var window = new PostItPropertiesCtrl(Store);
        window.LoadModel(Service, WindowPostIt, false);

        var res = window.RunModal();
        if (res.Entity == EControllerResult.Executed)
            return window.Model;

        return null;
    }

    public void HidePostIt() 
    {
        View.HideView();
    }

    public void ActivatePostIt()
    {
        View.ActivateView();
    }

    #endregion

    #region Private methods

    private async Task<WindowDto> GetNewWindowPostIt()
    {            
        if (Model == null)
            return null;

        var random = new Random();

        // TODO: get default values from Store.AppConfig ...
        return new WindowDto {
            NoteId = Model.NoteId,
            UserId = await PostItGetUserId(),
            PosX = random.Next(50, 150),
            PosY = random.Next(50, 150),
            Visible = true,                
            AlwaysOnTop = true,
            Width = 400,
            Height = 300,
            FontName = "Segoe UI",
            FontSize = 10,
            FontBold = false,
            FontItalic = false,
            FontStrikethru = false,
            FontUnderline = false,
            ForeColor = "Black",
            NoteColor = "#FFFFC0",
            TitleColor = "#FFFF80",
            TextNoteColor = "Black",
            TextTitleColor = "Black"
        };
    }

    private async Task<Guid> PostItGetUserId()
    {
        if (_userId != Guid.Empty)
            return _userId;

        var userId = await Store.GetUserId(Service);
        if (userId != null)
            return (Guid)userId;
        else
            return _userId;
    }

    #endregion 
}


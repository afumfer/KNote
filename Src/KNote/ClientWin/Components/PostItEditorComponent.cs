﻿using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service;

namespace KNote.ClientWin.Components;

public class PostItEditorComponent : ComponentEditor<IEditorViewExt<NoteDto>, NoteDto>
{
    private Guid _userId = Guid.Empty;

    public WindowDto WindowPostIt { get; set; }

    public FolderWithServiceRef FolderWithServiceRef { get; set; }

    public bool ForceAlwaysTop { get; set; }

    #region Constructor

    public PostItEditorComponent(Store store): base(store)
    {
        ComponentName = "PostIt editor";            
    }

    #endregion 

    #region Componet specific events 

    public event EventHandler<ComponentEventArgs<ServiceWithNoteId>> ExtendedEdit;
    protected virtual void OnExtendedEdit()
    {
        ExtendedEdit?.Invoke(this, new ComponentEventArgs<ServiceWithNoteId>(new ServiceWithNoteId { Service = Service, NoteId = Model.NoteId }));
    }

    #endregion

    #region IEditorView implementation

    protected override IEditorViewExt<NoteDto> CreateView()
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

            var resGetWindow = await Service.Notes.GetWindowAsync(Model.NoteId, await GetUserId());
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
            Model.ContentType = "markdown";

            // Context default values
            if (FolderWithServiceRef == null)
                FolderWithServiceRef = Store.ActiveFolderWithServiceRef;
            Model.FolderId = FolderWithServiceRef.FolderInfo.FolderId;
            Model.FolderDto = FolderWithServiceRef.FolderInfo.GetSimpleDto<FolderDto>();

            WindowPostIt = await GetNewWindowPostIt();

            Model.SetIsDirty(true);
            WindowPostIt.SetIsDirty(true);

            View.RefreshView();

            return await Task.FromResult<bool>(true);
        }
        catch (Exception ex)
        {
            View.ShowInfo(ex.Message);
        }

        return await Task.FromResult<bool>(false);
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
                View.ShowInfo(response.Message);
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
                    return await Task.FromResult<bool>(true);

                var response = await service.Notes.DeleteExtendedAsync(noteId);                    

                if (response.IsValid)
                {
                    OnDeletedEntity(response.Entity);
                    return true;
                }
                else
                    View.ShowInfo(response.Message);
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
            return await Task.FromResult<bool>(false);
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
            UserId = await GetUserId(),
            ActionType = EnumActionType.NoteAlarm,
            NotificationType = EnumNotificationType.PostIt,
            AlarmType = EnumAlarmType.Standard,
            AlarmDateTime = alarmDateTime,
            AlarmMinutes = 0,
            Comment = "(Fast alarm)",
            AlarmActivated = true

        };
            
        var resSaveMsg = await Service.Notes.SaveMessageAsync(alarm, true);
            
        return await Task.FromResult<bool>(resSaveMsg.IsValid);
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
            return await Task.FromResult<bool>(false);
        }
    }

    private async Task<bool> SaveFastModelTask()
    {
        var task = new NoteTaskDto
        {
            NoteId = Model.NoteId,
            UserId = await GetUserId(),
            Description = Model.Topic,
            Tags = "Fast task",
            Resolved = true,
            EndDate = DateTime.Now
        };

        var resSaveTask = await Service.Notes.SaveNoteTaskAsync(task, true);

        return await Task.FromResult<bool>(resSaveTask.IsValid);
    }

    public void FinalizeAndExtendEdit()
    {            
        OnExtendedEdit();
        Finalize();            
    }

    public virtual WindowDto GetWindow()
    {
        var window = new PostItPropertiesComponent(Store);
        window.LoadModel(Service, WindowPostIt, false);

        var res = window.RunModal();
        if (res.Entity == EComponentResult.Executed)
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

    private async Task<WindowDto> GetNewWindowPostIt()
    {            
        if (Model == null)
            return null;

        var random = new Random();

        // TODO: get default values from Store.AppConfig ...
        return new WindowDto {
            NoteId = Model.NoteId,
            UserId = await GetUserId(),
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

    private async Task<Guid> GetUserId()
    {
        if (_userId != Guid.Empty)
            return _userId;
         
        var userDto = (await Service.Users.GetByUserNameAsync(Store.AppUserName)).Entity;
        if(userDto != null)
            _userId = userDto.UserId;
        return _userId;
    }

}


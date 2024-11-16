using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Controllers;

public class MessagesManagmentCtrl : CtrlBase
{
    #region Fields

    static System.Windows.Forms.Timer kntTimerAlarms;
    static System.Windows.Forms.Timer kntTimerAutoSave;

    private static readonly object _lockObject = new object();
    static bool execAutoSave = true;

    #endregion

    #region Constructor 

    public MessagesManagmentCtrl(Store store): base(store)
    {
        ComponentName = "Messages Managment Component";
    }

    #endregion

    #region Events handlers

    public event EventHandler<ComponentEventArgs<ServiceWithNoteId>> PostItVisible;
    public event EventHandler<ComponentEventArgs<ServiceWithNoteId>> PostItAlarm;
    //public event EventHandler<ComponentEventArgs<ServiceWithNoteId>> EMailAlarm;
    //public event EventHandler<ComponentEventArgs<ServiceWithNoteId>> AppAlarm;
    public event EventHandler<ComponentEventArgs<ServiceWithNoteId>> ExecuteKntScript;

    #endregion

    #region Component override methods

    protected override Result<EComponentResult> OnInitialized()
    {
        try
        {
            VisibleWindows();
            //Task.Run(() => VisibleWindows());

            kntTimerAlarms = new System.Windows.Forms.Timer();
            kntTimerAlarms.Tick += kntTimerAlarms_Tick;
            kntTimerAlarms.Interval = Store.AppConfig.AlarmSeconds * 1000;
            kntTimerAlarms.Start();

            kntTimerAutoSave = new System.Windows.Forms.Timer();
            kntTimerAutoSave.Tick += KntTimerAutoSave_Tick;
            kntTimerAutoSave.Interval = Store.AppConfig.AutoSaveSeconds * 1000; 
            kntTimerAutoSave.Start();

            return new Result<EComponentResult>(EComponentResult.Executed);
        }
        catch (Exception ex)
        {
            var res = new Result<EComponentResult>(EComponentResult.Error);            
            res.AddErrorMessage(ex.Message);            
            return res;            
        }
    }

    #endregion 

    #region Private methods

    private async void KntTimerAutoSave_Tick(object sender, EventArgs e)
    {
        if (!Store.AppConfig.AutoSaveActivated)
            return;
        kntTimerAutoSave.Stop();
        await SaveNotes();
        kntTimerAutoSave.Enabled = true;
    }

    private async void kntTimerAlarms_Tick(object sender, EventArgs e)
    {
        if (!Store.AppConfig.AlarmActivated)
            return;
        kntTimerAlarms.Stop();
        await AlarmsWindows();
        kntTimerAlarms.Enabled = true;
    }

    private async void VisibleWindows()
    {
        lock (_lockObject)
            execAutoSave = false;
        foreach (var store in Store.GetAllServiceRef())
        {
            var service = store.Service;
            var res = await service.Notes.GetVisibleNotesIdAsync(Store.AppUserName);
            foreach(var id in res.Entity)                                    
                PostItVisible?.Invoke(this, new ComponentEventArgs<ServiceWithNoteId>(new ServiceWithNoteId { Service = service, NoteId = id }));
        }
        lock (_lockObject)
            execAutoSave = true;
    }

    private async Task AlarmsWindows()
    {
        lock (_lockObject)
            execAutoSave = false;
        foreach (var store in Store.GetAllServiceRef())
        {
            var service = store.Service;

            var resPostIt = await service.Notes.GetAlarmNotesIdAsync(Store.AppUserName, EnumNotificationType.PostIt);
            foreach (var id in resPostIt.Entity)                            
                PostItAlarm?.Invoke(this, new ComponentEventArgs<ServiceWithNoteId>(new ServiceWithNoteId { Service = service, NoteId = id }));
            
            //var resEMail = await service.Notes.GetAlarmNotesIdAsync(Store.AppUserName, EnumNotificationType.Email);
            //foreach (var id in resEMail.Entity)
            //    EMailAlarm?.Invoke(this, new ComponentEventArgs<ServiceWithNoteId>(new ServiceWithNoteId { Service = service, NoteId = id }));

            //var resAppInfo = await service.Notes.GetAlarmNotesIdAsync(Store.AppUserName, EnumNotificationType.AppInfo);
            //foreach (var id in resAppInfo.Entity)
            //    AppAlarm?.Invoke(this, new ComponentEventArgs<ServiceWithNoteId>(new ServiceWithNoteId { Service = service, NoteId = id }));

            var resKntScript = await service.Notes.GetAlarmNotesIdAsync(Store.AppUserName, EnumNotificationType.ExecuteKntScript);
            foreach (var id in resKntScript.Entity)
                ExecuteKntScript?.Invoke(this, new ComponentEventArgs<ServiceWithNoteId>(new ServiceWithNoteId { Service = service, NoteId = id }));
        }
        lock (_lockObject)
            execAutoSave = true;
    }

    private async Task SaveNotes()
    {        
        if (execAutoSave)
            await Store.SaveActiveNotes();        
    }

    #endregion 
}

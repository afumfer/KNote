using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Service.Core;
using Microsoft.Extensions.Logging;

namespace KNote.ClientWin.Controllers;

public class MessagesManagementCtrl : CtrlBase
{
    #region Fields

    private static System.Windows.Forms.Timer kntTimerAlarms;
    private static System.Windows.Forms.Timer kntTimerAutoSave;

    private static readonly object lockObject = new object();
    private static bool execAutoSave = true;

    #endregion

    #region Constructor 

    public MessagesManagementCtrl(Store store): base(store)
    {
        ControllerName = "Messages Management Controller";
    }

    #endregion

    #region Events handlers

    public event EventHandler<ControllerEventArgs<ServiceWithNoteId>> PostItVisible;
    public event EventHandler<ControllerEventArgs<ServiceWithNoteId>> PostItAlarm;
    public event EventHandler<ControllerEventArgs<ServiceWithNoteId>> EMailAlarm;
    public event EventHandler<ControllerEventArgs<ServiceWithNoteId>> AppAlarm;
    public event EventHandler<ControllerEventArgs<ServiceWithNoteId>> ExecuteKntScript;

    #endregion

    #region Controller override methods

    protected override Result<EControllerResult> OnInitialized()
    {
        try
        {
            VisibleWindows();
            //Task.Run(() => VisibleWindows());

            kntTimerAlarms = new System.Windows.Forms.Timer();
            kntTimerAlarms.Tick += kntTimerAlarms_Tick;
            kntTimerAlarms.Interval = Store.Settings.General.AlarmSeconds * 1000;
            kntTimerAlarms.Start();

            kntTimerAutoSave = new System.Windows.Forms.Timer();
            kntTimerAutoSave.Tick += KntTimerAutoSave_Tick;
            kntTimerAutoSave.Interval = Store.Settings.General.AutoSaveSeconds * 1000; 
            kntTimerAutoSave.Start();

            return new Result<EControllerResult>(EControllerResult.Executed);
        }
        catch (Exception ex)
        {
            var res = new Result<EControllerResult>(EControllerResult.Error);            
            res.AddErrorMessage(ex.Message);            
            return res;            
        }
    }

    #endregion 

    #region Private methods

    // Both timers are always re-enabled in "finally": an exception here used to leave them disabled,
    // silently stopping autosave/alarms for the rest of the session.
    private async void KntTimerAutoSave_Tick(object sender, EventArgs e)
    {
        if (!Store.Settings.General.AutoSaveActivated)
            return;
        kntTimerAutoSave.Enabled = false;
        try
        {
            await SaveNotes();
        }
        catch (Exception ex)
        {
            Store.Logger?.LogError(ex, "Autosave failed.");
        }
        finally
        {
            kntTimerAutoSave.Enabled = true;
        }
    }

    private async void kntTimerAlarms_Tick(object sender, EventArgs e)
    {
        if (!Store.Settings.General.AlarmActivated)
            return;
        kntTimerAlarms.Enabled = false;
        try
        {
            await AlarmsWindows();
        }
        catch (Exception ex)
        {
            Store.Logger?.LogError(ex, "Alarms check failed.");
        }
        finally
        {
            kntTimerAlarms.Enabled = true;
        }
    }

    // Every repository is processed on its own: one that fails (e.g. a SQL Server not reachable
    // yet at startup) is logged and skipped, without preventing the rest from being processed.
    private async void VisibleWindows()
    {
        lock (lockObject)
            execAutoSave = false;
        try
        {
            foreach (var serviceRef in Store.GetAllServiceRef())
            {
                try
                {
                    var service = serviceRef.Service;
                    var res = await service.Notes.GetVisibleNotesIdAsync(Store.AppUserName);
                    foreach (var id in NoteIdsOrEmpty(res, serviceRef, "visible PostIts"))
                        PostItVisible?.Invoke(this, new ControllerEventArgs<ServiceWithNoteId>(new ServiceWithNoteId { Service = service, NoteId = id }));
                }
                catch (Exception ex)
                {
                    Store.Logger?.LogError(ex, "Reopening the visible PostIts of repository {alias} failed.", serviceRef.Alias);
                }
            }
        }
        finally
        {
            lock (lockObject)
                execAutoSave = true;
        }
    }

    private async Task AlarmsWindows()
    {
        lock (lockObject)
            execAutoSave = false;
        try
        {
            foreach (var serviceRef in Store.GetAllServiceRef())
            {
                try
                {
                    var service = serviceRef.Service;

                    var resPostIt = await service.Notes.GetAlarmNotesIdAsync(Store.AppUserName, EnumNotificationType.PostIt);
                    foreach (var id in NoteIdsOrEmpty(resPostIt, serviceRef, "PostIt alarms"))
                        PostItAlarm?.Invoke(this, new ControllerEventArgs<ServiceWithNoteId>(new ServiceWithNoteId { Service = service, NoteId = id }));

                    var resEMail = await service.Notes.GetAlarmNotesIdAsync(Store.AppUserName, EnumNotificationType.Email);
                    foreach (var id in NoteIdsOrEmpty(resEMail, serviceRef, "Email alarms"))
                        EMailAlarm?.Invoke(this, new ControllerEventArgs<ServiceWithNoteId>(new ServiceWithNoteId { Service = service, NoteId = id }));

                    var resAppInfo = await service.Notes.GetAlarmNotesIdAsync(Store.AppUserName, EnumNotificationType.AppInfo);
                    foreach (var id in NoteIdsOrEmpty(resAppInfo, serviceRef, "AppInfo alarms"))
                        AppAlarm?.Invoke(this, new ControllerEventArgs<ServiceWithNoteId>(new ServiceWithNoteId { Service = service, NoteId = id }));

                    var resKntScript = await service.Notes.GetAlarmNotesIdAsync(Store.AppUserName, EnumNotificationType.ExecuteKntScript);
                    foreach (var id in NoteIdsOrEmpty(resKntScript, serviceRef, "script alarms"))
                        ExecuteKntScript?.Invoke(this, new ControllerEventArgs<ServiceWithNoteId>(new ServiceWithNoteId { Service = service, NoteId = id }));
                }
                catch (Exception ex)
                {
                    Store.Logger?.LogError(ex, "Checking the alarms of repository {alias} failed.", serviceRef.Alias);
                }
            }
        }
        finally
        {
            lock (lockObject)
                execAutoSave = true;
        }
    }

    // Entity is null when the repository caught a database error itself (see GenericRepositoryEF)
    // instead of throwing it - the error is then only in the result's error message.
    private List<Guid> NoteIdsOrEmpty(Result<List<Guid>> res, ServiceRef serviceRef, string query)
    {
        if (res.Entity != null)
            return res.Entity;

        Store.Logger?.LogWarning("Getting the {query} of repository {alias} failed: {error}", query, serviceRef.Alias, res.ErrorMessage);
        return new List<Guid>();
    }

    private async Task SaveNotes()
    {        
        if (execAutoSave)
            await Store.SaveActiveNotes();
        else
        {
            kntTimerAutoSave.Enabled = false;
            Thread.Sleep(100);
            kntTimerAutoSave.Enabled = true;
        }
    }

    #endregion 
}

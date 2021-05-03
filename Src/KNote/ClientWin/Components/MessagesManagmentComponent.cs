using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using System.Windows.Forms;

using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace KNote.ClientWin.Components
{
    public class MessagesManagmentComponent : ComponentBase
    {
        static Timer kntTimerAlarms;
        static Timer kntTimerAutoSave;        
        
        public MessagesManagmentComponent(Store store): base(store)
        {

        }

        public event EventHandler<ComponentEventArgs<ServiceWithNoteId>> PostItVisible;
        public event EventHandler<ComponentEventArgs<ServiceWithNoteId>> PostItAlarm;
        //public event EventHandler<ComponentEventArgs<ServiceWithNoteId>> EMailAlarm;
        //public event EventHandler<ComponentEventArgs<ServiceWithNoteId>> AppAlarm;
        public event EventHandler<ComponentEventArgs<ServiceWithNoteId>> ExecuteKntScript;

        protected override Result<EComponentResult> OnInitialized()
        {
            try
            {
                VisibleWindows();

                kntTimerAlarms = new Timer();
                kntTimerAlarms.Tick += kntTimerAlarms_Tick;
                kntTimerAlarms.Interval = Store.AppConfig.AlarmSeconds * 1000;
                kntTimerAlarms.Start();

                kntTimerAutoSave = new Timer();
                kntTimerAutoSave.Tick += KntTimerAutoSave_Tick;
                kntTimerAutoSave.Interval = Store.AppConfig.AutoSaveSeconds * 1000; 
                kntTimerAutoSave.Start();

                return new Result<EComponentResult>(EComponentResult.Executed);
            }
            catch (Exception)
            {
                return new Result<EComponentResult>(EComponentResult.Error);
            }
        }

        private void KntTimerAutoSave_Tick(object sender, EventArgs e)
        {
            if (!Store.AppConfig.AutoSaveActivated)
                return;
            kntTimerAutoSave.Stop();                        
            SaveNotes();             
            kntTimerAutoSave.Enabled = true;
        }

        private void kntTimerAlarms_Tick(object sender, EventArgs e)        
        {
            if (!Store.AppConfig.AlarmActivated)
                return;
            kntTimerAlarms.Stop();                        
            AlarmsWindows();            
            kntTimerAlarms.Enabled = true;            
        }

        private async void VisibleWindows()
        {
            foreach(var store in Store.GetAllServiceRef())
            {
                var service = store.Service;
                var res = await service.Notes.GetVisibleNotesIdAsync(Store.AppUserName);
                foreach(var id in res.Entity)                                    
                    PostItVisible?.Invoke(this, new ComponentEventArgs<ServiceWithNoteId>(new ServiceWithNoteId { Service = service, NoteId = id }));                
            }
        }

        private async void AlarmsWindows()
        {
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
        }

        private async void SaveNotes()
        {
            await Store.SaveActiveNotes();
        }
    }
}

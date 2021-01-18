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
        static Timer kntTimer;        
        static int alarmCounter = 1;
        //static bool exitFlag = false;
        
        public MessagesManagmentComponent(Store store): base(store)
        {

        }

        public event EventHandler<ComponentEventArgs<ServiceWithNoteId>> PostItVisible;
        //public event EventHandler<ComponentEventArgs<ServiceWithNoteId>> PostItAlarm;

        protected override Result<EComponentResult> OnInitialized()
        {
            try
            {
                VisibleWindows();

                kntTimer = new Timer();
                kntTimer.Tick += KntTimer_Tick;
                kntTimer.Interval = 30 * 1000;   // TODO: magic number refactor 
                kntTimer.Start();
                return new Result<EComponentResult>(EComponentResult.Executed);

            }
            catch (Exception)
            {

                return new Result<EComponentResult>(EComponentResult.Error);
            }
        }

        private void KntTimer_Tick(object sender, EventArgs e)        
        {
            kntTimer.Stop();
            alarmCounter++;
            AlarmsWindows();
            kntTimer.Enabled = true;
        }

        private async void VisibleWindows()
        {
            foreach(var store in Store.GetAllServiceRef())
            {
                var service = store.Service;
                var res = await service.Notes.GetVisibleNotesIdAsync(Store.AppUserName);
                foreach(var id in res.Entity)
                {                    
                    PostItVisible?.Invoke(this, new ComponentEventArgs<ServiceWithNoteId>(new ServiceWithNoteId { Service = service, NoteId = id }));
                }
            }
        }

        private async void AlarmsWindows()
        {
            foreach (var store in Store.GetAllServiceRef())
            {
                var service = store.Service;
                var res = await service.Notes.GetAlarmNotesIdAsync(Store.AppUserName);
                foreach (var id in res.Entity)
                {
                    PostItVisible?.Invoke(this, new ComponentEventArgs<ServiceWithNoteId>(new ServiceWithNoteId { Service = service, NoteId = id }));

                    // TODO: Set AlarmOK.

                }
            }
        }

    }
}

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
        static System.Windows.Forms.Timer kntTimer;        
        static int alarmCounter = 1;
        //static bool exitFlag = false;

        public MessagesManagmentComponent(Store store): base(store)
        {

        }

        protected override Result<EComponentResult> OnInitialized()
        {
            try
            {
                kntTimer = new System.Windows.Forms.Timer();
                kntTimer.Tick += KntTimer_Tick;
                kntTimer.Interval = 2000;
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

            if (MessageBox.Show("Continue running?", "Count is: " + alarmCounter,
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                // Restarts the timer and increments the counter.
                alarmCounter += 1;
                kntTimer.Enabled = true;
            }
            else
            {
                // Stops the component.
                Finalize();
            }
        }
    }
}

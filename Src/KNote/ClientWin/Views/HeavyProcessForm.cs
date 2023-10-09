using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KNote.ClientWin.Views
{
    public partial class HeavyProcessForm : Form
    {
        Func<Task> _process;

        public HeavyProcessForm()  // Func<Task> process
        {
            InitializeComponent();
            //_process = process;
        }

        private void HeavyProcessForm_Load(object sender, EventArgs e)
        {
            //if(_process != null) 
            //{ 
            //    await _process(); 
            //}
        }

        public async Task Exec(Func<Task> process)
        {
            if (process != null)
            {
                await process();
            }
        }
    }
}

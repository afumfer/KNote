using KNote.ClientWin.Components;
using KNote.Model.Dto;
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
        public HeavyProcessForm()  // Func<Task> process
        {
            InitializeComponent();
        }

        private void HeavyProcessForm_Load(object sender, EventArgs e)
        {

        }

        public async Task Exec(Func<Task> process)
        {
            progressProcess.Value = 0;

            if (process != null)
            {
                await process();
            }
        }

        //public async Task Exec2(Func<EnumChangeTag, List<NoteInfoDto>, string, Task> process, EnumChangeTag action, string tag, List<NoteInfoDto> selectedNotes)
        public async Task Exec3<TParam1, TParam2, TParam3>(Func<TParam1, TParam2, TParam3, Task> process, TParam1 action, TParam2 selectedNotes, TParam3 tag)
        {
            progressProcess.Value = 0;

            if (process != null)
            {
                await process(action, selectedNotes, tag);
            }
        }

        public void UpdateProgress(int progress)
        {
            progressProcess.Value = progress;
        }

        public void UpdateProcessName(string process)
        {
            labelProcess.Text = process;
        }

        public void UpdateProcessInfo(string info)
        {
            labelInfo.Text = info;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {

        }
    }
}

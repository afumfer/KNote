using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using KNote.ClientWin.Components;
using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Views
{
    public partial class NotifyForm : Form, IViewBase
    {
        private readonly KNoteManagmentComponent _com;

        public NotifyForm(KNoteManagmentComponent com)
        {
            InitializeComponent();
            _com = com;
        }

        #region IViewBase implementation

        public void ShowView()
        {
            this.Show();            
        }

        Result<EComponentResult> IViewBase.ShowModalView()
        {
            return _com.DialogResultToComponentResult(this.ShowDialog());
        }

        public DialogResult ShowInfo(string info, string caption = "KeyNotex", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            return MessageBox.Show(info, caption, buttons, icon);
        }

        public void OnClosingView()
        {
            
        }

        #endregion

        #region Menu events handlers 

        private void notifyKNote_DoubleClick(object sender, EventArgs e)
        {
            _com.AddDefaultNotePostIt();
        }

        private void menuNewNote_Click(object sender, EventArgs e)
        {
            _com.AddDefaultNotePostIt();
        }

        private void menuShowKNoteManagment_Click(object sender, EventArgs e)
        {
            _com.ShowKNoteManagment();
        }

        private void menuPostItsVisibles_Click(object sender, EventArgs e)
        {
            if (menuPostItsVisibles.Checked)
                _com.Store.ActivatePostIts();
            else
                _com.Store.HidePostIts();
        }

        private void menuKNoteOptions_Click(object sender, EventArgs e)
        {
            MessageBox.Show("TODO: ....");
        }

        private void menuHelp_Click(object sender, EventArgs e)
        {
            MessageBox.Show("TODO: ....");
        }

        private void menuAbout_Click(object sender, EventArgs e)
        {
            _com.About();
        }

        private void menuExit_Click(object sender, EventArgs e)
        {            
            _com?.FinalizeApp();
        }

        #endregion

    }
}

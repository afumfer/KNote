using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using KNote.ClientWin.Core;
using KNote.ClientWin.Components;

namespace KNote.ClientWin.Views
{
    public partial class MonitorForm : Form, IViewBase
    {
        private readonly MonitorComponent _com;
        private bool _viewFinalized = false;

        public MonitorForm(MonitorComponent com)
        {
            InitializeComponent();
            _com = com;
        }

        public Control PanelView()
        {
            return null; 
        }

        public void OnClosingView()
        {
            _viewFinalized = true;
            this.Close();
        }

        public void RefreshView()
        {
            Application.DoEvents();
        }

        public void ShowInfo(string info)
        {
            listBoxMessages.Items.Add(info);
        }

        public void ShowView()
        {
            this.Show();
        }

        private void MonitorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_viewFinalized)
                _com.Finalize();           
        }

        private void buttonClearMessages_Click(object sender, EventArgs e)
        {
            listBoxMessages.Items.Clear();
        }
    }
}

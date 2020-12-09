using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using KNote.ClientWin.Core;
using KNote.ClientWin.Components;
using KNote.Model;

namespace KNote.ClientWin.Views
{
    public partial class MonitorForm : Form, IViewConfigurable
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

        public DialogResult ShowInfo(string info, string caption = "KeyNote", MessageBoxButtons buttons = MessageBoxButtons.OK)
        {
            listBoxMessages.Items.Add(info);
            int visibleItems = listBoxMessages.ClientSize.Height / listBoxMessages.ItemHeight;
            listBoxMessages.TopIndex = Math.Max(listBoxMessages.Items.Count - visibleItems + 1, 0);
            return DialogResult.OK;
        }

        public void ShowView()
        {
            this.Show();
        }

        Result<EComponentResult> IViewBase.ShowModalView()
        {
            return null;
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

        public void ConfigureEmbededMode()
        {
            
        }

        public void ConfigureWindowMode()
        {
            
        }


    }
}

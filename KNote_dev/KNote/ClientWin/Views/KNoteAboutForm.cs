using KNote.ClientWin.Components;
using KNote.ClientWin.Core;
using KNote.Model;
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
    public partial class KNoteAboutForm : Form, IViewBase
    {
        private readonly KNoteManagmentComponent _com;

        public KNoteAboutForm(KNoteManagmentComponent com)
        {
            InitializeComponent();
            _com = com;
        }

        #region IViewBase implementation

        public void ShowView()
        {
            this.Show();
        }

        public Result<EComponentResult> ShowModalView()
        {
            return _com.DialogResultToComponentResult(this.ShowDialog());
        }

        public DialogResult ShowInfo(string info, string caption = "KaNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Asterisk)
        {
            return MessageBox.Show(info, caption, buttons, icon);
        }

        public void OnClosingView()
        {

        }

        #endregion 

        #region Form events handlers 

        private void KNoteAboutForm_Load(object sender, EventArgs e)
        {
            var info = "This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation; either version 2 of the License, or (at your option) any later version. " + Environment.NewLine + Environment.NewLine +
                       "This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. ";
            labelVersion.Text = "Version: " + Application.ProductVersion.ToString();
            labelInfo.Text = info;
        }

        #endregion

        private void buttonAccept_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
    }
}

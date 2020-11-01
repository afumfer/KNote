using KNote.ClientWin.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using KNote.ClientWin.Components;
using KNote.ClientWin.Core;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views
{
    public partial class KNoteManagmentForm : Form, IViewBase
    {
        private readonly KNoteManagmentComponent _com;
        private bool _viewFinalized = false;

        public KNoteManagmentForm(KNoteManagmentComponent com)
        {
            InitializeComponent();

            _com = com;
        }

        #region IViewBase interface 

        public void OnClosingView()
        {
            throw new NotImplementedException();
        }

        public void RefreshView()
        {
            throw new NotImplementedException();
        }

        public void ShowInfo(string info)
        {
            throw new NotImplementedException();
        }

        public void ShowView()
        {
            throw new NotImplementedException();
        }

        #endregion 
    }
}

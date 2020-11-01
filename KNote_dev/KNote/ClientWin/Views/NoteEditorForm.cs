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
    public partial class NoteEditorForm : Form, IEditorView<NoteDto>
    {
        private readonly NoteEditorComponent _com;
        private bool _viewFinalized = false;

        public NoteEditorForm(NoteEditorComponent com)
        {
            InitializeComponent();
            
            _com = com;
        }

        #region IEditorView interface 

        public void CleanView()
        {
            throw new NotImplementedException();
        }

        public void ConfigureEmbededMode()
        {
            throw new NotImplementedException();
        }

        public void ConfigureWindowMode()
        {
            throw new NotImplementedException();
        }

        public void OnClosingView()
        {
            throw new NotImplementedException();
        }

        public void RefreshBindingModel()
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

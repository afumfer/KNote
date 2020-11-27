using KNote.ClientWin.Components;
using KNote.ClientWin.Core;
using KNote.Model;
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
    public partial class FolderEditorForm : Form, IEditorView<FolderDto>
    {
        #region Private fields

        private readonly FolderEditorComponent _com;
        private bool _viewFinalized = false;

        #endregion 

        public FolderEditorForm(FolderEditorComponent com)
        {
            InitializeComponent();
            _com = com;
        }

        #region IEditorView implementation 

        public Control PanelView()
        {
            return panelForm;
        }

        public void ShowView()
        {
            this.Show();
        }

        public Result<EComponentResult> ShowModalView()
        {
            return _com.DialogResultToComponentResult(this.ShowDialog());
        }

        public DialogResult ShowInfo(string info, string caption = "KeyNote", MessageBoxButtons buttons = MessageBoxButtons.OK)
        {
            throw new NotImplementedException();
        }

        public void RefreshView()
        {
            RefreshBindingModel();
        }

        public void RefreshBindingModel()
        {

        }

        public void CleanView()
        {
            
        }

        public void OnClosingView()
        {
            _viewFinalized = true;
            this.Close();
        }

        public void ConfigureEmbededMode()
        {

        }

        public void ConfigureWindowMode()
        {

        }

        #endregion


        private void FolderEditorForm_Load(object sender, EventArgs e)
        {

        }
        private void FolderEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_viewFinalized)
            {
                //SaveModel();
                _com.Finalize();
            }
        }



    }
}

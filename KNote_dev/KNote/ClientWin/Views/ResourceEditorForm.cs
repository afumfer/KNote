using KNote.ClientWin.Components;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KNote.ClientWin.Views
{
    public partial class ResourceEditorForm : Form, IEditorView<ResourceDto>
    {
        #region Private fields

        private readonly ResourceEditorComponent _com;
        private bool _viewFinalized = false;
        private bool _formIsDisty = false;

        #endregion

        #region Constructor

        public ResourceEditorForm(ResourceEditorComponent com)
        {
            InitializeComponent();
            _com = com;
        }

        #endregion

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
            var res = _com.DialogResultToComponentResult(this.ShowDialog());
            return res;
        }

        public DialogResult ShowInfo(string info, string caption = "KeyNote", MessageBoxButtons buttons = MessageBoxButtons.OK)
        {
            return MessageBox.Show(info, caption, buttons);
        }

        public void RefreshView()
        {
            ModelToControls();
        }

        public void CleanView()
        {
            textDescription.Text = "";
            textOrder.Text = "";
            htmlPreview.BodyHtml = "";
            textFileName.Text = "";
        }

        public void ConfigureEmbededMode()
        {
            
        }

        public void ConfigureWindowMode()
        {
            
        }

        public void OnClosingView()
        {
            _viewFinalized = true;
            this.Close();
        }

        #endregion

        #region Form event handlers

        private void ResourceEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_viewFinalized)
            {
                var confirmExit = OnCandelEdition();
                if (!confirmExit)
                    e.Cancel = true;
            }
        }

        private void ResourceEditorForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
                _formIsDisty = true;
        }

        private void ResourceEditorForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            _formIsDisty = true;
        }

        private async void buttonAccept_Click(object sender, EventArgs e)
        {
            ControlsToModel();
            var res = await _com.SaveModel();
            if (res)
            {
                _formIsDisty = false;
                this.DialogResult = DialogResult.OK;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            OnCandelEdition();
        }

        #endregion


        #region Private methods

        private bool OnCandelEdition()
        {
            if (_formIsDisty)
            {
                if (MessageBox.Show("You have modified this entity, are you sure you want to exit without recording?", "KeyNote", MessageBoxButtons.YesNo) == DialogResult.No)
                    return false;
            }

            this.DialogResult = DialogResult.Cancel;
            _com.CancelEdition();
            return true;
        }

        private void ModelToControls()
        {
            textDescription.Text = _com.Model.Description;
            textOrder.Text = _com.Model.Order.ToString();
            htmlPreview.BodyHtml = "";
            textFileName.Text = _com.Model.Name;
        }

        private void ControlsToModel()
        {
            _com.Model.Description = textDescription.Text;
            _com.Model.Order = _com.TextToInt(textOrder.Text);
            //htmlPreview.BodyHtml
            //_com.Model.NameOut = textFileName.Text;
            _com.Model.Name = textFileName.Text;
        }


        #endregion

        private void buttonSelectFile_Click(object sender, EventArgs e)
        {
            textFileName.Text = "file_" + Guid.NewGuid().ToString();
            
            var arrayBytes = new byte[10];
            for (int i = 0; i < 10; i++)
                arrayBytes[i] = (byte)'a';

            _com.Model.ContentBase64 = Convert.ToBase64String(arrayBytes);
            _com.Model.FileType = "application/pdf"; 
        }
    }
}

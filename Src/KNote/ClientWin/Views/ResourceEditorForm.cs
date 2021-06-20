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

        private string varContentBase64;
        private string varFileType;
        private string varContainer;
        private byte[] varContentArrayBytes;
        private string varName;
        //private string varRelativeUrl;
        //private string varFullUrl;

        #endregion

        #region Constructor

        public ResourceEditorForm(ResourceEditorComponent com)
        {
            InitializeComponent();
            _com = com;

            var n = new ResourceDto();            
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

        public DialogResult ShowInfo(string info, string caption = "KaNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            return MessageBox.Show(info, caption, buttons, icon);
        }

        public void RefreshView()
        {
            ModelToControls();
        }

        public void RefreshModel()
        {
            ControlsToModel();
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
            var res = await _com.SaveModel();
            if (res)
            {
                htmlPreview.BodyHtml = "";
                htmlPreview.Refresh();
                _formIsDisty = false;
                this.DialogResult = DialogResult.OK;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            OnCandelEdition();
        }

        private void buttonSelectFile_Click(object sender, EventArgs e)
        {
            openFileDialog.Title = "Select file for KeyNote resource";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var fileTmp = openFileDialog.FileName;
                varContentArrayBytes = File.ReadAllBytes(fileTmp);
                varContentBase64 = Convert.ToBase64String(varContentArrayBytes);
                textFileName.Text = Path.GetFileName(fileTmp);
                textDescription.Text = textFileName.Text;
                varName = _com.Model.ResourceId.ToString() + "_" + textFileName.Text;
                varContainer = _com.Service.RepositoryRef.ResourcesContainer + @"\" + DateTime.Now.Year.ToString();
                varFileType = _com.ExtensionFileToFileType(Path.GetExtension(fileTmp));
                ShowPreview(fileTmp);
            }

        }

        #endregion


        #region Private methods

        private bool OnCandelEdition()
        {
            if (_formIsDisty)
            {
                if (MessageBox.Show("You have modified this entity, are you sure you want to exit without recording?", "KaNote", MessageBoxButtons.YesNo) == DialogResult.No)
                    return false;
            }

            this.DialogResult = DialogResult.Cancel;
            _com.CancelEdition();
            return true;
        }

        private void ModelToControls()
        {                      
            textFileName.Text = _com.Model.NameOut;
            varName = _com.Model.Name;
            textDescription.Text = _com.Model.Description;
            textOrder.Text = _com.Model.Order.ToString();
            varFileType = _com.Model.FileType;            
            varContainer = _com.Model.Container;            
            checkContentInDB.Checked = _com.Model.ContentInDB;

            // TODO: Refactor, this logic should be carried over to the component 
            if (_com.Model.ContentInDB)
            {
                (_com.Model.RelativeUrl, _com.Model.FullUrl) = 
                _com.GetOrSaveTmpFile(
                    _com.Service.RepositoryRef.ResourcesContainerCacheRootPath,
                    _com.Model.Container, 
                    _com.Model.Name, 
                    _com.Model.ContentArrayBytes);

                varContentArrayBytes = _com.Model.ContentArrayBytes;
                varContentBase64 = _com.Model.ContentBase64;
            }
            else
            {
                _com.Model.RelativeUrl = Path.Combine(_com.Model.Container, _com.Model.Name);                
                _com.Model.FullUrl = Path.Combine(_com.Service.RepositoryRef.ResourcesContainerCacheRootPath, _com.Model.RelativeUrl);
                varContentArrayBytes = File.ReadAllBytes(_com.Model.FullUrl);
                varContentBase64 = Convert.ToBase64String(varContentArrayBytes);
            }

            ShowPreview(_com.Model.FullUrl, false);
        }

        private void ControlsToModel()
        {
            _com.Model.Name = varName;
            _com.Model.Description = textDescription.Text;
            _com.Model.Order = _com.TextToInt(textOrder.Text);
            _com.Model.FileType = varFileType;
            _com.Model.Container = varContainer;
            _com.Model.ContentInDB = checkContentInDB.Checked;

            // TODO: Refactor, this logic should be carried over to the component 
            (_com.Model.RelativeUrl, _com.Model.FullUrl) =
            _com.GetOrSaveTmpFile(
                _com.Service.RepositoryRef.ResourcesContainerCacheRootPath,
                _com.Model.Container,
                _com.Model.Name,
                varContentArrayBytes);

            if (_com.Model.ContentInDB)
            {
                _com.Model.ContentBase64 = varContentBase64;                        
                _com.Model.ContentArrayBytes = varContentArrayBytes;
            }
            else
            {
                _com.Model.ContentBase64 = null;
                _com.Model.ContentArrayBytes = null;
            }
        }

        private void ShowPreview(string file, bool includePdf = true)
        {
            if (file == null)
            {
                htmlPreview.BodyHtml = "";
                return;
            }
                            
            var ext = Path.GetExtension(file);          
            // Hack, ... pdf not work in htmlEditor when edit ... :-( ...
            var fileTypes = (includePdf) ? ".jpg.jpeg.png.pdf" : ".jpg.jpeg.png";
            if (fileTypes.IndexOf(ext) >= 0)
            {
                try
                {                    
                    htmlPreview.BodyHtml = "";
                    htmlPreview.NavigateToUrl(file);
                }
                catch (Exception ex)
                {
                    ShowInfo(ex.Message, "KaNote");
                }
            }
            else
                htmlPreview.BodyHtml = "";
        }

        #endregion

    }
}

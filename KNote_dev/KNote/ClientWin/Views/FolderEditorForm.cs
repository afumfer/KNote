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
        private bool _formIsDisty = false;

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
            textName.Text = "";
            textTags.Text = "";
            textOrder.Text = "";
            textOrderNotes.Text = "";
            textParentFolder.Text = "";
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

        #region Form events handler

        private void FolderEditorForm_Load(object sender, EventArgs e)
        {

        }
        private void FolderEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_viewFinalized)
            {
                var confirmExit = OnCandelEdition();
                if (!confirmExit)
                    e.Cancel = true;
            }
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
            textName.Text = _com.Model.Name;
            textTags.Text = _com.Model.Tags;
            textOrder.Text = _com.Model.Order.ToString();
            textOrderNotes.Text = _com.Model.OrderNotes;
            textParentFolder.Text = _com.Model.ParentFolderDto?.Name;
        }

        private void ControlsToModel()
        {
            _com.Model.Name = textName.Text;
            _com.Model.Tags = textTags.Text;            
            int o;
            if (int.TryParse(textOrder.Text, out o))
                _com.Model.Order = o;

            _com.Model.OrderNotes = textOrderNotes.Text;

            // _com.Model.ParentFolder  // TODO ...
        }

        private void FolderEditorForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
                _formIsDisty = true;
        }

        private void FolderEditorForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            _formIsDisty = true;
        }
    }
}

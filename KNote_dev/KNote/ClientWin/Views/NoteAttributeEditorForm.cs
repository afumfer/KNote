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
    public partial class NoteAttributeEditorForm : Form, IEditorView<NoteKAttributeDto>
    {
        #region Private fields

        private readonly NoteAttributeEditorComponent _com;
        private bool _viewFinalized = false;
        private bool _formIsDisty = false;

        #endregion

        #region Constructor

        public NoteAttributeEditorForm(NoteAttributeEditorComponent com)
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
            textValue.Text = "";
            labelDescription.Text = "";
            labelAttribute.Text = "";
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

        #region Form events handlers

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

        private void NoteAttributeEditorForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
                _formIsDisty = true;
        }

        private void NoteAttributeEditorForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            _formIsDisty = true;
        }

        private void NoteAttributeEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_viewFinalized)
            {
                var confirmExit = OnCandelEdition();
                if (!confirmExit)
                    e.Cancel = true;
            }
        }

        #endregion

        #region Private methods

        private void ModelToControls()
        {
            textValue.Text = _com.Model.Value;
            labelDescription.Text = _com.Model.Description;
            labelAttribute.Text = _com.Model.Name;
        }

        private void ControlsToModel()
        {
            _com.Model.Value = textValue.Text;
        }

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

        #endregion 

    }
}
